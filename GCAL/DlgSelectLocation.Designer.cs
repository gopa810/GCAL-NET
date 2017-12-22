namespace GCAL
{
    partial class DlgSelectLocation
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
            this.gvControlContainer1 = new GCAL.Views.GVControlContainer();
            this.SuspendLayout();
            // 
            // gvControlContainer1
            // 
            this.gvControlContainer1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gvControlContainer1.Controller = null;
            this.gvControlContainer1.Location = new System.Drawing.Point(13, 14);
            this.gvControlContainer1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.gvControlContainer1.Name = "gvControlContainer1";
            this.gvControlContainer1.Size = new System.Drawing.Size(933, 656);
            this.gvControlContainer1.TabIndex = 0;
            // 
            // DlgSelectLocation
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(959, 684);
            this.ControlBox = false;
            this.Controls.Add(this.gvControlContainer1);
            this.Name = "DlgSelectLocation";
            this.Text = "DlgSelectLocation";
            this.ResumeLayout(false);

        }

        #endregion

        private Views.GVControlContainer gvControlContainer1;
    }
}