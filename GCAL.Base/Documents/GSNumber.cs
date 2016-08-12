using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GCAL.Base.Documents
{
    public class GSNumber: GSCore
    {
        public enum NumberType
        {
            Int,
            Double
        }

        private double dValue = 0.0;
        private long lValue = 0L;
        private NumberType nType = NumberType.Int;

        public override string ToString()
        {
            return lValue.ToString() + " " + dValue.ToString();
        }

        public long IntegerValue
        {
            get
            {
                return lValue;
            }
            set
            {
                lValue = value;
                nType = NumberType.Int;
                dValue = Convert.ToDouble(value);
            }
        }

        public double DoubleValue
        {
            get
            {
                return dValue;
            }
            set
            {
                dValue = value;
                nType = NumberType.Double;
                lValue = Convert.ToInt64(value);
            }
        }

        public override string getStringValue()
        {
            if (nType == NumberType.Int)
                return lValue.ToString();
            else
                return dValue.ToString();
        }

        public override long getIntegerValue()
        {
            return lValue;
        }

        public override bool getBooleanValue()
        {
            return lValue != 0;
        }

        public override double getDoubleValue()
        {
            return dValue;
        }

        public bool IsInteger { get { return nType == NumberType.Int; } }
        public bool IsDouble { get { return nType == NumberType.Double; } }
    }
}
