using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using GCAL.Base.Scripting;

namespace GCAL.Base
{
    public class TResultApp: TResultBase
    {
        public static readonly int TRESULT_APP_CELEBS = 3;
        public CLocationRef location;
        public GregorianDateTime eventTime;
        public GCAstroData details;
        public bool b_adhika;
        public int[] celeb_gy;
        public GregorianDateTime[] celeb_date;

        public class AppDayBase : GSCore
        {
            public int DsCondition = -1;
            public AppDayBase()
            {
            }

            public AppDayBase(int cond)
            {
                DsCondition = cond;
            }

            public override GSCore GetPropertyValue(string s)
            {
                if (s.Equals("DsCondition"))
                    return new GSNumber(DsCondition);
                return base.GetPropertyValue(s);
            }
        }

        public class AppDaySeparator : AppDayBase
        {
            public string Name = "";
            public AppDaySeparator()
            {
            }

            public AppDaySeparator(string n)
            {
                Name = n;
            }

            public AppDaySeparator(int i, string n)
            {
                DsCondition = i;
                Name = n;
            }

            public override GSCore GetPropertyValue(string s)
            {
                if (s.Equals("isInfo"))
                    return new GSBoolean(false);
                if (s.Equals("isSeparator"))
                    return new GSBoolean(true);
                if (s.Equals("Name"))
                    return new GSString(Name);
                return base.GetPropertyValue(s);
            }
        }

        public class AppDayInfo: AppDayBase
        {
            public string Name = "";
            public string Value = "";

            public AppDayInfo()
            {
            }

            public AppDayInfo(string a, string b)
            {
                Name = a;
                Value = b;
            }

            public AppDayInfo(int cond, string a, string b)
            {
                DsCondition = cond;
                Name = a;
                Value = b;
            }

            public override GSCore GetPropertyValue(string s)
            {
                if (s.Equals("isSeparator"))
                    return new GSBoolean(false);
                if (s.Equals("isInfo"))
                    return new GSBoolean(true);
                if (s.Equals("Name"))
                    return new GSString(Name);
                if (s.Equals("Value"))
                    return new GSString(Value);
                return base.GetPropertyValue(s);
            }

        }

        public override GSCore GetPropertyValue(string s)
        {
            if (s.Equals("items"))
            {
                GSList list = new GSList();
                foreach (AppDayBase adb in MainInfo)
                    list.Add(adb);
                return list;
            }
            else if (s.Equals("location"))
            {
                return location;
            }
            else if (s.Equals("eventTime"))
            {
                return eventTime;
            }
            else if (s.Equals("astroData"))
            {
                return details;
            }
            return base.GetPropertyValue(s);
        }

        public List<AppDayBase> MainInfo = new List<AppDayBase>();

        public void calculateAppDay(CLocationRef location, GregorianDateTime eventDate)
        {
            //MOONDATA moon;
            //SUNDATA sun;
            GCAstroData d = this.details = new GCAstroData();
            double dd;
            GregorianDateTime vc = new GregorianDateTime();
            vc.Set(eventDate);
            GregorianDateTime vcsun = new GregorianDateTime();
            vcsun.Set(eventDate);
            GCEarthData m_earth = location.EARTHDATA();

            this.b_adhika = false;
            this.eventTime = new GregorianDateTime(eventDate);
            this.location = location;

            //d.nTithi = GetPrevTithiStart(m_earth, vc, dprev);
            //GetNextTithiStart(m_earth, vc, dnext);
            vcsun.shour -= vcsun.TimezoneHours / 24.0;
            vcsun.NormalizeValues();
            vcsun.TimezoneHours = 0.0;
            d.sun.SunPosition(vcsun, m_earth, vcsun.shour - 0.5);
            d.moon.Calculate(vcsun.GetJulianComplete());
            d.msDistance = GCMath.putIn360(d.moon.longitude_deg - d.sun.longitude_deg - 180.0);
            d.msAyanamsa = GCAyanamsha.GetAyanamsa(vc.GetJulianComplete());

            // tithi
            dd = d.msDistance / 12.0;
            d.nTithi = GCMath.IntFloor((dd));
            d.nTithiElapse = (dd - Math.Floor(dd)) * 100.0;
            d.nPaksa = (d.nTithi >= 15) ? 1 : 0;


            // naksatra
            dd = GCMath.putIn360(d.moon.longitude_deg - d.msAyanamsa);
            dd = (dd * 3.0) / 40.0;
            d.nNaksatra = GCMath.IntFloor((dd));
            d.nNaksatraElapse = (dd - Math.Floor(dd)) * 100.0;
            d.nMasa = d.MasaCalc(vc, m_earth);
            d.nMoonRasi = GCRasi.GetRasi(d.moon.longitude_deg, d.msAyanamsa);
            d.nSunRasi = GCRasi.GetRasi(d.sun.longitude_deg, d.msAyanamsa);

            if (d.nMasa == (int)MasaId.ADHIKA_MASA)
            {
                d.nMasa = GCRasi.GetRasi(d.sun.longitude_deg, d.msAyanamsa);
                this.b_adhika = true;
            }

            vc.Today();
            vc.TimezoneHours = m_earth.offsetUtcHours;
            int m = 0;
            GaurabdaDate va = new GaurabdaDate();
            GregorianDateTime vctemp;

            va.tithi = d.nTithi;
            va.masa = d.nMasa;
            va.gyear = GCCalendar.GetGaurabdaYear(vc, m_earth);
            if (va.gyear < d.nGaurabdaYear)
                va.gyear = d.nGaurabdaYear;

            MainInfo.Add(new AppDayInfo(GCStrings.getString(7), eventDate.ToString()));
            MainInfo.Add(new AppDayBase());
            MainInfo.Add(new AppDayInfo(GCStrings.getString(8), eventDate.ShortTimeString()));
            MainInfo.Add(new AppDayBase());
            MainInfo.Add(new AppDayBase());
            MainInfo.Add(new AppDayInfo(GCStrings.getString(9), location.locationName));
            MainInfo.Add(new AppDayInfo(GCStrings.getString(10), location.GetLatitudeString()));
            MainInfo.Add(new AppDayInfo(GCStrings.getString(11), location.GetLongitudeString()));
            MainInfo.Add(new AppDayInfo(GCStrings.Localized("Timezone"), TTimeZone.GetTimeZoneName(location.timezoneId)));
            MainInfo.Add(new AppDayInfo("DST", "N/A"));
            MainInfo.Add(new AppDayBase());
            MainInfo.Add(new AppDayInfo(GCStrings.getString(13), GCTithi.GetName(d.nTithi)));
            MainInfo.Add(new AppDayInfo(GCStrings.getString(14), string.Format("{0:00.000}%", d.nTithiElapse)));
            MainInfo.Add(new AppDayInfo(GCStrings.getString(15), GCNaksatra.GetName(d.nNaksatra)));
            MainInfo.Add(new AppDayInfo(GCStrings.getString(16), string.Format("{0:00.000}% ({1} pada)", d.nNaksatraElapse, GCStrings.getString(811 + d.NaksatraPada))));
            MainInfo.Add(new AppDayInfo(GCStrings.Localized("Moon Rasi"), GCRasi.GetName(d.nMoonRasi)));
            MainInfo.Add(new AppDayInfo(GCStrings.Localized("Sun Rasi"), GCRasi.GetName(d.nSunRasi)));
            MainInfo.Add(new AppDayInfo(GCStrings.getString(20), GCPaksa.GetName(d.nPaksa)));

            if (b_adhika == true)
            {
                MainInfo.Add(new AppDayInfo(GCStrings.getString(22), string.Format("{0} {1}", GCMasa.GetName(d.nMasa), GCStrings.getString(21))));
            }
            else
                MainInfo.Add(new AppDayInfo(GCStrings.getString(22), GCMasa.GetName(d.nMasa)));
            MainInfo.Add(new AppDayInfo(GCStrings.getString(23), d.nGaurabdaYear.ToString()));

            if (GCDisplaySettings.getValue(48) == 1)
            {
                MainInfo.Add(new AppDayBase(GCDS.APP_CHILDNAMES));
                MainInfo.Add(new AppDaySeparator(GCStrings.getString(17)));
                MainInfo.Add(new AppDayBase(GCDS.APP_CHILDNAMES));

                MainInfo.Add(new AppDayInfo(GCDS.APP_CHILDNAMES, GCStrings.getString(18), GCStrings.GetNaksatraChildSylable(d.nNaksatra, d.NaksatraPada) + "..."));
                MainInfo.Add(new AppDayInfo(GCDS.APP_CHILDNAMES, GCStrings.getString(19), GCStrings.GetRasiChildSylable(d.nMoonRasi) + "..."));
            }

            MainInfo.Add(new AppDayBase());
            MainInfo.Add(new AppDaySeparator(GCStrings.getString(24)));
            MainInfo.Add(new AppDayBase());


            celeb_date = new GregorianDateTime[TRESULT_APP_CELEBS];
            celeb_gy = new int[TRESULT_APP_CELEBS];

            for (int i = 0; i < TRESULT_APP_CELEBS + 3; i++)
            {
                GCCalendar.VATIMEtoVCTIME(va, out vctemp, m_earth);
                if (va.gyear > d.nGaurabdaYear)
                {
                    if (m < TRESULT_APP_CELEBS)
                    {
                        MainInfo.Add(new AppDayInfo(string.Format("Gaurabda {0}", va.gyear), vctemp.ToString()));
                        this.celeb_date[m] = new GregorianDateTime(vctemp);
                        this.celeb_gy[m] = va.gyear;
                        m++;
                    }
                }
                va.gyear++;
            }
        }

        public override string formatText(string df)
        {
            GSScript script = new GSScript();
            switch (df)
            {
                case GCDataFormat.PlainText:
                    script.readTextTemplate(Properties.Resources.TplAppDayPlain);
                    break;
                case GCDataFormat.Rtf:
                    script.readTextTemplate(Properties.Resources.TplAppDayRtf);
                    break;
                case GCDataFormat.HTML:
                    script.readTextTemplate(Properties.Resources.TplAppDayHtml);
                    break;
                case GCDataFormat.XML:
                    script.readTextTemplate(Properties.Resources.TplAppDayXml);
                    break;
                case GCDataFormat.CSV:
                    script.readTextTemplate(Properties.Resources.TplAppDayCsv);
                    break;
                default:
                    break;
            }


            GSExecutor engine = new GSExecutor();
            engine.SetVariable("appday", this);
            engine.SetVariable("location", this.location);
            engine.SetVariable("app", GCUserInterface.Shared);
            engine.ExecuteElement(script);


            return engine.getOutput();
        }

        public override TResultFormatCollection getFormats()
        {
            TResultFormatCollection coll = base.getFormats();

            coll.ResultName = "AppearanceDay";
            coll.Formats.Add(new TResultFormat("Text File", "txt", GCDataFormat.PlainText));
            coll.Formats.Add(new TResultFormat("Rich Text File", "rtf", GCDataFormat.Rtf));
            coll.Formats.Add(new TResultFormat("XML File", "xml", GCDataFormat.XML));
            coll.Formats.Add(new TResultFormat("Comma Separated Values", "csv", GCDataFormat.CSV));
            coll.Formats.Add(new TResultFormat("HTML File (in List format)", "htm", GCDataFormat.HTML));
            return coll;
        }

    }
}
