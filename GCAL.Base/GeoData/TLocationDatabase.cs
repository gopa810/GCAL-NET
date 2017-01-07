using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace GCAL.Base
{
    public class TLocationDatabase
    {
        private static List<TLocation> locationList = null;

        /// <summary>
        /// This should be initialized as soon as possible, 
        /// because this will be used for lazy loading of database
        /// </summary>
        public static string FileName { get; set; }

        /// <summary>
        /// List of all built-in locations
        /// </summary>
        public static List<TLocation> LocationList
        {
            get
            {
                if (locationList != null) return locationList;
                LoadFile(FileName, true);
                return locationList;
            }
        }

        /// <summary>
        /// Flag denoting modification of database and the need of saving it to storage
        /// </summary>
        public static bool Modified { get; set; }


        /// <summary>
        /// Constructor
        /// </summary>
        public TLocationDatabase()
        {
            Modified = true;
        }

        /// <summary>
        /// Loading file in the format, where values are separated with TAB character
        /// </summary>
        /// <param name="fileName">File name</param>
        /// <param name="flagStandardFile">TRUE if loaded file is standard one, FALSE if loaded file is imported external file</param>
        /// <returns>TRUE if file was successfuly loaded, FALSE if the file was not loaded</returns>
        public static bool LoadFile(string fileName, bool flagStandardFile)
        {
            locationList = new List<TLocation>();

            if (!File.Exists(fileName))
            {
                if (flagStandardFile)
                    File.WriteAllText(fileName, Properties.Resources.cities2016);
                else
                    return false;
            }

            using(StreamReader sr = new StreamReader(fileName))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    if (line.StartsWith("#")) continue;
                    TLocation loc = new TLocation(line);
                    if (loc.Valid)
                        locationList.Add(loc);
                }
            }

            return true;
        }


        public static bool SaveFile(string lpszFileName, int nType)
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
                        foreach (TCountry country in TCountry.Countries)
                        {
                            str = string.Format("\t<ccn code=\"{0}\" country=\"{1}\" continent=\"{2}\" />\n", country.ISOCode, country.Name, country.Continent.Name);
                            f.Write(str);
                        }
                        f.Write("\t</countries>\n");
                        f.Write("\t<dsts>\n");
                        foreach (TTimeZone timeZone in TTimeZone.TimeZoneList)
                        {
                            str = timeZone.GetXMLString();
                            f.Write(str);
                        }
                        f.Write("\t</dsts>\n");
                        f.Write("\t<cities>\n");
                        for (i = 0; i < locationList.Count(); i++)
                        {
                            TLocation lc = locationList[i];
                            str = string.Format("\t<loc city=\"{0}\" lon=\"{1}\" lat=\"{2}\" tzone=\"{3}\"\n\t\tcountry=\"{4}\" />\n",
                                lc.CityName, lc.Longitude, lc.Latitude,
                                lc.TimeZone.OffsetMinutes/60.0, lc.Country.Name);
                            str.Replace("&", "&amp;");
                            f.Write(str);
                        }
                        f.Write("\t</cities>\n");
                        f.Write("</xml>");
                        break;
                    case 2:
                        f.Write("Countries:\n");
                        foreach (TCountry country in TCountry.Countries)
                        {
                            str = string.Format("\t{0}\t{1}\t{2}\n", country.ISOCode, country.Name, country.Continent.Name);
                            f.Write(str);
                        }
                        f.Write("Daylight Saving Time Systems:\n");
                        foreach (TTimeZone timeZone in TTimeZone.TimeZoneList)
                        {
                            str = string.Format("\t{0}\n", timeZone.Name);
                            f.Write(str);
                        }
                        f.Write("Cities:\n");
                        for (i = 0; i < locationList.Count(); i++)
                        {
                            TLocation lc = locationList[i];
                            str = string.Format("\t{0}\t{1}\t{2}\t{3}\t{4}\n",
                                lc.CityName,
                                lc.CountryISOCode,
                                lc.Longitude,
                                lc.Latitude,
                                lc.TimeZoneName);
                            f.Write(str);
                        }
                        break;
                    case 3:
                        for (i = 0; i < locationList.Count(); i++)
                        {
                            TLocation lc = locationList[i];
                            // city
                            f.Write("@city=");
                            f.Write(lc.CityName);
                            f.Write("\n");

                            f.Write("@countryCode=");
                            f.Write(lc.CountryISOCode);
                            f.Write("\n");

                            f.Write("@lat=");
                            strTemp = string.Format("{0}", lc.Latitude);
                            f.Write(strTemp);
                            f.Write("\n");

                            f.Write("@long=");
                            strTemp = string.Format("{0}", lc.Longitude);
                            f.Write(strTemp);
                            f.Write("\n");

                            f.Write("@timezone=");
                            strTemp = string.Format("{0}", lc.TimeZoneName);
                            f.Write(strTemp);
                            f.Write("\n@create\n\n");

                        }
                        break;
                    case 4:
                        foreach (TLocation lc in locationList)
                        {
                            f.WriteLine(lc.EncodedString);
                        }
                        break;
                    default:
                        break;
                }
            }

            return true;
        }

        /// <summary>
        /// Importing new file into database
        /// </summary>
        /// <param name="fileName">Path for importing file</param>
        /// <param name="flagDeleteCurrent">Flag for replacing of database or adding to database. Value TRUE means existing record are deleted.</param>
        /// <returns>Success of loading the specified file</returns>
        public static bool ImportFile(string fileName, bool flagDeleteCurrent)
        {
            if (flagDeleteCurrent)
            {
                locationList.Clear();
            }

            Modified = true;

            return LoadFile(fileName, false);
        }

        /// <summary>
        /// Reseting user database to the built-in one
        /// </summary>
        public static void SetDefaultDatabase()
        {
            File.Delete(FileName);
            LoadFile(FileName, true);
            Modified = true;
        }

    }
}
