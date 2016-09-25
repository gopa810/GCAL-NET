using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using GCAL.Base.Scripting;

namespace GCAL.Base
{
    public class CLocationRef: GSCore
    {
        public string timeZoneName;
        public string locationName = null;
        public double longitudeDeg;
        public double latitudeDeg;
        public double offsetUtcHours;
        public int timezoneId;
        // !!!!!!!!
        // when adding new properties, dont forget to modify EncodedString and DefaultEncodedStroing properties


        public CLocationRef()
        {
        }

        public CLocationRef(CLocationRef cl)
        {
            Set(cl);
        }

        public override bool Equals(object obj)
        {
            if (obj is CLocationRef)
            {
                CLocationRef arg = obj as CLocationRef;
                return arg.locationName.Equals(locationName) 
                    && arg.timeZoneName.Equals(timeZoneName)
                    && arg.GetLatitudeString().Equals(GetLatitudeString()) 
                    && arg.GetLongitudeString().Equals(GetLongitudeString());
            }
            else if (obj is string)
            {
                return (obj as string).Equals(GetFullName());
            }
            else
                return base.Equals(obj);
        }

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

        public override GSCore GetPropertyValue(string Token)
        {
            GSCore result = null;
            switch(Token)
            {
                case "latitude":
                    result = new GSNumber() { DoubleValue = latitudeDeg };
                    break;
                case "longitude":
                    result = new GSNumber() { DoubleValue = longitudeDeg };
                    break;
                case "latitudeString":
                    result = new GSString() { Value = GetLatitudeString() };
                    break;
                case "longitudeString":
                    result = new GSString() { Value = GetLongitudeString() };
                    break;
                case "name":
                    result = new GSString() { Value = locationName };
                    break;
                case "timezoneName":
                    result = new GSString() { Value = timeZoneName };
                    break;
                case "fullName":
                    result = new GSString(GetFullName());
                    break;
                default:
                    result = base.GetPropertyValue(Token);
                    break;
            }

            return result;
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
                format = format.Replace("{locationName}", locationName);
            if (format.IndexOf("{longitudeDeg}") >= 0)
                format = format.Replace("{longitudeDeg}", longitudeDeg.ToString());
            if (format.IndexOf("{latitudeDeg}") >= 0)
                format = format.Replace("{latitudeDeg}", latitudeDeg.ToString());
            if (format.IndexOf("{longitudeText}") >= 0)
                format = format.Replace("{longitudeText}", GetLongitudeString());
            if (format.IndexOf("{latitudeText}") >= 0)
                format = format.Replace("{latitudeText}", GetLatitudeString());


            if (format.IndexOf("{timeZoneName}") >= 0)
                format = format.Replace("{timeZoneName}", timeZoneName);

            if (args == null || args.Length == 0)
                return format.ToString();
            else
                return string.Format(format.ToString(), args);
        }

        public string EncodedString
        {
            get
            {
                if (locationName == null)
                    return null;
                return string.Format("{0}|{1}|{2}|{3}|{4}|{5}", GCFestivalBase.StringToSafe(locationName),
                    longitudeDeg, latitudeDeg, offsetUtcHours, timezoneId, timeZoneName);
            }
            set
            {
                if (value == null)
                    locationName = null;
                else
                {
                    string[] a = value.Split('|');
                    if (a.Length == 6)
                    {
                        double LO, LA, TZ;
                        int TZID;
                        if (double.TryParse(a[1], out LO)
                            && double.TryParse(a[2], out LA)
                            && double.TryParse(a[3], out TZ)
                            && int.TryParse(a[4], out TZID))
                        {
                            locationName = GCFestivalBase.SafeToString(a[0]);
                            longitudeDeg = LO;
                            latitudeDeg = LA;
                            offsetUtcHours = TZ;
                            timezoneId = TZID;
                            timeZoneName = a[5];
                        }
                    }
                }
            }
        }

        public static string DefaultEncodedString
        {
            get
            {
                return "Vrindavan, India|77.73|27.583|5.5|188|Asia/Calcutta";
            }
        }
    }
}
