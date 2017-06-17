namespace MonitorForm
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
            this.richTextBox1 = new System.Windows.Forms.RichTextBox();
            this.button_Stop = new System.Windows.Forms.Button();
            this.button_Start = new System.Windows.Forms.Button();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.BTN_PlayBackByTime = new System.Windows.Forms.Button();
            this.DTP_Start = new System.Windows.Forms.DateTimePicker();
            this.DTP_End = new System.Windows.Forms.DateTimePicker();
            this.BTN_DownloadByTime = new System.Windows.Forms.Button();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.listViewIPChannel = new System.Windows.Forms.ListView();
            this.ColumnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.ColumnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.BTN_GetList = new System.Windows.Forms.Button();
            this.BTN_StopDownload = new System.Windows.Forms.Button();
            this.BTN_StopPlayback = new System.Windows.Forms.Button();
            this.BTN_Pause = new System.Windows.Forms.Button();
            this.BTN_Resume = new System.Windows.Forms.Button();
            this.BTN_Slow = new System.Windows.Forms.Button();
            this.BTN_Fast = new System.Windows.Forms.Button();
            this.BTN_Normal = new System.Windows.Forms.Button();
            this.BTN_LiveView = new System.Windows.Forms.Button();
            this.BTN_STOPLiveView = new System.Windows.Forms.Button();
            this.BTN_PlayBackCapture = new System.Windows.Forms.Button();
            this.BTN_LiveViewCapture = new System.Windows.Forms.Button();
            this.BTN_LiveViewCaptureJPG = new System.Windows.Forms.Button();
            this.BTN_Reverse = new System.Windows.Forms.Button();
            this.listViewFile = new System.Windows.Forms.ListView();
            this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader4 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader5 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.BTN_DownloadByName = new System.Windows.Forms.Button();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.textBox2 = new System.Windows.Forms.TextBox();
            this.listView1 = new System.Windows.Forms.ListView();
            this.columnHeader6 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.radioButton1 = new System.Windows.Forms.RadioButton();
            this.radioButton2 = new System.Windows.Forms.RadioButton();
            this.LABEL_CardNum = new System.Windows.Forms.Label();
            this.BTN_Clear = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.textBox3 = new System.Windows.Forms.TextBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.checkBox2 = new System.Windows.Forms.CheckBox();
            this.checkBox1 = new System.Windows.Forms.CheckBox();
            this.label7 = new System.Windows.Forms.Label();
            this.textBox4 = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.timer2 = new System.Windows.Forms.Timer(this.components);
            this.pictureBox2 = new System.Windows.Forms.PictureBox();
            this.label8 = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).BeginInit();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // richTextBox1
            // 
            this.richTextBox1.Location = new System.Drawing.Point(12, 54);
            this.richTextBox1.Name = "richTextBox1";
            this.richTextBox1.Size = new System.Drawing.Size(165, 427);
            this.richTextBox1.TabIndex = 5;
            this.richTextBox1.Text = "";
            // 
            // button_Stop
            // 
            this.button_Stop.Location = new System.Drawing.Point(111, 22);
            this.button_Stop.Name = "button_Stop";
            this.button_Stop.Size = new System.Drawing.Size(75, 23);
            this.button_Stop.TabIndex = 6;
            this.button_Stop.Text = "Stop";
            this.button_Stop.UseVisualStyleBackColor = true;
            this.button_Stop.Click += new System.EventHandler(this.button_Stop_Click);
            // 
            // button_Start
            // 
            this.button_Start.Location = new System.Drawing.Point(12, 22);
            this.button_Start.Name = "button_Start";
            this.button_Start.Size = new System.Drawing.Size(75, 23);
            this.button_Start.TabIndex = 7;
            this.button_Start.Text = "Start";
            this.button_Start.UseVisualStyleBackColor = true;
            this.button_Start.Click += new System.EventHandler(this.button_Start_Click);
            // 
            // pictureBox1
            // 
            this.pictureBox1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pictureBox1.Location = new System.Drawing.Point(20, 166);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(593, 293);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox1.TabIndex = 8;
            this.pictureBox1.TabStop = false;
            // 
            // menuStrip1
            // 
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(1433, 24);
            this.menuStrip1.TabIndex = 9;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // BTN_PlayBackByTime
            // 
            this.BTN_PlayBackByTime.Location = new System.Drawing.Point(327, 5);
            this.BTN_PlayBackByTime.Name = "BTN_PlayBackByTime";
            this.BTN_PlayBackByTime.Size = new System.Drawing.Size(108, 37);
            this.BTN_PlayBackByTime.TabIndex = 10;
            this.BTN_PlayBackByTime.Text = "Play back by time";
            this.BTN_PlayBackByTime.UseVisualStyleBackColor = true;
            this.BTN_PlayBackByTime.Click += new System.EventHandler(this.BTN_PlayBackByTime_Click);
            // 
            // DTP_Start
            // 
            this.DTP_Start.CustomFormat = "yyyy-MM-dd HH:mm:ss";
            this.DTP_Start.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.DTP_Start.Location = new System.Drawing.Point(71, 12);
            this.DTP_Start.Name = "DTP_Start";
            this.DTP_Start.ShowUpDown = true;
            this.DTP_Start.Size = new System.Drawing.Size(138, 20);
            this.DTP_Start.TabIndex = 11;
            this.DTP_Start.Value = new System.DateTime(2017, 6, 15, 0, 0, 0, 0);
            // 
            // DTP_End
            // 
            this.DTP_End.CustomFormat = "yyyy-MM-dd HH:mm:ss";
            this.DTP_End.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.DTP_End.Location = new System.Drawing.Point(71, 43);
            this.DTP_End.Name = "DTP_End";
            this.DTP_End.ShowUpDown = true;
            this.DTP_End.Size = new System.Drawing.Size(138, 20);
            this.DTP_End.TabIndex = 12;
            this.DTP_End.Value = new System.DateTime(2017, 6, 15, 0, 10, 0, 0);
            // 
            // BTN_DownloadByTime
            // 
            this.BTN_DownloadByTime.Location = new System.Drawing.Point(441, 5);
            this.BTN_DownloadByTime.Name = "BTN_DownloadByTime";
            this.BTN_DownloadByTime.Size = new System.Drawing.Size(108, 37);
            this.BTN_DownloadByTime.TabIndex = 13;
            this.BTN_DownloadByTime.Text = "Download by time";
            this.BTN_DownloadByTime.UseVisualStyleBackColor = true;
            this.BTN_DownloadByTime.Click += new System.EventHandler(this.BTN_DownloadByTime_Click);
            // 
            // timer1
            // 
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // progressBar1
            // 
            this.progressBar1.Location = new System.Drawing.Point(74, 16);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(416, 28);
            this.progressBar1.Step = 1;
            this.progressBar1.TabIndex = 14;
            // 
            // listViewIPChannel
            // 
            this.listViewIPChannel.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.ColumnHeader1,
            this.ColumnHeader2});
            this.listViewIPChannel.FullRowSelect = true;
            this.listViewIPChannel.GridLines = true;
            this.listViewIPChannel.Location = new System.Drawing.Point(183, 54);
            this.listViewIPChannel.MultiSelect = false;
            this.listViewIPChannel.Name = "listViewIPChannel";
            this.listViewIPChannel.Size = new System.Drawing.Size(251, 218);
            this.listViewIPChannel.TabIndex = 33;
            this.listViewIPChannel.UseCompatibleStateImageBehavior = false;
            this.listViewIPChannel.View = System.Windows.Forms.View.Details;
            // 
            // ColumnHeader1
            // 
            this.ColumnHeader1.Text = "通道 Channel";
            this.ColumnHeader1.Width = 163;
            // 
            // ColumnHeader2
            // 
            this.ColumnHeader2.Text = "状态 Status";
            this.ColumnHeader2.Width = 90;
            // 
            // BTN_GetList
            // 
            this.BTN_GetList.Location = new System.Drawing.Point(201, 22);
            this.BTN_GetList.Name = "BTN_GetList";
            this.BTN_GetList.Size = new System.Drawing.Size(78, 23);
            this.BTN_GetList.TabIndex = 34;
            this.BTN_GetList.Text = "Get List";
            this.BTN_GetList.UseVisualStyleBackColor = true;
            this.BTN_GetList.Click += new System.EventHandler(this.BTN_GetList_Click);
            // 
            // BTN_StopDownload
            // 
            this.BTN_StopDownload.Location = new System.Drawing.Point(441, 93);
            this.BTN_StopDownload.Name = "BTN_StopDownload";
            this.BTN_StopDownload.Size = new System.Drawing.Size(108, 37);
            this.BTN_StopDownload.TabIndex = 35;
            this.BTN_StopDownload.Text = "Stop download";
            this.BTN_StopDownload.UseVisualStyleBackColor = true;
            this.BTN_StopDownload.Click += new System.EventHandler(this.BTN_StopDownload_Click);
            // 
            // BTN_StopPlayback
            // 
            this.BTN_StopPlayback.Location = new System.Drawing.Point(327, 93);
            this.BTN_StopPlayback.Name = "BTN_StopPlayback";
            this.BTN_StopPlayback.Size = new System.Drawing.Size(108, 37);
            this.BTN_StopPlayback.TabIndex = 36;
            this.BTN_StopPlayback.Text = "Stop Playback";
            this.BTN_StopPlayback.UseVisualStyleBackColor = true;
            this.BTN_StopPlayback.Click += new System.EventHandler(this.BTN_StopPlayback_Click);
            // 
            // BTN_Pause
            // 
            this.BTN_Pause.Location = new System.Drawing.Point(20, 145);
            this.BTN_Pause.Name = "BTN_Pause";
            this.BTN_Pause.Size = new System.Drawing.Size(48, 20);
            this.BTN_Pause.TabIndex = 37;
            this.BTN_Pause.Text = "Pause";
            this.BTN_Pause.UseVisualStyleBackColor = true;
            this.BTN_Pause.Click += new System.EventHandler(this.BTN_Pause_Click);
            // 
            // BTN_Resume
            // 
            this.BTN_Resume.Location = new System.Drawing.Point(71, 145);
            this.BTN_Resume.Name = "BTN_Resume";
            this.BTN_Resume.Size = new System.Drawing.Size(56, 20);
            this.BTN_Resume.TabIndex = 38;
            this.BTN_Resume.Text = "Resume";
            this.BTN_Resume.UseVisualStyleBackColor = true;
            this.BTN_Resume.Click += new System.EventHandler(this.BTN_Resume_Click);
            // 
            // BTN_Slow
            // 
            this.BTN_Slow.Location = new System.Drawing.Point(133, 145);
            this.BTN_Slow.Name = "BTN_Slow";
            this.BTN_Slow.Size = new System.Drawing.Size(45, 20);
            this.BTN_Slow.TabIndex = 39;
            this.BTN_Slow.Text = "Slow";
            this.BTN_Slow.UseVisualStyleBackColor = true;
            this.BTN_Slow.Click += new System.EventHandler(this.BTN_Slow_Click);
            // 
            // BTN_Fast
            // 
            this.BTN_Fast.Location = new System.Drawing.Point(184, 145);
            this.BTN_Fast.Name = "BTN_Fast";
            this.BTN_Fast.Size = new System.Drawing.Size(46, 20);
            this.BTN_Fast.TabIndex = 40;
            this.BTN_Fast.Text = "Fast";
            this.BTN_Fast.UseVisualStyleBackColor = true;
            this.BTN_Fast.Click += new System.EventHandler(this.BTN_Fast_Click);
            // 
            // BTN_Normal
            // 
            this.BTN_Normal.Location = new System.Drawing.Point(236, 145);
            this.BTN_Normal.Name = "BTN_Normal";
            this.BTN_Normal.Size = new System.Drawing.Size(53, 20);
            this.BTN_Normal.TabIndex = 41;
            this.BTN_Normal.Text = "Normal";
            this.BTN_Normal.UseVisualStyleBackColor = true;
            this.BTN_Normal.Click += new System.EventHandler(this.BTN_Normal_Click);
            // 
            // BTN_LiveView
            // 
            this.BTN_LiveView.Location = new System.Drawing.Point(213, 5);
            this.BTN_LiveView.Name = "BTN_LiveView";
            this.BTN_LiveView.Size = new System.Drawing.Size(108, 37);
            this.BTN_LiveView.TabIndex = 42;
            this.BTN_LiveView.Text = "Live View";
            this.BTN_LiveView.UseVisualStyleBackColor = true;
            this.BTN_LiveView.Click += new System.EventHandler(this.BTN_LiveView_Click);
            // 
            // BTN_STOPLiveView
            // 
            this.BTN_STOPLiveView.Location = new System.Drawing.Point(213, 93);
            this.BTN_STOPLiveView.Name = "BTN_STOPLiveView";
            this.BTN_STOPLiveView.Size = new System.Drawing.Size(108, 37);
            this.BTN_STOPLiveView.TabIndex = 43;
            this.BTN_STOPLiveView.Text = "Stop Live View";
            this.BTN_STOPLiveView.UseVisualStyleBackColor = true;
            this.BTN_STOPLiveView.Click += new System.EventHandler(this.BTN_STOPLiveView_Click);
            // 
            // BTN_PlayBackCapture
            // 
            this.BTN_PlayBackCapture.Location = new System.Drawing.Point(327, 49);
            this.BTN_PlayBackCapture.Name = "BTN_PlayBackCapture";
            this.BTN_PlayBackCapture.Size = new System.Drawing.Size(108, 37);
            this.BTN_PlayBackCapture.TabIndex = 44;
            this.BTN_PlayBackCapture.Text = "Playback_capture";
            this.BTN_PlayBackCapture.UseVisualStyleBackColor = true;
            this.BTN_PlayBackCapture.Click += new System.EventHandler(this.BTN_PlayBackCapture_Click);
            // 
            // BTN_LiveViewCapture
            // 
            this.BTN_LiveViewCapture.Location = new System.Drawing.Point(213, 49);
            this.BTN_LiveViewCapture.Name = "BTN_LiveViewCapture";
            this.BTN_LiveViewCapture.Size = new System.Drawing.Size(108, 37);
            this.BTN_LiveViewCapture.TabIndex = 45;
            this.BTN_LiveViewCapture.Text = "LiveView_Capture";
            this.BTN_LiveViewCapture.UseVisualStyleBackColor = true;
            this.BTN_LiveViewCapture.Click += new System.EventHandler(this.BTN_LiveViewCapture_Click);
            // 
            // BTN_LiveViewCaptureJPG
            // 
            this.BTN_LiveViewCaptureJPG.Location = new System.Drawing.Point(73, 93);
            this.BTN_LiveViewCaptureJPG.Name = "BTN_LiveViewCaptureJPG";
            this.BTN_LiveViewCaptureJPG.Size = new System.Drawing.Size(136, 37);
            this.BTN_LiveViewCaptureJPG.TabIndex = 46;
            this.BTN_LiveViewCaptureJPG.Text = "LiveView_CaptureJPG";
            this.BTN_LiveViewCaptureJPG.UseVisualStyleBackColor = true;
            this.BTN_LiveViewCaptureJPG.Click += new System.EventHandler(this.BTN_LiveViewCaptureJPG_Click);
            // 
            // BTN_Reverse
            // 
            this.BTN_Reverse.Location = new System.Drawing.Point(295, 145);
            this.BTN_Reverse.Name = "BTN_Reverse";
            this.BTN_Reverse.Size = new System.Drawing.Size(53, 20);
            this.BTN_Reverse.TabIndex = 47;
            this.BTN_Reverse.Text = "Reverse";
            this.BTN_Reverse.UseVisualStyleBackColor = true;
            this.BTN_Reverse.Visible = false;
            this.BTN_Reverse.Click += new System.EventHandler(this.BTN_Reverse_Click);
            // 
            // listViewFile
            // 
            this.listViewFile.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader3,
            this.columnHeader4,
            this.columnHeader5});
            this.listViewFile.FullRowSelect = true;
            this.listViewFile.GridLines = true;
            this.listViewFile.Location = new System.Drawing.Point(183, 283);
            this.listViewFile.Name = "listViewFile";
            this.listViewFile.Size = new System.Drawing.Size(251, 165);
            this.listViewFile.TabIndex = 50;
            this.listViewFile.UseCompatibleStateImageBehavior = false;
            this.listViewFile.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader3
            // 
            this.columnHeader3.Text = "File Name";
            this.columnHeader3.Width = 80;
            // 
            // columnHeader4
            // 
            this.columnHeader4.Text = "Starting Time";
            this.columnHeader4.Width = 95;
            // 
            // columnHeader5
            // 
            this.columnHeader5.Text = "Ending Time";
            this.columnHeader5.Width = 113;
            // 
            // BTN_DownloadByName
            // 
            this.BTN_DownloadByName.Location = new System.Drawing.Point(441, 49);
            this.BTN_DownloadByName.Name = "BTN_DownloadByName";
            this.BTN_DownloadByName.Size = new System.Drawing.Size(108, 37);
            this.BTN_DownloadByName.TabIndex = 51;
            this.BTN_DownloadByName.Text = "Download by name";
            this.BTN_DownloadByName.UseVisualStyleBackColor = true;
            this.BTN_DownloadByName.Click += new System.EventHandler(this.BTN_DownloadByName_Click);
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(59, 487);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(199, 20);
            this.textBox1.TabIndex = 52;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(18, 490);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(36, 13);
            this.label1.TabIndex = 53;
            this.label1.Text = "Guard";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(22, 516);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(31, 13);
            this.label2.TabIndex = 55;
            this.label2.Text = "Shelf";
            // 
            // textBox2
            // 
            this.textBox2.Location = new System.Drawing.Point(59, 513);
            this.textBox2.Name = "textBox2";
            this.textBox2.Size = new System.Drawing.Size(199, 20);
            this.textBox2.TabIndex = 54;
            // 
            // listView1
            // 
            this.listView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader6});
            this.listView1.FullRowSelect = true;
            this.listView1.GridLines = true;
            this.listView1.Location = new System.Drawing.Point(275, 487);
            this.listView1.Name = "listView1";
            this.listView1.Size = new System.Drawing.Size(159, 130);
            this.listView1.TabIndex = 56;
            this.listView1.UseCompatibleStateImageBehavior = false;
            this.listView1.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader6
            // 
            this.columnHeader6.Text = "RFID Card";
            this.columnHeader6.Width = 252;
            // 
            // radioButton1
            // 
            this.radioButton1.AutoSize = true;
            this.radioButton1.Checked = true;
            this.radioButton1.Location = new System.Drawing.Point(25, 539);
            this.radioButton1.Name = "radioButton1";
            this.radioButton1.Size = new System.Drawing.Size(50, 17);
            this.radioButton1.TabIndex = 57;
            this.radioButton1.TabStop = true;
            this.radioButton1.Text = "Enter";
            this.radioButton1.UseVisualStyleBackColor = true;
            // 
            // radioButton2
            // 
            this.radioButton2.AutoSize = true;
            this.radioButton2.Location = new System.Drawing.Point(99, 539);
            this.radioButton2.Name = "radioButton2";
            this.radioButton2.Size = new System.Drawing.Size(42, 17);
            this.radioButton2.TabIndex = 58;
            this.radioButton2.Text = "Exit";
            this.radioButton2.UseVisualStyleBackColor = true;
            // 
            // LABEL_CardNum
            // 
            this.LABEL_CardNum.AutoSize = true;
            this.LABEL_CardNum.Location = new System.Drawing.Point(22, 594);
            this.LABEL_CardNum.Name = "LABEL_CardNum";
            this.LABEL_CardNum.Size = new System.Drawing.Size(57, 13);
            this.LABEL_CardNum.TabIndex = 59;
            this.LABEL_CardNum.Text = "Card Num:";
            // 
            // BTN_Clear
            // 
            this.BTN_Clear.Location = new System.Drawing.Point(183, 594);
            this.BTN_Clear.Name = "BTN_Clear";
            this.BTN_Clear.Size = new System.Drawing.Size(75, 23);
            this.BTN_Clear.TabIndex = 60;
            this.BTN_Clear.Text = "Clear";
            this.BTN_Clear.UseVisualStyleBackColor = true;
            this.BTN_Clear.Click += new System.EventHandler(this.BTN_Clear_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(18, 559);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(24, 13);
            this.label3.TabIndex = 62;
            this.label3.Text = "text";
            // 
            // textBox3
            // 
            this.textBox3.Location = new System.Drawing.Point(59, 556);
            this.textBox3.Name = "textBox3";
            this.textBox3.Size = new System.Drawing.Size(199, 20);
            this.textBox3.TabIndex = 61;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.checkBox2);
            this.groupBox1.Controls.Add(this.checkBox1);
            this.groupBox1.Controls.Add(this.label7);
            this.groupBox1.Controls.Add(this.textBox4);
            this.groupBox1.Controls.Add(this.label6);
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.progressBar1);
            this.groupBox1.Location = new System.Drawing.Point(440, 27);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(624, 84);
            this.groupBox1.TabIndex = 63;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "状态显示";
            // 
            // checkBox2
            // 
            this.checkBox2.AutoSize = true;
            this.checkBox2.Location = new System.Drawing.Point(500, 16);
            this.checkBox2.Name = "checkBox2";
            this.checkBox2.Size = new System.Drawing.Size(93, 17);
            this.checkBox2.TabIndex = 21;
            this.checkBox2.Text = "启动LiveView";
            this.checkBox2.UseVisualStyleBackColor = true;
            // 
            // checkBox1
            // 
            this.checkBox1.AutoSize = true;
            this.checkBox1.Location = new System.Drawing.Point(500, 31);
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.Size = new System.Drawing.Size(98, 17);
            this.checkBox1.TabIndex = 20;
            this.checkBox1.Text = "启动录制视频";
            this.checkBox1.UseVisualStyleBackColor = true;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(411, 59);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(79, 13);
            this.label7.TabIndex = 19;
            this.label7.Text = "视频录制间隔";
            // 
            // textBox4
            // 
            this.textBox4.Location = new System.Drawing.Point(496, 54);
            this.textBox4.Name = "textBox4";
            this.textBox4.Size = new System.Drawing.Size(114, 20);
            this.textBox4.TabIndex = 18;
            this.textBox4.Text = "10";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label6.Location = new System.Drawing.Point(76, 52);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(43, 20);
            this.label6.TabIndex = 17;
            this.label6.Text = "正常";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(34, 57);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(34, 13);
            this.label5.TabIndex = 16;
            this.label5.Text = "状态:";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(11, 22);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(58, 13);
            this.label4.TabIndex = 15;
            this.label4.Text = "下载进度:";
            // 
            // timer2
            // 
            this.timer2.Tick += new System.EventHandler(this.timer2_Tick);
            // 
            // pictureBox2
            // 
            this.pictureBox2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pictureBox2.Location = new System.Drawing.Point(1070, 63);
            this.pictureBox2.Name = "pictureBox2";
            this.pictureBox2.Size = new System.Drawing.Size(351, 328);
            this.pictureBox2.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox2.TabIndex = 64;
            this.pictureBox2.TabStop = false;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(1390, 43);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(31, 13);
            this.label8.TabIndex = 20;
            this.label8.Text = "截图";
            this.label8.Click += new System.EventHandler(this.label8_Click);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.BTN_DownloadByName);
            this.groupBox2.Controls.Add(this.BTN_Reverse);
            this.groupBox2.Controls.Add(this.BTN_LiveViewCaptureJPG);
            this.groupBox2.Controls.Add(this.BTN_LiveViewCapture);
            this.groupBox2.Controls.Add(this.BTN_PlayBackCapture);
            this.groupBox2.Controls.Add(this.BTN_STOPLiveView);
            this.groupBox2.Controls.Add(this.BTN_LiveView);
            this.groupBox2.Controls.Add(this.BTN_Normal);
            this.groupBox2.Controls.Add(this.BTN_Fast);
            this.groupBox2.Controls.Add(this.BTN_Slow);
            this.groupBox2.Controls.Add(this.BTN_Resume);
            this.groupBox2.Controls.Add(this.BTN_Pause);
            this.groupBox2.Controls.Add(this.BTN_StopPlayback);
            this.groupBox2.Controls.Add(this.BTN_StopDownload);
            this.groupBox2.Controls.Add(this.BTN_DownloadByTime);
            this.groupBox2.Controls.Add(this.DTP_End);
            this.groupBox2.Controls.Add(this.DTP_Start);
            this.groupBox2.Controls.Add(this.BTN_PlayBackByTime);
            this.groupBox2.Controls.Add(this.pictureBox1);
            this.groupBox2.Location = new System.Drawing.Point(440, 117);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(624, 479);
            this.groupBox2.TabIndex = 65;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "LiveView";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1433, 644);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.pictureBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.textBox3);
            this.Controls.Add(this.BTN_Clear);
            this.Controls.Add(this.LABEL_CardNum);
            this.Controls.Add(this.radioButton2);
            this.Controls.Add(this.radioButton1);
            this.Controls.Add(this.listView1);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.textBox2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.listViewFile);
            this.Controls.Add(this.BTN_GetList);
            this.Controls.Add(this.listViewIPChannel);
            this.Controls.Add(this.button_Stop);
            this.Controls.Add(this.button_Start);
            this.Controls.Add(this.richTextBox1);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "Form1";
            this.Text = "广州威拓电子科技有限公司-智能监控";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.Shown += new System.EventHandler(this.Form1_Shown);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).EndInit();
            this.groupBox2.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.RichTextBox richTextBox1;
        private System.Windows.Forms.Button button_Stop;
        private System.Windows.Forms.Button button_Start;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.Button BTN_PlayBackByTime;
        private System.Windows.Forms.DateTimePicker DTP_Start;
        private System.Windows.Forms.DateTimePicker DTP_End;
        private System.Windows.Forms.Button BTN_DownloadByTime;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.ListView listViewIPChannel;
        private System.Windows.Forms.ColumnHeader ColumnHeader1;
        private System.Windows.Forms.ColumnHeader ColumnHeader2;
        private System.Windows.Forms.Button BTN_GetList;
        private System.Windows.Forms.Button BTN_StopDownload;
        private System.Windows.Forms.Button BTN_StopPlayback;
        private System.Windows.Forms.Button BTN_Pause;
        private System.Windows.Forms.Button BTN_Resume;
        private System.Windows.Forms.Button BTN_Slow;
        private System.Windows.Forms.Button BTN_Fast;
        private System.Windows.Forms.Button BTN_Normal;
        private System.Windows.Forms.Button BTN_LiveView;
        private System.Windows.Forms.Button BTN_STOPLiveView;
        private System.Windows.Forms.Button BTN_PlayBackCapture;
        private System.Windows.Forms.Button BTN_LiveViewCapture;
        private System.Windows.Forms.Button BTN_LiveViewCaptureJPG;
        private System.Windows.Forms.Button BTN_Reverse;
        private System.Windows.Forms.ListView listViewFile;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private System.Windows.Forms.ColumnHeader columnHeader4;
        private System.Windows.Forms.ColumnHeader columnHeader5;
        private System.Windows.Forms.Button BTN_DownloadByName;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBox2;
        private System.Windows.Forms.ListView listView1;
        private System.Windows.Forms.ColumnHeader columnHeader6;
        private System.Windows.Forms.RadioButton radioButton1;
        private System.Windows.Forms.RadioButton radioButton2;
        private System.Windows.Forms.Label LABEL_CardNum;
        private System.Windows.Forms.Button BTN_Clear;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox textBox3;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox textBox4;
        private System.Windows.Forms.Timer timer2;
        private System.Windows.Forms.PictureBox pictureBox2;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.CheckBox checkBox1;
        private System.Windows.Forms.CheckBox checkBox2;
        private System.Windows.Forms.GroupBox groupBox2;
    }
}

