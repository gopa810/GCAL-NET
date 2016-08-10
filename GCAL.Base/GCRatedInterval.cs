using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GCAL.Base
{
    public class GCRatedInterval
    {
        public GregorianDateTime startTime;
        public GregorianDateTime endTime;
        public double ratingPos = 0.0;
        public double ratingNeg = 0.0;
        public string Title;
        public string[] Notes;

        public double Length
        {
            get
            {
                if (startTime == null || endTime == null)
                    return -1.0;
                return endTime.GetJulianComplete() - startTime.GetJulianComplete();
            }
        }
    }
}
