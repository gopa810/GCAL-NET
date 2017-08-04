using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using GCAL.Base.Scripting;

namespace GCAL.Base
{
    public class VAISNAVAEVENT: GSCore
    {
        public int prio;
        public int dispItem;
        public string text;
        public int fasttype;
        public string fastsubject;
        public string spec = null;

        public override GSCore GetPropertyValue(string s)
        {
            switch (s)
            {
                case "prio": return new GSNumber() { IntegerValue = prio };
                case "dispItem": return new GSNumber() { IntegerValue = dispItem };
                case "fastType": return new GSNumber() { IntegerValue = fasttype };
                case "text": return new GSString() { Value = text };
                case "htmlText": return new GSString() { 
                    Value = GetHtmlText() 
                };
                case "fastSubject": return new GSString() { Value = text };
                case "spec": return new GSString() { Value = (spec != null ? spec : string.Empty) };
                case "isSpec": return new GSBoolean() { Value = (spec != null) };
                case "specTextColor": return new GSString() { Value = "blue" };
                default: break;
            }
            return base.GetPropertyValue(s);
        }

        public string GetHtmlText()
        {
            if (dispItem == GCDS.CAL_SUN_RISE || dispItem == GCDS.CAL_SUN_SANDHYA ||
                dispItem == GCDS.CAL_BRAHMA_MUHURTA)
                return "&#9724; " + text;
            else if (dispItem == GCDS.CAL_COREEVENTS)
                return "&#9723; " + text;
            else
                return text;
        }

    }

    public class VAISNAVADAY: GSCore
    {
        // date
        public GregorianDateTime date;
        // moon times
        public GCHourTime moonrise;
        public GCHourTime moonset;
        // astronomical data from astro-sub-layer
        public GCAstroData astrodata;

        public int BiasMinutes;

        //
        // day events and fast
        public List<VAISNAVAEVENT> dayEvents;
        public int nFeasting;
        public int nFastID;

        //
        // Ekadasi data
        //
        public int nMahadvadasiID;
        public String ekadasi_vrata_name;
        public bool ekadasi_parana;
        public double eparana_time1, eparana_time2;
        public int eparana_type1, eparana_type2;

        //
        // Sankranti data
        public int sankranti_zodiac;
        public GregorianDateTime sankranti_day;

        // Ksaya and Vridhi data
        //
        public int ksayaTithi = -1;
        public int ksayaMasa = -1;
        public int vriddhiDayNo = 1;


        public VAISNAVADAY()
        {
            nFastID = FastType.FAST_NULL;
            nMahadvadasiID = MahadvadasiType.EV_NULL;
            ekadasi_parana = false;
            ekadasi_vrata_name = "";
            eparana_time1 = eparana_time2 = 0.0;
            eparana_type1 = eparana_type2 = EkadasiParanaType.EP_TYPE_NULL;
            sankranti_zodiac = -1;
            BiasMinutes = 0;
            moonrise.SetValue(0);
            moonset.SetValue(0);
            dayEvents = new List<VAISNAVAEVENT>();
        }

        public void Clear()
        {
            // init
            nFastID = FastType.FAST_NULL;
            nFeasting = FeastType.FEAST_NULL;
            nMahadvadasiID = MahadvadasiType.EV_NULL;
            ekadasi_parana = false;
            ekadasi_vrata_name = "";
            eparana_time1 = eparana_time2 = 0.0;
            sankranti_zodiac = -1;
            sankranti_day = null;
            //dayEvents.Clear();
            //moonset.SetValue(0);
            //moonrise.SetValue(0);
            //nDST = 0;
        }

        public override GSCore GetPropertyValue(string Token)
        {
            if (Token.Equals("date"))
            {
                return date;
            }
            else if (Token.Equals("astro"))
            {
                return astrodata;
            }
            else if (Token.Equals("dateHumanName"))
            {
                return new GSString(GregorianDateTime.GetDateTextWithTodayExt(date));
            }
            else if (Token.Equals("nDST"))
                return new GSNumber(BiasMinutes);
            else if (Token.Equals("events"))
            {
                GSList list = new GSList();
                list.Parts.AddRange(dayEvents);
                return list;
            }
            else if (Token.Equals("visibleEvents"))
            {
                GSList list = new GSList();
                list.Parts.AddRange(VisibleEvents);
                return list;
            }
            else if (Token.Equals("htmlDayColor"))
            {
                return new GSString(TResultCalendar.getDayBkgColorCode(this));
            }
            else if (Token.Equals("dstSignature"))
            {
                return new GSString(GCStrings.GetDSTSignature(BiasMinutes));
            }
            else if (Token.Equals("tithiNameExt"))
            {
                return new GSString(GetFullTithiName());
            }
            else if (Token.Equals("isWeekend"))
            {
                return new GSBoolean(date.dayOfWeek == 6 || date.dayOfWeek == 0);
            }
            else if (Token.Equals("fastType"))
                return new GSNumber(nFastID);
            else if (Token.Equals("fastTypeMark"))
                return new GSString(nFastID != 0 ? "*" : " ");
            else if (Token.Equals("ekadasiParana"))
                return new GSBoolean(ekadasi_parana);
            else if (Token.Equals("ekadasiParanaStart"))
            {
                GregorianDateTime gdt = new GregorianDateTime(date);
                gdt.shour = eparana_time1;
                return gdt;
            }
            else if (Token.Equals("ekadasiParanaEnd"))
            {
                GregorianDateTime gdt = new GregorianDateTime(date);
                gdt.shour = eparana_time2;
                return gdt;
            }
            else if (Token.Equals("hasParanaStart"))
                return new GSBoolean(eparana_time1 >= 0.0);
            else if (Token.Equals("hasParanaEnd"))
                return new GSBoolean(eparana_time2 >= 0.0);
            else if (Token.Equals("sankrantiZodiac"))
                return new GSNumber(sankranti_zodiac);
            else if (Token.Equals("sankrantiDateTime"))
                return sankranti_day;
            else if (Token.Equals("ksayaTithi"))
                return new GSNumber(ksayaTithi);
            else if (Token.Equals("ksayaMasa"))
                return new GSNumber(ksayaMasa);
            else
            {
                return base.GetPropertyValue(Token);
            }
        }

        public List<VAISNAVAEVENT> VisibleEvents
        {
            get
            {
                List<VAISNAVAEVENT> ve = new List<VAISNAVAEVENT>();
                foreach (VAISNAVAEVENT ed in dayEvents)
                {
                    int disp = ed.dispItem;
                    if (ed.dispItem != 0 && (disp == -1 || GCDisplaySettings.getValue(disp) != 0))
                    {
                        ve.Add(ed);
                    }
                }

                return ve;
            }
        }

        public bool GetTithiTimeRange(GCEarthData earth, out GregorianDateTime from, out GregorianDateTime to)
        {
            GregorianDateTime start = new GregorianDateTime();

            start.Set(date);
            start.shour = astrodata.sunRise.TotalDays;

            GCTithi.GetNextTithiStart(earth, start, out to);
            GCTithi.GetPrevTithiStart(earth, start, out from);

            return true;

        }

        public bool GetNaksatraTimeRange(GCEarthData earth, out GregorianDateTime from, out GregorianDateTime to)
        {
            GregorianDateTime start = new GregorianDateTime();

            start.Set(date);
            start.shour = astrodata.sunRise.TotalDays;

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
                        GCStrings.GetDSTSignature(BiasMinutes));
                else
                    str = string.Format("{0} {1:00}:{2:00} - {3:00}:{4:00} ({5})", GCStrings.getString(60),
                        h1, m1, h2, m2, GCStrings.GetDSTSignature(BiasMinutes));
            }
            else if (eparana_time1 >= 0.0)
            {
                if (GCDisplaySettings.getValue(50) == 1)
                    str = string.Format("{0} {1:00}:{2:00} ({3}) {4}", GCStrings.getString(61),
                        h1, m1, GCEkadasi.GetParanaReasonText(eparana_type1), GCStrings.GetDSTSignature(BiasMinutes));
                else
                    str = string.Format("{0} {1:00}:{2:00} ({3})", GCStrings.getString(61),
                        h1, m1, GCStrings.GetDSTSignature(BiasMinutes));
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
                format = format.Replace("{day}", date.day.ToString());
            if (format.IndexOf("{month}") >= 0)
                format = format.Replace("{month}", date.month.ToString());
            if (format.IndexOf("{monthAbr}") >= 0)
                format = format.Replace("{monthAbr}", GregorianDateTime.GetMonthName(date.month));
            if (format.IndexOf("{monthName}") >= 0)
                format = format.Replace("{monthName}", GregorianDateTime.GetMonthName(date.month));
            if (format.IndexOf("{year}") >= 0)
                format = format.Replace("{year}", date.year.ToString());
            if (format.IndexOf("{hour}") >= 0)
                format = format.Replace("{hour}", date.GetHour().ToString("D2"));
            if (format.IndexOf("{min}") >= 0)
                format = format.Replace("{min}", date.GetMinute().ToString("D2"));
            if (format.IndexOf("{minRound}") >= 0)
                format = format.Replace("{minRound}", date.GetMinuteRound().ToString("D2"));
            if (format.IndexOf("{sec}") >= 0)
                format = format.Replace("{sec}", date.GetSecond().ToString("D2"));

            if (format.IndexOf("{masaName}") >= 0)
                format = format.Replace("{masaName}", GCMasa.GetName(astrodata.Masa));
            if (format.IndexOf("{gaurabdaYear}") >= 0)
                format = format.Replace("{gaurabdaYear}", astrodata.GaurabdaYear.ToString());
            if (format.IndexOf("{tithiName}") >= 0)
                format = format.Replace("{tithiName}", GCTithi.GetName(astrodata.sunRise.Tithi));
            if (format.IndexOf("{prevTithiName}") >= 0)
                format = format.Replace("{prevTithiName}", GCTithi.GetName((astrodata.sunRise.Tithi + 29) % 30));
            if (format.IndexOf("{nextTithiName}") >= 0)
                format = format.Replace("{nextTithiName}", GCTithi.GetName((astrodata.sunRise.Tithi + 1) % 30));
            if (format.IndexOf("{paksaName}") >= 0)
                format = format.Replace("{paksaName}", GCPaksa.GetName(astrodata.sunRise.Paksa));
            if (format.IndexOf("{yogaName}") >= 0)
                format = format.Replace("{yogaName}", GCYoga.GetName(astrodata.sunRise.Yoga));
            if (format.IndexOf("{naksatraName}") >= 0)
                format = format.Replace("{naksatraName}", GCNaksatra.GetName(astrodata.sunRise.Naksatra));
            if (format.IndexOf("{naksatraElapse}") >= 0)
                format = format.Replace("{naksatraElapse}", astrodata.sunRise.NaksatraElapse.ToString("P2"));
            if (format.IndexOf("{naksatraPada}") >= 0)
                format = format.Replace("{naksatraPada}", GCNaksatra.GetPadaText(astrodata.sunRise.NaksatraPada));

            if (format.IndexOf("{sankranti.day}") >= 0)
                format = format.Replace("{sankranti.day}", sankranti_day.day.ToString());
            if (format.IndexOf("{sankranti.month}") >= 0)
                format = format.Replace("{sankranti.month}", sankranti_day.month.ToString());
            if (format.IndexOf("{sankranti.monthAbr}") >= 0)
                format = format.Replace("{sankranti.monthAbr}", GregorianDateTime.GetMonthName(sankranti_day.month));
            if (format.IndexOf("{sankranti.monthName}") >= 0)
                format = format.Replace("{sankranti.monthName}", GregorianDateTime.GetMonthName(sankranti_day.month));
            if (format.IndexOf("{sankranti.hour}") >= 0)
                format = format.Replace("{sankranti.hour}", sankranti_day.GetHour().ToString("D2"));
            if (format.IndexOf("{sankranti.min}") >= 0)
                format = format.Replace("{sankranti.min}", sankranti_day.GetMinute().ToString("D2"));
            if (format.IndexOf("{sankranti.minRound}") >= 0)
                format = format.Replace("{sankranti.minRound}", sankranti_day.GetMinuteRound().ToString("D2"));
            if (format.IndexOf("{sankranti.sec}") >= 0)
                format = format.Replace("{sankranti.sec}", sankranti_day.GetSecond().ToString("D2"));
            if (format.IndexOf("{sankranti.rasiNameEn}") >= 0)
                format = format.Replace("{sankranti.rasiNameEn}", GCRasi.GetNameEn(sankranti_zodiac));
            if (format.IndexOf("{sankranti.rasiName}") >= 0)
                format = format.Replace("{sankranti.rasiName}", GCRasi.GetName(sankranti_zodiac));

            if (format.IndexOf("{dstSig}") >= 0)
                format = format.Replace("{dstSig}", GCStrings.GetDSTSignature(BiasMinutes));

            if (format.IndexOf("{moonRiseTime}") >= 0)
                format = format.Replace("{moonRiseTime}", moonrise.ToShortTimeString());
            if (format.IndexOf("{moonSetTime}") >= 0)
                format = format.Replace("{moonSetTime}", moonset.ToShortTimeString());
            if (format.IndexOf("{moonRasiName}") >= 0)
                format = format.Replace("{moonRasiName}", GCRasi.GetName(astrodata.sunRise.RasiOfMoon));
            if (format.IndexOf("{moonRasiNameEn}") >= 0)
                format = format.Replace("{moonRasiNameEn}", GCRasi.GetNameEn(astrodata.sunRise.RasiOfMoon));

            if (args == null || args.Length == 0)
                return format.ToString();
            else
                return string.Format(format.ToString(), args);
        }

        public String GetFullTithiName()
        {
            string str;
            str = GCTithi.GetName(astrodata.sunRise.Tithi);

            if (HasExtraFastingNote())
            {
                str = string.Format("{0} {1}", str, GetExtraFastingNote());
            }

            return str;
        }

        public bool HasExtraFastingNote()
        {
            int t = astrodata.sunRise.Tithi;
            if ((t == 10) || (t == 25) || (t == 11) || (t == 26))
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
                if (nMahadvadasiID == MahadvadasiType.EV_NULL)
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
