using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

using GCAL.Base;
using GCAL.Views;

namespace GCAL
{
    public partial class FrameMainTable : Form
    {
        public int printYearStart;
        public int printYearEnd;
        public int printMonthStart;
        public int printMonthEnd;
        public TResultCalendar m_calendarToPrint;


        public FrameMainTable()
        {
            InitializeComponent();

            calendarTableView1.LiveRefresh = true;

        }


        private static DialogResult AskForLocation(FormStartPosition formPos)
        {
            DialogResult dr;
            DlgGetLocation d1 = new DlgGetLocation();
            d1.StartPosition = formPos;
            dr = d1.ShowDialog();
            if (dr == DialogResult.Yes)
            {
                GCGlobal.lastLocation = d1.LocationRef;
            }
            return dr;
        }


        private void gvButton4_Click(object sender, EventArgs e)
        {
            DialogResult dr = DialogResult.None;
            FormStartPosition formPos = FormStartPosition.CenterParent;
            dr = AskForLocation(formPos);
            calendarTableView1.RecalculateCalendar();
        }

        private void gvButton3_Click(object sender, EventArgs e)
        {
            calendarTableView1.LiveRefresh = false;
            DateTime dt = DateTime.Now;
            calendarTableView1.CurrentYear = dt.Year;
            calendarTableView1.CurrentMonth = dt.Month;
            calendarTableView1.LiveRefresh = true;

        }

        private void gvButton1_Click(object sender, EventArgs e)
        {
            int mm = calendarTableView1.CurrentYear * 12 + calendarTableView1.CurrentMonth - 1;
            mm--;
            calendarTableView1.LiveRefresh = false;
            calendarTableView1.CurrentYear = mm / 12;
            calendarTableView1.CurrentMonth = mm % 12 + 1;
            calendarTableView1.LiveRefresh = true;
        }

        private void gvButton2_Click(object sender, EventArgs e)
        {
            int mm = calendarTableView1.CurrentYear * 12 + calendarTableView1.CurrentMonth - 1;
            mm++;
            calendarTableView1.LiveRefresh = false;
            calendarTableView1.CurrentYear = mm / 12;
            calendarTableView1.CurrentMonth = mm % 12 + 1;
            calendarTableView1.LiveRefresh = true;
        }

        private void gvButtonSave_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();

            string dir;
            string locationFileName = GCGlobal.lastLocation.GetNameAsFileName();
            sfd.Filter = "PNG image - current month (*.png)|*.png|HTML pages - whole year (*.html)|*.html";
            sfd.FileName = locationFileName;

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                dir = Path.GetDirectoryName(sfd.FileName);
                switch (sfd.FilterIndex)
                {
                    case 1:
                        CalendarTableDrawer.ExportPng(dir, locationFileName, calendarTableView1.CurrentYear, calendarTableView1.CurrentMonth);
                        break;
                    case 2:
                        CalendarTableDrawer.ExportPngYear(dir, locationFileName, calendarTableView1.CurrentYear);
                        break;
                }
            }
        }

        private void gvButtonPrint_Click(object sender, EventArgs e)
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
                m_calendarToPrint.CalculateCalendar(GCGlobal.lastLocation, startDate, endDate.GetJulianInteger() - startDate.GetJulianInteger() + 31);
                printDocumentTable.PrinterSettings = pd.PrinterSettings;
                printDocumentTable.Print();
            }

        }

        private void printDocumentTable_PrintPage(object sender, System.Drawing.Printing.PrintPageEventArgs e)
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

        private void FrameMainTable_HelpButtonClicked(object sender, CancelEventArgs e)
        {
            MessageBox.Show("Table Calendar window provides overview of calendar events in table style. When saving or printing the content, current year showed in the window is used for generating calendar pages.");
        }

        private void gvButtonOrganizer_Click(object sender, EventArgs e)
        {
            FrameOrganizer.ShowFrame();
        }
    }
}
