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


        public static string ParseDegreesFromString(string str, bool bNorth, ref double Longitude)
        {
            double l = 0.0;
            double sig = 1.0;
            double coma = 10.0;
            bool after_coma = false;
            bool is_deg = false;
            string pret = bNorth ?
                "Incorrect format for longitude. Correct examples: 35E05, 23W45"
                : "Incorrect format for latitude. Correct examples: 45N05, 13S45";
            bool was_comma = false;
            bool was_letter = false;
            bool was_digit = false;
            bool was_sign = false;
            int numsAfterComma = 0;

            foreach (char s in str)
            {
                switch (s)
                {
                    case '0':
                    case '1':
                    case '2':
                    case '3':
                    case '4':
                    case '5':
                    case '6':
                    case '7':
                    case '8':
                    case '9':
                        was_digit = true;
                        if (after_coma)
                        {
                            if (is_deg)
                            {
                                numsAfterComma++;
                                if (numsAfterComma > 2)
                                {
                                    return "Minutes are allowed only from range 0 - 59";
                                }
                                l += (s - '0') * 5.0 / (coma * 3.0);
                            }
                            else
                            {
                                l += (s - '0') / coma;
                            }
                            coma *= 10.0;
                        }
                        else
                        {
                            l = l * 10.0 + (s - '0');
                            if (!bNorth)
                            {
                                if (l > 90.0)
                                    return "Latitude is allowed only in range 0 - 90";
                            }
                            else
                            {
                                if (l > 180.0)
                                    return "Longitude is allowed only in range 0 - 180";
                            }
                        }
                        break;
                    case 'n':
                    case 'N':
                        if (was_letter || was_sign || was_comma)
                            return pret;

                        was_letter = true;
                        if (!bNorth)
                        {
                            after_coma = true;
                            is_deg = true;
                            sig = 1.0;
                        }
                        else
                        {
                            return pret;
                        }
                        break;
                    case 's':
                    case 'S':
                        if (was_letter || was_sign || was_comma)
                            return pret;
                        was_letter = true;
                        if (!bNorth)
                        {
                            after_coma = true;
                            is_deg = true;
                            sig = -1.0;
                        }
                        else
                        {
                            return pret;
                        }
                        break;
                    case 'e':
                    case 'E':
                        if (was_letter || was_sign || was_comma)
                            return pret;
                        was_letter = true;
                        if (bNorth)
                        {
                            after_coma = true;
                            is_deg = true;
                            sig = 1.0;
                        }
                        else
                        {
                            return pret;
                        }
                        break;
                    case 'w':
                    case 'W':
                        if (was_letter || was_sign || was_comma)
                            return pret;
                        was_letter = true;
                        if (bNorth)
                        {
                            after_coma = true;
                            is_deg = true;
                            sig = -1.0;
                        }
                        else
                        {
                            return pret;
                        }
                        break;
                    case '-':
                        if (was_letter || was_digit || was_comma)
                            return pret;
                        was_sign = true;
                        sig = -1.0;
                        break;
                    case '+':
                        if (was_letter || was_digit || was_comma)
                            return pret;
                        was_sign = true;
                        sig = 1.0;
                        break;
                    case '.':
                    case ',':
                        if (was_letter || was_comma)
                            return pret;
                        was_comma = true;
                        after_coma = true;
                        is_deg = false;
                        break;
                    default:
                        return pret;
                }
            }

            Longitude = l * sig;

            return null;
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
