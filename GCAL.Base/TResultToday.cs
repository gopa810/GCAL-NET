using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using GCAL.Base.Documents;

namespace GCAL.Base
{
    public class TResultToday
    {
        public GregorianDateTime currentDay;
        public TResultCalendar calendar;

        public TResultToday()
        {
            calendar = new TResultCalendar();
        }

        public void Calculate(GregorianDateTime dateTime, CLocationRef location)
        {
            GregorianDateTime vc2 = new GregorianDateTime();
            currentDay = new GregorianDateTime();
            currentDay.Set(dateTime);
            currentDay.InitWeekDay();
            vc2.Set(currentDay);

            vc2.TimezoneHours = location.offsetUtcHours;
            vc2.PreviousDay();
            vc2.PreviousDay();
            vc2.PreviousDay();
            vc2.PreviousDay();
            calendar = new TResultCalendar();
            calendar.CalculateCalendar(location, vc2, 9);


        }


        public VAISNAVADAY GetCurrentDay()
        {
            int i = calendar.FindDate(currentDay);
            return calendar.GetDay(i);
        }

        public void formatPlain(StringBuilder str)
        {
            string str2;
            VAISNAVADAY p = GetCurrentDay();
            CLocationRef loc = calendar.m_Location;
            GregorianDateTime vc = p.date;
            GCStringBuilder sb = new GCStringBuilder();

            GDDocument doc = new GDDocument();

            doc.Title = "";

            sb.Format = GCStringBuilder.FormatType.PlainText;
            sb.Target = str;

            if (p == null)
                return;

            str.Clear();
            str.AppendFormat("{0} ({1}, {2}, Timezone: {3})\r\n\r\n[{4} {5} {6} - {7}]\r\n  {8}, {9} {10}\r\n  {11} {12}, {13} Gaurabda\r\n\r\n",
                loc.locationName, GCEarthData.GetTextLatitude(loc.latitudeDeg), GCEarthData.GetTextLongitude(loc.longitudeDeg),
                TTimeZone.GetTimeZoneName(loc.timezoneId),
                vc.day, GregorianDateTime.GetMonthAbreviation(vc.month), vc.year, GCStrings.getString(vc.dayOfWeek),
                GCTithi.GetName(p.astrodata.nTithi), GCPaksa.GetName(p.astrodata.nPaksa), GCStrings.getString(20),
                GCMasa.GetName(p.astrodata.nMasa), GCStrings.getString(22), p.astrodata.nGaurabdaYear);


            for (int i = 0; i < p.dayEvents.Count(); i++)
            {
                VAISNAVAEVENT ed = p.dayEvents[i];
                int disp = ed.dispItem;
                if (ed.dispItem != 0 && (disp == -1 || GCDisplaySettings.getValue(disp) != 0))
                {
                    if (ed.spec != null)
                    {
                        sb.AppendTextSeparatorLine(ed.text, 80);
                    }
                    else
                    {
                        sb.AppendTabChar();
                        sb.AppendLine(ed.text);
                    }
                }
            }

            str.AppendLine();


            /*BEGIN GCAL 1.4.3*/
            GCHourTime tdA = new GCHourTime();
            GCHourTime tdB = new GCHourTime();

            if (GCDisplaySettings.getValue(45) != 0)
            {
                tdA.Set(p.astrodata.sun.rise);
                tdB.Set(p.astrodata.sun.rise);
                tdA.AddMinutes(-96);
                tdB.AddMinutes(-48);
                str2 = string.Format("\r\nBrahma Muhurta {0} - {1} ({2})",
                    tdA.ToShortTimeString(), tdB.ToShortTimeString(), GCStrings.GetDSTSignature(p.nDST));
                str.Append(str2);
            }

            if (GCDisplaySettings.getValue(29) != 0)
            {
                str.AppendFormat("\r\n{0} {1} ", GCStrings.Localized("Sunrise"), p.astrodata.sun.rise.ToShortTimeString());
                if (GCDisplaySettings.getValue(32) != 0)
                {
                    tdA.Set(p.astrodata.sun.rise);
                    tdB.Set(p.astrodata.sun.rise);
                    tdA.AddMinutes(-24);
                    tdB.AddMinutes(24);
                    str.AppendFormat(" sandhya {0} - {1} ", tdA.ToShortTimeString(), tdB.ToShortTimeString());
                }
                str.AppendFormat(" ({0})\r\n", GCStrings.GetDSTSignature(p.nDST));
            }
            if (GCDisplaySettings.getValue(30) != 0)
            {
                str.AppendFormat("{0}    {1} ", GCStrings.Localized("Noon"), p.astrodata.sun.noon.ToShortTimeString());
                if (GCDisplaySettings.getValue(32) != 0)
                {
                    tdA.Set(p.astrodata.sun.noon);
                    tdB.Set(p.astrodata.sun.noon);
                    tdA.AddMinutes(-24);
                    tdB.AddMinutes(24);
                    str.AppendFormat(" sandhya {0} - {1} ", tdA.ToShortTimeString(), tdB.ToShortTimeString());
                }
                str.AppendFormat(" ({0})\r\n", GCStrings.GetDSTSignature(p.nDST));
            }
            if (GCDisplaySettings.getValue(31) != 0)
            {
                str.AppendFormat("{0}  {1} ", GCStrings.Localized("Sunset"), p.astrodata.sun.set.ToShortTimeString());
                if (GCDisplaySettings.getValue(32) != 0)
                {
                    tdA.Set(p.astrodata.sun.set);
                    tdB.Set(p.astrodata.sun.set);
                    tdA.AddMinutes(-24);
                    tdB.AddMinutes(+24);
                    str.AppendFormat(" sandhya {0} - {1} ", tdA.ToShortTimeString(), tdB.ToShortTimeString());
                }
                str.AppendFormat(" ({0})\r\n", GCStrings.GetDSTSignature(p.nDST));
            }

            if (GCDisplaySettings.getValue(33) != 0)
            {
                str2 = string.Format("\r\n{0} info\r\n   Moon in {1} {2}",
                    GCStrings.Localized("Sunrise"),
                    GCNaksatra.GetName(p.astrodata.nNaksatra), GCStrings.getString(15));
                str.Append(str2);
                if (GCDisplaySettings.getValue(47) != 0)
                {
                    str2 = string.Format(", {0}%% passed ({1} Pada)", p.astrodata.nNaksatraElapse, GCStrings.getString(811 + GCMath.IntFloor(p.astrodata.nNaksatraElapse / 25)));
                    str.Append(str2);
                }
                if (GCDisplaySettings.getValue(46) != 0)
                {
                    str2 = string.Format(", Moon in {0} {1}", GCRasi.GetName(p.astrodata.nMoonRasi), GCStrings.getString(105));
                    str.Append(str2);
                }
                str.Append(", ");
                str.AppendFormat(GCStrings.Localized("{0} yoga"), GCYoga.GetName(p.astrodata.nYoga));
                sb.AppendLine("   ");
                str.AppendFormat(GCStrings.Localized("Sun in {0} rasi"), GCRasi.GetName(p.astrodata.nSunRasi));
                sb.AppendLine();
            }


            sb.AppendNote();
            sb.AppendDocumentTail();


        }

        public void formatRtf(StringBuilder str)
        {
            VAISNAVADAY p = GetCurrentDay();
            CLocationRef loc = calendar.m_Location;
            GregorianDateTime vc = p.date;

            GDDocument doc = new GDDocument();
            GDDocumentBlock div;
            GDTextRun textRun;

            doc.Title = "Day Inspector";
            doc.MinimumPageWidth = new GDLength(80, GDLengthUnit.Emspace);
            doc.Format.AddStyle(new GDTextStyle(GDStyleKey.FontFamily, "Arial"));
            doc.Format.AddStyle(new GDTextStyle(GDStyleKey.TextSize, new GDLength(GCLayoutData.textSizeText)));
            doc.Format.AddStyle(new GDTextStyle(GDStyleKey.TextColor, new GDStyleColor(0,0,0)));

            doc.Styles.AddStyle(new GDStyleDefinition("hdr1",
                new GDTextStyle(GDStyleKey.TextSize, new GDLength(GCLayoutData.textSizeH1)),
                new GDTextStyle(GDStyleKey.FontFamily, "Arial"),
                new GDTextStyle(GDStyleKey.TextAlign, "left")));
            doc.Styles.AddStyle(new GDStyleDefinition("hdr2",
                new GDTextStyle(GDStyleKey.TextSize, new GDLength(GCLayoutData.textSizeH2)),
                new GDTextStyle(GDStyleKey.FontFamily, "Arial"),
                new GDTextStyle(GDStyleKey.TextAlign, "left")));
            doc.Styles.AddStyle(new GDStyleDefinition("normal",
                new GDTextStyle(GDStyleKey.TextSize, new GDLength(GCLayoutData.textSizeText)),
                new GDTextStyle(GDStyleKey.FontFamily, "Arial"),
                new GDTextStyle(GDStyleKey.TextAlign, "left")));
            doc.Styles.AddStyle(new GDStyleDefinition("note",
                new GDTextStyle(GDStyleKey.TextSize, new GDLength(GCLayoutData.textSizeNote)),
                new GDTextStyle(GDStyleKey.FontFamily, "Arial"),
                new GDTextStyle(GDStyleKey.TextAlign, "left")));

            GCStringBuilder sb = new GCStringBuilder();


            sb.Format = GCStringBuilder.FormatType.RichText;
            sb.fontSizeH1 = GCLayoutData.textSizeH1;
            sb.fontSizeH2 = GCLayoutData.textSizeH2;
            sb.fontSizeText = GCLayoutData.textSizeText;
            sb.fontSizeNote = GCLayoutData.textSizeNote;
            sb.Target = str;

            str.Clear();

            sb.AppendDocumentHeader();

            div = doc.AddBlock();
            textRun = div.Append(GregorianDateTime.GetDateTextWithTodayExt(vc));
            textRun.Format.SetStyle(GDStyleKey.StyleName, "hdr1");

            div = doc.AddBlock();
            textRun = div.Append(GCCalendar.GetWeekdayName(p.date.dayOfWeek));
            textRun.Format.SetStyle(GDStyleKey.StyleName, "hdr2");

            div = doc.AddBlock();
            textRun = div.Append(loc.Format("{locationName} ({latitudeText}, {longitudeText}, Timezone: {timeZoneName})"));
            div.AddLineBreak();
            textRun = div.Append(p.Format("  {tithiName}, {paksaName} Paksa, "));
            textRun = div.Append(p.Format(" {masaName} Masa, {gaurabdaYear} Gaurabda"));


            sb.AppendHeader1(GregorianDateTime.GetDateTextWithTodayExt(vc));
            sb.AppendHeader2(GCCalendar.GetWeekdayName(p.date.dayOfWeek));
            sb.AppendLine();
            sb.AppendLine(loc.Format("{locationName} ({latitudeText}, {longitudeText}, Timezone: {timeZoneName})"));
            sb.AppendLine();
            sb.AppendLine(p.Format("  {tithiName}, {paksaName} Paksa"));
            sb.AppendLine(p.Format("  {masaName} Masa, {gaurabdaYear} Gaurabda"));

            div = doc.AddBlock();
            div.Format.SetStyle(GDStyleKey.LeftMarginSize, new GDLength("3em"));
            div.Format.SetStyle(GDStyleKey.TopMarginSize, new GDLength("1em"));

            for (int i = 0; i < p.dayEvents.Count(); i++)
            {
                VAISNAVAEVENT ed = p.dayEvents[i];
                int disp = ed.dispItem;
                if (ed.dispItem != 0 && (disp == -1 || GCDisplaySettings.getValue(disp) != 0))
                {
                    div.Append(ed.text);
                    div.AddLineBreak();

                    if (ed.spec != null)
                    {
                        sb.AppendTextSeparatorLine(ed.text, 80);
                    }
                    else
                    {
                        sb.AppendTabChar();
                        sb.AppendString(ed.text);
                        sb.AppendLine(); ;
                    }
                }
            }

            GDTable table = doc.AddTable();
            GDTableRow row;
            GDTableCell cell;

            sb.AppendLine(); ;

            /*BEGIN GCAL 1.4.3*/
            GCHourTime tdA = new GCHourTime(), tdB = new GCHourTime();

            if (GCDisplaySettings.getValue(45) != 0)
            {
                tdA.Set(p.astrodata.sun.rise);
                tdB.Set(p.astrodata.sun.rise);
                tdA.AddMinutes(-96);
                tdB.AddMinutes(-48);
                sb.AppendLine();
                str.AppendFormat("{0} {1} - {2} ({3})",
                    GCStrings.Localized("Brahma Muhurta"), tdA.ToShortTimeString(), 
                    tdB.ToShortTimeString(), GCStrings.GetDSTSignature(p.nDST));

                div = doc.AddBlock();
                div.Append(GCStrings.Localized("Brahma Muhurta"));
                div.Append(string.Format(" {0} - {1} ({3})", tdA.ToShortTimeString(), 
                    tdB.ToShortTimeString(), GCStrings.GetDSTSignature(p.nDST)));
            }

            if (GCDisplaySettings.getValue(29) != 0)
            {
                row = table.NewRow();

                sb.AppendLine();
                str.AppendFormat("{0} {1} ", GCStrings.Localized("Sunrise"), p.astrodata.sun.rise.ToShortTimeString());

                cell = row.NewCell();
                cell.Append(GCStrings.Localized("Sunrise"));
                cell = row.NewCell();
                cell.Append(p.astrodata.sun.rise.ToShortTimeString());
                if (GCDisplaySettings.getValue(32) != 0)
                {
                    tdA.Set(p.astrodata.sun.rise);
                    tdB.Set(p.astrodata.sun.rise);
                    tdA.AddMinutes(-24);
                    tdB.AddMinutes(+24);
                    str.AppendFormat(GCStrings.Localized(" sandhya {0} - {1} "), tdA.ToShortTimeString(), tdB.ToShortTimeString());
                    row.NewCell().AppendFormat(GCStrings.Localized(" sandhya {0} - {1} "), tdA.ToShortTimeString(), tdB.ToShortTimeString());
                }
                str.AppendFormat(" ({0})", GCStrings.GetDSTSignature(p.nDST));

                row.NewCell().Append(p.Format(GCStrings.Localized(" ({dstSig})")));
                sb.AppendLine();
            }
            if (GCDisplaySettings.getValue(30) != 0)
            {
                row = table.NewRow();
                cell = row.NewCell();
                cell.Append(GCStrings.Localized("Noon"));
                cell = row.NewCell();
                cell.Append(p.astrodata.sun.noon.ToShortTimeString());

                str.AppendFormat("{0}    {1} ", GCStrings.Localized("Noon"), p.astrodata.sun.noon.ToShortTimeString());
                if (GCDisplaySettings.getValue(32) != 0)
                {
                    tdA.Set(p.astrodata.sun.noon);
                    tdB.Set(p.astrodata.sun.noon);
                    tdA.AddMinutes(-24);
                    tdB.AddMinutes(+24);
                    str.AppendFormat(GCStrings.Localized(" sandhya {0} - {1} "), tdA.ToShortTimeString(), tdB.ToShortTimeString());
                    row.NewCell().AppendFormat(GCStrings.Localized(" sandhya {0} - {1} "), tdA.ToShortTimeString(), tdB.ToShortTimeString());
                }
                str.AppendFormat(" ({0})", GCStrings.GetDSTSignature(p.nDST));
                row.NewCell().Append(p.Format(GCStrings.Localized(" ({dstSig})")));
                sb.AppendLine();
            }
            if (GCDisplaySettings.getValue(31) != 0)
            {
                row = table.NewRow();
                cell = row.NewCell();
                cell.Append(GCStrings.Localized("Sunset"));
                cell = row.NewCell();
                cell.Append(p.astrodata.sun.set.ToShortTimeString());

                str.AppendFormat("{0}  {1} ", GCStrings.Localized("Sunset"), p.astrodata.sun.set.ToShortTimeString());
                if (GCDisplaySettings.getValue(32) != 0)
                {
                    tdA.Set(p.astrodata.sun.set);
                    tdB.Set(p.astrodata.sun.set);
                    tdA.AddMinutes(-24);
                    tdB.AddMinutes(+24);
                    str.AppendFormat(GCStrings.Localized(" sandhya {0} - {1} "), tdA.ToShortTimeString(), tdB.ToShortTimeString());
                    row.NewCell().AppendFormat(GCStrings.Localized(" sandhya {0} - {1} "), tdA.ToShortTimeString(), tdB.ToShortTimeString());
                }
                str.AppendFormat(" ({0})", GCStrings.GetDSTSignature(p.nDST));
                row.NewCell().Append(p.Format(GCStrings.Localized(" ({dstSig})")));
                sb.AppendLine("");
            }
            if (GCDisplaySettings.getValue(33) != 0)
            {
                div = doc.AddBlock();
                div.Format.SetStyle(GDStyleKey.StyleName, "hdr2");
                div.Append(GCStrings.Localized("Sunrise info"));

                div = doc.AddBlock();
                div.Append(p.Format(GCStrings.Localized("Moon in {naksatraName} Naksatra")));

                sb.AppendLine("");
                str.Append(GCStrings.Localized("Sunrise info"));
                sb.AppendLine("");
                str.AppendFormat(GCStrings.Localized("Moon in {0} naksatra"), GCNaksatra.GetName(p.astrodata.nNaksatra));
                if (GCDisplaySettings.getValue(47) != 0)
                {
                    div.Append(p.Format(GCStrings.Localized(", {naksatraElapse} passed ({naksatraPada})")));

                    str.Append(", ");
                    str.AppendFormat(GCStrings.Localized("{0}% passed"), p.astrodata.nNaksatraElapse);
                    str.Append(" ");
                    str.AppendFormat(GCStrings.Localized("({0})"), GCNaksatra.GetPadaText(p.astrodata.NaksatraPada));
                }
                if (GCDisplaySettings.getValue(46) != 0)
                {
                    div.Append(p.Format(GCStrings.Localized(", Moon in {moonRasiName} Rasi")));

                    str.Append(", ");
                    str.AppendFormat(GCStrings.Localized("Moon in {0} rasi"), GCRasi.GetName(p.astrodata.nMoonRasi));
                }

                div.Append(p.Format(GCStrings.Localized(", {yogaName} Yoga")));

                str.Append(p.Format(GCStrings.Localized(", {yogaName} Yoga")));
                sb.AppendLine("");

                div.Append(p.Format(GCStrings.Localized(", Sun in {sunRasiName} Rasi")));

                str.Append("   ");
                str.AppendFormat(GCStrings.Localized("Sun in {0} rasi"), GCRasi.GetName(p.astrodata.nSunRasi));
                sb.AppendLine(",");
            }
            /* END GCAL 1.4.3 */

            div = doc.AddBlock();
            div.Append(GCStrings.Localized("Note"));

            div.AddLineBreak();
            div.AddLineBreak();
            div.Append(GCStrings.Localized("  DST - Time is in \'Daylight Saving Time\'"));
            div.AddLineBreak();
            div.Append(GCStrings.Localized("  LT - Time in in \'Local Time\'\n"));
            div.AddLineBreak();
            div.AddLineBreak();
            div.Append(string.Format(GCStrings.Localized("Generated by {0}"), GCStrings.FullVersionText));

            sb.AppendLine();

            sb.AppendNote();
        }


        /// <summary>
        /// Writes TODAY output text in HTML format into string builder
        /// </summary>
        /// <param name="f"></param>
        public void writeHtml(StringBuilder f)
        {
            VAISNAVADAY p = GetCurrentDay();
            CLocationRef loc = calendar.m_Location;
            GregorianDateTime vc = p.date;

            if (p == null)
                return;

            GDDocument doc = new GDDocument();

            doc.Title = "Today Results";

            f.Append("<html>\n<head>\n<title></title>");
            f.Append("<style>\n<!--\nbody {\n  font-family:Verdana;\n  font-size:9.5pt;\n}\n\ntd.hed {\n  font-size:9.5pt;\n  font-weight:bold;\n");
            f.Append("  background:#aaaaaa;\n  color:white;\n  text-align:center;\n  vertical-align:center;\n  padding-left:15pt;\n  padding-right:15pt;\n");
            f.Append("  padding-top:5pt;\n  padding-bottom:5pt;\n}\n-.\n</style>\n");
            f.Append("</head>\n");
            f.Append("<body>\n");
            f.AppendFormat("<h2>{0}</h2>\n", GregorianDateTime.GetDateTextWithTodayExt(vc));
            f.AppendFormat("<h4>{0}</h4>\n", loc.GetFullName());
            f.AppendFormat("<p>  {0}, {1} {2}<br>  {3} {4}, {5} Gaurabda</p>",
                GCTithi.GetName(p.astrodata.nTithi), GCPaksa.GetName(p.astrodata.nPaksa), GCStrings.getString(20),
                GCMasa.GetName(p.astrodata.nMasa), GCStrings.getString(22), p.astrodata.nGaurabdaYear);

            if (p.dayEvents.Count() > 0)
            {
                f.AppendFormat("<table style=\'border-width:1pt;border-color:black;border-style:solid\'><tr><td style=\'font-size:9pt;background:{0};padding-left:25pt;padding-right:35pt;padding-top:15pt;padding-bottom:15pt;vertical-align:center\'>\n", TResultCalendar.getDayBkgColorCode(p));
                for (int i = 0; i < p.dayEvents.Count(); i++)
                {
                    VAISNAVAEVENT ed = p.dayEvents[i];
                    int disp = ed.dispItem;
                    if (ed.dispItem != 0 && (disp == -1 || GCDisplaySettings.getValue(disp) != 0))
                    {
                        if (ed.spec != null)
                            f.Append("<span style=\'color:#110033\'>");
                        f.AppendFormat("<br>{0}", ed.text);
                        if (ed.spec != null)
                            f.Append("</span>");
                    }
                }
                f.Append("</td></tr></table>\n");
            }


            f.Append("<p>");


            /*BEGIN GCAL 1.4.3*/
            GCHourTime tdA = new GCHourTime(), tdB = new GCHourTime();

            if (GCDisplaySettings.getValue(29) != 0)
            {
                f.AppendFormat("<br>{0}   {1} ", GCStrings.Localized("Sunrise"), p.astrodata.sun.rise.ToShortTimeString());
                if (GCDisplaySettings.getValue(32) != 0)
                {
                    tdA.Set(p.astrodata.sun.rise);
                    tdB.Set(p.astrodata.sun.rise);
                    tdA.AddMinutes(-24);
                    tdB.AddMinutes(+24);
                    f.AppendFormat(GCStrings.Localized(" sandhya {0} - {1} "), tdA.ToShortTimeString(), tdB.ToShortTimeString());
                }
                f.AppendFormat(" ({0})", GCStrings.GetDSTSignature(p.nDST));
            }
            if (GCDisplaySettings.getValue(30) != 0)
            {
                f.AppendFormat("<br>{0}    {1} ", GCStrings.Localized("Noon"), p.astrodata.sun.noon.ToShortTimeString());
                if (GCDisplaySettings.getValue(32) != 0)
                {
                    tdA.Set(p.astrodata.sun.noon);
                    tdB.Set(p.astrodata.sun.noon);
                    tdA.AddMinutes(-24);
                    tdB.AddMinutes(+24);
                    f.AppendFormat(GCStrings.Localized(" sandhya {0} - {1} "), tdA.ToShortTimeString(), tdB.ToShortTimeString());
                }
                f.AppendFormat(" ({0})\r\n", GCStrings.GetDSTSignature(p.nDST));
            }
            if (GCDisplaySettings.getValue(31) != 0)
            {
                f.AppendFormat("<br>{0}  {1} ", GCStrings.Localized("Sunset"), p.astrodata.sun.set.ToShortTimeString());
                if (GCDisplaySettings.getValue(32) != 0)
                {
                    tdA.Set(p.astrodata.sun.set);
                    tdB.Set(p.astrodata.sun.set);
                    tdA.AddMinutes(-24);
                    tdB.AddMinutes(+24);
                    f.AppendFormat(GCStrings.Localized(" sandhya {0} - {1} "), tdA.ToShortTimeString(), tdB.ToShortTimeString());
                }
                f.AppendFormat(" ({0})\r\n", GCStrings.GetDSTSignature(p.nDST));
            }
            if (GCDisplaySettings.getValue(33) != 0)
            {
                f.Append("<br>");
                f.Append(GCStrings.Localized("Sunrise info"));
                f.Append(": ");
                f.AppendFormat(GCStrings.Localized("Moon in {1} naksatra"), GCNaksatra.GetName(p.astrodata.nNaksatra));
                if (GCDisplaySettings.getValue(47) != 0)
                {
                    f.Append(", ");
                    f.AppendFormat(GCStrings.Localized("{0}%% passed"), p.astrodata.nNaksatraElapse, GCStrings.getString(811 + GCMath.IntFloor(p.astrodata.nNaksatraElapse / 25)));
                    f.Append(" ");
                    f.AppendFormat("({0})", p.astrodata.nNaksatraElapse, GCNaksatra.GetPadaText(p.astrodata.NaksatraPada));
                }
                if (GCDisplaySettings.getValue(46) != 0)
                {
                    f.AppendFormat(", ");
                    f.AppendFormat(GCStrings.Localized("Moon in the {0} rasi"), GCRasi.GetName(p.astrodata.nMoonRasi));
                }
                f.Append(", ");
                f.AppendFormat(GCStrings.Localized("{0} yoga"), GCYoga.GetName(p.astrodata.nYoga));
                f.Append("<br>");
                f.AppendFormat(GCStrings.Localized("Sun in {0} rasi"), GCRasi.GetName(p.astrodata.nSunRasi));
            }
            f.Append("</p>");
            f.Append("</body>");
            f.Append("</html>");
            /* END GCAL 1.4.3 */

        }

    }
}
