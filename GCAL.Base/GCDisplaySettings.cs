using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;

namespace GCAL.Base
{
    public class CShowSetting
    {
        public int val;
        public int old_val;
        public string sig;
        public string text;

        public CShowSetting(int a, int b, string s, string t)
        {
            val = a;
            old_val = b;
            sig = s;
            text = t;
        }
    }

    public class GCDisplaySettings
    {
        protected static CShowSetting[] gss = {
	        new CShowSetting(0, 0, "ARTI", "Tithi at arunodaya"),//0
	        new CShowSetting(0, 0, "ARTM", "Arunodaya Time"),//1
	        new CShowSetting(0, 0, "SRTM", "Sunrise Time"),//2
	        new CShowSetting(0, 0, "SSTM", "Sunset Time"),//3
	        new CShowSetting(0, 0, "MRTM", "Moonrise Time"),//4
	        new CShowSetting(0, 0, "MSTM", "Moonset Time"),//5
	        new CShowSetting(1, 1, "FEST", "Festivals"),//6
	        new CShowSetting(0, 0, "KSAY", "Info about ksaya tithi"),//7
	        new CShowSetting(0, 0, "VRDH", "Info about vriddhi tithi"),//8
	        new CShowSetting(0, 0, "SLON", "Sun Longitude"),//9
	        new CShowSetting(0, 0, "MLON", "Moon Longitude"),//10
	        new CShowSetting(0, 0, "AYAN", "Ayanamsha value"),//11
	        new CShowSetting(0, 0, "JDAY", "Julian Day"),//12
	        new CShowSetting(0, 0, "CPUR", "Caturmasya Purnima System"), //13
	        new CShowSetting(1, 1, "CPRA", "Caturmasya Pratipat System"), //14
	        new CShowSetting(0, 0, "CEKA", "Caturmasya Ekadasi System"), //15
	        new CShowSetting(1, 1, "SANI", "Sankranti Info"), //16
	        new CShowSetting(1, 1, "EKAI", "Ekadasi Info"), //17
	        new CShowSetting(1, 1, "VHDR", "Masa Header Info"), //18
	        new CShowSetting(0, 0, "PHDR", "Month Header Info"), //19
	        new CShowSetting(0, 0, "EDNS", "Do not show empty days"), //20
	        new CShowSetting(0, 0, "SBEM", "Show begining of masa"), //21
	        new CShowSetting(1, 1, "F000", "Appearance days of the Lord"),//22
	        new CShowSetting(1, 1, "F001", "Events in the pastimes of the Lord"),//23
	        new CShowSetting(1, 1, "F002", "App, Disapp of Recent Acaryas"),//24
	        new CShowSetting(1, 1, "F003", "App, Disapp of Mahaprabhu's Associates and Other Acaryas"),//25
	        new CShowSetting(1, 1, "F004", "ISKCON's Historical Events"),//26
	        new CShowSetting(1, 1, "F005", "Bengal-specific Holidays"),//27
	        new CShowSetting(1, 1, "F006", "My Personal Events"), //28
	        /* BEGIN GCAL 1.4.3 */
	        new CShowSetting(1, 1, "TSSR", "Todat Sunrise"),  //29 Today sunrise
	        new CShowSetting(1, 1, "TSSN", "Today Noon"),  //30 today noon
	        new CShowSetting(1, 1, "TSSS", "Today Sunset"),  //31 today sunset
	        new CShowSetting(0, 0, "TSAN", "Sandhya Times"),  //32 today + sandhya times
	        new CShowSetting(1, 1, "TSIN", "Sunrise Info"),  //33 today sunrise info
	        new CShowSetting(0, 0, "ASIN", "Noon Time"),  //34 astro - noon time
	        new CShowSetting(1, 1, "NDST", "Notice about DST"), //35 notice about the change of the DST
	        new CShowSetting(1, 1, "DNAK", "Naksatra"), // 36 naksatra info for each day
	        new CShowSetting(1, 1, "DYOG", "Yoga"), //37 yoga info for each day
	        new CShowSetting(1, 1, "FFLG", "Fasting Flag"),//38
	        new CShowSetting(1, 1, "DPAK", "Paksa Info"),//39 paksa info
	        new CShowSetting(0, 0, "FDIW", "First Day in Week"),//40 first day in week
	        new CShowSetting(0, 0, "DRAS", "Rasi"), //41 moon rasi for each calendar day
	        new CShowSetting(0, 0, "OSFA", "Old Style Fasting text"), //42 old style fasting text
	        new CShowSetting(0, 0, "MLNT", "Name of month - type"), //43 month type name 0-vaisnava,1-bengal,2-hindu,3-vedic
	        /* END GCAL 1.4.3 */
	        new CShowSetting(0, 0, "EDBL", "Editable Default Events"), //44 editable default events
	        new CShowSetting(0, 0, "TSBM", "Today Brahma Muhurta"),     //45 brahma muhurta in today screen
	        new CShowSetting(0, 0, "TROM", "Today Rasi of the Moon"), // 46 rasi of the moon in today screen
	        new CShowSetting(0, 0, "TNPD", "Today Naksatra Pada details"), // 47 naksatra pada details in today screen
	        new CShowSetting(0, 0, "ADCS", "Child Names Suggestions"), // 48 child name suggestions in Appearance Day screen
	        new CShowSetting(0, 0, "MNFO", "Masa Name Format"), // 49 format of masa name
	        new CShowSetting(0, 0, "EPDR", "Ekadasi Parana details"), // 50 ekadasi parana details
	        new CShowSetting(0, 0, "ANIV", "Aniversary show format"), // 51 format of aniversary info
	        new CShowSetting(1, 1, "CE01", "Sun events"), // 52
	        new CShowSetting(1, 1, "CE02", "Tithi events"), //53
	        new CShowSetting(1, 1, "CE03", "Naksatra Events"), //54
	        new CShowSetting(1, 1, "CE04", "Sankranti Events"),//55
	        new CShowSetting(1, 1, "CE05", "Conjunction Events"),//56
	        new CShowSetting(0, 0, "CE06", "Rahu kalam"), //57
	        new CShowSetting(0, 0, "CE07", "Yama ghanti"), //58
	        new CShowSetting(0, 0, "CE08", "Guli kalam"), //59
	        new CShowSetting(0, 0, "CE09", "Moon events"), //60
	        new CShowSetting(0, 0, "CE10", "Moon rasi"), //61
	        new CShowSetting(0, 0, "CE11", "Ascendent"), //62
	        new CShowSetting(1, 1, "CE12", "Sort results core events"),//63
	        new CShowSetting(0, 0, "CE13", "Abhijit Muhurta"), //64
	        new CShowSetting(0, 0, "CE14", "Yoga Events"), //65
	        new CShowSetting(1, 1, "CSYS", "Caturmasya System"), //66
            new CShowSetting(0, 0, "CCEL", "Core Events in Calendar"),
	        new CShowSetting(0, 0, null, null)
        };

        public static int getCount()
        {
            int i = 0;
            while (gss[i].text != null)
                i++;
            return i;
        }
        static int getCountChanged()
        {
            int i, count = 0;
            int size = GCDisplaySettings.getCount();

            for (i = 0; i < size; i++)
            {
                if (gss[i].val != gss[i].old_val)
                    count++;
                gss[i].old_val = gss[i].val;
            }

            return count;
        }

        public static string getSettingName(int i)
        {
            return gss[i].text;
        }

        public static int getValue(int i)
        {
            return gss[i].val;
        }

        public static bool getBoolValue(int i)
        {
            return gss[i].val != 0;
        }
        public static void setValue(int i, int val)
        {
            gss[i].val = val;
            gss[i].old_val = val;
        }

        public static void setBoolValue(int i, bool val)
        {
            gss[i].val = val ? 1 : 0;
            gss[i].old_val = gss[i].val;
        }

        static void setValue(string pszSign, int val)
        {
            int i, max = GCDisplaySettings.getCount();

            for (i = 0; i < max; i++)
            {
                if (gss[i].sig.Equals(pszSign))
                {
                    gss[i].val = val;
                    gss[i].old_val = val;
                    break;
                }
            }
        }

        public static void readFile(string psz)
        {
            if (!File.Exists(psz))
                return;

            using (StreamReader sr = new StreamReader(psz))
            {
                string s = null;
                while ((s = sr.ReadLine()) != null)
                {
                    if (s.StartsWith("#"))
                        continue;

                    string[] ps = s.Split('=');
                    if (ps.Length == 2)
                    {
                        GCDisplaySettings.setValue(int.Parse(ps[0]), int.Parse(ps[1]));
                    }
                }
            }
        }



        public static void writeFile(string fileName)
        {
            using (StreamWriter sw = new StreamWriter(fileName))
            {
                for (int i = 0; i < gss.Length; i++ )
                {
                    sw.WriteLine("{0}={1}", i, gss[i].val);
                }
            }
        }

        protected static List<int[]> SettingsStack = new List<int[]>();

        public static void Push()
        {
            int[] arr = new int[gss.Length];
            for (int i = 0; i < gss.Length; i++)
            {
                arr[i] = gss[i].val;
            }

            SettingsStack.Add(arr);
        }

        public static void Pop()
        {
            if (SettingsStack.Count > 0)
            {
                int[] arr = SettingsStack[SettingsStack.Count - 1];
                for (int i = 0; i < arr.Length; i++)
                {
                    gss[i].val = arr[i];
                }
                SettingsStack.RemoveAt(SettingsStack.Count - 1);
            }
        }
    }

    public sealed class GCDS
    {
        public static readonly int DISP_ALWAYS = -1;
        public static readonly int CAL_ARUN_TIME = 1;
        public static readonly int CAL_ARUN_TITHI = 0;
        public static readonly int CAL_SUN_RISE = 2;
        public static readonly int CAL_SUN_SANDHYA = 34;
        public static readonly int CAL_BRAHMA_MUHURTA = 3;
        public static readonly int CAL_MOON_RISE = 4;
        public static readonly int CAL_MOON_SET = 5;
        public static readonly int CAL_KSAYA = 7;
        public static readonly int CAL_VRDDHI = 8;
        public static readonly int CAL_SUN_LONG = 9;
        public static readonly int CAL_MOON_LONG = 10;
        public static readonly int CAL_AYANAMSHA = 11;
        public static readonly int CAL_JULIAN = 12;
        public static readonly int CATURMASYA_SYSTEM = 66;
        public static readonly int CATURMASYA_PURNIMA = 13;
        public static readonly int CATURMASYA_PRATIPAT = 14;
        public static readonly int CATURMASYA_EKADASI = 15;
        public static readonly int CAL_SANKRANTI = 16;
        public static readonly int CAL_EKADASI_PARANA = 17;
        public static readonly int CAL_HEADER_MASA = 18;
        public static readonly int CAL_HEADER_MONTH = 19;
        public static readonly int CAL_MASA_CHANGE = 21;
        public static readonly int CAL_FEST_0 = 22;
        public static readonly int CAL_FEST_1 = 23;
        public static readonly int CAL_FEST_2 = 24;
        public static readonly int CAL_FEST_3 = 25;
        public static readonly int CAL_FEST_4 = 26;
        public static readonly int CAL_FEST_5 = 27;
        public static readonly int CAL_FEST_6 = 28;
        public static readonly int CAL_DST_CHANGE = 35;
        public static readonly int GENERAL_FIRST_DOW = 40;
        public static readonly int APP_CHILDNAMES = 48;
        public static readonly int GENERAL_MASA_FORMAT = 49;
        public static readonly int GENERAL_ANNIVERSARY_FMT = 51;
        public static readonly int COREEVENTS_SUN = 52;
        public static readonly int COREEVENTS_TITHI = 53;
        public static readonly int COREEVENTS_NAKSATRA = 54;
        public static readonly int COREEVENTS_SANKRANTI = 55;
        public static readonly int COREEVENTS_CONJUNCTION = 56;
        public static readonly int COREEVENTS_RAHUKALAM = 57;
        public static readonly int COREEVENTS_YAMAGHANTI = 58;
        public static readonly int COREEVENTS_GULIKALAM = 59;
        public static readonly int COREEVENTS_MOON = 60;
        public static readonly int COREEVENTS_MOONRASI = 61;
        public static readonly int COREEVENTS_ASCENDENT = 62;
        public static readonly int COREEVENTS_SORT = 63;
        public static readonly int COREEVENTS_ABHIJIT_MUHURTA = 64;
        public static readonly int COREEVENTS_YOGA = 65;
        public static readonly int CAL_COREEVENTS = 67;
    };

    public sealed class DisplayPriorities
    {
        public static readonly int PRIO_MAHADVADASI = 10;
        public static readonly int PRIO_EKADASI = 20;
        public static readonly int PRIO_EKADASI_PARANA = 90;
        public static readonly int PRIO_FESTIVALS_0 = 100;
        public static readonly int PRIO_FESTIVALS_1 = 200;
        public static readonly int PRIO_FESTIVALS_2 = 300;
        public static readonly int PRIO_FESTIVALS_3 = 400;
        public static readonly int PRIO_FESTIVALS_4 = 500;
        public static readonly int PRIO_FESTIVALS_5 = 600;
        public static readonly int PRIO_FESTIVALS_6 = 700;
        public static readonly int PRIO_FASTING = 900;
        public static readonly int PRIO_SANKRANTI = 920;
        public static readonly int PRIO_MASA_CHANGE = 940;
        public static readonly int PRIO_DST_CHANGE = 950;
        public static readonly int PRIO_KSAYA = 965;
        public static readonly int PRIO_CM_CONT = 971;
        public static readonly int PRIO_CM_DAY = 972;
        public static readonly int PRIO_CM_DAYNOTE = 973;
        public static readonly int PRIO_ARUN = 975;
        public static readonly int PRIO_SUN = 980;
        public static readonly int PRIO_MOON = 990;
        public static readonly int PRIO_CORE_ASTRO = 995;
        public static readonly int PRIO_ASTRO = 1000;

    };
}
