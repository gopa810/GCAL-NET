namespace GCAL
{
    partial class FrameMainTable
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrameMainTable));
            this.printDocumentTable = new System.Drawing.Printing.PrintDocument();
            this.gvButtonOrganizer = new GCAL.GVButton();
            this.gvButtonPrint = new GCAL.GVButton();
            this.gvButtonSave = new GCAL.GVButton();
            this.gvButton4 = new GCAL.GVButton();
            this.gvButton3 = new GCAL.GVButton();
            this.gvButton2 = new GCAL.GVButton();
            this.gvButton1 = new GCAL.GVButton();
            this.calendarTableView1 = new GCAL.Views.CalendarTableView();
            this.SuspendLayout();
            // 
            // printDocumentTable
            // 
            this.printDocumentTable.PrintPage += new System.Drawing.Printing.PrintPageEventHandler(this.printDocumentTable_PrintPage);
            // 
            // gvButtonOrganizer
            // 
            this.gvButtonOrganizer.FlatAppearance.BorderSize = 0;
            this.gvButtonOrganizer.FlatAppearance.MouseOverBackColor = System.Drawing.Color.LightGreen;
            this.gvButtonOrganizer.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.gvButtonOrganizer.Highlighted = false;
            this.gvButtonOrganizer.HighlightedBkgColor = System.Drawing.Color.DarkGreen;
            this.gvButtonOrganizer.HighlightedForeColor = System.Drawing.Color.White;
            this.gvButtonOrganizer.Location = new System.Drawing.Point(431, 2);
            this.gvButtonOrganizer.Name = "gvButtonOrganizer";
            this.gvButtonOrganizer.OverBackColor = System.Drawing.Color.LightGreen;
            this.gvButtonOrganizer.Size = new System.Drawing.Size(75, 38);
            this.gvButtonOrganizer.TabIndex = 13;
            this.gvButtonOrganizer.Text = "Organizer";
            this.gvButtonOrganizer.UseVisualStyleBackColor = true;
            this.gvButtonOrganizer.Click += new System.EventHandler(this.gvButtonOrganizer_Click);
            // 
            // gvButtonPrint
            // 
            this.gvButtonPrint.FlatAppearance.BorderSize = 0;
            this.gvButtonPrint.FlatAppearance.MouseOverBackColor = System.Drawing.Color.LightGreen;
            this.gvButtonPrint.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.gvButtonPrint.Highlighted = false;
            this.gvButtonPrint.HighlightedBkgColor = System.Drawing.Color.DarkGreen;
            this.gvButtonPrint.HighlightedForeColor = System.Drawing.Color.White;
            this.gvButtonPrint.Location = new System.Drawing.Point(374, 2);
            this.gvButtonPrint.Name = "gvButtonPrint";
            this.gvButtonPrint.OverBackColor = System.Drawing.Color.LightGreen;
            this.gvButtonPrint.Size = new System.Drawing.Size(54, 38);
            this.gvButtonPrint.TabIndex = 12;
            this.gvButtonPrint.Text = "Print";
            this.gvButtonPrint.UseVisualStyleBackColor = true;
            this.gvButtonPrint.Click += new System.EventHandler(this.gvButtonPrint_Click);
            // 
            // gvButtonSave
            // 
            this.gvButtonSave.FlatAppearance.BorderSize = 0;
            this.gvButtonSave.FlatAppearance.MouseOverBackColor = System.Drawing.Color.LightGreen;
            this.gvButtonSave.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.gvButtonSave.Highlighted = false;
            this.gvButtonSave.HighlightedBkgColor = System.Drawing.Color.DarkGreen;
            this.gvButtonSave.HighlightedForeColor = System.Drawing.Color.White;
            this.gvButtonSave.Location = new System.Drawing.Point(315, 2);
            this.gvButtonSave.Name = "gvButtonSave";
            this.gvButtonSave.OverBackColor = System.Drawing.Color.LightGreen;
            this.gvButtonSave.Size = new System.Drawing.Size(56, 38);
            this.gvButtonSave.TabIndex = 11;
            this.gvButtonSave.Text = "Save";
            this.gvButtonSave.UseVisualStyleBackColor = true;
            this.gvButtonSave.Click += new System.EventHandler(this.gvButtonSave_Click);
            // 
            // gvButton4
            // 
            this.gvButton4.FlatAppearance.BorderSize = 0;
            this.gvButton4.FlatAppearance.MouseOverBackColor = System.Drawing.Color.LightGreen;
            this.gvButton4.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.gvButton4.Highlighted = false;
            this.gvButton4.HighlightedBkgColor = System.Drawing.Color.DarkGreen;
            this.gvButton4.HighlightedForeColor = System.Drawing.Color.White;
            this.gvButton4.Location = new System.Drawing.Point(210, 2);
            this.gvButton4.Name = "gvButton4";
            this.gvButton4.OverBackColor = System.Drawing.Color.LightGreen;
            this.gvButton4.Size = new System.Drawing.Size(102, 38);
            this.gvButton4.TabIndex = 10;
            this.gvButton4.Text = "Change Location";
            this.gvButton4.UseVisualStyleBackColor = true;
            this.gvButton4.Click += new System.EventHandler(this.gvButton4_Click);
            // 
            // gvButton3
            // 
            this.gvButton3.FlatAppearance.BorderSize = 0;
            this.gvButton3.FlatAppearance.MouseOverBackColor = System.Drawing.Color.LightGreen;
            this.gvButton3.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.gvButton3.Highlighted = false;
            this.gvButton3.HighlightedBkgColor = System.Drawing.Color.DarkGreen;
            this.gvButton3.HighlightedForeColor = System.Drawing.Color.White;
            this.gvButton3.Location = new System.Drawing.Point(44, 2);
            this.gvButton3.Name = "gvButton3";
            this.gvButton3.OverBackColor = System.Drawing.Color.LightGreen;
            this.gvButton3.Size = new System.Drawing.Size(64, 38);
            this.gvButton3.TabIndex = 9;
            this.gvButton3.Text = "Today";
            this.gvButton3.UseVisualStyleBackColor = true;
            this.gvButton3.Click += new System.EventHandler(this.gvButton3_Click);
            // 
            // gvButton2
            // 
            this.gvButton2.FlatAppearance.BorderSize = 0;
            this.gvButton2.FlatAppearance.MouseOverBackColor = System.Drawing.Color.LightGreen;
            this.gvButton2.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.gvButton2.Font = new System.Drawing.Font("Arial Narrow", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.gvButton2.Highlighted = false;
            this.gvButton2.HighlightedBkgColor = System.Drawing.Color.DarkGreen;
            this.gvButton2.HighlightedForeColor = System.Drawing.Color.White;
            this.gvButton2.Location = new System.Drawing.Point(109, 2);
            this.gvButton2.Name = "gvButton2";
            this.gvButton2.OverBackColor = System.Drawing.Color.LightGreen;
            this.gvButton2.Size = new System.Drawing.Size(38, 38);
            this.gvButton2.TabIndex = 8;
            this.gvButton2.Text = "►";
            this.gvButton2.UseVisualStyleBackColor = true;
            this.gvButton2.Click += new System.EventHandler(this.gvButton2_Click);
            // 
            // gvButton1
            // 
            this.gvButton1.FlatAppearance.BorderSize = 0;
            this.gvButton1.FlatAppearance.MouseOverBackColor = System.Drawing.Color.LightGreen;
            this.gvButton1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.gvButton1.Font = new System.Drawing.Font("Arial Narrow", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.gvButton1.Highlighted = false;
            this.gvButton1.HighlightedBkgColor = System.Drawing.Color.DarkGreen;
            this.gvButton1.HighlightedForeColor = System.Drawing.Color.White;
            this.gvButton1.Location = new System.Drawing.Point(5, 2);
            this.gvButton1.Name = "gvButton1";
            this.gvButton1.OverBackColor = System.Drawing.Color.LightGreen;
            this.gvButton1.Size = new System.Drawing.Size(38, 38);
            this.gvButton1.TabIndex = 7;
            this.gvButton1.Text = "◄";
            this.gvButton1.UseVisualStyleBackColor = true;
            this.gvButton1.Click += new System.EventHandler(this.gvButton1_Click);
            // 
            // calendarTableView1
            // 
            this.calendarTableView1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.calendarTableView1.BackColor = System.Drawing.Color.White;
            this.calendarTableView1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.calendarTableView1.CurrentMonth = 12;
            this.calendarTableView1.CurrentYear = 2015;
            this.calendarTableView1.LiveRefresh = false;
            this.calendarTableView1.Location = new System.Drawing.Point(2, 42);
            this.calendarTableView1.Name = "calendarTableView1";
            this.calendarTableView1.SelectedCalendar = null;
            this.calendarTableView1.Size = new System.Drawing.Size(739, 477);
            this.calendarTableView1.TabIndex = 6;
            // 
            // FrameMainTable
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(743, 521);
            this.Controls.Add(this.gvButtonOrganizer);
            this.Controls.Add(this.gvButtonPrint);
            this.Controls.Add(this.gvButtonSave);
            this.Controls.Add(this.gvButton4);
            this.Controls.Add(this.gvButton3);
            this.Controls.Add(this.gvButton2);
            this.Controls.Add(this.gvButton1);
            this.Controls.Add(this.calendarTableView1);
            this.HelpButton = true;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "FrameMainTable";
            this.Text = "GCAL - Table Calendar";
            this.HelpButtonClicked += new System.ComponentModel.CancelEventHandler(this.FrameMainTable_HelpButtonClicked);
            this.ResumeLayout(false);

        }

        #endregion

        private GVButton gvButton4;
        private GVButton gvButton3;
        private GVButton gvButton2;
        private GVButton gvButton1;
        private Views.CalendarTableView calendarTableView1;
        private GVButton gvButtonSave;
        private GVButton gvButtonPrint;
        private System.Drawing.Printing.PrintDocument printDocumentTable;
        private GVButton gvButtonOrganizer;
    }
}