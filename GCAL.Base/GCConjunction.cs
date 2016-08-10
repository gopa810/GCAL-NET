using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GCAL.Base
{
    public class GCConjunction
    {

        /*********************************************************************/
        /*                                                                   */
        /*  m1 - previous moon position                                      */
        /*  m2 - next moon position                                          */
        /*  s1 - previous sun position                                       */
        /*  s2 - next sun position                                           */
        /*                                                                   */
        /*  Test for conjunction of the sun and moon                         */
        /*  m1,s1 is in one time moment                                      */
        /*  m2,s2 is in second time moment                                   */
        /*                                                                   */
        /*  this function tests whether conjunction occurs between           */
        /*  these two moments                                                */
        /*                                                                   */
        /*********************************************************************/

        public static bool IsConjunction(double m1, double s1, double s2, double m2)
        {
            if (m2 < m1)
                m2 += 360.0;
            if (s2 < s1)
                s2 += 360.0;
            if ((m1 <= s1) && (s1 < s2) && (s2 <= m2))
                return true;

            m1 = GCMath.putIn180(m1);
            m2 = GCMath.putIn180(m2);
            s1 = GCMath.putIn180(s1);
            s2 = GCMath.putIn180(s2);

            if ((m1 <= s1) && (s1 < s2) && (s2 <= m2))
                return true;

            return false;
        }

        ///////////////////////////////////////////////////////////////////////
        // GET PREVIOUS CONJUNCTION OF THE SUN AND MOON
        //
        // THIS IS HELP FUNCTION FOR GetPrevConjunction(VCTIME test_date, 
        //                                         VCTIME &found, bool this_day, EARTHDATA earth)
        //
        // looking for previous sun-moon conjunction
        // it starts from input day from 12:00 AM (noon) UTC
        // so output can be the same day
        // if output is the same day, then conjunction occurs between 00:00 AM and noon of this day
        //
        // date - input / output
        // earth - input
        // return value - sun longitude in degs
        //
        // error is when return value is greater than 999.0 deg

        public static double GetPrevConjunction(ref GregorianDateTime date, GCEarthData earth)
        {
            int bCont = 32;
            double prevSun = 0.0, prevMoon = 0.0, prevDiff = 0.0;
            double nowSun = 0.0, nowMoon = 0.0, nowDiff = 0.0;
            //	SUNDATA sun;
            GCMoonData moon = new GCMoonData();
            double jd;
            GregorianDateTime d = new GregorianDateTime();

            d.Set(date);
            d.shour = 0.5;
            d.TimezoneHours = 0.0;
            jd = d.GetJulian();//GetJulianDay(d.year, d.month, d.day);

            // set initial data for input day
            // NOTE: for grenwich
            //SunPosition(d, earth, sun);
            moon.Calculate(jd, earth);
            prevSun = GCSunData.GetSunLongitude(d);
            prevMoon = moon.longitude_deg;
            prevDiff = GCMath.putIn180(prevSun - prevMoon);

            do
            {
                // shift to day before
                d.PreviousDay();
                jd -= 1.0;
                // calculation
                //SunPosition(d, earth, sun);
                moon.Calculate(jd, earth);
                nowSun = GCSunData.GetSunLongitude(d);
                nowMoon = moon.longitude_deg;
                nowDiff = GCMath.putIn180(nowSun - nowMoon);
                // if difference of previous has another sign than now calculated
                // then it is the case that moon was faster than sun and he 
                //printf("        prevsun=%f\nprevmoon=%f\nnowsun=%f\nnowmoon=%f\n", prevSun, prevMoon, nowSun, nowMoon);


                if (IsConjunction(nowMoon, nowSun, prevSun, prevMoon))
                {
                    // now it calculates actual time and zodiac of conjunction
                    double x;
                    if (prevDiff == nowDiff)
                        return 0;
                    x = Math.Abs(nowDiff) / Math.Abs(prevDiff - nowDiff);
                    if (x < 0.5)
                    {
                        d.shour = x + 0.5;
                    }
                    else
                    {
                        d.NextDay();
                        d.shour = x - 0.5;
                    }
                    date.Set(d);
                    double other = GCSunData.GetSunLongitude(d);
                    //			double other2 = nowSun + (prevSun - nowSun)*x;
                    GCMath.putIn360(prevSun);
                    GCMath.putIn360(nowSun);
                    if (Math.Abs(prevSun - nowSun) > 10.0)
                    {
                        return GCMath.putIn180(nowSun) + (GCMath.putIn180(prevSun) - GCMath.putIn180(nowSun)) * x;
                    }
                    else
                        return nowSun + (prevSun - nowSun) * x;
                    //			return other2;
                }
                prevSun = nowSun;
                prevMoon = nowMoon;
                prevDiff = nowDiff;
                bCont--;
            }
            while (bCont >= 0);

            return 1000.0;
        }

        ///////////////////////////////////////////////////////////////////////
        // GET NEXT CONJUNCTION OF THE SUN AND MOON
        //
        // Help function for GetNextConjunction(VCTIME test_date, VCTIME &found, 
        //                                      bool this_day, EARTHDATA earth)
        //
        // looking for next sun-moon conjunction
        // it starts from input day from 12:00 AM (noon) UTC
        // so output can be the same day
        // if output is the same day, then conjunction occurs 
        // between noon and midnight of this day
        //
        // date - input / output
        // earth - input
        // return value - sun longitude
        //
        // error is when return value is greater than 999.0 deg

        public static double GetNextConjunction(ref GregorianDateTime date, GCEarthData earth)
        {
            int bCont = 32;
            double prevSun = 0.0, prevMoon = 0.0, prevDiff = 0.0;
            double nowSun = 0.0, nowMoon = 0.0, nowDiff = 0.0;
            //SUNDATA sun;
            GCMoonData moon = new GCMoonData();
            double jd;
            GregorianDateTime d = new GregorianDateTime();

            d.Set(date);
            d.shour = 0.5;
            d.TimezoneHours = 0.0;
            jd = d.GetJulian();

            // set initial data for input day
            // NOTE: for grenwich
            //SunPosition(d, earth, sun, 0.0);
            moon.Calculate(jd, earth);
            nowSun = GCMath.putIn360(GCSunData.GetSunLongitude(d));
            nowMoon = GCMath.putIn360(moon.longitude_deg);
            nowDiff = GCMath.putIn180(nowSun - nowMoon);

            do
            {
                // shift to day before
                d.NextDay();
                jd += 1.0;
                // calculation
                //SunPosition(d, earth, sun, 0.0);
                moon.Calculate(jd, earth);
                prevSun = GCSunData.GetSunLongitude(d);
                prevMoon = moon.longitude_deg;
                prevDiff = GCMath.putIn180(prevSun - prevMoon);
                // if difference of previous has another sign than now calculated
                // then it is the case that moon was faster than sun and he 
                //printf("        prevsun=%f\nprevmoon=%f\nnowsun=%f\nnowmoon=%f\n", prevSun, prevMoon, nowSun, nowMoon);


                if (IsConjunction(nowMoon, nowSun, prevSun, prevMoon))
                {
                    // now it calculates actual time and zodiac of conjunction
                    double x;
                    if (prevDiff == nowDiff)
                        return 0;
                    x = Math.Abs(nowDiff) / Math.Abs(prevDiff - nowDiff);
                    if (x < 0.5)
                    {
                        d.PreviousDay();
                        d.shour = x + 0.5;
                    }
                    else
                    {
                        d.shour = x - 0.5;
                    }
                    date.Set(d);
                    GCMath.putIn360(prevSun);
                    GCMath.putIn360(nowSun);
                    if (Math.Abs(prevSun - nowSun) > 10.0)
                    {
                        return GCMath.putIn180(nowSun) + (GCMath.putIn180(prevSun) - GCMath.putIn180(nowSun)) * x;
                    }
                    else
                        return nowSun + (prevSun - nowSun) * x;
                }
                nowSun = prevSun;
                nowMoon = prevMoon;
                nowDiff = prevDiff;
                bCont--;
            }
            while (bCont >= 0);

            return 1000.0;
        }

        /*********************************************************************/
        /*                                                                   */
        /*                                                                   */
        /*                                                                   */
        /*                                                                   */
        /*                                                                   */
        /*********************************************************************/

        public static double GetPrevConjunction(GregorianDateTime test_date, out GregorianDateTime found, bool this_conj, GCEarthData earth)
        {
            double phi = 12.0;
            double l1, l2, sunl;

            if (this_conj)
            {
                test_date.shour -= 0.2;
                test_date.NormalizeValues();
            }


            double jday = test_date.GetJulianComplete();
            double xj;
            GCMoonData moon = new GCMoonData();
            GregorianDateTime d = new GregorianDateTime();
            d.Set(test_date);
            GregorianDateTime xd = new GregorianDateTime();
            double scan_step = 1.0;
            int prev_tit = 0;
            int new_tit = -1;

            moon.Calculate(jday, earth);
            sunl = GCSunData.GetSunLongitude(d);
            l1 = GCMath.putIn180(moon.longitude_deg - sunl);
            prev_tit = GCMath.IntFloor(l1 / phi);

            int counter = 0;
            while (counter < 20)
            {
                xj = jday;
                xd.Set(d);

                jday -= scan_step;
                d.shour -= scan_step;
                if (d.shour < 0.0)
                {
                    d.shour += 1.0;
                    d.PreviousDay();
                }

                moon.Calculate(jday, earth);
                sunl = GCSunData.GetSunLongitude(d);
                l2 = GCMath.putIn180(moon.longitude_deg - sunl);
                new_tit = GCMath.IntFloor(l2 / phi);

                if (prev_tit >= 0 && new_tit < 0)
                {
                    jday = xj;
                    d.Set(xd);
                    scan_step *= 0.5;
                    counter++;
                    continue;
                }
                else
                {
                    l1 = l2;
                    prev_tit = new_tit;
                }

            }
            found = new GregorianDateTime();
            found.Set(d);
            return sunl;
        }

        /*********************************************************************/
        /*                                                                   */
        /*                                                                   */
        /*                                                                   */
        /*                                                                   */
        /*                                                                   */
        /*********************************************************************/

        public static double GetNextConjunction(GregorianDateTime test_date, out GregorianDateTime found, bool this_conj, GCEarthData earth)
        {
            double phi = 12.0;
            double l1, l2, sunl;

            if (this_conj)
            {
                test_date.shour += 0.2;
                test_date.NormalizeValues();
            }


            double jday = test_date.GetJulianComplete();
            double xj;
            GCMoonData moon = new GCMoonData();
            GregorianDateTime d = new GregorianDateTime();
            d.Set(test_date);
            GregorianDateTime xd = new GregorianDateTime();
            double scan_step = 1.0;
            int prev_tit = 0;
            int new_tit = -1;

            moon.Calculate(jday, earth);
            sunl = GCSunData.GetSunLongitude(d);
            l1 = GCMath.putIn180(moon.longitude_deg - sunl);
            prev_tit = GCMath.IntFloor(l1 / phi);

            int counter = 0;
            while (counter < 20)
            {
                xj = jday;
                xd.Set(d);

                jday += scan_step;
                d.shour += scan_step;
                if (d.shour > 1.0)
                {
                    d.shour -= 1.0;
                    d.NextDay();
                }

                moon.Calculate(jday, earth);
                sunl = GCSunData.GetSunLongitude(d);
                l2 = GCMath.putIn180(moon.longitude_deg - sunl);
                new_tit = GCMath.IntFloor(l2 / phi);

                if (prev_tit < 0 && new_tit >= 0)
                {
                    jday = xj;
                    d.Set(xd);
                    scan_step *= 0.5;
                    counter++;
                    continue;
                }
                else
                {
                    l1 = l2;
                }

                prev_tit = new_tit;
            }
            found = new GregorianDateTime();
            found.Set(d);
            return sunl;
        }

    }
}
