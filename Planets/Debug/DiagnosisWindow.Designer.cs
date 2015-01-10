namespace SimpleTriangle.Debug
{
    partial class DiagnosisWindow
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
            this.m_rscGroupBox = new System.Windows.Forms.GroupBox();
            this.m_waitingThreadsTextbox = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.m_threadsCountTextbox = new System.Windows.Forms.TextBox();
            this.m_rscGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // m_rscGroupBox
            // 
            this.m_rscGroupBox.Controls.Add(this.m_waitingThreadsTextbox);
            this.m_rscGroupBox.Controls.Add(this.label2);
            this.m_rscGroupBox.Controls.Add(this.label1);
            this.m_rscGroupBox.Controls.Add(this.m_threadsCountTextbox);
            this.m_rscGroupBox.Location = new System.Drawing.Point(13, 13);
            this.m_rscGroupBox.Name = "m_rscGroupBox";
            this.m_rscGroupBox.Size = new System.Drawing.Size(368, 91);
            this.m_rscGroupBox.TabIndex = 0;
            this.m_rscGroupBox.TabStop = false;
            this.m_rscGroupBox.Text = "Ressources";
            // 
            // m_waitingThreadsTextbox
            // 
            this.m_waitingThreadsTextbox.Location = new System.Drawing.Point(165, 39);
            this.m_waitingThreadsTextbox.Name = "m_waitingThreadsTextbox";
            this.m_waitingThreadsTextbox.Size = new System.Drawing.Size(197, 20);
            this.m_waitingThreadsTextbox.TabIndex = 3;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(56, 42);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(103, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Threads en attente :";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 19);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(153, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Threads en cours d\'exécution :";
            // 
            // m_threadsCountTextbox
            // 
            this.m_threadsCountTextbox.Location = new System.Drawing.Point(165, 16);
            this.m_threadsCountTextbox.Name = "m_threadsCountTextbox";
            this.m_threadsCountTextbox.Size = new System.Drawing.Size(197, 20);
            this.m_threadsCountTextbox.TabIndex = 0;
            // 
            // DiagnosisWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(393, 108);
            this.Controls.Add(this.m_rscGroupBox);
            this.Name = "DiagnosisWindow";
            this.Text = "DiagnosisWindow";
            this.m_rscGroupBox.ResumeLayout(false);
            this.m_rscGroupBox.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox m_rscGroupBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox m_threadsCountTextbox;
        private System.Windows.Forms.TextBox m_waitingThreadsTextbox;
        private System.Windows.Forms.Label label2;
    }
}