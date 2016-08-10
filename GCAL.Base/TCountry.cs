using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
	        TFileRichList F = new TFileRichList();

	        if (!TFile.FileExists(strFile))
	        {
		        TFile.CreateFileFromResource("countries.rl", strFile);
	        }

	        if (F.ReadFile(strFile))
	        {
		        //
		        // init from file
		        //
		        while(F.ReadLine() > 0)
		        {
			        if (int.Parse(F.GetTag()) == 77)
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
            TFileRichList F = new TFileRichList();

            int i;
            for (i = 0; i < gcountries.Count(); i++)
            {
                F.Clear();
                F.AddTag(77);
                F.AddText(gcountries[i].abbreviatedName);
                F.AddText(gcountries[i].name);
                F.AddInt(gcountries[i].continent);
                F.WriteLine();
            }

            F.WriteFile(szFile);

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
