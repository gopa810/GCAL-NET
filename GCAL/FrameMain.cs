using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Printing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;

using GCAL.Base;
using GCAL.Views;

namespace GCAL
{
    public partial class FrameMain : Form
    {
        public FrameDelegate frameDelegate;

        public TResultMasaList m_masalist;
        public TResultToday m_today;
        public TResultCalendar m_calendar;
        public TResultEvents m_events;
        public TResultApp m_appday;
        public TResultRatedEvents m_ratedevents;
        public GregorianDateTime m_eventDayE;
        public GregorianDateTime m_eventDayA;
        public Font printFont;
        public string[] stringToPrint;
        public int printLineCurr;

        public bool m_bJumpToFinalStep = false;
        public string m_strTxt;
        public string m_strXml;
        public bool m_bKeyShift;
        public bool m_bKeyControl = false;
        public int m_nInfoType;

        public FrameMain()
        {
            InitializeComponent();

            frameDelegate = new FrameDelegate(this);
            if (GCUserInterface.windowController == null)
                GCUserInterface.windowController = frameDelegate;

            frameDelegate.ShowTipAtStartup();

            panelPlainText.Dock = DockStyle.Fill;
            panelRichText.Dock = DockStyle.Fill;

            ShowMode = GCUserInterface.ShowMode;

            m_events = new TResultEvents();
            m_ratedevents = new TResultRatedEvents();
            m_calendar = new TResultCalendar();
            m_masalist = new TResultMasaList();
            m_appday = new TResultApp();

            normalViewToolStripMenuItem.Checked = (ShowMode == 0);
            enhancedViewToolStripMenuItem.Checked = (ShowMode == 1);

            printFont = new Font("Lucida Console", 10);

            OnCalculateToday();
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CAboutDlg dialog = new CAboutDlg();

            dialog.ShowDialog();
        }

        public int ShowMode
        {
            get
            {
                return GCUserInterface.ShowMode;
            }
            set
            {
                GCUserInterface.ShowMode = value;
                switch (value)
                {
                    case 0:
                        panelPlainText.Visible = true;
                        panelRichText.Visible = false;
                        break;
                    case 1:
                        panelRichText.Visible = true;
                        panelPlainText.Visible = false;
                        break;
                    case 2:
                        panelRichText.Visible = false;
                        panelPlainText.Visible = false;
                        break;
                    default:
                        panelPlainText.Visible = false;
                        panelRichText.Visible = true;
                        break;
                }
            }
        }

        public void OnCalculateToday()
        {
            StringBuilder str = new StringBuilder();
            DateTime st = DateTime.Now;

            GCGlobal.dateTimeShown.day = st.Day;
            GCGlobal.dateTimeShown.month = st.Month;
            GCGlobal.dateTimeShown.year = st.Year;
            GCGlobal.dateTimeShown.shour = 0.5;
            GCGlobal.dateTimeShown.TimezoneHours = GCGlobal.myLocation.offsetUtcHours;
            GCGlobal.dateTimeToday.Set(GCGlobal.dateTimeShown);
            GCGlobal.dateTimeTomorrow.Set(GCGlobal.dateTimeToday);
            GCGlobal.dateTimeTomorrow.NextDay();
            GCGlobal.dateTimeYesterday.Set(GCGlobal.dateTimeToday);
            GCGlobal.dateTimeYesterday.PreviousDay();

            m_today = new TResultToday();
            m_today.Calculate(GCGlobal.dateTimeShown, GCGlobal.myLocation);
            m_nInfoType = MainFrameContentType.MW_MODE_TODAY;

            if (GCUserInterface.ShowMode == 1)
            {
                m_today.formatRtf(str);
                m_textRTF.Rtf = str.ToString();
            }
            else
            {
                m_today.formatPlain(str);
                SetInfoText(GregorianDateTime.GetDateTextWithTodayExt(GCGlobal.dateTimeShown), MainFrameContentType.MW_MODE_TODAY);
                m_strTxt = str.ToString();
                m_textTXT.Text = str.ToString();
            }


        }

        public void OnCalculatePreviousday()
        {
            StringBuilder str = new StringBuilder();
            if (m_nInfoType == MainFrameContentType.MW_MODE_TODAY)
            {
                GCGlobal.dateTimeShown.PreviousDay();
                GCGlobal.dateTimeShown.TimezoneHours = GCGlobal.myLocation.offsetUtcHours;

                m_today.Calculate(GCGlobal.dateTimeShown, GCGlobal.myLocation);

                if (GCUserInterface.ShowMode == 1)
                {
                    m_today.formatRtf(str);
                    m_nInfoType = MainFrameContentType.MW_MODE_TODAY;
                    m_textRTF.Rtf = str.ToString();
                }
                else
                {
                    m_today.formatPlain(str);
                    SetInfoText(GregorianDateTime.GetDateTextWithTodayExt(GCGlobal.dateTimeShown), MainFrameContentType.MW_MODE_TODAY);
                    m_textTXT.Text = str.ToString();
                    m_strTxt = str.ToString();
                }
            }
        }

        public void OnCalculateNextday()
        {
            if (m_nInfoType == MainFrameContentType.MW_MODE_TODAY)
            {
                StringBuilder str = new StringBuilder();

                GCGlobal.dateTimeShown.NextDay();
                GCGlobal.dateTimeShown.TimezoneHours = GCGlobal.myLocation.offsetUtcHours;

                m_today.Calculate(GCGlobal.dateTimeShown, GCGlobal.myLocation);

                if (GCUserInterface.ShowMode == 1)
                {
                    m_today.formatRtf(str);
                    m_nInfoType = MainFrameContentType.MW_MODE_TODAY;
                    m_textRTF.Rtf = str.ToString();
                }
                else
                {
                    m_today.formatPlain(str);
                    SetInfoText(GregorianDateTime.GetDateTextWithTodayExt(GCGlobal.dateTimeShown), MainFrameContentType.MW_MODE_TODAY);
                    m_textTXT.Text = str.ToString();
                    m_strTxt = str.ToString();
                }
            }
        }




        public void SetInfoText(string str, int nType)
        {
            m_Info.Text = str.Replace("&", "&&");
            m_nInfoType = nType;
        }

        private void FrameMain_KeyDown(object sender, KeyEventArgs e)
        {
            m_bKeyControl = e.Control;
            m_bKeyShift = e.Shift;
        }

        private void FrameMain_KeyUp(object sender, KeyEventArgs e)
        {
            m_bKeyControl = e.Control;
            m_bKeyShift = e.Shift;
        }

        private void organizerToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            FrameOrganizer.ShowFrame();
        }

        private void calendarToolStripMenuItem_Click(object sender, EventArgs e)
        {
            runCalendarDialogSequence(1);
        }

        private void coreEventsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            runCoreEventDialogSequence(1);
        }

        private void runCalendarDialogSequence(int initialStage)
        {
            DialogResult dr = DialogResult.None;
            FormStartPosition formPos = FormStartPosition.CenterParent;
            GCEarthData earth = null;
            int stage = initialStage;

            while (stage != 0)
            {
                switch (stage)
                {
                    case 1:
                        DlgGetLocation d1 = new DlgGetLocation();
                        d1.StartPosition = formPos;
                        dr = d1.ShowDialog();
                        if (dr == DialogResult.Yes)
                            earth = d1.LocationRef.EARTHDATA();
                        break;
                    case 2:
                        DlgGetStartDate d2 = new DlgGetStartDate();
                        d2.StartPosition = formPos;
                        d2.EarthLocation = earth;
                        d2.GregorianTime = GCGlobal.dialogStartDate;

                        dr = d2.ShowDialog();
                        if (dr == DialogResult.Yes)
                            GCGlobal.dialogStartDate = d2.GregorianTime;
                        break;
                    case 3:
                        DlgGetEndDate d3 = new DlgGetEndDate();
                        d3.StartPosition = formPos;
                        d3.Panel.EarthLocation = earth;
                        d3.Panel.StartDate = GCGlobal.dialogStartDate;
                        d3.Panel.PeriodLength = 1;
                        d3.Panel.PeriodType = GCCalendar.PeriodUnit.Years;

                        dr = d3.ShowDialog();
                        if (dr == DialogResult.Yes)
                        {
                            GCGlobal.dialogPeriodType = d3.Panel.PeriodType;
                            GCGlobal.dialogPeriodLength = d3.Panel.PeriodLength;
                            GCGlobal.dialogEndDate = d3.Panel.EndDate;
                        }
                        break;
                    case 4:
                        stage = 0;
                        m_nInfoType = MainFrameContentType.MW_MODE_CAL;
                        m_calendar.CalculateCalendar(GCGlobal.lastLocation, 
                            GCGlobal.dialogStartDate,
                            GCGlobal.dialogEndDate.GetJulianInteger() - GCGlobal.dialogStartDate.GetJulianInteger());
                        RecalculateCurrentScreen();
                        return;
                }
                if (dr == DialogResult.Cancel) stage = 0;
                if (dr == DialogResult.No) stage--;
                if (dr == DialogResult.Yes) stage++;
            }
        }

        private void runCoreEventDialogSequence(int initialStage)
        {
            DialogResult dr = DialogResult.None;
            FormStartPosition formPos = FormStartPosition.CenterParent;
            int stage = initialStage;

            while (stage != 0)
            {
                switch (stage)
                {
                    case 1:
                        dr = AskForLocation(formPos);
                        break;
                    case 2:
                        dr = AskForStartDate(formPos);
                        break;
                    case 3:
                        dr = AskForEndDate(formPos);
                        break;
                    case 4:
                        stage = 0;
                        m_nInfoType = MainFrameContentType.MW_MODE_EVENTS;
                        m_events.CalculateEvents(GCGlobal.lastLocation, GCGlobal.dialogStartDate, GCGlobal.dialogEndDate);
                        m_strXml = m_events.FormatXml(null);
                        RecalculateCurrentScreen();
                        return;
                }
                if (dr == DialogResult.Cancel) stage = 0;
                if (dr == DialogResult.No) stage--;
                if (dr == DialogResult.Yes) stage++;
            }
        }

        private static DialogResult AskForEndDate(FormStartPosition formPos)
        {
            DialogResult dr;
            DlgGetEndDate d3 = new DlgGetEndDate();
            d3.StartPosition = formPos;
            d3.Panel.EarthLocation = GCGlobal.lastLocation.EARTHDATA();
            d3.Panel.StartDate = GCGlobal.dialogStartDate;
            d3.Panel.PeriodLength = 1;
            d3.Panel.PeriodType = GCCalendar.PeriodUnit.Years;

            dr = d3.ShowDialog();
            if (dr == DialogResult.Yes)
            {
                GCGlobal.dialogPeriodType = d3.Panel.PeriodType;
                GCGlobal.dialogPeriodLength = d3.Panel.PeriodLength;
                GCGlobal.dialogEndDate = d3.Panel.EndDate;
            }
            return dr;
        }

        private static DialogResult AskForStartDate(FormStartPosition formPos)
        {
            DialogResult dr;
            DlgGetStartDate d2 = new DlgGetStartDate();
            d2.StartPosition = formPos;
            d2.EarthLocation = GCGlobal.lastLocation.EARTHDATA();
            d2.GregorianTime = GCGlobal.dialogStartDate;

            dr = d2.ShowDialog();
            if (dr == DialogResult.Yes)
                GCGlobal.dialogStartDate = d2.GregorianTime;
            return dr;
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


        private void runAppDayDialogSequence(int initialStage)
        {
            DialogResult dr = DialogResult.None;
            FormStartPosition formPos = FormStartPosition.CenterParent;
            int stage = initialStage;

            while (stage != 0)
            {
                switch (stage)
                {
                    case 1:
                        dr = AskForLocation(formPos);
                        break;
                    case 2:
                        dr = AskForDateTime(formPos);
                        break;
                    case 3:
                        stage = 0;
                        m_nInfoType = MainFrameContentType.MW_MODE_APPDAY;
                        RecalculateCurrentScreen();
                        return;
                }
                if (dr == DialogResult.Cancel) stage = 0;
                if (dr == DialogResult.No) stage--;
                if (dr == DialogResult.Yes) stage++;
            }
        }

        private static DialogResult AskForDateTime(FormStartPosition formPos)
        {
            DialogResult dr;
            DlgGetDateTime d2 = new DlgGetDateTime();
            d2.StartPosition = formPos;
            d2.Panel.Timezone = TTimeZone.GetTimeZoneById(GCGlobal.lastLocation.timezoneId);
            d2.Panel.DateTime = GCGlobal.dialogDateTime;

            dr = d2.ShowDialog();
            if (dr == DialogResult.Yes)
                GCGlobal.dialogDateTime = d2.Panel.DateTime;
            return dr;
        }

        private void runMasaListDialogSequence(int initialStage)
        {
            DialogResult dr = DialogResult.None;
            FormStartPosition formPos = FormStartPosition.CenterParent;
            int stage = initialStage;

            while (stage != 0)
            {
                switch (stage)
                {
                    case 1:
                        dr = AskForLocation(formPos);
                        break;
                    case 2:
                        dr = AskForMonthRange(formPos);
                        break;
                    case 3:
                        stage = 0;
                        m_nInfoType = MainFrameContentType.MW_MODE_MASALIST;
                        RecalculateCurrentScreen();
                        return;
                }
                if (dr == DialogResult.Cancel) stage = 0;
                if (dr == DialogResult.No) stage--;
                if (dr == DialogResult.Yes) stage++;
            }
        }

        private static DialogResult AskForMonthRange(FormStartPosition formPos)
        {
            DialogResult dr;
            DlgGetMonthRange d2 = new DlgGetMonthRange();
            d2.StartPosition = formPos;
            d2.StartYear = GCGlobal.dialogStartYear;
            d2.NumberMonths = GCGlobal.dialogNumberMonths;

            dr = d2.ShowDialog();
            if (dr == DialogResult.Yes)
            {
                GCGlobal.dialogStartYear = d2.StartYear;
                GCGlobal.dialogNumberMonths = d2.NumberMonths;
            }
            return dr;
        }

        private void runRatedEventsDialogSequence(int initialStage)
        {
            DialogResult dr = DialogResult.None;
            FormStartPosition formPos = FormStartPosition.CenterParent;
            int stage = initialStage;

            while (stage != 0)
            {
                switch (stage)
                {
                    case 1:
                        DlgRatedEventSelection d0 = new DlgRatedEventSelection();
                        d0.StartPosition = formPos;
                        dr = d0.ShowDialog();
                        break;
                    case 2:
                        dr = AskForLocation(formPos);
                        break;
                    case 3:
                        dr = AskForStartDate(formPos);
                        break;
                    case 4:
                        dr = AskForEndDate(formPos);
                        break;
                    case 5:
                        stage = 0;
                        m_nInfoType = MainFrameContentType.MW_MODE_RATEDEVENTS;
                        m_ratedevents.CompleteCalculation(GCGlobal.lastLocation, GCGlobal.dialogStartDate, GCGlobal.dialogEndDate);
                        RecalculateCurrentScreen();
                        return;
                }
                if (dr == DialogResult.Cancel) stage = 0;
                if (dr == DialogResult.No) stage--;
                if (dr == DialogResult.Yes) stage++;
            }
        }


        private void appearanceDayToolStripMenuItem_Click(object sender, EventArgs e)
        {
            runAppDayDialogSequence(1);
        }

        private void masaListToolStripMenuItem_Click(object sender, EventArgs e)
        {
            runMasaListDialogSequence(1);
        }

        private void eventsFinderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FrameSearch.ShowFrame();
        }

        private void previousDayToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OnCalculatePreviousday();
        }

        private void todayToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OnCalculateToday();
        }

        private void nextDayToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OnCalculateNextday();
        }

        private void newWindowToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FrameMain frame = new FrameMain();

            frame.Icon = Properties.Resources.GCalIcon2;
            frame.Text = "GCAL (secondary window)";
            frame.Show();
        }

        private void closeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void organizerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FrameOrganizer.ShowFrame();
        }

        /// <summary>
        /// Show Help Window
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void helpToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            ShowMode = 1;
            m_textRTF.Rtf = Properties.Resources.gcal_help;
        }

        private void showStartupTipsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            frameDelegate.ShowTipOfTheDay();
        }

        private void copyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (GCUserInterface.ShowMode == 1)
            {
                m_textRTF.Copy();
            }
            else
            {
                m_textTXT.Copy();
            }
        }

        private void selectAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (GCUserInterface.ShowMode == 1)
            {
                m_textRTF.SelectAll();
            }
            else
            {
                m_textTXT.SelectAll();
            }
        }

        private void clearSelectionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (GCUserInterface.ShowMode == 1)
            {
                m_textRTF.SelectionLength = 0;
            }
            else
            {
                m_textTXT.SelectionLength = 0;
            }

        }

        private void normalViewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ShowMode = 0;
            RecalculateCurrentScreen();
        }

        private void enhancedViewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ShowMode = 1;
            RecalculateCurrentScreen();
        }

        private void settingsToolStripMenuItem_DropDownOpening(object sender, EventArgs e)
        {
            normalViewToolStripMenuItem.Checked = (ShowMode == 0);
            enhancedViewToolStripMenuItem.Checked = (ShowMode == 1);
            textSize10ToolStripMenuItem.Checked = (GCLayoutData.LayoutSizeIndex == 0);
            textSize11ToolStripMenuItem.Checked = (GCLayoutData.LayoutSizeIndex == 1);
            textSize12ToolStripMenuItem.Checked = (GCLayoutData.LayoutSizeIndex == 2);
            textSize13ToolStripMenuItem.Checked = (GCLayoutData.LayoutSizeIndex == 3);
            textSize14ToolStripMenuItem.Checked = (GCLayoutData.LayoutSizeIndex == 4);
        }

        public void RecalculateCurrentScreen()
        {
            StringBuilder text = new StringBuilder();

            switch (m_nInfoType)
            {
                case MainFrameContentType.MW_MODE_CAL:
                    {
                        GCUserInterface.CreateProgressWindow();

                        if (GCUserInterface.ShowMode == 0)
                        {
                            m_calendar.formatPlainText(text);
                            m_textTXT.Text = text.ToString();
                            m_strTxt = text.ToString();
                        }
                        else if (GCUserInterface.ShowMode == 1)
                        {
                            m_calendar.formatRtf(text);
                            m_textRTF.Rtf = text.ToString();
                        }

                        GCUserInterface.CloseProgressWindow();
                    }
                    break;
                case MainFrameContentType.MW_MODE_EVENTS:
                    if (GCUserInterface.ShowMode == 0)
                    {
                        m_events.FormatPlainText(text);
                        m_textTXT.Text = text.ToString();
                        m_strTxt = text.ToString();
                    }
                    else if (GCUserInterface.ShowMode == 1)
                    {
                        m_events.formatRtf(text);
                        m_textRTF.Rtf = text.ToString();
                    }
                    break;
                case MainFrameContentType.MW_MODE_MASALIST:
                    if (GCUserInterface.ShowMode == 0)
                    {
                        m_masalist.formatText(text);
                        m_textTXT.Text = text.ToString();
                        m_strTxt = text.ToString();
                    }
                    else if (GCUserInterface.ShowMode == 1)
                    {
                        m_masalist.formatRtf(text);
                        m_textRTF.Rtf = text.ToString();
                    }
                    break;
                case MainFrameContentType.MW_MODE_APPDAY:
                    if (GCUserInterface.ShowMode == 0)
                    {
                        m_appday.formatPlainText(text);
                        m_textTXT.Text = text.ToString();
                        m_strTxt = text.ToString();
                    }
                    else if (GCUserInterface.ShowMode == 1)
                    {
                        m_appday.formatRtf(text);
                        m_textRTF.Rtf = text.ToString();
                    }
                    break;
                case MainFrameContentType.MW_MODE_TODAY:
                    GCGlobal.dateTimeShown.NextDay();
                    OnCalculatePreviousday();
                    break;
                case MainFrameContentType.MW_MODE_RATEDEVENTS:
                    if (GCUserInterface.ShowMode == 0)
                    {
                        m_ratedevents.formatPlainText(text);
                        m_textTXT.Text = text.ToString();
                        m_strTxt = text.ToString();
                    }
                    else if (GCUserInterface.ShowMode == 1)
                    {
                        m_ratedevents.formatRichText(text);
                        m_textRTF.Rtf = text.ToString();
                    }
                    break;
                default:
                    break;
            }
        }

        private void saveContentToolStripMenuItem_Click(object sender, EventArgs e)
        {
            switch(ShowMode)
            {
                case 0:
                    SaveContentPlain();
                    break;
                case 1:
                    SaveContentRich();
                    break;
            }
        }

        private void SaveContentRich()
        {
            SaveContentPlain();
        }

        private void SaveContentPlain()
        {
            string szFilter;
            string szFileName;

            switch (m_nInfoType)
            {
                case 1:
                    szFilter = "Text Files (*.txt)|*.txt|Rich Text Files (*.rtf)|*.rtf|XML Files (*.xml)|*.xml|iCalendar Files (*.ics)|*.ics|vCalendar Files (*.vcs)|*.vcs|Comma Separated Values (*.csv)|*.csv|HTML File (in Table format) (*.htm)|*.htm|HTML File (in List format) (*.htm)|*.htm||";
                    szFileName = "Calendar";
                    break;
                case 2:
                    szFilter = "Text Files (*.txt)|*.txt|Rich Text Files (*.rtf)|*.rtf|XML Files (*.xml)|*.xml|HTML Files (*.html)|*.html||";
                    szFileName = "Day_Details";
                    break;
                case 3:
                    szFilter = "Text Files (*.txt)|*.txt|Rich Text Files (*.rtf)|*.rtf|HTML Files (*.html)|*.html||";
                    szFileName = "Masa_List";
                    break;
                case 4:
                    szFilter = "Text Files (*.txt)|*.txt|Rich Text Files (*.rtf)|*.rtf|XML Files (*.xml)|*.xml||";
                    szFileName = "Sankranti_List";
                    break;
                case 5:
                    szFilter = "Text Files (*.txt)|*.txt|Rich Text Files (*.rtf)|*.rtf|XML Files (*.xml)|*.xml||";
                    szFileName = "Tithi_Naksatra_List";
                    break;
                case 6:
                    szFilter = "Text Files (*.txt)|*.txt|Rich Text Files (*.rtf)|*.rtf|XML Files (*.xml)|*.xml|HTML Files (*.html)|*.html||";
                    szFileName = "AppDay_Info";
                    break;
                default:
                    szFilter = "Text Files (*.txt)|*.txt|Rich Text Files (*.rtf)|*.rtf||";
                    szFileName = "GCal_Result";
                    break;
            }

            SaveFileDialog fd = new SaveFileDialog();
            fd.FileName = szFileName;
            fd.OverwritePrompt = true;
            fd.Filter = szFilter;

            StringBuilder sb = new StringBuilder();

            if (fd.ShowDialog() == DialogResult.OK)
            {
                switch (fd.FilterIndex)
                {
                    case 1:
                        File.WriteAllText(fd.FileName, RetrieveCurrentScreenInText());
                        break;
                    case 2:
                        File.WriteAllText(fd.FileName, RetrieveCurrentScreenInRtf());
                        break;
                    case 3:
                        {
                            switch (m_nInfoType)
                            {
                                case 1:
                                    m_calendar.writeXml(sb);
                                    File.WriteAllText(fd.FileName, sb.ToString());
                                    break;
                                case 3:
                                    m_masalist.writeHtml(sb);
                                    File.WriteAllText(fd.FileName, sb.ToString());
                                    break;
                                case 2:
                                case 4:
                                case 5:
                                case 6:
                                    File.WriteAllText(fd.FileName, m_strXml);
                                    break;
                            }
                        }
                        break;

                    case 4:
                        {
                            switch (m_nInfoType)
                            {
                                case 1:
                                    m_calendar.formatICal(sb);
                                    File.WriteAllText(fd.FileName, sb.ToString());
                                    break;
                                case 2:
                                    m_events.writeHtml(sb);
                                    File.WriteAllText(fd.FileName, sb.ToString());
                                    break;
                                case 6:
                                    m_appday.writeHtml(sb);
                                    File.WriteAllText(fd.FileName, sb.ToString());
                                    break;
                                default:
                                    break;
                            }
                        }
                        break;
                    case 5:
                        m_calendar.formatVCal(sb);
                        File.WriteAllText(fd.FileName, sb.ToString());
                        break;
                    case 6:
                        m_calendar.formatCsv(sb);
                        File.WriteAllText(fd.FileName, sb.ToString());
                        break;
                    case 7:
                        m_calendar.formatHtmlTable(sb);
                        File.WriteAllText(fd.FileName, sb.ToString());
                        break;
                    case 8:
                        m_calendar.formatHtml(sb);
                        File.WriteAllText(fd.FileName, sb.ToString());
                        break;
                    default:
                        break;
                }
            }

        }


        public string RetrieveCurrentScreenInText()
        {
            if (GCUserInterface.ShowMode == 0)
            {
                return m_strTxt;
            }

            StringBuilder text = new StringBuilder();
            switch (m_nInfoType)
            {
                case MainFrameContentType.MW_MODE_CAL:
                    m_calendar.formatPlainText(text);
                    break;
                case MainFrameContentType.MW_MODE_EVENTS:
                    m_events.FormatPlainText(text);
                    break;
                case MainFrameContentType.MW_MODE_MASALIST:
                    m_masalist.formatText(text);
                    break;
                case MainFrameContentType.MW_MODE_APPDAY:
                    m_appday.formatPlainText(text);
                    break;
                case MainFrameContentType.MW_MODE_TODAY:
                    m_today.formatPlain(text);
                    break;
                default:
                    break;
            }
            m_strTxt = text.ToString();
            return m_strTxt;
        }

        public string RetrieveCurrentScreenInRtf()
        {
            StringBuilder text = new StringBuilder();

            switch (m_nInfoType)
            {
                case MainFrameContentType.MW_MODE_CAL:
                    m_calendar.formatRtf(text);
                    break;
                case MainFrameContentType.MW_MODE_EVENTS:
                    m_events.formatRtf(text);
                    break;
                case MainFrameContentType.MW_MODE_MASALIST:
                    m_masalist.formatRtf(text);
                    break;
                case MainFrameContentType.MW_MODE_APPDAY:
                    m_appday.formatRtf(text);
                    break;
                case MainFrameContentType.MW_MODE_TODAY:
                    m_today.formatRtf(text);
                    break;
                default:
                    break;
            }

            return text.ToString();
        }

        private void textSize10ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            GCLayoutData.LayoutSizeIndex = 0;
            RecalculateCurrentScreen();
        }

        private void textSize11ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            GCLayoutData.LayoutSizeIndex = 0;
            RecalculateCurrentScreen();
        }

        private void textSize12ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            GCLayoutData.LayoutSizeIndex = 0;
            RecalculateCurrentScreen();
        }

        private void textSize13ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            GCLayoutData.LayoutSizeIndex = 0;
            RecalculateCurrentScreen();
        }

        private void textSize14ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            GCLayoutData.LayoutSizeIndex = 0;
            RecalculateCurrentScreen();
        }

        private void printToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PrintDialog pd = new PrintDialog();

            if (pd.ShowDialog() != DialogResult.OK)
                return;

            if (ShowMode == 0 || ShowMode == 1)
            {
                stringToPrint = RetrieveCurrentScreenInText().Split('\r', '\n');
                printLineCurr = 0;
                printDocument1.PrinterSettings = pd.PrinterSettings;
                printDocument1.Print();
            }
        }

        private void exitToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void printDocument1_PrintPage(object sender, PrintPageEventArgs ev)
        {
            float linesPerPage = 0;
            float yPos = 0;
            int count = 0;
            float leftMargin = ev.MarginBounds.Left;
            float topMargin = ev.MarginBounds.Top;
            String line = null;

            // Calculate the number of lines per page.
            linesPerPage = ev.MarginBounds.Height /
               printFont.GetHeight(ev.Graphics);

            // Iterate over the file, printing each line.
            while (count < linesPerPage && printLineCurr < stringToPrint.Length)
            {
                line = stringToPrint[printLineCurr];
                printLineCurr++;

                yPos = topMargin + (count * printFont.GetHeight(ev.Graphics));
                ev.Graphics.DrawString(line, printFont, Brushes.Black,
                   leftMargin, yPos, new StringFormat());
                count++;
            }

            // If more lines exist, print another page.
            if (printLineCurr < stringToPrint.Length)
                ev.HasMorePages = true;
            else
                ev.HasMorePages = false;
        }

        private void ratedEventsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            runRatedEventsDialogSequence(1);
        }

        private void newTableCalendarToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FrameMainTable frame = new FrameMainTable();

            frame.Show();
        }

        private void myLocationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DlgSetMyLocation dialog = new DlgSetMyLocation();

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                if (m_nInfoType == MainFrameContentType.MW_MODE_TODAY)
                {
                    RecalculateCurrentScreen();
                }
            }
        }


    }


}
