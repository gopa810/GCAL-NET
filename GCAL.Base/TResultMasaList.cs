using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GCAL.Base
{
    public class TResultMasaList
    {
        public List<TResultMasa> arr = new List<TResultMasa>();
        public GregorianDateTime vc_end;
        public GregorianDateTime vc_start;
        public int n_countYears;
        public int n_countMasa;
        public int n_startYear;
        public CLocationRef m_location;


        public TResultMasaList()
        {
            n_countMasa = 0;
            n_countYears = 0;
            n_startYear = 0;
            arr.Clear();
        }

        public int CalculateMasaList(CLocationRef loc, int nYear, int nCount)
        {
            GCAstroData day = new GCAstroData();
            GregorianDateTime d = new GregorianDateTime(), de = new GregorianDateTime(), t = new GregorianDateTime();
            int lm = -1;
            TResultMasaList mlist = this;
            GCEarthData earth = loc.EARTHDATA();

            mlist.n_startYear = nYear;
            mlist.n_countYears = nCount;
            mlist.vc_start = new GregorianDateTime();
            mlist.vc_end = new GregorianDateTime();
            d.Set(GCAstroData.GetFirstDayOfYear(earth, nYear));
            de.Set(GCAstroData.GetFirstDayOfYear(earth, nYear + nCount));
            mlist.vc_start.Set(d);
            mlist.vc_end.Set(de);
            mlist.m_location.Set(loc);


            int i = 0;
            int prev_paksa = -1;
            int current = 0;


            while (d.IsBeforeThis(de))
            {
                day.DayCalc(d, earth);
                if (prev_paksa != day.nPaksa)
                {
                    day.nMasa = day.MasaCalc(d, earth);

                    if (lm != day.nMasa)
                    {
                        if (lm >= 0)
                        {
                            t.Set(d);
                            t.PreviousDay();
                            if (mlist.arr.Count <= current)
                                mlist.arr.Add(new TResultMasa());
                            mlist.arr[current].vc_end.Set(t);
                            current++;
                        }
                        lm = day.nMasa;
                        if (mlist.arr.Count <= current)
                            mlist.arr.Add(new TResultMasa());
                        mlist.arr[current].masa = day.nMasa;
                        mlist.arr[current].year = day.nGaurabdaYear;
                        mlist.arr[current].vc_start.Set(d);
                    }
                }
                prev_paksa = day.nPaksa;
                d.NextDay();
                i++;
            }

            mlist.arr[current].vc_end.Set(d);
            current++;
            mlist.n_countMasa = current;

            return 1;
        }

        public int formatText(StringBuilder str)
        {
            String stt;
            TResultMasaList mlist = this;
            GCStringBuilder sb = new GCStringBuilder();

            sb.Target = str;
            sb.Format = GCStringBuilder.FormatType.PlainText;

            str.Clear();
            str.AppendFormat(" {0}\r\n\r\n{1}: {2}\r\n", GCStrings.getString(39), GCStrings.getString(40), mlist.m_location.GetFullName());
            str.AppendFormat("{0} {1} {2} {3} {4} {5} {6} {7}\r\n", GCStrings.getString(41), mlist.vc_start.day, GregorianDateTime.GetMonthAbreviation(mlist.vc_start.month), mlist.vc_start.year
                , GCStrings.getString(42), mlist.vc_end.day, GregorianDateTime.GetMonthAbreviation(mlist.vc_end.month), mlist.vc_end.year);
            str.Append("==================================================================\r\n\r\n");

            int i;

            for (i = 0; i < mlist.n_countMasa; i++)
            {
                stt = string.Format("{0} {1}", GCMasa.GetName(mlist.arr[i].masa), mlist.arr[i].year);
                str.Append(stt.PadRight(30, ' '));
                stt = string.Format("{0} {1} {2} - ", mlist.arr[i].vc_start.day, GregorianDateTime.GetMonthAbreviation(mlist.arr[i].vc_start.month), mlist.arr[i].vc_start.year);
                str.Append(stt.PadLeft(16, ' '));
                stt = string.Format("   {0} {1} {2}\r\n", mlist.arr[i].vc_end.day, GregorianDateTime.GetMonthAbreviation(mlist.arr[i].vc_end.month), mlist.arr[i].vc_end.year);
                str.Append(stt.PadLeft(13, ' '));
            }

            sb.AppendNote();
            sb.AppendDocumentTail();

            return 1;
        }

        public int formatRtf(StringBuilder str)
        {
            String stt;
            String stt2;
            TResultMasaList mlist = this;

            GCStringBuilder sb = new GCStringBuilder();
            sb.Target = str;
            sb.Format = GCStringBuilder.FormatType.RichText;
            sb.fontSizeH1 = GCLayoutData.textSizeH1;
            sb.fontSizeH2 = GCLayoutData.textSizeH2;
            sb.fontSizeText = GCLayoutData.textSizeText;
            sb.fontSizeNote = GCLayoutData.textSizeNote;

            sb.AppendDocumentHeader();

            str.Clear();
            str.AppendFormat("{\\fs{0}\\f2 {1}}\\par\\tx{2}\\tx{3}\\f2\\fs{4}\r\n\\par\r\n{5}: {6}\\par\r\n"
                , GCLayoutData.textSizeH1
                , GCStrings.getString(39), 1000 * GCLayoutData.textSizeText / 24, 4000 * GCLayoutData.textSizeText / 24
                , GCLayoutData.textSizeText, GCStrings.getString(40), mlist.m_location.GetFullName());
            str.AppendFormat("{0} {1} {2} {3} {4} {5} {6} {7}\\par\r\n", GCStrings.getString(41), mlist.vc_start.day, GregorianDateTime.GetMonthAbreviation(mlist.vc_start.month), mlist.vc_start.year
                , GCStrings.getString(42), mlist.vc_end.day, GregorianDateTime.GetMonthAbreviation(mlist.vc_end.month), mlist.vc_end.year);
            str.Append("==================================================================\\par\r\n\\par\r\n");

            int i;

            for (i = 0; i < mlist.n_countMasa; i++)
            {
                str.AppendFormat("\\tab {0} {1}\\tab ", GCMasa.GetName(mlist.arr[i].masa), mlist.arr[i].year);
                str.AppendFormat("{0} {1} {2} - ", mlist.arr[i].vc_start.day, GregorianDateTime.GetMonthAbreviation(mlist.arr[i].vc_start.month), mlist.arr[i].vc_start.year);
                str.AppendFormat("{0} {1} {2}\\par\r\n", mlist.arr[i].vc_end.day, GregorianDateTime.GetMonthAbreviation(mlist.arr[i].vc_end.month), mlist.arr[i].vc_end.year);
            }

            sb.AppendNote();
            sb.AppendDocumentTail();

            return 1;
        }

        public int writeHtml(StringBuilder f)
        {
            TResultMasaList mlist = this;

            f.Append("<html>\n<head>\n<title>Masa List</title>\n\n");
            f.Append("<style>\n<!--\nbody {\n  font-family:Verdana;\n  font-size:11pt;\n}\n\ntd.hed {\n  font-size:11pt;\n  font-weight:bold;\n");
            f.Append("  background:#aaaaaa;\n  color:white;\n  text-align:center;\n  vertical-align:center;\n  padding-left:15pt;\n  padding-right:15pt;\n");
            f.Append("  padding-top:5pt;\n  padding-bottom:5pt;\n}\n-->\n</style>\n");
            f.Append("</head>\n");
            f.Append("<body>\n\n");

            f.AppendFormat("<p style=\'text-align:center\'><span style=\'font-size:14pt\'>Masa List</span></br>{0}: {1}</p>\n", GCStrings.getString(40), mlist.m_location.GetFullName());
            f.AppendFormat("<p align=center>{0} {1} {2} {3} {4} {5} {6} {7}</p>\n", GCStrings.getString(41), mlist.vc_start.day, GregorianDateTime.GetMonthAbreviation(mlist.vc_start.month), mlist.vc_start.year
                , GCStrings.getString(42), mlist.vc_end.day, GregorianDateTime.GetMonthAbreviation(mlist.vc_end.month), mlist.vc_end.year);
            f.Append("<hr width=\"50%%\">");

            f.Append("<table align=center>");
            f.Append("<tr><td class=\"hed\" style=\'text-align:left\'>MASA NAME&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;</td><td class=\"hed\">START</td><td class=\"hed\">END</td></tr>");
            int i;
            for (i = 0; i < mlist.n_countMasa; i++)
            {
                f.Append("<tr>");
                f.AppendFormat("<td>{0} {1}&nbsp;&nbsp;&nbsp;&nbsp;</td>", GCMasa.GetName(mlist.arr[i].masa), mlist.arr[i].year);
                f.AppendFormat("<td>{0} {1} {2}</td>", mlist.arr[i].vc_start.day, GregorianDateTime.GetMonthAbreviation(mlist.arr[i].vc_start.month), mlist.arr[i].vc_start.year);
                f.AppendFormat("<td>{0} {1} {2}</td>", mlist.arr[i].vc_end.day, GregorianDateTime.GetMonthAbreviation(mlist.arr[i].vc_end.month), mlist.arr[i].vc_end.year);
                f.Append("</tr>");
            }
            f.Append("</table>");
            f.AppendFormat("<hr width=\"50%%\">\n<p align=center>Generated by {0}</p>", GCStrings.getString(130));
            f.Append("</body></html>");
            return 1;
        }


    }

    public class TResultMasa
	{
		public int masa;
		public int year;
		public GregorianDateTime vc_start;
		public GregorianDateTime vc_end;
	};

}
