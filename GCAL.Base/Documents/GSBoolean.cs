using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GCAL.Base.Documents
{
    public class GSBoolean: GSCore
    {
        public bool Value = false;

        public override string getStringValue()
        {
            return Value ? "1" : "0";
        }

        public override long getIntegerValue()
        {
            return Value ? 1 : 0;
        }

        public override bool getBooleanValue()
        {
            return Value;
        }

        public override double getDoubleValue()
        {
            return Value ? 1.0 : 0.0;
        }

    }
}
