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
        public static bool Modified = false;
        public static List<TCountry> Countries = new List<TCountry>();

        public int FirstDayOfWeek { get; set; }
        public string ISOCode { get; set; }
        public string ISO3Code { get; set; }
        public string Fips { get; set; }
        public string Name { get; set; }
        public string Capital { get; set; }
        public double Area { get; set; }
        public double Population { get; set; }
        public string ContinentISOCode { get; set; }
        public TContinent Continent
        {
            get
            {
                return TContinent.FindByISOCode(ContinentISOCode);
            }
        }
        public string Neighbours { get; set; }


        public static TCountry DefaultCountry = new TCountry() { ISOCode = "UC", ISO3Code = "UCO", Name = "(Unknown Country)",
        Population = 1000000, Area = 189230.0, Capital = "(Uknown City)", ContinentISOCode = "EP", Fips = "", Neighbours = ""};


        public TCountry()
        {
            FirstDayOfWeek = 1;
        }

        public override string ToString()
        {
            return Name;
        }

        public static TCountry GetCurrentCountry()
        {
            CultureInfo ci = CultureInfo.CurrentCulture;
            RegionInfo ri = new RegionInfo(ci.LCID);
            TCountry tc = FindCountryByISOCode(ri.TwoLetterISORegionName);
            if (tc == null)
                return FindCountryByISOCode("ID");
            else
                return tc;
        }

        public static TCountry FindCountryByISOCode(string s)
        {
            foreach (TCountry tc in Countries)
            {
                if (tc.ISOCode.Equals(s))
                    return tc;
            }
            return null;
        }
        
        public static TCountry FindCountryByName(string s)
        {
            foreach (TCountry tc in Countries)
            {
                if (tc.Name.Equals(s))
                    return tc;
            }
            return null;
        }

        /// <summary>
        /// Loading countries from permanent storage
        /// </summary>
        /// <param name="strFile"></param>
        /// <returns></returns>
        public static int LoadFile(string strFile)
        {
	        if (!File.Exists(strFile))
	        {
                File.WriteAllText(strFile, Properties.Resources.countries2016);
	        }

	        using (StreamReader sr = new StreamReader(strFile))
	        {
		        //
		        // init from file
		        //
                string line;
                Countries.Clear();
		        while((line = sr.ReadLine()) != null)
		        {
                    if (line.StartsWith("#"))
                        continue;

                    TCountry tc = new TCountry();
                    tc.EncodedString = line;
                    if (tc.Continent != null)
                        Countries.Add(tc);
		        }
	        }

	        return Countries.Count();
        }

        public static int SaveFile(string szFile)
        {
            using (StreamWriter sw = new StreamWriter(szFile))
            {
                foreach(TCountry tc in Countries)
                {
                    sw.WriteLine(tc.EncodedString);
                }
            }

            return 1;
        }

        public static int SetCountryName(int nSelected, string psz)
        {
	        Countries[nSelected].Name = psz;
	        Modified = true;
	        return 1;
        }


        public static bool IsModified()
        {
	        return Modified;
        }

        public string EncodedString
        {
            get
            {
                return string.Format("{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}\t{7}\t{8}\t{9}",
                        ISOCode, ISO3Code, Fips,
                        Name, Capital, Area, Population,
                        ContinentISOCode, Neighbours,
                        FirstDayOfWeek);
            }
            set
            {
                string[] p = value.Split('\t');
                if (p.Length >= 9)
                {
                    TCountry tc = this;
                    tc.ISOCode = p[0];
                    tc.ISO3Code = p[1];
                    tc.Fips = p[2];
                    tc.Name = p[3];
                    tc.Capital = p[4];
                    tc.Area = double.Parse(p[5]);
                    tc.Population = double.Parse(p[6]);
                    tc.ContinentISOCode = p[7];
                    tc.Neighbours = p[8];
                    tc.FirstDayOfWeek = int.Parse(p[9]);
                }
            }
        }

    }
}
