using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using GCAL.Base.Scripting;

namespace GCAL.Base
{
    public class TResultCalendar: TResultBase
    {
        public static readonly int BEFORE_DAYS = 9;
        public VAISNAVADAY[] m_pData;
        public int m_nCount;
        public int m_PureCount;
        public GCLocation m_Location;
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

        public TResultCalendar(GCLocation loc, int nYear) : this()
        {
            CalculateCalendar(loc, new GregorianDateTime(nYear, 1, 1), GregorianDateTime.IsLeapYear(nYear) ? 366 : 365);
        }

        public TResultCalendar(GCLocation loc, int nYear, int nMonth) : this()
        {
            CalculateCalendar(loc, new GregorianDateTime(nYear, nMonth, 1), GregorianDateTime.GetMonthMaxDays(nYear, nMonth));
        }

        public override GSCore GetPropertyValue(string s)
        {
            switch (s)
            {
                case "itemIndexes":
                    GSList list = new GSList();
                    for (int i = 0; i < m_vcCount; i++)
                        list.Add(new GSNumber(i + BEFORE_DAYS));
                    return list;
                case "startDate":
                    return m_vcStart;
                default:
                    return base.GetPropertyValue(s);
            }
        }

        public override GSCore ExecuteMessage(string token, GSCoreCollection args)
        {
            switch(token)
            {
                case "getDay":
                    return GetDay((int)args.getSafe(0).getIntegerValue() - BEFORE_DAYS);
                case "daysToEndweek":
                    return new GSNumber(DAYS_TO_ENDWEEK((int)args.getSafe(0).getIntegerValue()));
                case "daysFromBeginweek":
                    return new GSNumber(DAYS_FROM_BEGINWEEK((int)args.getSafe(0).getIntegerValue()));
                case "dayIndex":
                    return new GSNumber(DAY_INDEX((int)args.getSafe(0).getIntegerValue()));
                default:
                    return base.ExecuteMessage(token, args);
            }
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

        public static string getDayBkgColorCode(VAISNAVADAY p)
        {
            if (p == null)
                return "white";
            if (p.nFastID == FastType.FAST_EKADASI)
                return "#FFFFBB";
            if (p.nFastID != 0)
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
                nTithi = m_pData[nIndex].astrodata.sunRise.Tithi;
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
            int sunRiseNaksatra = t.astrodata.sunRise.Naksatra;

            if (sunRiseNaksatra != u.astrodata.sunRise.Naksatra)
                return false;

            if (t.astrodata.sunRise.Paksa != 1)
                return false;

            if (t.astrodata.sunRise.Tithi == t.astrodata.sunSet.Tithi)
            {
                if (sunRiseNaksatra == 6) // punarvasu
                {
                    nMahaType = MahadvadasiType.EV_JAYA;
                    return true;
                }
                else if (sunRiseNaksatra == 3) // rohini
                {
                    nMahaType = MahadvadasiType.EV_JAYANTI;
                    return true;
                }
                else if (sunRiseNaksatra == 7) // pusyami
                {
                    nMahaType = MahadvadasiType.EV_PAPA_NASINI;
                    return true;
                }
                else if (sunRiseNaksatra == 21) // sravana
                {
                    nMahaType = MahadvadasiType.EV_VIJAYA;
                    return true;
                }
                else
                    return false;
            }
            else
            {
                if (sunRiseNaksatra == 21) // sravana
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


        public int CalculateCalendar(GCLocation loc, GregorianDateTime begDate, int iCount)
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
            m_Location = loc;
            m_vcStart = new GregorianDateTime(begDate);
            m_vcCount = iCount;
            earth = loc.GetEarthData();

            // alokacia pola
            m_pData = new VAISNAVADAY[nTotalCount];

            // inicializacia poctovych premennych
            m_nCount = nTotalCount;
            m_PureCount = iCount;

            date = new GregorianDateTime();
            date.Set(begDate);
            date.shour = 0.0;
            date.TimezoneHours = loc.OffsetUtcHours;
            date.SubtractDays(BEFORE_DAYS);
            date.InitWeekDay();

            weekday = date.dayOfWeek;

            GCFestivalSpecialExecutor exec = new GCFestivalSpecialExecutor(this);

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
                m_pData[i].BiasMinutes = loc.TimeZone.GetBiasMinutesForDay(m_pData[i].date);
                //		TRACE("DST %d.%d.%d = %d ", m_pData[i].date.day, m_pData[i].date.month, m_pData[i].date.year, m_pData[i].nDST);
            }

            // 3
            if (bCalcMoon)
            {
                for (i = 0; i < nTotalCount; i++)
                {
                    GCMoonData.CalcMoonTimes(earth, m_pData[i].date, Convert.ToDouble(m_pData[i].BiasMinutes/60.0), out m_pData[i].moonrise, out m_pData[i].moonset);

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
                m_pData[i].astrodata = new GCAstroData();
                m_pData[i].astrodata.DayCalc(m_pData[i].date, earth);
            }

            bool calc_masa;

            // 5
            // init of masa
            prev_paksa = -1;

            for (i = 0; i < nTotalCount; i++)
            {
                calc_masa = (m_pData[i].astrodata.sunRise.Paksa != prev_paksa);
                prev_paksa = m_pData[i].astrodata.sunRise.Paksa;

                if (i == 0)
                    calc_masa = true;

                if (calc_masa)
                {
                    m_pData[i].astrodata.MasaCalc(m_pData[i].date, earth);
                    lastMasa = m_pData[i].astrodata.Masa;
                    lastGYear = m_pData[i].astrodata.GaurabdaYear;
                }
                m_pData[i].astrodata.Masa = lastMasa;
                m_pData[i].astrodata.GaurabdaYear = lastGYear;

                if (GCDisplaySettings.getValue(GCDS.CAL_ARUN_TITHI) != 0)
                {
                    tempStr = string.Format("{0}: {1}", GCStrings.Localized("Tithi at Arunodaya"), GCTithi.GetName(m_pData[i].astrodata.sunArunodaya.Tithi));
                    m_pData[i].AddEvent(DisplayPriorities.PRIO_ARUN, GCDS.CAL_ARUN_TITHI, tempStr);
                }

                if (GCDisplaySettings.getValue(GCDS.CAL_ARUN_TIME) != 0)
                {
                    tempStr = string.Format("{0} {1} ({2})", GCStrings.Localized("Time of Arunodaya"), m_pData[i].astrodata.sunArunodaya.ToShortTimeString()
                        , GCStrings.GetDSTSignature(m_pData[i].BiasMinutes));
                    m_pData[i].AddEvent(DisplayPriorities.PRIO_ARUN, GCDS.CAL_ARUN_TIME, tempStr);
                }

                if (GCDisplaySettings.getValue(GCDS.CAL_SUN_LONG) != 0)
                {
                    tempStr = string.Format("{0}: {1} (*)", GCStrings.Localized("Sun Longitude"), m_pData[i].astrodata.sunRise.longitude);
                    m_pData[i].AddEvent(DisplayPriorities.PRIO_ASTRO, GCDS.CAL_SUN_LONG, tempStr);
                }

                if (GCDisplaySettings.getValue(GCDS.CAL_MOON_LONG) != 0)
                {
                    tempStr = string.Format("{0}: {1} (*)", GCStrings.Localized("Moon Longitude"), m_pData[i].astrodata.sunRise.longitudeMoon);
                    m_pData[i].AddEvent(DisplayPriorities.PRIO_ASTRO, GCDS.CAL_MOON_LONG, tempStr);
                }

                if (GCDisplaySettings.getValue(GCDS.CAL_AYANAMSHA) != 0)
                {
                    tempStr = string.Format("{0} {1} ({2}) (*)", GCStrings.Localized("Ayanamsha"), m_pData[i].astrodata.Ayanamsa, GCAyanamsha.GetAyanamsaName(GCAyanamsha.GetAyanamsaType()));
                    m_pData[i].AddEvent(DisplayPriorities.PRIO_ASTRO, GCDS.CAL_AYANAMSHA, tempStr);
                }

                if (GCDisplaySettings.getValue(GCDS.CAL_JULIAN) != 0)
                {
                    tempStr = string.Format("{0} {1} (*)", GCStrings.Localized("Julian Time"), m_pData[i].astrodata.JulianDay);
                    m_pData[i].AddEvent(DisplayPriorities.PRIO_ASTRO, GCDS.CAL_JULIAN, tempStr);
                }
            }


            if (GCDisplaySettings.getValue(GCDS.CAL_MASA_CHANGE) != 0)
            {
                String str;

                for (i = BEFORE_DAYS; i < m_PureCount + BEFORE_DAYS + 2; i++)
                {
                    if (m_pData[i - 1].astrodata.Masa != m_pData[i].astrodata.Masa)
                    {
                        str = m_pData[i].Format(GCStrings.Localized("First day of {masaName} Masa"));
                        m_pData[i].AddEvent(DisplayPriorities.PRIO_MASA_CHANGE, GCDS.CAL_MASA_CHANGE, str);
                    }

                    if (m_pData[i + 1].astrodata.Masa != m_pData[i].astrodata.Masa)
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
                    if (m_pData[i - 1].BiasMinutes == 0 && m_pData[i].BiasMinutes != 0)
                        m_pData[i].AddEvent(DisplayPriorities.PRIO_DST_CHANGE, GCDS.CAL_DST_CHANGE, GCStrings.Localized("First day of Daylight Saving Time"));
                    else if (m_pData[i].BiasMinutes != 0 && m_pData[i + 1].BiasMinutes == 0)
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
                EkadasiCalc(i, earth);
            }

            // init ksaya data
            // init of second day of vriddhi
            CalculateKsayaVriddhiTithis();

            //
            // calculate sankrantis
            CalculateSankrantis();

            // 7
            // init of festivals
            CompleteCalc(exec);

            // 8
            // init of festivals
            /*for (i = BEFORE_DAYS; i < m_PureCount + BEFORE_DAYS; i++)
            {
                ExtendedCalc(i, earth);
            }*/
            //
            // apply daylight saving time
            ApplyDaylightSavingHours();

            //
            // resolve festivals fasting
            for (i = BEFORE_DAYS; i < m_PureCount + BEFORE_DAYS; i++)
            {
                ResolveFestivalsFasting(i);



                if (GCDisplaySettings.getValue(GCDS.CAL_SUN_RISE) != 0)
                {
                    tempStr = string.Format("{0}-{1}-{2}  {3} - {4} - {5} ({6})",
                        GCStrings.Localized("Sunrise"),
                        GCStrings.Localized("Noon"),
                        GCStrings.Localized("Sunset"),
                        m_pData[i].astrodata.sunRise.ToShortTimeString(),
                        m_pData[i].astrodata.sunNoon.ToShortTimeString(),
                        m_pData[i].astrodata.sunSet.ToShortTimeString(),
                        GCStrings.GetDSTSignature(m_pData[i].BiasMinutes));
                    m_pData[i].AddEvent(DisplayPriorities.PRIO_SUN, GCDS.CAL_SUN_RISE, tempStr);
                }

                if (GCDisplaySettings.getValue(GCDS.CAL_SUN_SANDHYA) != 0)
                {
                    tempStr = string.Format("{0}: {1} | {2} | {3}   ({4})",
                        GCStrings.Localized("Sandhyas"),
                        m_pData[i].astrodata.sunRise.ShortSandhyaString(),
                        m_pData[i].astrodata.sunNoon.ShortSandhyaString(),
                        m_pData[i].astrodata.sunSet.ShortSandhyaString(),
                        GCStrings.GetDSTSignature(m_pData[i].BiasMinutes));
                    m_pData[i].AddEvent(DisplayPriorities.PRIO_SUN, GCDS.CAL_SUN_SANDHYA, tempStr);
                }

                if (GCDisplaySettings.getValue(GCDS.CAL_BRAHMA_MUHURTA) != 0)
                {
                    tempStr = string.Format("{0}: {1}   ({2})",
                        GCStrings.Localized("Brahma Muhurta"),
                        m_pData[i].astrodata.sunRise.ShortMuhurtaString(-2),
                        GCStrings.GetDSTSignature(m_pData[i].BiasMinutes));
                    m_pData[i].AddEvent(DisplayPriorities.PRIO_SUN, GCDS.CAL_BRAHMA_MUHURTA, tempStr);
                }


            }



            if (GCDisplaySettings.getValue(GCDS.CAL_COREEVENTS) != 0)
            {
                TResultEvents coreEvents = new TResultEvents();
                GCDisplaySettings.Push();
                GCDisplaySettings.setValue(GCDS.COREEVENTS_ABHIJIT_MUHURTA, 0);
                GCDisplaySettings.setValue(GCDS.COREEVENTS_ASCENDENT, 0);
                GCDisplaySettings.setValue(GCDS.COREEVENTS_CONJUNCTION, 0);
                GCDisplaySettings.setValue(GCDS.COREEVENTS_GULIKALAM, 0);
                GCDisplaySettings.setValue(GCDS.COREEVENTS_MOON, 0);
                GCDisplaySettings.setValue(GCDS.COREEVENTS_MOONRASI, 0);
                GCDisplaySettings.setValue(GCDS.COREEVENTS_NAKSATRA, 1);
                GCDisplaySettings.setValue(GCDS.COREEVENTS_RAHUKALAM, 0);
                GCDisplaySettings.setValue(GCDS.COREEVENTS_SANKRANTI, 0);
                GCDisplaySettings.setValue(GCDS.COREEVENTS_SUN, 0);
                GCDisplaySettings.setValue(GCDS.COREEVENTS_TITHI, 1);
                GCDisplaySettings.setValue(GCDS.COREEVENTS_YAMAGHANTI, 0);
                GCDisplaySettings.setValue(GCDS.COREEVENTS_YOGA, 1);
                coreEvents.CalculateEvents(loc, begDate, new GregorianDateTime(begDate, iCount));
                for (i = BEFORE_DAYS; i < m_PureCount + BEFORE_DAYS; i++)
                {
                    for (int j = 0; j < coreEvents.p_events.Count; j++)
                    {
                        TDayEvent tde = coreEvents.p_events[j];
                        if (tde.Time.EqualDay(m_pData[i].date))
                        {
                            if (m_pData[i].BiasMinutes != 0)
                                tde.Time.AddHours(m_pData[i].BiasMinutes / 60.0);
                        }
                    }
                }
                coreEvents.Sort(TResultEvents.SORTING_BY_DATE);
                GCDisplaySettings.Pop();

                for (i = BEFORE_DAYS; i < m_PureCount + BEFORE_DAYS; i++)
                {
                    int number = 0;
                    for (int j = 0; j < coreEvents.p_events.Count; j++)
                    {
                        TDayEvent tde = coreEvents.p_events[j];
                        if (tde.Time.EqualDay(m_pData[i].date))
                        {
                            m_pData[i].AddEvent(DisplayPriorities.PRIO_ASTRO + number, GCDS.CAL_COREEVENTS,
                                tde.TypeString + "   " + tde.Time.LongTime + " ("
                                + GCStrings.GetDSTSignature(m_pData[i].BiasMinutes) + ")");
                            number++;
                        }
                    }
                }
            }


            // sorting day events according priority
            VAISNAVAEVENTComparer vec = new VAISNAVAEVENTComparer();
            for (i = BEFORE_DAYS; i < m_PureCount + BEFORE_DAYS; i++)
            {
                m_pData[i].dayEvents.Sort(vec);
            }


            return 1;

        }

        private void ApplyDaylightSavingHours()
        {
            for (int i = BEFORE_DAYS; i < m_PureCount + BEFORE_DAYS; i++)
            {
                if (m_pData[i].eparana_time1 > 0.0)
                {
                    m_pData[i].eparana_time1 += m_pData[i].BiasMinutes/60.0;
                }

                if (m_pData[i].eparana_time2 > 0.0)
                {
                    m_pData[i].eparana_time2 += m_pData[i].BiasMinutes/60.0;
                }

                if (m_pData[i].astrodata.sunRise.longitude > 0.0)
                {
                    m_pData[i].astrodata.sunRise.AddMinutes(m_pData[i].BiasMinutes);
                    m_pData[i].astrodata.sunSet.AddMinutes(m_pData[i].BiasMinutes);
                    m_pData[i].astrodata.sunNoon.AddMinutes(m_pData[i].BiasMinutes);
                    m_pData[i].astrodata.sunArunodaya.AddMinutes(m_pData[i].BiasMinutes);
                }
            }
        }

        private int CalculateKsayaVriddhiTithis()
        {
            int i;
            GCEarthData earth = m_Location.GetEarthData();

            for (i = BEFORE_DAYS; i < m_PureCount + BEFORE_DAYS; i++)
            {
                if (m_pData[i].astrodata.sunRise.Tithi == m_pData[i - 1].astrodata.sunRise.Tithi)
                {
                    m_pData[i].vriddhiDayNo = 2;
                    if (GCDisplaySettings.getValue(GCDS.CAL_VRDDHI) != 0)
                        m_pData[i].AddEvent(DisplayPriorities.PRIO_KSAYA, GCDS.CAL_VRDDHI, GCStrings.getString(90));
                }
                else if (m_pData[i].astrodata.sunRise.Tithi != GCTithi.NEXT_TITHI(m_pData[i - 1].astrodata.sunRise.Tithi))
                {
                    m_pData[i - 1].ksayaTithi = GCTithi.NEXT_TITHI(m_pData[i - 1].astrodata.sunRise.Tithi);
                    m_pData[i - 1].ksayaMasa = (m_pData[i - 1].ksayaTithi == 0 ? m_pData[i].astrodata.Masa : m_pData[i - 1].astrodata.Masa);

                    String str;
                    GregorianDateTime day1, d1, d2;

                    day1 = new GregorianDateTime();
                    day1.Set(m_pData[i].date);
                    day1.shour = m_pData[i].astrodata.sunRise.TotalDays;

                    GCTithi.GetPrevTithiStart(earth, day1, out d2);
                    day1.Set(d2);
                    day1.shour -= 0.1;
                    day1.NormalizeValues();
                    GCTithi.GetPrevTithiStart(earth, day1, out d1);

                    d1.shour += (m_pData[i].BiasMinutes / 1440.0);
                    d2.shour += (m_pData[i].BiasMinutes / 1440.0);

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
            return i;
        }

        protected void CalculateSankrantis()
        {
            // init for sankranti
            GregorianDateTime date = new GregorianDateTime(m_pData[0].date);
            int i = 0;
            GCEarthData earth = m_Location.GetEarthData();
            bool bFoundSan;
            int zodiac;
            int i_target;
            do
            {
                date.Set(GCSankranti.GetNextSankranti(date, earth, out zodiac));
                date.shour += m_Location.TimeZone.GetBiasMinutesForDay(date) / 1440.0;
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
                                if (date.shour < m_pData[i].astrodata.sunRise.TotalDays)
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
                                if (date.shour > m_pData[i].astrodata.sunNoon.TotalDays)
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
                                if (date.shour > m_pData[i].astrodata.sunSet.TotalDays)
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

                date.AddDays(25);
            } while (bFoundSan == true);

        }

        public int EkadasiCalc(int nIndex, GCEarthData earth)
        {
            VAISNAVADAY s = m_pData[nIndex - 1];
            VAISNAVADAY t = m_pData[nIndex];
            VAISNAVADAY u = m_pData[nIndex + 1];

            if (GCTithi.TITHI_EKADASI(t.astrodata.sunRise.Tithi))
            {
                // if TAT < 11 then NOT_EKADASI
                if (GCTithi.TITHI_LESS_EKADASI(t.astrodata.sunArunodaya.Tithi))
                {
                    t.nMahadvadasiID = MahadvadasiType.EV_NULL;
                    t.ekadasi_vrata_name = "";
                    t.nFastID = FastType.FAST_NULL;
                }
                else
                {
                    // else ak MD13 then MHD1 and/or 3
                    if (GCTithi.TITHI_EKADASI(s.astrodata.sunRise.Tithi) && GCTithi.TITHI_EKADASI(s.astrodata.sunArunodaya.Tithi))
                    {
                        if (GCTithi.TITHI_TRAYODASI(u.astrodata.sunRise.Tithi))
                        {
                            t.nMahadvadasiID = MahadvadasiType.EV_UNMILANI_TRISPRSA;
                            t.ekadasi_vrata_name = GCEkadasi.GetEkadasiName(t.astrodata.Masa, t.astrodata.sunRise.Paksa);
                            t.nFastID = FastType.FAST_EKADASI;
                        }
                        else
                        {
                            t.nMahadvadasiID = MahadvadasiType.EV_UNMILANI;
                            t.ekadasi_vrata_name = GCEkadasi.GetEkadasiName(t.astrodata.Masa, t.astrodata.sunRise.Paksa);
                            t.nFastID = FastType.FAST_EKADASI;
                        }
                    }
                    else
                    {
                        if (GCTithi.TITHI_TRAYODASI(u.astrodata.sunRise.Tithi))
                        {
                            t.nMahadvadasiID = MahadvadasiType.EV_TRISPRSA;
                            t.ekadasi_vrata_name = GCEkadasi.GetEkadasiName(t.astrodata.Masa, t.astrodata.sunRise.Paksa);
                            t.nFastID = FastType.FAST_EKADASI;
                        }
                        else
                        {
                            // else ak U je MAHADVADASI then NOT_EKADASI
                            if (GCTithi.TITHI_EKADASI(u.astrodata.sunRise.Tithi) || (u.nMahadvadasiID >= MahadvadasiType.EV_SUDDHA))
                            {
                                t.nMahadvadasiID = MahadvadasiType.EV_NULL;
                                t.ekadasi_vrata_name = "";
                                t.nFastID = FastType.FAST_NULL;
                            }
                            else if (u.nMahadvadasiID == MahadvadasiType.EV_NULL)
                            {
                                // else suddha ekadasi
                                t.nMahadvadasiID = MahadvadasiType.EV_SUDDHA;
                                t.ekadasi_vrata_name = GCEkadasi.GetEkadasiName(t.astrodata.Masa, t.astrodata.sunRise.Paksa);
                                t.nFastID = FastType.FAST_EKADASI;
                            }
                        }
                    }
                }
            }
            // test for break fast

            if (s.nFastID == FastType.FAST_EKADASI)
            {
                double parBeg, parEnd;

                CalculateEParana(s, t, out parBeg, out parEnd, earth);

            }

            return 1;
        }


        public int CompleteCalc(GCFestivalSpecialExecutor exec)
        {
            int currFestTop = 0;

            for (int i = BEFORE_DAYS; i < m_PureCount + BEFORE_DAYS - 1; i++)
            {
                exec.CurrentIndex = i;
                foreach (GCFestivalBook book in GCFestivalBookCollection.Books)
                {
                    if (!book.Visible)
                        continue;

                    foreach (GCFestivalBase fb in book.Festivals)
                    {
                        if (fb.nReserved == 1 && fb.nVisible > 0 && fb.IsFestivalDay(exec))
                        {
                            currFestTop = AddEventToDay(exec, 0, currFestTop, fb);
                        }
                    }
                }
            }

            return 1;
        }


        /* OLD CODE FOR GOVARDHANA AND JANMASTAMI
         * 
            VAISNAVADAY s = exec.day(-1);
            VAISNAVADAY t = exec.day(0);
            VAISNAVADAY u = exec.day(1);
            VAISNAVADAY v = exec.day(2);

         if (t.astrodata.nMasa == MasaId.DAMODARA_MASA)
        {
            if (t.astrodata.nTithi == TithiId.TITHI_GAURA_PRATIPAT)
            {
                GCMoonData.CalcMoonTimes(earth, s.date, s.nDST, out s.moonrise, out s.moonset);
                GCMoonData.CalcMoonTimes(earth, t.date, t.nDST, out t.moonrise, out t.moonset);
                if (s.astrodata.nTithi == TithiId.TITHI_GAURA_PRATIPAT)
                {
                    if (s.moonrise.hour >= 0)
                    {
                        if (!s.moonrise.IsGreaterThan(t.astrodata.sun.rise))
                            t.AddSpecFestival(SpecialFestivalId.SPEC_GOVARDHANPUJA, GCDS.CAL_FEST_1);
                    }
                    else if (t.moonrise.hour >= 0)
                    {
                        if (!t.moonrise.IsLessThan(u.astrodata.sun.rise))
                            t.AddSpecFestival(SpecialFestivalId.SPEC_GOVARDHANPUJA, GCDS.CAL_FEST_1);
                    }
                }
                else if (u.astrodata.nTithi == TithiId.TITHI_GAURA_PRATIPAT)
                {
                    if (t.moonrise.hour >= 0)
                    {
                        if (t.moonrise.IsGreaterThan(t.astrodata.sun.rise))
                            // today is GOVARDHANA PUJA
                            t.AddSpecFestival(SpecialFestivalId.SPEC_GOVARDHANPUJA, GCDS.CAL_FEST_1);
                    }
                    else if (u.moonrise.hour >= 0)
                    {
                        if (u.moonrise.IsLessThan(u.astrodata.sun.rise))
                            // today is GOVARDHANA PUJA
                            t.AddSpecFestival(SpecialFestivalId.SPEC_GOVARDHANPUJA, GCDS.CAL_FEST_1);
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
                        mid_nak_t = (int)GCNaksatra.CalculateMidnightNaksatra(t.date);
                        mid_nak_u = (int)GCNaksatra.CalculateMidnightNaksatra(u.date);

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
        }*/

        /// <summary>
        /// 
        /// </summary>
        /// <param name="exec"></param>
        /// <param name="offsetWithToday">If value of this parameter is 0, then current day is processed,
        /// -1 is for yesterday, +1 is for tomorrow</param>
        /// <param name="currFestTop"></param>
        /// <param name="fb"></param>
        /// <returns></returns>
        private int AddEventToDay(GCFestivalSpecialExecutor exec, int offsetWithToday, int currFestTop, GCFestivalBase fb)
        {
            VAISNAVADAY t = exec.day(offsetWithToday);
            VAISNAVAEVENT md = t.AddEvent(DisplayPriorities.PRIO_FESTIVALS_0 + fb.BookID * 100 + currFestTop, 
                GCDS.CAL_FEST_0 + fb.BookID, fb.Text);
            currFestTop += 5;
            if (fb.FastID > 0)
            {
                md.fasttype = fb.FastID;
                md.fastsubject = fb.FastingSubject;
            }

            if (GCDisplaySettings.getValue(51) != 2 && fb.StartYear > -7000)
            {
                String ss1;
                int years = t.astrodata.GaurabdaYear - (fb.StartYear - 1496);
                string appx = "th";
                if (years % 10 == 1) appx = "st";
                else if (years % 10 == 2) appx = "nd";
                else if (years % 10 == 3) appx = "rd";
                if (GCDisplaySettings.getValue(51) == 0)
                {
                    ss1 = string.Format("{0} ({1}{2} anniversary)", md.text, years, appx);
                }
                else
                {
                    ss1 = string.Format("{0} ({1}{2})", md.text, years, appx);
                }
                md.text = ss1;
            }

            if (fb.EventsCount > 0)
            {
                foreach (GCFestivalBase re in fb.Events)
                {
                    if (re is GCFestivalRelated)
                    {
                        GCFestivalRelated related = re as GCFestivalRelated;
                        AddEventToDay(exec, fb.DayOffset + related.DayOffset, 0, related);
                    }
                }
            }

            return currFestTop;
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
            if (GCTithi.TITHI_DVADASI(s.astrodata.sunRise.Tithi))
                return 1;

            if (TithiId.TITHI_GAURA_DVADASI == t.astrodata.sunRise.Tithi && TithiId.TITHI_GAURA_DVADASI == t.astrodata.sunSet.Tithi && IsMhd58(nIndex, out nMahaType))
            {
                t.nMahadvadasiID = nMahaType;
                nMhdDay = nIndex;
            }
            else if (GCTithi.TITHI_DVADASI(t.astrodata.sunRise.Tithi))
            {
                if (GCTithi.TITHI_DVADASI(u.astrodata.sunRise.Tithi) && GCTithi.TITHI_EKADASI(s.astrodata.sunRise.Tithi) && GCTithi.TITHI_EKADASI(s.astrodata.sunArunodaya.Tithi))
                {
                    t.nMahadvadasiID = MahadvadasiType.EV_VYANJULI;
                    nMhdDay = nIndex;
                }
                else if (NextNewFullIsVriddhi(nIndex, earth))
                {
                    t.nMahadvadasiID = MahadvadasiType.EV_PAKSAVARDHINI;
                    nMhdDay = nIndex;
                }
                else if (GCTithi.TITHI_LESS_EKADASI(s.astrodata.sunArunodaya.Tithi))
                {
                    t.nMahadvadasiID = MahadvadasiType.EV_SUDDHA;
                    nMhdDay = nIndex;
                }
            }

            if (nMhdDay >= 0)
            {
                // fasting day
                m_pData[nMhdDay].nFastID = FastType.FAST_EKADASI;
                m_pData[nMhdDay].ekadasi_vrata_name = GCEkadasi.GetEkadasiName(t.astrodata.Masa, t.astrodata.sunRise.Paksa);
                m_pData[nMhdDay].ekadasi_parana = false;
                m_pData[nMhdDay].eparana_time1 = 0.0;
                m_pData[nMhdDay].eparana_time2 = 0.0;

                // parana day
                m_pData[nMhdDay + 1].nFastID = FastType.FAST_NULL;
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

        /*public int ExtendedCalc(int nIndex, GCEarthData earth)
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
        }*/


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
            t.nMahadvadasiID = MahadvadasiType.EV_NULL;
            t.ekadasi_parana = true;
            t.nFastID = FastType.FAST_NULL;

            double titBeg, titEnd, tithi_quart;
            double sunRise, third_day, naksEnd;
            double parBeg = -1.0, parEnd = -1.0;
            double tithi_len;

            sunRise = t.astrodata.sunRise.TotalDays;
            third_day = sunRise + (t.astrodata.sunSet.TotalDays - sunRise)/3;
            tithi_len = GCTithi.GetTithiTimes(earth, t.date, out titBeg, out titEnd, sunRise);
            tithi_quart = tithi_len / 4.0 + titBeg;

            switch (s.nMahadvadasiID)
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
                    if (GCTithi.TITHI_DVADASI(t.astrodata.sunRise.Tithi))
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

                    if (GCTithi.TITHI_DVADASI(t.astrodata.sunRise.Tithi))
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

                    if (GCTithi.TITHI_DVADASI(s.astrodata.sunRise.Tithi))
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

        public void ResolveFestivalsFasting(int nIndex)
        {
            VAISNAVADAY s = m_pData[nIndex - 1];
            VAISNAVADAY t = m_pData[nIndex];
            VAISNAVADAY u = m_pData[nIndex + 1];

            String str;
            String subject = string.Empty;
            int fastForDay = 0;
            int fastForEvent = 0;
            string ch;
            VAISNAVAEVENT md;

            if (t.nMahadvadasiID != MahadvadasiType.EV_NULL)
            {
                str = string.Format(GCStrings.Localized("Fasting for {0}"), t.ekadasi_vrata_name);
                t.AddEvent(DisplayPriorities.PRIO_EKADASI, GCDS.CAL_EKADASI_PARANA, str);
            }

            ch = GCEkadasi.GetMahadvadasiName((int)t.nMahadvadasiID);
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
                fastForEvent = FastType.FAST_NULL;
                if (md.fasttype != FastType.FAST_NULL)
                {
                    fastForEvent = md.fasttype;
                    subject = md.fastsubject;
                }

                if (fastForEvent != FastType.FAST_NULL)
                {
                    if (s.nFastID == FastType.FAST_EKADASI)
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
                    else if (t.nFastID == FastType.FAST_EKADASI)
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
                        /* OLD STYLE FASTING
                        if (GCDisplaySettings.getValue(42) == 0)
                        {
                            if (nftype > 1)
                                nftype = 7;
                            else nftype = 0;
                        }*/
                        if (fastForEvent != FastType.FAST_NULL)
                        {
                            t.AddEvent(md.prio + 1, md.dispItem,
                                GCStrings.GetFastingName(fastForEvent));
                        }
                    }
                }
                if (fastForDay < fastForEvent)
                    fastForDay = fastForEvent;
            }

            if (fastForDay != FastType.FAST_NULL)
            {
                if (s.nFastID == FastType.FAST_EKADASI)
                {
                    t.nFeasting = FeastType.FEAST_TODAY_FAST_YESTERDAY;
                    s.nFeasting = FeastType.FEAST_TOMMOROW_FAST_TODAY;
                }
                else if (t.nFastID == FastType.FAST_EKADASI)
                {
                    u.nFeasting = FeastType.FEAST_TODAY_FAST_YESTERDAY;
                    t.nFeasting = FeastType.FEAST_TOMMOROW_FAST_TODAY;
                }
                else
                {
                    t.nFastID = fastForDay;
                }
            }

        }

        public override string formatText(string df)
        {
            GSScript script = new GSScript();
            switch (df)
            {
                case GCDataFormat.PlainText:
                    script.readTextTemplate(Properties.Resources.TplCalendarPlain);
                    break;
                case GCDataFormat.Rtf:
                    script.readTextTemplate(Properties.Resources.TplCalendarRtf);
                    break;
                case GCDataFormat.HTML:
                    script.readTextTemplate(Properties.Resources.TplCalendarHtml);
                    break;
                case GCDataFormat.XML:
                    script.readTextTemplate(Properties.Resources.TplCalendarXml);
                    break;
                case GCDataFormat.CSV:
                    script.readTextTemplate(Properties.Resources.TplCalendarCsv);
                    break;
                case GCDataFormat.ICAL:
                    script.readTextTemplate(Properties.Resources.TplCalendarICAL);
                    break;
                case GCDataFormat.VCAL:
                    script.readTextTemplate(Properties.Resources.TplCalendarVCAL);
                    break;
                case "htmlTable":
                    script.readTextTemplate(Properties.Resources.TplCalendarHtmlTable);
                    break;
                case "htmlSadhana":
                    script.readTextTemplate(Properties.Resources.TplCalendarSadhana);
                    break;
                default:
                    break;
            }


            GSExecutor engine = new GSExecutor();
            engine.SetVariable("calendar", this);
            engine.SetVariable("location", this.m_Location);
            engine.SetVariable("app", GCUserInterface.Shared);
            engine.ExecuteElement(script);

            return engine.getOutput();
        }

        public override TResultFormatCollection getFormats()
        {
            TResultFormatCollection coll = base.getFormats();

            coll.ResultName = "Calendar";
            coll.Formats.Add(new TResultFormat("Text File", "txt", GCDataFormat.PlainText));
            coll.Formats.Add(new TResultFormat("Rich Text File", "rtf", GCDataFormat.Rtf));
            coll.Formats.Add(new TResultFormat("XML File", "xml", GCDataFormat.XML));
            coll.Formats.Add(new TResultFormat("iCalendar File", "ics", GCDataFormat.ICAL));
            coll.Formats.Add(new TResultFormat("vCalendar File", "vcs", GCDataFormat.VCAL));
            coll.Formats.Add(new TResultFormat("Comma Separated Values", "csv", GCDataFormat.CSV));
            coll.Formats.Add(new TResultFormat("HTML File (in Table format)", "htm", "htmlTable"));
            coll.Formats.Add(new TResultFormat("HTML File (in List format)", "htm", GCDataFormat.HTML));
            coll.Formats.Add(new TResultFormat("HTML format daily sadhana", "htm", "htmlSadhana"));
            return coll;
        }


        public static string getPlainDayTemplate()
        {
            return Properties.Resources.TplCalendarDayPlain;
        }
    }
}
