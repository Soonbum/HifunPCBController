namespace HifunPCBController
{
    partial class HifunPCBController
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            cboPorts = new ComboBox();
            btnConnect = new Button();
            btnDisconnect = new Button();
            btnCheckPos = new Button();
            btnBuzzerOn = new Button();
            btnLedOn = new Button();
            btnLedOff = new Button();
            rtbLog = new RichTextBox();
            btnClear = new Button();
            btnBuzzerOff = new Button();
            btnListen = new Button();
            SuspendLayout();
            // 
            // cboPorts
            // 
            cboPorts.FormattingEnabled = true;
            cboPorts.Location = new Point(12, 12);
            cboPorts.Name = "cboPorts";
            cboPorts.Size = new Size(113, 23);
            cboPorts.TabIndex = 0;
            // 
            // btnConnect
            // 
            btnConnect.Location = new Point(12, 41);
            btnConnect.Name = "btnConnect";
            btnConnect.Size = new Size(113, 45);
            btnConnect.TabIndex = 1;
            btnConnect.Text = "Connect";
            btnConnect.UseVisualStyleBackColor = true;
            btnConnect.Click += btnConnect_Click;
            // 
            // btnDisconnect
            // 
            btnDisconnect.Location = new Point(133, 41);
            btnDisconnect.Name = "btnDisconnect";
            btnDisconnect.Size = new Size(113, 45);
            btnDisconnect.TabIndex = 2;
            btnDisconnect.Text = "Disconnect";
            btnDisconnect.UseVisualStyleBackColor = true;
            btnDisconnect.Click += btnDisconnect_Click;
            // 
            // btnCheckPos
            // 
            btnCheckPos.Location = new Point(12, 92);
            btnCheckPos.Name = "btnCheckPos";
            btnCheckPos.Size = new Size(113, 45);
            btnCheckPos.TabIndex = 3;
            btnCheckPos.Text = "CheckPos";
            btnCheckPos.UseVisualStyleBackColor = true;
            btnCheckPos.Click += btnCheckPos_Click;
            // 
            // btnBuzzerOn
            // 
            btnBuzzerOn.Location = new Point(131, 92);
            btnBuzzerOn.Name = "btnBuzzerOn";
            btnBuzzerOn.Size = new Size(113, 45);
            btnBuzzerOn.TabIndex = 4;
            btnBuzzerOn.Text = "Buzzer On";
            btnBuzzerOn.UseVisualStyleBackColor = true;
            btnBuzzerOn.Click += btnBuzzerOn_Click;
            // 
            // btnLedOn
            // 
            btnLedOn.Location = new Point(12, 143);
            btnLedOn.Name = "btnLedOn";
            btnLedOn.Size = new Size(113, 45);
            btnLedOn.TabIndex = 5;
            btnLedOn.Text = "LED On";
            btnLedOn.UseVisualStyleBackColor = true;
            btnLedOn.Click += btnLedOn_Click;
            // 
            // btnLedOff
            // 
            btnLedOff.Location = new Point(133, 143);
            btnLedOff.Name = "btnLedOff";
            btnLedOff.Size = new Size(113, 45);
            btnLedOff.TabIndex = 6;
            btnLedOff.Text = "LED Off";
            btnLedOff.UseVisualStyleBackColor = true;
            btnLedOff.Click += btnLedOff_Click;
            // 
            // rtbLog
            // 
            rtbLog.Location = new Point(11, 223);
            rtbLog.Name = "rtbLog";
            rtbLog.Size = new Size(471, 299);
            rtbLog.TabIndex = 7;
            rtbLog.Text = "";
            // 
            // btnClear
            // 
            btnClear.Location = new Point(11, 528);
            btnClear.Name = "btnClear";
            btnClear.Size = new Size(471, 35);
            btnClear.TabIndex = 8;
            btnClear.Text = "Clear";
            btnClear.UseVisualStyleBackColor = true;
            btnClear.Click += btnClear_Click;
            // 
            // btnBuzzerOff
            // 
            btnBuzzerOff.Location = new Point(250, 92);
            btnBuzzerOff.Name = "btnBuzzerOff";
            btnBuzzerOff.Size = new Size(113, 45);
            btnBuzzerOff.TabIndex = 9;
            btnBuzzerOff.Text = "Buzzer Off";
            btnBuzzerOff.UseVisualStyleBackColor = true;
            btnBuzzerOff.Click += btnBuzzerOff_Click;
            // 
            // btnListen
            // 
            btnListen.Location = new Point(252, 41);
            btnListen.Name = "btnListen";
            btnListen.Size = new Size(113, 45);
            btnListen.TabIndex = 10;
            btnListen.Text = "Listen";
            btnListen.UseVisualStyleBackColor = true;
            btnListen.Click += btnListen_Click;
            // 
            // HifunPCBController
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(498, 577);
            Controls.Add(btnListen);
            Controls.Add(btnBuzzerOff);
            Controls.Add(btnClear);
            Controls.Add(rtbLog);
            Controls.Add(btnLedOff);
            Controls.Add(btnLedOn);
            Controls.Add(btnBuzzerOn);
            Controls.Add(btnCheckPos);
            Controls.Add(btnDisconnect);
            Controls.Add(btnConnect);
            Controls.Add(cboPorts);
            Name = "HifunPCBController";
            Text = "HifunPCBController";
            Load += HifunPCBController_Load;
            ResumeLayout(false);
        }

        #endregion

        private ComboBox cboPorts;
        private Button btnConnect;
        private Button btnDisconnect;
        private Button btnCheckPos;
        private Button btnBuzzerOn;
        private Button btnLedOn;
        private Button btnLedOff;
        private RichTextBox rtbLog;
        private Button btnClear;
        private Button btnBuzzerOff;
        private Button btnListen;
    }
}
