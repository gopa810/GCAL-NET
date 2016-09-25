using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace GCAL.Base
{
    public class TTimeZone
    {
        public string Name;
        public int OffsetMinutes;
        public int BiasMinutes;
        public UInt32 val;
        public float latA;
        public float latB;
        public float lonA;
        public float lonB;
        public int TimezoneId;

        public static List<TTimeZone> gzone = new List<TTimeZone>();

        public static bool Modified = false;

        public override string ToString()
        {
            return GetTimeZoneOffsetText(OffsetMinutes/60.0) + " " + Name;
        }

        public static void SaveFile(string pszFile)
        {
            using (StreamWriter sw = new StreamWriter(pszFile))
            {
                foreach (TTimeZone timezone in gzone)
                {
                    sw.WriteLine("{0}|{1}|{2}|{3}|{4}|{5}|{6}|{7}|{8}",
                        timezone.Name.Replace('|', ' '), timezone.OffsetMinutes,
                        timezone.BiasMinutes, timezone.val,
                        timezone.latA, timezone.latB,
                        timezone.lonA, timezone.lonB,
                        timezone.TimezoneId);
                }
            }
        }

        public static int OpenFile(string pszFile)
        {
            TTimeZone pce;

            if (!File.Exists(pszFile))
            {
                File.WriteAllBytes(pszFile, Properties.Resources.timezones);
            }

            using (StreamReader sr = new StreamReader(pszFile))
            {
                string line;
                TTimeZone.gzone = new List<TTimeZone>();
                while ((line = sr.ReadLine()) != null)
                {
                    string[] fields = line.Split('|');
                    if (fields.Length < 9)
                        continue;

                    pce = new TTimeZone();
                    pce.Name = fields[0];
                    pce.OffsetMinutes = int.Parse(fields[1]);
                    pce.BiasMinutes = int.Parse(fields[2]);
                    pce.val = UInt32.Parse(fields[3]);
                    pce.latA = float.Parse(fields[4]);
                    pce.latB = float.Parse(fields[5]);
                    pce.lonA = float.Parse(fields[6]);
                    pce.lonB = float.Parse(fields[7]);
                    pce.TimezoneId = int.Parse(fields[8]);
                    gzone.Add(pce);

                    line = sr.ReadLine();
                }

            }

            return gzone.Count;
        }


        public static int GetTimeZoneCount()
        {
            return gzone.Count;
        }

        public static int ID2INDEX(int _id)
        {
            int i, m;
            for (i = 0, m = gzone.Count; i < m; i++)
            {
                if (_id == gzone[i].TimezoneId)
                    return i;
            }

            return 0;
        }

        public static TTimeZone GetTimeZoneById(int idx)
        {
            foreach (TTimeZone tzone in gzone)
            {
                if (tzone.TimezoneId == idx)
                    return tzone;
            }
            return null;
        }

        public static string GetTimeZoneName(int nIndex)
        {
            return gzone[ID2INDEX(nIndex)].Name;
        }

        public static double GetTimeZoneOffsetHours(int nIndex)
        {
            return Convert.ToDouble(gzone[ID2INDEX(nIndex)].OffsetMinutes) / 60.0;
        }

        public static int GetTimeZoneOffsetMinutes(int nIndex)
        {
            return gzone[ID2INDEX(nIndex)].OffsetMinutes;
        }

        public static UInt32[] ExpandVal(UInt32 val)
        {
            UInt32[] a = new UInt32[8];
            a[7] = val & 0xf;
            val >>= 4;
            a[6] = val & 0x3f;
            val >>= 6;
            a[5] = val & 0x3;
            val >>= 2;
            a[4] = val & 0xf;
            val >>= 4;
            a[3] = val & 0xf;
            val >>= 4;
            a[2] = val & 0x3f;
            val >>= 6;
            a[1] = val & 0x3;
            val >>= 2;
            a[0] = val & 0xf;
            return a;
        }

        public static string GetXMLString(int nIndex)
        {
            UInt32[] DSTtable;

            nIndex = ID2INDEX(nIndex);

            if (nIndex < 0 || nIndex >= GetTimeZoneCount())
                return "";

            DSTtable = ExpandVal(gzone[nIndex].val);

            return string.Format("\t<dayss name=\"{0}\" types=\"{1}\" months=\"{2}\" weeks=\"{3}\" days=\"{4}\"\n\t\ttypee=\"{5}\" monthe=\"{6}\" weeke=\"{7}\" daye=\"{8}\" shift=\"{9}\"/>\n"
                        , gzone[nIndex].Name, DSTtable[1], DSTtable[0], DSTtable[2], DSTtable[3],
                         DSTtable[5], DSTtable[4], DSTtable[6], DSTtable[7], gzone[nIndex].BiasMinutes);

        }

        public static int GetTimeZoneBias(int ndst)
        {
            return gzone[ID2INDEX(ndst)].BiasMinutes;
        }

        public static int GetDaylightTimeStartDate(int nDst, int nYear, ref GregorianDateTime vcStart)
        {
            UInt32[] a;
            nDst = ID2INDEX(nDst);
            a = ExpandVal(gzone[nDst].val);

            vcStart.day = 1;
            vcStart.month = Convert.ToInt32(a[0]);
            vcStart.year = nYear;
            if (a[1] == 1)
            {
                vcStart.day = (int)a[2];
            }
            else
            {
                if (a[2] == 5)
                {
                    vcStart.day = GregorianDateTime.GetMonthMaxDays(nYear, (int)a[0]);
                    vcStart.InitWeekDay();
                    while (vcStart.dayOfWeek != a[3])
                    {
                        vcStart.PreviousDay();
                        vcStart.dayOfWeek = (vcStart.dayOfWeek + 6) % 7;
                    }
                }
                else
                {
                    vcStart.day = 1;
                    vcStart.InitWeekDay();
                    while (vcStart.dayOfWeek != a[3])
                    {
                        vcStart.NextDay();
                        vcStart.dayOfWeek = (vcStart.dayOfWeek + 1) % 7;
                    }
                    vcStart.day += (int)a[2] * 7;
                }
            }
            vcStart.shour = 1 / 24.0;
            return 0;
        }

        public static int GetNormalTimeStartDate(int nDst, int nYear, ref GregorianDateTime vcStart)
        {
            vcStart.day = 1;
            vcStart.month = 10;
            vcStart.year = nYear;
            vcStart.shour = 3 / 24.0;
            return 0;
        }


        public static int GetID(string p)
        {
            int i = 0;
            int m = GetTimeZoneCount();
            foreach (TTimeZone tz in gzone)
            {
                if (tz.Name.Equals(p, StringComparison.CurrentCultureIgnoreCase))
                    return tz.TimezoneId;
            }
            foreach (TTimeZone tz in gzone)
            {
                if (tz.Name.Equals(p, StringComparison.CurrentCulture))
                    return tz.TimezoneId;
            }
            int.TryParse(p, out i);
            return i;
        }


        public static string GetTimeZoneOffsetText(double d)
        {
            int a4, a5;
            int sig;

            if (d < 0.0)
            {
                sig = -1;
                d = -d;
            }
            else
            {
                sig = 1;
            }
            a4 = GCMath.IntFloor(d);
            a5 = Convert.ToInt32((d - a4) * 60);

            return string.Format("{0}{1}:{2:00}", (sig > 0 ? '+' : '-'), a4, a5);
        }

        public static string GetTimeZoneOffsetTextArg(double d)
        {
            int a4, a5;
            int sig;

            if (d < 0.0)
            {
                sig = -1;
                d = -d;
            }
            else
            {
                sig = 1;
            }
            a4 = Convert.ToInt32(d);
            a5 = Convert.ToInt32((d - a4) * 60 + 0.5);

            return string.Format("{0}{1}{2:00}", a4, (sig > 0 ? 'E' : 'W'), a5);
        }


        // return values
        // 0 - DST is off, yesterday was off
        // 1 - DST is on, yesterday was off
        // 2 - DST is on, yesterday was on
        // 3 - DST is off, yesterday was on
        public static int determineDaylightChange(GregorianDateTime vc2, int nIndex)
        {
            int t2 = TTimeZone.determineDaylightStatus(vc2, nIndex);
            GregorianDateTime vc3 = new GregorianDateTime();
            vc3.Set(vc2);
            vc3.PreviousDay();
            int t1 = TTimeZone.determineDaylightStatus(vc3, nIndex);
            if (t1 != 0)
            {
                if (t2 != 0)
                    return 2;
                else
                    return 3;
            }
            else if (t2 != 0)
            {
                return 1;
            }
            else
                return 0;
        }

        // n - is order number of given day
        // x - is number of day in week (0-sunday, 1-monday, ..6-saturday)
        // if x >= 5, then is calculated whether day is after last x-day

        public static int is_n_xday(GregorianDateTime vc, UInt32 n, UInt32 x)
        {
            int[] xx = { 1, 7, 6, 5, 4, 3, 2 };

            int fdm, fxdm, nxdm, max;

            // prvy den mesiaca
            fdm = xx[(7 + vc.day - vc.dayOfWeek) % 7];

            // 1. x-day v mesiaci ma datum
            fxdm = xx[(fdm - x + 7) % 7];

            // n-ty x-day ma datum
            if ((n < 0) || (n >= 5))
            {
                nxdm = fxdm + 28;
                max = GregorianDateTime.GetMonthMaxDays(vc.year, vc.month);
                while (nxdm > max)
                {
                    nxdm -= 7;
                }
            }
            else
            {
                nxdm = Convert.ToInt32(fxdm + (n - 1) * 7);
            }

            return (vc.day >= nxdm) ? 1 : 0;
        }

        // This table has 8 items for each line:
        //  [0]: starting month
        //  [4]: ending month
        // 
        //  [1]: type of day, 0-day is given as n-th x-day of month, 1- day is given as DATE
        //  [2]: for [1]==1 this means day of month
        //     : for [1]==0 this order number of occurance of the given day (1,2,3,4 is acceptable, 5 means *last*)
        //  [3]: used only for [1]==0, and this means day of week (0=sunday,1=monday,2=tuesday,3=wednesday,...)
        //     : [1] to [3] are used for starting month
        //  [5] to [7] is used for ending month in the same way as [1] to [3] for starting month
        //
        // EXAMPLE: (first line)   3 0 5 0 10 0 5 0
        // [0] == 3, that means starting month is March
        // [1] == 0, that means starting system is (day of week)
        // [2] == 5, that would mean that we are looking for 5th occurance of given day in the month, but 5 here means,
        //           that we are looking for *LAST* occurance of given day
        // [3] == 0, this is *GIVEN DAY*, and it is SUNDAY
        //
        //         so, DST is starting on last sunday of March
        //
        // similarly analysed, DST is ending on last sunday of October
        //

        public static int GetDaylightBias(GregorianDateTime vc, UInt32 val)
        {
            UInt32[] DSTtable;
            int bias = 1;

            DSTtable = ExpandVal(val);

            if (vc.month == DSTtable[0])
            {
                if (DSTtable[1] == 0)
                    return is_n_xday(vc, DSTtable[2], DSTtable[3]) * bias;
                else
                    return (vc.day >= DSTtable[2]) ? bias : 0;
            }
            else if (vc.month == DSTtable[4])
            {
                if (DSTtable[5] == 0)
                    return (1 - is_n_xday(vc, DSTtable[6], DSTtable[7])) * bias;
                else
                    return (vc.day >= DSTtable[6]) ? 0 : bias;
            }
            else
            {
                if (DSTtable[0] > DSTtable[4])
                {
                    // zaciatocny mesiac ma vyssie cislo nez koncovy
                    // napr. pre australiu
                    if ((vc.month > DSTtable[0]) || (vc.month < DSTtable[4]))
                        return bias;
                }
                else
                {
                    // zaciatocny mesiac ma nizsie cislo nez koncovy
                    // usa, europa, asia
                    if ((vc.month > DSTtable[0]) && (vc.month < DSTtable[4]))
                        return bias;
                }

                return 0;
            }
        }


        public static int determineDaylightStatus(GregorianDateTime vc, int nIndex)
        {
            return GetDaylightBias(vc, TTimeZone.gzone[TTimeZone.ID2INDEX(nIndex)].val);
        }


        public string HumanDstText()
        {
            UInt32[] a;
            string ret;

            if (val == 0)
            {
                ret = GCStrings.getString(807);
            }
            else
            {
                a = TTimeZone.ExpandVal(val);

                ret = GCStrings.getString(808);
                // pre datumovy den
                if (a[1] == 1)
                {

                    ret += string.Format("from {0} {1} ", GCStrings.getString((int)a[2] + 810), GCStrings.getString((int)a[0] + 794));
                    //SetDlgItemText(IDC_STATIC_DST_INFO1, str);
                }
                else
                {
                    // pre tyzdenny den
                    ret += string.Format("from {0} {1} {2} ", GCStrings.getString((int)a[2] + 781), GCStrings.getString((int)a[3] + 787),
                        GCStrings.getString((int)a[0] + 794));
                    //SetDlgItemText(IDC_STATIC_DST_INFO1, str);
                }

                if (a[5] == 1)
                {
                    ret += string.Format("to {0} {1}", GCStrings.getString(810 + (int)a[6]), GCStrings.getString((int)a[4] + 794));
                    //SetDlgItemText(IDC_STATIC_DST_INFO2, str);
                }
                else
                {
                    // pre tyzdenny den
                    ret += string.Format("to {0} {1} {2}", GCStrings.getString((int)a[6] + 781),
                        GCStrings.getString((int)a[7] + 787), GCStrings.getString((int)a[4] + 794));
                    //SetDlgItemText(IDC_STATIC_DST_INFO2, str);
                }
            }

            return ret;
        }
    }
}
