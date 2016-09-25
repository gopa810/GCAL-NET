using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;

using GCAL.Base;
using GCAL.Views;

namespace GCAL.CompositeViews
{
    public partial class CalendarTab : UserControl
    {
        private int p_mode = 0;

        public TResultCalendar m_calendar;
        public TResultCalendar m_calendarToPrint;

        public int printYearStart;
        public int printYearEnd;
        public int printMonthStart;
        public int printMonthEnd;

        public CalendarTabController Controller { get; set; }
        public CLocationRef calLocation = null;
        public GregorianDateTime calStartDate = new GregorianDateTime();
        public GregorianDateTime calEndDate = new GregorianDateTime();

        public CalendarTab()
        {
            InitializeComponent();

            richTextBox1.Dock = DockStyle.Fill;
            calendarTableView1.Dock = DockStyle.Fill;
            pictureBox1.Dock = DockStyle.Fill;


            string s = Properties.Settings.Default.CalendarLocation;
            if (s.Length < 1)
                s = GCGlobal.LastLocation.EncodedString;
            calLocation = new CLocationRef();
            calLocation.EncodedString = s;
            s = Properties.Settings.Default.CalendarStartDate;
            if (s.Length > 1)
                calStartDate.EncodedString = s;
            s = Properties.Settings.Default.CalendarEndDate;
            if (s.Length > 1)
                calEndDate.EncodedString = s;

            if ((calStartDate.year < 1500 || calStartDate.year > 3000)
                || calEndDate.year < 1500 || calEndDate.year > 3000)
            {
                calStartDate = new GregorianDateTime();
                calStartDate.day = 1;
                calEndDate = new GregorianDateTime(calStartDate);
                calEndDate.AddDays(60);
            }

            calendarTableView1.calLocation = calLocation;
            calendarTableView1.LiveRefresh = true;

            SetMode(Properties.Settings.Default.CalendarShowMode);
        }

        public TResultBase getCurrentContent()
        {
            return m_calendar;
        }


        public void SetMode(int i)
        {
            if (i == -1)
            {
                richTextBox1.Visible = false;
                calendarTableView1.Visible = false;
                pictureBox1.Visible = true;
            }
            else if (i == 0)
            {
                richTextBox1.Visible = true;
                calendarTableView1.Visible = false;
                pictureBox1.Visible = false;

                toolStripLabelStart.Visible = true;
                toolStripLabelEnd.Visible = true;
                toolStripButton2.Visible = true;
                toolStripButton3.Visible = true;
                toolStripSeparator3.Visible = true;
                toolStripButton4.Visible = false;
                toolStripButton5.Visible = false;
                toolStripButton6.Visible = false;
                p_mode = i;
                DisplayCalendarResult();
                Properties.Settings.Default.CalendarShowMode = i;
                Properties.Settings.Default.Save();
            }
            else if (i == 1)
            {
                richTextBox1.Visible = true;
                calendarTableView1.Visible = false;
                pictureBox1.Visible = false;

                toolStripLabelStart.Visible = true;
                toolStripLabelEnd.Visible = true;
                toolStripButton2.Visible = true;
                toolStripButton3.Visible = true;
                toolStripSeparator3.Visible = true;
                toolStripButton4.Visible = false;
                toolStripButton5.Visible = false;
                toolStripButton6.Visible = false;
                p_mode = i;
                DisplayCalendarResult();
                Properties.Settings.Default.CalendarShowMode = i;
                Properties.Settings.Default.Save();
            }
            else if (i == 2)
            {
                richTextBox1.Visible = false;
                calendarTableView1.Visible = true;
                pictureBox1.Visible = false;

                toolStripLabelStart.Visible = false;
                toolStripLabelEnd.Visible = false;
                toolStripButton2.Visible = false;
                toolStripButton3.Visible = false;
                toolStripSeparator3.Visible = false;
                toolStripButton4.Visible = true;
                toolStripButton5.Visible = true;
                toolStripButton6.Visible = true;
                p_mode = i;
                Properties.Settings.Default.CalendarShowMode = i;
                Properties.Settings.Default.Save();
            }
        }

        public string LocationText(string s)
        {
            if (s != null)
            {
                toolStripButton1.Text = s;
            }
            return toolStripButton1.Text;
        }
        public string StartDateText(string s)
        {
            if (s != null)
            {
                toolStripButton2.Text = s;
            }
            return toolStripButton2.Text;
        }
        public string EndDateText(string s)
        {
            if (s != null)
            {
                toolStripButton3.Text = s;
            }
            return toolStripButton3.Text;
        }

        private void onLocationClick(object sender, EventArgs e)
        {
            SelectLocationInputPanel d = new SelectLocationInputPanel();
            d.OnLocationSelected += new TBButtonPressed(onLocationDone);
            SelectLocationInputPanelController dc = new SelectLocationInputPanelController(d);
            dc.ShowInContainer(Controller.ViewContainer, GVControlAlign.Center);
        }

        private void onLocationDone(object sender, EventArgs e)
        {
            if (sender is CLocationRef)
            {
                CLocationRef lr = sender as CLocationRef;
                GCGlobal.AddRecentLocation(lr);
                calLocation = lr;
                Recalculate();
            }
        }

        private void onDateRangeClick(object sender, EventArgs e)
        {
            EnterPeriodPanel d = new EnterPeriodPanel();
            d.OnPeriodSelected += new TBButtonPressed(d_OnPeriodSelected);
            d.EarthLocation = calLocation.EARTHDATA();
            d.InputStartDate = calStartDate;
            d.InputEndDate = calEndDate;
            EnterPeriodPanelController dlg15 = new EnterPeriodPanelController(d);
            dlg15.ShowInContainer(Controller.ViewContainer, GVControlAlign.Center);
        }

        private void d_OnPeriodSelected(object sender, EventArgs e)
        {
            if (sender is EnterPeriodPanel)
            {
                EnterPeriodPanel d = sender as EnterPeriodPanel;
                calStartDate = d.InputStartDate;
                calEndDate = d.InputEndDate;
                Recalculate();
            }
        }


        private void toolBarInfoLabel2_ButtonPressed(object sender, EventArgs e)
        {
            EnterPeriodPanel d = new EnterPeriodPanel();
            d.EarthLocation = calLocation.EARTHDATA();
            d.OnPeriodSelected += new TBButtonPressed(d_OnPeriodSelected);
            d.InputStartDate = calStartDate;
            d.InputEndDate = calEndDate;
            EnterPeriodPanelController dlg15 = new EnterPeriodPanelController(d);
            dlg15.ShowInContainer(Controller.ViewContainer, GVControlAlign.Center);
        }

        public void Recalculate()
        {
            bool settingsChanged = false;
            string s = calLocation.EncodedString;
            if (!s.Equals(Properties.Settings.Default.CalendarLocation))
            {
                Properties.Settings.Default.CalendarLocation = s;
                settingsChanged = true;
            }
            s = calStartDate.EncodedString;
            if (!s.Equals(Properties.Settings.Default.CalendarStartDate))
            {
                Properties.Settings.Default.CalendarStartDate = s;
                settingsChanged = true;
            }
            s = calEndDate.EncodedString;
            if (!s.Equals(Properties.Settings.Default.CalendarEndDate))
            {
                Properties.Settings.Default.CalendarEndDate = s;
                settingsChanged = true;
            }
            if (settingsChanged)
            {
                Properties.Settings.Default.Save();
            }
            if (p_mode == 0 || p_mode == 1)
            {
                LocationText(calLocation.locationName);
                StartDateText(calStartDate.ToString());
                EndDateText(calEndDate.ToString());
                m_calendar = new TResultCalendar();
                m_calendar.CalculateCalendar(calLocation, calStartDate, calEndDate.GetJulianInteger() - calStartDate.GetJulianInteger());
                DisplayCalendarResult();
            }
            else if (p_mode == 2)
            {
                calendarTableView1.calLocation = this.calLocation;
                calendarTableView1.RecalculateCalendar();
            }
        }

        public void DisplayCalendarResult()
        {
            if (p_mode == 0)
            {
                if (m_calendar == null)
                    Recalculate();
                else
                    richTextBox1.Text = m_calendar.formatText(GCDataFormat.PlainText);
            }
            else if (p_mode == 1)
            {
                if (m_calendar == null)
                    Recalculate();
                else
                    richTextBox1.Rtf = m_calendar.formatText(GCDataFormat.Rtf);
            }
        }

        private void exportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (p_mode == 0 || p_mode == 1)
            {
                FrameMain.SaveContentPlain(m_calendar);
            }
            else if (p_mode == 2)
            {
                SaveFileDialog sfd = new SaveFileDialog();

                string dir;
                string locationFileName = calLocation.GetNameAsFileName();
                sfd.Filter = "PNG image - current month (*.png)|*.png|HTML pages - whole year (*.html)|*.html";
                sfd.FileName = locationFileName;

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    dir = Path.GetDirectoryName(sfd.FileName);
                    switch (sfd.FilterIndex)
                    {
                        case 1:
                            CalendarTableDrawer.ExportPng(calLocation, dir, locationFileName, calendarTableView1.CurrentYear, calendarTableView1.CurrentMonth);
                            break;
                        case 2:
                            CalendarTableDrawer.ExportPngYear(calLocation, dir, locationFileName, calendarTableView1.CurrentYear);
                            break;
                    }
                }
            }
        }

        private void printToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (p_mode == 0 || p_mode == 1)
            {
                if (Controller != null)
                    Controller.ExecuteMessage("printContent", m_calendar);
            }
            else if (p_mode == 2)
            {
                PrintDialog pd = new PrintDialog();

                if (pd.ShowDialog() != DialogResult.OK)
                    return;

                DialogResult dr = MessageBox.Show("Do you want to print a whole year ?", "Time Period", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                if (dr == DialogResult.Yes)
                {
                    printMonthStart = 1;
                    printMonthEnd = 12;
                    printYearStart = calendarTableView1.CurrentYear;
                    printYearEnd = printYearStart;
                }
                else if (dr == DialogResult.No)
                {
                    printMonthEnd = printMonthStart = calendarTableView1.CurrentMonth;
                    printYearStart = printYearEnd = calendarTableView1.CurrentYear;
                }
                else
                {
                    printMonthStart = printMonthEnd = -1;
                    printYearStart = printYearEnd = -1;
                }

                if (printYearStart > 0)
                {
                    m_calendarToPrint = new TResultCalendar();
                    GregorianDateTime startDate = new GregorianDateTime(printYearStart, printMonthStart, 1);
                    GregorianDateTime endDate = new GregorianDateTime(printYearEnd, printMonthEnd, 1);
                    m_calendarToPrint.CalculateCalendar(calLocation, startDate, endDate.GetJulianInteger() - startDate.GetJulianInteger() + 31);
                    printDocumentTable.PrinterSettings = pd.PrinterSettings;
                    printDocumentTable.Print();
                }
            }

        }

        private void toolStripButton4_Click(object sender, EventArgs e)
        {
            int mm = calendarTableView1.CurrentYear * 12 + calendarTableView1.CurrentMonth - 1;
            mm--;
            calendarTableView1.LiveRefresh = false;
            calendarTableView1.CurrentYear = mm / 12;
            calendarTableView1.CurrentMonth = mm % 12 + 1;
            calendarTableView1.LiveRefresh = true;
        }

        private void toolStripButton5_Click(object sender, EventArgs e)
        {
            calendarTableView1.LiveRefresh = false;
            DateTime dt = DateTime.Now;
            calendarTableView1.CurrentYear = dt.Year;
            calendarTableView1.CurrentMonth = dt.Month;
            calendarTableView1.LiveRefresh = true;
        }

        private void toolStripButton6_Click(object sender, EventArgs e)
        {
            int mm = calendarTableView1.CurrentYear * 12 + calendarTableView1.CurrentMonth - 1;
            mm++;
            calendarTableView1.LiveRefresh = false;
            calendarTableView1.CurrentYear = mm / 12;
            calendarTableView1.CurrentMonth = mm % 12 + 1;
            calendarTableView1.LiveRefresh = true;
        }

        private void printDocument1_PrintPage(object sender, System.Drawing.Printing.PrintPageEventArgs e)
        {
            if (m_calendarToPrint == null)
            {
                e.HasMorePages = false;
                return;
            }

            CalendarTableDrawer ctd = new CalendarTableDrawer();

            ctd.PaddingTop = Math.Abs(e.MarginBounds.Top - e.PageBounds.Top);
            ctd.PaddingRight = Math.Abs(e.MarginBounds.Right - e.PageBounds.Right);
            ctd.PaddingLeft = Math.Abs(e.PageBounds.Left - e.MarginBounds.Left);
            ctd.PaddingBottom = Math.Abs(e.PageBounds.Bottom - e.MarginBounds.Bottom);

            ctd.Draw(e.Graphics, e.PageBounds.Size, m_calendarToPrint, printYearStart, printMonthStart);

            printMonthStart++;
            if (printMonthStart > 12)
            {
                printMonthStart = 1;
                printYearStart++;
            }
            e.HasMorePages = ((printYearStart * 12 + printMonthStart) <= (printYearEnd * 12 + printMonthEnd));
            return;

        }

        private void toolStripDropDownButton2_DropDownOpening(object sender, EventArgs e)
        {
            plainTextToolStripMenuItem.Checked = (p_mode == 0);
            richTextToolStripMenuItem.Checked = (p_mode == 1);
            tableToolStripMenuItem.Checked = (p_mode == 2);

            smallTextToolStripMenuItem.Enabled = (p_mode != 2);
            smallTextToolStripMenuItem1.Enabled = (p_mode != 2);
            normalTextToolStripMenuItem.Enabled = (p_mode != 2);
            largestTextToolStripMenuItem.Enabled = (p_mode != 2);
            largeTextToolStripMenuItem.Enabled = (p_mode != 2);

            smallTextToolStripMenuItem.Checked = (GCLayoutData.LayoutSizeIndex == 0);
            smallTextToolStripMenuItem1.Checked = (GCLayoutData.LayoutSizeIndex == 1);
            normalTextToolStripMenuItem.Checked = (GCLayoutData.LayoutSizeIndex == 2);
            largeTextToolStripMenuItem.Checked = (GCLayoutData.LayoutSizeIndex == 3);
            largestTextToolStripMenuItem.Checked = (GCLayoutData.LayoutSizeIndex == 4);

        }

        private void plainTextToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SetMode(0);
        }

        private void richTextToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SetMode(1);
        }

        private void tableToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SetMode(2);
        }

        private void smallTextToolStripMenuItem_Click(object sender, EventArgs e)
        {
            GCLayoutData.LayoutSizeIndex = 0;
            DisplayCalendarResult();
        }

        private void smallTextToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            GCLayoutData.LayoutSizeIndex = 1;
            DisplayCalendarResult();
        }

        private void normalTextToolStripMenuItem_Click(object sender, EventArgs e)
        {
            GCLayoutData.LayoutSizeIndex = 2;
            DisplayCalendarResult();
        }

        private void largeTextToolStripMenuItem_Click(object sender, EventArgs e)
        {
            GCLayoutData.LayoutSizeIndex = 3;
            DisplayCalendarResult();
        }

        private void largestTextToolStripMenuItem_Click(object sender, EventArgs e)
        {
            GCLayoutData.LayoutSizeIndex = 4;
            DisplayCalendarResult();
        }
    }
}
