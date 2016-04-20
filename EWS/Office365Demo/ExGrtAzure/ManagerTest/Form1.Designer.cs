namespace ManagerTest
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
            this.BtnStart = new System.Windows.Forms.Button();
            this.BtnEnd = new System.Windows.Forms.Button();
            this.BtnAddJob = new System.Windows.Forms.Button();
            this.BtnCancelJob = new System.Windows.Forms.Button();
            this.AddLine = new System.Windows.Forms.Button();
            this.button6 = new System.Windows.Forms.Button();
            this.JobList = new System.Windows.Forms.ListBox();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // BtnStart
            // 
            this.BtnStart.Location = new System.Drawing.Point(61, 57);
            this.BtnStart.Name = "BtnStart";
            this.BtnStart.Size = new System.Drawing.Size(75, 23);
            this.BtnStart.TabIndex = 0;
            this.BtnStart.Text = "Start";
            this.BtnStart.UseVisualStyleBackColor = true;
            this.BtnStart.Click += new System.EventHandler(this.BtnStart_Click);
            // 
            // BtnEnd
            // 
            this.BtnEnd.Location = new System.Drawing.Point(186, 57);
            this.BtnEnd.Name = "BtnEnd";
            this.BtnEnd.Size = new System.Drawing.Size(75, 23);
            this.BtnEnd.TabIndex = 1;
            this.BtnEnd.Text = "End";
            this.BtnEnd.UseVisualStyleBackColor = true;
            this.BtnEnd.Click += new System.EventHandler(this.BtnEnd_Click);
            // 
            // BtnAddJob
            // 
            this.BtnAddJob.Location = new System.Drawing.Point(61, 104);
            this.BtnAddJob.Name = "BtnAddJob";
            this.BtnAddJob.Size = new System.Drawing.Size(75, 23);
            this.BtnAddJob.TabIndex = 2;
            this.BtnAddJob.Text = "AddJob";
            this.BtnAddJob.UseVisualStyleBackColor = true;
            this.BtnAddJob.Click += new System.EventHandler(this.BtnAddJob_Click);
            // 
            // BtnCancelJob
            // 
            this.BtnCancelJob.Location = new System.Drawing.Point(186, 104);
            this.BtnCancelJob.Name = "BtnCancelJob";
            this.BtnCancelJob.Size = new System.Drawing.Size(75, 23);
            this.BtnCancelJob.TabIndex = 3;
            this.BtnCancelJob.Text = "CancelJob";
            this.BtnCancelJob.UseVisualStyleBackColor = true;
            this.BtnCancelJob.Click += new System.EventHandler(this.BtnCancelJob_Click);
            // 
            // AddLine
            // 
            this.AddLine.Location = new System.Drawing.Point(309, 104);
            this.AddLine.Name = "AddLine";
            this.AddLine.Size = new System.Drawing.Size(75, 23);
            this.AddLine.TabIndex = 4;
            this.AddLine.Text = "AddLine";
            this.AddLine.UseVisualStyleBackColor = true;
            this.AddLine.Click += new System.EventHandler(this.AddLine_Click);
            // 
            // button6
            // 
            this.button6.Location = new System.Drawing.Point(431, 104);
            this.button6.Name = "button6";
            this.button6.Size = new System.Drawing.Size(75, 23);
            this.button6.TabIndex = 5;
            this.button6.Text = "button6";
            this.button6.UseVisualStyleBackColor = true;
            // 
            // JobList
            // 
            this.JobList.FormattingEnabled = true;
            this.JobList.Location = new System.Drawing.Point(61, 155);
            this.JobList.Name = "JobList";
            this.JobList.Size = new System.Drawing.Size(445, 615);
            this.JobList.TabIndex = 6;
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(545, 155);
            this.textBox1.Multiline = true;
            this.textBox1.Name = "textBox1";
            this.textBox1.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textBox1.Size = new System.Drawing.Size(925, 619);
            this.textBox1.TabIndex = 7;
            this.textBox1.WordWrap = false;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1482, 824);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.JobList);
            this.Controls.Add(this.button6);
            this.Controls.Add(this.AddLine);
            this.Controls.Add(this.BtnCancelJob);
            this.Controls.Add(this.BtnAddJob);
            this.Controls.Add(this.BtnEnd);
            this.Controls.Add(this.BtnStart);
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button BtnStart;
        private System.Windows.Forms.Button BtnEnd;
        private System.Windows.Forms.Button BtnAddJob;
        private System.Windows.Forms.Button BtnCancelJob;
        private System.Windows.Forms.Button AddLine;
        private System.Windows.Forms.Button button6;
        private System.Windows.Forms.ListBox JobList;
        private System.Windows.Forms.TextBox textBox1;
    }
}

