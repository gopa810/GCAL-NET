using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GCAL.Base
{
    public class CLocation
    {
        public string countryName;
        public string cityName;
        public double longitudeDeg;
        public double latitudeDeg;
        public double offsetUtcHours;
        public int timezoneId;

        public CLocation()
        {
            latitudeDeg = 0.0;
            longitudeDeg = 0.0;
            offsetUtcHours = 0.0;
            timezoneId = 0;
        }

        public CLocationRef GetLocationRef()
        {
            CLocationRef lc = new CLocationRef();
            lc.locationName = string.Format("{0} ({1})", cityName, countryName);
            lc.latitudeDeg = latitudeDeg;
            lc.longitudeDeg = longitudeDeg;
            lc.offsetUtcHours = offsetUtcHours;
            lc.timezoneId = timezoneId;
            lc.timeZoneName = TTimeZone.GetTimeZoneName(timezoneId);
            return lc;
        }

        public void Empty()
        {
            countryName = String.Empty;
            cityName = String.Empty;
            latitudeDeg = 0.0;
            longitudeDeg = 0.0;
            offsetUtcHours = 0.0;
            timezoneId = 0;
        }

    }
}
