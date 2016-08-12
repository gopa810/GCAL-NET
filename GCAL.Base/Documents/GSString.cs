using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GCAL.Base.Documents
{
    public class GSString: GSCore
    {
        public string Value = String.Empty;

        public override string getStringValue()
        {
            return Value;
        }

        public override long getIntegerValue()
        {
            long l = 0;
            if (long.TryParse(Value, out l))
                return l;
            return 0;
        }

        public override bool getBooleanValue()
        {
            return getIntegerValue() != 0;
        }

        public override double getDoubleValue()
        {
            double d = 0.0;
            if (double.TryParse(Value, out d))
                return d;
            return 0.0;
        }

        public override GSCore GetTokenValue(string s)
        {
            if (s.Equals("length"))
            {
                return new GSNumber() { IntegerValue = Value.Length };
            }
            if (s.Equals("lower"))
            {
                GSString str = new GSString();
                str.Value = Value.ToLower();
                return str;
            }
            if (s.Equals("upper"))
            {
                GSString str = new GSString();
                str.Value = Value.ToUpper();
                return str;
            }
            return base.GetTokenValue(s);
        }

        public override string ToString()
        {
            return Value;
        }
    }
}
