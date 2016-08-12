using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GCAL.Base.Documents
{
    public class GSToken: GSCore
    {
        public string Token = String.Empty;


        public override GSCore Execute()
        {
            return Executor.EvaluateToken(Token);
        }

        public override string getStringValue()
        {
            GSCore tokenValue = Executor.EvaluateToken(Token);
            return tokenValue.getStringValue();
        }

        public override string ToString()
        {
            return Token;
        }
    }
}
