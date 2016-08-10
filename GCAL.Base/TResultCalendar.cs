using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GCAL.Base
{
    public class TResultCalendar
    {
        public static readonly int BEFORE_DAYS = 8;
        public static readonly int CDB_MAXDAYS = 16;
        public VAISNAVADAY[] m_pData;
        public int m_nCount;
        public int m_PureCount;
        public CLocationRef m_Location;
        public GregorianDateTime m_vcStart;
        public int m_vcCount;
        public bool updateCalculationProgress;
        public int nBeg;
        public int nTop;

        public TResultCalendar()
        {
            nTop = 0;
            nBeg = 0;
            m_pData = null;
            m_PureCount = 0;
            m_nCount = 0;
            updateCalculationProgress = true;
        }

        int DAYS_TO_ENDWEEK(int lastMonthDay)
        {
            return (21 - (lastMonthDay - GCDisplaySettings.getValue(GCDS.GENERAL_FIRST_DOW))) % 7;
        }

        int DAYS_FROM_BEGINWEEK(int firstMonthDay)
        {
            return (firstMonthDay - GCDisplaySettings.getValue(GCDS.GENERAL_FIRST_DOW) + 14) % 7;
        }

        int DAY_INDEX(int day)
        {
            return (day + GCDisplaySettings.getValue(GCDS.GENERAL_FIRST_DOW)) % 7;
        }

        public static string formatPlainTextDay(VAISNAVADAY pvd)
        {
            String str;

            GCStringBuilder sb = new GCStringBuilder();
            sb.Format = GCStringBuilder.FormatType.PlainText;
            sb.Target = new StringBuilder();

            if (pvd.astrodata.sun.longitude_deg < 0.0)
            {
                sb.AppendTwoColumnText(pvd.GetDateText(), "No rise and no set of the sun. No calendar information.");
                return sb.Target.ToString();
            }
            str = pvd.GetTextA(GCDisplaySettings.getValue(39), GCDisplaySettings.getValue(36), GCDisplaySettings.getValue(37), GCDisplaySettings.getValue(38), GCDisplaySettings.getValue(41));
            sb.AppendTwoColumnText(pvd.GetDateText(), str);


            for (int i = 0; i < pvd.dayEvents.Count(); i++)
            {
                VAISNAVAEVENT ed = pvd.dayEvents[i];
                int disp = ed.dispItem;
                if (disp != 0 && (disp == -1 || GCDisplaySettings.getValue(disp) == 1))
                {
                    if (ed.spec != null)
                    {
                        str = ed.text;
                        int length = str.Length;
                        length = (82 - length) / 2;
                        sb.AppendSeparatorWithWidth(length);
                        sb.AppendString(str);
                        sb.AppendSeparatorWithWidth(length);
                        sb.AppendLine();
                    }
                    else
                    {
                        sb.AppendTwoColumnText("", ed.text);
                    }
                }
            }

            return sb.Target.ToString();
        }

        public static string getDayBkgColorCode(VAISNAVADAY p)
        {
            if (p == null)
                return "white";
            if (p.nFastType == FastType.FAST_EKADASI)
                return "#FFFFBB";
            if (p.nFastType != 0)
                return "#BBFFBB";
            return "white";
        }

        public bool NextNewFullIsVriddhi(int nIndex, GCEarthData earth)
        {
            int i = 0;
            int nTithi;
            int nPrevTithi = 100;

            for (i = 0; i < BEFORE_DAYS; i++)
            {
                nTithi = m_pData[nIndex].astrodata.nTithi;
                if ((nTithi == nPrevTithi) && GCTithi.TITHI_FULLNEW_MOON(nTithi))
                {
                    return true;
                }
                nPrevTithi = nTithi;
                nIndex++;
            }

            return false;
        }

        // test for MAHADVADASI 5 TO 8
        public bool IsMhd58(int nIndex, out int nMahaType)
        {
            VAISNAVADAY t = m_pData[nIndex];
            VAISNAVADAY u = m_pData[nIndex + 1];

            nMahaType = MahadvadasiType.EV_NULL;

            if (t.astrodata.nNaksatra != u.astrodata.nNaksatra)
                return false;

            if (t.astrodata.nPaksa != 1)
                return false;

            if (t.astrodata.nTithi == t.astrodata.nTithiSunset)
            {
                if (t.astrodata.nNaksatra == 6) // punarvasu
                {
                    nMahaType = MahadvadasiType.EV_JAYA;
                    return true;
                }
                else if (t.astrodata.nNaksatra == 3) // rohini
                {
                    nMahaType = MahadvadasiType.EV_JAYANTI;
                    return true;
                }
                else if (t.astrodata.nNaksatra == 7) // pusyami
                {
                    nMahaType = MahadvadasiType.EV_PAPA_NASINI;
                    return true;
                }
                else if (t.astrodata.nNaksatra == 21) // sravana
                {
                    nMahaType = MahadvadasiType.EV_VIJAYA;
                    return true;
                }
                else
                    return false;
            }
            else
            {
                if (t.astrodata.nNaksatra == 21) // sravana
                {
                    nMahaType = MahadvadasiType.EV_VIJAYA;
                    return true;
                }
            }

            return false;
        }

        /******************************************************************************************/
        /* Main fucntion for VCAL calculations                                                    */
        /*                                                                                        */
        /*                                                                                        */
        /******************************************************************************************/


        public int CalculateCalendar(CLocationRef loc, GregorianDateTime begDate, int iCount)
        {
            int i, weekday;
            int nTotalCount = BEFORE_DAYS + iCount + BEFORE_DAYS;
            GregorianDateTime date;
            GCEarthData earth;
            int prev_paksa = 0;
            int lastMasa = 0;
            int lastGYear = 0;
            String tempStr;
            bool bCalcMoon = (GCDisplaySettings.getValue(4) > 0 || GCDisplaySettings.getValue(5) > 0);
            bool[] bCalcMasa = 
		        { true, true, false, false, false, false, false, false, false, false, false, false, false, false, true, 
		        true, true, false, false, false, false, false, false, false, false, false, false, false, false, true };

            m_nCount = 0;
            m_Location = new CLocationRef();
            m_Location.Set(loc);
            m_vcStart = new GregorianDateTime(begDate);
            m_vcCount = iCount;
            earth = loc.EARTHDATA();

            // alokacia pola
            m_pData = new VAISNAVADAY[nTotalCount];

            // inicializacia poctovych premennych
            m_nCount = nTotalCount;
            m_PureCount = iCount;

            date = new GregorianDateTime();
            date.Set(begDate);
            date.shour = 0.0;
            date.TimezoneHours = loc.offsetUtcHours;
            date.SubtractDays(BEFORE_DAYS);
            date.InitWeekDay();

            weekday = date.dayOfWeek;

            // 1
            // initialization of days
            for (i = 0; i < nTotalCount; i++)
            {
                m_pData[i] = new VAISNAVADAY();
                m_pData[i].date = new GregorianDateTime(date);
                m_pData[i].date.dayOfWeek = weekday;
                date.NextDay();
                weekday = (weekday + 1) % 7;
                m_pData[i].moonrise.SetValue(-1);
                m_pData[i].moonset.SetValue(-1);
            }

            // 2
            // calculating moon times
            for (i = 0; i < nTotalCount; i++)
            {
                m_pData[i].nDST = TTimeZone.determineDaylightStatus(m_pData[i].date, loc.timezoneId);
                //		TRACE("DST %d.%d.%d = %d\n", m_pData[i].date.day, m_pData[i].date.month, m_pData[i].date.year, m_pData[i].nDST);
            }

            // 3
            if (bCalcMoon)
            {
                for (i = 0; i < nTotalCount; i++)
                {
                    if (updateCalculationProgress)
                    {
                        GCUserInterface.SetProgressWindowPos((0 + 85 * i / (iCount + 1)) * 0.908);
                    }

                    GCMoonData.CalcMoonTimes(earth, m_pData[i].date, Convert.ToDouble(m_pData[i].nDST), out m_pData[i].moonrise, out m_pData[i].moonset);

                    if (GCDisplaySettings.getValue(GCDS.CAL_MOON_RISE) != 0 && m_pData[i].moonrise.hour >= 0)
                    {
                        tempStr = m_pData[i].Format(GCStrings.Localized("Moonrise {moonRiseTime} ({dstSig})"));
                        //tempStr = string.Format("{0} {1} ({2})", GCStrings.Localized("Moonrise"), m_pData[i].moonrise.ToShortTimeString(),
                        //    GCStrings.GetDSTSignature(m_pData[i].nDST));
                        m_pData[i].AddEvent(DisplayPriorities.PRIO_MOON, GCDS.CAL_MOON_RISE, tempStr);
                    }

                    if (GCDisplaySettings.getValue(GCDS.CAL_MOON_SET) != 0 && m_pData[i].moonset.hour >= 0)
                    {
                        tempStr = m_pData[i].Format(GCStrings.Localized("Moonset {moonSetTime} ({dstSig})"));
                        //tempStr = string.Format("{0} {1} ({2})", GCStrings.Localized("Moonset"), m_pData[i].moonset.ToShortTimeString(),
                        //    GCStrings.GetDSTSignature(m_pData[i].nDST));
                        m_pData[i].AddEvent(DisplayPriorities.PRIO_MOON, GCDS.CAL_MOON_SET, tempStr);
                    }
                }
            }

            // 4
            // init of astro data
            for (i = 0; i < nTotalCount; i++)
            {
                if (updateCalculationProgress)
                {
                    if (bCalcMoon)
                    {
                        GCUserInterface.SetProgressWindowPos((85 + 2 * i / nTotalCount) * 0.908);
                    }
                    else
                    {
                        GCUserInterface.SetProgressWindowPos(0.588 * 14.8 * i / nTotalCount);
                    }
                }
                m_pData[i].astrodata = new GCAstroData();
                m_pData[i].astrodata.DayCalc(m_pData[i].date, earth);

            }

            bool calc_masa;

            // 5
            // init of masa
            prev_paksa = -1;

            for (i = 0; i < nTotalCount; i++)
            {
                calc_masa = (m_pData[i].astrodata.nPaksa != prev_paksa);
                prev_paksa = m_pData[i].astrodata.nPaksa;

                if (updateCalculationProgress)
                {
                    if (bCalcMoon)
                    {
                        GCUserInterface.SetProgressWindowPos((87 + 2 * i / nTotalCount) * 0.908);
                    }
                    else
                    {
                        GCUserInterface.SetProgressWindowPos(0.588 * (14.8 + 32.2 * i / nTotalCount));
                    }
                }

                if (i == 0)
                    calc_masa = true;

                if (calc_masa)
                {
                    m_pData[i].astrodata.MasaCalc(m_pData[i].date, earth);
                    lastMasa = m_pData[i].astrodata.nMasa;
                    lastGYear = m_pData[i].astrodata.nGaurabdaYear;
                }
                m_pData[i].astrodata.nMasa = lastMasa;
                m_pData[i].astrodata.nGaurabdaYear = lastGYear;

                if (GCDisplaySettings.getValue(GCDS.CAL_ARUN_TITHI) != 0)
                {
                    tempStr = string.Format("{0}: {1}", GCStrings.Localized("Tithi at Arunodaya"), GCTithi.GetName(m_pData[i].astrodata.nTithiArunodaya));
                    m_pData[i].AddEvent(DisplayPriorities.PRIO_ARUN, GCDS.CAL_ARUN_TITHI, tempStr);
                }

                if (GCDisplaySettings.getValue(GCDS.CAL_ARUN_TIME) != 0)
                {
                    tempStr = string.Format("{0} {1} ({2})", GCStrings.Localized("Time of Arunodaya"), m_pData[i].astrodata.sun.arunodaya.ToShortTimeString()
                        , GCStrings.GetDSTSignature(m_pData[i].nDST));
                    m_pData[i].AddEvent(DisplayPriorities.PRIO_ARUN, GCDS.CAL_ARUN_TIME, tempStr);
                }

                if (GCDisplaySettings.getValue(GCDS.CAL_SUN_RISE) != 0)
                {
                    tempStr = string.Format("{0} {1} ({2})", GCStrings.Localized("Sunrise"), 
                        m_pData[i].astrodata.sun.rise.ToShortTimeString(), GCStrings.GetDSTSignature(m_pData[i].nDST));
                    m_pData[i].AddEvent(DisplayPriorities.PRIO_SUN, GCDS.CAL_SUN_RISE, tempStr);
                }

                if (GCDisplaySettings.getValue(GCDS.CAL_SUN_NOON) != 0)
                {
                    tempStr = string.Format("{0} {1} ({2})", GCStrings.Localized("Noon"), m_pData[i].astrodata.sun.noon.ToShortTimeString()
                        , GCStrings.GetDSTSignature(m_pData[i].nDST));
                    m_pData[i].AddEvent(DisplayPriorities.PRIO_SUN, GCDS.CAL_SUN_NOON, tempStr);
                }

                if (GCDisplaySettings.getValue(GCDS.CAL_SUN_SET) != 0)
                {
                    tempStr = string.Format("{0} {1} ({2})", GCStrings.Localized("Sunset"), m_pData[i].astrodata.sun.set.ToShortTimeString()
                        , GCStrings.GetDSTSignature(m_pData[i].nDST));
                    m_pData[i].AddEvent(DisplayPriorities.PRIO_SUN, GCDS.CAL_SUN_SET, tempStr);
                }

                if (GCDisplaySettings.getValue(GCDS.CAL_SUN_LONG) != 0)
                {
                    tempStr = string.Format("{0}: {1} (*)", GCStrings.Localized("Sun Longitude"), m_pData[i].astrodata.sun.longitude_deg);
                    m_pData[i].AddEvent(DisplayPriorities.PRIO_ASTRO, GCDS.CAL_SUN_LONG, tempStr);
                }

                if (GCDisplaySettings.getValue(GCDS.CAL_MOON_LONG) != 0)
                {
                    tempStr = string.Format("{0}: {1} (*)", GCStrings.Localized("Moon Longitude"), m_pData[i].astrodata.moon.longitude_deg);
                    m_pData[i].AddEvent(DisplayPriorities.PRIO_ASTRO, GCDS.CAL_MOON_LONG, tempStr);
                }

                if (GCDisplaySettings.getValue(GCDS.CAL_AYANAMSHA) != 0)
                {
                    tempStr = string.Format("{0} {1} ({2}) (*)", GCStrings.Localized("Ayanamsha"), m_pData[i].astrodata.msAyanamsa, GCAyanamsha.GetAyanamsaName(GCAyanamsha.GetAyanamsaType()));
                    m_pData[i].AddEvent(DisplayPriorities.PRIO_ASTRO, GCDS.CAL_AYANAMSHA, tempStr);
                }

                if (GCDisplaySettings.getValue(GCDS.CAL_JULIAN) != 0)
                {
                    tempStr = string.Format("{0} {1} (*)", GCStrings.Localized("Julian Time"), m_pData[i].astrodata.jdate);
                    m_pData[i].AddEvent(DisplayPriorities.PRIO_ASTRO, GCDS.CAL_JULIAN, tempStr);
                }
            }


            if (GCDisplaySettings.getValue(GCDS.CAL_MASA_CHANGE) != 0)
            {
                String str;

                for (i = BEFORE_DAYS; i < m_PureCount + BEFORE_DAYS + 2; i++)
                {
                    if (m_pData[i - 1].astrodata.nMasa != m_pData[i].astrodata.nMasa)
                    {
                        str = m_pData[i].Format(GCStrings.Localized("First day of {masaName} Masa"));
                        m_pData[i].AddEvent(DisplayPriorities.PRIO_MASA_CHANGE, GCDS.CAL_MASA_CHANGE, str);
                    }

                    if (m_pData[i + 1].astrodata.nMasa != m_pData[i].astrodata.nMasa)
                    {
                        str = m_pData[i].Format(GCStrings.Localized("Last day of {masaName} Masa"));
                        m_pData[i].AddEvent(DisplayPriorities.PRIO_MASA_CHANGE, GCDS.CAL_MASA_CHANGE, str);
                    }
                }
            }

            if (GCDisplaySettings.getValue(GCDS.CAL_DST_CHANGE) != 0)
            {
                for (i = BEFORE_DAYS; i < m_PureCount + BEFORE_DAYS + 2; i++)
                {
                    if (m_pData[i - 1].nDST == 0 && m_pData[i].nDST == 1)
                        m_pData[i].AddEvent(DisplayPriorities.PRIO_DST_CHANGE, GCDS.CAL_DST_CHANGE, GCStrings.Localized("First day of Daylight Saving Time"));
                    else if (m_pData[i].nDST == 1 && m_pData[i + 1].nDST == 0)
                        m_pData[i].AddEvent(DisplayPriorities.PRIO_DST_CHANGE, GCDS.CAL_DST_CHANGE, GCStrings.Localized("Last day of Daylight Saving Time"));
                }
            }

            // 6
            // init of mahadvadasis
            for (i = 2; i < m_PureCount + BEFORE_DAYS; i++)
            {
                m_pData[i].Clear();
                MahadvadasiCalc(i, earth);
            }

            // 6,5
            // init for Ekadasis
            for (i = 3; i < m_PureCount + BEFORE_DAYS; i++)
            {
                if (updateCalculationProgress)
                {
                    if (bCalcMoon)
                    {
                        GCUserInterface.SetProgressWindowPos((89 + 5 * i / nTotalCount) * 0.908);
                    }
                    else
                    {
                        GCUserInterface.SetProgressWindowPos(0.588 * (47.0 + 39.5 * i / nTotalCount));
                    }
                }
                EkadasiCalc(i, earth);
            }

            // 7
            // init of festivals
            for (i = BEFORE_DAYS; i < m_PureCount + BEFORE_DAYS - 1; i++)
            {
                CompleteCalc(i, earth);
            }

            // 8
            // init of festivals
            for (i = BEFORE_DAYS; i < m_PureCount + BEFORE_DAYS; i++)
            {
                ExtendedCalc(i, earth);
            }

            // resolve festivals fasting
            for (i = BEFORE_DAYS; i < m_PureCount + BEFORE_DAYS; i++)
            {
                if (m_pData[i].eparana_time1 > 0.0)
                {
                    m_pData[i].eparana_time1 += m_pData[i].nDST;
                }

                if (m_pData[i].eparana_time2 > 0.0)
                {
                    m_pData[i].eparana_time2 += m_pData[i].nDST;
                }

                if (m_pData[i].astrodata.sun.longitude_deg > 0.0)
                {
                    m_pData[i].astrodata.sun.rise.hour += m_pData[i].nDST;
                    m_pData[i].astrodata.sun.set.hour += m_pData[i].nDST;
                    m_pData[i].astrodata.sun.noon.hour += m_pData[i].nDST;
                    m_pData[i].astrodata.sun.arunodaya.hour += m_pData[i].nDST;
                }

                ResolveFestivalsFasting(i);
            }

            // init for sankranti
            date.Set(m_pData[0].date);
            i = 0;
            bool bFoundSan;
            int zodiac;
            int i_target;
            do
            {
                date.Set(GCSankranti.GetNextSankranti(date, out zodiac));
                date.shour += TTimeZone.determineDaylightStatus(date, loc.timezoneId) / 24.0;
                date.NormalizeValues();

                bFoundSan = false;
                for (i = 0; i < m_nCount - 1; i++)
                {
                    i_target = -1;

                    switch (GCSankranti.GetSankrantiType())
                    {
                        case 0:
                            if (date.CompareYMD(m_pData[i].date) == 0)
                            {
                                i_target = i;
                            }
                            break;
                        case 1:
                            if (date.CompareYMD(m_pData[i].date) == 0)
                            {
                                if (date.shour < m_pData[i].astrodata.sun.rise.GetDayTime())
                                {
                                    i_target = i - 1;
                                }
                                else
                                {
                                    i_target = i;
                                }
                            }
                            break;
                        case 2:
                            if (date.CompareYMD(m_pData[i].date) == 0)
                            {
                                if (date.shour > m_pData[i].astrodata.sun.noon.GetDayTime())
                                {
                                    i_target = i + 1;
                                }
                                else
                                {
                                    i_target = i;
                                }
                            }
                            break;
                        case 3:
                            if (date.CompareYMD(m_pData[i].date) == 0)
                            {
                                if (date.shour > m_pData[i].astrodata.sun.set.GetDayTime())
                                {
                                    i_target = i + 1;
                                }
                                else
                                {
                                    i_target = i;
                                }
                            }
                            break;
                    }

                    if (i_target >= 0)
                    {
                        String str;

                        m_pData[i_target].sankranti_zodiac = zodiac;
                        m_pData[i_target].sankranti_day = new GregorianDateTime(date);

                        if (GCDisplaySettings.getValue(GCDS.CAL_SANKRANTI) != 0)
                        {
                            str = m_pData[i_target].Format(GCStrings.Localized("  {sankranti.rasiName} Sankranti (Sun enters {sankranti.rasiNameEn} on {sankranti.day} {sankranti.monthAbr}, {sankranti.hour} {sankranti.minRound}) ({dstSig})"));

                            VAISNAVAEVENT dc = m_pData[i_target].AddEvent(DisplayPriorities.PRIO_SANKRANTI, GCDS.CAL_SANKRANTI, str);
                            dc.spec = "sankranti";
                        }
                        bFoundSan = true;
                        break;
                    }
                }
                date.NextDay();
                date.NextDay();
            }
            while (bFoundSan == true);

            // 9
            // init for festivals dependent on sankranti
            for (i = BEFORE_DAYS; i < m_PureCount + BEFORE_DAYS; i++)
            {
                foreach (GCCalendarEventSankranti se in GCCalendarEventList.SanBasedList)
                {
                    if (m_pData[i - se.DayOffset].sankranti_zodiac == se.Rasi)
                    {
                        m_pData[i].AddEvent(DisplayPriorities.PRIO_FESTIVALS_5, GCDS.CAL_FEST_5, se.Text);
                    }
                }
            }

            // 10
            // init ksaya data
            // init of second day of vriddhi
            for (i = BEFORE_DAYS; i < m_PureCount + BEFORE_DAYS; i++)
            {
                if (updateCalculationProgress)
                {
                    if (bCalcMoon)
                    {
                        GCUserInterface.SetProgressWindowPos((94 + 6 * i / nTotalCount) * 0.908);
                    }
                    else
                    {
                        GCUserInterface.SetProgressWindowPos(0.588 * (86.5 + 13.5 * i / nTotalCount));
                    }
                }

                if (m_pData[i].astrodata.nTithi == m_pData[i - 1].astrodata.nTithi)
                {
                    if (GCDisplaySettings.getValue(GCDS.CAL_VRDDHI) != 0)
                        m_pData[i].AddEvent(DisplayPriorities.PRIO_KSAYA, GCDS.CAL_VRDDHI, GCStrings.getString(90));
                }
                else if (m_pData[i].astrodata.nTithi != ((m_pData[i - 1].astrodata.nTithi + 1) % 30))
                {
                    String str;
                    GregorianDateTime day1, d1, d2;

                    day1 = new GregorianDateTime();
                    day1.Set(m_pData[i].date);
                    day1.shour = m_pData[i].astrodata.sun.sunrise_deg / 360.0 + earth.offsetUtcHours / 24.0;

                    GCTithi.GetPrevTithiStart(earth, day1, out d2);
                    day1.Set(d2);
                    day1.shour -= 0.1;
                    day1.NormalizeValues();
                    GCTithi.GetPrevTithiStart(earth, day1, out d1);

                    d1.shour += (m_pData[i].nDST / 24.0);
                    d2.shour += (m_pData[i].nDST / 24.0);

                    d1.NormalizeValues();
                    d2.NormalizeValues();

                    /*
                    // zaciatok ksaya tithi
                    h = GCMath.IntFloor(d1.shour * 24);
                    m = Convert.ToInt32(Math.Round(GCMath.getFraction(d1.shour * 24) * 60));
                    str2 = string.Format("{0} {1} {2:00}:{3:00}", d1.day, GregorianDateTime.GetMonthAbreviation(d1.month), h, m);

                    // end of ksaya tithi
                    h = GCMath.IntFloor(d2.shour * 24);
                    m = Convert.ToInt32(Math.Round(GCMath.getFraction(d2.shour * 24) * 60));
                    str3 = string.Format("{0} {1} {2:00}:{3:00}", d2.day, GregorianDateTime.GetMonthAbreviation(d2.month), h, m);
                    */

                    str = m_pData[i].Format(GCStrings.Localized("Kshaya tithi: {prevTithiName} — {0} to {1} ({dstSig})"), 
                        d1.Format("{day} {monthAbr} {hour}:{minRound}"),
                        d2.Format("{day} {monthAbr} {hour}:{minRound}"));

                    // print info
                    //str = string.Format(str, d1.Format("{day} {monthAbr} {hour}:{minRound}"),
                    //    d2.Format("{day} {monthAbr} {hour}:{minRound}"));

                    m_pData[i].AddEvent(DisplayPriorities.PRIO_KSAYA, GCDS.CAL_KSAYA, str);

                }
            }

            VAISNAVAEVENTComparer vec = new VAISNAVAEVENTComparer();

            for (i = BEFORE_DAYS; i < m_PureCount + BEFORE_DAYS; i++)
            {
                m_pData[i].dayEvents.Sort(vec);
            }


            return 1;

        }

        public int EkadasiCalc(int nIndex, GCEarthData earth)
        {
            VAISNAVADAY s = m_pData[nIndex - 1];
            VAISNAVADAY t = m_pData[nIndex];
            VAISNAVADAY u = m_pData[nIndex + 1];

            if (GCTithi.TITHI_EKADASI(t.astrodata.nTithi))
            {
                // if TAT < 11 then NOT_EKADASI
                if (GCTithi.TITHI_LESS_EKADASI(t.astrodata.nTithiArunodaya))
                {
                    t.nMhdType = MahadvadasiType.EV_NULL;
                    t.ekadasi_vrata_name = "";
                    t.nFastType = FastType.FAST_NULL;
                }
                else
                {
                    // else ak MD13 then MHD1 and/or 3
                    if (GCTithi.TITHI_EKADASI(s.astrodata.nTithi) && GCTithi.TITHI_EKADASI(s.astrodata.nTithiArunodaya))
                    {
                        if (GCTithi.TITHI_TRAYODASI(u.astrodata.nTithi))
                        {
                            t.nMhdType = MahadvadasiType.EV_UNMILANI_TRISPRSA;
                            t.ekadasi_vrata_name = GCEkadasi.GetEkadasiName(t.astrodata.nMasa, t.astrodata.nPaksa);
                            t.nFastType = FastType.FAST_EKADASI;
                        }
                        else
                        {
                            t.nMhdType = MahadvadasiType.EV_UNMILANI;
                            t.ekadasi_vrata_name = GCEkadasi.GetEkadasiName(t.astrodata.nMasa, t.astrodata.nPaksa);
                            t.nFastType = FastType.FAST_EKADASI;
                        }
                    }
                    else
                    {
                        if (GCTithi.TITHI_TRAYODASI(u.astrodata.nTithi))
                        {
                            t.nMhdType = MahadvadasiType.EV_TRISPRSA;
                            t.ekadasi_vrata_name = GCEkadasi.GetEkadasiName(t.astrodata.nMasa, t.astrodata.nPaksa);
                            t.nFastType = FastType.FAST_EKADASI;
                        }
                        else
                        {
                            // else ak U je MAHADVADASI then NOT_EKADASI
                            if (GCTithi.TITHI_EKADASI(u.astrodata.nTithi) || (u.nMhdType >= MahadvadasiType.EV_SUDDHA))
                            {
                                t.nMhdType = MahadvadasiType.EV_NULL;
                                t.ekadasi_vrata_name = "";
                                t.nFastType = FastType.FAST_NULL;
                            }
                            else if (u.nMhdType == MahadvadasiType.EV_NULL)
                            {
                                // else suddha ekadasi
                                t.nMhdType = MahadvadasiType.EV_SUDDHA;
                                t.ekadasi_vrata_name = GCEkadasi.GetEkadasiName(t.astrodata.nMasa, t.astrodata.nPaksa);
                                t.nFastType = FastType.FAST_EKADASI;
                            }
                        }
                    }
                }
            }
            // test for break fast

            if (s.nFastType == FastType.FAST_EKADASI)
            {
                double parBeg, parEnd;

                CalculateEParana(s, t, out parBeg, out parEnd, earth);

            }

            return 1;
        }


        public int CompleteCalc(int nIndex, GCEarthData earth)
        {
            VAISNAVADAY s = m_pData[nIndex - 1];
            VAISNAVADAY t = m_pData[nIndex];
            VAISNAVADAY u = m_pData[nIndex + 1];
            VAISNAVADAY v = m_pData[nIndex + 2];
            String tmp;

            // test for Govardhan-puja
            if (t.astrodata.nMasa == MasaId.DAMODARA_MASA)
            {
                if (t.astrodata.nTithi == TithiId.TITHI_GAURA_PRATIPAT)
                {
                    GCMoonData.CalcMoonTimes(earth, u.date, s.nDST, out s.moonrise, out s.moonset);
                    GCMoonData.CalcMoonTimes(earth, t.date, t.nDST, out t.moonrise, out t.moonset);
                    if (s.astrodata.nTithi == TithiId.TITHI_GAURA_PRATIPAT)
                    {
                    }
                    else if (u.astrodata.nTithi == TithiId.TITHI_GAURA_PRATIPAT)
                    {
                        if (t.moonrise.hour >= 0)
                        {
                            if (t.moonrise.IsGreaterThan(t.astrodata.sun.rise))
                                // today is GOVARDHANA PUJA
                                t.AddSpecFestival(SpecialFestivalId.SPEC_GOVARDHANPUJA, GCDS.CAL_FEST_1);
                            else
                                u.AddSpecFestival(SpecialFestivalId.SPEC_GOVARDHANPUJA, GCDS.CAL_FEST_1);
                        }
                        else if (u.moonrise.hour >= 0)
                        {
                            if (u.moonrise.IsLessThan(u.astrodata.sun.rise))
                                // today is GOVARDHANA PUJA
                                t.AddSpecFestival(SpecialFestivalId.SPEC_GOVARDHANPUJA, GCDS.CAL_FEST_1);
                            else
                                u.AddSpecFestival(SpecialFestivalId.SPEC_GOVARDHANPUJA, GCDS.CAL_FEST_1);
                        }
                        else
                        {
                            t.AddSpecFestival(SpecialFestivalId.SPEC_GOVARDHANPUJA, GCDS.CAL_FEST_1);
                        }
                    }
                    else
                    {
                        // today is GOVARDHANA PUJA
                        t.AddSpecFestival(SpecialFestivalId.SPEC_GOVARDHANPUJA, GCDS.CAL_FEST_1);
                    }

                }
                else if ((t.astrodata.nTithi == TithiId.TITHI_GAURA_DVITIYA) && (s.astrodata.nTithi == TithiId.TITHI_AMAVASYA))
                {
                    // today is GOVARDHANA PUJA
                    t.AddSpecFestival(SpecialFestivalId.SPEC_GOVARDHANPUJA, GCDS.CAL_FEST_1);
                }
            }

            int mid_nak_t, mid_nak_u;

            if (t.astrodata.nMasa == MasaId.HRSIKESA_MASA)
            {
                // test for Janmasthami
                if (IsFestivalDay(s, t, TithiId.TITHI_KRSNA_ASTAMI))
                {
                    // if next day is not astami, so that means that astami is not vriddhi
                    // then today is SKJ
                    if (u.astrodata.nTithi != TithiId.TITHI_KRSNA_ASTAMI)
                    {
                        // today is Sri Krsna Janmasthami
                        t.AddSpecFestival(SpecialFestivalId.SPEC_JANMASTAMI, GCDS.CAL_FEST_0);
                        u.AddSpecFestival(SpecialFestivalId.SPEC_NANDAUTSAVA, GCDS.CAL_FEST_1);
                        u.AddSpecFestival(SpecialFestivalId.SPEC_PRABHAPP, GCDS.CAL_FEST_2);
                        //				t.nFastType = (GCDisplaySettings.getValue(42) ? FAST_MIDNIGHT : FAST_TODAY);
                    }
                    else // tithi is vriddhi and we have to test both days
                    {
                        // test when both days have ROHINI
                        if ((t.astrodata.nNaksatra == NaksatraId.ROHINI_NAKSATRA) && (u.astrodata.nNaksatra == NaksatraId.ROHINI_NAKSATRA))
                        {
                            mid_nak_t = (int)GCNaksatra.CalculateMidnightNaksatra(t.date, earth);
                            mid_nak_u = (int)GCNaksatra.CalculateMidnightNaksatra(u.date, earth);

                            // test when both days have modnight naksatra ROHINI
                            if ((NaksatraId.ROHINI_NAKSATRA == mid_nak_u)
                                && (mid_nak_t == NaksatraId.ROHINI_NAKSATRA))
                            {
                                // choice day which is monday or wednesday
                                if ((u.date.dayOfWeek == WeekDayId.DW_MONDAY) || (u.date.dayOfWeek == WeekDayId.DW_WEDNESDAY))
                                {
                                    u.AddSpecFestival(SpecialFestivalId.SPEC_JANMASTAMI, GCDS.CAL_FEST_0);
                                    v.AddSpecFestival(SpecialFestivalId.SPEC_NANDAUTSAVA, GCDS.CAL_FEST_1);
                                    v.AddSpecFestival(SpecialFestivalId.SPEC_PRABHAPP, GCDS.CAL_FEST_2);
                                    //							u.nFastType = (GCDisplaySettings.getValue(42) ? FAST_MIDNIGHT : FAST_TODAY);
                                }
                                else
                                {
                                    // today is Sri Krsna Janmasthami
                                    t.AddSpecFestival(SpecialFestivalId.SPEC_JANMASTAMI, GCDS.CAL_FEST_0);
                                    u.AddSpecFestival(SpecialFestivalId.SPEC_NANDAUTSAVA, GCDS.CAL_FEST_1);
                                    u.AddSpecFestival(SpecialFestivalId.SPEC_PRABHAPP, GCDS.CAL_FEST_2);
                                    //							t.nFastType = (GCDisplaySettings.getValue(42) ? FAST_MIDNIGHT : FAST_TODAY);
                                }
                            }
                            else if (mid_nak_t == NaksatraId.ROHINI_NAKSATRA)
                            {
                                // today is Sri Krsna Janmasthami
                                t.AddSpecFestival(SpecialFestivalId.SPEC_JANMASTAMI, GCDS.CAL_FEST_0);
                                u.AddSpecFestival(SpecialFestivalId.SPEC_NANDAUTSAVA, GCDS.CAL_FEST_1);
                                //						t.nFastType = (GCDisplaySettings.getValue(42) ? FAST_MIDNIGHT : FAST_TODAY);
                                u.AddSpecFestival(SpecialFestivalId.SPEC_PRABHAPP, GCDS.CAL_FEST_2);
                            }
                            else if (mid_nak_u == NaksatraId.ROHINI_NAKSATRA)
                            {
                                u.AddSpecFestival(SpecialFestivalId.SPEC_JANMASTAMI, GCDS.CAL_FEST_0);
                                v.AddSpecFestival(SpecialFestivalId.SPEC_NANDAUTSAVA, GCDS.CAL_FEST_1);
                                v.AddSpecFestival(SpecialFestivalId.SPEC_PRABHAPP, GCDS.CAL_FEST_2);
                                //						u.nFastType = (GCDisplaySettings.getValue(42) ? FAST_MIDNIGHT : FAST_TODAY);
                            }
                            else
                            {
                                if ((u.date.dayOfWeek == WeekDayId.DW_MONDAY) || (u.date.dayOfWeek == WeekDayId.DW_WEDNESDAY))
                                {
                                    u.AddSpecFestival(SpecialFestivalId.SPEC_JANMASTAMI, GCDS.CAL_FEST_0);
                                    v.AddSpecFestival(SpecialFestivalId.SPEC_NANDAUTSAVA, GCDS.CAL_FEST_1);
                                    v.AddSpecFestival(SpecialFestivalId.SPEC_PRABHAPP, GCDS.CAL_FEST_2);
                                    //							u.nFastType = (GCDisplaySettings.getValue(42) ? FAST_MIDNIGHT : FAST_TODAY);
                                }
                                else
                                {
                                    // today is Sri Krsna Janmasthami
                                    t.AddSpecFestival(SpecialFestivalId.SPEC_JANMASTAMI, GCDS.CAL_FEST_0);
                                    u.AddSpecFestival(SpecialFestivalId.SPEC_NANDAUTSAVA, GCDS.CAL_FEST_1);
                                    u.AddSpecFestival(SpecialFestivalId.SPEC_PRABHAPP, GCDS.CAL_FEST_2);
                                    //							t.nFastType = (GCDisplaySettings.getValue(42) ? FAST_MIDNIGHT : FAST_TODAY);
                                }
                            }
                        }
                        else if (t.astrodata.nNaksatra == NaksatraId.ROHINI_NAKSATRA)
                        {
                            // today is Sri Krsna Janmasthami
                            t.AddSpecFestival(SpecialFestivalId.SPEC_JANMASTAMI, GCDS.CAL_FEST_0);
                            u.AddSpecFestival(SpecialFestivalId.SPEC_NANDAUTSAVA, GCDS.CAL_FEST_1);
                            u.AddSpecFestival(SpecialFestivalId.SPEC_PRABHAPP, GCDS.CAL_FEST_2);
                            //					t.nFastType = (GCDisplaySettings.getValue(42) ? FAST_MIDNIGHT : FAST_TODAY);
                        }
                        else if (u.astrodata.nNaksatra == NaksatraId.ROHINI_NAKSATRA)
                        {
                            u.AddSpecFestival(SpecialFestivalId.SPEC_JANMASTAMI, GCDS.CAL_FEST_0);
                            v.AddSpecFestival(SpecialFestivalId.SPEC_NANDAUTSAVA, GCDS.CAL_FEST_1);
                            v.AddSpecFestival(SpecialFestivalId.SPEC_PRABHAPP, GCDS.CAL_FEST_2);
                            //					u.nFastType = (GCDisplaySettings.getValue(42) ? FAST_MIDNIGHT : FAST_TODAY);
                        }
                        else
                        {
                            if ((u.date.dayOfWeek == WeekDayId.DW_MONDAY) || (u.date.dayOfWeek == WeekDayId.DW_WEDNESDAY))
                            {
                                u.AddSpecFestival(SpecialFestivalId.SPEC_JANMASTAMI, GCDS.CAL_FEST_0);
                                v.AddSpecFestival(SpecialFestivalId.SPEC_NANDAUTSAVA, GCDS.CAL_FEST_1);
                                v.AddSpecFestival(SpecialFestivalId.SPEC_PRABHAPP, GCDS.CAL_FEST_2);
                                //						u.nFastType = (GCDisplaySettings.getValue(42) ? FAST_MIDNIGHT : FAST_TODAY);
                            }
                            else
                            {
                                // today is Sri Krsna Janmasthami
                                t.AddSpecFestival(SpecialFestivalId.SPEC_JANMASTAMI, GCDS.CAL_FEST_0);
                                u.AddSpecFestival(SpecialFestivalId.SPEC_NANDAUTSAVA, GCDS.CAL_FEST_1);
                                u.AddSpecFestival(SpecialFestivalId.SPEC_PRABHAPP, GCDS.CAL_FEST_2);
                                //						t.nFastType = (GCDisplaySettings.getValue(42) ? FAST_MIDNIGHT : FAST_TODAY);
                            }
                        }
                    }
                }

                /*		if (s.nSpecFestival == SpecialFestivalId.SPEC_JANMASTAMI)
                        {
                            // today is SP VyasaPuja
                            // today is Nandottsava
                            t.AddSpecFestival(SpecialFestivalId.SPEC_NANDOTSAVA);
                //			t.nFastType = FAST_NOON;
                        }
                */
                // test for Vamana Dvadasi
                /*		if (IsFestivalDay(s, t, TithiId.TITHI_GAURA_DVADASI))
                        {
                            // today is Vamana Dvadasi
                            t.nSpecFestival = SpecialFestivalId.SPEC_VAMANADVADASI;

                            if (t.nFastType == FAST_EKADASI)
                            {
                                t.nFeasting = FEAST_TOMMOROW_FAST_TODAY;
                                u.nFeasting = FEAST_TODAY_FAST_YESTERDAY;
                            }
                            else
                            {
                                t.nFeasting = FEAST_TODAY_FAST_YESTERDAY;
                            }
                        }*/
            }

            // test for Varaha Dvadasi
            /*if (t.astrodata.nMasa == MADHAVA_MASA)
            {
                if (((t.astrodata.nTithi == TithiId.TITHI_GAURA_DVADASI) && (s.astrodata.nTithi < TithiId.TITHI_GAURA_DVADASI))
                    || ((t.astrodata.nTithi == TithiId.TITHI_GAURA_EKADASI) && (u.astrodata.nTithi == TithiId.TITHI_GAURA_TRAYODASI)))
                {
                    // today is Varaha Dvadasi
                    t.nSpecFestival = SpecialFestivalId.SPEC_VARAHADVADASI;

                    if (t.nFastType == FAST_EKADASI)
                    {
                        t.nFeasting = FEAST_TOMMOROW_FAST_TODAY;
                        u.nFeasting = FEAST_TODAY_FAST_YESTERDAY;
                    }
                    else
                    {
                        t.nFeasting = FEAST_TODAY_FAST_YESTERDAY;
                    }
                }
            }*/

            // test for RathaYatra
            if (t.astrodata.nMasa == MasaId.VAMANA_MASA)
            {
                if (IsFestivalDay(s, t, TithiId.TITHI_GAURA_DVITIYA))
                {
                    t.AddSpecFestival(SpecialFestivalId.SPEC_RATHAYATRA, GCDS.CAL_FEST_1);
                }

                if (nIndex > 4)
                {
                    if (IsFestivalDay(m_pData[nIndex - 5], m_pData[nIndex - 4], TithiId.TITHI_GAURA_DVITIYA))
                    {
                        t.AddSpecFestival(SpecialFestivalId.SPEC_HERAPANCAMI, GCDS.CAL_FEST_1);
                    }
                }

                if (nIndex > 8)
                {
                    if (IsFestivalDay(m_pData[nIndex - 9], m_pData[nIndex - 8], TithiId.TITHI_GAURA_DVITIYA))
                    {
                        t.AddSpecFestival(SpecialFestivalId.SPEC_RETURNRATHA, GCDS.CAL_FEST_1);
                    }
                }

                if (IsFestivalDay(m_pData[nIndex], m_pData[nIndex + 1], TithiId.TITHI_GAURA_DVITIYA))
                {
                    t.AddSpecFestival(SpecialFestivalId.SPEC_GUNDICAMARJANA, GCDS.CAL_FEST_1);
                }

            }

            // test for Gaura Purnima
            if (s.astrodata.nMasa == MasaId.GOVINDA_MASA)
            {
                if (IsFestivalDay(s, t, TithiId.TITHI_PURNIMA))
                {
                    t.AddSpecFestival(SpecialFestivalId.SPEC_GAURAPURNIMA, GCDS.CAL_FEST_0);
                    //			t.nFastType = FAST_MOONRISE;
                }
            }

            // test for Jagannatha Misra festival
            if (m_pData[nIndex - 2].astrodata.nMasa == MasaId.GOVINDA_MASA)
            {
                if (IsFestivalDay(m_pData[nIndex - 2], s, TithiId.TITHI_PURNIMA))
                {
                    t.AddSpecFestival(SpecialFestivalId.SPEC_MISRAFESTIVAL, GCDS.CAL_FEST_1);
                }
            }


            // ------------------------
            // test for other festivals
            // ------------------------

            int n, n2;
            int _masa_from = -1, _masa_to;
            int _tithi_from = -1, _tithi_to;
            GCCalendarEvent pEvx;

            bool s1 = true, s2 = false;

            if (t.astrodata.nMasa > 11)
                goto other_fest;

            n = t.astrodata.nMasa * 30 + t.astrodata.nTithi;
            _tithi_to = t.astrodata.nTithi;
            _masa_to = t.astrodata.nMasa;

            if (s.astrodata.nTithi == t.astrodata.nTithi)
                s1 = false;

            // if ksaya tithi, then s2 is true
            if ((t.astrodata.nTithi != s.astrodata.nTithi) &&
                (t.astrodata.nTithi != (s.astrodata.nTithi + 1) % 30))
            {
                n2 = (n + 359) % 360; // this is index into table of festivals for previous tithi
                _tithi_from = n2 % 30;
                _masa_from = n2 / 30;
                s2 = true;
            }

            int currFestTop = 0;
            VAISNAVAEVENT md = null;

            if (s2)
            {
                for (int kn = 0; kn < GCCalendarEventList.Count(); kn++)
                {
                    pEvx = GCCalendarEventList.EventAtIndex(kn);
                    if (pEvx.nMasa == _masa_from && pEvx.nTithi == _tithi_from && pEvx.nUsed != 0 && pEvx.nVisible != 0)
                    {
                        md = t.AddEvent(DisplayPriorities.PRIO_FESTIVALS_0 + pEvx.nClass * 100 + currFestTop, GCDS.CAL_FEST_0 + pEvx.nClass,
                            pEvx.strText);
                        currFestTop += 5;
                        if (pEvx.nFastType > 0)
                        {
                            md.fasttype = pEvx.nFastType;
                            md.fastsubject = pEvx.strFastSubject;
                        }

                        if (GCDisplaySettings.getValue(51) != 2 && pEvx.nStartYear > -7000)
                        {
                            String ss1;
                            int years = t.astrodata.nGaurabdaYear - (pEvx.nStartYear - 1496);
                            string appx = "th";
                            if (years % 10 == 1) appx = "st";
                            else if (years % 10 == 2) appx = "nd";
                            else if (years % 10 == 3) appx = "rd";
                            if (GCDisplaySettings.getValue(51) == 0)
                            {
                                ss1 = string.Format("{0} ({1}{2} anniversary)", pEvx.strText, years, appx);
                            }
                            else
                            {
                                ss1 = string.Format("{0} ({1}{2})", pEvx.strText, years, appx);
                            }
                            md.text = ss1;
                        }

                    }
                }
            }

            if (s1)
            {
                for (int kn = 0; kn < GCCalendarEventList.Count(); kn++)
                {
                    pEvx = GCCalendarEventList.EventAtIndex(kn);
                    if (pEvx.nMasa == _masa_to && pEvx.nTithi == _tithi_to && pEvx.nUsed != 0 && pEvx.nVisible != 0)
                    {
                        md = t.AddEvent(DisplayPriorities.PRIO_FESTIVALS_0 + pEvx.nClass * 100 + currFestTop, GCDS.CAL_FEST_0 + pEvx.nClass,
                            pEvx.strText);
                        currFestTop += 5;
                        if (pEvx.nFastType > 0)
                        {
                            md.fasttype = pEvx.nFastType;
                            md.fastsubject = pEvx.strFastSubject;
                        }

                        if (GCDisplaySettings.getValue(51) != 2 && pEvx.nStartYear > -7000)
                        {
                            String ss1;
                            int years = t.astrodata.nGaurabdaYear - (pEvx.nStartYear - 1496);
                            string appx = "th";
                            if (years % 10 == 1) appx = "st";
                            else if (years % 10 == 2) appx = "nd";
                            else if (years % 10 == 3) appx = "rd";
                            if (GCDisplaySettings.getValue(51) == 0)
                            {
                                ss1 = string.Format("{0} ({1}{2} anniversary)", pEvx.strText, years, appx);
                            }
                            else
                            {
                                ss1 = string.Format("{0} ({1}{2})", pEvx.strText, years, appx);
                            }
                            md.text = ss1;
                        }
                    }
                }
            }

        other_fest:
            // ---------------------------
            // bhisma pancaka test
            // ---------------------------

            if (t.astrodata.nMasa == MasaId.DAMODARA_MASA)
            {
                if ((t.astrodata.nPaksa == PaksaId.GAURA_PAKSA) && (t.nFastType == FastType.FAST_EKADASI))
                {
                    t.AddEvent(DisplayPriorities.PRIO_CM_DAYNOTE, GCDS.DISP_ALWAYS, GCStrings.getString(81));
                    //if (t.festivals.GetLength() > 0)
                    //	t.festivals += "#";
                    //t.festivals += GCStrings.getString(81);
                }
            }

            // ---------------------------
            // caturmasya tests
            // ---------------------------

            // first month for punima and ekadasi systems
            if (t.astrodata.nMasa == MasaId.VAMANA_MASA)
            {
                // purnima system
                if (GCTithi.TITHI_TRANSIT(t.astrodata.nTithi, u.astrodata.nTithi, TithiId.TITHI_GAURA_CATURDASI, TithiId.TITHI_PURNIMA))
                {
                    if (GCDisplaySettings.getValue(GCDS.CATURMASYA_PURNIMA) != 0)
                    {
                        tmp = string.Format("{0} [PURNIMA SYSTEM]", GCStrings.getString(112));
                        u.AddEvent(DisplayPriorities.PRIO_CM_DAY, GCDS.CATURMASYA_PURNIMA, tmp);
                        u.AddEvent(DisplayPriorities.PRIO_CM_DAYNOTE, GCDS.CATURMASYA_PURNIMA, GCStrings.getString(114));
                        u.nCaturmasya = CaturmasyaCodes.CMASYA_SYSTEM_PURNIMA | CaturmasyaCodes.CMASYA_DAY_FIRST | CaturmasyaCodes.CMASYA_MONTH_1;
                    }
                }

                // ekadasi system
                //if (TithiId.TITHI_TRANSIT(s.astrodata.nTithi, t.astrodata.nTithi, TithiId.TITHI_GAURA_DASAMI, TithiId.TITHI_GAURA_EKADASI))
                if ((t.astrodata.nPaksa == PaksaId.GAURA_PAKSA) && (t.nMhdType != MahadvadasiType.EV_NULL))
                {
                    if (GCDisplaySettings.getValue(GCDS.CATURMASYA_EKADASI) != 0)
                    {
                        tmp = string.Format("{0} [EKADASI SYSTEM]", GCStrings.getString(112));
                        t.AddEvent(DisplayPriorities.PRIO_CM_DAY, GCDS.CATURMASYA_EKADASI, tmp);
                        t.AddEvent(DisplayPriorities.PRIO_CM_DAYNOTE, GCDS.CATURMASYA_EKADASI, GCStrings.getString(114));
                        t.nCaturmasya = CaturmasyaCodes.CMASYA_SYSTEM_EKADASI | CaturmasyaCodes.CMASYA_DAY_FIRST | CaturmasyaCodes.CMASYA_MONTH_1;
                    }
                }
            }

            // first month for pratipat system
            // month transit for purnima and ekadasi systems
            if (t.astrodata.nMasa == MasaId.SRIDHARA_MASA)
            {
                if (s.astrodata.nMasa == MasaId.ADHIKA_MASA)
                {
                    t.AddEvent(DisplayPriorities.PRIO_CM_DAYNOTE, GCDS.DISP_ALWAYS, GCStrings.getString(115));
                }

                // pratipat system
                if (GCTithi.TITHI_TRANSIT(s.astrodata.nTithi, t.astrodata.nTithi, TithiId.TITHI_PURNIMA, TithiId.TITHI_KRSNA_PRATIPAT))
                {
                    if (GCDisplaySettings.getValue(GCDS.CATURMASYA_PRATIPAT) != 0)
                    {
                        tmp = string.Format("{0} [PRATIPAT SYSTEM]", GCStrings.getString(112));
                        t.AddEvent(DisplayPriorities.PRIO_CM_DAY, GCDS.CATURMASYA_PRATIPAT, tmp);
                        t.AddEvent(DisplayPriorities.PRIO_CM_DAYNOTE, GCDS.CATURMASYA_PRATIPAT, GCStrings.getString(114));
                        t.nCaturmasya = CaturmasyaCodes.CMASYA_SYSTEM_PRATIPAT | CaturmasyaCodes.CMASYA_DAY_FIRST | CaturmasyaCodes.CMASYA_MONTH_1;
                    }
                }

                // first day of particular month for PURNIMA system, when purnima is not KSAYA
                if (GCTithi.TITHI_TRANSIT(t.astrodata.nTithi, u.astrodata.nTithi, TithiId.TITHI_GAURA_CATURDASI, TithiId.TITHI_PURNIMA))
                {
                    if (GCDisplaySettings.getValue(GCDS.CATURMASYA_PURNIMA) != 0)
                    {
                        tmp = string.Format("{0} [PURNIMA SYSTEM]", GCStrings.getString(116));
                        u.AddEvent(DisplayPriorities.PRIO_CM_DAY, GCDS.CATURMASYA_PURNIMA, tmp);
                        u.AddEvent(DisplayPriorities.PRIO_CM_DAYNOTE, GCDS.CATURMASYA_PURNIMA, GCStrings.getString(118));
                        u.nCaturmasya = CaturmasyaCodes.CMASYA_SYSTEM_PURNIMA | CaturmasyaCodes.CMASYA_DAY_FIRST | CaturmasyaCodes.CMASYA_MONTH_2;
                        tmp = string.Format("{0} [PURNIMA SYSTEM]", GCStrings.getString(113));
                        t.AddEvent(DisplayPriorities.PRIO_CM_DAY, GCDS.CATURMASYA_PURNIMA, tmp);
                        t.nCaturmasya = CaturmasyaCodes.CMASYA_SYSTEM_PURNIMA | CaturmasyaCodes.CMASYA_DAY_LAST | CaturmasyaCodes.CMASYA_MONTH_1;
                    }
                }

                // ekadasi system
                //if (TITHI_TRANSIT(s.astrodata.nTithi, t.astrodata.nTithi, TITHI_GAURA_DASAMI, TITHI_GAURA_EKADASI))
                if ((t.astrodata.nPaksa == PaksaId.GAURA_PAKSA) && (t.nMhdType != MahadvadasiType.EV_NULL))
                {
                    if (GCDisplaySettings.getValue(GCDS.CATURMASYA_EKADASI) != 0)
                    {
                        tmp = string.Format("{0} [EKADASI SYSTEM]", GCStrings.getString(116));
                        t.AddEvent(DisplayPriorities.PRIO_CM_DAY, GCDS.CATURMASYA_EKADASI, tmp);
                        t.AddEvent(DisplayPriorities.PRIO_CM_DAYNOTE, GCDS.CATURMASYA_EKADASI, GCStrings.getString(118));
                        t.nCaturmasya = CaturmasyaCodes.CMASYA_SYSTEM_EKADASI | CaturmasyaCodes.CMASYA_DAY_FIRST | CaturmasyaCodes.CMASYA_MONTH_2;
                        tmp = string.Format("{0} [EKADASI SYSTEM]", GCStrings.getString(113));
                        s.AddEvent(DisplayPriorities.PRIO_CM_DAY, GCDS.CATURMASYA_EKADASI, tmp);
                        s.nCaturmasya = CaturmasyaCodes.CMASYA_SYSTEM_EKADASI | CaturmasyaCodes.CMASYA_DAY_LAST | CaturmasyaCodes.CMASYA_MONTH_1;
                    }
                }
            }

            // second month for pratipat system
            // month transit for purnima and ekadasi systems
            if (t.astrodata.nMasa == MasaId.HRSIKESA_MASA)
            {
                if (s.astrodata.nMasa == MasaId.ADHIKA_MASA)
                {
                    t.AddEvent(DisplayPriorities.PRIO_CM_DAYNOTE, GCDS.DISP_ALWAYS, GCStrings.getString(119));
                }

                // pratipat system
                if (GCTithi.TITHI_TRANSIT(s.astrodata.nTithi, t.astrodata.nTithi, TithiId.TITHI_PURNIMA, TithiId.TITHI_KRSNA_PRATIPAT))
                {
                    if (GCDisplaySettings.getValue(GCDS.CATURMASYA_PRATIPAT) != 0)
                    {
                        tmp = string.Format("{0} [PRATIPAT SYSTEM]", GCStrings.getString(116));
                        t.AddEvent(DisplayPriorities.PRIO_CM_DAY, GCDS.CATURMASYA_PRATIPAT, tmp);
                        t.AddEvent(DisplayPriorities.PRIO_CM_DAYNOTE, GCDS.CATURMASYA_PRATIPAT, GCStrings.getString(118));
                        t.nCaturmasya = CaturmasyaCodes.CMASYA_SYSTEM_PRATIPAT | CaturmasyaCodes.CMASYA_DAY_FIRST | CaturmasyaCodes.CMASYA_MONTH_2;
                        tmp = string.Format("{0} [PRATIPAT SYSTEM]", GCStrings.getString(113));
                        s.AddEvent(DisplayPriorities.PRIO_CM_DAY, GCDS.CATURMASYA_PRATIPAT, tmp);
                        s.nCaturmasya = CaturmasyaCodes.CMASYA_SYSTEM_PRATIPAT | CaturmasyaCodes.CMASYA_DAY_LAST | CaturmasyaCodes.CMASYA_MONTH_1;
                    }
                }

                // first day of particular month for PURNIMA system, when purnima is not KSAYA
                if (GCTithi.TITHI_TRANSIT(t.astrodata.nTithi, u.astrodata.nTithi, TithiId.TITHI_GAURA_CATURDASI, TithiId.TITHI_PURNIMA))
                {
                    if (GCDisplaySettings.getValue(GCDS.CATURMASYA_PURNIMA) != 0)
                    {
                        tmp = string.Format("{0} [PURNIMA SYSTEM]", GCStrings.getString(120));
                        u.AddEvent(DisplayPriorities.PRIO_CM_DAY, GCDS.CATURMASYA_PURNIMA, tmp);
                        u.AddEvent(DisplayPriorities.PRIO_CM_DAYNOTE, GCDS.CATURMASYA_PURNIMA, GCStrings.getString(122));
                        u.nCaturmasya = CaturmasyaCodes.CMASYA_SYSTEM_PURNIMA | CaturmasyaCodes.CMASYA_DAY_FIRST | CaturmasyaCodes.CMASYA_MONTH_3;
                        tmp = string.Format("{0} [PURNIMA SYSTEM]", GCStrings.getString(117));
                        t.AddEvent(DisplayPriorities.PRIO_CM_DAY, GCDS.CATURMASYA_PURNIMA, tmp);
                        t.nCaturmasya = CaturmasyaCodes.CMASYA_SYSTEM_PURNIMA | CaturmasyaCodes.CMASYA_DAY_LAST | CaturmasyaCodes.CMASYA_MONTH_2;
                    }
                }
                // ekadasi system
                if ((t.astrodata.nPaksa == PaksaId.GAURA_PAKSA) && (t.nMhdType != MahadvadasiType.EV_NULL))
                //if (TITHI_TRANSIT(s.astrodata.nTithi, t.astrodata.nTithi, TITHI_GAURA_DASAMI, TITHI_GAURA_EKADASI))
                {
                    if (GCDisplaySettings.getValue(GCDS.CATURMASYA_EKADASI) != 0)
                    {
                        tmp = string.Format("{0} [EKADASI SYSTEM]", GCStrings.getString(120));
                        t.AddEvent(DisplayPriorities.PRIO_CM_DAY, GCDS.CATURMASYA_EKADASI, tmp);
                        t.AddEvent(DisplayPriorities.PRIO_CM_DAYNOTE, GCDS.CATURMASYA_EKADASI, GCStrings.getString(122));
                        t.nCaturmasya = CaturmasyaCodes.CMASYA_SYSTEM_EKADASI | CaturmasyaCodes.CMASYA_DAY_FIRST | CaturmasyaCodes.CMASYA_MONTH_3;
                        tmp = string.Format("{0} [EKADASI SYSTEM]", GCStrings.getString(117));
                        s.AddEvent(DisplayPriorities.PRIO_CM_DAY, GCDS.CATURMASYA_EKADASI, tmp);
                        s.nCaturmasya = CaturmasyaCodes.CMASYA_SYSTEM_EKADASI | CaturmasyaCodes.CMASYA_DAY_LAST | CaturmasyaCodes.CMASYA_MONTH_2;
                    }
                }
            }

            // third month for pratipat
            // month transit for purnima and ekadasi systems
            if (t.astrodata.nMasa == MasaId.PADMANABHA_MASA)
            {
                if (s.astrodata.nMasa == MasaId.ADHIKA_MASA)
                {
                    t.AddEvent(DisplayPriorities.PRIO_CM_DAYNOTE, GCDS.DISP_ALWAYS, GCStrings.getString(123));
                }
                // pratipat system
                if (GCTithi.TITHI_TRANSIT(s.astrodata.nTithi, t.astrodata.nTithi, TithiId.TITHI_PURNIMA, TithiId.TITHI_KRSNA_PRATIPAT))
                //		if (s.astrodata.nMasa == HRSIKESA_MASA)
                {
                    if (GCDisplaySettings.getValue(GCDS.CATURMASYA_PRATIPAT) != 0)
                    {
                        tmp = string.Format("{0} [PRATIPAT SYSTEM]", GCStrings.getString(120));
                        t.AddEvent(DisplayPriorities.PRIO_CM_DAY, GCDS.CATURMASYA_PRATIPAT, tmp);
                        t.AddEvent(DisplayPriorities.PRIO_CM_DAYNOTE, GCDS.CATURMASYA_PRATIPAT, GCStrings.getString(122));
                        t.nCaturmasya = CaturmasyaCodes.CMASYA_SYSTEM_PRATIPAT | CaturmasyaCodes.CMASYA_DAY_FIRST | CaturmasyaCodes.CMASYA_MONTH_3;
                        tmp = string.Format("{0} [PRATIPAT SYSTEM]", GCStrings.getString(117));
                        s.AddEvent(DisplayPriorities.PRIO_CM_DAY, GCDS.CATURMASYA_PRATIPAT, tmp);
                        s.nCaturmasya = CaturmasyaCodes.CMASYA_SYSTEM_PRATIPAT | CaturmasyaCodes.CMASYA_DAY_LAST | CaturmasyaCodes.CMASYA_MONTH_2;
                    }
                }

                // first day of particular month for PURNIMA system, when purnima is not KSAYA
                if (GCTithi.TITHI_TRANSIT(t.astrodata.nTithi, u.astrodata.nTithi, TithiId.TITHI_GAURA_CATURDASI, TithiId.TITHI_PURNIMA))
                {
                    if (GCDisplaySettings.getValue(GCDS.CATURMASYA_PURNIMA) != 0)
                    {
                        tmp = string.Format("{0} [PURNIMA SYSTEM]", GCStrings.getString(124));
                        u.AddEvent(DisplayPriorities.PRIO_CM_DAY, GCDS.CATURMASYA_PURNIMA, tmp);
                        u.AddEvent(DisplayPriorities.PRIO_CM_DAYNOTE, GCDS.CATURMASYA_PURNIMA, GCStrings.getString(126));
                        u.nCaturmasya = CaturmasyaCodes.CMASYA_SYSTEM_PURNIMA | CaturmasyaCodes.CMASYA_DAY_FIRST | CaturmasyaCodes.CMASYA_MONTH_4;
                        tmp = string.Format("{0} [PURNIMA SYSTEM]", GCStrings.getString(121));
                        t.AddEvent(DisplayPriorities.PRIO_CM_DAY, GCDS.CATURMASYA_PURNIMA, tmp);
                        t.nCaturmasya = CaturmasyaCodes.CMASYA_SYSTEM_PURNIMA | CaturmasyaCodes.CMASYA_DAY_LAST | CaturmasyaCodes.CMASYA_MONTH_3;
                    }
                }

                // ekadasi system
                if ((t.astrodata.nPaksa == PaksaId.GAURA_PAKSA) && (t.nMhdType != MahadvadasiType.EV_NULL))
                //if (TITHI_TRANSIT(s.astrodata.nTithi, t.astrodata.nTithi, TITHI_GAURA_DASAMI, TITHI_GAURA_EKADASI))
                {
                    if (GCDisplaySettings.getValue(GCDS.CATURMASYA_EKADASI) != 0)
                    {
                        tmp = string.Format("{0} [EKADASI SYSTEM]", GCStrings.getString(124));
                        t.AddEvent(DisplayPriorities.PRIO_CM_DAY, GCDS.CATURMASYA_EKADASI, tmp);
                        t.AddEvent(DisplayPriorities.PRIO_CM_DAYNOTE, GCDS.CATURMASYA_EKADASI, GCStrings.getString(126));
                        t.nCaturmasya = CaturmasyaCodes.CMASYA_SYSTEM_EKADASI | CaturmasyaCodes.CMASYA_DAY_FIRST | CaturmasyaCodes.CMASYA_MONTH_4;
                        tmp = string.Format("{0} [EKADASI SYSTEM]", GCStrings.getString(121));
                        s.AddEvent(DisplayPriorities.PRIO_CM_DAY, GCDS.CATURMASYA_EKADASI, tmp);
                        s.nCaturmasya = CaturmasyaCodes.CMASYA_SYSTEM_EKADASI | CaturmasyaCodes.CMASYA_DAY_LAST | CaturmasyaCodes.CMASYA_MONTH_3;
                    }
                }
            }

            // fourth month for pratipat system
            // month transit for purnima and ekadasi systems
            if (t.astrodata.nMasa == MasaId.DAMODARA_MASA)
            {
                if (s.astrodata.nMasa == MasaId.ADHIKA_MASA)
                {
                    t.AddEvent(DisplayPriorities.PRIO_CM_DAYNOTE, GCDS.DISP_ALWAYS, GCStrings.getString(127));
                }
                // pratipat system
                if (GCTithi.TITHI_TRANSIT(s.astrodata.nTithi, t.astrodata.nTithi, TithiId.TITHI_PURNIMA, TithiId.TITHI_KRSNA_PRATIPAT))
                {
                    if (GCDisplaySettings.getValue(GCDS.CATURMASYA_PRATIPAT) != 0)
                    {
                        tmp = string.Format("{0} [PRATIPAT SYSTEM]", GCStrings.getString(124));
                        t.AddEvent(DisplayPriorities.PRIO_CM_DAY, GCDS.CATURMASYA_PRATIPAT, tmp);
                        t.AddEvent(DisplayPriorities.PRIO_CM_DAYNOTE, GCDS.CATURMASYA_PRATIPAT, GCStrings.getString(126));
                        t.nCaturmasya = CaturmasyaCodes.CMASYA_SYSTEM_PRATIPAT | CaturmasyaCodes.CMASYA_DAY_FIRST | CaturmasyaCodes.CMASYA_MONTH_4;
                        tmp = string.Format("{0} [PRATIPAT SYSTEM]", GCStrings.getString(121));
                        s.AddEvent(DisplayPriorities.PRIO_CM_DAY, GCDS.CATURMASYA_PRATIPAT, tmp);
                        s.nCaturmasya = CaturmasyaCodes.CMASYA_SYSTEM_PRATIPAT | CaturmasyaCodes.CMASYA_DAY_LAST | CaturmasyaCodes.CMASYA_MONTH_3;
                    }
                }

                // last day for punima system
                if (GCTithi.TITHI_TRANSIT(t.astrodata.nTithi, u.astrodata.nTithi, TithiId.TITHI_GAURA_CATURDASI, TithiId.TITHI_PURNIMA))
                {
                    if (GCDisplaySettings.getValue(GCDS.CATURMASYA_PURNIMA) != 0)
                    {
                        tmp = string.Format("{0} [PURNIMA SYSTEM]", GCStrings.getString(125));
                        t.AddEvent(DisplayPriorities.PRIO_CM_DAY, GCDS.CATURMASYA_PURNIMA, tmp);
                        t.nCaturmasya = CaturmasyaCodes.CMASYA_SYSTEM_PURNIMA | CaturmasyaCodes.CMASYA_DAY_LAST | CaturmasyaCodes.CMASYA_MONTH_4;
                    }
                }

                // ekadasi system
                //if (TITHI_TRANSIT(t.astrodata.nTithi, u.astrodata.nTithi, TITHI_GAURA_DASAMI, TITHI_GAURA_EKADASI))
                if ((t.astrodata.nPaksa == PaksaId.GAURA_PAKSA) && (t.nMhdType != MahadvadasiType.EV_NULL))
                {
                    if (GCDisplaySettings.getValue(GCDS.CATURMASYA_EKADASI) != 0)
                    {
                        tmp = string.Format("{0} [EKADASI SYSTEM]", GCStrings.getString(125));
                        s.AddEvent(DisplayPriorities.PRIO_CM_DAY, GCDS.CATURMASYA_EKADASI, tmp);
                        s.nCaturmasya = CaturmasyaCodes.CMASYA_SYSTEM_EKADASI | CaturmasyaCodes.CMASYA_DAY_LAST | CaturmasyaCodes.CMASYA_MONTH_4;
                    }
                }

                if (GCTithi.TITHI_TRANSIT(t.astrodata.nTithi, u.astrodata.nTithi, TithiId.TITHI_PURNIMA, TithiId.TITHI_KRSNA_PRATIPAT))
                {
                    if (GCDisplaySettings.getValue(GCDS.CATURMASYA_PRATIPAT) != 0)
                    {
                        tmp = string.Format("{0} [PRATIPAT SYSTEM]", GCStrings.getString(125));
                        t.AddEvent(DisplayPriorities.PRIO_CM_DAY, GCDS.CATURMASYA_PRATIPAT, tmp);
                        t.nCaturmasya = CaturmasyaCodes.CMASYA_SYSTEM_PRATIPAT | CaturmasyaCodes.CMASYA_DAY_LAST | CaturmasyaCodes.CMASYA_MONTH_4;
                    }

                    // on last day of Caturmasya pratipat system is Bhisma Pancaka ending
                    //if (t.festivals.GetLength() > 0)
                    //	t.festivals += "#";
                    //t.festivals += GCStrings.getString(82);
                    t.AddEvent(DisplayPriorities.PRIO_CM_DAYNOTE, GCDS.DISP_ALWAYS, GCStrings.getString(82));
                }
            }

            return 1;
        }

        public int MahadvadasiCalc(int nIndex, GCEarthData earth)
        {
            int nMahaType = MahadvadasiType.EV_NULL;
            int nMhdDay = -1;

            VAISNAVADAY s = m_pData[nIndex - 1];
            VAISNAVADAY t = m_pData[nIndex];
            VAISNAVADAY u = m_pData[nIndex + 1];

            // if yesterday is dvadasi
            // then we skip this day
            if (GCTithi.TITHI_DVADASI(s.astrodata.nTithi))
                return 1;

            if (TithiId.TITHI_GAURA_DVADASI == t.astrodata.nTithi && TithiId.TITHI_GAURA_DVADASI == t.astrodata.nTithiSunset && IsMhd58(nIndex, out nMahaType))
            {
                t.nMhdType = nMahaType;
                nMhdDay = nIndex;
            }
            else if (GCTithi.TITHI_DVADASI(t.astrodata.nTithi))
            {
                if (GCTithi.TITHI_DVADASI(u.astrodata.nTithi) && GCTithi.TITHI_EKADASI(s.astrodata.nTithi) && GCTithi.TITHI_EKADASI(s.astrodata.nTithiArunodaya))
                {
                    t.nMhdType = MahadvadasiType.EV_VYANJULI;
                    nMhdDay = nIndex;
                }
                else if (NextNewFullIsVriddhi(nIndex, earth))
                {
                    t.nMhdType = MahadvadasiType.EV_PAKSAVARDHINI;
                    nMhdDay = nIndex;
                }
                else if (GCTithi.TITHI_LESS_EKADASI(s.astrodata.nTithiArunodaya))
                {
                    t.nMhdType = MahadvadasiType.EV_SUDDHA;
                    nMhdDay = nIndex;
                }
            }

            if (nMhdDay >= 0)
            {
                // fasting day
                m_pData[nMhdDay].nFastType = FastType.FAST_EKADASI;
                m_pData[nMhdDay].ekadasi_vrata_name = GCEkadasi.GetEkadasiName(t.astrodata.nMasa, t.astrodata.nPaksa);
                m_pData[nMhdDay].ekadasi_parana = false;
                m_pData[nMhdDay].eparana_time1 = 0.0;
                m_pData[nMhdDay].eparana_time2 = 0.0;

                // parana day
                m_pData[nMhdDay + 1].nFastType = FastType.FAST_NULL;
                m_pData[nMhdDay + 1].ekadasi_parana = true;
                m_pData[nMhdDay + 1].eparana_time1 = 0.0;
                m_pData[nMhdDay + 1].eparana_time2 = 0.0;
            }

            return 1;
        }

        public VAISNAVADAY GetDay(int nIndex)
        {
            int nReturn = nIndex + BEFORE_DAYS;

            if (nReturn >= m_nCount)
                return null;

            return m_pData[nReturn];
        }

        public int ExtendedCalc(int nIndex, GCEarthData earth)
        {
            VAISNAVADAY s = m_pData[nIndex - 1];
            VAISNAVADAY t = m_pData[nIndex];
            VAISNAVADAY u = m_pData[nIndex + 1];
            VAISNAVADAY v = m_pData[nIndex + 2];

            // test for Rama Navami
            if ((t.astrodata.nMasa == MasaId.VISNU_MASA) && (t.astrodata.nPaksa == PaksaId.GAURA_PAKSA))
            {
                if (IsFestivalDay(s, t, TithiId.TITHI_GAURA_NAVAMI))
                {
                    if (u.nFastType >= FastType.FAST_EKADASI)
                    {
                        // yesterday was Rama Navami
                        s.AddSpecFestival(SpecialFestivalId.SPEC_RAMANAVAMI, GCDS.CAL_FEST_0);
                        //s.nFastType = FAST_SUNSET;
                    }
                    else
                    {
                        // today is Rama Navami
                        t.AddSpecFestival(SpecialFestivalId.SPEC_RAMANAVAMI, GCDS.CAL_FEST_0);
                        //t.nFastType = FAST_SUNSET;
                    }
                }
            }

            return 1;
        }

        /******************************************************************************************/
        /*                                                                                        */
        /*  TEST if today is given festival tithi                                                 */
        /*                                                                                        */
        /*  if today is given tithi and yesterday is not this tithi                               */
        /*  then it is festival day (it is first day of this tithi, when vriddhi)                 */
        /*                                                                                        */
        /*  if yesterday is previous tithi to the given one and today is next to the given one    */
        /*  then today is day after ksaya tithi which is given                                    */
        /*                                                                                        */
        /*                                                                                        */
        /******************************************************************************************/

        public bool IsFestivalDay(VAISNAVADAY yesterday, VAISNAVADAY today, int nTithi)
        {
            return ((today.astrodata.nTithi == nTithi) && GCTithi.TITHI_LESS_THAN(yesterday.astrodata.nTithi, nTithi))
                    || (GCTithi.TITHI_LESS_THAN(yesterday.astrodata.nTithi, nTithi) && GCTithi.TITHI_GREAT_THAN(today.astrodata.nTithi, nTithi));

        }

        public int FindDate(GregorianDateTime vc)
        {
            int i;
            for (i = BEFORE_DAYS; i < m_nCount; i++)
            {
                if ((m_pData[i].date.day == vc.day) && (m_pData[i].date.month == vc.month) && (m_pData[i].date.year == vc.year))
                    return (i - BEFORE_DAYS);
            }

            return -1;
        }

        /*
         * Function before is writen accoring this algorithms:


1. Normal - fasting day has ekadasi at sunrise and dvadasi at next sunrise.

2. Viddha - fasting day has dvadasi at sunrise and trayodasi at next
sunrise, and it is not a naksatra mahadvadasi

3. Unmilani - fasting day has ekadasi at both sunrises

4. Vyanjuli - fasting day has dvadasi at both sunrises, and it is not a
naksatra mahadvadasi

5. Trisprsa - fasting day has ekadasi at sunrise and trayodasi at next
sunrise.

6. Jayanti/Vijaya - fasting day has gaura dvadasi and specified naksatra at
sunrise and same naksatra at next sunrise

7. Jaya/Papanasini - fasting day has gaura dvadasi and specified naksatra at
sunrise and same naksatra at next sunrise

==============================================
Case 1 Normal (no change)

If dvadasi tithi ends before 1/3 of daylight
   then PARANA END = TIME OF END OF TITHI
but if dvadasi TITHI ends after 1/3 of daylight
   then PARANA END = TIME OF 1/3 OF DAYLIGHT

if 1/4 of dvadasi tithi is before sunrise
   then PARANA BEGIN is sunrise time
but if 1/4 of dvadasi tithi is after sunrise
   then PARANA BEGIN is time of 1/4 of dvadasi tithi

if PARANA BEGIN is before PARANA END
   then we will write "BREAK FAST FROM xx TO yy
but if PARANA BEGIN is after PARANA END
   then we will write "BREAK FAST AFTER xx"

==============================================
Case 2 Viddha

If trayodasi tithi ends before 1/3 of daylight
   then PARANA END = TIME OF END OF TITHI
but if trayodasi TITHI ends after 1/3 of daylight
   then PARANA END = TIME OF 1/3 OF DAYLIGHT

PARANA BEGIN is sunrise time

we will write "BREAK FAST FROM xx TO yy

==============================================
Case 3 Unmilani

PARANA END = TIME OF 1/3 OF DAYLIGHT

PARANA BEGIN is end of Ekadasi tithi

if PARANA BEGIN is before PARANA END
   then we will write "BREAK FAST FROM xx TO yy
but if PARANA BEGIN is after PARANA END
   then we will write "BREAK FAST AFTER xx"

==============================================
Case 4 Vyanjuli

PARANA BEGIN = Sunrise

PARANA END is end of Dvadasi tithi

we will write "BREAK FAST FROM xx TO yy

==============================================
Case 5 Trisprsa

PARANA BEGIN = Sunrise

PARANA END = 1/3 of daylight hours

we will write "BREAK FAST FROM xx TO yy

==============================================
Case 6 Jayanti/Vijaya

PARANA BEGIN = Sunrise

PARANA END1 = end of dvadasi tithi or sunrise, whichever is later
PARANA END2 = end of naksatra

PARANA END is earlier of END1 and END2

we will write "BREAK FAST FROM xx TO yy

==============================================
Case 7 Jaya/Papanasini

PARANA BEGIN = end of naksatra

PARANA END = 1/3 of Daylight hours

if PARANA BEGIN is before PARANA END
   then we will write "BREAK FAST FROM xx TO yy
but if PARANA BEGIN is after PARANA END
   then we will write "BREAK FAST AFTER xx"


         * */
        public int CalculateEParana(VAISNAVADAY s, VAISNAVADAY t, out double begin, out double end, GCEarthData earth)
        {
            t.nMhdType = MahadvadasiType.EV_NULL;
            t.ekadasi_parana = true;
            t.nFastType = FastType.FAST_NULL;

            double titBeg, titEnd, tithi_quart;
            double sunRise, third_day, naksEnd;
            double parBeg = -1.0, parEnd = -1.0;
            double tithi_len;
            GregorianDateTime snd, nend;

            sunRise = t.astrodata.sun.sunrise_deg / 360.0 + earth.offsetUtcHours / 24.0;
            third_day = sunRise + t.astrodata.sun.length_deg / 1080.0;
            tithi_len = GCTithi.GetTithiTimes(earth, t.date, out titBeg, out titEnd, sunRise);
            tithi_quart = tithi_len / 4.0 + titBeg;

            switch (s.nMhdType)
            {
                case MahadvadasiType.EV_UNMILANI:
                    parEnd = titEnd;
                    t.eparana_type2 = EkadasiParanaType.EP_TYPE_TEND;
                    if (parEnd > third_day)
                    {
                        parEnd = third_day;
                        t.eparana_type2 = EkadasiParanaType.EP_TYPE_3DAY;
                    }
                    parBeg = sunRise;
                    t.eparana_type1 = EkadasiParanaType.EP_TYPE_SUNRISE;
                    break;
                case MahadvadasiType.EV_VYANJULI:
                    parBeg = sunRise;
                    t.eparana_type1 = EkadasiParanaType.EP_TYPE_SUNRISE;
                    parEnd = GCMath.Min(titEnd, third_day);
                    if (parEnd == titEnd)
                        t.eparana_type2 = EkadasiParanaType.EP_TYPE_TEND;
                    else
                        t.eparana_type2 = EkadasiParanaType.EP_TYPE_3DAY;
                    break;
                case MahadvadasiType.EV_TRISPRSA:
                    parBeg = sunRise;
                    parEnd = third_day;
                    t.eparana_type1 = EkadasiParanaType.EP_TYPE_SUNRISE;
                    t.eparana_type2 = EkadasiParanaType.EP_TYPE_3DAY;
                    break;
                case MahadvadasiType.EV_JAYANTI:
                case MahadvadasiType.EV_VIJAYA:

                    naksEnd = GCNaksatra.GetEndHour(earth, s.date, t.date); //GetNextNaksatra(earth, snd, nend);
                    if (GCTithi.TITHI_DVADASI(t.astrodata.nTithi))
                    {
                        if (naksEnd < titEnd)
                        {
                            if (naksEnd < third_day)
                            {
                                parBeg = naksEnd;
                                t.eparana_type1 = EkadasiParanaType.EP_TYPE_NAKEND;
                                parEnd = GCMath.Min(titEnd, third_day);
                                if (parEnd == titEnd)
                                    t.eparana_type2 = EkadasiParanaType.EP_TYPE_TEND;
                                else
                                    t.eparana_type2 = EkadasiParanaType.EP_TYPE_3DAY;
                            }
                            else
                            {
                                parBeg = naksEnd;
                                t.eparana_type1 = EkadasiParanaType.EP_TYPE_NAKEND;
                                parEnd = titEnd;
                                t.eparana_type2 = EkadasiParanaType.EP_TYPE_TEND;
                            }
                        }
                        else
                        {
                            parBeg = sunRise;
                            t.eparana_type1 = EkadasiParanaType.EP_TYPE_SUNRISE;
                            parEnd = GCMath.Min(titEnd, third_day);
                            if (parEnd == titEnd)
                                t.eparana_type2 = EkadasiParanaType.EP_TYPE_TEND;
                            else
                                t.eparana_type2 = EkadasiParanaType.EP_TYPE_3DAY;
                        }
                    }
                    else
                    {
                        parBeg = sunRise;
                        t.eparana_type1 = EkadasiParanaType.EP_TYPE_SUNRISE;
                        parEnd = GCMath.Min(naksEnd, third_day);
                        if (parEnd == naksEnd)
                            t.eparana_type2 = EkadasiParanaType.EP_TYPE_NAKEND;
                        else
                            t.eparana_type2 = EkadasiParanaType.EP_TYPE_3DAY;
                    }

                    break;
                case MahadvadasiType.EV_JAYA:
                case MahadvadasiType.EV_PAPA_NASINI:

                    naksEnd = GCNaksatra.GetEndHour(earth, s.date, t.date); //GetNextNaksatra(earth, snd, nend);

                    if (GCTithi.TITHI_DVADASI(t.astrodata.nTithi))
                    {
                        if (naksEnd < titEnd)
                        {
                            if (naksEnd < third_day)
                            {
                                parBeg = naksEnd;
                                t.eparana_type1 = EkadasiParanaType.EP_TYPE_NAKEND;
                                parEnd = GCMath.Min(titEnd, third_day);
                                if (parEnd == titEnd)
                                    t.eparana_type2 = EkadasiParanaType.EP_TYPE_TEND;
                                else
                                    t.eparana_type2 = EkadasiParanaType.EP_TYPE_3DAY;
                            }
                            else
                            {
                                parBeg = naksEnd;
                                t.eparana_type1 = EkadasiParanaType.EP_TYPE_NAKEND;
                                parEnd = titEnd;
                                t.eparana_type2 = EkadasiParanaType.EP_TYPE_TEND;
                            }
                        }
                        else
                        {
                            parBeg = sunRise;
                            t.eparana_type1 = EkadasiParanaType.EP_TYPE_SUNRISE;
                            parEnd = GCMath.Min(titEnd, third_day);
                            if (parEnd == titEnd)
                                t.eparana_type2 = EkadasiParanaType.EP_TYPE_TEND;
                            else
                                t.eparana_type2 = EkadasiParanaType.EP_TYPE_3DAY;
                        }
                    }
                    else
                    {
                        if (naksEnd < third_day)
                        {
                            parBeg = naksEnd;
                            t.eparana_type1 = EkadasiParanaType.EP_TYPE_NAKEND;
                            parEnd = third_day;
                            t.eparana_type2 = EkadasiParanaType.EP_TYPE_3DAY;
                        }
                        else
                        {
                            parBeg = naksEnd;
                            t.eparana_type1 = EkadasiParanaType.EP_TYPE_NAKEND;
                            parEnd = -1.0;
                            t.eparana_type2 = EkadasiParanaType.EP_TYPE_NULL;
                        }
                    }

                    break;
                default:
                    // first initial
                    parEnd = GCMath.Min(titEnd, third_day);
                    if (parEnd == titEnd)
                        t.eparana_type2 = EkadasiParanaType.EP_TYPE_TEND;
                    else
                        t.eparana_type2 = EkadasiParanaType.EP_TYPE_3DAY;
                    parBeg = GCMath.Max(sunRise, tithi_quart);
                    if (parBeg == sunRise)
                        t.eparana_type1 = EkadasiParanaType.EP_TYPE_SUNRISE;
                    else
                        t.eparana_type1 = EkadasiParanaType.EP_TYPE_4TITHI;

                    if (GCTithi.TITHI_DVADASI(s.astrodata.nTithi))
                    {
                        parBeg = sunRise;
                        t.eparana_type1 = EkadasiParanaType.EP_TYPE_SUNRISE;
                    }

                    //if (parBeg > third_day)
                    if (parBeg > parEnd)
                    {
                        //			parBeg = sunRise;
                        parEnd = -1.0;
                        t.eparana_type2 = EkadasiParanaType.EP_TYPE_NULL;
                    }
                    break;
            }


            begin = parBeg;
            end = parEnd;

            if (begin > 0.0)
                begin *= 24.0;
            if (end > 0.0)
                end *= 24.0;

            t.eparana_time1 = begin;
            t.eparana_time2 = end;

            return 1;
        }









        /******************************************************************************************/
        /*                                                                                        */
        /*                                                                                        */
        /*                                                                                        */
        /******************************************************************************************/


        /******************************************************************************************/
        /*                                                                                        */
        /*                                                                                        */
        /*                                                                                        */
        /******************************************************************************************/



        /******************************************************************************************/
        /*                                                                                        */
        /*                                                                                        */
        /*                                                                                        */
        /******************************************************************************************/




        /* 

          */



        public void ResolveFestivalsFasting(int nIndex)
        {
            VAISNAVADAY s = m_pData[nIndex - 1];
            VAISNAVADAY t = m_pData[nIndex];
            VAISNAVADAY u = m_pData[nIndex + 1];

            int nftype;
            String pers, str, S;
            String subject = string.Empty;
            int fasting = 0;
            string ch;
            VAISNAVAEVENT md;

            if (t.nMhdType != MahadvadasiType.EV_NULL)
            {
                str = string.Format(GCStrings.Localized("Fasting for {0}"), t.ekadasi_vrata_name);
                t.AddEvent(DisplayPriorities.PRIO_EKADASI, GCDS.CAL_EKADASI_PARANA, str);
            }

            ch = GCEkadasi.GetMahadvadasiName((int)t.nMhdType);
            if (ch != null)
            {
                t.AddEvent(DisplayPriorities.PRIO_MAHADVADASI, GCDS.CAL_EKADASI_PARANA, ch);
            }

            if (t.ekadasi_parana)
            {
                str = t.GetTextEP();
                t.AddEvent(DisplayPriorities.PRIO_EKADASI_PARANA, GCDS.CAL_EKADASI_PARANA, str);
            }

            for (int h = 0; h < t.dayEvents.Count(); h++)
            {
                md = t.dayEvents[h];
                nftype = 0;
                if (md.fasttype != FastType.FAST_NULL)
                {
                    nftype = md.fasttype;
                    subject = md.fastsubject;
                }

                if (nftype != 0)
                {
                    if (s.nFastType == FastType.FAST_EKADASI)
                    {
                        if (GCDisplaySettings.getValue(42) == 0)
                        {
                            str = string.Format(GCStrings.Localized("(Fast today for {0})"), subject);
                            s.AddEvent(DisplayPriorities.PRIO_FASTING, GCDS.DISP_ALWAYS, str);
                            t.AddEvent(md.prio + 1, md.dispItem, GCStrings.Localized("(Fasting is done yesterday)"));
                            //"(Fasting is done yesterday)"
                        }
                        else
                        {
                            str = string.Format(GCStrings.Localized("(Fast till noon for {0}, with feast tomorrow)"), subject);
                            s.AddEvent(DisplayPriorities.PRIO_FASTING, GCDS.DISP_ALWAYS, str);
                            t.AddEvent(md.prio + 1, md.dispItem, GCStrings.Localized("(Fasting is done yesterday, today is feast)"));
                            //"(Fasting is done yesterday, today is feast)";
                        }
                    }
                    else if (t.nFastType == FastType.FAST_EKADASI)
                    {
                        if (GCDisplaySettings.getValue(42) != 0)
                            t.AddEvent(md.prio + 1, md.dispItem, GCStrings.Localized("(Fasting till noon, with feast tomorrow)"));
                        //"(Fasting till noon, with feast tomorrow)";
                        else
                            t.AddEvent(md.prio + 1, md.dispItem, GCStrings.Localized("(Fast today)"));
                        //"(Fast today)"
                    }
                    else
                    {
                        if (GCDisplaySettings.getValue(42) == 0)
                        {
                            if (nftype > 1)
                                nftype = 7;
                            else nftype = 0;
                        }
                        if (nftype != 0)
                        {
                            t.AddEvent(md.prio + 1, md.dispItem,
                                GCStrings.GetFastingName(0x200 + nftype));
                        }
                    }
                }
                if (fasting < nftype)
                    fasting = nftype;
            }

            if (fasting != FastType.FAST_NULL)
            {
                if (s.nFastType == FastType.FAST_EKADASI)
                {
                    t.nFeasting = FeastType.FEAST_TODAY_FAST_YESTERDAY;
                    s.nFeasting = FeastType.FEAST_TOMMOROW_FAST_TODAY;
                }
                else if (t.nFastType == FastType.FAST_EKADASI)
                {
                    u.nFeasting = FeastType.FEAST_TODAY_FAST_YESTERDAY;
                    t.nFeasting = FeastType.FEAST_TOMMOROW_FAST_TODAY;
                }
                else
                {
                    t.nFastType = (0x200 + fasting);
                }
            }

        }

        public int writeXml(StringBuilder xml)
        {
            int k;
            String str, st;
            VAISNAVADAY pvd;
            int nPrevMasa = -1;
            TResultCalendar daybuff = this;


            xml.Append("<xml>\n");
            xml.Append("\t<request name=\"Calendar\" version=\"");
            xml.Append(GCStrings.getString(130));
            xml.Append("\">\n");
            xml.Append("\t\t<arg name=\"longitude\" val=\"");
            xml.Append(daybuff.m_Location.longitudeDeg);
            xml.Append("\" />\n");
            xml.Append("\t\t<arg name=\"latitude\" val=\"");
            xml.Append(daybuff.m_Location.latitudeDeg);
            xml.Append("\" />\n");
            xml.Append("\t\t<arg name=\"timezone\" val=\"");
            xml.Append(daybuff.m_Location.offsetUtcHours);
            xml.Append("\" />\n");
            xml.Append("\t\t<arg name=\"startdate\" val=\"");
            xml.Append(daybuff.m_vcStart);
            xml.Append("\" />\n");
            xml.Append("\t\t<arg name=\"daycount\" val=\"");
            xml.Append(daybuff.m_vcCount);
            xml.Append("\" />\n");
            xml.Append("\t\t<arg name=\"dst\" val=\"");
            xml.Append(daybuff.m_Location.timezoneId);
            xml.Append("\" />\n");
            xml.Append("\t</request>\n");
            xml.Append("\t<result name=\"Calendar\">\n");
            if (daybuff.m_Location.timezoneId > 0)
                xml.Append("\t<dstsystem name=\"");
            xml.Append(TTimeZone.GetTimeZoneName(daybuff.m_Location.timezoneId));
            xml.Append("\" />\n");

            for (k = 0; k < daybuff.m_vcCount; k++)
            {
                pvd = daybuff.GetDay(k);
                if (pvd != null)
                {
                    if (nPrevMasa != pvd.astrodata.nMasa)
                    {
                        if (nPrevMasa != -1)
                            xml.Append("\t</masa>\n");
                        xml.Append("\t<masa name=\"");
                        xml.Append(GCMasa.GetName(pvd.astrodata.nMasa));
                        xml.Append(" Masa");
                        if (nPrevMasa == MasaId.ADHIKA_MASA)
                            xml.Append(" ");
                        xml.Append(GCStrings.Localized("(Second half)"));
                        xml.Append("\"");
                        xml.Append(" gyear=\"Gaurabda ");
                        xml.Append(pvd.astrodata.nGaurabdaYear);
                        xml.Append("\"");
                        xml.Append(">\n");
                    }

                    nPrevMasa = pvd.astrodata.nMasa;

                    // date data
                    xml.Append("\t<day date=\"");
                    xml.Append(pvd.date);
                    xml.Append("\" dayweekid=\"");
                    xml.Append(pvd.date.dayOfWeek);
                    xml.Append("\" dayweek=\"");
                    st = GCStrings.getString(pvd.date.dayOfWeek).Substring(0, 2);
                    xml.Append(st);
                    xml.Append("\">\n");

                    // sunrise data
                    xml.Append("\t\t<sunrise time=\"");
                    xml.Append(pvd.astrodata.sun.rise);
                    xml.Append("\">\n");

                    xml.Append("\t\t\t<tithi name=\"");
                    xml.Append(pvd.GetFullTithiName());
                    str = string.Format("\" elapse=\"{0}\" index=\"{1}\"/>\n"
                        , pvd.astrodata.nTithiElapse, pvd.astrodata.nTithi % 30 + 1);
                    xml.Append(str);

                    str = string.Format("\t\t\t<naksatra name=\"{0}\" elapse=\"{1}\" />\n"
                        , GCNaksatra.GetName(pvd.astrodata.nNaksatra), pvd.astrodata.nNaksatraElapse);
                    xml.Append(str);

                    str = string.Format("\t\t\t<yoga name=\"{0}\"/>\n", GCYoga.GetName(pvd.astrodata.nYoga));
                    xml.Append(str);

                    str = string.Format("\t\t\t<paksa id=\"{0}\" name=\"{1}\"/>\n", GCPaksa.GetAbbr(pvd.astrodata.nPaksa), GCPaksa.GetName(pvd.astrodata.nPaksa));
                    xml.Append(str);

                    xml.Append("\t\t</sunrise>\n");

                    xml.Append("\t\t<dst offset=\"");
                    xml.Append(pvd.nDST);
                    xml.Append("\" />\n");
                    // arunodaya data
                    xml.Append("\t\t<arunodaya time=\"");
                    xml.Append(pvd.astrodata.sun.arunodaya);
                    xml.Append("\">\n");
                    xml.Append("\t\t\t<tithi name=\"");
                    xml.Append(GCTithi.GetName(pvd.astrodata.nTithiArunodaya));
                    xml.Append("\" />\n");
                    xml.Append("\t\t</arunodaya>\n");

                    xml.Append("\t\t<noon time=\"");
                    xml.Append(pvd.astrodata.sun.noon);
                    xml.Append("\" />\n");

                    xml.Append("\t\t<sunset time=\"");
                    xml.Append(pvd.astrodata.sun.set);
                    xml.Append("\" />\n");

                    // moon data
                    xml.Append("\t\t<moon rise=\"");
                    xml.Append(pvd.moonrise);
                    xml.Append("\" set=\"");
                    xml.Append(pvd.moonset);
                    xml.Append("\" />\n");

                    if (pvd.ekadasi_parana)
                    {
                        int h1, m1, h2, m2;
                        GCMath.DaytimeToHourMin(pvd.eparana_time1, out h1, out m1);
                        if (pvd.eparana_time2 >= 0.0)
                        {
                            GCMath.DaytimeToHourMin(pvd.eparana_time2, out h2, out m2);
                            xml.AppendFormat("\t\t<parana from=\"{0:00}:{1:00}\" to=\"{2:00}:{3:00}\" />\n", h1, m1, h2, m2);
                        }
                        else
                        {
                            xml.AppendFormat("\t\t<parana after=\"{0:00}:{1:00}\" />\n", h1, m1);
                        }
                    }
                    str = "";

                    for (int h = 0; h < pvd.dayEvents.Count(); h++)
                    {
                        VAISNAVAEVENT md = pvd.dayEvents[h];
                        int prio = md.prio;
                        if (prio >= DisplayPriorities.PRIO_FESTIVALS_0 && prio <= DisplayPriorities.PRIO_FESTIVALS_6)
                        {
                            xml.AppendFormat("\t\t<festival name=\"{0}\" class=\"{1}\"/>\n", md.text, md.dispItem - GCDS.CAL_FEST_0);
                        }
                    }

                    if (pvd.nFastType != FastType.FAST_NULL)
                    {
                        xml.Append("\t\t<fast type=\"\" mark=\"");
                        if (pvd.nFastType == FastType.FAST_EKADASI)
                        {
                            xml.Append("*");
                        }
                        xml.Append("\" />\n");
                    }

                    if (pvd.sankranti_zodiac >= 0)
                    {
                        //double h1, m1, s1;
                        //m1 = modf(pvd.sankranti_day.shour*24, &h1);
                        //				s1 = modf(m1*60, &m1);
                        xml.AppendFormat("\t\t<sankranti rasi=\"{0}\" time=\"{1:00}:{2:00}:{3:00}\" />\n"
                            , GCRasi.GetName(pvd.sankranti_zodiac), pvd.sankranti_day.GetHour()
                            , pvd.sankranti_day.GetMinute(), pvd.sankranti_day.GetSecond());
                    }

                    if (pvd.nCaturmasya != 0)
                    {
                        xml.Append("\t\t<caturmasya day=\"");
                        xml.Append((pvd.nCaturmasya & CaturmasyaCodes.CMASYA_DAY_MASK) == CaturmasyaCodes.CMASYA_DAY_FIRST ? "last" : "first");
                        xml.Append("\" month=\"");
                        xml.Append((int)(pvd.nCaturmasya & CaturmasyaCodes.CMASYA_MONTH_MASK) - CaturmasyaCodes.CMASYA_MONTH_MASK + 1);
                        xml.Append("\" system=\"");
                        if ((pvd.nCaturmasya & CaturmasyaCodes.CMASYA_SYSTEM_MASK) == CaturmasyaCodes.CMASYA_SYSTEM_PURNIMA)
                            xml.Append("PURNIMA");
                        else if ((pvd.nCaturmasya & CaturmasyaCodes.CMASYA_SYSTEM_MASK) == CaturmasyaCodes.CMASYA_SYSTEM_PRATIPAT)
                            xml.Append("PRATIPAT");
                        else
                            xml.Append("EKADASI");
                        xml.Append("\" />\n");
                    }

                    xml.Append("\t</day>\n\n");

                }
            }
            xml.Append("\t</masa>\n");


            xml.Append("</result>\n");
            xml.Append("</xml>\n");

            return 1;
        }

        public int formatPlainText(StringBuilder m_text)
        {
            int k, nMasaHeader;
            String str = string.Empty;
            string dayText;

            VAISNAVADAY pvd, prevd, nextd;
            int lastmasa = -1;
            int lastmonth = -1;
            int tp1;
            double rate;
            string locationText = m_Location.GetFullName();
            string versionText = GCStrings.ShortVersionText;
            bool bCalcMoon = (GCDisplaySettings.getValue(4) > 0 || GCDisplaySettings.getValue(5) > 0);
            TResultCalendar daybuff = this;
            m_text.Clear();
            GCStringBuilder sb = new GCStringBuilder();

            sb.Target = m_text;
            sb.Format = GCStringBuilder.FormatType.PlainText;

            for (k = 0; k < daybuff.m_vcCount; k++)
            {

                prevd = daybuff.GetDay(k - 1);
                pvd = daybuff.GetDay(k);
                nextd = daybuff.GetDay(k + 1);

                if (updateCalculationProgress)
                {
                    if (bCalcMoon)
                    {
                        GCUserInterface.SetProgressWindowPos(Convert.ToInt32(90.8 + 9.2 * k / daybuff.m_vcCount));
                    }
                    else
                    {
                        rate = Convert.ToDouble(k) / daybuff.m_vcCount;
                        GCUserInterface.SetProgressWindowPos(Convert.ToInt32(58.8 + 41.2 * rate * rate));
                    }
                }

                if (pvd != null)
                {
                    nMasaHeader = 0;
                    if ((GCDisplaySettings.getValue(GCDS.CAL_HEADER_MASA) == 0) && (pvd.astrodata.nMasa != lastmasa))
                    {
                        nMasaHeader = 1;
                        m_text.AppendLine();
                        str = pvd.Format(GCStrings.Localized(" {masaName} Masa, Gaurabda {gaurabdaYear} "));
                        //str = string.Format(" {0} {1}, Gaurabda {2} ", GCMasa.GetName(pvd.astrodata.nMasa), GCStrings.getString(22), pvd.astrodata.nGaurabdaYear);
                        tp1 = (80 - str.Length) / 2;
                        tp1 = tp1 < 0 ? 0 : tp1;
                        m_text.Append(string.Empty.PadLeft(tp1));
                        m_text.Append(str);
                        if (tp1 > versionText.Length)
                            m_text.Append(string.Empty.PadLeft(tp1 - versionText.Length));
                        m_text.Append(versionText);
                        m_text.AppendLine();
                        if ((pvd.astrodata.nMasa == MasaId.ADHIKA_MASA) && ((lastmasa >= MasaId.SRIDHARA_MASA) && (lastmasa <= MasaId.DAMODARA_MASA)))
                        {
                            sb.AppendTwoColumnText("", GCStrings.getString(128));
                        }
                        m_text.AppendLine();
                        lastmasa = pvd.astrodata.nMasa;
                    }

                    if ((GCDisplaySettings.getValue(GCDS.CAL_HEADER_MASA) == 1) && (pvd.date.month != lastmonth))
                    {
                        nMasaHeader = 1;
                        m_text.AppendLine();
                        str = string.Format("{0} {1}", GCStrings.getString(759 + pvd.date.month), pvd.date.year);
                        tp1 = (80 - str.Length) / 2;
                        tp1 = tp1 < 0 ? 0 : tp1;
                        m_text.Append(string.Empty.PadLeft(tp1));
                        m_text.Append(str);
                        if (tp1 > versionText.Length)
                            m_text.Append(string.Empty.PadLeft(tp1 - versionText.Length));
                        m_text.Append(versionText);
                        m_text.AppendLine();
                        lastmonth = pvd.date.month;
                    }

                    if (nMasaHeader != 0)
                    {
                        str = m_Location.GetFullName();
                        tp1 = (80 - str.Length) / 2;
                        tp1 = tp1 < 0 ? 0 : tp1;
                        m_text.Append(string.Empty.PadLeft(tp1));
                        m_text.Append(str);
                        m_text.Append(string.Empty.PadLeft(tp1));
                        m_text.AppendLine();
                        m_text.AppendLine();

                        nMasaHeader = m_text.Length;
                        m_text.Append(" DATE            TITHI                         ");
                        if (GCDisplaySettings.getValue(39) != 0) m_text.Append("PAKSA ");
                        else m_text.Append("      ");
                        if (GCDisplaySettings.getValue(37) != 0) m_text.Append("YOGA      ");
                        if (GCDisplaySettings.getValue(36) != 0) m_text.Append("NAKSATRA       ");
                        if (GCDisplaySettings.getValue(38) != 0) m_text.Append("FAST ");
                        if (GCDisplaySettings.getValue(41) != 0) m_text.Append("RASI           ");
                        nMasaHeader = m_text.Length - nMasaHeader;
                        m_text.AppendLine();
                        
                        while (nMasaHeader > 0)
                        {
                            m_text.Append("----------");
                            nMasaHeader -= 10;
                        }
                        nMasaHeader = 0;
                        m_text.AppendLine();
                    }

                    dayText = formatPlainTextDay(pvd);

                    if (GCDisplaySettings.getValue(20) == 0 || pvd.dayEvents.Count() > 0)
                        m_text.Append(dayText);


                }
                //		date.shour = 0;
                //		date.NextDay();
            }

            sb.AppendNote();
            //	dcp.DestroyWindow();

            return 1;
        }


        /****************************************************************************
        *
        *
        *
        *****************************************************************************/

        public int formatRtf(StringBuilder m_text)
        {
            int k;
            int bShowColumnHeaders = 0;
            String str;
            StringBuilder dayText = new StringBuilder();

            VAISNAVADAY pvd, prevd, nextd;
            int lastmasa = -1;
            int lastmonth = -1;
            //	int tp1;
            //	double rate;
            bool bCalcMoon = (GCDisplaySettings.getValue(4) > 0 || GCDisplaySettings.getValue(5) > 0);
            TResultCalendar daybuff = this;
            m_text.Clear();

            GCStringBuilder sb = new GCStringBuilder();

            sb.Target = m_text;
            sb.Format = GCStringBuilder.FormatType.RichText;
            sb.fontSizeH1 = GCLayoutData.textSizeH1;
            sb.fontSizeH2 = GCLayoutData.textSizeH2;
            sb.fontSizeText = GCLayoutData.textSizeText;
            sb.fontSizeNote = GCLayoutData.textSizeNote;

            sb.AppendDocumentHeader();

            for (k = 0; k < daybuff.m_vcCount; k++)
            {

                prevd = daybuff.GetDay(k - 1);
                pvd = daybuff.GetDay(k);
                nextd = daybuff.GetDay(k + 1);

                if (pvd != null)
                {
                    bShowColumnHeaders = 0;
                    if ((GCDisplaySettings.getValue(GCDS.CAL_HEADER_MASA) == 0) && (pvd.astrodata.nMasa != lastmasa))
                    {
                        if (bShowColumnHeaders == 0)
                            m_text.Append("\\par ");
                        bShowColumnHeaders = 1;
                        //				m_text.Append("\\par\r\n";
                        str = string.Format("\\par \\pard\\f2\\fs{0}\\qc {1} {2}, Gaurabda {3}", GCLayoutData.textSizeH2
                            , GCMasa.GetName(pvd.astrodata.nMasa), GCStrings.getString(22)
                            , pvd.astrodata.nGaurabdaYear);
                        if ((pvd.astrodata.nMasa == MasaId.ADHIKA_MASA) && ((lastmasa >= MasaId.SRIDHARA_MASA) && (lastmasa <= MasaId.DAMODARA_MASA)))
                        {
                            str += "\\line ";
                            str += GCStrings.getString(128);
                        }
                        m_text.Append(str);

                        lastmasa = pvd.astrodata.nMasa;
                    }

                    if ((GCDisplaySettings.getValue(GCDS.CAL_HEADER_MASA) == 1) && (pvd.date.month != lastmonth))
                    {
                        if (bShowColumnHeaders == 0)
                            m_text.Append("\\par ");
                        bShowColumnHeaders = 1;
                        m_text.AppendFormat("\\par\\pard\\f2\\qc\\fs{0}\r\n", GCLayoutData.textSizeH2);
                        m_text.Append(pvd.Format("{monthName} {year}"));
                        lastmonth = pvd.date.month;
                    }

                    // print location text
                    if (bShowColumnHeaders != 0)
                    {
                        m_text.Append("\\par\\pard\\qc\\cf2\\fs22 ");
                        m_text.Append(daybuff.m_Location.GetFullName());

                        m_text.AppendFormat("\\par\\pard\\fs{0}\\qc {1}", GCLayoutData.textSizeNote, GCStrings.ShortVersionText);
                        m_text.Append("\\par\\par\r\n");

                        int tabStop = 5760 * GCLayoutData.textSizeText / 24;
                        m_text.AppendFormat("\\pard\\tx{0}\\tx{1} ", 2000 * GCLayoutData.textSizeText / 24, tabStop);
                        if (GCDisplaySettings.getValue(39) != 0)
                        {
                            tabStop += 990 * GCLayoutData.textSizeText / 24;
                            m_text.AppendFormat("\\tx{0}", tabStop);
                        }
                        if (GCDisplaySettings.getValue(37) != 0)
                        {
                            tabStop += 1720 * GCLayoutData.textSizeText / 24;
                            m_text.AppendFormat("\\tx{0}", tabStop);
                        }
                        if (GCDisplaySettings.getValue(36) != 0)
                        {
                            tabStop += 1800 * GCLayoutData.textSizeText / 24;
                            m_text.AppendFormat("\\tx{0}", tabStop);
                        }
                        if (GCDisplaySettings.getValue(38) != 0)
                        {
                            tabStop += 750 * GCLayoutData.textSizeText / 24;
                            m_text.AppendFormat("\\tx{0}", tabStop);
                        }
                        if (GCDisplaySettings.getValue(41) != 0)
                        {
                            tabStop += 1850 * GCLayoutData.textSizeText / 24;
                            m_text.AppendFormat("\\tx{0}", tabStop);
                        }
                        // paksa width 990
                        // yoga width 1720
                        // naks width 1800
                        // fast width 990
                        // rasi width 1850
                        m_text.AppendFormat("{{\\highlight15\\cf7\\fs{0}\\b DATE\\tab TITHI", GCLayoutData.textSizeNote);
                        if (GCDisplaySettings.getValue(39) != 0)
                        {
                            m_text.Append("\\tab PAKSA");
                        }
                        if (GCDisplaySettings.getValue(37) != 0)
                        {
                            m_text.Append("\\tab YOGA");
                        }
                        if (GCDisplaySettings.getValue(36) != 0)
                        {
                            m_text.Append("\\tab NAKSATRA");
                        }
                        if (GCDisplaySettings.getValue(38) != 0)
                        {
                            m_text.Append("\\tab FAST");
                        }
                        if (GCDisplaySettings.getValue(41) != 0)
                        {
                            m_text.Append("\\tab RASI");
                        }
                        m_text.Append("}");
                        //m_text.Append("\\par\r\n";
                        //m_text.Append(gpszSeparator;
                    }
                    m_text.AppendFormat("\\fs{0} ", GCLayoutData.textSizeText);

                    formatRtfDay(pvd, dayText);

                    if (GCDisplaySettings.getValue(20) == 0)
                        m_text.Append(dayText);
                    else if (pvd.dayEvents.Count() != 0)
                        m_text.Append(dayText);


                }
            }

            sb.AppendNote();
            sb.AppendDocumentTail();

            return 1;
        }

        public int formatRtfDay(VAISNAVADAY pvd, StringBuilder dayText)
        {
            String str;

            dayText.Clear();

            GCStringBuilder sb = new GCStringBuilder();

            sb.Target = dayText;
            sb.Format = GCStringBuilder.FormatType.RichText;
            sb.fontSizeH1 = GCLayoutData.textSizeH1;
            sb.fontSizeH2 = GCLayoutData.textSizeH2;
            sb.fontSizeText = GCLayoutData.textSizeText;
            sb.fontSizeNote = GCLayoutData.textSizeNote;

            if (pvd.astrodata.sun.longitude_deg < 0.0)
            {
                dayText.Append("\\par\\tab No rise and no set of the sun. No calendar information.");
                return 1;
            }

            str = pvd.GetTextRtf(GCDisplaySettings.getValue(39), GCDisplaySettings.getValue(36), GCDisplaySettings.getValue(37), GCDisplaySettings.getValue(38), GCDisplaySettings.getValue(41));
            dayText.Append(str);

            for (int i = 0; i < pvd.dayEvents.Count(); i++)
            {
                VAISNAVAEVENT ed = pvd.dayEvents[i];
                int disp = ed.dispItem;
                if (ed.dispItem != 0 && (disp == -1 || GCDisplaySettings.getValue(disp) != 0))
                {
                    if (ed.spec != null)
                    {
                        str = ed.text;
                        int length = str.Length;
                        length = (80 - length) / 2;
                        dayText.Append("\\par ");
                        sb.AppendSeparatorWithWidth(length);
                        dayText.Append(str);
                        sb.AppendSeparatorWithWidth(length);
                    }
                    else
                    {
                        sb.AppendTwoColumnText("", ed.text);
                    }
                }
            }

            return 0;
        }


        public int formatICal(StringBuilder m_text)
        {
            int k;
            int initialLength = 0;
            int lastmasa = -1;
            int tzoffset = 0, tzoff;
            string str, str2;
            StringBuilder dayText = new StringBuilder();
            VAISNAVADAY pvd, prevd, nextd;
            string SPACE_BEFORE_LINE = " , ";
            GregorianDateTime vc = new GregorianDateTime();
            TResultCalendar daybuff = this;
            CLocationRef loc = daybuff.m_Location;

            DateTime st = DateTime.Now;

            m_text.Append("BEGIN:VCALENDAR\nVERSION:2.0\nX-WR-CALNAME:VAISNAVA\nPRODID:-//GBC Calendar Comitee//GCAL//EN\n");
            m_text.Append("X-WR-RELCALID:");
            str2 = string.Format("{0:00000000}-{1:0000}-{2:0000}-{3:0000}-{4:0000}{5:00000000}", st.Year + st.Millisecond, st.Day * (int)st.DayOfWeek, st.Month,
                st.Hour, st.Minute + st.Millisecond);
            m_text.Append(str2);
            m_text.Append("\nX-WR-TIMEZONE:");

            m_text.Append(TTimeZone.GetTimeZoneName(loc.timezoneId));
            m_text.Append("\n");

            m_text.Append("CALSCALE:GREGORIAN\nMETHOD:PUBLISH\n");
            m_text.Append("BEGIN:VTIMEZONE\nTZID:");
            m_text.Append(TTimeZone.GetTimeZoneName(loc.timezoneId));
            str2 = string.Format("\nLAST-MODIFIED:%04d%02d%02dT%02d%02d%02dZ", st.Year, st.Month, st.Day, st.Hour, st.Minute, st.Second);
            m_text.Append(str2);

            tzoffset = TTimeZone.GetTimeZoneOffsetMinutes(loc.timezoneId);
            tzoff = tzoffset + TTimeZone.GetTimeZoneBias(loc.timezoneId);

            if (TTimeZone.GetTimeZoneBias(loc.timezoneId) > 0)
            {
                TTimeZone.GetDaylightTimeStartDate(loc.timezoneId, st.Year, ref vc);
                str2 = string.Format("\nBEGIN:DAYLIGHT\nDTSTART:{0:0000}{1:00}{2:00}T{3:00}0000", vc.year, vc.month, vc.day, vc.GetHour());
                m_text.Append(str2);

                str2 = string.Format("\nTZOFFSETTO:%c%02d%02d", (tzoff > 0 ? '+' : '-'), Math.Abs(tzoff) / 60, Math.Abs(tzoff) % 60);
                m_text.Append(str2);

                str2 = string.Format("\nTZOFFSETFROM:%c%02d%02d", '+', 0, 0);
                m_text.Append(str2);

                TTimeZone.GetNormalTimeStartDate(loc.timezoneId, st.Year, ref vc);
                m_text.Append("\nEND:DAYLIGHT\nBEGIN:STANDARD\nDTSTART:");
                str2 = string.Format("%04d%02d%02dT%02d0000", vc.year, vc.month, vc.day, vc.GetHour());
                m_text.Append(str2);

                str2 = string.Format("\nTZOFFSETTO:%c%02d%02d", (tzoffset > 0 ? '+' : '-'), Math.Abs(tzoffset) / 60, Math.Abs(tzoffset) % 60);
                m_text.Append(str2);
                str2 = string.Format("\nTZOFFSETFROM:%c%02d%02d", (tzoff > 0 ? '+' : '-'), Math.Abs(tzoff) / 60, Math.Abs(tzoff) % 60);
                m_text.Append(str2);
                m_text.Append("\nEND:STANDARD\n");
            }
            else
            {
                m_text.Append("\nBEGIN:STANDARD\nDTSTART:");
                str2 = string.Format("%04d0101T000000", vc.year, vc.month, vc.day, vc.GetHour());
                m_text.Append(str2);

                str = string.Format("\nTZOFFSETTO:%+02d%02d", tzoffset / 60, Math.Abs(tzoffset) % 60);
                m_text.Append(str2);
                str2 = string.Format("\nTZOFFSETFROM:+0000");
                m_text.Append(str2);
                m_text.Append("\nEND:STANDARD\n");
            }

            m_text.Append("END:VTIMEZONE\n");

            for (k = 0; k < daybuff.m_PureCount; k++)
            {
                //		date.shour = 0.0;
                //		date.TimeZone = earth.tzone;

                prevd = daybuff.GetDay(k - 1);
                pvd = daybuff.GetDay(k);
                nextd = daybuff.GetDay(k + 1);

                if (pvd != null)
                {
                    dayText.Clear();

                    if (pvd.astrodata.nMasa != lastmasa)
                    {
                        dayText.AppendFormat("{0} {1}, Gaurabda {2}", GCMasa.GetName(pvd.astrodata.nMasa), GCStrings.getString(22), pvd.astrodata.nGaurabdaYear);
                        dayText.AppendLine();
                        if ((pvd.astrodata.nMasa == MasaId.ADHIKA_MASA) && ((lastmasa >= MasaId.SRIDHARA_MASA) && (lastmasa <= MasaId.DAMODARA_MASA)))
                        {
                            if (dayText.Length > 0)
                                dayText.Append(SPACE_BEFORE_LINE);
                            dayText.AppendLine(GCStrings.getString(128));
                        }

                        lastmasa = pvd.astrodata.nMasa;
                        initialLength = -1;
                    }
                    else
                    {
                        initialLength = 0;
                    }

                    if (dayText.Length > 0)
                        dayText.Append(SPACE_BEFORE_LINE);
                    dayText.AppendLine(pvd.GetFullTithiName());

                    initialLength += dayText.Length;

                    if (pvd.astrodata.sun.longitude_deg < 0.0)
                    {
                        goto _resolve_text;
                    }

                    for (int i = 0; i < pvd.dayEvents.Count(); i++)
                    {
                        VAISNAVAEVENT ed = pvd.dayEvents[i];
                        int disp = ed.dispItem;
                        if (ed.dispItem != 0 && (disp == -1 || GCDisplaySettings.getValue(disp) != 0))
                        {
                            dayText.Append(SPACE_BEFORE_LINE);
                            dayText.AppendLine(ed.text);
                        }
                    }

                _resolve_text:
                    if (dayText.Length > initialLength)
                    {
                        m_text.Append("BEGIN:VEVENT\n");
                        m_text.AppendFormat("DTSTART;VALUE=DATE:{0:0000}{1:00}{2:00}\n", pvd.date.year, pvd.date.month, pvd.date.day);
                        str2 = string.Format("LOCATION:{0}\n", loc.locationName).Replace(",", "\\,");
                        m_text.Append(str2);
                        m_text.Append("SUMMARY:");
                        str2 = dayText.ToString().Trim().Replace(",", "\\,");
                        m_text.Append(str2);
                        str2 = string.Format("UID:{0:00000000}-{1:0000}-{2:0000}-{3:0000}-{4:00000000}{5:0000}\n", st.Year,
                            st.Month * 30 + st.Day, st.Hour * 60 + st.Minute, st.Second, st.Millisecond, k);
                        m_text.Append(str2);
                        m_text.Append("DURATION:P1D\nSEQUENCE:1\nEND:VEVENT\n");
                    }
                }
            }

            m_text.Append("END:VCALENDAR\n");
            return 1;
        }



        public int formatVCal(StringBuilder m_text)
        {
            int k;
            int initialLength = 0;
            int lastmasa = -1;
            string str, str2;
            StringBuilder dayText = new StringBuilder();
            VAISNAVADAY pvd, prevd, nextd;
            string SPACE_BEFORE_LINE = " , ";
            TResultCalendar daybuff = this;

            DateTime st = DateTime.Now;

            m_text.Append("BEGIN:VCALENDAR\nVERSION:1.0\nX-WR-CALNAME:VAISNAVA\nPRODID:-//GBC Calendar Comitee//GCAL//EN\n");
            m_text.Append("X-WR-RELCALID:");
            str2 = string.Format("{0:00000000}-{1:0000}-{2:0000}-{3:0000}-{4:0000}{5:00000000}", st.Year + st.Millisecond, st.Day * (int)st.DayOfWeek, st.Month,
                st.Hour, st.Minute + st.Millisecond);
            m_text.Append(str2);
            m_text.Append("\nX-WR-TIMEZONE:");

            m_text.Append(TTimeZone.GetTimeZoneName(daybuff.m_Location.timezoneId));
            m_text.Append("\n");

            m_text.Append("CALSCALE:GREGORIAN\nMETHOD:PUBLISH\n");
            /*m_text.Append("BEGIN:VTIMEZONE\nTZID:";
            m_text.Append(TTimeZone.GetTimeZoneName(nDst);
            str2 = string.Format("\nLAST-MODIFIED:%04d%02d%02dT%02d%02d%02dZ", st.Year, st.wMonth, st.wDay, st.wHour, st.wMinute, st.wSecond);
            m_text.Append(str2);

            tzoffset = int(TTimeZone.GetTimeZoneOffset(nDst) * 60.0);
            tzoff = tzoffset + TTimeZone.GetTimeZoneBias(nDst);

            if (TTimeZone.GetTimeZoneBias(nDst) > 0)
            {
                TTimeZone.GetDaylightTimeStartDate(nDst, st.wYear, vc);
                str2 = string.Format("\nBEGIN:DAYLIGHT\nDTSTART:%04d%02d%02dT%02d0000", vc.year, vc.month, vc.day, vc.GetHour());
                m_text.Append(str2);

                str2 = string.Format("\nTZOFFSETTO:%c%02d%02d", (tzoff > 0 ? '+' : '-'), Math.Abs(tzoff) / 60, Math.Abs(tzoff) % 60);
                m_text.Append(str2);

                str2 = string.Format("\nTZOFFSETFROM:%c%02d%02d", '+', 0, 0);
                m_text.Append(str2);

                TTimeZone.GetNormalTimeStartDate(nDst, st.wYear, vc);
                m_text.Append("\nEND:DAYLIGHT\nBEGIN:STANDARD\nDTSTART:";
                str2 = string.Format("%04d%02d%02dT%02d0000", vc.year, vc.month, vc.day, vc.GetHour());
                m_text.Append(str2);

                str2 = string.Format("\nTZOFFSETTO:%c%02d%02d", (tzoffset > 0 ? '+' : '-'), Math.Abs(tzoffset)/60, Math.Abs(tzoffset) % 60);
                m_text.Append(str2);
                str2 = string.Format("\nTZOFFSETFROM:%c%02d%02d", (tzoff > 0 ? '+' : '-'), Math.Abs(tzoff) / 60, Math.Abs(tzoff) % 60);
                m_text.Append(str2);
                m_text.Append("\nEND:STANDARD\n";
            }
            else
            {
                m_text.Append("\nBEGIN:STANDARD\nDTSTART:";
                str2 = string.Format("%04d0101T000000", vc.year, vc.month, vc.day, vc.GetHour());
                m_text.Append(str2);

                str.Format("\nTZOFFSETTO:%+02d%02d", tzoffset/60, Math.Abs(tzoffset) % 60);
                m_text.Append(str2);
                str2 = string.Format("\nTZOFFSETFROM:+0000");
                m_text.Append(str2);
                m_text.Append("\nEND:STANDARD\n";
            }

            m_text.Append("END:VTIMEZONE\n";
        */
            for (k = 0; k < daybuff.m_PureCount; k++)
            {
                //		date.shour = 0.0;
                //		date.TimeZone = earth.tzone;

                prevd = daybuff.GetDay(k - 1);
                pvd = daybuff.GetDay(k);
                nextd = daybuff.GetDay(k + 1);

                if (pvd != null)
                {
                    dayText.Clear();

                    if (pvd.astrodata.nMasa != lastmasa)
                    {
                        //str = string.Format("{0} {1}, Gaurabda {2}", GCMasa.GetName(pvd.astrodata.nMasa), GCStrings.getString(22), pvd.astrodata.nGaurabdaYear);
                        dayText.Append(pvd.Format(GCStrings.Localized("{masaName} Masa, Gaurabda {gaurabdaYear}")));
                        dayText.Append("\n");
                        if ((pvd.astrodata.nMasa == MasaId.ADHIKA_MASA) && ((lastmasa >= MasaId.SRIDHARA_MASA) && (lastmasa <= MasaId.DAMODARA_MASA)))
                        {
                            if (dayText.Length > 0)
                                dayText.Append(SPACE_BEFORE_LINE);
                            dayText.Append(GCStrings.getString(128));
                            dayText.Append("\n");
                        }

                        lastmasa = pvd.astrodata.nMasa;
                        initialLength = -1;
                    }
                    else
                    {
                        initialLength = 0;
                    }

                    if (dayText.Length > 0)
                        dayText.Append(SPACE_BEFORE_LINE);
                    dayText.Append(pvd.GetFullTithiName());
                    dayText.Append("\n");

                    initialLength += dayText.Length;

                    if (pvd.astrodata.sun.longitude_deg < 0.0)
                    {
                        goto _resolve_text;
                    }

                    //			if (GCDisplaySettings.getValue(17) == 1)
                    {
                        int h1, m1;
                        if (pvd.ekadasi_parana)
                        {
                            m_text.Append("BEGIN:VEVENT\n");
                            if (pvd.eparana_time1 >= 0.0)
                            {
                                GCMath.DaytimeToHourMin(pvd.eparana_time1, out h1, out m1);
                                str2 = string.Format("DTSTART:{0:0000}{1:00}{2:00}T{3:00}{4:00}00\n", pvd.date.year, pvd.date.month, pvd.date.day, (h1), (m1));
                            }
                            else
                            {
                                str2 = string.Format("DTSTART:{0:0000}{1:00}{2:00}T000000\n", pvd.date.year, pvd.date.month, pvd.date.day);
                            }
                            m_text.Append(str2);
                            if (pvd.eparana_time2 >= 0.0)
                            {
                                GCMath.DaytimeToHourMin(pvd.eparana_time2, out h1, out m1);
                                str2 = string.Format("DTEND:{0:0000}{1:00}{2:00}T{3:00}{4:00}00\n", pvd.date.year, pvd.date.month, pvd.date.day, (h1), (m1));
                            }
                            else
                            {
                                str2 = string.Format("DTEND:{0:0000}{1:00}{2:00}T{3:00}{4:00}00\n", pvd.date.year, pvd.date.month, pvd.date.day, pvd.astrodata.sun.set.hour, pvd.astrodata.sun.set.min);
                            }
                            m_text.Append(str2);
                            m_text.Append("SUMMARY:");
                            m_text.Append(GCStrings.getString(60));
                            m_text.Append("\nSEQUENCE:1\nEND:VEVENT\n");

                        }
                    }

                    for (int i = 0; i < pvd.dayEvents.Count(); i++)
                    {
                        VAISNAVAEVENT ed = pvd.dayEvents[i];
                        int disp = ed.dispItem;
                        if (ed.dispItem != 0 && (disp == -1 || GCDisplaySettings.getValue(disp) != 0))
                        {
                            dayText.Append(SPACE_BEFORE_LINE);
                            dayText.Append(ed.text);
                            dayText.Append("\n");
                        }
                    }

                _resolve_text:
                    if (dayText.Length > initialLength)
                    {
                        m_text.Append("BEGIN:VEVENT\n");
                        str2 = string.Format("DTSTART:{0:0000}{1:00}{2:00}T{3:00}{4:00}{5:00}\n", pvd.date.year, pvd.date.month, pvd.date.day,
                            pvd.astrodata.sun.rise.hour, pvd.astrodata.sun.rise.min, pvd.astrodata.sun.rise.sec);
                        m_text.Append(str2);
                        str2 = string.Format("DTEND:{0:0000}{1:00}{2:00}T{3:00}{4:00}{5:00}\n", pvd.date.year, pvd.date.month, pvd.date.day,
                            pvd.astrodata.sun.set.hour, pvd.astrodata.sun.set.min, pvd.astrodata.sun.set.sec);
                        m_text.Append(str2);
                        str2 = string.Format("LOCATION:{0}\n", daybuff.m_Location.locationName);
                        str2.Replace(",", "\\,");
                        m_text.Append(str2);
                        m_text.Append("SUMMARY:");
                        m_text.Append(dayText.ToString().Trim().Replace(",", "\\,"));
                        str2 = string.Format("UID:{0:00000000}-{1:0000}-{2:0000}-{3:0000}-{4:00000000}{5:0000}\n", st.Year, st.Month * 30 + st.Day, st.Hour * 60 + st.Minute, st.Second, st.Millisecond, k);
                        m_text.Append(str2);
                        m_text.Append("SEQUENCE:1\nEND:VEVENT\n");
                    }
                }
            }

            m_text.Append("END:VCALENDAR\n");
            return 1;
        }


        public int formatCsv(StringBuilder m_text)
        {
            int k;
            int initialLength = 0;
            int lastmasa = -1;
            string str2;
            StringBuilder dayText = new StringBuilder();
            VAISNAVADAY pvd, prevd, nextd;
            string SPACE_BEFORE_LINE = " , ";

            TResultCalendar daybuff = this;
            DateTime st = DateTime.Now;

            m_text.Append("\"Subject\",\"Begin Date\",\"Start\",\"End Date\",\"End\",\"WholeDay\",\"Alarm\"\n");

            for (k = 0; k < daybuff.m_PureCount; k++)
            {
                prevd = daybuff.GetDay(k - 1);
                pvd = daybuff.GetDay(k);
                nextd = daybuff.GetDay(k + 1);

                if (pvd != null)
                {
                    dayText.Clear();

                    if (pvd.astrodata.nMasa != lastmasa)
                    {
                        lastmasa = pvd.astrodata.nMasa;
                        initialLength = -1;
                    }
                    else
                    {
                        initialLength = 0;
                    }

                    if (dayText.Length > 0)
                        dayText.Append(SPACE_BEFORE_LINE);
                    dayText.Append(pvd.GetFullTithiName());
                    dayText.Append("; ");

                    initialLength = dayText.Length;

                    if (pvd.astrodata.sun.longitude_deg >= 0.0)
                    {
                        for (int i = 0; i < pvd.dayEvents.Count(); i++)
                        {
                            VAISNAVAEVENT ed = pvd.dayEvents[i];
                            int disp = ed.dispItem;
                            if (ed.dispItem != 0 && (disp == -1 || GCDisplaySettings.getValue(disp) != 0))
                            {
                                dayText.Append(ed.text);
                                dayText.Append("; ");
                            }
                        }
                    }

                    if (dayText.Length > initialLength || (GCDisplaySettings.getValue(20) == 0))
                    {
                        m_text.Append("\"");
                        m_text.Append(dayText);
                        m_text.Append("\",");

                        str2 = string.Format("\"{0}.{1}.{2}\",\"0:00:00\",\"{3}.{4}.{5}\",\"0:00:00\",\"True\",\"False\"\n",
                            pvd.date.day, pvd.date.month, pvd.date.year, nextd.date.day,
                            nextd.date.month, nextd.date.year);
                        m_text.Append(str2);
                    }
                }
            }

            return 1;
        }



        /******************************************************************************************/
        /*                                                                                        */
        /*                                                                                        */
        /*                                                                                        */
        /*                                                                                        */
        /*                                                                                        */
        /*                                                                                        */
        /*                                                                                        */
        /*                                                                                        */
        /*                                                                                        */
        /*                                                                                        */
        /*                                                                                        */
        /*                                                                                        */
        /*                                                                                        */
        /*                                                                                        */
        /*                                                                                        */
        /******************************************************************************************/

        public int formatHtml(StringBuilder xml)
        {
            int k;
            StringBuilder str = new StringBuilder();
            VAISNAVADAY pvd;
            int nPrevMasa = -1;
            TResultCalendar daybuff = this;

            xml.Append("<html><head><title>\n");
            xml.Append("Calendar ");
            xml.Append(daybuff.m_vcStart.year);
            xml.Append("</title>");
            xml.Append("<style>\n");
            xml.Append("<!--\nbody {\n");
            xml.Append("  font-family:Verdana;\n");
            xml.Append("  font-size:11pt;\n}\n\n");
            xml.Append("td.hed {\n");
            xml.Append("  font-family:Verdana;\n");
            xml.Append("  font-size:9pt;\n");
            xml.Append("  font-weight:bold;\n");
            xml.Append("  background:#aaaaaa;\n");
            xml.Append("  color:white;\n");
            xml.Append("  text-align:center;\n");
            xml.Append("  vertical-align:center;\n  padding-left:15pt;\n  padding-right:15pt;\n");
            xml.Append("  padding-top:5pt;\n  padding-bottom:5pt;\n}\n-->\n</style>\n");
            xml.Append("</head>\n<body>");

            for (k = 0; k < daybuff.m_vcCount; k++)
            {
                pvd = daybuff.GetDay(k);
                if (pvd != null)
                {
                    if (nPrevMasa != pvd.astrodata.nMasa)
                    {
                        if (nPrevMasa != -1)
                            xml.Append("\t</table>\n");
                        xml.Append("<p style=\'text-align:center;font-weight:bold\'><span style =\'font-size:14pt\'>");
                        xml.Append(GCMasa.GetName(pvd.astrodata.nMasa));
                        xml.Append(" Masa");
                        if (nPrevMasa == MasaId.ADHIKA_MASA)
                            xml.Append(" ");
                        xml.Append(GCStrings.Localized("(Second half)"));
                        xml.Append("</span>");
                        xml.Append("<br><span style=\'font-size:10pt;\'>Gaurabda ");
                        xml.Append(pvd.astrodata.nGaurabdaYear);
                        xml.Append("<br>");
                        xml.Append(daybuff.m_Location.GetFullName());
                        xml.Append("</font>");
                        xml.Append("</span></p>\n<table align=center>");
                        xml.Append("<tr><td  class=\"hed\"colspan=2>");
                        xml.Append("DATE</td><td class=\"hed\">TITHI</td><td class=\"hed\">P</td><td class=\"hed\">NAKSATRA</td><td class=\"hed\">YOGA</td><td class=\"hed\">FAST</td></tr>");
                    }

                    nPrevMasa = pvd.astrodata.nMasa;

                    if (pvd.dayEvents.Count() > 0)
                        continue;

                    // date data
                    xml.Append("<tr>");
                    xml.Append("<td>");
                    xml.Append(pvd.date);
                    xml.Append("</td><td>");
                    xml.Append(GCCalendar.GetWeekdayAbbr(pvd.date.dayOfWeek));
                    xml.Append("</td>\n");

                    // sunrise data

                    xml.Append("<td>\n");
                    xml.Append(pvd.GetFullTithiName());
                    xml.Append("</td>\n");

                    xml.AppendFormat("<td>{0}</td>\n", GCPaksa.GetAbbr(pvd.astrodata.nPaksa));
                    xml.AppendFormat("<td>{0}</td>\n", GCNaksatra.GetName(pvd.astrodata.nNaksatra));
                    xml.AppendFormat("<td>{0}</td>\n", GCYoga.GetName(pvd.astrodata.nYoga));


                    xml.Append("<td>");
                    xml.Append(((pvd.nFastType != FastType.FAST_NULL) ? "FAST</td>" : "</td>"));

                    str.Clear();


                    xml.Append("</tr>\n\n<tr>\n<td></td><td></td><td colspan=4>");
                    for (int i = 0; i < pvd.dayEvents.Count(); i++)
                    {
                        VAISNAVAEVENT ed = pvd.dayEvents[i];
                        int disp = ed.dispItem;
                        if (ed.dispItem != 0 && (disp == -1 || GCDisplaySettings.getValue(disp) != 0))
                        {
                            if (ed.spec != null)
                            {
                                xml.Append("<font color=\"blue\">");
                                xml.Append(ed.text);
                                xml.Append("</font><br>\n");
                            }
                            else
                            {
                                xml.Append(ed.text);
                                xml.Append("<br>\n");
                            }
                        }
                    }

                    xml.Append("\t</tr>\n\n");

                }
            }
            xml.Append("\t</table>\n\n");

            GCStringBuilder sb = new GCStringBuilder();
            sb.Target = xml;
            sb.Format = GCStringBuilder.FormatType.HtmlText;
            sb.AppendNote();

            xml.Append("</body>\n</html>\n");

            return 1;
        }


        /******************************************************************************************/
        /*                                                                                        */
        /*                                                                                        */
        /*                                                                                        */
        /*                                                                                        */
        /*                                                                                        */
        /*                                                                                        */
        /*                                                                                        */
        /*                                                                                        */
        /*                                                                                        */
        /*                                                                                        */
        /*                                                                                        */
        /*                                                                                        */
        /*                                                                                        */
        /*                                                                                        */
        /*                                                                                        */
        /******************************************************************************************/

        public int formatHtmlTable(StringBuilder xml)
        {
            int g_firstday_in_week = GCDisplaySettings.getValue(GCDS.GENERAL_FIRST_DOW);
            int k, y, lwd = 0;
            StringBuilder str = new StringBuilder();
            VAISNAVADAY pvd;
            int nPrevMasa = -1;
            int prevMas = -1;
            int brw = 0;

            TResultCalendar daybuff = this;

            // first = 1
            //int i_end[7] = {0, 6, 5, 4, 3, 2, 1}; //(6-(i-first))%7
            //int i_beg[7] = {6, 0, 1, 2, 3, 4, 5}; //(i-first)%7

            xml.Append("<html>\n<head>\n<title>Calendar ");
            xml.Append(daybuff.m_vcStart);
            xml.Append("</title>\n");

            xml.Append("<style>\n<!--\np.MsoNormal, li.MsoNormal, div.MsoNormal\n	{mso-style-parent:\"\";");
            xml.Append("margin:0in;margin-bottom:.0001pt;mso-pagination:widow-orphan;font-size:8.0pt;font-family:Arial;");
            xml.Append("mso-fareast-font-family:Arial;}");
            xml.Append("p.month\n{mso-style-name:month;\nmso-margin-top-alt:auto;\nmargin-right:0in;\nmso-margin-bottom-alt:auto;\nmargin-left:0in;\nmso-pagination:widow-orphan;\nfont-size:17.0pt;font-family:Arial;mso-fareast-font-family:Arial;}\n");
            xml.Append(".text\n{mso-style-name:text;\nmso-margin-top-alt:auto;\nmargin-right:0in;\nmso-margin-bottom-alt:auto;\nmargin-left:0in;\n	mso-pagination:widow-orphan;\nfont-size:6.0pt;\nmso-bidi-font-size:6.0pt;\nfont-family:Arial;	mso-fareast-font-family:\"Arial\";mso-bidi-font-family:\"Arial\";}\n");
            xml.Append(".tnote\n{mso-style-name:text;\nmso-margin-top-alt:auto;\nmargin-right:0in;\nmso-margin-bottom-alt:auto;\nmargin-left:0in;\n	mso-pagination:widow-orphan;\nfont-size:7.0pt;\nmso-bidi-font-size:7.0pt;\nfont-family:Arial;	mso-fareast-font-family:Arial;mso-bidi-font-family:Arial;}\n");
            xml.Append(".tithiname\n{mso-style-name:text;\nmso-margin-top-alt:auto;\nmargin-right:0in;\nmso-margin-bottom-alt:auto;\nmargin-left:0in;\n	mso-pagination:widow-orphan;\nfont-size:8.0pt;\nmso-bidi-font-size:8.0pt;\nfont-family:Arial;	mso-fareast-font-family:\"Arial\";mso-bidi-font-family:\"Arial\";}\n");
            xml.Append(".dayt\n	{mso-style-name:dayt;\nfont-size:12.0pt;\nmso-ansi-font-size:12.0pt;\nfont-family:Arial;\nmso-ascii-font-family:Arial;\nmso-hansi-font-family:Arial;\nfont-weight:bold;\nmso-bidi-font-weight:normal;}\n");
            xml.Append("span.SpellE\n{mso-style-name:\"\";\nmso-spl-e:yes;}\n");
            xml.Append("span.GramE\n{mso-style-name:\"\";\nmso-gram-e:yes;}\n");
            xml.Append("-->\n</style>\n");

            xml.Append("</head>\n\n<body>\n\n");

            for (k = 0; k < daybuff.m_vcCount; k++)
            {
                pvd = daybuff.GetDay(k);
                if (pvd != null)
                {
                    bool bSemicolon = false;
                    bool bBr = false;
                    lwd = pvd.date.dayOfWeek;
                    if (nPrevMasa != pvd.date.month)
                    {
                        if (nPrevMasa != -1)
                        {
                            for (y = 0; y < DAYS_TO_ENDWEEK(lwd); y++)
                            {
                                xml.Append("<td style=\'border:solid windowtext 1.0pt;mso-border-alt:solid windowtext .5pt;padding:3.0pt 3.0pt 3.0pt 3.0pt\'>&nbsp;</td>");
                            }
                            xml.Append("</tr></table>\n<p>&nbsp;</p>");
                        }
                        xml.Append("\n<table width=\"100%\" border=0 frame=bottom cellspacing=0 cellpadding=0><tr><td width=\"60%\"><p class=month>");
                        xml.Append(GregorianDateTime.GetMonthName(pvd.date.month));
                        xml.Append(" ");
                        xml.Append(pvd.date.year);
                        xml.Append("</p></td><td><p class=tnote align=right>");
                        xml.Append(daybuff.m_Location.locationName);
                        xml.Append("<br>Timezone: ");
                        xml.Append(TTimeZone.GetTimeZoneName(daybuff.m_Location.timezoneId));
                        xml.Append("</p>");
                        xml.Append("</td></tr></table><hr>");
                        nPrevMasa = pvd.date.month;
                        xml.Append("\n<table width=\"100%\" bordercolor=black cellpadding=0 cellspacing=0>\n<tr>\n");
                        for (y = 0; y < 7; y++)
                        {
                            xml.Append("<td width=\"14%\" align=center style=\'font-size:10.0pt;border:none\'>");
                            xml.Append(GCCalendar.GetWeekdayName(DAY_INDEX(y)));
                            xml.Append("</td>\n");
                        }
                        xml.Append("<tr>\n");
                        for (y = 0; y < DAYS_FROM_BEGINWEEK(pvd.date.dayOfWeek); y++)
                            xml.Append("<td style=\'border:solid windowtext 1.0pt;mso-border-alt:solid windowtext .5pt;padding:3.0pt 3.0pt 3.0pt 3.0pt\'>&nbsp;</td>");
                    }
                    else
                    {
                        if (pvd.date.dayOfWeek == g_firstday_in_week)
                            xml.Append("<tr>\n");
                    }

                    // date data
                    xml.Append("\n<td valign=top style=\'border:solid windowtext 1.0pt;mso-border-alt:solid windowtext .5pt;padding:3.0pt 3.0pt 3.0pt 3.0pt\' ");
                    xml.Append("bgcolor=\"");
                    xml.Append(getDayBkgColorCode(pvd));
                    xml.Append("\">");
                    xml.Append("<table width=\"100%\" border=0><tr><td><p class=text><span class=dayt>");
                    xml.Append(pvd.date.day);
                    xml.Append("</span></td><td>");


                    xml.Append("<span class=\"tithiname\">");
                    xml.Append(pvd.GetFullTithiName());
                    xml.Append("</span>");
                    xml.Append("</td></tr></table>\n");
                    brw = 0;
                    xml.Append("<span class=\"text\">\n");

                    str.Clear();

                    if (pvd.dayEvents.Count() > 0)
                    {
                        if (brw != 0)
                            xml.Append("<br>\n");
                        brw = 1;
                        bSemicolon = false;
                    }

                    for (int i = 0; i < pvd.dayEvents.Count(); i++)
                    {
                        VAISNAVAEVENT ed = pvd.dayEvents[i];
                        int disp = ed.dispItem;
                        if (ed.dispItem != 0 && (disp == -1 || GCDisplaySettings.getValue(disp) != 0))
                        {
                            if (bSemicolon)
                                xml.Append("; ");
                            bSemicolon = true;
                            if (ed.spec != null)
                            {
                                xml.Append(ed.text);
                            }
                            else
                            {
                                xml.AppendFormat("<i>{0}</i>\n", ed.text);
                            }
                        }
                    }


                    if (prevMas != pvd.astrodata.nMasa)
                    {
                        if (brw != 0)
                            xml.Append("<br>\n");
                        brw = 1;
                        xml.Append("<b>[" + pvd.Format(GCStrings.Localized("{masaName} Masa")) + "]</b>");
                        //xml.Append(GCMasa.GetName(pvd.astrodata.nMasa));
                        //xml.Append(" Masa]</b>");
                        prevMas = pvd.astrodata.nMasa;
                    }
                    xml.Append("</span>");
                    xml.Append("</td>\n\n");

                }
            }

            for (y = 1; y < DAYS_TO_ENDWEEK(lwd); y++)
            {
                xml.Append("<td style=\'border:solid windowtext 1.0pt;mso-border-alt:solid windowtext .5pt;padding:3.0pt 3.0pt 3.0pt 3.0pt\'>&nbsp;</td>");
            }

            xml.Append("</tr>\n</table>\n");
            xml.Append("</body>\n</html>\n");

            return 1;
        }


    }
}
