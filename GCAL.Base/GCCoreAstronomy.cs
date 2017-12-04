using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GCAL.Base
{
    public class GCCoreAstronomy
    {
        public enum AstronomySystem
        {
            Meeus,
            //SuryaSiddhanta
        }

        static GCCoreAstronomy()
        {
            System = AstronomySystem.Meeus;
        }

        public static AstronomySystem System { get; set; }

        public static double GetSunLongitude(GregorianDateTime vct, GCEarthData earth)
        {
            switch(System)
            {
                case AstronomySystem.Meeus:
                    return GCSunData.GetSunLongitude(vct);
            }

            return 0;
        }

        public static GCHourTime CalcSunrise(GregorianDateTime vct, GCEarthData earth)
        {
            return GCSunData.CalcSunrise(vct, earth);
        }

        public static GCHourTime CalcSunset(GregorianDateTime vct, GCEarthData earth)
        {
            return GCSunData.CalcSunset(vct, earth);
        }

        public static double GetMoonLongitude(GregorianDateTime vct, GCEarthData earth)
        {
            switch (System)
            {
                case AstronomySystem.Meeus:
                    GCMoonData moon = new GCMoonData();
                    moon.Calculate(vct.GetJulianComplete());
                    return moon.longitude_deg;
            }

            return 0;
        }

        public static double GetMoonElevation(GCEarthData e, GregorianDateTime vc)
        {
            GCMoonData moon = new GCMoonData();
            double d = vc.GetJulianComplete();
            moon.Calculate(d);
            moon.CorrectEqatorialWithParallax(d, e.latitudeDeg, e.longitudeDeg, 0);
            moon.calc_horizontal(d, e.longitudeDeg, e.latitudeDeg);

            return moon.elevation;
        }
    }
}
