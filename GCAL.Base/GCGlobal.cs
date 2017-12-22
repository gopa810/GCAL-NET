using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.IO;
using System.Drawing;
using System.Globalization;

using GCAL.Base.Scripting;

namespace GCAL.Base
{
    public class GCGlobal
    {
        public static GCLocation myLocation = new GCLocation();

        public static List<GCLocation> recentLocations = new List<GCLocation>();

        public static GregorianDateTime dateTimeShown = new GregorianDateTime();

        public static GregorianDateTime dateTimeToday = new GregorianDateTime();

        public static GregorianDateTime dateTimeTomorrow = new GregorianDateTime();

        public static GregorianDateTime dateTimeYesterday = new GregorianDateTime();

        public static TLangFileList languagesList;

        public static Dictionary<AppFileName,string> applicationStrings = new Dictionary<AppFileName, string>();

        public static string dialogLastRatedSpec = string.Empty;


        public static string ConfigFolder
        {
            get
            {
                return applicationStrings[AppFileName.ConfigurationFolder];
            }
        }
        public static string CoreDataFolder
        {
            get
            {
                return applicationStrings[AppFileName.CoreDataFolder];
            }
        }

        public static string GetFileName(AppFileName folder, string fileName)
        {
            return Path.Combine(applicationStrings[folder], fileName);
        }

        public static int initFolders()
        {
            //string pszBuffer = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string pszBuffer = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

            pszBuffer = Path.Combine(pszBuffer, "GCAL");
            if (!Directory.Exists(pszBuffer))
                Directory.CreateDirectory(pszBuffer);

            applicationStrings[AppFileName.MainFolder] = pszBuffer;

            applicationStrings[AppFileName.ConfigurationFolder] = Path.Combine(applicationStrings[AppFileName.MainFolder], "config");
            Directory.CreateDirectory(applicationStrings[AppFileName.ConfigurationFolder]);

            applicationStrings[AppFileName.LanguagesFolder] = Path.Combine(applicationStrings[AppFileName.MainFolder], "lang");
            Directory.CreateDirectory(applicationStrings[AppFileName.LanguagesFolder]);

            applicationStrings[AppFileName.TemporaryFolder] = Path.Combine(applicationStrings[AppFileName.MainFolder], "temp");
            Directory.CreateDirectory(applicationStrings[AppFileName.TemporaryFolder]);

            applicationStrings[AppFileName.CoreDataFolder] = Path.Combine(applicationStrings[AppFileName.MainFolder], "cores");
            Directory.CreateDirectory(applicationStrings[AppFileName.CoreDataFolder]);

            applicationStrings[AppFileName.MapsDataFolder] = Path.Combine(applicationStrings[AppFileName.MainFolder], "maps");
            Directory.CreateDirectory(applicationStrings[AppFileName.MapsDataFolder]);

            string confDir = applicationStrings[AppFileName.ConfigurationFolder];

            applicationStrings[AppFileName.GSTR_CE_FILE] = GetFileName(AppFileName.ConfigurationFolder, "cevents.cfg");
            applicationStrings[AppFileName.GSTR_CONF_FILE] = GetFileName(AppFileName.ConfigurationFolder, "current.cfg");
            applicationStrings[AppFileName.GSTR_LOC_FILE] = GetFileName(AppFileName.ConfigurationFolder, "locations.cfg");
            applicationStrings[AppFileName.GSTR_SSET_FILE] = GetFileName(AppFileName.ConfigurationFolder, "showset.cfg");
            applicationStrings[AppFileName.GSTR_LOCX_FILE] = GetFileName(AppFileName.ConfigurationFolder, "gcal_locations3.rl");//GCAL 3.0
            applicationStrings[AppFileName.GSTR_CEX_FILE] = GetFileName(AppFileName.ConfigurationFolder, "gcal_events3.rl");//GCAL 3.0
            applicationStrings[AppFileName.GSTR_CONFX_FILE] = GetFileName(AppFileName.ConfigurationFolder, "gcal_settings2.rl");
            applicationStrings[AppFileName.GSTR_TZ_FILE] = GetFileName(AppFileName.ConfigurationFolder, "gcal_timezones1.rl");
            applicationStrings[AppFileName.GSTR_COUNTRY_FILE] = GetFileName(AppFileName.ConfigurationFolder, "gcal_countries1.rl");
            applicationStrings[AppFileName.GSTR_TEXT_FILE] = GetFileName(AppFileName.ConfigurationFolder, "gcal_strings2.rl");
            applicationStrings[AppFileName.GSTR_TIPS_FILE] = GetFileName(AppFileName.ConfigurationFolder, "gcal_tips1.txt");
            applicationStrings[AppFileName.GSTR_HELP_FILE] = GetFileName(AppFileName.MainFolder, "gcal.chm");

            return 1;
        }

        public static string GetAppString(AppFileName n)
        {
            return applicationStrings[n];
        }


        public static void OpenFile(string fileName)
        {
            GCRichFileLine rf = new GCRichFileLine();
            Rectangle rc;
            recentLocations.Clear();
            if (!File.Exists(fileName))
                return;
            using(StreamReader sr = new StreamReader(fileName))
            {
                // nacitava
                while (rf.SetLine(sr.ReadLine()))
                {
                    switch (rf.TagInt)
                    {
                        case 12703:
                            myLocation.Title = rf.GetField(0);
                            myLocation.Longitude = double.Parse(rf.GetField(1));
                            myLocation.Latitude = double.Parse(rf.GetField(2));
                            myLocation.TimeZoneName = rf.GetField(3);
                            break;
                        case 12704:
                            {
                                GCLocation loc = new GCLocation();
                                loc.Title = rf.GetField(0);
                                loc.Longitude = double.Parse(rf.GetField(1));
                                loc.Latitude = double.Parse(rf.GetField(2));
                                loc.TimeZoneName = rf.GetField(3);
                                recentLocations.Add(loc);
                            }
                            break;
                        case 12710:
                            rc = new Rectangle();
                            string rcs = String.Format("{0}|{1}|{2}|{3}", rf.GetField(0), rf.GetField(1),
                                rf.GetField(2), rf.GetField(3));
                            if (GCUserInterface.windowController != null)
                                GCUserInterface.windowController.ExecuteMessage("setMainRectangle", new GSString(rcs));
                            break;
                        case 12711:
                            GCDisplaySettings.setValue(int.Parse(rf.GetField(0)), int.Parse(rf.GetField(1)));
                            break;
                        case 12800:
                            GCUserInterface.ShowMode = int.Parse(rf.GetField(0));
                            break;
                        case 12802:
                            GCLayoutData.LayoutSizeIndex = int.Parse(rf.GetField(0));
                            break;
                        case 12900:
                            dialogLastRatedSpec = rf.GetField(0);
                            break;
                        default:
                            break;
                    }
                }
            }

        }


        public static void SaveFile(string fileName)
        {
            GSCore rcs = GCUserInterface.windowController.ExecuteMessage("getMainRectangle", (GSCore)null);

            using(StreamWriter f = new StreamWriter(fileName))
            {
                f.WriteLine("12703 {0}|{1}|{2}|{3}", myLocation.Title, 
                    myLocation.Longitude, myLocation.Latitude, myLocation.TimeZoneName);
                foreach (GCLocation loc in recentLocations)
                {
                    f.WriteLine("12704 {0}|{1}|{2}|{3}", loc.Title,
                        loc.Longitude, loc.Latitude, loc.TimeZoneName);
                }
                f.WriteLine("12710 {0}", rcs.getStringValue());
                for (int y = 0; y < GCDisplaySettings.getCount(); y++)
                {
                    f.WriteLine("12711 {0}|{1}", y, GCDisplaySettings.getValue(y));
                }
                f.WriteLine("12800 {0}", GCUserInterface.ShowMode);
                f.WriteLine("12802 {0}", GCLayoutData.LayoutSizeIndex);
                f.WriteLine("12900 {0}", dialogLastRatedSpec);
            }
        }


        public static bool GetLangFileForAcr(string pszAcr, out string strFile)
        {
            foreach (TLangFileInfo p in languagesList.list)
            {
                if (p.m_strAcr.Equals(pszAcr, StringComparison.CurrentCultureIgnoreCase))
                {
                    strFile = p.m_strFile;
                    return true;
                }
            }
            strFile = null;
            return false;
        }


        public static void LoadInstanceData()
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            System.Threading.Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;

            // initialization for AppDir
            initFolders();

            // initialization of global strings
            GCStrings.readFile(GetAppString(AppFileName.GSTR_TEXT_FILE));

            // inicializacia timezones
            TTimeZone.LoadFile(GetAppString(AppFileName.GSTR_TZ_FILE));

            // inicializacia countries
            TCountry.LoadFile(GetAppString(AppFileName.GSTR_COUNTRY_FILE));

            // inicializacia miest a kontinentov
            // lazy loading of data
            TLocationDatabase.FileName = GetAppString(AppFileName.GSTR_LOCX_FILE);
            //CLocationList.OpenFile();

            // inicializacia zobrazovanych nastaveni
            GCDisplaySettings.readFile(GetAppString(AppFileName.GSTR_SSET_FILE));

            // inicializacia custom events
            GCFestivalBookCollection.OpenFile(GetAppString(AppFileName.ConfigurationFolder));

            // looking for files *.recn
            GCConfigRatedManager.RefreshListFromDirectory(GetAppString(AppFileName.ConfigurationFolder));

            // initialization of global variables
            myLocation.EncodedString = GCLocation.DefaultEncodedString;

            recentLocations.Add(new GCLocation(myLocation));

            OpenFile(GetAppString(AppFileName.GSTR_CONFX_FILE));
            // refresh fasting style after loading user settings
            //GCFestivalBook.SetOldStyleFasting(GCDisplaySettings.getValue(42));

            // inicializacia tipov dna
            if (!File.Exists(GetAppString(AppFileName.GSTR_TIPS_FILE)))
            {
                File.WriteAllText(GetAppString(AppFileName.GSTR_TIPS_FILE), Properties.Resources.tips);
            }

        }


        public static void SaveInstanceData()
        {
            SaveFile(GetAppString(AppFileName.GSTR_CONFX_FILE));

            if (TLocationDatabase.Modified)
            {
                TLocationDatabase.SaveFile(GetAppString(AppFileName.GSTR_LOCX_FILE), 4);//GCAL 3.0
            }

            if (TCountry.IsModified())
            {
                TCountry.SaveFile(GetAppString(AppFileName.GSTR_COUNTRY_FILE));
            }

            if (GCStrings.gstr_Modified)
            {
                GCStrings.writeFile(GetAppString(AppFileName.GSTR_TEXT_FILE));
            }

            GCDisplaySettings.writeFile(GetAppString(AppFileName.GSTR_SSET_FILE));

            GCFestivalBookCollection.SaveAllChangedFestivalBooks(GetAppString(AppFileName.ConfigurationFolder));

            if (TTimeZone.Modified)
            {
                TTimeZone.SaveFile(GetAppString(AppFileName.GSTR_TZ_FILE));
            }

        }


        public static void AddRecentLocation(GCLocation cLocationRef)
        {
            int rl = recentLocations.IndexOf(cLocationRef);
            if (rl != 0)
            {
                if (rl > 0)
                {
                    recentLocations.RemoveAt(rl);
                }
                recentLocations.Insert(0, cLocationRef);
            }
        }

        public static GCLocation LastLocation
        {
            get
            {
                if (recentLocations.Count == 0)
                    return myLocation;
                return recentLocations[0];
            }
        }
    }
}
