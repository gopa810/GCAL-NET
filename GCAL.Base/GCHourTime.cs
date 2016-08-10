using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GCAL.Base
{
    public struct GCHourTime
    {
        public int hour;
        public int min;
        public int sec;
        public int mili;


        public bool IsGreaterThan(GCHourTime dt)
        {
            if (hour > dt.hour)
                return true;
            else if (hour < dt.hour)
                return false;

            if (min > dt.min)
                return true;
            else if (min < dt.min)
                return false;

            if (sec > dt.sec)
                return true;
            else if (sec < dt.sec)
                return false;

            if (mili > dt.mili)
                return true;

            return false;
        }


        public bool IsLessThan(GCHourTime dt)
        {
            if (hour < dt.hour)
                return true;
            else if (hour > dt.hour)
                return false;

            if (min < dt.min)
                return true;
            else if (min > dt.min)
                return false;

            if (sec < dt.sec)
                return true;
            else if (sec > dt.sec)
                return false;

            if (mili < dt.mili)
                return true;

            return false;
        }

        public bool IsGreaterOrEqualThan(GCHourTime dt)
        {
            if (hour >= dt.hour)
                return true;
            else if (hour < dt.hour)
                return false;

            if (min >= dt.min)
                return true;
            else if (min < dt.min)
                return false;

            if (sec >= dt.sec)
                return true;
            else if (sec < dt.sec)
                return false;

            if (mili >= dt.mili)
                return true;

            return false;
        }

        public bool IsLessOrEqualThan(GCHourTime dt)
        {
            if (hour <= dt.hour)
                return true;
            else if (hour > dt.hour)
                return false;

            if (min <= dt.min)
                return true;
            else if (min > dt.min)
                return false;

            if (sec <= dt.sec)
                return true;
            else if (sec > dt.sec)
                return false;

            if (mili <= dt.mili)
                return true;

            return false;
        }


        public void AddMinutes(int mn)
        {
            min += mn;
            while (min < 0) { min += 60; hour--; }
            while (min > 59) { min -= 60; hour++; }
        }

        public double GetDayTime()
        {
            return ((Convert.ToDouble(hour) * 60.0 + Convert.ToDouble(min)) * 60.0 + Convert.ToDouble(sec)) / 86400.0;
        }



        public void SetValue(int i)
        {
            hour = min = sec = mili = i;
        }

        public void Set(GCHourTime d)
        {
            hour = d.hour;
            min = d.min;
            sec = d.sec;
            mili = d.mili;
        }

        public void SetDayTime(double d)
        {
            double time_hr = 0.0;

            // hour
            time_hr = d * 24;
            hour = Convert.ToInt32(Math.Floor(time_hr));

            // minute
            time_hr -= hour;
            time_hr *= 60;
            min = Convert.ToInt32(Math.Floor(time_hr));

            // second
            time_hr -= min;
            time_hr *= 60;
            sec = Convert.ToInt32(Math.Floor(time_hr));

            // miliseconds
            time_hr -= sec;
            time_hr *= 1000;
            mili = Convert.ToInt32(Math.Floor(time_hr));
        }

        ////////////////////////////////////////////////////////////////
        //
        //  Conversion time from DEGREE fromat to H:M:S:MS format
        //
        //  time - output
        //  time_deg - input time in range 0 deg to 360 deg
        //             where 0 deg = 0:00 AM and 360 deg = 12:00 PM
        //
        public void SetDegTime(double time_deg)
        {
            double time_hr = 0.0;

            time_deg = GCMath.putIn360(time_deg);

            // hour
            time_hr = time_deg / 360 * 24;
            hour = Convert.ToInt32(Math.Floor(time_hr));

            // minute
            time_hr -= hour;
            time_hr *= 60;
            min = Convert.ToInt32(Math.Floor(time_hr));

            // second
            time_hr -= min;
            time_hr *= 60;
            sec = Convert.ToInt32(Math.Floor(time_hr));

            // miliseconds
            time_hr -= sec;
            time_hr *= 1000;
            mili = Convert.ToInt32(Math.Floor(time_hr));
        }

        public string ToLongTimeString()
        {
            return string.Format("{0:00}:{1:00}:{2:00}", hour, min, sec);
        }

        public string ToShortTimeString()
        {
            return string.Format("{0:00}:{1:00}", hour, min);
        }

    }

}
