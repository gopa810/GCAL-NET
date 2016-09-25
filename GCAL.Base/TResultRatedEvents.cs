using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Xml;

using GCAL.Base.Scripting;

namespace GCAL.Base
{
    public class TResultRatedEvents: TResultBase
    {
        public CLocationRef EarthLocation;
        public GregorianDateTime DateStart;
        public GregorianDateTime DateEnd;
        public bool ShowAboveOnly = false;
        public double RatingsAboveOnly = -100.0;
        public bool ShowLongerThan = false;
        public double MinIntervalLength = 0.0;

        private List<TDayEvent> p_events = new List<TDayEvent>();
        private List<GCRatedMoment> p_ratings = new List<GCRatedMoment>();
        public List<GCRatedInterval> Intervals = new List<GCRatedInterval>(); 

        public bool AddEvent(GregorianDateTime time, int inType, int inData, int inDst)
        {
            TDayEvent de = new TDayEvent();

            de.Time = new GregorianDateTime(time);

            switch (inDst)
            {
                case 0:
                    de.nDst = 0;
                    break;
                case 1:
                    if (de.Time.shour >= 2 / 24.0)
                    {
                        de.Time.shour += 1 / 24.0;
                        de.Time.NormalizeValues();
                        de.nDst = 1;
                    }
                    else
                    {
                        de.nDst = 0;
                    }
                    break;
                case 2:
                    de.Time.shour += 1 / 24.0;
                    de.Time.NormalizeValues();
                    de.nDst = 1;
                    break;
                case 3:
                    if (de.Time.shour <= 2 / 24.0)
                    {
                        de.Time.shour += 1 / 24.0;
                        de.Time.NormalizeValues();
                        de.nDst = 1;
                    }
                    else
                    {
                        de.nDst = 0;
                    }
                    break;
                default:
                    de.nDst = 0;
                    break;
            }

            de.nData = inData;
            de.nType = inType;

            p_events.Add(de);

            return true;
        }

        public override GSCore GetPropertyValue(string s)
        {
            switch (s)
            {
                case "startDate":
                    return DateStart;
                case "endDate":
                    return DateEnd;
                case "showLongerThan":
                    return new GSBoolean(ShowLongerThan);
                case "showAboveOnly":
                    return new GSBoolean(ShowAboveOnly);
                case "ratingsAboveOnly":
                    return new GSNumber(RatingsAboveOnly);
                case "minIntervalLength":
                    return new GSNumber(MinIntervalLength);
                case "location":
                    return EarthLocation;
                case "events":
                    return new GSList(p_events);
                case "intervals":
                    return new GSList(Intervals);
                default:
                    return base.GetPropertyValue(s);
            }
        }

        public void Sort()
        {
            TDayEventComparer dec = new TDayEventComparer();
            dec.SortType = TResultEvents.SORTING_BY_DATE;
            p_events.Sort(dec);
        }

        public void AddRating(GregorianDateTime julian, GCConfigRatedEntry now, GCConfigRatedEntry prev)
        {
            if (prev.Rating != now.Rating || now.Rating != 0.0)
            {
                GCRatedMoment m = new GCRatedMoment();
                m.JulianDay = new GregorianDateTime(julian);
                //m.Entry = now;
                m.Title = now.Title;
                m.Rating = now.Rating;
                m.Note = now.Note;
                m.Key = now.Key;
                p_ratings.Add(m);
            }

            if (now.Margins != null)
            {
                int counter = 0;
                foreach (GCConfigRatedMargin e in now.Margins)
                {
                    if (e.Rating != 0.0)
                    {
                        GCRatedMoment m = new GCRatedMoment();
                        //m.Entry = e;
                        m.Title = e.Title;
                        m.Rating = e.Rating;
                        m.Note = e.Note;
                        m.JulianDay = julian.TimeWithOffset(e.OffsetMinutesStart/1440.0);
                        m.Key = now.Key + ".s" + counter.ToString();
                        p_ratings.Add(m);

                        m = new GCRatedMoment();
                        //m.Entry = e;
                        m.Title = "";
                        m.Rating = 0.0;
                        m.Note = null;
                        m.Key = now.Key + ".s" + counter.ToString();
                        m.JulianDay = julian.TimeWithOffset(e.OffsetMinutesEnd / 1440.0);
                        p_ratings.Add(m);
                    }
                    counter++;
                }
            }
        }

        private int Prev(int a, int max)
        {
            return (a + (max - 1)) % max;
        }

        private void CalculateEvents(CLocationRef loc, GregorianDateTime vcStart, GregorianDateTime vcEnd, GCConfigRatedEvents rec)
        {
            GregorianDateTime vc = new GregorianDateTime(vcStart);
            GCSunData sun = new GCSunData();
            int ndst = 0;

            ShowAboveOnly = rec.useAcceptLimit;
            ShowLongerThan = rec.useMinPeriodLength;
            RatingsAboveOnly = rec.acceptLimit;
            MinIntervalLength = rec.minPeriodLength;

            bool hasAscendent = rec.RequiredGrahaRasi(9);
            bool hasRahuKalam = rec.rateKalas[KalaType.KT_RAHU_KALAM].Usable;
            bool hasYamaghanti = rec.rateKalas[KalaType.KT_YAMA_GHANTI].Usable;
            bool hasGulikalam = rec.rateKalas[KalaType.KT_GULI_KALAM].Usable;
            bool hasAbhijit = rec.rateKalas[KalaType.KT_ABHIJIT].Usable;
            bool hasMuhurta = rec.RequiredMuhurta();

            p_events.Clear();
            EarthLocation = new CLocationRef(loc);
            DateStart = new GregorianDateTime(vcStart);
            DateEnd = new GregorianDateTime(vcEnd);

            bool hasPreviousSunset = false;
            GregorianDateTime previousSunset = new GregorianDateTime();
            GregorianDateTime vcAdd = new GregorianDateTime(vcStart);
            GregorianDateTime vcNext = new GregorianDateTime();
            GCEarthData earth = loc.EARTHDATA();

            vcAdd.InitWeekDay();

            double sunRise, sunSet;
            double r1, r2;
            double previousLongitude = -100;
            double previousSunriseHour = 0, todaySunriseHour;
            double todayLongitude = 0;
            double fromTimeLimit = 0;
            double muhurtaLength = 48.0 / 1440.0;

            GregorianDateTime muhurtaDate = new GregorianDateTime();

            while (vcAdd.IsBeforeThis(vcEnd))
            {
                ndst = TTimeZone.determineDaylightStatus(vcAdd, loc.timezoneId);
                sun.SunCalc(vcAdd, earth);

                vcAdd.shour = sunRise = sun.rise.GetDayTime(ndst);
                vcAdd.InitWeekDay();

                //AddEvent(vcAdd, CoreEventType.CCTYPE_S_RISE, 0, ndst);
                AddRating(vcAdd, rec.rateDayHours[0], rec.rateDayHours[3]);
                AddRating(vcAdd, rec.rateDay[0], rec.rateDayHours[1]);
                //AddEvent(vcAdd, CoreEventType.CCTYPE_DAY_OF_WEEK, vcAdd.dayOfWeek, ndst);
                AddRating(vcAdd, rec.weekday[vcAdd.dayOfWeek], rec.weekday[Prev(vcAdd.dayOfWeek,7)]);

                if (hasPreviousSunset)
                {
                    previousSunset.shour += (vcAdd.shour + 1.0 - previousSunset.shour) / 2;
                    previousSunset.NormalizeValues();
                    //AddEvent(previousSunset, CoreEventType.CCTYPE_S_MIDNIGHT, 0, ndst);
                    AddRating(vcAdd, rec.rateDayHours[3], rec.rateDayHours[2]);
                }

                if (hasMuhurta)
                {
                    muhurtaDate.Set(vcAdd);
                    muhurtaDate.shour -= 2 * muhurtaLength;
                    muhurtaDate.NormalizeValues();
                    for (int j = 0; j < 30; j++)
                    {
                        //AddEvent(muhurtaDate, CoreEventType.CCTYPE_DAY_MUHURTA, (j + 28) % 30, ndst);
                        int mi = (j + 28) % 30;
                        AddRating(muhurtaDate, rec.rateMuhurta[mi], rec.rateMuhurta[Prev(mi, 30)]);
                    }

                }

                vcAdd.shour = sun.noon.GetDayTime(ndst);
                //AddEvent(vcAdd, CoreEventType.CCTYPE_S_NOON, 0, ndst);
                AddRating(vcAdd, rec.rateDayHours[1], rec.rateDayHours[0]);

                vcAdd.shour = sunSet = sun.set.GetDayTime(ndst);
                //AddEvent(vcAdd, CoreEventType.CCTYPE_S_SET, 0, ndst);
                AddRating(vcAdd, rec.rateDayHours[2], rec.rateDayHours[1]);
                AddRating(vcAdd, rec.rateDay[1], rec.rateDayHours[0]);
                previousSunset.Set(vcAdd);
                hasPreviousSunset = true;

                if (hasAscendent)
                {
                    todayLongitude = sun.longitude_deg;
                    vcAdd.shour = sunRise;
                    todaySunriseHour = sunRise;
                    if (previousLongitude < -10)
                    {
                        GregorianDateTime prevSunrise = new GregorianDateTime();
                        prevSunrise.Set(vcAdd);
                        prevSunrise.PreviousDay();
                        sun.SunCalc(prevSunrise, earth);
                        previousSunriseHour = sun.rise.GetDayTime() - 1;
                        previousLongitude = sun.longitude_deg;
                        fromTimeLimit = 0;
                    }

                    double a, b;
                    double jd = vcAdd.GetJulianComplete();
                    double ayan = GCAyanamsha.GetAyanamsa(jd);
                    r1 = GCMath.putIn360(previousLongitude - ayan) / 30;
                    r2 = GCMath.putIn360(todayLongitude - ayan) / 30;

                    while (r2 > r1 + 13)
                    {
                        r2 -= 12.0;
                    }
                    while (r2 < r1 + 11)
                    {
                        r2 += 12.0;
                    }

                    a = (r2 - r1) / (todaySunriseHour - previousSunriseHour);
                    b = r2 - a * todaySunriseHour;

                    for (double tr = Math.Floor(r1) + 1.0; tr < r2; tr += 1.0)
                    {
                        double tm = (tr - b) / a;
                        if (tm > fromTimeLimit)
                        {
                            vcNext.Set(vcAdd);
                            vcNext.shour = tm;
                            vcNext.NormalizeValues();
                            //AddEvent(vcNext, CoreEventType.CCTYPE_ASCENDENT, (int)tr, ndst);
                            AddRating(vcNext, rec.rateGrahaRasi[9, (int)tr], rec.rateGrahaRasi[9, Prev((int)tr, 12)]);
                        }
                    }

                    previousLongitude = todayLongitude;
                    previousSunriseHour = todaySunriseHour - 1;
                    fromTimeLimit = previousSunriseHour;
                }

                if (hasRahuKalam)
                {
                    CalculateKalam(ndst, vcAdd, sunRise, sunSet, KalaType.KT_RAHU_KALAM, rec);
                }

                if (hasYamaghanti)
                {
                    CalculateKalam(ndst, vcAdd, sunRise, sunSet, KalaType.KT_YAMA_GHANTI, rec);
                }

                if (hasGulikalam)
                {
                    CalculateKalam(ndst, vcAdd, sunRise, sunSet, KalaType.KT_GULI_KALAM, rec);
                }

                if (hasAbhijit)
                {
                    CalculateKalam(ndst, vcAdd, sunRise, sunSet, KalaType.KT_ABHIJIT, rec);
                }

                vcAdd.NextDay();
            }

            if (rec.RequiredTithi())
            {
                vcAdd.Set(vc);
                vcAdd.shour = 0.0;
                CalculateTithis(loc, vcEnd, vcAdd, earth, rec);
            }


            if (rec.RequiredNaksatra())
            {
                vcAdd.Set(vc);
                vcAdd.shour = 0.0;
                CalculateNaksatras(loc, vcEnd, vcAdd, earth, rec);
            }

            if (rec.RequiredYoga())
            {
                vcAdd.Set(vc);
                vcAdd.shour = 0.0;
                CalculateYoga(loc, vcEnd, vcAdd, earth, rec);
            }

            if (rec.RequiredGrahaRasi(0))
            {
                vcAdd.Set(vc);
                vcAdd.shour = 0.0;
                CalculateSunRasi(loc, vcEnd, vcAdd, rec);
            }

            if (rec.RequiredGrahaRasi(1))
            {
                vcAdd.Set(vc);
                vcAdd.shour = 0.0;
                CalculateMoonRasi(loc, vcEnd, vcAdd, earth, rec);

            }

            if (rec.RequiredMoonTimes())
            {
                vcAdd.Set(vc);
                vcAdd.shour = 0.0;
                CalculateMoonTimes(vcEnd, ndst, vcAdd, earth, rec);
            }

            IComparer<GCRatedMoment> C = new GCRatedMoment.ComparerClass();
            p_ratings.Sort(C);

        }


        private int CalculateTithis(CLocationRef loc, GregorianDateTime vcEnd, GregorianDateTime vcAdd, GCEarthData earth, GCConfigRatedEvents rec)
        {
            int nData = 0;
            GregorianDateTime vcNext;
            int ndst;

            vcAdd.SubtractDays(1);
            while (vcAdd.IsBeforeThis(vcEnd))
            {
                nData = GCTithi.GetNextTithiStart(earth, vcAdd, out vcNext);
                if (vcNext.GetDayInteger() < vcEnd.GetDayInteger())
                {
                    vcNext.InitWeekDay();
                    ndst = TTimeZone.determineDaylightStatus(vcNext, loc.timezoneId);
                    vcNext.AddHours(ndst);
                    //AddEvent(vcNext, CoreEventType.CCTYPE_TITHI, nData, ndst);
                    AddRating(vcNext, rec.rateTithi[nData], rec.rateTithi[Prev(nData, 30)]);
                }
                else
                {
                    break;
                }
                vcAdd.Set(vcNext);
                vcAdd.shour += 0.2;
                if (vcAdd.shour >= 1.0)
                {
                    vcAdd.shour -= 1.0;
                    vcAdd.NextDay();
                }
            }
            return nData;
        }

        private int CalculateNaksatras(CLocationRef loc, GregorianDateTime vcEnd, GregorianDateTime vcAdd, GCEarthData earth, GCConfigRatedEvents rec)
        {
            int nData = 0;
            GregorianDateTime vcNext;
            int ndst;
            bool prevNaksatraValid = false;
            GregorianDateTime prevNaksatra = new GregorianDateTime();

            vcAdd.SubtractDays(1);
            while (vcAdd.IsBeforeThis(vcEnd))
            {
                nData = GCNaksatra.GetNextNaksatra(earth, vcAdd, out vcNext);
                if (vcNext.GetDayInteger() < vcEnd.GetDayInteger())
                {
                    vcNext.InitWeekDay();
                    vcNext.AddHours(TTimeZone.determineDaylightStatus(vcNext, loc.timezoneId));
                    //AddEvent(vcNext, CoreEventType.CCTYPE_NAKS, nData, ndst);
                    AddRating(vcNext, rec.rateNaksatra[nData], rec.rateNaksatra[Prev(nData, 27)]);

                    if (prevNaksatraValid)
                    {
                        double padaLength = (vcNext.GetJulianComplete() - prevNaksatra.GetJulianComplete()) / 4.0;

                        for (int j = 0; j < 4; j++)
                        {
                            //AddEvent(prevNaksatra, CoreEventType.CCTYPE_NAKS_PADA1 + j, nData, ndst);
                            int prevPada = (nData*4 + j + 107) % 108;
                            AddRating(vcNext, rec.rateNaksatraPada[nData, j], rec.rateNaksatraPada[prevPada / 4, prevPada % 4]);
                            prevNaksatra.shour += padaLength;
                            prevNaksatra.NormalizeValues();
                        }
                    }

                    prevNaksatra.Set(vcNext);
                    prevNaksatraValid = true;
                }
                else
                {
                    break;
                }
                vcAdd.Set(vcNext);
                vcAdd.shour += 0.2;
                if (vcAdd.shour >= 1.0)
                {
                    vcAdd.shour -= 1.0;
                    vcAdd.NextDay();
                }
            }
            return nData;
        }

        private int CalculateYoga(CLocationRef loc, GregorianDateTime vcEnd, GregorianDateTime vcAdd, GCEarthData earth, GCConfigRatedEvents rec)
        {
            int nData = 0;
            GregorianDateTime vcNext = new GregorianDateTime();
            int ndst;

            vcAdd.SubtractDays(1);
            while (vcAdd.IsBeforeThis(vcEnd))
            {
                nData = GCYoga.GetNextYogaStart(earth, vcAdd, out vcNext);
                if (vcNext.GetDayInteger() < vcEnd.GetDayInteger())
                {
                    vcNext.InitWeekDay();
                    vcNext.AddHours(TTimeZone.determineDaylightStatus(vcNext, loc.timezoneId));
                    //AddEvent(vcNext, CoreEventType.CCTYPE_YOGA, nData, ndst);
                    AddRating(vcNext, rec.rateYoga[nData], rec.rateYoga[Prev(nData, 27)]);
                }
                else
                {
                    break;
                }
                vcAdd.Set(vcNext);
                vcAdd.shour += 0.2;
                if (vcAdd.shour >= 1.0)
                {
                    vcAdd.shour -= 1.0;
                    vcAdd.NextDay();
                }
            }
            return nData;
        }

        private void CalculateSunRasi(CLocationRef loc, GregorianDateTime vcEnd, GregorianDateTime vcAdd, GCConfigRatedEvents rec)
        {
            int nData;
            GregorianDateTime vcNext = new GregorianDateTime();
            int ndst;

            vcAdd.SubtractDays(30);

            while (vcAdd.IsBeforeThis(vcEnd))
            {
                vcNext.Set(GCSankranti.GetNextSankranti(vcAdd, out nData));
                if (vcNext.GetDayInteger() < vcEnd.GetDayInteger())
                {
                    vcNext.InitWeekDay();
                    vcNext.AddHours(TTimeZone.determineDaylightStatus(vcNext, loc.timezoneId));
                    //AddEvent(vcNext, CoreEventType.CCTYPE_SANK, nData, ndst);
                    AddRating(vcNext, rec.rateGrahaRasi[0, nData], rec.rateGrahaRasi[0, Prev(nData, 12)]);
                }
                else
                {
                    break;
                }
                vcAdd.Set(vcNext);
                vcAdd.NextDay();
            }
        }

        private void CalculateMoonRasi(CLocationRef loc, GregorianDateTime vcEnd, GregorianDateTime vcAdd, GCEarthData earth, GCConfigRatedEvents rec)
        {
            int nData;
            GregorianDateTime vcNext = new GregorianDateTime();

            vcAdd.SubtractDays(4);
            while (vcAdd.IsBeforeThis(vcEnd))
            {
                nData = GCMoonData.GetNextMoonRasi(earth, vcAdd, out vcNext);
                if (vcNext.GetDayInteger() < vcEnd.GetDayInteger())
                {
                    vcNext.InitWeekDay();
                    vcNext.AddHours(TTimeZone.determineDaylightStatus(vcNext, loc.timezoneId));
                    //AddEvent(vcNext, CoreEventType.CCTYPE_M_RASI, nData, ndst);
                    AddRating(vcNext, rec.rateGrahaRasi[1, nData], rec.rateGrahaRasi[1, Prev(nData, 12)]);
                }
                else
                {
                    break;
                }
                vcAdd.Set(vcNext);
                vcAdd.shour += 0.5;
                vcAdd.NormalizeValues();
            }
        }

        private void CalculateMoonTimes(GregorianDateTime vcEnd, int ndst, GregorianDateTime vcAdd, GCEarthData earth, GCConfigRatedEvents rec)
        {
            GregorianDateTime vcNext = new GregorianDateTime();

            vcAdd.SubtractDays(2);
            while (vcAdd.IsBeforeThis(vcEnd))
            {
                vcNext.Set(GCMoonData.GetNextRise(earth, vcAdd, true));
                vcNext.AddHours(TTimeZone.determineDaylightStatus(vcNext, earth.timezoneId));
                //AddEvent(vcNext, CoreEventType.CCTYPE_M_RISE, 0, ndst);
                AddRating(vcNext, rec.rateMoonTime[0], rec.rateMoonTime[1]);

                vcNext.Set(GCMoonData.GetNextRise(earth, vcNext, false));
                vcNext.AddHours(TTimeZone.determineDaylightStatus(vcNext, earth.timezoneId));
                //AddEvent(vcNext, CoreEventType.CCTYPE_M_SET, 0, ndst);
                AddRating(vcNext, rec.rateMoonTime[1], rec.rateMoonTime[0]);

                vcNext.shour += 0.05;
                vcNext.NormalizeValues();
                vcAdd.Set(vcNext);
            }
        }

        private void CalculateKalam(int ndst, GregorianDateTime vcAdd, double sunRise, double sunSet, int kalaType, GCConfigRatedEvents rec)
        {
            double r1, r2;
            GCSunData.CalculateKala(sunRise, sunSet, vcAdd.dayOfWeek, out r1, out r2, kalaType);

            if (r1 > 0 && r2 > 0)
            {
                vcAdd.shour = r1;
                //AddEvent(vcAdd, CoreEventType.CCTYPE_KALA_START, kalaType, ndst);
                AddRating(vcAdd, rec.rateKalas[kalaType], rec.rateKalas[KalaType.KT_NONE]);

                vcAdd.shour = r2;
                //AddEvent(vcAdd, CoreEventType.CCTYPE_KALA_END, kalaType, ndst);
                AddRating(vcAdd, rec.rateKalas[KalaType.KT_NONE], rec.rateKalas[kalaType]);
            }
        }


        public void CreateRatedList(GCConfigRatedEvents rec)
        {
            StringBuilder sb = new StringBuilder();
            List<string> nt = new List<string>();
            Intervals.Clear();

            Dictionary<string, GCRatedMoment> notes = new Dictionary<string, GCRatedMoment>();
            GCRatedMoment lastRec = null;
            double lastMoment = -1;
            double thisMoment = 0;
            foreach (GCRatedMoment rm in p_ratings)
            {
                thisMoment = rm.JulianDay.GetJulianComplete();
                if (lastMoment < 0)
                {
                    lastMoment = thisMoment;
                    lastRec = rm;
                }

                // test if this period is acceptable
                if ((rec.useMinPeriodLength && rec.minPeriodLength < (thisMoment - lastMoment) * 1440.0)
                    || !rec.useMinPeriodLength)
                {
                    GCRatedInterval gi = new GCRatedInterval();
                    gi.startTime = lastRec.JulianDay;
                    gi.endTime = rm.JulianDay;
                    nt.Clear();
                    sb.Clear();
                    foreach (string k in notes.Keys)
                    {
                        GCRatedMoment b = notes[k];
                        if (b.Rating < 0.0) gi.ratingNeg -= b.Rating;
                        else if (b.Rating > 0.0) gi.ratingPos += b.Rating;

                        if (b.Note != null)
                            nt.Add(b.Note);

                        if (sb.Length > 0)
                            sb.Append(", ");
                        sb.Append(b.Title);
                    }

                    // last test for acceptance limit
                    if (!rec.useAcceptLimit || (gi.ratingPos + gi.ratingNeg > rec.acceptLimit))
                    {
                        if (sb.Length > 0)
                        {
                            if (nt.Count > 0)
                                gi.Notes = nt.ToArray<string>();
                            gi.Title = sb.ToString();
                            Intervals.Add(gi);
                        }
                    }
                }

                if (rm.Key != null)
                {
                    notes[rm.Key] = rm;
                }

                lastMoment = thisMoment;
                lastRec = rm;

            }

        }

        public void CompleteCalculation(CLocationRef loc, GregorianDateTime vcStart, GregorianDateTime vcEnd, GCConfigRatedEvents rec)
        {
            // calculate raw events
            CalculateEvents(loc, vcStart, vcEnd, rec);

            // sort them
            Sort();

            foreach (GCRatedMoment tde in p_ratings)
            {
                Debugger.Log(0, "", " DE: " + tde.ToString() + "\n");
            }

            // create rated time intervals
            CreateRatedList(rec);

        }

        public override string formatText(string templateName)
        {
            GSScript script = new GSScript();
            switch (templateName)
            {
                case GCDataFormat.PlainText:
                    script.readTextTemplate(Properties.Resources.TplRatedPlain);
                    break;
                case GCDataFormat.Rtf:
                    script.readTextTemplate(Properties.Resources.TplRatedRtf);
                    break;
                case GCDataFormat.HTML:
                    script.readTextTemplate(Properties.Resources.TplRatedHtml);
                    break;
                case GCDataFormat.XML:
                    script.readTextTemplate(Properties.Resources.TplRatedXml);
                    break;
                case GCDataFormat.CSV:
                    script.readTextTemplate(Properties.Resources.TplRatedCsv);
                    break;
                default:
                    break;
            }


            GSExecutor engine = new GSExecutor();
            engine.SetVariable("events", this);
            engine.SetVariable("location", this.EarthLocation);
            engine.SetVariable("app", GCUserInterface.Shared);
            engine.ExecuteElement(script);


            return engine.getOutput();
        }

        public override TResultFormatCollection getFormats()
        {
            TResultFormatCollection coll = base.getFormats();

            coll.ResultName = "RatedEvents";
            coll.Formats.Add(new TResultFormat("Text File", "txt", GCDataFormat.PlainText));
            coll.Formats.Add(new TResultFormat("Rich Text File", "rtf", GCDataFormat.Rtf));
            coll.Formats.Add(new TResultFormat("XML File", "xml", GCDataFormat.XML));
            coll.Formats.Add(new TResultFormat("Comma Separated Values", "csv", GCDataFormat.CSV));
            coll.Formats.Add(new TResultFormat("HTML File (in List format)", "htm", GCDataFormat.HTML));
            return coll;
        }
    
    }
}
