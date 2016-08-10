using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GCAL.Base
{
    public class TResultApp
    {
        public static readonly int TRESULT_APP_CELEBS = 3;
        public CLocationRef location;
        public GregorianDateTime eventTime;
        public GCAstroData details;
        public bool b_adhika;
        public int[] celeb_gy;
        public GregorianDateTime[] celeb_date;

        public class AppDayBase
        {
            public int DsCondition = -1;
            public AppDayBase()
            {
            }

            public AppDayBase(int cond)
            {
                DsCondition = cond;
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

        }

        public List<AppDayBase> MainInfo = new List<AppDayBase>();
        public List<AppDayBase> Celebrations = new List<AppDayBase>();

        public void calculateAppDay(CLocationRef location, GregorianDateTime eventDate)
        {
            //MOONDATA moon;
            //SUNDATA sun;
            GCAstroData d = this.details;
            double dd;
            GregorianDateTime vc = new GregorianDateTime();
            vc.Set(eventDate);
            GregorianDateTime vcsun = new GregorianDateTime();
            vcsun.Set(eventDate);
            GCEarthData m_earth = location.EARTHDATA();

            this.b_adhika = false;
            this.eventTime.Set(eventDate);
            this.location.Set(location);

            //d.nTithi = GetPrevTithiStart(m_earth, vc, dprev);
            //GetNextTithiStart(m_earth, vc, dnext);
            vcsun.shour -= vcsun.TimezoneHours / 24.0;
            vcsun.NormalizeValues();
            vcsun.TimezoneHours = 0.0;
            d.sun.SunPosition(vcsun, m_earth, vcsun.shour - 0.5);
            d.moon.Calculate(vcsun.GetJulianComplete(), m_earth);
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

            MainInfo.Add(new AppDayInfo(GCStrings.getString(7), vc.ToString()));
            MainInfo.Add(new AppDayBase());
            MainInfo.Add(new AppDayInfo(GCStrings.getString(8), vc.ShortTimeString()));
            MainInfo.Add(new AppDayBase());
            MainInfo.Add(new AppDayBase());
            MainInfo.Add(new AppDayInfo(GCStrings.getString(9), location.locationName));
            MainInfo.Add(new AppDayInfo(GCStrings.getString(10), location.GetLatitudeString()));
            MainInfo.Add(new AppDayInfo(GCStrings.getString(11), location.GetLongitudeString()));
            MainInfo.Add(new AppDayInfo(GCStrings.Localized("Timezone"), TTimeZone.GetTimeZoneName(location.timezoneId)));
            MainInfo.Add(new AppDayInfo("DST", "N/A"));
            MainInfo.Add(new AppDayBase());
            MainInfo.Add(new AppDayInfo(GCStrings.getString(13), GCTithi.GetName(d.nTithi)));
            MainInfo.Add(new AppDayInfo(GCStrings.getString(14), d.nTithiElapse.ToString() + " %"));
            MainInfo.Add(new AppDayInfo(GCStrings.getString(15), GCNaksatra.GetName(d.nNaksatra)));
            MainInfo.Add(new AppDayInfo(GCStrings.getString(16), string.Format("{0} % ({1} pada)", d.nNaksatraElapse, GCStrings.getString(811 + d.NaksatraPada))));
            MainInfo.Add(new AppDayInfo("Moon Rasi", GCRasi.GetName(d.nMoonRasi)));
            MainInfo.Add(new AppDayInfo("Sun Rasi", GCRasi.GetName(d.nSunRasi)));
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
                        this.celeb_date[m].Set(vctemp);
                        this.celeb_gy[m] = va.gyear;
                        m++;
                    }
                }
                va.gyear++;
            }
        }

        public void formatPlainText(StringBuilder strResult)
        {
	        TResultApp app = this;
	        GCAstroData d = app.details;
	        GregorianDateTime vc = new GregorianDateTime();
	        vc.Set(app.eventTime);
	        GCEarthData m_earth = app.location.EARTHDATA();
	        GCStringBuilder sb = new GCStringBuilder();
            sb.Target = strResult;
            sb.Format = GCStringBuilder.FormatType.PlainText;

	        sb.AppendLine(GCStrings.getString(25));
	        sb.AppendLine();

            foreach (AppDayBase ab in MainInfo)
            {
                if (ab.DsCondition < 0 || (ab.DsCondition >= 0 && GCDisplaySettings.getBoolValue(ab.DsCondition)))
                {
                    if (ab is AppDayInfo)
                    {
                        AppDayInfo adi = ab as AppDayInfo;
                        sb.AppendFormat("{0} : {1}", adi.Name, adi.Value);
                        sb.AppendLine();
                    }
                    else if (ab is AppDaySeparator)
                    {
                        AppDaySeparator ase = ab as AppDaySeparator;
                        sb.AppendLine(ase.Name);
                    }
                }
            }

	        sb.AppendNote();
        }

        public void formatRtf(StringBuilder strResult)
        {
	        TResultApp app = this;
	        GCAstroData d = app.details;
	        GregorianDateTime vc = new GregorianDateTime();
	        vc.Set(app.eventTime);
	        GCEarthData m_earth = app.location.EARTHDATA();
	
	        GCStringBuilder sb = new GCStringBuilder();

	        sb.fontSizeH1 = GCLayoutData.textSizeH1;
	        sb.fontSizeH2 = GCLayoutData.textSizeH2;
	        sb.fontSizeText = GCLayoutData.textSizeText;
	        sb.fontSizeNote = GCLayoutData.textSizeNote;
	        sb.Format = GCStringBuilder.FormatType.RichText;
	        sb.Target = strResult;

	        strResult.Clear();
	        sb.AppendDocumentHeader();

	        sb.AppendHeader1(GCStrings.getString(25));

            foreach (AppDayBase ab in MainInfo)
            {
                if (ab.DsCondition < 0 || (ab.DsCondition >= 0 && GCDisplaySettings.getBoolValue(ab.DsCondition)))
                {
                    if (ab is AppDayInfo)
                    {
                        AppDayInfo adi = ab as AppDayInfo;
                        sb.AppendFormat("\\tab {0} : {{\\b {1}}", adi.Name, adi.Value);
                        sb.AppendLine();
                    }
                    else if (ab is AppDaySeparator)
                    {
                        AppDaySeparator ase = ab as AppDaySeparator;
                        sb.AppendHeader2(ase.Name);
                    }
                }
            }



	        sb.AppendDocumentTail();
        }

        public void formatXml(StringBuilder strResult)
        {
            TResultApp app = this;
            GCAstroData d = this.details;
            GregorianDateTime vc = new GregorianDateTime();
            vc.Set(eventTime);
            GCEarthData m_earth = this.location.EARTHDATA();
            CLocationRef loc = this.location;
            GCStringBuilder strText = new GCStringBuilder();
            strText.Format = GCStringBuilder.FormatType.PlainText;
            strText.Target = strResult;
            int npada;
            bool bDuringAdhika = false;

            strText.AppendFormat(
                "<xml>\n" +
                "\t<request name=\"AppDay\" version=\"{0}\">\n" +
                "\t\t<arg name=\"longitude\" value=\"{1}\" />\n" +
                "\t\t<arg name=\"latitude\" value=\"{2}\" />\n" +
                "\t\t<arg name=\"timezone\" value=\"{3}\" />\n" +
                "\t\t<arg name=\"year\" value=\"{4}\" />\n" +
                "\t\t<arg name=\"month\" value=\"{5}\" />\n" +
                "\t\t<arg name=\"day\" value=\"{6}\" />\n" +
                "\t\t<arg name=\"hour\" value=\"{7}\" />\n" +
                "\t\t<arg name=\"minute\" value=\"{8}\" />\n" +
                "\t</request>\n", GCStrings.getString(130),
                loc.longitudeDeg, loc.latitudeDeg, loc.offsetUtcHours,
                app.eventTime.year, app.eventTime.month, app.eventTime.day,
                app.eventTime.GetHour(), app.eventTime.GetMinuteRound()
                );


            npada = d.NaksatraPada;
            if (npada > 4)
                npada = 4;

            strText.AppendFormat("\t<result name=\"AppDay\" >\n" +
                "\t\t<tithi name=\"{0}\" elapse=\"{1}\" />\n" +
                "\t\t<naksatra name=\"{2}\" elapse=\"{3}\" pada=\"{4}\"/>\n" +
                "\t\t<paksa name=\"{5}\" />\n" +
                "\t\t<masa name=\"{6}\" adhikamasa=\"{7}\"/>\n" +
                "\t\t<gaurabda value=\"{8}\" />\n"

                , GCTithi.GetName(d.nTithi), d.nTithiElapse
                , GCNaksatra.GetName(d.nNaksatra), d.nNaksatraElapse, npada
                , GCPaksa.GetName(d.nPaksa)
                , GCMasa.GetName(d.nMasa), (bDuringAdhika ? "yes" : "no")
                , d.nGaurabdaYear
                );

            strText.AppendString("\t\t<celebrations>\n");

            for (int i = 0; i < TRESULT_APP_CELEBS; i++)
            {
                strText.AppendFormat("\t\t\t<celebration gaurabda=\"{0}\" day=\"{1}\" month=\"{2}\" monthabr=\"{3}\" year=\"{4}\" />\n"
                    , app.celeb_gy[i], app.celeb_date[i].day, app.celeb_date[i].month, GregorianDateTime.GetMonthAbreviation(app.celeb_date[i].month), app.celeb_date[i].year);
            }

            strText.AppendString("\t\t</celebrations>\n\t</result>\n</xml>\n");
        }


        /******************************************************************************************/
        /*                                                                                        */
        /*                                                                                        */
        /*                                                                                        */
        /*                                                                                        */
        /*                                                                                        */
        /*                                                                                        */
        /*                                                                                        */
        /*                                                                                        */
        /*                                                                                        */
        /*                                                                                        */
        /*                                                                                        */
        /*                                                                                        */
        /*                                                                                        */
        /*                                                                                        */
        /*                                                                                        */
        /******************************************************************************************/

        public void writeHtml(StringBuilder F)
        {
            GCAstroData d = this.details;
            GregorianDateTime vc = new GregorianDateTime();
            vc.Set(eventTime);
            GCEarthData m_earth = this.location.EARTHDATA();

            GCStringBuilder sb = new GCStringBuilder();
            sb.Format = GCStringBuilder.FormatType.PlainText;
            sb.Target = F;

            sb.AppendString("<html><head><title>Appearance day</title>");
            sb.AppendString("<style>\n<!--\nbody {\n  font-family:Verdana;\n  font-size:11pt;\n}\n\ntd.hed {\n  font-size:11pt;\n  font-weight:bold;\n");
            sb.AppendString("  background:#aaaaaa;\n  color:white;\n  text-align:center;\n  vertical-align:center;\n  padding-left:15pt;\n  padding-right:15pt;\n");
            sb.AppendString("  padding-top:5pt;\n  padding-bottom:5pt;\n}\n-->\n</style>\n");
            sb.AppendString("</head>\n\n<body>\n");
            sb.AppendString("<h2 align=center>Appearance day Calculation</h2>");
            sb.AppendString("<table align=center><tr><td valign=top>\n\n");
            sb.AppendString("<table align=center>");
            sb.AppendString("<tr><td colspan=3 class=hed>Details</td></tr>\n");
            sb.AppendFormat("<tr><td colspan=2>{0}</td><td> {1} {2} {3}</td></tr>\n", GCStrings.getString(7), vc.day, GregorianDateTime.GetMonthAbreviation(vc.month), vc.year);
            sb.AppendFormat("<tr><td colspan=2>{0}</td><td> {1}:{2:00}</td></tr>\n\n", GCStrings.getString(8), vc.GetHour(), vc.GetMinuteRound());
            sb.AppendFormat("<tr><td colspan=2>{0}</td><td> {1}</td></tr>\n", GCStrings.getString(9), this.location.locationName);
            sb.AppendFormat("<tr><td colspan=2>{0}</td><td> {1}</td></tr>\n", GCStrings.getString(10), GCEarthData.GetTextLatitude(this.location.latitudeDeg));
            sb.AppendFormat("<tr><td colspan=2>{0}</td><td> {1}</td></tr>\n", GCStrings.getString(11), GCEarthData.GetTextLongitude(this.location.longitudeDeg));
            sb.AppendFormat("<tr><td colspan=2>{0}</td><td> ", GCStrings.Localized("Timezone"));
            sb.AppendString(TTimeZone.GetTimeZoneOffsetText(this.location.offsetUtcHours));
            sb.AppendString("</td></tr>\n");
            sb.AppendString("<tr><td colspan=2>DST</td><td>N/A</td></tr>\n");
            sb.AppendFormat("<tr><td colspan=2>{0}</td><td> {1}</td></tr>\n", GCStrings.getString(13), GCTithi.GetName(d.nTithi));
            sb.AppendFormat("<tr><td colspan=2>{0}</td><td> {1} %%</td></tr>\n", GCStrings.getString(14), d.nTithiElapse);
            sb.AppendFormat("<tr><td colspan=2>{0}</td><td> {1}</td></tr>\n", GCStrings.getString(15), GCNaksatra.GetName(d.nNaksatra));
            sb.AppendFormat("<tr><td colspan=2>{0}</td><td> {1} %%</td></tr>\n", GCStrings.getString(16), d.nNaksatraElapse);
            sb.AppendFormat("<tr><td colspan=2>{0}</td><td> {1}</td></tr>\n", GCStrings.getString(20), GCPaksa.GetName(d.nPaksa));
            if (this.b_adhika == true)
            {
                sb.AppendFormat("<tr><td colspan=2>{0}</td><td> {1} {2}</td></tr>\n", GCStrings.getString(22), GCMasa.GetName(d.nMasa), GCStrings.getString(21));
            }
            else
                sb.AppendFormat("<tr><td colspan=2>{0}</td><td> {1}</td></tr>\n", GCStrings.getString(22), GCMasa.GetName(d.nMasa));
            sb.AppendFormat("<tr><td colspan=2>{0}</td><td> {1}</td></tr>\n\n", GCStrings.getString(23), d.nGaurabdaYear);

            sb.AppendString("</table></td><td valign=top><table>");
            sb.AppendFormat("<tr><td colspan=3 class=hed>{0}</td></tr>\n", GCStrings.getString(24));

            //sb.AppendCFormat("<table align=center>");
            for (int o = 0; o < TRESULT_APP_CELEBS; o++)
            {
                sb.AppendFormat("<tr><td>Gaurabda {0:###}</td><td>&nbsp;&nbsp;:&nbsp;&nbsp;</td><td><b>{1} {2} {3}</b></td></tr>", this.celeb_gy[o], this.celeb_date[o].day,
                    GregorianDateTime.GetMonthAbreviation(this.celeb_date[o].month),
                    this.celeb_date[o].year);
            }
            sb.AppendString("</table>");
            sb.AppendString("</td></tr></table>\n\n");
            sb.AppendFormat("<hr align=center width=\"50%%\">\n<p style=\'text-align:center;font-size:8pt\'>Generated by {0}</p>", GCStrings.getString(130));
            sb.AppendString("</body></html>");

        }

    }
}
