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
using GCAL.CalendarDataView;

namespace GCAL.CompositeViews
{
    public partial class CalendarTab : UserControl, CDVDataSource
    {
        private int p_mode = 0;

        public TResultCalendar m_calendar;
        public TResultCalendar m_calendarToPrint;

        public int printYearStart;
        public int printYearEnd;
        public int printMonthStart;
        public int printMonthEnd;

        public CalendarTabController Controller { get; set; }
        public GCLocation calLocation = null;
        public GregorianDateTime calStartDate = new GregorianDateTime();
        public GregorianDateTime calEndDate = new GregorianDateTime();

        public CalendarTab()
        {
            InitializeComponent();

            calendarDataView1.Dock = DockStyle.Fill;
            richTextBox1.Visible = false;
            //OLDVIEW: richTextBox1.Dock = DockStyle.Fill;
            calendarTableView1.Dock = DockStyle.Fill;
            pictureBox1.Dock = DockStyle.Fill;


            string s = Properties.Settings.Default.CalendarLocation;
            if (s.Length < 1)
                s = GCGlobal.LastLocation.EncodedString;
            calLocation = new GCLocation();
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

            calendarDataView1.DataSource = this;
            calendarDataView1.InitWithKey("2017-10");

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
                calendarDataView1.Visible = false;
                //OLDVIEW: richTextBox1.Visible = false;
                calendarTableView1.Visible = false;
                pictureBox1.Visible = true;
            }
            else if (i == 0)
            {
                calendarDataView1.Visible = true;
                //OLDVIEW: richTextBox1.Visible = true;
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
                calendarDataView1.Visible = true;
                //OLDVIEW: richTextBox1.Visible = true;
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
                calendarDataView1.Visible = false;
                //OLDVIEW: richTextBox1.Visible = false;
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
            if (sender is GCLocation)
            {
                GCLocation lr = sender as GCLocation;
                GCGlobal.AddRecentLocation(lr);
                calLocation = lr;
                Recalculate();
            }
        }

        private void onDateRangeClick(object sender, EventArgs e)
        {
            EnterPeriodPanel d = new EnterPeriodPanel();
            d.OnPeriodSelected += new TBButtonPressed(d_OnPeriodSelected);
            d.EarthLocation = calLocation.GetEarthData();
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
            d.EarthLocation = calLocation.GetEarthData();
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
                LocationText(calLocation.Title);
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
                    printDocumentTable.DocumentName = string.Format("{0}_{1}", calLocation.Title, printYearStart);
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

        private void printMultipleToolStripMenuItem_Click(object sender, EventArgs e)
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

        private void ekadasiMapToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int step = 0;
            VAISNAVADAY dayEkadasi = null;
            GregorianDateTime dateEkadasi = null;
            GCMap selectedMap = null;

            while (step >= 0)
            {
                switch(step)
                {
                    case 0: // select ekadasi
                        DlgGetEkadasiName dlg = new DlgGetEkadasiName();
                        if (dlg.ShowDialog() == DialogResult.OK)
                        {
                            dayEkadasi = dlg.SelectedDate;
                            dateEkadasi = dlg.SelectedDate.date;
                            step = 1;
                        }
                        else
                        {
                            step = -1;
                        }
                        break;
                    case 1: // select location map
                        DlgSelectMap maps = new DlgSelectMap();
                        if (maps.ShowDialog() == DialogResult.OK)
                        {
                            step = 2;
                            selectedMap = maps.SelectedMap;
                            if (selectedMap == null)
                                step = -1;
                        }
                        else
                        {
                            step = -1;
                        }
                        break;
                    case 2: // calculate Ekadasi map
                        DlgCalcEkadasiBoundaries d2 = new DlgCalcEkadasiBoundaries(dayEkadasi.date);
                        d2.SelectedDate = dateEkadasi;
                        d2.SelectedVaisnavaDay = dayEkadasi;
                        d2.FindType = 1;
                        d2.SelectedMap = selectedMap;
                        d2.Show();
                        step = -1;
                        break;
                }
            }
        }

        private void exportCompleteDataToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ExportCompleteDataDlg dlg = new ExportCompleteDataDlg();

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                ExportCompleteProgressDlg ep = new ExportCompleteProgressDlg();

                ep.SetData(dlg.SelectedLocations, dlg.StartYear, dlg.EndYear, dlg.SelectedDirectory,
                    dlg.includeSun, dlg.includeCore);
                ep.Show();

                ep.Start(1);
            }
        }

        public delegate void AsyncTask1(CDVDataTarget req, CDVDocumentCell cell);
        public delegate void AsyncTask2(CDVDocumentCell result);

        public void AsyncRequestData(CDVDataTarget requestor, CDVDocumentCell data)
        {
            //FetchCalendarDataSync(requestor, data);

            AsyncTask1 at = new AsyncTask1(FetchCalendarDataSync);
            at.BeginInvoke(requestor, data, null, null);
        }

        private void FetchCalendarDataSync(CDVDataTarget requestor, CDVDocumentCell data)
        {
            // TODO:
            // here calculate data
            // but this function should perform asynchronously
            int i_year, i_month;
            string[] sa = data.Key.Split('-');
            if (sa.Length != 2 || !int.TryParse(sa[0], out i_year) || !int.TryParse(sa[1], out i_month))
                return;

            if (i_month < 2)
                data.PrevKey = string.Format("{0}-{1}", i_year - 1, 12);
            else
                data.PrevKey = string.Format("{0}-{1}", i_year, i_month - 1);

            if (i_month > 11)
                data.NextKey = string.Format("{0}-{1}", i_year + 1, 1);
            else
                data.NextKey = string.Format("{0}-{1}", i_year, i_month + 1);


            TResultCalendar rc = new TResultCalendar(calLocation, i_year, i_month);

            CDVParaStyle pstitle = new CDVParaStyle();
            pstitle.StyleName = "TitleC";
            pstitle.Align = CDVAlign.Center;
            pstitle.Padding.All = 12;
            pstitle.Margin.All = 12;
            pstitle.BorderWidth.All = 1;
            pstitle.BorderColor = CDVColor.Black;
            pstitle.BackgroundColor = CDVColor.LightYellow;

            CDVParaStyle pshdr = new CDVParaStyle();
            pshdr.StyleName = "Hdr";
            pshdr.Margin.Top = 2;
            pshdr.Margin.Bottom = 12;
            pshdr.BackgroundColor = new CDVColor(255, 64, 32, 0);

            CDVParaStyle ps = new CDVParaStyle();
            ps.StyleName = "Centered";
            ps.Align = CDVAlign.Center;

            CDVTextStyle ts = new CDVTextStyle();
            ts.FontSize = 18;

            CDVTextStyle tsWeekday = new CDVTextStyle();
            tsWeekday.FontSize = 9;

            CDVTextStyle tsHdr = new CDVTextStyle();
            tsHdr.Color = CDVColor.White;
            tsHdr.FontSize = 10;

            CDVRuler atithi = new CDVRuler(null, 10);
            CDVRuler aday = new CDVRuler(null, 0);
            CDVRuler apak = new CDVRuler(null);
            CDVRuler anak = new CDVRuler(null);
            CDVRuler ayog = new CDVRuler(null);
            CDVRuler awd = new CDVRuler(null, 0);
            CDVRuler afast = new CDVRuler(null, 0);
            CDVRuler amr = new CDVRuler(null);
            CDVRuler amsun1 = new CDVRuler(null);
            CDVRuler amsun2 = new CDVRuler(null);
            CDVRuler amsun3 = new CDVRuler(null);

            CDVAtom doc = requestor.GetDocument();
            CDVPara para = new CDVPara(doc, CDVOrientation.Vertical,
                new CDVPara(null, CDVOrientation.Vertical, pstitle, CDVSpan.Maximum,
                    new CDVWord(null, string.Format("{0} {1}", GregorianDateTime.GetMonthName(i_month) ,i_year), ts, ps, CDVSpan.Maximum),
                    new CDVWord(null, calLocation.Format("{locationName}, {longitudeText} {latitudeText}, {timeZoneName}"), ps, CDVSpan.Maximum)
                )
            );

            CDVPara row = new CDVPara(para, CDVOrientation.Horizontal, pshdr, tsHdr);
            para.Add(row);
            row.Add(new CDVWord(row, "Date"));
            row.Add(aday);
            row.Add(new CDVWord(row, ""));
            row.Add(awd);
            row.Add(new CDVWord(row, "Tithi/Festival/Paksa"));
            row.Add(atithi);
            if (GCDisplaySettings.getBoolValue(39))
            {
                row.Add(new CDVWord(row, ""));
                row.Add(apak);
            }
            if (GCDisplaySettings.getBoolValue(36))
            {
                row.Add(new CDVWord(row, "Naksatra"));
                row.Add(anak);
            }
            if (GCDisplaySettings.getBoolValue(37))
            {
                row.Add(new CDVWord(row, "Yoga"));
                row.Add(ayog);
            }
            if (GCDisplaySettings.getBoolValue(38))
            {
                row.Add(new CDVWord(row, "Fast"));
                row.Add(afast);
            }
            if (GCDisplaySettings.getBoolValue(41))
            {
                row.Add(new CDVWord(row, "Moon Rasi"));
                row.Add(amr);
            }
            if (GCDisplaySettings.getBoolValue(GCDS.CAL_COL_SUNRISE))
            {
                row.Add(new CDVWord(row, "Sunrise"));
                row.Add(amsun1);
            }
            if (GCDisplaySettings.getBoolValue(GCDS.CAL_COL_NOON))
            {
                row.Add(new CDVWord(row, "Noon"));
                row.Add(amsun2);
            }
            if (GCDisplaySettings.getBoolValue(GCDS.CAL_COL_SUNSET))
            {
                row.Add(new CDVWord(row, "Sunset"));
                row.Add(amsun3);
            }

            for (int i = 0; i < rc.m_PureCount; i++)
            {
                VAISNAVADAY vd = rc.GetDay(i);
                row = new CDVPara(para, CDVOrientation.Horizontal);
                para.Add(row);
                row.Add(new CDVWord(row, vd.date.ToString()));
                row.Add(aday);
                row.Add(new CDVWord(row, GCCalendar.GetWeekdayAbbr(vd.date.dayOfWeek), tsWeekday));
                row.Add(awd);
                row.Add(new CDVWord(row, vd.GetFullTithiName()));
                row.Add(atithi);
                if (GCDisplaySettings.getBoolValue(39))
                {
                    row.Add(new CDVWord(row, GCPaksa.GetAbbr(vd.astrodata.sunRise.Paksa).ToString()));
                    row.Add(apak);
                }
                if (GCDisplaySettings.getBoolValue(36))
                {
                    row.Add(new CDVWord(row, GCNaksatra.GetName(vd.astrodata.sunRise.Naksatra)));
                    row.Add(anak);
                }
                if (GCDisplaySettings.getBoolValue(37))
                {
                    row.Add(new CDVWord(row, GCYoga.GetName(vd.astrodata.sunRise.Yoga)));
                    row.Add(ayog);
                }
                if (GCDisplaySettings.getBoolValue(38))
                {
                    row.Add(new CDVWord(row, vd.nFastID > 0 ? "*" : ""));
                    row.Add(afast);
                }
                if (GCDisplaySettings.getBoolValue(41))
                {
                    row.Add(new CDVWord(row, GCRasi.GetName(vd.astrodata.sunRise.RasiOfMoon)));
                    row.Add(amr);
                }

                if (GCDisplaySettings.getBoolValue(GCDS.CAL_COL_SUNRISE))
                {
                    row.Add(new CDVWord(row, vd.astrodata.sunRise.ToShortTimeString()));
                    row.Add(amsun1);
                }

                if (GCDisplaySettings.getBoolValue(GCDS.CAL_COL_NOON))
                {
                    row.Add(new CDVWord(row, vd.astrodata.sunNoon.ToShortTimeString()));
                    row.Add(amsun2);
                }

                if (GCDisplaySettings.getBoolValue(GCDS.CAL_COL_SUNSET))
                {
                    row.Add(new CDVWord(row, vd.astrodata.sunSet.ToShortTimeString()));
                    row.Add(amsun3);
                }

                List<VAISNAVAEVENT> evs = vd.VisibleEvents;
                if (evs.Count > 0 && vd.astrodata.sunRise.longitude >= 0.0)
                {
                    foreach (VAISNAVAEVENT ve in evs)
                    {
                        para.Add(new CDVPara(null, CDVOrientation.Horizontal,
                            awd,
                            new CDVWord(null, ve.text)
                        ));
                    }

                }
            }

            data.Item = para;

            AsyncTask2 at2 = new AsyncTask2(requestor.OnCDVDataAvailable);

            while(!this.IsHandleCreated)
            {
                System.Threading.Thread.Sleep(100);
            }
            this.Invoke(at2, data);
            // then return calculated data
            //requestor.OnCDVDataAvailable(data);
        }
    }
}
