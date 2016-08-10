using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GCAL.Base
{
    public class CLocationRef
    {
        public string timeZoneName;
        public string locationName;
        public double longitudeDeg;
        public double latitudeDeg;
        public double offsetUtcHours;
        public int timezoneId;

        public GCEarthData EARTHDATA()
        {
            GCEarthData e = new GCEarthData();
            e.timezoneId = timezoneId;
            e.latitudeDeg = latitudeDeg;
            e.longitudeDeg = longitudeDeg;
            e.offsetUtcHours = offsetUtcHours;
            return e;
        }

        public GCEarthData SetEARTHDATA(GCEarthData e)
        {
            longitudeDeg = e.longitudeDeg;
            latitudeDeg = e.latitudeDeg;
            offsetUtcHours = e.offsetUtcHours;
            timezoneId = e.timezoneId;
            return e;
        }

        public void Set(CLocationRef L)
        {
            timeZoneName = L.timeZoneName;
            locationName = L.locationName;
            longitudeDeg = L.longitudeDeg;
            latitudeDeg = L.latitudeDeg;
            offsetUtcHours = L.offsetUtcHours;
            timezoneId = L.timezoneId;
        }

        public string GetLatitudeString()
        {
            return GCEarthData.GetTextLatitude(latitudeDeg);
        }

        public string GetLongitudeString()
        {
            return GCEarthData.GetTextLongitude(longitudeDeg);
        }

        public string GetFullName()
        {
            return string.Format("{0} ({1}, {2}, {3}: {4})"
                    , locationName
                    , GCEarthData.GetTextLatitude(latitudeDeg)
                    , GCEarthData.GetTextLongitude(longitudeDeg)
                    , GCStrings.Localized("Timezone")
                    , timeZoneName);
        }


        public string GetNameAsFileName()
        {
            StringBuilder sb = new StringBuilder();
            int m = 0;
            foreach (char c in locationName)
            {
                if (Char.IsLetter(c))
                {
                    if (m == 0)
                        sb.Append(Char.ToUpper(c));
                    else
                        sb.Append(Char.ToLower(c));
                    m = 1;
                }
                else
                {
                    m = 0;
                }
            }
            return sb.ToString();
        }

        public string Format(string format, params string[] args)
        {
            StringBuilder sb = new StringBuilder(format);

            if (format.IndexOf("{locationName}") >= 0)
                format.Replace("{locationName}", locationName);
            if (format.IndexOf("{longitudeDeg}") >= 0)
                format.Replace("{longitudeDeg}", longitudeDeg.ToString());
            if (format.IndexOf("{latitudeDeg}") >= 0)
                format.Replace("{latitudeDeg}", latitudeDeg.ToString());
            if (format.IndexOf("{longitudeText}") >= 0)
                format.Replace("{longitudeText}", GetLongitudeString());
            if (format.IndexOf("{latitudeText}") >= 0)
                format.Replace("{latitudeText}", GetLatitudeString());


            if (format.IndexOf("{timeZoneName}") >= 0)
                format.Replace("{timeZoneName}", timeZoneName);

            if (args == null || args.Length == 0)
                return format.ToString();
            else
                return string.Format(format.ToString(), args);
        }
    }
}
