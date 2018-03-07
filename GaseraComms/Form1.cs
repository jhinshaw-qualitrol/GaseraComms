using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;
using System.Globalization;


namespace GaseraComms
{
    public partial class Form1 : Form
    {
        /* NOTES
         * comms in b/g with timeout
         * 
         * UI Functionality
         *  Start New Measurement
         *      OnClick
         *  Stop Current Measurement
         *      OnClick
         *      STPM command
         *  Poll Measurement Results (concentrations)
         *      OnClick
         *      Accumulate to tab-delimited file
         *      ACON command
         *  Set / update IP address
         *  Set / update logfile path
         *  Display Device Status
         *  Display Error Status
         *  Display Task List
         *  
         * Internal
         * Start New Measurement
         *          STAM command
         * Read measurement results
         *      OnTimer2, configurable interval
         *      ACON command
         * Get Device Status
         *      OnTimer1, 1 second
         *      ASTS command
         * Get Active Errors
         *      OnTimer1, 1 second
         *      AERR command
         * Get Task List
         *      OnTimer1, 1 second
         *      ATSK command
         * Parse IP address
         * Parse logfile path
         * Logfile append / overwrite flag
         * Open Socket
         * Close Socket
         * Open Logfile
         * Write to logfile
         * Close logfile
         *  
         * Controls
         *  Start/Stop Communications button
         *  Start Measurement, button
         *  Stop Measurement, another button
         *  Quit Program button
         *  Logfile path
         *  IP Address and socket (default socket = 8888)
         *   
         * 
         * 
         * 
        */
        
        internal IPAddress gIP;
        internal commState cState;
        // datatable to accumulate results from the monitor
        internal DataTable gasDt;
        // dictionary of CAS numbers (as strings) and gas names
        internal Dictionary<string, string> casGasDict;
        // Dict of Gasera errors by error number
        internal Dictionary<string, string> gaseraErrors;
        // dict of Gasera status by number
        internal Dictionary<string, string> gaseraDStatus;
        // dict of Gasera measurement status (phase)
        internal Dictionary<string, string> gaseraMStatus;
       

        // time (ms) to wait in b/g task between communication cycles
        public const int REPEATINTERVAL = 2000;
        // time to wait before sending individual commands
        public const int COMMDELAY = 200;
        // socket timeout
        // set this to something useful for other than test
        public const int COMMTIMEOUT = 5000;
        // max byte count in a sent commmand
        public const int MAXCMDBYTECOUNT = 32;
        // max byte count of a response from the Gasera
        public const int MAXRESPONSECOUNT = 256;
        // max number of elements in a Gasera response
        public const int MAXRESPONSEELEMENTS = 32;
        // socket number to connect to
        public const int SOCKNUM = 8888;
        /// <summary>
        /// Passed into bgworker at the start
        /// </summary>
        internal struct doWorkArg
        {
            internal IPAddress ipAddress;
            internal int socket;
        }
        /// <summary>
        /// filled in upon establishing comms
        /// also keeps track of state 
        /// and seen by bgworker as a global
        /// </summary>
        internal class commState
        {
            internal EndPoint eP;
            internal bool commsUp;
            internal bool measuring;
            internal string Status;
            internal string DevErrors;
            // dict of task number and task name
            internal Dictionary<string, string> TaskList;
            internal bool taskListUpdated;
            internal string[] respStr;
            internal string selectedTask;
            internal bool enableMeasure;
            internal bool haveErrors;
            //internal string[] measStr;
            internal bool canOpenFile;
            internal string fileName;
            internal bool fileAppend;
            internal string MeasPhase;
            internal bool starting;

            internal commState()
            {
                commsUp = false;
                eP = null;
                taskListUpdated = false;
                TaskList = new Dictionary<string, string>();
                DevErrors = "Not Connected";
                measuring = false;
                DevErrors = "Not Connected";
                respStr = new string[0];
                selectedTask = string.Empty ;
                enableMeasure = false;
                haveErrors = false;
                //measStr = new string[0];
                canOpenFile = false;
                fileName = string.Empty;
                fileAppend = true;
            }
        }
        
        

        // commands for Gasera
        internal enum gaseraCmds
        {
            ASTS, AERR, ATSK, STAM, ACON, STPM, AMST,
            // not implemented in this version
            SCOR, ANAM
        }
        /// <summary>
        /// Enum of DGA gases. 
        /// Same values and order as found in RogueProc
        /// </summary>
        public enum Gases : int
        { UNK = 0, CH4, C2H2, C2H4, C2H6, CO, CO2, H2O, Water_Offset, Zero, O2, H2, N2, C3H8, C3H6 }
        /// <summary>
        /// Enumeration of CAS registry strings
        /// to control iteration of gasDict 
        /// These must correspond 1:1 with the low-end of the above Gases enum, i.e. start with 1
        /// The prefix casPrefix = "CAS" is not included in the Gasera One output strings
        /// But is needed here to make the enum vars legal
        /// Note: H2O is included in the Gasera output
        /// </summary>
        internal enum CasGases
        {
            CAS74_82_8 = 1, // CH4
            CAS74_84_0,     // C2H2
            CAS74_85_1,     // C2H4
            CAS74_86_2,     // C2H6
            CAS630_08_0,    // CO
            CAS124_38_9,    // CO2
            CAS7732_18_5    // H2O
        }
        // comms start/stop bytes
        static readonly byte STX = 02;
        static readonly byte ETX = 03;
        static readonly byte SPC = 32; // space char
        static readonly byte K = 75; // K char
        static readonly byte ZERO = 48; // 0 char
        // string prefixed in GasGases enum to the CAS registry values
        static readonly string CASPREFIX = "CAS"; 
        
        // buffer array for comms requests
        // first index corresp to the gaseraCmds enum
        // second array is the content for the command
        // outgoing commands are all < 32 bytes
        // last byte [31] is the number of bytes that comprise that command buffer
        internal byte[][] reqCmds = new  byte[Enum.GetValues( typeof (gaseraCmds)).Length][];

        /// <summary>
        /// Compares two dictionary <string, string>
        /// overrides Equals() method
        /// returns true if each key and corresponding value match exclusively
        /// order of entries is not important
        /// </summary>
        public class DictComparer : EqualityComparer<Dictionary<string, string>>
        {
            /// <summary>
            /// true if each key and corresponding value match exclusively
            /// but don't need to be in order
            /// </summary>
            /// <param name="x"></param>
            /// <param name="y"></param>
            /// <returns></returns>
            public override bool Equals(Dictionary<string, string> x, Dictionary<string, string> y)
            {
                if (x == null && y == null)
                    return true;
                else if (x == null | y == null)
                    return false;
                else if (x.Count != y.Count)
                    return false;
                foreach (KeyValuePair<string, string> kvp in x)
                {
                    string xStr = kvp.Value;
                    string yStr = string.Empty;
                    if (!y.TryGetValue(kvp.Key, out yStr) || kvp.Value != yStr)
                        return false;
                }
                return true;
            }

            public override int GetHashCode(Dictionary<string, string> obj)
            {
                return obj.GetHashCode();
            }
        }

    
        public Form1()
        {
            InitializeComponent();
        }

 
        private void Form1_Load(object sender, EventArgs e)
        {
            // display a default IP address 
            // TODO if important, use DNS to lookup
            gIP = new IPAddress(new byte[] {10,75,50,116});
            // fill the IP Address text box accordingly
            ipTextBox.Text = gIP.ToString();
            // initialize the UI and cState
            initUI();
            // build comms request strings
            foreach ( gaseraCmds cmd in Enum.GetValues( typeof (gaseraCmds)) )
            {
                GenGaseraCmd(cmd, out reqCmds[(int) cmd]);
            }

            // init gasDt datatable
            int gSeries;
            gasDt = new DataTable("Gas Concentrations");
            gasDt.Columns.Add("DateStamp", typeof(DateTime));
            // iterate thru the Gases enum and add them to the table
            // also adding to the CAS --> gases dictionary
            // indices 1 - 7 or CH4 to H2O are the needed gases
            string[] casGasNames = Enum.GetNames(typeof(CasGases));
            casGasDict = new Dictionary<string, string>();

            // clear existing chart series
            chart1.Series.Clear();
            // set up chart1 axes
            chart1.ChartAreas[0].AxisY.Title = "ppm";
            chart1.ChartAreas[0].AxisY.LabelStyle.Format = "{0:F0,}";
            // chart1.ChartAreas[0].AxisX.LabelStyle.Format = "";
           
            // create a contextmenu to handle right-click on the chart
            // for show/hide the series
            ContextMenu cM = new ContextMenu();
            chart1.ContextMenu = cM;
            
            // have to set zero y-axis values to something small, 
            // like 0.01, to use log scaling. Do this when adding rows
            // as the data is uploaded from the monitor
            // chart1.ChartAreas[0].AxisY.IsLogarithmic = true;
            for (Gases gas = Gases.CH4; gas < Gases.Water_Offset; gas++)
            {
                gSeries = (int) gas -1;
                gasDt.Columns.Add(gas.ToString(), typeof(double));
                casGasDict.Add(casGasNames[gSeries], gas.ToString());
                // also add to the chart
                chart1.Series.Add(gas.ToString());
                chart1.Series[gSeries].XValueMember = "DateStamp";
                chart1.Series[gSeries].YValueMembers= gas.ToString();
                chart1.Series[gSeries].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
                chart1.Series[gSeries].MarkerStyle = System.Windows.Forms.DataVisualization.Charting.MarkerStyle.Circle;
                chart1.Series[gSeries].XValueType = System.Windows.Forms.DataVisualization.Charting.ChartValueType.Time;
                chart1.Series[gSeries].ToolTip = "#SERIESNAME: #VALY ppm, #VALX";
                // add a menuitem to the contextmenu
                cM.MenuItems.Add(gas.ToString(), new EventHandler(cM_MouseDown));
            }
            // add an ALL context menu item to enable all chart series
                cM.MenuItems.Add("All", new EventHandler(cM_MouseDown));

            // set primary key
            gasDt.PrimaryKey = new DataColumn[] { gasDt.Columns["DateStamp"] };
            // fire event when datatable updates with a new row
            //gasDt.TableNewRow += GasDt_TableNewRow;

            // bind Chart1 to the gasDV dataview
            chart1.DataSource = gasDt;
            chart1.DataBind();

            // init device status dict
            gaseraDStatus = new Dictionary<string, string>();
            gaseraDStatus.Add("0","Initializing");
            gaseraDStatus.Add("1", "Initialization_Error");
            gaseraDStatus.Add("2", "Idle");
            gaseraDStatus.Add("3", "Self_Test");
            gaseraDStatus.Add("4", "Malfunction");
            gaseraDStatus.Add("5", "Measuring");
            gaseraDStatus.Add("6", "Calibrating");
            gaseraDStatus.Add("7", "Laserscan");

            // init error dict
            gaseraErrors = new Dictionary<string, string>();
            gaseraErrors.Add("8001", "Warning: Cell temperature is low");
            gaseraErrors.Add("8002", "Warning: Cell temperature is high");
            gaseraErrors.Add("8003", "Blocker: Cell temperature is low");
            gaseraErrors.Add("8004", "Blocker: Cell temperature is high");
            gaseraErrors.Add("8011", "Critical: Sampling line is blocked");
            gaseraErrors.Add("8012", "Critical: Measured overpressure in sampling line");
            gaseraErrors.Add("8013", "Warning: Cell pressure is incorrect");
            gaseraErrors.Add("8021", "Blocker: Source intensity is too high");
            gaseraErrors.Add("8022", "Blocker: Source intensity is too low");
            gaseraErrors.Add("8023", "Warning: Source intensity is too high");
            gaseraErrors.Add("8024", "Warning: Source intensity is too low");
            gaseraErrors.Add("8025", "Warning: Source voltage is too low");
            gaseraErrors.Add("8026", "Blocker: Source is warming up");
            gaseraErrors.Add("8043", "Blocker: Laser temperature is too low");
            gaseraErrors.Add("8044", "Blocker: Laser temperature is too high");
            gaseraErrors.Add("E001", "Critical: UART communications not working");
            gaseraErrors.Add("E002", "Critical: uDSP has been reset");

            // init measurement status dict
            gaseraMStatus = new Dictionary<string, string>();
            gaseraMStatus.Add("0", "Idle");
            gaseraMStatus.Add("1", "Gas exchange");
            gaseraMStatus.Add("2", "Sample integration");
            gaseraMStatus.Add("3", "Sample analysis");


        }

        private void cM_MouseDown(object sender, EventArgs e)
        {
            MenuItem mI = (MenuItem)sender;
            if (mI.Text != "All")
            {
                // use the menuitem text to toggle visibility
                chart1.Series[mI.Text].Enabled = !chart1.Series[mI.Text].Enabled;
            }
            else
            {
                // enable all gas items in the chart
                for (Gases gas = Gases.CH4; gas < Gases.Water_Offset; gas++)
                    chart1.Series[gas.ToString()].Enabled = true;
            }
        }

        /// <summary>
        ///  Generate the command strings for each 
        ///  Gasera command
        /// </summary>
        /// <param name="cmd">(gaseraCmds) command to build</param>
        /// <param name="gCmd">(byte[]) where to put the command bytes</param>
        private void GenGaseraCmd(gaseraCmds cmd, out byte[] gCmd)
        {
            string cmdStr = cmd.ToString();
            char[] cmdChr = cmdStr.ToCharArray();
            int iCmd = (int)cmd;
            int i = 2;
            gCmd = new byte[MAXCMDBYTECOUNT];
            gCmd[0] = STX;
            gCmd[1] = SPC;
            while (i < cmdStr.Length + 2)
            {
                gCmd[i] = (byte)cmdChr[i - 2];
                i++;
            }
            gCmd[i] = SPC;
            i++;
            gCmd[i] = K;
            i++;
            gCmd[i] = ZERO;
            i++;
            // adding the undocumented trailing space
            gCmd[i] = SPC;
            i++;
            // the start measurement (STAM) command
            // takes an additional parameter, the task number
            // this will be added on when needed.
            // Otherwise all commands end with the ETX as set here
            gCmd[i] = ETX;
            gCmd[MAXCMDBYTECOUNT - 1] = (byte)(i + 1);
        }

        /// <summary>
        /// (re)Initialize UI on start or error
        /// </summary>
        private void initUI()
        {
            // init state tracker
            cState = new commState();
            startStopCommsButton.Text = "Start Communications";
            startMeasurementButton.Enabled = false;
            stopMeasurementButton.Enabled = false;
            commStatusToolStripLabel.Text = ("Disconnected");
            deviceErrorToolStipStatusLabel.Text = (string.Format("Device Errors: {0}", cState.DevErrors));
            deviceStatusToolStripLabel.Text = (string.Format("Device Status: {0}", cState.Status));
            fileAppendRadioButton.Checked = cState.fileAppend;
        }
        private void quitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        /// <summary>
        /// Start or stop communications in the b/g
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void startStopCommsButton_Click(object sender, EventArgs e)
        {
            // validate the ip address
            // the button click does not cause the tb to validate
            ToolStripButton btn = (ToolStripButton)sender;
            if (cState.commsUp)
            {
                // Stop comms
                if (commsBGWorker.IsBusy)
                    commsBGWorker.CancelAsync();
                    btn.Text = "Start Communications";
            }
            else
            {
                if (validateIPAddress(ipTextBox, out gIP))
                {
                    // initialize states
                    cState = new commState();
                    // start communications in b/g task
                    doWorkArg wArg;
                    wArg.ipAddress = gIP;
                    wArg.socket = SOCKNUM;
                    cState.starting = true;
                    commsBGWorker.RunWorkerAsync(wArg);
                    btn.Text = "Stop Communications";
                    // don't allow more starts
                    // btn.Enabled = false;
                }
            }
        }
        /// <summary>
        /// Start measurement
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void startMeasurementButton_Click(object sender, EventArgs e)
        {
            if (cState.commsUp && !cState.measuring)
            {
                // selected task is set on the
                // taskCombobox_SelectionChanged event
                // here, regenerate and update 
                // the start measure command
                // with the selected task value:
                // add the task number followed by a space
                int idx;
                if (cState.selectedTask.Length > 0)
                {
                    idx = (int) gaseraCmds.STAM;
                    GenGaseraCmd(gaseraCmds.STAM, out reqCmds[idx]);
                    int byteCount = reqCmds[idx][MAXCMDBYTECOUNT - 1] - 1;
                    char[] taskChr = cState.selectedTask.ToCharArray();
                    for (int i = 0; i < cState.selectedTask.Length; i++)
                    {
                        reqCmds[idx][byteCount] = (byte)taskChr[i];
                        byteCount++;
                    }
                    reqCmds[idx][byteCount] = SPC;
                    reqCmds[idx][byteCount + 1] = ETX;
                    reqCmds[idx][MAXCMDBYTECOUNT - 1] = (byte) (byteCount + 2);
                    cState.enableMeasure = true;
                }
                else
                    cState.enableMeasure = false;
            }
        }

        /// <summary>
        /// Stop measurement
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void stopMeasurementButton_Click(object sender, EventArgs e)
        {
            if (cState.commsUp)
                cState.enableMeasure  = false;
        }
        /// <summary>
        /// Perform comms tasks in the background
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void commsBGWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker bgW = (BackgroundWorker)sender;
            int rInterval = REPEATINTERVAL;
            try
            {
                // b/g thread(s) loop below until the bgw receives a cancel request
                using (Socket clientSocket = new Socket(SocketType.Stream, ProtocolType.Tcp))
                {
                    doWorkArg dWa = (doWorkArg)e.Argument;
                    string sendStr = string.Empty;
                    // init 
                    clientSocket.ReceiveTimeout = COMMTIMEOUT;
                    clientSocket.SendTimeout = COMMTIMEOUT;
                    clientSocket.ExclusiveAddressUse = true;
                    //clientSocket.NoDelay = true;
                    clientSocket.Connect(dWa.ipAddress, dWa.socket);
                    if (clientSocket.Connected)
                    {
                        cState.commsUp = true;
                        cState.eP = clientSocket.RemoteEndPoint;
                    }

                    // main loop
                    while (!bgW.CancellationPending)
                    {
                        // get device status, errors, and task list

                        // Status command
                        SendReceive(clientSocket, reqCmds[(int)gaseraCmds.ASTS], out cState.respStr);
                        cState.Status = cState.respStr[2];
                        // First time thru after starting comms,
                        // if the device is already measuring, adjust the cState measurement status to that situation
                        if (cState.starting)
                        {
                            string stateStr;
                            if (gaseraDStatus.TryGetValue(cState.Status, out stateStr))
                            {
                                switch (stateStr.ToUpper())
                                {
                                    case "MEASURING":
                                        cState.measuring = true;
                                        cState.enableMeasure = true;
                                        break;
                                    default:
                                        cState.measuring = false;
                                        cState.enableMeasure = false;
                                        break;
                                }
                            }
                            cState.starting = false;
                        }

                        // device measurement phase command
                        SendReceive(clientSocket, reqCmds[(int)gaseraCmds.AMST], out cState.respStr);
                        cState.MeasPhase = cState.respStr[2];
                        

                        // Device Error command
                        SendReceive(clientSocket, reqCmds[(int)gaseraCmds.AERR], out cState.respStr);
                        // catenate to the end of the response
                        // its not clear if these are supposed to be 
                        // delimited by spaces in the response or not
                        // zero element of respStr is echo-back of the command
                        // first element is the response error code,
                        // which is handled in SendReceive()
                        int i = 2;
                        StringBuilder strB = new StringBuilder();
                        while (i < cState.respStr.Length)
                        {
                            strB.Append(cState.respStr[i]);
                            i++;
                        }
                        cState.DevErrors = strB.ToString();
                        cState.haveErrors = strB.Length > 0 ? true : false;


                        // Device tasklist command
                        if (!cState.measuring)
                        {
                            SendReceive(clientSocket, reqCmds[(int)gaseraCmds.ATSK], out cState.respStr);

                            // add each element to Dictionary<int, string>taskList 
                            // delimited by spaces in the response
                            // The instrument has a built-in task called "Calibration task"
                            // but the name is not editable using the default "administrator" login
                            // the embedded space causes it to be parsed apart; need to put it back together
                            // so have to do a fixup if the name is identified
                            // NOTE The code below will fail if a taskname contains a space!
                            DictComparer dComp = new DictComparer();
                            Dictionary<string, string> locTaskList = new Dictionary<string, string>();
                            i = 2;
                            while (i < cState.respStr.Length - 1)
                            {
                                int j;
                                if (int.TryParse(cState.respStr[i], out j))
                                {
                                    // FIX UP for "Calibration task" embedded space
                                    if (cState.respStr[i + 1].ToUpper() == "CALIBRATION" && cState.respStr[i + 2].ToUpper() == "TASK")
                                    {
                                        cState.respStr[i + 2] = "Calibration task";
                                        i++;
                                    }
                                    // remove the following 'else' to allow the
                                    // calibration task to be added to the list of tasks
                                    // the user sees
                                    else
                                        locTaskList.Add(cState.respStr[i], cState.respStr[i + 1]);
                                }
                                i += 2;
                            }

                            // if the new dict contains something, 
                            // and the contents != existing dict contents
                            // then update cState.TaskList with new
                            if (locTaskList.Count > 0 && !dComp.Equals(locTaskList, cState.TaskList))
                            {
                                cState.TaskList = locTaskList;
                                // flag to UI to update the combobox with tasks listed
                                cState.taskListUpdated = true;
                            }
                        }

                        // control measurements,
                        
                        if (!cState.haveErrors && cState.enableMeasure)
                        {
                            if (cState.measuring)
                            {
                                // parse the response for timestamps, gas identities, and concentrations
                                // format is ACON 0 <time0> <component0> <ppm0> <time1> <component1> <ppm1> <time2> <component2> <ppm2> ... 
                                // These will have been parsed into cState.respStr[]
                                // Unique data points are identified by their timestamps
                                // The timestamps correspond to the start of analysis and
                                // should be the same for each gas in any one concentration response (assumption...)
                                // and all of the gas values for a datestamp appear on one line (assumption...)

                                // Add each gas conc to datatable if no data has been previously added at this time
                                // the response from the instrument must have at least 5 space-delimited components:
                                // ACON 0 <time> <gas> <conc>

                                string[] measStr;
                                int j = 2;
                                DataRow dR;
                                DateTime tStamp;
                                long epochSeconds;
                                string gasName;
                                double conc;
                                string casSearchString;
                                StringBuilder logStr = new StringBuilder();

                                // read current concentrations
                                SendReceive(clientSocket, reqCmds[(int)gaseraCmds.ACON], out measStr);

                                // using the first timestamp (at i = 2) and
                                // assuming the rest of the timestamps
                                // in the response are the same (as documneted by Gasera)
                                // check that there's something here and try to parse the CAS number
                                if (measStr.Length > 4)
                                {
                                    if (long.TryParse(measStr[j], out epochSeconds))
                                    {
                                        // do the time conversion 
                                        tStamp = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                                        tStamp = tStamp.AddSeconds(epochSeconds);
                                        // convert to local time
                                        tStamp = tStamp.ToLocalTime();
                                        // is this time already accounted for?
                                        if (gasDt.Rows.Find(tStamp) == null)
                                        {
                                            dR = gasDt.NewRow();
                                            dR["DateStamp"] = tStamp;
                                            // add the ppm values in this response
                                            while (j < measStr.Length - 2)
                                            {
                                                // gasDict keys are the CAS numbers but
                                                // prefixed with CASPREFIX and 
                                                // with '_' instead of the '-' that's
                                                // received from the monitor
                                                casSearchString = (CASPREFIX + measStr[j + 1]).Replace('-', '_');
                                                if (!casGasDict.TryGetValue(casSearchString, out gasName))
                                                {
                                                    throw new ApplicationException (string.Format ("Unable to find CAS {0} in the gas dictionary", casSearchString));
                                                }
                                                // try to parse the concentration
                                                if (!double.TryParse(measStr[j + 2], out conc))
                                                {
                                                    throw new ApplicationException (string.Format("Unable to parse concentration {0}", measStr[j + 2]));
                                                }

                                                dR[gasName] = conc;
                                                // increment by 3 to skip the next timestamp field
                                                j += 3;
                                            }
                                            // keep this row
                                            gasDt.Rows.Add(dR);
                                            gasDt.AcceptChanges();
                                            // write to log file if enabled
                                            if (cState.fileName.Length > 0 && cState.canOpenFile)
                                            {
                                                // iterate thru the new row, which keeps the order of the gases in line
                                                // with the concentrations

                                                // format date for excel, going into the log file
                                                // dT should already be in UTC, but we'll convert it anyway...
                                                // use an ISO-8601-like format that Excel can understand natively, i.e.
                                                // YYYY-MM-DD hh:mm:ss.s
                                                // have to leave out the T and the timezone from ISO-8601: YYYY-MM-DDThh:mm:ss.sTZD
                                                logStr.AppendFormat("{0}", TimeZoneInfo.ConvertTime(tStamp, TimeZoneInfo.Utc).ToString("yyyy'-'MM'-'dd' 'HH':'mm':'ss'.'f", CultureInfo.InvariantCulture));
                                                for (int k = 1; k < gasDt.Columns.Count; k ++)
                                                {
                                                    logStr.AppendFormat("\t{0}", (double)dR[k]);
                                                }
                                                logStr.Append("\n");
                                                writeLogFile(cState.fileName, logStr.ToString());
                                            }
                                        }
                                    }
                                    else
                                        throw new ApplicationException (string.Format("Unable to parse epoch time {0}", measStr[j]));
                                }
                            }
                            else
                            {
                                // start measuring
                                // the command must have had the task number appended
                                SendReceive(clientSocket, reqCmds[(int)gaseraCmds.STAM], out cState.respStr);
                                cState.measuring = true;
                            }
                        }

                        else if (!cState.enableMeasure & cState.measuring)
                        {
                            // stop measuring if measurement is disabled but ongoing
                            SendReceive(clientSocket, reqCmds[(int)gaseraCmds.STPM], out cState.respStr);
                            cState.measuring = false;
                        }

                        else if (cState.haveErrors )
                        {
                            // assess stop measuring due to error (from up in the UI)
                            // stop measuring only if the error is Critical or Blocking, not Warning
                            // TODO need a string[] for the errors so can catch more than one at a time
                            string errStr;
                            if (gaseraErrors.TryGetValue(cState.DevErrors, out errStr))
                            {
                                if (!errStr.ToUpper().Contains("WARNING"))
                                {
                                    SendReceive(clientSocket, reqCmds[(int)gaseraCmds.STPM], out cState.respStr);
                                    cState.measuring = false;
                                }
                            }
                        }

                        // send progress update
                        bgW.ReportProgress(0, cState);

                        Thread.Sleep(rInterval);
                    } // end of while
                        // Shutdown the socket
                    clientSocket.Shutdown(SocketShutdown.Both);
                    
                    // throw new ApplicationException(string.Format("Unable to open a connection to {0}", clientSocket.RemoteEndPoint.ToString()));
                     
                    // Socket will be closed and destroyed automatically    
                    // upon leaving this using block
                } // end of using

            } // end of try
            catch (SocketException sEx)
            {
                throw sEx;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                cState.commsUp = false;
                cState.measuring = false;
                cState.enableMeasure = false;
                cState.DevErrors = "Not Connected";
                cState.Status = "Not Connected";
                e.Result = cState;
                bgW.ReportProgress(100, cState);

            }

        }
        /// <summary>
        /// sends cmd and waits to receive data back up to the first ETX byte
        /// returns parsed array of strings excluding white and ctrl chars
        /// </summary>
        /// <param name="clientSocket"></param>
        /// <param name="cmd"></param>
        /// <param name="respStr"></param>
        private void SendReceive( Socket clientSocket, byte[] cmd, out string[] respStr)
        {
            // delay a bit
            Thread.Sleep(COMMDELAY);
            // send
            clientSocket.Send (cmd, cmd[cmd.Length - 1], SocketFlags.None);
            // receive bytes until ETX char or end of the rec buffer
            // the socket blocks, so timeout exception will be raised if waiting too long
            // responses are assumed always =< MAXRESPONSECOUNT bytes
            byte[] recBuff = new byte[MAXRESPONSECOUNT];
            int i = 0;
            do
            {
                clientSocket.Receive(recBuff, i, 1, SocketFlags.None);
                if (recBuff[i] == ETX)
                    break;
                i++;
            }
            while (i < MAXRESPONSECOUNT); //recBuff.Length);

            // parse the response
            string tStr = System.Text.Encoding.UTF8.GetString(recBuff, 0, i).Trim();
            respStr = tStr.Split(new char[] {(char)STX, (char)ETX, ' ' }, StringSplitOptions.RemoveEmptyEntries);
            // check for response error flag
            if (respStr.Length < 2 || respStr[1] != "0")
                throw new ApplicationException(string.Format("Error from monitor responding to {0} command. Received: \"{1}\"", System.Text.Encoding.UTF8.GetString(cmd, 0, cmd[cmd.Length - 1]), tStr));
        }

        private void commsBGWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            string statusValue = string.Empty;
            if (cState.commsUp)
            {
                startStopCommsButton.Text = "Stop Communications";
                startMeasurementButton.Enabled = true;
                IPEndPoint ipEp = (IPEndPoint)cState.eP;
                commStatusToolStripLabel.Text = (string.Format("Connected to {0}:{1}", IPAddress.Parse (ipEp.Address.ToString()), ipEp.Port));

                // update the toolstrip status labels
                // The device error status might be more than one error
                // might expand the error display to multiple lines in a control instead of the statusLabel
                string errStr = "None";
                if (cState.haveErrors)
                    errStr = cState.DevErrors;
                deviceErrorToolStipStatusLabel.Text = (string.Format("Device Errors: {0}", errStr));

                // update the device status displayed
                if (! gaseraDStatus.TryGetValue( cState.Status, out statusValue ))
                    MessageBox.Show (string.Format("Invalid device status value received: {0}", cState.Status));
                else
                    deviceStatusToolStripLabel.Text = (string.Format("Device Status: {0}", statusValue ));

                // update the measurement phase displayed, if measuring
                if(cState.measuring)
                    if (!gaseraMStatus.TryGetValue(cState.MeasPhase , out statusValue))
                        MessageBox.Show(string.Format("Invalid measurement phase received: {0}", cState.MeasPhase ));
                    else
                        deviceStatusToolStripLabel.Text += (string.Format(", {0}", statusValue));

                // enable/ disable the start/stop measurement buttons
                stopMeasurementButton.Enabled = cState.measuring;
                startMeasurementButton.Enabled = !cState.measuring;
                if(cState.measuring )
                {
                    //// parse the response for timestamps, gas identities, and concentrations
                    //// format is ACON 0 <time0> <component0> <ppm0> <time1> <component1> <ppm1> <time2> <component2> <ppm2> ... 
                    //// These will have been parsed into cState.respStr[]
                    //// Unique data points are identified by their timestamps
                    //// The timestamps correspond to the start of analysis and
                    //// should be the same for each gas in any one concentration response (assumption...)
                    //// and all of the gas values for a datestamp appear on one line (assumption...)

                    //// Add each gas conc to datatable if no data has been previously added at this time
                    //// the response from the instrument must have at least 5 space-delimited components:
                    //// ACON 0 <time> <gas> <conc>
                    //int i = 2;
                    //DataRow dR;
                    //DateTime tStamp;
                    //long epochSeconds;
                    //string gasName;
                    //double conc;
                    //string casSearchString;

                    //// using the first timestamp (at i = 2) and
                    //// assuming the rest of the timestamps
                    //// in the response are the same
                    //// check that there's something here and try to parse the CAS number
                    //if (cState.measStr.Length > 4)
                    //{
                    //    if (long.TryParse(cState.measStr[i], out epochSeconds))
                    //    {
                    //        // do the time conversion 
                    //        tStamp = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                    //        tStamp = tStamp.AddSeconds(epochSeconds);
                    //        // is this time already accounted for?
                    //        if (gasDt.Rows.Find(tStamp) == null)
                    //        {
                    //            dR = gasDt.NewRow();
                    //            dR["DateStamp"] = tStamp;
                    //            // add the ppm values in this response
                    //            while (i < cState.measStr.Length - 2)
                    //            {
                    //                // gasDict keys are the CAS numbers but
                    //                // prefixed with CASPREFIX and 
                    //                // with '_' instead of the '-' that's
                    //                // received from the monitor
                    //                casSearchString = (CASPREFIX + cState.measStr[i + 1]).Replace('-', '_');
                    //                if (!casGasDict.TryGetValue(casSearchString, out gasName))
                    //                {
                    //                    MessageBox.Show("Unable to find CAS {0} in the gas dictionary", casSearchString );
                    //                    break;
                    //                }
                    //                // try to parse the concentration
                    //                if (!double.TryParse(cState.measStr[i + 2], out conc))
                    //                {
                    //                    MessageBox.Show("Unable to parse concentration {0}", cState.measStr[i + 2]);
                    //                    break;
                    //                }

                    //                dR[gasName] = conc;
                    //                // increment by 3 to skip the next timestamp field
                    //                i += 3;
                    //            }
                    //            // fires GasDt_TableNewRow() event
                    //            // which writes the new info to file
                    //            gasDt.Rows.Add(dR);
                    //            gasDt.AcceptChanges();
                    //        }
                    //    }
                    //    else
                    //        MessageBox.Show("Unable to parse epoch time {0}", cState.measStr[i]);
                    //}
                    // display the information on the UI
                    // a text box, a graph, or a datatview linked to the data table?

                    // update the chart
                   
                    chart1.DataBind();

                    // update the UI to show some action
                    measureProgressBar.PerformStep();
                    if (measureProgressBar.Value >= measureProgressBar.Maximum)
                        measureProgressBar.Value = measureProgressBar.Minimum;
                }
                else if (cState.taskListUpdated)
                {
                    // update task combobox if list has changed while not measuring
                    taskComboBox.Items.Clear();
                    foreach(KeyValuePair<string, string> kvp in cState.TaskList)
                    {
                        taskComboBox.Items.Add(kvp);
                    }
                    // select the first item
                    if (taskComboBox.Items.Count > 0)
                    {
                        taskComboBox.SelectedIndex = 0;
                    }
                    //update the selected task
                    UpdateSelectedTask(taskComboBox);
                    cState.taskListUpdated = false;

                }

                // put last response onto form for debug
                // will be the measure result when measuring, 
                // or the tasklist if not measuring
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i <  cState.respStr.Length; i++)
                    sb.Append(cState.respStr[i]);
                respLabel.Text = sb.ToString();
            }
            //else
            //{
            //    initUI();
            //    // other?
            //}
            startStopCommsButton.Enabled = true;
        }

        private void commsBGWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                MessageBox.Show(e.Error.Message, "Error completing background worker task");
                // reset the UI and cState
                initUI();
            }
            else if (e.Cancelled)
            {
                // purposely cancelled
                MessageBox.Show(e.Error.Message, "Background worker task was cancelled");
                cState.commsUp = false;
            }
            else
            {
                // ended normally
                cState.commsUp = false;
               

            }
        }
        /// <summary>
        /// Validate and update the IP Address from the text string value
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ipTextBox_Validated(object sender, EventArgs e)
        {
            TextBox tb = (TextBox)sender;
            validateIPAddress(tb, out gIP);
        }
        private bool validateIPAddress(TextBox tB, out IPAddress newAddress)
        {
            bool ret = false;
            if (!IPAddress.TryParse(tB.Text, out newAddress))
                errorProvider1.SetError(tB, "Not a valid IP address");
            else
            {
                errorProvider1.SetError(tB, "");
                ret = true;
            }
            return ret;
        }
        private void taskComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            ComboBox cB = (ComboBox)sender;
            UpdateSelectedTask(cB);
        }
        /// <summary>
        /// Updates the selected task string
        /// to the key of the kvp item selected in the ComboBox
        /// </summary>
        private void UpdateSelectedTask(ComboBox cB)
        {
            cState.selectedTask = ((KeyValuePair<string, string>)cB.SelectedItem).Key ; 
        }

        /// <summary>
        /// Raise file open dialog or stop file updating
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void fileOpenButton_Click(object sender, EventArgs e)
        {
            ToolStripButton tsB = (ToolStripButton)sender;
            if(cState.canOpenFile)
            {
                // disable opening the file
                cState.canOpenFile = false;
                // change button label accordingly
                tsB.Text = "Open File...";
            }
            else
            {
                SaveFileDialog mDlg = new SaveFileDialog();
                
                mDlg.DefaultExt = "txt";
                mDlg.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";
                mDlg.Title = "Open or save log file";
                mDlg.ValidateNames = true;
                mDlg.AddExtension = true;
                mDlg.CheckPathExists = true;
                mDlg.CheckFileExists = false;
                mDlg.RestoreDirectory = true;
                if(mDlg.ShowDialog() == DialogResult.OK)
                {
                    // file is opened, written, and closed 
                    // on-demand on GasDt_TableNewRow()
                    // calling writeLogFile()
                    // below sets up the transactions
                    // and initializes the file
                    // cState.canOpenFile already is false;
                    cState.fileName = mDlg.FileName;
                    try
                    {
                        if (!cState.fileAppend)
                            // open the file and write a header
                            initLogFile(cState.fileName);
                        else
                        {
                            // if the file exists can it be opened?
                            if (File.Exists(cState.fileName))
                                writeLogFile(cState.fileName, string.Empty);
                            else
                            {
                                // if the file doen's exist,
                                // open the file and write a header
                                initLogFile(cState.fileName);
                            }
                        }
                        cState.canOpenFile = true;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(string.Format("Error creating or opening log file: {0}. ", cState.fileName, ex.Message));
                        cState.canOpenFile = false;
                    }
                    finally
                    {
                        tsB.Text = cState.canOpenFile ? "Close File" : "Open File ...";
                    }
                }
            }
        }
        /// <summary>
        /// Append the passed string to the file, waiting for
        /// the operation to complete
        /// if the string == string.empty then only open/close
        /// but nothing is written
        /// </summary>
        /// <param name="fileName">full path to the file</param>
        /// <param name="strToWrite">string to be written</param>
        /// <param name="Append">Append to file</param>
        static void writeLogFile(string fileName, string strToWrite, bool append = true)
        {
            using (StreamWriter sW = new StreamWriter (fileName, append, Encoding.UTF8))
            {
                if (strToWrite.Length > 0)
                {
                    sW.WriteAsync(strToWrite );
                }
                sW.Close();
            }
        }

        /// <summary>
        /// Initialize a new logfile and write a header
        /// overwrites the file
        /// </summary>
        private void initLogFile(string fileName)
        {
            // header to use for log file
            StringBuilder header = new StringBuilder();
            header.AppendLine("#PASVERSION=Gasera_One");
            header.AppendLine("#GasID=UNK");
            header.Append("DateStamp");
            // array of gas names in order
            string[] gasNames = Enum.GetNames(typeof(Gases));
            for (int i = (int) Gases.CH4; i < (int) Gases.Water_Offset ; i++)
            {
                header.Append(string.Format("\t{0}", gasNames[i]));
            }
            header.Append("\n");
            writeLogFile(fileName, header.ToString(), false);
        }

        ///// <summary>
        ///// Delegate to update the output data file
        ///// as rows are added
        ///// </summary>
        ///// <param name="sender"></param>
        ///// <param name="e"></param>
        //private void GasDt_TableNewRow(object sender, DataTableNewRowEventArgs e)
        //{
        //    StringBuilder strValue = new StringBuilder(); 
        //    DataRow dR = e.Row;
        //    if (dR["DateStamp"] != null)
        //    {
        //        DateTime dT = (DateTime)dR["DateStamp"]; 
        //        // dT should already be in UTC, but we'll convert it anyway...
        //        // use an ISO-8601-like format that Excel can understand natively
        //        // YYYY-MM-DD hh:mm:ss.s
        //        // have to leave out the T and the timezone from 8601: YYYY-MM-DDThh:mm:ss.sTZD
        //        strValue.AppendFormat("{0}\t", TimeZoneInfo.ConvertTime(dT, TimeZoneInfo.Utc).ToString("yyyy'-'MM'-'dd' 'HH':'mm':'ss'.'f", CultureInfo.InvariantCulture));
        //        // iterate the gas names from the gasDict
        //        // find the value
        //        for (int i = 0; i < casGasDict.Count; i++)
        //            writeLogFile(cState.fileName, strValue.ToString());
        //    }
        //}
        /// <summary>
        /// change file overwrite mode on radiobutton CheckedChanged
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void fileAppendRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            RadioButton rB = (RadioButton)sender;
            cState.fileAppend = rB.Checked;
        }
    }
}
