using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GCAL.Base
{
    public class CLocationList
    {
        public static List<CLocation> locationList;
        public static bool m_bModified;

        public static bool IsModified()
        {
            return m_bModified;
        }


        public static bool OpenFile(string pszFileList)
        {
            TFileRichList file = new TFileRichList();
            CLocation loc = null;
            int notNullCountry = 0;
            locationList = new List<CLocation>();

            if (!TFile.FileExists(pszFileList))
            {
                TFile.CreateFileFromResource("locations.rl", pszFileList);
            }

            // try to open
            if (file.ReadFile(pszFileList))
            {
                while (file.ReadLine() > 0)
                {
                    loc = new CLocation();
                    loc.cityName = file.GetField(0);
                    loc.countryName = file.GetField(1);
                    if (loc.countryName.Length > 0)
                        notNullCountry++;
                    loc.longitudeDeg = double.Parse(file.GetField(2));
                    loc.latitudeDeg = double.Parse(file.GetField(3));
                    loc.offsetUtcHours = double.Parse(file.GetField(4));
                    loc.timezoneId = int.Parse(file.GetField(5));

                    locationList.Add(loc);
                }
            }

            return true;
        }


        public static bool SaveAs(string lpszFileName, int nType)
        {
            String str, strTemp;
            TFile f = new TFile();
            int i, ni;

            switch (nType)
            {
                case 1:
                    f.WriteString("<xml>\n");
                    f.WriteString("\t<countries>\n");
                    ni = TCountry.GetCountryCount();
                    for (i = 0; i < ni; i++)
                    {
                        str = string.Format("\t<ccn country=\"{0}\" continent=\"{1}\" />\n", TCountry.GetCountryNameByIndex(i),
                            TCountry.GetCountryContinentNameByIndex(i));
                        f.WriteString(str);
                    }
                    f.WriteString("\t</countries>\n");
                    f.WriteString("\t<dsts>\n");
                    ni = TTimeZone.GetTimeZoneCount();
                    for (i = 1; i < ni; i++)
                    {
                        str = TTimeZone.GetXMLString(i);
                        f.WriteString(str);
                    }
                    f.WriteString("\t</dsts>\n");
                    f.WriteString("\t<cities>\n");
                    for (i = 0; i < locationList.Count(); i++)
                    {
                        CLocation lc = locationList[i];
                        str = string.Format("\t<loc city=\"{0}\" lon=\"{1}\" lat=\"{2}\" tzone=\"{3}\"\n\t\tcountry=\"{4}\" />\n",
                            lc.cityName, lc.longitudeDeg, lc.latitudeDeg,
                            lc.offsetUtcHours, lc.countryName);
                        str.Replace("&", "&amp;");
                        f.WriteString(str);
                    }
                    f.WriteString("\t</cities>\n");
                    f.WriteString("</xml>");
                    break;
                case 2:
                    f.WriteString("Countries:\n");
                    ni = TCountry.GetCountryCount();
                    for (i = 0; i < ni; i++)
                    {
                        str = string.Format("{0}, {1}\n", TCountry.GetCountryNameByIndex(i),
                            TCountry.GetCountryContinentNameByIndex(i));
                        f.WriteString(str);
                    }
                    f.WriteString("Daylight Saving Time Systems:\n");
                    ni = TTimeZone.GetTimeZoneCount();
                    for (i = 1; i < ni; i++)
                    {
                        str = string.Format("\t{0}\n", TTimeZone.GetTimeZoneName(i));
                        f.WriteString(str);
                    }
                    f.WriteString("Cities:\n");
                    for (i = 0; i < locationList.Count(); i++)
                    {
                        CLocation lc = locationList[i];
                        str = string.Format("\t{0} {1} {2} {3} {4}\n",
                            lc.cityName,
                            lc.countryName,
                            lc.longitudeDeg,
                            lc.latitudeDeg,
                            lc.offsetUtcHours);
                        f.WriteString(str);
                    }
                    break;
                case 3:
                    for (i = 0; i < locationList.Count(); i++)
                    {
                        CLocation lc = locationList[i];
                        // city
                        f.WriteString("@city=");
                        f.WriteString(lc.cityName);
                        f.WriteString("\n");

                        f.WriteString("@country=");
                        f.WriteString(lc.countryName);
                        f.WriteString("\n");

                        f.WriteString("@lat=");
                        strTemp = string.Format("{0}", lc.latitudeDeg);
                        f.WriteString(strTemp);
                        f.WriteString("\n");

                        f.WriteString("@long=");
                        strTemp = string.Format("{0}", lc.longitudeDeg);
                        f.WriteString(strTemp);
                        f.WriteString("\n");

                        f.WriteString("@timezone=");
                        strTemp = string.Format("{0}", lc.offsetUtcHours);
                        f.WriteString(strTemp);
                        f.WriteString("\n");

                        f.WriteString("@dst=");
                        strTemp = string.Format("{0}", lc.timezoneId);
                        f.WriteString(strTemp);
                        f.WriteString("\n@create\n\n");

                    }
                    break;
                case 4:
                    for (i = 0; i < locationList.Count(); i++)
                    {
                        CLocation lc = locationList[i];
                        strTemp = string.Format("26700 {0}|{1}|{2}|{3}|{4}|{5}\n", lc.cityName, lc.countryName,
                            lc.longitudeDeg,
                            lc.latitudeDeg, lc.offsetUtcHours, lc.timezoneId);
                        f.WriteString(strTemp);
                    }
                    break;
                default:
                    break;
            }

            f.WriteFile(lpszFileName);

            return true;
        }

        public static bool ImportFile(string pszFile, bool bDeleteCurrent)
        {
            if (bDeleteCurrent)
            {
                RemoveAll();
            }

            m_bModified = true;

            return OpenFile(pszFile);
        }

        public static void RemoveAll()
        {
            locationList.Clear();
            m_bModified = true;
        }
        public static void Add(CLocation loc)
        {
            locationList.Add(loc);
            m_bModified = true;
        }
        public static void RemoveAt(int index)
        {
            locationList.RemoveAt(index);
            m_bModified = true;
        }

        public static void InitInternal(string fileName)
        {
            TFile.DeleteFile(fileName);
            OpenFile(fileName);
        }

        public static int RenameCountry(string pszOld, string pszNew)
        {
            for (int i = 0; i < locationList.Count(); i++)
            {
                CLocation L = locationList[i];
                if (L.countryName.Equals(pszOld))
                    L.countryName = pszNew;
            }
            return 1;
        }

        public static int LocationCountForCountry(string countryName)
        {
            int count = 0;
            foreach (CLocation location in locationList)
            {
                if (location.countryName.Equals(countryName))
                    count++;
            }
            return count;
        }

        public CLocationList()
        {
            m_bModified = true;
        }


        public static int LocationCount()
        {
            return locationList.Count;
        }

        public static CLocation LocationAtIndex(int index)
        {
            return locationList[index];
        }
    }
}
