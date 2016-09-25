using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Globalization;

namespace GCAL.Base
{
    public class TCountry
    {
        public static bool _modified = false;

        public static string[] gcontinents = {
		    "Europe", //1
		    "Asia",   //2
		    "Africa", //3
		    "America",//4
		    "Pacific",//5
		    "Indiana",//6
		    "Atlantic"//7
        };

        public static List<TCountry> gcountries = new List<TCountry>();


        public string abbreviatedName;
        public string name;
        public UInt16 code;
        public UInt16 continent;

        public override string ToString()
        {
            return name;
        }

        public static TCountry GetCurrentCountry()
        {
            CultureInfo ci = CultureInfo.CurrentCulture;
            RegionInfo ri = new RegionInfo(ci.LCID);
            TCountry tc = GetCountryByAcronym(ri.TwoLetterISORegionName);
            if (tc == null)
                return GetCountryByAcronym("ID");
            else
                return tc;
        }

        public static UInt16 CodeFromString(string str)
        {
            if (str.Length < 2)
                return 0;
            return Convert.ToUInt16((str[0] - 'A') * 256 + (str[1] - 'A'));
        }

        public static TCountry GetCountryByIndex(int nIndex)
        {
            return gcountries[nIndex];
        }

        public static TCountry GetCountryByAcronym(string s)
        {
            foreach (TCountry tc in gcountries)
            {
                if (tc.abbreviatedName.Equals(s))
                    return tc;
            }
            return null;
        }
        
        public static TCountry GetCountryByName(string s)
        {
            foreach (TCountry tc in gcountries)
            {
                if (tc.name.Equals(s))
                    return tc;
            }
            return null;
        }

        public static int InitWithFile(string strFile)
        {
	        GCRichFileLine F = new GCRichFileLine();

	        if (!File.Exists(strFile))
	        {
                File.WriteAllBytes(strFile, Properties.Resources.countries);
	        }

	        using (StreamReader sr = new StreamReader(strFile))
	        {
		        //
		        // init from file
		        //
		        while(F.SetLine(sr.ReadLine()))
		        {
			        if (F.TagInt == 77)
			        {
				        AddCountry(F.GetField(0), F.GetField(1), int.Parse(F.GetField(2)));
			        }
		        }
	        }

	        return gcountries.Count();
        }

        public static string GetCountryName(UInt16 w)
        {
            for (int i = 0; i < gcountries.Count(); i++)
            {
                if (gcountries[i].code == w)
                    return gcountries[i].name;
            }

            return "";
        }

        public static string GetCountryContinentName(UInt16 w)
        {
            for (int i = 0; i < gcountries.Count(); i++)
            {
                if (gcountries[i].code == w)
                    return gcontinents[gcountries[i].continent];
            }

            return "";
        }

        public static int GetCountryCount()
        {
            return gcountries.Count;
        }


        public static string GetCountryNameByIndex(int nIndex)
        {
            return gcountries[nIndex].name;
        }

        public static string GetCountryContinentNameByIndex(int nIndex)
        {
            return gcontinents[gcountries[nIndex].continent];
        }

        
        public static string GetCountryAcronymByIndex(int nIndex)
        {
	        return gcountries[nIndex].abbreviatedName;
        }

        public static int SaveToFile(string szFile)
        {
            using (StreamWriter sw = new StreamWriter(szFile))
            {
                for (int i = 0; i < gcountries.Count(); i++)
                {
                    sw.WriteLine("77 {0}|{1}|{2}",
                        gcountries[i].abbreviatedName,
                        gcountries[i].name,
                        gcountries[i].continent);
                }
            }

            return 1;
        }

        public static int AddCountry(string pszCode, string pszName, int nContinent)
        {
	        TCountry country = new TCountry();

	        country.abbreviatedName = pszCode;
	        country.code = Convert.ToUInt16(Convert.ToUInt32(pszCode[0])*256 + Convert.ToUInt32(pszCode[1]));
	        country.name = pszName;
	        country.continent = Convert.ToUInt16(nContinent);

	        gcountries.Add(country);
            _modified = true;

	        return gcountries.Count;
        }

        
        public static int SetCountryName(int nSelected, string psz)
        {
	        gcountries[nSelected].name = psz;
	        _modified = true;
	        return 1;
        }


        public static UInt16 GetCountryCode(int nIndex)
        {
	        return gcountries[nIndex].code;
        }


        public static bool IsModified()
        {
	        return _modified;
        }

    }
}
