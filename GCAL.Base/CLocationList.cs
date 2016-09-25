using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

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
            GCRichFileLine file = new GCRichFileLine();
            CLocation loc = null;
            int notNullCountry = 0;
            locationList = new List<CLocation>();

            if (!File.Exists(pszFileList))
            {
                File.WriteAllBytes(pszFileList, Properties.Resources.locations);
            }

            using(StreamReader sr = new StreamReader(pszFileList))
            {
                while (file.SetLine(sr.ReadLine()))
                {
                    loc = new CLocation();
                    loc.cityName = file[0];
                    loc.countryName = file[1];
                    if (loc.countryName.Length > 0)
                        notNullCountry++;
                    loc.longitudeDeg = double.Parse(file[2]);
                    loc.latitudeDeg = double.Parse(file[3]);
                    loc.offsetUtcHours = double.Parse(file[4]);
                    loc.timezoneId = int.Parse(file[5]);

                    locationList.Add(loc);
                }
            }

            return true;
        }


        public static bool SaveAs(string lpszFileName, int nType)
        {
            String str, strTemp;
            int i, ni;

            using (StreamWriter f = new StreamWriter(lpszFileName))
            {
                switch (nType)
                {
                    case 1:
                        f.Write("<xml>\n");
                        f.Write("\t<countries>\n");
                        ni = TCountry.GetCountryCount();
                        for (i = 0; i < ni; i++)
                        {
                            str = string.Format("\t<ccn country=\"{0}\" continent=\"{1}\" />\n", TCountry.GetCountryNameByIndex(i),
                                TCountry.GetCountryContinentNameByIndex(i));
                            f.Write(str);
                        }
                        f.Write("\t</countries>\n");
                        f.Write("\t<dsts>\n");
                        ni = TTimeZone.GetTimeZoneCount();
                        for (i = 1; i < ni; i++)
                        {
                            str = TTimeZone.GetXMLString(i);
                            f.Write(str);
                        }
                        f.Write("\t</dsts>\n");
                        f.Write("\t<cities>\n");
                        for (i = 0; i < locationList.Count(); i++)
                        {
                            CLocation lc = locationList[i];
                            str = string.Format("\t<loc city=\"{0}\" lon=\"{1}\" lat=\"{2}\" tzone=\"{3}\"\n\t\tcountry=\"{4}\" />\n",
                                lc.cityName, lc.longitudeDeg, lc.latitudeDeg,
                                lc.offsetUtcHours, lc.countryName);
                            str.Replace("&", "&amp;");
                            f.Write(str);
                        }
                        f.Write("\t</cities>\n");
                        f.Write("</xml>");
                        break;
                    case 2:
                        f.Write("Countries:\n");
                        ni = TCountry.GetCountryCount();
                        for (i = 0; i < ni; i++)
                        {
                            str = string.Format("{0}, {1}\n", TCountry.GetCountryNameByIndex(i),
                                TCountry.GetCountryContinentNameByIndex(i));
                            f.Write(str);
                        }
                        f.Write("Daylight Saving Time Systems:\n");
                        ni = TTimeZone.GetTimeZoneCount();
                        for (i = 1; i < ni; i++)
                        {
                            str = string.Format("\t{0}\n", TTimeZone.GetTimeZoneName(i));
                            f.Write(str);
                        }
                        f.Write("Cities:\n");
                        for (i = 0; i < locationList.Count(); i++)
                        {
                            CLocation lc = locationList[i];
                            str = string.Format("\t{0} {1} {2} {3} {4}\n",
                                lc.cityName,
                                lc.countryName,
                                lc.longitudeDeg,
                                lc.latitudeDeg,
                                lc.offsetUtcHours);
                            f.Write(str);
                        }
                        break;
                    case 3:
                        for (i = 0; i < locationList.Count(); i++)
                        {
                            CLocation lc = locationList[i];
                            // city
                            f.Write("@city=");
                            f.Write(lc.cityName);
                            f.Write("\n");

                            f.Write("@country=");
                            f.Write(lc.countryName);
                            f.Write("\n");

                            f.Write("@lat=");
                            strTemp = string.Format("{0}", lc.latitudeDeg);
                            f.Write(strTemp);
                            f.Write("\n");

                            f.Write("@long=");
                            strTemp = string.Format("{0}", lc.longitudeDeg);
                            f.Write(strTemp);
                            f.Write("\n");

                            f.Write("@timezone=");
                            strTemp = string.Format("{0}", lc.offsetUtcHours);
                            f.Write(strTemp);
                            f.Write("\n");

                            f.Write("@dst=");
                            strTemp = string.Format("{0}", lc.timezoneId);
                            f.Write(strTemp);
                            f.Write("\n@create\n\n");

                        }
                        break;
                    case 4:
                        for (i = 0; i < locationList.Count(); i++)
                        {
                            CLocation lc = locationList[i];
                            strTemp = string.Format("26700 {0}|{1}|{2}|{3}|{4}|{5}\n", lc.cityName, lc.countryName,
                                lc.longitudeDeg,
                                lc.latitudeDeg, lc.offsetUtcHours, lc.timezoneId);
                            f.Write(strTemp);
                        }
                        break;
                    default:
                        break;
                }
            }

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
            File.Delete(fileName);
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
