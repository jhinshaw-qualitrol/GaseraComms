namespace GaseraComms
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea1 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Legend legend1 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            System.Windows.Forms.DataVisualization.Charting.Series series1 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.deviceStatusToolStripLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.deviceErrorToolStipStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.commStatusToolStripLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.measureProgressBar = new System.Windows.Forms.ToolStripProgressBar();
            this.toolStripContainer1 = new System.Windows.Forms.ToolStripContainer();
            this.chart1 = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.overWriteRadioButton = new System.Windows.Forms.RadioButton();
            this.fileAppendRadioButton = new System.Windows.Forms.RadioButton();
            this.label2 = new System.Windows.Forms.Label();
            this.taskComboBox = new System.Windows.Forms.ComboBox();
            this.respLabel = new System.Windows.Forms.Label();
            this.ipTextBox = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.fileOpenButton = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.startStopCommsButton = new System.Windows.Forms.ToolStripButton();
            this.startMeasurementButton = new System.Windows.Forms.ToolStripButton();
            this.stopMeasurementButton = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.quitButton = new System.Windows.Forms.ToolStripButton();
            this.commsBGWorker = new System.ComponentModel.BackgroundWorker();
            this.errorProvider1 = new System.Windows.Forms.ErrorProvider(this.components);
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.statusStrip1.SuspendLayout();
            this.toolStripContainer1.BottomToolStripPanel.SuspendLayout();
            this.toolStripContainer1.ContentPanel.SuspendLayout();
            this.toolStripContainer1.LeftToolStripPanel.SuspendLayout();
            this.toolStripContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.chart1)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.toolStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider1)).BeginInit();
            this.SuspendLayout();
            // 
            // statusStrip1
            // 
            this.statusStrip1.Dock = System.Windows.Forms.DockStyle.None;
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.deviceStatusToolStripLabel,
            this.deviceErrorToolStipStatusLabel,
            this.commStatusToolStripLabel,
            this.measureProgressBar});
            this.statusStrip1.Location = new System.Drawing.Point(0, 0);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(807, 24);
            this.statusStrip1.TabIndex = 1;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // deviceStatusToolStripLabel
            // 
            this.deviceStatusToolStripLabel.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.Right;
            this.deviceStatusToolStripLabel.Name = "deviceStatusToolStripLabel";
            this.deviceStatusToolStripLabel.Size = new System.Drawing.Size(81, 19);
            this.deviceStatusToolStripLabel.Text = "Device Status";
            // 
            // deviceErrorToolStipStatusLabel
            // 
            this.deviceErrorToolStipStatusLabel.BorderSides = ((System.Windows.Forms.ToolStripStatusLabelBorderSides)((System.Windows.Forms.ToolStripStatusLabelBorderSides.Left | System.Windows.Forms.ToolStripStatusLabelBorderSides.Right)));
            this.deviceErrorToolStipStatusLabel.Name = "deviceErrorToolStipStatusLabel";
            this.deviceErrorToolStipStatusLabel.Size = new System.Drawing.Size(71, 19);
            this.deviceErrorToolStipStatusLabel.Text = "Error Status";
            // 
            // commStatusToolStripLabel
            // 
            this.commStatusToolStripLabel.Name = "commStatusToolStripLabel";
            this.commStatusToolStripLabel.Size = new System.Drawing.Size(79, 19);
            this.commStatusToolStripLabel.Text = "Comm Status";
            // 
            // measureProgressBar
            // 
            this.measureProgressBar.Name = "measureProgressBar";
            this.measureProgressBar.Size = new System.Drawing.Size(100, 18);
            // 
            // toolStripContainer1
            // 
            // 
            // toolStripContainer1.BottomToolStripPanel
            // 
            this.toolStripContainer1.BottomToolStripPanel.Controls.Add(this.statusStrip1);
            // 
            // toolStripContainer1.ContentPanel
            // 
            this.toolStripContainer1.ContentPanel.Controls.Add(this.chart1);
            this.toolStripContainer1.ContentPanel.Controls.Add(this.groupBox1);
            this.toolStripContainer1.ContentPanel.Controls.Add(this.label2);
            this.toolStripContainer1.ContentPanel.Controls.Add(this.taskComboBox);
            this.toolStripContainer1.ContentPanel.Controls.Add(this.respLabel);
            this.toolStripContainer1.ContentPanel.Controls.Add(this.ipTextBox);
            this.toolStripContainer1.ContentPanel.Controls.Add(this.label1);
            this.toolStripContainer1.ContentPanel.Size = new System.Drawing.Size(676, 476);
            this.toolStripContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            // 
            // toolStripContainer1.LeftToolStripPanel
            // 
            this.toolStripContainer1.LeftToolStripPanel.Controls.Add(this.toolStrip1);
            this.toolStripContainer1.Location = new System.Drawing.Point(0, 0);
            this.toolStripContainer1.Name = "toolStripContainer1";
            this.toolStripContainer1.Size = new System.Drawing.Size(807, 525);
            this.toolStripContainer1.TabIndex = 2;
            this.toolStripContainer1.Text = "toolStripContainer1";
            // 
            // chart1
            // 
            chartArea1.Name = "ChartArea1";
            this.chart1.ChartAreas.Add(chartArea1);
            legend1.Name = "Legend1";
            this.chart1.Legends.Add(legend1);
            this.chart1.Location = new System.Drawing.Point(56, 165);
            this.chart1.Name = "chart1";
            series1.ChartArea = "ChartArea1";
            series1.Legend = "Legend1";
            series1.Name = "Series1";
            this.chart1.Series.Add(series1);
            this.chart1.Size = new System.Drawing.Size(591, 292);
            this.chart1.TabIndex = 6;
            this.chart1.Text = "chart1";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.overWriteRadioButton);
            this.groupBox1.Controls.Add(this.fileAppendRadioButton);
            this.groupBox1.Location = new System.Drawing.Point(56, 17);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(150, 49);
            this.groupBox1.TabIndex = 5;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "File Save Mode";
            // 
            // overWriteRadioButton
            // 
            this.overWriteRadioButton.AutoSize = true;
            this.overWriteRadioButton.Location = new System.Drawing.Point(74, 19);
            this.overWriteRadioButton.Name = "overWriteRadioButton";
            this.overWriteRadioButton.Size = new System.Drawing.Size(70, 17);
            this.overWriteRadioButton.TabIndex = 1;
            this.overWriteRadioButton.TabStop = true;
            this.overWriteRadioButton.Text = "Overwrite";
            this.overWriteRadioButton.UseVisualStyleBackColor = true;
            // 
            // fileAppendRadioButton
            // 
            this.fileAppendRadioButton.AutoSize = true;
            this.fileAppendRadioButton.Checked = true;
            this.fileAppendRadioButton.Location = new System.Drawing.Point(6, 19);
            this.fileAppendRadioButton.Name = "fileAppendRadioButton";
            this.fileAppendRadioButton.Size = new System.Drawing.Size(62, 17);
            this.fileAppendRadioButton.TabIndex = 0;
            this.fileAppendRadioButton.TabStop = true;
            this.fileAppendRadioButton.Text = "Append";
            this.fileAppendRadioButton.UseVisualStyleBackColor = true;
            this.fileAppendRadioButton.CheckedChanged += new System.EventHandler(this.fileAppendRadioButton_CheckedChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(18, 121);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(82, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "Available Tasks";
            // 
            // taskComboBox
            // 
            this.taskComboBox.FormattingEnabled = true;
            this.taskComboBox.Location = new System.Drawing.Point(106, 118);
            this.taskComboBox.Name = "taskComboBox";
            this.taskComboBox.Size = new System.Drawing.Size(100, 21);
            this.taskComboBox.TabIndex = 3;
            this.taskComboBox.SelectedIndexChanged += new System.EventHandler(this.taskComboBox_SelectedIndexChanged);
            // 
            // respLabel
            // 
            this.respLabel.AutoSize = true;
            this.respLabel.Location = new System.Drawing.Point(357, 33);
            this.respLabel.Name = "respLabel";
            this.respLabel.Size = new System.Drawing.Size(39, 13);
            this.respLabel.TabIndex = 2;
            this.respLabel.Text = "Debug";
            // 
            // ipTextBox
            // 
            this.ipTextBox.Location = new System.Drawing.Point(106, 83);
            this.ipTextBox.Name = "ipTextBox";
            this.ipTextBox.Size = new System.Drawing.Size(100, 20);
            this.ipTextBox.TabIndex = 1;
            this.ipTextBox.Validated += new System.EventHandler(this.ipTextBox_Validated);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(42, 83);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(58, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "IP Address";
            // 
            // toolStrip1
            // 
            this.toolStrip1.Dock = System.Windows.Forms.DockStyle.None;
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.startStopCommsButton,
            this.toolStripSeparator3,
            this.fileOpenButton,
            this.toolStripSeparator2,
            this.startMeasurementButton,
            this.stopMeasurementButton,
            this.toolStripSeparator1,
            this.quitButton});
            this.toolStrip1.Location = new System.Drawing.Point(0, 3);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(131, 158);
            this.toolStrip1.TabIndex = 0;
            // 
            // fileOpenButton
            // 
            this.fileOpenButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.fileOpenButton.Image = ((System.Drawing.Image)(resources.GetObject("fileOpenButton.Image")));
            this.fileOpenButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.fileOpenButton.Name = "fileOpenButton";
            this.fileOpenButton.Size = new System.Drawing.Size(129, 19);
            this.fileOpenButton.Text = "Open File ...";
            this.fileOpenButton.Click += new System.EventHandler(this.fileOpenButton_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(129, 6);
            // 
            // startStopCommsButton
            // 
            this.startStopCommsButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.startStopCommsButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.startStopCommsButton.Name = "startStopCommsButton";
            this.startStopCommsButton.Size = new System.Drawing.Size(129, 19);
            this.startStopCommsButton.Text = "Start Communications";
            this.startStopCommsButton.ToolTipText = "Start or Stop Ethernet Communications";
            this.startStopCommsButton.Click += new System.EventHandler(this.startStopCommsButton_Click);
            // 
            // startMeasurementButton
            // 
            this.startMeasurementButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.startMeasurementButton.Enabled = false;
            this.startMeasurementButton.Image = ((System.Drawing.Image)(resources.GetObject("startMeasurementButton.Image")));
            this.startMeasurementButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.startMeasurementButton.Name = "startMeasurementButton";
            this.startMeasurementButton.Size = new System.Drawing.Size(129, 19);
            this.startMeasurementButton.Text = "Start Measurement";
            this.startMeasurementButton.ToolTipText = "Start measurement using the selected task";
            this.startMeasurementButton.Click += new System.EventHandler(this.startMeasurementButton_Click);
            // 
            // stopMeasurementButton
            // 
            this.stopMeasurementButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.stopMeasurementButton.Enabled = false;
            this.stopMeasurementButton.Image = ((System.Drawing.Image)(resources.GetObject("stopMeasurementButton.Image")));
            this.stopMeasurementButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.stopMeasurementButton.Name = "stopMeasurementButton";
            this.stopMeasurementButton.Size = new System.Drawing.Size(129, 19);
            this.stopMeasurementButton.Text = "Stop Measurement";
            this.stopMeasurementButton.ToolTipText = "Stop measurement";
            this.stopMeasurementButton.Click += new System.EventHandler(this.stopMeasurementButton_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(129, 6);
            // 
            // quitButton
            // 
            this.quitButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.quitButton.Image = ((System.Drawing.Image)(resources.GetObject("quitButton.Image")));
            this.quitButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.quitButton.Name = "quitButton";
            this.quitButton.Size = new System.Drawing.Size(129, 19);
            this.quitButton.Text = "Quit Program";
            this.quitButton.Click += new System.EventHandler(this.quitToolStripMenuItem_Click);
            // 
            // commsBGWorker
            // 
            this.commsBGWorker.WorkerReportsProgress = true;
            this.commsBGWorker.WorkerSupportsCancellation = true;
            this.commsBGWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(this.commsBGWorker_DoWork);
            this.commsBGWorker.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.commsBGWorker_ProgressChanged);
            this.commsBGWorker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.commsBGWorker_RunWorkerCompleted);
            // 
            // errorProvider1
            // 
            this.errorProvider1.ContainerControl = this;
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(129, 6);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(807, 525);
            this.Controls.Add(this.toolStripContainer1);
            this.Name = "Form1";
            this.Text = "Gasera One Communications";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.toolStripContainer1.BottomToolStripPanel.ResumeLayout(false);
            this.toolStripContainer1.BottomToolStripPanel.PerformLayout();
            this.toolStripContainer1.ContentPanel.ResumeLayout(false);
            this.toolStripContainer1.ContentPanel.PerformLayout();
            this.toolStripContainer1.LeftToolStripPanel.ResumeLayout(false);
            this.toolStripContainer1.LeftToolStripPanel.PerformLayout();
            this.toolStripContainer1.ResumeLayout(false);
            this.toolStripContainer1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.chart1)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel deviceStatusToolStripLabel;
        private System.Windows.Forms.ToolStripStatusLabel deviceErrorToolStipStatusLabel;
        private System.Windows.Forms.ToolStripContainer toolStripContainer1;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton startStopCommsButton;
        private System.Windows.Forms.ToolStripButton startMeasurementButton;
        private System.Windows.Forms.ToolStripButton stopMeasurementButton;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripButton quitButton;
        private System.ComponentModel.BackgroundWorker commsBGWorker;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox ipTextBox;
        private System.Windows.Forms.ErrorProvider errorProvider1;
        private System.Windows.Forms.ToolStripStatusLabel commStatusToolStripLabel;
        private System.Windows.Forms.Label respLabel;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox taskComboBox;
        private System.Windows.Forms.ToolStripProgressBar measureProgressBar;
        private System.Windows.Forms.ToolStripButton fileOpenButton;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.RadioButton overWriteRadioButton;
        private System.Windows.Forms.RadioButton fileAppendRadioButton;
        private System.Windows.Forms.DataVisualization.Charting.Chart chart1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
    }
}

