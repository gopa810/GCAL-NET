using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GCAL.Base
{
    public class VAISNAVAEVENT
    {
        public int prio;
        public int dispItem;
        public string text;
        public int fasttype;
        public string fastsubject;
        public string spec = null;
    }

    public class VAISNAVADAY
    {
        // date
        public GregorianDateTime date;
        // moon times
        public GCHourTime moonrise;
        public GCHourTime moonset;
        // astronomical data from astro-sub-layer
        public GCAstroData astrodata;

        public UInt32 nCaturmasya;
        public int nDST;
        public int nFeasting;
        // data for vaisnava calculations
        public List<VAISNAVAEVENT> dayEvents;

        public String festivals;
        public int nFastType;
        public int nMhdType;
        public String ekadasi_vrata_name;
        public bool ekadasi_parana;
        public double eparana_time1, eparana_time2;
        public int eparana_type1, eparana_type2;
        public int sankranti_zodiac;
        //double sankranti_time;
        public GregorianDateTime sankranti_day;

        public VAISNAVADAY()
        {
            nFastType = FastType.FAST_NULL;
            nMhdType = MahadvadasiType.EV_NULL;
            ekadasi_parana = false;
            ekadasi_vrata_name = "";
            eparana_time1 = eparana_time2 = 0.0;
            eparana_type1 = eparana_type2 = EkadasiParanaType.EP_TYPE_NULL;
            sankranti_zodiac = -1;
            nDST = 0;
            nCaturmasya = 0;
            moonrise.SetValue(0);
            moonset.SetValue(0);
            dayEvents = new List<VAISNAVAEVENT>();
        }

        public void Clear()
        {
            // init
            nFastType = FastType.FAST_NULL;
            nFeasting = FeastType.FEAST_NULL;
            nMhdType = MahadvadasiType.EV_NULL;
            ekadasi_parana = false;
            ekadasi_vrata_name = "";
            eparana_time1 = eparana_time2 = 0.0;
            sankranti_zodiac = -1;
            sankranti_day = null;
            nCaturmasya = 0;
            dayEvents.Clear();
            //moonset.SetValue(0);
            //moonrise.SetValue(0);
            //nDST = 0;
        }

        public bool GetTithiTimeRange(GCEarthData earth, out GregorianDateTime from, out GregorianDateTime to)
        {
            GregorianDateTime start = new GregorianDateTime();

            start.Set(date);
            start.shour = astrodata.sun.sunrise_deg / 360 + earth.offsetUtcHours / 24.0;

            GCTithi.GetNextTithiStart(earth, start, out to);
            GCTithi.GetPrevTithiStart(earth, start, out from);

            return true;

        }

        public bool GetNaksatraTimeRange(GCEarthData earth, out GregorianDateTime from, out GregorianDateTime to)
        {
            GregorianDateTime start = new GregorianDateTime();

            start.Set(date);
            start.shour = astrodata.sun.sunrise_deg / 360 + earth.offsetUtcHours / 24.0;

            GCNaksatra.GetNextNaksatra(earth, start, out to);
            GCNaksatra.GetPrevNaksatra(earth, start, out from);

            return true;
        }

        public string GetTextEP()
        {
            string str = string.Empty;
            int h1, m1, h2, m2;
            h1 = GCMath.IntFloor(eparana_time1);
            m1 = GCMath.IntFloor(GCMath.getFraction(eparana_time1) * 60);

            if (eparana_time2 >= 0.0)
            {
                h2 = GCMath.IntFloor(eparana_time2);
                m2 = GCMath.IntFloor(GCMath.getFraction(eparana_time2) * 60);

                if (GCDisplaySettings.getValue(50) == 1)
                    str = string.Format("{0} {1:00}:{2:00} ({3}) - {4:00}:{5:00} ({6}) {7}", GCStrings.getString(60),
                        h1, m1, GCEkadasi.GetParanaReasonText(eparana_type1),
                        h2, m2, GCEkadasi.GetParanaReasonText(eparana_type2),
                        GCStrings.GetDSTSignature(nDST));
                else
                    str = string.Format("{0} {1:00}:{2:00} - {3:00}:{4:00} ({5})", GCStrings.getString(60),
                        h1, m1, h2, m2, GCStrings.GetDSTSignature(nDST));
            }
            else if (eparana_time1 >= 0.0)
            {
                if (GCDisplaySettings.getValue(50) == 1)
                    str = string.Format("{0} {1:00}:{2:00} ({3}) {4}", GCStrings.getString(61),
                        h1, m1, GCEkadasi.GetParanaReasonText(eparana_type1), GCStrings.GetDSTSignature(nDST));
                else
                    str = string.Format("{0} {1:00}:{2:00} ({3})", GCStrings.getString(61),
                        h1, m1, GCStrings.GetDSTSignature(nDST));
            }
            else
            {
                str = GCStrings.getString(62);
            }
            return str;
        }

        public string GetDateText()
        {
            return string.Format("{0:00} {1} {2} {3}", date.day, GregorianDateTime.GetMonthAbreviation(date.month), 
                date.year, GCCalendar.GetWeekdayAbbr(date.dayOfWeek));
        }

        public string GetTextA(int bPaksa, int bNaks, int bYoga, int bFlag, int bRasi)
        {
            //	static char * dow[] = {"Su", "Mo", "Tu", "We", "Th", "Fr", "Sa" };

            string s1, str;

            s1 = GetFullTithiName();

            str = string.Format("{0}{1} ", 
                s1.PadRight(34, ' '), (bPaksa != 0 ? GCPaksa.GetAbbr(astrodata.nPaksa) : ' '));

            if (bYoga != 0)
            {
                str += GCYoga.GetName(astrodata.nYoga).PadRight(10, ' ');
            }

            if (bNaks != 0)
            {
                str += GCNaksatra.GetName(astrodata.nNaksatra).PadRight(15, ' ');
            }

            if (nFastType != FastType.FAST_NULL && bFlag != 0)
                str += " *";
            else
                str += "  ";

            if (bRasi != 0)
            {
                if (bRasi == 1)
                    str += GCRasi.GetName(GCRasi.GetRasi(astrodata.moon.longitude_deg, astrodata.msAyanamsa)).PadRight(12, ' ').PadLeft(15, ' ');
                else if (bRasi == 2)
                    str += GCRasi.GetNameEn(GCRasi.GetRasi(astrodata.moon.longitude_deg, astrodata.msAyanamsa)).PadRight(12, ' ').PadLeft(15, ' ');
            }

            return str;
        }

        public string GetTextRtf(int bPaksa, int bNaks, int bYoga, int bFlag, int bRasi)
        {
            String s1, s2, str;

            s1 = GetFullTithiName();

            s2 = GCStrings.getString(date.dayOfWeek).Substring(0, 2);

            str = string.Format("\\par {0:##} {1} {2} {3}\\tab {4}\\tab {5} ", date.day, GregorianDateTime.GetMonthAbreviation(date.month), date.year
                , s2, s1, (bPaksa != 0 ? GCPaksa.GetAbbr(astrodata.nPaksa) : ' '));

            if (bYoga != 0)
            {
                str += "\\tab " + GCYoga.GetName(astrodata.nYoga);
            }

            if (bNaks != 0)
            {
                str += "\\tab " + GCNaksatra.GetName(astrodata.nNaksatra);
            }

            if (nFastType != FastType.FAST_NULL && bFlag != 0)
                str += "\\tab *";
            else if (bFlag != 0)
                str += "\\tab ";

            if (bRasi != 0)
            {

                if (bRasi == 1)
                    s2 = string.Format("\\tab {0}", GCRasi.GetName(GCRasi.GetRasi(astrodata.moon.longitude_deg, astrodata.msAyanamsa)));
                else
                    s2 = string.Format("\\tab {0}", GCRasi.GetNameEn(GCRasi.GetRasi(astrodata.moon.longitude_deg, astrodata.msAyanamsa)));
                str += s2;
            }

            str += "\r\n";

            return str;
        }


        bool hasEventsOfDisplayIndex(int dispIndex)
        {
            foreach (VAISNAVAEVENT md in dayEvents)
            {
                if (md.dispItem == dispIndex)
                    return true;
            }

            return false;
        }


        public VAISNAVAEVENT findEventsText(string text)
        {

            foreach (VAISNAVAEVENT md in dayEvents)
            {
                if (md.text != null && md.text.IndexOf(text, 0, StringComparison.CurrentCultureIgnoreCase) >= 0)
                    return md;
            }

            return null;
        }


        public VAISNAVAEVENT AddEvent(int priority, int dispItem, string text)
        {
            VAISNAVAEVENT dc = new VAISNAVAEVENT();

            dc.prio = priority;
            dc.dispItem = dispItem;
            dc.text = text;

            dayEvents.Add(dc);

            return dc;
        }
        
        public bool AddSpecFestival(int nSpecialFestival, int nFestClass)
        {
            String str;
            int fasting = FastType.FAST_NULL;
            string fastingSubject = null;

            switch (nSpecialFestival)
            {
                case SpecialFestivalId.SPEC_JANMASTAMI:
                    str = GCStrings.getString(741);
                    fasting = FastType.FAST_MIDNIGHT;
                    fastingSubject = "Sri Krsna";
                    break;
                case SpecialFestivalId.SPEC_GAURAPURNIMA:
                    str = GCStrings.getString(742);
                    fasting = FastType.FAST_MOONRISE;
                    fastingSubject = "Sri Caitanya Mahaprabhu";
                    break;
                case SpecialFestivalId.SPEC_RETURNRATHA:
                    str = GCStrings.getString(743);
                    break;
                case SpecialFestivalId.SPEC_HERAPANCAMI:
                    str = GCStrings.getString(744);
                    break;
                case SpecialFestivalId.SPEC_GUNDICAMARJANA:
                    str = GCStrings.getString(745);
                    break;
                case SpecialFestivalId.SPEC_GOVARDHANPUJA:
                    str = GCStrings.getString(746);
                    break;
                case SpecialFestivalId.SPEC_RAMANAVAMI:
                    str = GCStrings.getString(747);
                    fasting = FastType.FAST_SUNSET;
                    fastingSubject = "Sri Ramacandra";
                    break;
                case SpecialFestivalId.SPEC_RATHAYATRA:
                    str = GCStrings.getString(748);
                    break;
                case SpecialFestivalId.SPEC_NANDAUTSAVA:
                    str = GCStrings.getString(749);
                    break;
                case SpecialFestivalId.SPEC_PRABHAPP:
                    str = GCStrings.getString(759);
                    fasting = FastType.FAST_NOON;
                    fastingSubject = "Srila Prabhupada";
                    break;
                case SpecialFestivalId.SPEC_MISRAFESTIVAL:
                    str = GCStrings.getString(750);
                    break;
                default:
                    return false;
            }

            VAISNAVAEVENT md = AddEvent((int)DisplayPriorities.PRIO_FESTIVALS_0 + (nFestClass - (int)GCDS.CAL_FEST_0) * 100, nFestClass, str);
            if (fasting > 0)
            {
                md.fasttype = fasting;
                md.fastsubject = fastingSubject;
            }

            return false;
        }

        public string Format(string format, params string[] args)
        {
            StringBuilder sb = new StringBuilder(format);

            if (format.IndexOf("{day}") >= 0)
                format.Replace("{day}", date.day.ToString());
            if (format.IndexOf("{month}") >= 0)
                format.Replace("{month}", date.month.ToString());
            if (format.IndexOf("{monthAbr}") >= 0)
                format.Replace("{monthAbr}", GregorianDateTime.GetMonthName(date.month));
            if (format.IndexOf("{monthName}") >= 0)
                format.Replace("{monthName}", GregorianDateTime.GetMonthName(date.month));
            if (format.IndexOf("{year}") >= 0)
                format.Replace("{year}", date.year.ToString());
            if (format.IndexOf("{hour}") >= 0)
                format.Replace("{hour}", date.GetHour().ToString("D2"));
            if (format.IndexOf("{min}") >= 0)
                format.Replace("{min}", date.GetMinute().ToString("D2"));
            if (format.IndexOf("{minRound}") >= 0)
                format.Replace("{minRound}", date.GetMinuteRound().ToString("D2"));
            if (format.IndexOf("{sec}") >= 0)
                format.Replace("{sec}", date.GetSecond().ToString("D2"));

            if (format.IndexOf("{masaName}") >= 0)
                format.Replace("{masaName}", GCMasa.GetName(astrodata.nMasa));
            if (format.IndexOf("{gaurabdaYear}") >= 0)
                format.Replace("{gaurabdaYear}", astrodata.nGaurabdaYear.ToString());
            if (format.IndexOf("{tithiName}") >= 0)
                format.Replace("{tithiName}", GCTithi.GetName(astrodata.nTithi));
            if (format.IndexOf("{prevTithiName}") >= 0)
                format.Replace("{prevTithiName}", GCTithi.GetName((astrodata.nTithi + 29) % 30));
            if (format.IndexOf("{nextTithiName}") >= 0)
                format.Replace("{nextTithiName}", GCTithi.GetName((astrodata.nTithi + 1) % 30));
            if (format.IndexOf("{paksaName}") >= 0)
                format.Replace("{paksaName}", GCPaksa.GetName(astrodata.nPaksa));
            if (format.IndexOf("{yogaName}") >= 0)
                format.Replace("{yogaName}", GCYoga.GetName(astrodata.nYoga));
            if (format.IndexOf("{naksatraName}") >= 0)
                format.Replace("{naksatraName}", GCNaksatra.GetName(astrodata.nNaksatra));
            if (format.IndexOf("{naksatraElapse}") >= 0)
                format.Replace("{naksatraElapse}", astrodata.nNaksatraElapse.ToString("P2"));
            if (format.IndexOf("{naksatraPada}") >= 0)
                format.Replace("{naksatraPada}", GCNaksatra.GetPadaText(astrodata.NaksatraPada));

            if (format.IndexOf("{sankranti.day}") >= 0)
                format.Replace("{sankranti.day}", sankranti_day.day.ToString());
            if (format.IndexOf("{sankranti.month}") >= 0)
                format.Replace("{sankranti.month}", sankranti_day.month.ToString());
            if (format.IndexOf("{sankranti.monthAbr}") >= 0)
                format.Replace("{sankranti.monthAbr}", GregorianDateTime.GetMonthName(sankranti_day.month));
            if (format.IndexOf("{sankranti.monthName}") >= 0)
                format.Replace("{sankranti.monthName}", GregorianDateTime.GetMonthName(sankranti_day.month));
            if (format.IndexOf("{sankranti.hour}") >= 0)
                format.Replace("{sankranti.hour}", sankranti_day.GetHour().ToString("D2"));
            if (format.IndexOf("{sankranti.min}") >= 0)
                format.Replace("{sankranti.min}", sankranti_day.GetMinute().ToString("D2"));
            if (format.IndexOf("{sankranti.minRound}") >= 0)
                format.Replace("{sankranti.minRound}", sankranti_day.GetMinuteRound().ToString("D2"));
            if (format.IndexOf("{sankranti.sec}") >= 0)
                format.Replace("{sankranti.sec}", sankranti_day.GetSecond().ToString("D2"));
            if (format.IndexOf("{sankranti.rasiNameEn}") >= 0)
                format.Replace("{sankranti.rasiNameEn}", GCRasi.GetNameEn(sankranti_zodiac));
            if (format.IndexOf("{sankranti.rasiName}") >= 0)
                format.Replace("{sankranti.rasiName}", GCRasi.GetName(sankranti_zodiac));

            if (format.IndexOf("{dstSig}") >= 0)
                format.Replace("{dstSig}", GCStrings.GetDSTSignature(nDST));

            if (format.IndexOf("{moonRiseTime}") >= 0)
                format.Replace("{moonRiseTime}", moonrise.ToShortTimeString());
            if (format.IndexOf("{moonSetTime}") >= 0)
                format.Replace("{moonSetTime}", moonset.ToShortTimeString());
            if (format.IndexOf("{moonRasiName}") >= 0)
                format.Replace("{moonRasiName}", GCRasi.GetName(astrodata.nMoonRasi));
            if (format.IndexOf("{moonRasiNameEn}") >= 0)
                format.Replace("{moonRasiNameEn}", GCRasi.GetNameEn(astrodata.nMoonRasi));

            if (args == null || args.Length == 0)
                return format.ToString();
            else
                return string.Format(format.ToString(), args);
        }

        public String GetFullTithiName()
        {
            string str;
            str = GCTithi.GetName(astrodata.nTithi);

            if (HasExtraFastingNote())
            {
                str = string.Format("{0} {1}", str, GetExtraFastingNote());
            }

            return str;
        }

        public bool HasExtraFastingNote()
        {
            if ((astrodata.nTithi == 10) || (astrodata.nTithi == 25) || (astrodata.nTithi == 11) || (astrodata.nTithi == 26))
            {
                if (ekadasi_parana == false)
                {
                    return true;
                }
            }

            return false;
        }

        public string GetExtraFastingNote()
        {
            if (HasExtraFastingNote())
            {
                if (nMhdType == MahadvadasiType.EV_NULL)
                {
                    return GCStrings.getString(58);
                }
                else
                {
                    return GCStrings.getString(59);
                }
            }
            return "";
        }

        public int GetTextLineCount()
        {
            int nCount = 0;
            String str2;

            nCount++;

            foreach (VAISNAVAEVENT ed in dayEvents)
            {
                if (ed.dispItem != 0 && (ed.dispItem == -1 || GCDisplaySettings.getValue(ed.dispItem) != 0))
                {
                    nCount++;
                }
            }

            return nCount;
        }



    }

    public class VAISNAVAEVENTComparer : Comparer<VAISNAVAEVENT>
    {
        public override int Compare(VAISNAVAEVENT x, VAISNAVAEVENT y)
        {
            return x.prio - y.prio;
        }
    }
}
