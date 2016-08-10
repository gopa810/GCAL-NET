using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GCAL.Base
{
    public class TResultEvents
    {
        public const int SORTING_BY_TYPE = 0;
        public const int SORTING_BY_DATE = 1;

        public GregorianDateTime StartDateTime = new GregorianDateTime();
        public GregorianDateTime EndDateTime = new GregorianDateTime();
        public CLocationRef EarthLocation = new CLocationRef();
        public int SortType = SORTING_BY_DATE;

        public void Clear()
        {
            p_events.Clear();
        }

        public List<TDayEvent> p_events = new List<TDayEvent>();

        public bool AddEvent(GregorianDateTime inTime, int inType, int inData, int inDst)
        {
            TDayEvent de = new TDayEvent();

            switch (inDst)
            {
                case 0:
                    de.nDst = 0;
                    break;
                case 1:
                    if (inTime.shour >= 2 / 24.0)
                    {
                        inTime.shour += 1 / 24.0;
                        inTime.NormalizeValues();
                        de.nDst = 1;
                    }
                    else
                    {
                        de.nDst = 0;
                    }
                    break;
                case 2:
                    inTime.shour += 1 / 24.0;
                    inTime.NormalizeValues();
                    de.nDst = 1;
                    break;
                case 3:
                    if (inTime.shour <= 2 / 24.0)
                    {
                        inTime.shour += 1 / 24.0;
                        inTime.NormalizeValues();
                        de.nDst = 1;
                    }
                    else
                    {
                        de.nDst = 0;
                    }
                    break;
                default:
                    de.nDst = 0;
                    break;
            }
            de.Time = new GregorianDateTime();
            de.Time.Set(inTime);
            de.nData = inData;
            de.nType = inType;

            p_events.Add(de);

            return true;
        }

        public void Sort(int nSortType)
        {
            SortType = nSortType;
            TDayEventComparer dec = new TDayEventComparer();
            dec.SortType = nSortType;
            p_events.Sort(dec);
        }

        public TDayEvent this[int i]
        {
            get
            {
                return p_events[i];
            }
        }
        public TResultEvents()
        {
            SortType = SORTING_BY_DATE;
            p_events.Clear();
        }

        public void CalculateEvents(CLocationRef loc, GregorianDateTime vcStart, GregorianDateTime vcEnd)
        {
            GregorianDateTime vc = new GregorianDateTime();
            GCSunData sun = new GCSunData();
            int ndst = 0;
            int nData;

            TResultEvents inEvents = this;
            this.Clear();
            this.EarthLocation.Set(loc);
            this.StartDateTime.Set(vcStart);
            this.EndDateTime.Set(vcEnd);

            GregorianDateTime vcAdd = new GregorianDateTime(), vcNext = new GregorianDateTime();
            GCEarthData earth = loc.EARTHDATA();

            vc.Set(vcStart);

            vcAdd.Set(vc);
            vcAdd.InitWeekDay();

            double sunRise, sunSet;
            double r1, r2;
            double previousSunriseHour = 0, todaySunriseHour;
            double previousLongitude = -100;
            double todayLongitude = 0;
            double fromTimeLimit = 0;

            while (vcAdd.IsBeforeThis(vcEnd))
            {
                if (GCDisplaySettings.getValue(GCDS.COREEVENTS_SUN) != 0)
                {
                    ndst = TTimeZone.determineDaylightChange(vcAdd, loc.timezoneId);
                    sun.SunCalc(vcAdd, earth);

                    vcAdd.shour = sun.arunodaya.GetDayTime();
                    inEvents.AddEvent(vcAdd, CoreEventType.CCTYPE_S_ARUN, 0, ndst);

                    vcAdd.shour = sunRise = sun.rise.GetDayTime();
                    inEvents.AddEvent(vcAdd, CoreEventType.CCTYPE_S_RISE, 0, ndst);

                    vcAdd.shour = sun.noon.GetDayTime();
                    inEvents.AddEvent(vcAdd, CoreEventType.CCTYPE_S_NOON, 0, ndst);

                    vcAdd.shour = sunSet = sun.set.GetDayTime();
                    inEvents.AddEvent(vcAdd, CoreEventType.CCTYPE_S_SET, 0, ndst);
                }
                else
                {
                    ndst = TTimeZone.determineDaylightChange(vcAdd, loc.timezoneId);
                    sun.SunCalc(vcAdd, earth);
                    sunRise = sun.rise.GetDayTime();
                    sunSet = sun.set.GetDayTime();
                }

                if (GCDisplaySettings.getValue(GCDS.COREEVENTS_ASCENDENT) != 0)
                {
                    todayLongitude = sun.longitude_deg;
                    vcAdd.shour = sunRise;
                    todaySunriseHour = sunRise;
                    if (previousLongitude < -10)
                    {
                        GregorianDateTime prevSunrise = new GregorianDateTime();
                        prevSunrise.Set(vcAdd);
                        prevSunrise.PreviousDay();
                        sun.SunCalc(prevSunrise, earth);
                        previousSunriseHour = sun.rise.GetDayTime() - 1;
                        previousLongitude = sun.longitude_deg;
                        fromTimeLimit = 0;
                    }

                    double a, b;
                    double jd = vcAdd.GetJulianComplete();
                    double ayan = GCAyanamsha.GetAyanamsa(jd);
                    r1 = GCMath.putIn360(previousLongitude - ayan) / 30;
                    r2 = GCMath.putIn360(todayLongitude - ayan) / 30;

                    while (r2 > r1 + 13)
                    {
                        r2 -= 12.0;
                    }
                    while (r2 < r1 + 11)
                    {
                        r2 += 12.0;
                    }

                    a = (r2 - r1) / (todaySunriseHour - previousSunriseHour);
                    b = r2 - a * todaySunriseHour;

                    for (double tr = Math.Floor(r1) + 1.0; tr < r2; tr += 1.0)
                    {
                        double tm = (tr - b) / a;
                        if (tm > fromTimeLimit)
                        {
                            vcNext.Set(vcAdd);
                            vcNext.shour = tm;
                            vcNext.NormalizeValues();
                            inEvents.AddEvent(vcNext, CoreEventType.CCTYPE_ASCENDENT, (int)tr, ndst);
                        }
                    }

                    previousLongitude = todayLongitude;
                    previousSunriseHour = todaySunriseHour - 1;
                    fromTimeLimit = previousSunriseHour;
                }

                if (GCDisplaySettings.getValue(GCDS.COREEVENTS_RAHUKALAM) != 0)
                {
                    GCSunData.CalculateKala(sunRise, sunSet, vcAdd.dayOfWeek, out r1, out r2, KalaType.KT_RAHU_KALAM);

                    vcAdd.shour = r1;
                    inEvents.AddEvent(vcAdd, CoreEventType.CCTYPE_KALA_START, KalaType.KT_RAHU_KALAM, ndst);

                    vcAdd.shour = r2;
                    inEvents.AddEvent(vcAdd, CoreEventType.CCTYPE_KALA_END, KalaType.KT_RAHU_KALAM, ndst);
                }

                if (GCDisplaySettings.getValue(GCDS.COREEVENTS_YAMAGHANTI) != 0)
                {
                    GCSunData.CalculateKala(sunRise, sunSet, vcAdd.dayOfWeek, out r1, out r2, KalaType.KT_YAMA_GHANTI);

                    vcAdd.shour = r1;
                    inEvents.AddEvent(vcAdd, CoreEventType.CCTYPE_KALA_START, KalaType.KT_YAMA_GHANTI, ndst);

                    vcAdd.shour = r2;
                    inEvents.AddEvent(vcAdd, CoreEventType.CCTYPE_KALA_END, KalaType.KT_YAMA_GHANTI, ndst);
                }

                if (GCDisplaySettings.getValue(GCDS.COREEVENTS_GULIKALAM) != 0)
                {
                    GCSunData.CalculateKala(sunRise, sunSet, vcAdd.dayOfWeek, out r1, out r2, KalaType.KT_GULI_KALAM);

                    vcAdd.shour = r1;
                    inEvents.AddEvent(vcAdd, CoreEventType.CCTYPE_KALA_START, KalaType.KT_GULI_KALAM, ndst);

                    vcAdd.shour = r2;
                    inEvents.AddEvent(vcAdd, CoreEventType.CCTYPE_KALA_END, KalaType.KT_GULI_KALAM, ndst);
                }

                if (GCDisplaySettings.getValue(GCDS.COREEVENTS_ABHIJIT_MUHURTA) != 0)
                {
                    GCSunData.CalculateKala(sunRise, sunSet, vcAdd.dayOfWeek, out r1, out r2, KalaType.KT_ABHIJIT);

                    if (r1 > 0 && r2 > 0)
                    {
                        vcAdd.shour = r1;
                        inEvents.AddEvent(vcAdd, CoreEventType.CCTYPE_KALA_START, KalaType.KT_ABHIJIT, ndst);

                        vcAdd.shour = r2;
                        inEvents.AddEvent(vcAdd, CoreEventType.CCTYPE_KALA_END, KalaType.KT_ABHIJIT, ndst);
                    }
                }

                vcAdd.NextDay();
            }

            if (GCDisplaySettings.getValue(GCDS.COREEVENTS_TITHI) != 0)
            {
                vcAdd.Set(vc);
                vcAdd.shour = 0.0;
                while (vcAdd.IsBeforeThis(vcEnd))
                {
                    nData = GCTithi.GetNextTithiStart(earth, vcAdd, out vcNext);
                    if (vcNext.GetDayInteger() < vcEnd.GetDayInteger())
                    {
                        vcNext.InitWeekDay();
                        ndst = TTimeZone.determineDaylightChange(vcNext, loc.timezoneId);
                        inEvents.AddEvent(vcNext, CoreEventType.CCTYPE_TITHI, nData, ndst);
                    }
                    else
                    {
                        break;
                    }
                    vcAdd.Set(vcNext);
                    vcAdd.shour += 0.2;
                    if (vcAdd.shour >= 1.0)
                    {
                        vcAdd.shour -= 1.0;
                        vcAdd.NextDay();
                    }
                }
            }

            if (GCDisplaySettings.getValue(GCDS.COREEVENTS_NAKSATRA) != 0)
            {
                vcAdd.Set(vc);
                vcAdd.shour = 0.0;
                while (vcAdd.IsBeforeThis(vcEnd))
                {
                    nData = GCNaksatra.GetNextNaksatra(earth, vcAdd, out vcNext);
                    if (vcNext.GetDayInteger() < vcEnd.GetDayInteger())
                    {
                        vcNext.InitWeekDay();
                        ndst = TTimeZone.determineDaylightChange(vcNext, loc.timezoneId);
                        inEvents.AddEvent(vcNext, CoreEventType.CCTYPE_NAKS, nData, ndst);
                    }
                    else
                    {
                        break;
                    }
                    vcAdd.Set(vcNext);
                    vcAdd.shour += 0.2;
                    if (vcAdd.shour >= 1.0)
                    {
                        vcAdd.shour -= 1.0;
                        vcAdd.NextDay();
                    }
                }
            }

            if (GCDisplaySettings.getValue(GCDS.COREEVENTS_YOGA) != 0)
            {
                vcAdd.Set(vc);
                vcAdd.shour = 0.0;
                while (vcAdd.IsBeforeThis(vcEnd))
                {
                    nData = GCYoga.GetNextYogaStart(earth, vcAdd, out vcNext);
                    if (vcNext.GetDayInteger() < vcEnd.GetDayInteger())
                    {
                        vcNext.InitWeekDay();
                        ndst = TTimeZone.determineDaylightChange(vcNext, loc.timezoneId);
                        inEvents.AddEvent(vcNext, CoreEventType.CCTYPE_YOGA, nData, ndst);
                    }
                    else
                    {
                        break;
                    }
                    vcAdd.Set(vcNext);
                    vcAdd.shour += 0.2;
                    if (vcAdd.shour >= 1.0)
                    {
                        vcAdd.shour -= 1.0;
                        vcAdd.NextDay();
                    }
                }
            }

            if (GCDisplaySettings.getValue(GCDS.COREEVENTS_SANKRANTI) != 0)
            {
                vcNext = new GregorianDateTime();
                vcAdd.Set(vc);
                vcAdd.shour = 0.0;
                while (vcAdd.IsBeforeThis(vcEnd))
                {
                    vcNext.Set(GCSankranti.GetNextSankranti(vcAdd, out nData));
                    if (vcNext.GetDayInteger() < vcEnd.GetDayInteger())
                    {
                        vcNext.InitWeekDay();
                        ndst = TTimeZone.determineDaylightChange(vcNext, loc.timezoneId);
                        inEvents.AddEvent(vcNext, CoreEventType.CCTYPE_SANK, nData, ndst);
                    }
                    else
                    {
                        break;
                    }
                    vcAdd.Set(vcNext);
                    vcAdd.NextDay();
                }
            }

            if (GCDisplaySettings.getValue(GCDS.COREEVENTS_MOONRASI) != 0)
            {
                vcAdd.Set(vc);
                vcAdd.shour = 0.0;
                while (vcAdd.IsBeforeThis(vcEnd))
                {
                    nData = GCMoonData.GetNextMoonRasi(earth, vcAdd, out vcNext);
                    if (vcNext.GetDayInteger() < vcEnd.GetDayInteger())
                    {
                        vcNext.InitWeekDay();
                        ndst = TTimeZone.determineDaylightChange(vcNext, loc.timezoneId);
                        inEvents.AddEvent(vcNext, CoreEventType.CCTYPE_M_RASI, nData, ndst);
                    }
                    else
                    {
                        break;
                    }
                    vcAdd.Set(vcNext);
                    vcAdd.shour += 0.5;
                    vcAdd.NormalizeValues();
                }

            }
            if (GCDisplaySettings.getValue(GCDS.COREEVENTS_CONJUNCTION) != 0)
            {
                double dlong;
                vcAdd.Set(vc);
                vcAdd.shour = 0.0;
                while (vcAdd.IsBeforeThis(vcEnd))
                {
                    dlong = GCConjunction.GetNextConjunction(vcAdd, out vcNext, true, earth);
                    if (vcNext.GetDayInteger() < vcEnd.GetDayInteger())
                    {
                        vcNext.InitWeekDay();
                        ndst = TTimeZone.determineDaylightChange(vcNext, loc.timezoneId);
                        inEvents.AddEvent(vcNext, CoreEventType.CCTYPE_CONJ, GCRasi.GetRasi(dlong, GCAyanamsha.GetAyanamsa(vcNext.GetJulianComplete())), ndst);
                    }
                    else
                    {
                        break;
                    }
                    vcAdd.Set(vcNext);
                    vcAdd.NextDay();
                }
            }

            if (GCDisplaySettings.getValue(GCDS.COREEVENTS_MOON) != 0)
            {
                vcAdd.Set(vc);
                vcAdd.shour = 0.0;
                while (vcAdd.IsBeforeThis(vcEnd))
                {
                    vcNext.Set(GCMoonData.GetNextRise(earth, vcAdd, true));
                    inEvents.AddEvent(vcNext, CoreEventType.CCTYPE_M_RISE, 0, ndst);

                    vcNext.Set(GCMoonData.GetNextRise(earth, vcNext, false));
                    inEvents.AddEvent(vcNext, CoreEventType.CCTYPE_M_SET, 0, ndst);

                    vcNext.shour += 0.05;
                    vcNext.NormalizeValues();
                    vcAdd.Set(vcNext);
                }
            }

            if (GCDisplaySettings.getValue(GCDS.COREEVENTS_ASCENDENT) != 0)
            {/*
		        vcAdd = vc;
		        vcAdd.shour = 0.0;
		        while(vcAdd.IsBeforeThis(vcEnd))
		        {
			        nData = earth.GetNextAscendentStart(vcAdd, vcNext);
			        if (vcNext.GetDayInteger() < vcEnd.GetDayInteger())
			        {
				        vcNext.InitWeekDay();
				        ndst = TTimeZone.determineDaylightChange(vcNext, loc.m_nDST);
				        inEvents.AddEvent(vcNext, CCTYPE_ASCENDENT, nData, ndst);
			        }
			        else
			        {
				        break;
			        }
			        vcAdd = vcNext;
			        vcAdd.shour += 1/24.0;
			        if (vcAdd.shour >= 1.0)
			        {
				        vcAdd.shour -= 1.0;
				        vcAdd.NextDay();
			        }
		        }

		        */
            }

            if (GCDisplaySettings.getValue(GCDS.COREEVENTS_SORT) != 0)
                inEvents.Sort(SORTING_BY_DATE);
            else
                inEvents.Sort(SORTING_BY_TYPE);
        }

        public int FormatPlainText(StringBuilder res)
        {
            int i;

            GCStringBuilder sb = new GCStringBuilder();

            sb.Target = res;
            sb.Format = GCStringBuilder.FormatType.PlainText;

            TResultEvents inEvents = this;

            res.Clear();
            res.AppendFormat("Events from {0} {1} {2} to {3} {4} {5}.\r\n\r\n",
                inEvents.StartDateTime.day,
                GregorianDateTime.GetMonthAbreviation(inEvents.StartDateTime.month),
                inEvents.StartDateTime.year,
                inEvents.EndDateTime.day,
                GregorianDateTime.GetMonthAbreviation(inEvents.EndDateTime.month),
                inEvents.EndDateTime.year);

            res.Append(inEvents.EarthLocation.GetFullName());
            res.Append("\r\n\r\n");

            GregorianDateTime prevd = new GregorianDateTime();
            int prevt = -1;

            prevd.day = 0;
            prevd.month = 0;
            prevd.year = 0;
            for (i = 0; i < inEvents.p_events.Count; i++)
            {
                TDayEvent dnr = inEvents[i];

                if (inEvents.SortType == TResultEvents.SORTING_BY_DATE)
                {
                    if (prevd.day != dnr.Time.day || prevd.month != dnr.Time.month || prevd.year != dnr.Time.year)
                    {
                        res.AppendFormat("\r\n ===========  {0} - {1} ====================================\r\n\r\n",
                            dnr.Time.ToString(), GCCalendar.GetWeekdayName(dnr.Time.dayOfWeek));
                    }
                    prevd.Set(dnr.Time);
                }
                else
                {
                    if (prevt != dnr.nType)
                    {
                        res.AppendFormat("\r\n ========== {0} ==========================================\r\n\r\n",
                            dnr.GroupNameString);
                        prevt = dnr.nType;
                    }
                }

                res.AppendFormat("            {0} {1}   {2}\r\n", dnr.Time.LongTimeString(), 
                    GCStrings.GetDSTSignature(dnr.nDst), dnr.TypeString);
            }

            res.Append("\r\n");

            sb.AppendNote();

            return 1;

        }

        public string FormatXml(StringBuilder strXml)
        {
            TResultEvents inEvents = this;
            int i;

            if (strXml == null)
                strXml = new StringBuilder();

            strXml.Clear();
            strXml.AppendFormat("<xml>\r\n<program version=\"{0}\">\r\n<location longitude=\"{1}\" latitude=\"{2}\" timezone=\"{3}\" dst=\"{4}\" />\n"
                , GCStrings.getString(130), inEvents.EarthLocation.longitudeDeg, inEvents.EarthLocation.latitudeDeg
                , inEvents.EarthLocation.offsetUtcHours, TTimeZone.GetTimeZoneName(inEvents.EarthLocation.timezoneId));
            GregorianDateTime prevd = new GregorianDateTime();

            prevd.day = 0;
            prevd.month = 0;
            prevd.year = 0;
            for (i = 0; i < inEvents.p_events.Count; i++)
            {
                TDayEvent dnr = inEvents[i];

                if (inEvents.SortType == TResultEvents.SORTING_BY_DATE)
                {
                    if (prevd.day != dnr.Time.day || prevd.month != dnr.Time.month || prevd.year != dnr.Time.year)
                    {
                        strXml.AppendFormat("\t<day date=\"{0}/{1}/{2}\" />\n", dnr.Time.day, dnr.Time.month, dnr.Time.year);
                    }
                    prevd.Set(dnr.Time);
                }

                strXml.AppendFormat("\t<event type=\"{0}\" time=\"{1}\" dst=\"{2}\" />\n", dnr.TypeString,
                    dnr.Time.LongTimeString(), dnr.nDst);

            }

            strXml.Append("</xml>\n");

            return strXml.ToString();

        }

        public int formatRtf(StringBuilder res)
        {
            int i;
            TResultEvents inEvents = this;

            GCStringBuilder sb = new GCStringBuilder();

            sb.Target = res;
            sb.Format = GCStringBuilder.FormatType.RichText;
            sb.fontSizeH1 = GCLayoutData.textSizeH1;
            sb.fontSizeH2 = GCLayoutData.textSizeH2;
            sb.fontSizeText = GCLayoutData.textSizeText;
            sb.fontSizeNote = GCLayoutData.textSizeNote;

            res.Clear();

            sb.AppendDocumentHeader();

            sb.AppendHeader1("Events");

            sb.AppendLine();
            sb.AppendLine(" From {0} to {1}.", inEvents.StartDateTime.ToString(), inEvents.EndDateTime.ToString());
            sb.AppendLine(inEvents.EarthLocation.GetFullName());
            sb.AppendLine();


            GregorianDateTime prevd = new GregorianDateTime();
            int prevt = -1;

            prevd.day = 0;
            prevd.month = 0;
            prevd.year = 0;
            for (i = 0; i < inEvents.p_events.Count; i++)
            {
                TDayEvent dnr = inEvents[i];

                if (inEvents.SortType == TResultEvents.SORTING_BY_DATE)
                {
                    if (prevd.day != dnr.Time.day || prevd.month != dnr.Time.month || prevd.year != dnr.Time.year)
                    {
                        sb.AppendLine();
                        sb.AppendFormat(" ===========  {0} {1} {2}  - {3} =================================== ", dnr.Time.day, GregorianDateTime.GetMonthAbreviation(dnr.Time.month), dnr.Time.year,
                            GCCalendar.GetWeekdayName(dnr.Time.dayOfWeek));
                        sb.AppendLine();
                        sb.AppendLine();
                    }
                    prevd.Set(dnr.Time);
                }
                else
                {
                    if (prevt != dnr.nType)
                    {
                        sb.AppendLine();
                        sb.AppendFormat(" ========== {0} ========================================== ", dnr.GroupNameString);
                        sb.AppendLine();
                        sb.AppendLine();
                        prevt = dnr.nType;
                    }
                }

                sb.AppendFormat("            {0} {1}   {2}",
                    dnr.Time.LongTimeString(), GCStrings.GetDSTSignature(dnr.nDst), dnr.TypeString);
                sb.AppendLine();
            }

            sb.AppendLine();
            sb.AppendNote();
            sb.AppendDocumentTail();


            return 1;

        }

        public int writeHtml(StringBuilder xml)
        {
            TResultEvents inEvents = this;
            int i;

            xml.Clear();
            xml.Append("<html>\n<head>\n<title>Core Events</title>\n\n");
            xml.Append("<style>\n<!--\nbody {\n  font-family:Verdana;\n  font-size:11pt;\n}\n\ntd.hed {\n  font-size:11pt;\n  font-weight:bold;\n");
            xml.Append("  background:#aaaaaa;\n  color:white;\n  text-align:center;\n  vertical-align:center;\n  padding-left:15pt;\n  padding-right:15pt;\n");
            xml.Append("  padding-top:5pt;\n  padding-bottom:5pt;\n}\n-->\n</style>\n");
            xml.Append("</head>\n");
            xml.Append("<body>\n\n");
            xml.AppendFormat("<h1 align=center>Events</h1>\n<p align=center>From {0} {1} {2} to {3} {4} {5}.</p>\n\n",
                inEvents.StartDateTime.day,
                GregorianDateTime.GetMonthAbreviation(inEvents.StartDateTime.month),
                inEvents.StartDateTime.year,
                inEvents.EndDateTime.day,
                GregorianDateTime.GetMonthAbreviation(inEvents.EndDateTime.month),
                inEvents.EndDateTime.year);

            xml.AppendFormat("<p align=center>{0}</p>\n", inEvents.EarthLocation.GetFullName());

            GregorianDateTime prevd = new GregorianDateTime();
            int prevt = -1;

            prevd.day = 0;
            prevd.month = 0;
            prevd.year = 0;

            xml.Append("<table align=center><tr>\n");
            for (i = 0; i < inEvents.p_events.Count; i++)
            {
                TDayEvent dnr = inEvents[i];

                if (inEvents.SortType == TResultEvents.SORTING_BY_TYPE)
                {
                    if (prevd.day != dnr.Time.day || prevd.month != dnr.Time.month || prevd.year != dnr.Time.year)
                    {
                        xml.AppendFormat("<td class=\"hed\" colspan=2>{0} </td></tr>\n<tr>",
                            dnr.Time.ToString());
                    }
                    prevd.Set(dnr.Time);
                }
                else
                {
                    if (prevt != dnr.nType)
                    {
                        xml.AppendFormat("<td class=\"hed\" colspan=3>{0}</td></tr>\n<tr>\n", dnr.GroupNameString);
                        prevt = dnr.nType;
                    }
                }

                if (inEvents.SortType == TResultEvents.SORTING_BY_DATE)
                {
                    xml.AppendFormat("<td>{0} {1} {2} </td>", dnr.Time.day, GregorianDateTime.GetMonthAbreviation(dnr.Time.month), dnr.Time.year);
                }

                xml.AppendFormat("<td>{0}</td><td>{1}</td></tr><tr>\n", dnr.TypeString, dnr.Time.LongTimeString());
            }

            xml.Append("</tr></table>\n");
            xml.AppendFormat("<hr align=center width=\"50%%\">\n<p align=center>Generated by {0}</p>", GCStrings.getString(130));
            xml.Append("</body>\n</html>\n");

            return 1;

        }


    }

    public sealed class CoreEventType
    {
        public const int CCTYPE_DATE = 1;
        public const int CCTYPE_S_ARUN = 10;
        public const int CCTYPE_S_RISE = 11;
        public const int CCTYPE_S_NOON = 12;
        public const int CCTYPE_S_SET = 13;
        public const int CCTYPE_S_MIDNIGHT = 14;

        public const int CCTYPE_TITHI = 20;
        public const int CCTYPE_NAKS = 21;
        public const int CCTYPE_SANK = 22;
        public const int CCTYPE_CONJ = 23;
        public const int CCTYPE_YOGA = 24;
        public const int CCTYPE_KALA_START = 30;
        public const int CCTYPE_KALA_END = 31;
        public const int CCTYPE_M_RISE = 41;
        public const int CCTYPE_M_SET = 42;
        public const int CCTYPE_M_RASI = 45;
        public const int CCTYPE_ASCENDENT = 50;

        public const int CCTYPE_TITHI_BASE = 60;
        public const int CCTYPE_DAY_MUHURTA = 61;
        public const int CCTYPE_DAY_OF_WEEK = 62;

        public const int CCTYPE_NAKS_PADA1 = 65;
        public const int CCTYPE_NAKS_PADA2 = 66;
        public const int CCTYPE_NAKS_PADA3 = 67;
        public const int CCTYPE_NAKS_PADA4 = 68;

    };

    public class TDayEvent
    {
        public int nType;
        public int nData;
        public GregorianDateTime Time;
        public int nDst;
        void Set(TDayEvent de)
        {
            nType = de.nType;
            nData = de.nData;
            Time.Set(de.Time);
            nDst = de.nDst;
        }

        public static string GetTypeString(int nType)
        {
            switch (nType)
            {
                case CoreEventType.CCTYPE_ASCENDENT:
                    return "Ascendent";
                case CoreEventType.CCTYPE_CONJ:
                    return "Conjunction";
                case CoreEventType.CCTYPE_DATE:
                    return "Date";
                case CoreEventType.CCTYPE_DAY_MUHURTA:
                    return "Muhurta";
                case CoreEventType.CCTYPE_DAY_OF_WEEK:
                    return "Day of Week";
                case CoreEventType.CCTYPE_KALA_START:
                    return "Special interval start";
                case CoreEventType.CCTYPE_KALA_END:
                    return "Special interval end";
                case CoreEventType.CCTYPE_M_RASI:
                    return "Moon rasi";
                case CoreEventType.CCTYPE_M_RISE:
                    return "Moon rise";
                case CoreEventType.CCTYPE_M_SET:
                    return "Moon set";
                case CoreEventType.CCTYPE_NAKS:
                    return "Naksatra";
                case CoreEventType.CCTYPE_S_ARUN:
                    return "Arunodaya";
                case CoreEventType.CCTYPE_S_MIDNIGHT:
                    return "Midnight";
                case CoreEventType.CCTYPE_S_NOON:
                    return "Noon";
                case CoreEventType.CCTYPE_S_RISE:
                    return "Sunrise";
                case CoreEventType.CCTYPE_S_SET:
                    return "Sunset";
                case CoreEventType.CCTYPE_SANK:
                    return "Sankranti";
                case CoreEventType.CCTYPE_TITHI:
                    return "Tithi";
                case CoreEventType.CCTYPE_YOGA:
                    return "Yoga";
                default:
                    return "Unspecified event";
            }

        }

        public static string GetTypeString(int nType, int nData)
        {
            switch (nType)
            {
                case CoreEventType.CCTYPE_ASCENDENT:
                    return "Ascendent " + GCRasi.GetName(nData);
                case CoreEventType.CCTYPE_CONJ:
                    return "Conjunction in " + GCRasi.GetName(nData);
                case CoreEventType.CCTYPE_DATE:
                    return "Date";
                case CoreEventType.CCTYPE_DAY_MUHURTA:
                    return string.Format("{0} Muhurta", GCStrings.GetMuhurtaName(nData));
                case CoreEventType.CCTYPE_DAY_OF_WEEK:
                    return GCCalendar.GetWeekdayName(nData);
                case CoreEventType.CCTYPE_KALA_END:
                    return string.Format("{0} ends", GCStrings.GetKalaName(nData));
                case CoreEventType.CCTYPE_KALA_START:
                    return string.Format("{0} starts", GCStrings.GetKalaName(nData));
                case CoreEventType.CCTYPE_M_RASI:
                    return string.Format("Moon in {0} rasi", GCRasi.GetName(nData));
                case CoreEventType.CCTYPE_M_RISE:
                    return "Moon rise";
                case CoreEventType.CCTYPE_M_SET:
                    return "Moon set";
                case CoreEventType.CCTYPE_NAKS:
                    return string.Format("{0} Naksatra", GCNaksatra.GetName(nData));
                case CoreEventType.CCTYPE_S_ARUN:
                    return "Arunodaya";
                case CoreEventType.CCTYPE_S_MIDNIGHT:
                    return "Midnight";
                case CoreEventType.CCTYPE_S_NOON:
                    return "Noon";
                case CoreEventType.CCTYPE_S_RISE:
                    return "Sunrise";
                case CoreEventType.CCTYPE_S_SET:
                    return "Sunset";
                case CoreEventType.CCTYPE_SANK:
                    return string.Format("{0} Sankranti", GCRasi.GetName(nData));
                case CoreEventType.CCTYPE_TITHI:
                    return string.Format("{0} Tithi", GCTithi.GetName(nData));
                case CoreEventType.CCTYPE_YOGA:
                    return string.Format("{0} Yoga", GCYoga.GetName(nData));
                default:
                    return string.Format("Unspecified event {0} / {1}", nType, nData);
            }

        }

        public string GroupNameString
        {
            get
            {
                return GetTypeString(nType);
            }
        }

        public string TypeString
        {
            get
            {
                return GetTypeString(nType, nData);
            }
        }
    }

    public class TDayEventComparer : Comparer<TDayEvent>
    {
        public int SortType { get; set; }

        public override int Compare(TDayEvent x, TDayEvent y)
        {
            if (SortType == TResultEvents.SORTING_BY_TYPE)
            {
                if (x.nType != y.nType)
                    return x.nType - y.nType;
            }

            double d = x.Time.GetJulianComplete() - y.Time.GetJulianComplete();
            if (d < 0)
                return -1;
            if (d > 0)
                return +1;
            return 0;
        }
    }

}
