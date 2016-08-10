using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Xml;

namespace GCAL.Base
{
    public class TResultRatedEvents
    {
        public CLocationRef EarthLocation;
        public GregorianDateTime DateStart;
        public GregorianDateTime DateEnd;
        public bool ShowAboveOnly = false;
        public double RatingsAboveOnly = -100.0;
        public bool ShowLongerThan = false;
        public double IntervaleLengthBelowRemove = 0.0;
        public bool OnlyPositiveRatings = false;

        private List<TDayEvent> p_events = new List<TDayEvent>();
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

        public void Sort()
        {
            TDayEventComparer dec = new TDayEventComparer();
            dec.SortType = TResultEvents.SORTING_BY_DATE;
            p_events.Sort(dec);
        }

        public void CalculateEvents(CLocationRef loc, GregorianDateTime vcStart, GregorianDateTime vcEnd)
        {
            GregorianDateTime vc = new GregorianDateTime();
            GCSunData sun = new GCSunData();
            int ndst = 0;

            OnlyPositiveRatings = GCRatedEventsList.ShowOnlyPositive;
            ShowAboveOnly = GCRatedEventsList.ShowOnlyAboveLevel;
            ShowLongerThan = GCRatedEventsList.ShowPeriodLongerThan;
            RatingsAboveOnly = GCRatedEventsList.AboveLevelValue;
            IntervaleLengthBelowRemove = GCRatedEventsList.PeriodLongerValue;

            bool hasSandhya0 = GCRatedEventsList.HasItemWithType(CoreEventType.CCTYPE_KALA_START, KalaType.KT_SANDHYA_SUNRISE);
            bool hasSandhya1 = GCRatedEventsList.HasItemWithType(CoreEventType.CCTYPE_KALA_START, KalaType.KT_SANDHYA_NOON);
            bool hasSandhya2 = GCRatedEventsList.HasItemWithType(CoreEventType.CCTYPE_KALA_START, KalaType.KT_SANDHYA_SUNSET);
            bool hasSandhya3 = GCRatedEventsList.HasItemWithType(CoreEventType.CCTYPE_KALA_START, KalaType.KT_SANDHYA_MIDNIGHT);
            bool hasAscendent = GCRatedEventsList.HasItemWithType(CoreEventType.CCTYPE_ASCENDENT);
            bool hasRahuKalam = GCRatedEventsList.HasItemWithType(CoreEventType.CCTYPE_KALA_START, KalaType.KT_RAHU_KALAM);
            bool hasYamaghanti = GCRatedEventsList.HasItemWithType(CoreEventType.CCTYPE_KALA_START, KalaType.KT_YAMA_GHANTI);
            bool hasGulikalam = GCRatedEventsList.HasItemWithType(CoreEventType.CCTYPE_KALA_START, KalaType.KT_GULI_KALAM);
            bool hasAbhijit = GCRatedEventsList.HasItemWithType(CoreEventType.CCTYPE_KALA_START, KalaType.KT_ABHIJIT);
            bool hasNaksatraPada = GCRatedEventsList.HasItemTypeRange(CoreEventType.CCTYPE_NAKS_PADA1, CoreEventType.CCTYPE_NAKS_PADA4);
            bool hasMuhurta = GCRatedEventsList.HasItemWithType(CoreEventType.CCTYPE_DAY_MUHURTA);

            p_events.Clear();
            EarthLocation = new CLocationRef();
            EarthLocation.Set(loc);
            DateStart = new GregorianDateTime(vcStart);
            DateEnd = new GregorianDateTime(vcEnd);

            bool hasPreviousSunset = false;
            GregorianDateTime previousSunset = new GregorianDateTime();
            GregorianDateTime vcAdd = new GregorianDateTime(), vcNext = new GregorianDateTime();
            GCEarthData earth = loc.EARTHDATA();

            vc.Set(vcStart);

            vcAdd.Set(vc);
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
                ndst = TTimeZone.determineDaylightChange(vcAdd, loc.timezoneId);
                sun.SunCalc(vcAdd, earth);

                vcAdd.shour = sun.arunodaya.GetDayTime();
                AddEvent(vcAdd, CoreEventType.CCTYPE_S_ARUN, 0, ndst);

                vcAdd.shour = sunRise = sun.rise.GetDayTime();
                AddEvent(vcAdd, CoreEventType.CCTYPE_S_RISE, 0, ndst);
                vcAdd.InitWeekDay();

                AddEvent(vcAdd, CoreEventType.CCTYPE_DAY_OF_WEEK, vcAdd.dayOfWeek, ndst);

                if (hasPreviousSunset)
                {
                    previousSunset.shour += (vcAdd.shour + 1.0 - previousSunset.shour) / 2;
                    previousSunset.NormalizeValues();
                    AddEvent(previousSunset, CoreEventType.CCTYPE_S_MIDNIGHT, 0, ndst);

                    if (hasSandhya3)
                    {
                        previousSunset.shour -= 24.0 / 1440.0;
                        AddEvent(previousSunset, CoreEventType.CCTYPE_KALA_START, KalaType.KT_SANDHYA_MIDNIGHT, ndst);
                        previousSunset.shour += muhurtaLength;
                        AddEvent(previousSunset, CoreEventType.CCTYPE_KALA_END, KalaType.KT_SANDHYA_MIDNIGHT, ndst);
                    }
                }

                if (hasMuhurta)
                {
                    muhurtaDate.Set(vcAdd);
                    muhurtaDate.shour -= 2 * muhurtaLength;
                    muhurtaDate.NormalizeValues();
                    for (int j = 0; j < 30; j++)
                    {
                        AddEvent(muhurtaDate, CoreEventType.CCTYPE_DAY_MUHURTA, (j + 28) % 30, ndst);
                    }

                }

                if (hasSandhya0)
                {
                    vcAdd.shour -= 24.0 / 1440.0;
                    AddEvent(vcAdd, CoreEventType.CCTYPE_KALA_START, KalaType.KT_SANDHYA_SUNRISE, ndst);
                    vcAdd.shour += muhurtaLength;
                    AddEvent(vcAdd, CoreEventType.CCTYPE_KALA_END, KalaType.KT_SANDHYA_SUNRISE, ndst);
                }

                vcAdd.shour = sun.noon.GetDayTime();
                AddEvent(vcAdd, CoreEventType.CCTYPE_S_NOON, 0, ndst);

                if (hasSandhya1)
                {
                    vcAdd.shour -= 24.0 / 1440.0;
                    AddEvent(vcAdd, CoreEventType.CCTYPE_KALA_START, KalaType.KT_SANDHYA_NOON, ndst);
                    vcAdd.shour += muhurtaLength;
                    AddEvent(vcAdd, CoreEventType.CCTYPE_KALA_END, KalaType.KT_SANDHYA_NOON, ndst);
                }

                vcAdd.shour = sunSet = sun.set.GetDayTime();
                AddEvent(vcAdd, CoreEventType.CCTYPE_S_SET, 0, ndst);
                previousSunset.Set(vcAdd);
                hasPreviousSunset = true;

                if (hasSandhya2)
                {
                    vcAdd.shour -= 24.0 / 1440.0;
                    AddEvent(vcAdd, CoreEventType.CCTYPE_KALA_START, KalaType.KT_SANDHYA_SUNSET, ndst);
                    vcAdd.shour += 48.0 / 1440.0;
                    AddEvent(vcAdd, CoreEventType.CCTYPE_KALA_END, KalaType.KT_SANDHYA_SUNSET, ndst);
                }

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
                            AddEvent(vcNext, CoreEventType.CCTYPE_ASCENDENT, (int)tr, ndst);
                        }
                    }

                    previousLongitude = todayLongitude;
                    previousSunriseHour = todaySunriseHour - 1;
                    fromTimeLimit = previousSunriseHour;
                }

                if (hasRahuKalam)
                {
                    CalculateKalam(ndst, vcAdd, sunRise, sunSet, KalaType.KT_RAHU_KALAM);
                }

                if (hasYamaghanti)
                {
                    CalculateKalam(ndst, vcAdd, sunRise, sunSet, KalaType.KT_YAMA_GHANTI);
                }

                if (hasGulikalam)
                {
                    CalculateKalam(ndst, vcAdd, sunRise, sunSet, KalaType.KT_GULI_KALAM);
                }

                if (hasAbhijit)
                {
                    CalculateKalam(ndst, vcAdd, sunRise, sunSet, KalaType.KT_ABHIJIT);
                }

                vcAdd.NextDay();
            }

            if (GCRatedEventsList.HasItemWithType(CoreEventType.CCTYPE_TITHI) ||
                GCRatedEventsList.HasItemWithType(CoreEventType.CCTYPE_TITHI_BASE))
            {
                vcAdd.Set(vc);
                vcAdd.shour = 0.0;
                CalculateTithis(loc, vcEnd, vcAdd, earth);
            }


            if (GCRatedEventsList.HasItemWithType(CoreEventType.CCTYPE_NAKS) || hasNaksatraPada)
            {
                vcAdd.Set(vc);
                vcAdd.shour = 0.0;
                CalculateNaksatras(loc, vcEnd, hasNaksatraPada, vcAdd, earth);
            }

            if (GCRatedEventsList.HasItemWithType(CoreEventType.CCTYPE_YOGA))
            {
                vcAdd.Set(vc);
                vcAdd.shour = 0.0;
                CalculateYoga(loc, vcEnd, vcAdd, earth);
            }

            if (GCRatedEventsList.HasItemWithType(CoreEventType.CCTYPE_SANK))
            {
                vcAdd.Set(vc);
                vcAdd.shour = 0.0;
                CalculateSunRasi(loc, vcEnd, vcAdd);
            }

            if (GCRatedEventsList.HasItemWithType(CoreEventType.CCTYPE_M_RASI))
            {
                vcAdd.Set(vc);
                vcAdd.shour = 0.0;
                CalculateMoonRasi(loc, vcEnd, vcAdd, earth);

            }

            if (GCRatedEventsList.HasItemTypeRange(CoreEventType.CCTYPE_M_RISE, CoreEventType.CCTYPE_M_SET))
            {
                vcAdd.Set(vc);
                vcAdd.shour = 0.0;
                CalculateMoonTimes(vcEnd, ndst, vcAdd, earth);
            }

        }

        private int CalculateTithis(CLocationRef loc, GregorianDateTime vcEnd, GregorianDateTime vcAdd, GCEarthData earth)
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
                    ndst = TTimeZone.determineDaylightChange(vcNext, loc.timezoneId);
                    AddEvent(vcNext, CoreEventType.CCTYPE_TITHI, nData, ndst);
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

        private int CalculateNaksatras(CLocationRef loc, GregorianDateTime vcEnd, bool hasNaksatraPada, GregorianDateTime vcAdd, GCEarthData earth)
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
                    ndst = TTimeZone.determineDaylightChange(vcNext, loc.timezoneId);
                    AddEvent(vcNext, CoreEventType.CCTYPE_NAKS, nData, ndst);

                    if (hasNaksatraPada && prevNaksatraValid)
                    {
                        double padaLength = (vcNext.GetJulianComplete() - prevNaksatra.GetJulianComplete()) / 4.0;

                        for (int j = 0; j < 4; j++)
                        {
                            AddEvent(prevNaksatra, CoreEventType.CCTYPE_NAKS_PADA1 + j, nData, ndst);
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

        private int CalculateYoga(CLocationRef loc, GregorianDateTime vcEnd, GregorianDateTime vcAdd, GCEarthData earth)
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
                    ndst = TTimeZone.determineDaylightChange(vcNext, loc.timezoneId);
                    AddEvent(vcNext, CoreEventType.CCTYPE_YOGA, nData, ndst);
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

        private void CalculateSunRasi(CLocationRef loc, GregorianDateTime vcEnd, GregorianDateTime vcAdd)
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
                    ndst = TTimeZone.determineDaylightChange(vcNext, loc.timezoneId);
                    AddEvent(vcNext, CoreEventType.CCTYPE_SANK, nData, ndst);
                }
                else
                {
                    break;
                }
                vcAdd.Set(vcNext);
                vcAdd.NextDay();
            }
        }

        private void CalculateMoonRasi(CLocationRef loc, GregorianDateTime vcEnd, GregorianDateTime vcAdd, GCEarthData earth)
        {
            int nData;
            int ndst;
            GregorianDateTime vcNext = new GregorianDateTime();

            vcAdd.SubtractDays(4);
            while (vcAdd.IsBeforeThis(vcEnd))
            {
                nData = GCMoonData.GetNextMoonRasi(earth, vcAdd, out vcNext);
                if (vcNext.GetDayInteger() < vcEnd.GetDayInteger())
                {
                    vcNext.InitWeekDay();
                    ndst = TTimeZone.determineDaylightChange(vcNext, loc.timezoneId);
                    AddEvent(vcNext, CoreEventType.CCTYPE_M_RASI, nData, ndst);
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

        private void CalculateMoonTimes(GregorianDateTime vcEnd, int ndst, GregorianDateTime vcAdd, GCEarthData earth)
        {
            GregorianDateTime vcNext = new GregorianDateTime();

            vcAdd.SubtractDays(2);
            while (vcAdd.IsBeforeThis(vcEnd))
            {
                vcNext.Set(GCMoonData.GetNextRise(earth, vcAdd, true));
                AddEvent(vcNext, CoreEventType.CCTYPE_M_RISE, 0, ndst);

                vcNext.Set(GCMoonData.GetNextRise(earth, vcNext, false));
                AddEvent(vcNext, CoreEventType.CCTYPE_M_SET, 0, ndst);

                vcNext.shour += 0.05;
                vcNext.NormalizeValues();
                vcAdd.Set(vcNext);
            }
        }

        private void CalculateKalam(int ndst, GregorianDateTime vcAdd, double sunRise, double sunSet, int kalaType)
        {
            double r1, r2;
            GCSunData.CalculateKala(sunRise, sunSet, vcAdd.dayOfWeek, out r1, out r2, kalaType);

            if (r1 > 0 && r2 > 0)
            {
                vcAdd.shour = r1;
                AddEvent(vcAdd, CoreEventType.CCTYPE_KALA_START, kalaType, ndst);

                vcAdd.shour = r2;
                AddEvent(vcAdd, CoreEventType.CCTYPE_KALA_END, kalaType, ndst);
            }
        }


        public void CreateRatedList()
        {
            TDayEvent prev = null;
            GCRatedInterval prevInterval = null;
            List<GCRatedEvent> active = new List<GCRatedEvent>();
            List<GCRatedEvent> list = new List<GCRatedEvent>();
            double ratingsPos = 0.0;
            double ratingsNeg = 0.0;
            bool rejected = false;
            List<string> notes = new List<string>();
            StringBuilder sb = new StringBuilder();
            Intervals.Clear();

            foreach (TDayEvent de in p_events)
            {
                Debugger.Log(0, "", string.Format("-----------------------------------\n"));
                Debugger.Log(0, "", string.Format("-DE {0}, data {1}, time {2} ------\n",
                    TDayEvent.GetTypeString(de.nType,de.nData), de.nData, de.Time.ShortTimeString()));
                rejected = false;
                if (prev != null)
                {
                    if (active.Count == 0)
                    {
                        rejected = true;
                    }
                    else
                    {
                        ratingsPos = 0.0;
                        ratingsNeg = 0.0;
                        notes.Clear();
                        sb.Clear();
                        MakeSumActive(active, ref ratingsPos, ref ratingsNeg, ref rejected, notes, sb);
                    }

                    if (!rejected)
                    {
                        if (prevInterval != null && prevInterval.ratingPos == ratingsPos
                            && prevInterval.ratingNeg == ratingsNeg)
                        {
                            prevInterval.endTime = de.Time;
                        }
                        else
                        {
                            GCRatedInterval gi = new GCRatedInterval();
                            gi.startTime = prev.Time;
                            gi.endTime = de.Time;
                            gi.ratingPos = ratingsPos;
                            gi.ratingNeg = ratingsNeg;
                            gi.Notes = notes.ToArray<string>();
                            gi.Title = sb.ToString();

                            if (AcceptableInterval(gi))
                            {
                                prevInterval = gi;
                                Intervals.Add(gi);
                            }
                            else
                            {
                                prevInterval = null;
                            }
                        }
                    }
                    else
                    {
                        prevInterval = null;
                    }
                }
                
                Debugger.Log(0, "", " - intervals - \n\n");
                foreach (GCRatedInterval gi in Intervals)
                {
                    Debugger.Log(0,"", string.Format("  {0} - {1}    {2} {3}\n", gi.startTime.FullString(), gi.endTime.FullString(), gi.ratingPos, gi.ratingNeg));
                }

                Debugger.Log(0, "", "\n - active - \n\n");
                foreach (GCRatedEvent ge in active)
                {
                    Debugger.Log(0, "", string.Format("    ITEM {0} {1}/{2} ", ge.StartEqual ? "start" : "startn",
                        TDayEvent.GetTypeString(ge.StartType,ge.StartData), ge.StartData));
                    Debugger.Log(0, "", string.Format("  {0} {1}/{2} ", ge.EndEqual ? "end" : "endn",
                        TDayEvent.GetTypeString(ge.EndType,ge.EndData) , ge.EndData));
                    Debugger.Log(0, "", string.Format("  rat:{0}\n", ge.Rating));
                }

                // remove old events
                RemoveClosedRatedEvents(active, list, de);

                // add new events
                if (FindStartRatedEventForDayEvent(de, list))
                {
                    Debugger.Log(0, "", "\n - added to active - \n\n");
                    foreach (GCRatedEvent ge in list)
                    {
                        Debugger.Log(0, "", string.Format("    ITEM {0} {1}/{2} ", ge.StartEqual ? "start" : "startn",
                            TDayEvent.GetTypeString(ge.StartType,ge.StartData), ge.StartData));
                        Debugger.Log(0, "", string.Format("  {0} {1}/{2} ", ge.EndEqual ? "end" : "endn",
                            TDayEvent.GetTypeString(ge.EndType,ge.EndData) , ge.EndData));
                        Debugger.Log(0, "", string.Format("  rat:{0}\n", ge.Rating));
                    }
                    active.AddRange(list);
                }

                prev = de;
            }
        }

        private static void MakeSumActive(List<GCRatedEvent> active, ref double ratingsPos, ref double ratingsNeg, ref bool rejected, List<string> notes, StringBuilder sb)
        {
            foreach (GCRatedEvent re in active)
            {
                if (re.Rejected)
                {
                    rejected = true;
                    break;
                }
                if (re.Rating > 0.0)
                    ratingsPos += re.Rating;
                else if (re.Rating < 0.0)
                    ratingsNeg += re.Rating;
                if (re.Note != null && re.Note.Length > 0)
                    notes.Add(re.Note);
                if (re.Title.Length == 0)
                    re.Title = "";

                if (sb.Length > 0)
                {
                    sb.Append(", ");
                }
                sb.Append(re.Title);
            }
        }

        private bool AcceptableInterval(GCRatedInterval gi)
        {
            if (ShowLongerThan && (gi.Length < IntervaleLengthBelowRemove))
                return false;
            if (OnlyPositiveRatings && (gi.ratingNeg < 0.0))
                return false;
            if (ShowAboveOnly && (gi.ratingNeg + gi.ratingPos < RatingsAboveOnly))
                return false;
            return true;
        }

        private static void RemoveClosedRatedEvents(List<GCRatedEvent> active, List<GCRatedEvent> list, TDayEvent de)
        {
            Debugger.Log(0, "", "\n - removed from active - \n\n");
            list.Clear();
            foreach (GCRatedEvent re in active)
            {
                if (!re.MeetsEnd(de))
                    list.Add(re);
                else
                {
                    Debugger.Log(0, "", string.Format("    ITEM {0} {1}/{2} ", re.StartEqual ? "start" : "startn",
                        TDayEvent.GetTypeString(re.StartType,re.StartData) , re.StartData));
                    Debugger.Log(0, "", string.Format("  {0} {1}/{2} ", re.EndEqual ? "end" : "endn",
                        TDayEvent.GetTypeString(re.EndType,re.EndData) , re.EndData));
                    Debugger.Log(0, "", string.Format("  rat:{0}\n", re.Rating));
                }
            }

            active.Clear();
            active.AddRange(list);

            Debugger.Log(0, "", "\n - after removed from active - \n\n");
            foreach (GCRatedEvent ge in active)
            {
                Debugger.Log(0, "", string.Format("    ITEM {0} {1}/{2} ", ge.StartEqual ? "start" : "startn",
                    TDayEvent.GetTypeString(ge.StartType,ge.StartData) , ge.StartData));
                Debugger.Log(0, "", string.Format("  {0} {1}/{2} ", ge.EndEqual ? "end" : "endn",
                    TDayEvent.GetTypeString(ge.EndType, ge.EndData), ge.EndData));
                Debugger.Log(0, "", string.Format("  rat:{0}\n", ge.Rating));
            }

        }

        public bool FindStartRatedEventForDayEvent(TDayEvent de, List<GCRatedEvent> list)
        {
            list.Clear();
            foreach (GCRatedEvent re in GCRatedEventsList.List)
            {
                if (re.StartEqual)
                {
                    if (de.nType == re.StartType && de.nData == re.StartData && re.IsActive())
                    {
                        list.Add(re);
                    }
                }
                else
                {
                    if (de.nType == re.StartType && de.nData != re.StartData && re.IsActive())
                    {
                        list.Add(re);
                    }
                }
            }
            return list.Count > 0;
        }

        public void CompleteCalculation(CLocationRef loc, GregorianDateTime vcStart, GregorianDateTime vcEnd)
        {
            // calculate raw events
            CalculateEvents(loc, vcStart, vcEnd);

            // sort them
            Sort();

            foreach (TDayEvent tde in p_events)
            {
                Debugger.Log(0, "", " DE: " + tde.TypeString + "/" + tde.nData + "  " + tde.Time.FullString() + "\n");
            }

            // create rated time intervals
            CreateRatedList();

        }

        public void formatPlainText(StringBuilder sb)
        {
            sb.AppendLine("RATED EVENTS");
            sb.AppendFormat("From {0} to {1}\n", DateStart.ToString(), DateEnd.ToString());
            sb.AppendFormat("{0}\n", EarthLocation.GetFullName());
            sb.AppendLine();

            int prevDay = -1;

            foreach (GCRatedInterval ri in Intervals)
            {
                if (prevDay != ri.startTime.day)
                    sb.AppendFormat("\n === {0} - {1} ========\n", ri.startTime.ToString().PadLeft(12, ' '),
                        GCCalendar.GetWeekdayName(ri.startTime.dayOfWeek));

                string title = ri.Title;
                if (title.Length > 30)
                    title = title.Substring(30);
                title = title.PadRight(30);

                sb.AppendFormat("      {0} - {1}  {2}  +{3}  {4}\n",
                    ri.startTime.ShortTimeString(),
                    ri.endTime.ShortTimeString(), title, 
                    ri.ratingPos, ri.ratingNeg);

                if (ri.Notes != null)
                {
                    foreach (string s in ri.Notes)
                    {
                        sb.AppendFormat("                 {0}\n", s);
                    }
                }

                prevDay = ri.startTime.day;
            }
        }

        public void formatRichText(StringBuilder res)
        {
            GCStringBuilder sb = new GCStringBuilder();

            sb.Target = res;
            sb.Format = GCStringBuilder.FormatType.RichText;
            sb.fontSizeH1 = GCLayoutData.textSizeH1;
            sb.fontSizeH2 = GCLayoutData.textSizeH2;
            sb.fontSizeText = GCLayoutData.textSizeText;
            sb.fontSizeNote = GCLayoutData.textSizeNote;

            sb.AppendDocumentHeader();
            sb.AppendHeader1("Rated Events");

            sb.AppendLine("From {0} to {1}", DateStart.ToString(), DateEnd.ToString());
            sb.AppendLine(EarthLocation.GetFullName());
            sb.AppendLine();

            int prevDay = -1;

            foreach (GCRatedInterval ri in Intervals)
            {
                if (prevDay != ri.startTime.day)
                {
                    sb.AppendLine();
                    sb.AppendLine(" === {0} - {1} ======================", ri.startTime.ToString().PadLeft(12, ' '),
                        GCCalendar.GetWeekdayName(ri.startTime.dayOfWeek));
                }

                string title = ri.Title;
                if (title.Length > 30)
                    title = title.Substring(30);
                title = title.PadRight(30);

                sb.AppendLine("      {0} - {1}  {2}   +{3}  {4}", ri.startTime.ShortTimeString(),
                    ri.endTime.ShortTimeString(), title, ri.ratingPos, ri.ratingNeg);

                if (ri.Notes != null)
                {
                    foreach (string s in ri.Notes)
                    {
                        sb.AppendLine("                 {0}", s);
                    }
                }

                prevDay = ri.startTime.day;
            }

            sb.AppendLine();
            sb.AppendNote();
            sb.AppendDocumentTail();

        }

        /// <summary>
        /// Retrieves XML document with all information calculated by engine
        /// </summary>
        /// <returns></returns>
        public XmlDocument GetXml()
        {
            XmlDocument doc = new XmlDocument();
            XmlElement root, x1, x2, x3;

            root = doc.CreateElement("RatedEvents");
            doc.AppendChild(root);

            x1 = doc.CreateElement("StartTime");
            x1.InnerText = DateStart.ToString();
            root.AppendChild(x1);

            x1 = doc.CreateElement("EndTime");
            x1.InnerText = DateEnd.ToString();
            root.AppendChild(x1);

            x1 = doc.CreateElement("Location");
            x1.InnerText = EarthLocation.GetFullName();
            root.AppendChild(x1);


            int prevDay = -1;

            foreach (GCRatedInterval ri in Intervals)
            {
                if (prevDay != ri.startTime.day)
                {
                    x1 = doc.CreateElement("Day");
                    x1.SetAttribute("Date", ri.startTime.ToString());
                    x1.SetAttribute("DayOfWeek", GCCalendar.GetWeekdayName(ri.startTime.dayOfWeek));
                    root.AppendChild(x1);
                    prevDay = ri.startTime.day;
                }


                x2 = doc.CreateElement("Interval");
                x2.SetAttribute("Start", ri.startTime.ShortTimeString());
                x2.SetAttribute("End", ri.endTime.ShortTimeString());
                x2.SetAttribute("Title", ri.Title);
                x2.SetAttribute("RatingPos", ri.ratingPos.ToString());
                x2.SetAttribute("RatingNeg", ri.ratingNeg.ToString());
                x1.AppendChild(x2);

                if (ri.Notes != null)
                {
                    foreach (string s in ri.Notes)
                    {
                        x3 = doc.CreateElement("Note");
                        x3.InnerText = s;
                        x2.AppendChild(x3);
                    }
                }

            }

            return doc;

        }

    
    }
}
