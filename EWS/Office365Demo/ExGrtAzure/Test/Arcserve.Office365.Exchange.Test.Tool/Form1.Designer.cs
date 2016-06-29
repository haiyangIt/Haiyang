namespace Arcserve.Office365.Exchange.Test.Tool
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
            this.txtBinPath = new System.Windows.Forms.TextBox();
            this.txtMsgPath = new System.Windows.Forms.TextBox();
            this.btnChooseBin = new System.Windows.Forms.Button();
            this.btnChooseMsg = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.BtnConvert = new System.Windows.Forms.Button();
            this.btnBinToTxt = new System.Windows.Forms.Button();
            this.btnTestEntitiyFrameWork = new System.Windows.Forms.Button();
            this.txtResult = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // txtBinPath
            // 
            this.txtBinPath.Location = new System.Drawing.Point(62, 12);
            this.txtBinPath.Name = "txtBinPath";
            this.txtBinPath.Size = new System.Drawing.Size(372, 20);
            this.txtBinPath.TabIndex = 1;
            // 
            // txtMsgPath
            // 
            this.txtMsgPath.Location = new System.Drawing.Point(62, 38);
            this.txtMsgPath.Name = "txtMsgPath";
            this.txtMsgPath.Size = new System.Drawing.Size(372, 20);
            this.txtMsgPath.TabIndex = 3;
            // 
            // btnChooseBin
            // 
            this.btnChooseBin.Location = new System.Drawing.Point(452, 10);
            this.btnChooseBin.Name = "btnChooseBin";
            this.btnChooseBin.Size = new System.Drawing.Size(107, 23);
            this.btnChooseBin.TabIndex = 2;
            this.btnChooseBin.Text = "Choose bin...";
            this.btnChooseBin.UseVisualStyleBackColor = true;
            this.btnChooseBin.Click += new System.EventHandler(this.btnChooseBin_Click);
            // 
            // btnChooseMsg
            // 
            this.btnChooseMsg.Location = new System.Drawing.Point(452, 36);
            this.btnChooseMsg.Name = "btnChooseMsg";
            this.btnChooseMsg.Size = new System.Drawing.Size(107, 23);
            this.btnChooseMsg.TabIndex = 4;
            this.btnChooseMsg.Text = "Choose Msg...";
            this.btnChooseMsg.UseVisualStyleBackColor = true;
            this.btnChooseMsg.Click += new System.EventHandler(this.btnChooseMsg_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 15);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(44, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Bin File:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(10, 41);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(46, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "MsgFile:";
            // 
            // BtnConvert
            // 
            this.BtnConvert.Location = new System.Drawing.Point(62, 64);
            this.BtnConvert.Name = "BtnConvert";
            this.BtnConvert.Size = new System.Drawing.Size(107, 23);
            this.BtnConvert.TabIndex = 5;
            this.BtnConvert.Text = "Convert";
            this.BtnConvert.UseVisualStyleBackColor = true;
            this.BtnConvert.Click += new System.EventHandler(this.BtnConvert_Click);
            // 
            // btnBinToTxt
            // 
            this.btnBinToTxt.Location = new System.Drawing.Point(184, 64);
            this.btnBinToTxt.Name = "btnBinToTxt";
            this.btnBinToTxt.Size = new System.Drawing.Size(110, 23);
            this.btnBinToTxt.TabIndex = 6;
            this.btnBinToTxt.Text = "BinToTxt";
            this.btnBinToTxt.UseVisualStyleBackColor = true;
            this.btnBinToTxt.Click += new System.EventHandler(this.btnBinToTxt_Click);
            // 
            // btnTestEntitiyFrameWork
            // 
            this.btnTestEntitiyFrameWork.Location = new System.Drawing.Point(15, 147);
            this.btnTestEntitiyFrameWork.Name = "btnTestEntitiyFrameWork";
            this.btnTestEntitiyFrameWork.Size = new System.Drawing.Size(154, 23);
            this.btnTestEntitiyFrameWork.TabIndex = 7;
            this.btnTestEntitiyFrameWork.Text = "btnTestEntitiyFrameWork";
            this.btnTestEntitiyFrameWork.UseVisualStyleBackColor = true;
            this.btnTestEntitiyFrameWork.Click += new System.EventHandler(this.btnTestEntitiyFrameWork_Click);
            // 
            // txtResult
            // 
            this.txtResult.Location = new System.Drawing.Point(15, 176);
            this.txtResult.Multiline = true;
            this.txtResult.Name = "txtResult";
            this.txtResult.Size = new System.Drawing.Size(544, 221);
            this.txtResult.TabIndex = 8;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(595, 419);
            this.Controls.Add(this.txtResult);
            this.Controls.Add(this.btnTestEntitiyFrameWork);
            this.Controls.Add(this.btnBinToTxt);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.BtnConvert);
            this.Controls.Add(this.btnChooseMsg);
            this.Controls.Add(this.btnChooseBin);
            this.Controls.Add(this.txtMsgPath);
            this.Controls.Add(this.txtBinPath);
            this.Name = "Form1";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txtBinPath;
        private System.Windows.Forms.TextBox txtMsgPath;
        private System.Windows.Forms.Button btnChooseBin;
        private System.Windows.Forms.Button btnChooseMsg;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button BtnConvert;
        private System.Windows.Forms.Button btnBinToTxt;
        private System.Windows.Forms.Button btnTestEntitiyFrameWork;
        private System.Windows.Forms.TextBox txtResult;
    }
}

