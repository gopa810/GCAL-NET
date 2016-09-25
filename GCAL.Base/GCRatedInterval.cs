using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using GCAL.Base.Scripting;

namespace GCAL.Base
{
    public class GCRatedInterval: GSCore
    {
        public GregorianDateTime startTime;
        public GregorianDateTime endTime;
        public double ratingPos = 0.0;
        public double ratingNeg = 0.0;
        public string Title;
        public string[] Notes;

        public override string ToString()
        {
            return string.Format("{0} {1} - {2} {3} :: {4} [{5}]", startTime.ToString(), startTime.LongTimeString(), endTime.ToString(), 
                endTime.LongTimeString(), Title, ratingPos + ratingNeg);
        }

        public double Length
        {
            get
            {
                if (startTime == null || endTime == null)
                    return -1.0;
                return endTime.GetJulianComplete() - startTime.GetJulianComplete();
            }
        }

        public override GSCore GetPropertyValue(string s)
        {
            switch (s)
            {
                case "startTime":
                    return startTime;
                case "endTime":
                    return endTime;
                case "ratingPos":
                    return new GSNumber(ratingPos);
                case "ratingNeg":
                    return new GSNumber(ratingNeg);
                case "ratingTotal":
                    return new GSNumber(ratingPos + ratingNeg);
                case "length":
                    return new GSNumber(Length);
                case "title":
                    return new GSString(Title);
                case "notes":
                    {
                        GSList list = new GSList();
                        foreach(string str in Notes)
                            list.Parts.Add(new GSString(str));
                        return list;
                    }
                default:
                    return base.GetPropertyValue(s);
            }
        }
    }
}
