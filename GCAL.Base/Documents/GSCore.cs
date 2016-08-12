using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GCAL.Base.Documents
{
    public class GSCore
    {
        protected GSExecutor Executor = null;

        public virtual GSCore Execute()
        {
            return this;
        }

        public virtual void setExecutor(GSExecutor exec)
        {
            Executor = exec;
        }

        #region Methods - Should be overriden for correct results

        public virtual string getStringValue()
        {
            return String.Empty;
        }

        public virtual long getIntegerValue()
        {
            return 0L;
        }

        public virtual double getDoubleValue()
        {
            return 0.0;
        }

        public virtual bool getBooleanValue()
        {
            return false;
        }

        /// <summary>
        /// This should return value for single token name
        /// Token name does not contain dot separator
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public virtual GSCore GetTokenValue(string s)
        {
            return new GSString();
        }

        /// <summary>
        /// Token can contain dot separator
        /// We should not normally override this method
        /// Rather method GetTokenValue shoudl be overriden
        /// </summary>
        /// <param name="Token"></param>
        /// <returns></returns>
        public virtual GSCore EvaluateToken(string Token)
        {
            int dotPos = Token.IndexOf('.');
            if (dotPos >= 0)
            {
                string str = Token.Substring(0, dotPos);
                GSCore obj = GetTokenValue(str);
                if (obj == null)
                    return new GSString();
                return obj.EvaluateToken(Token.Substring(dotPos + 1));
            }
            else
            {
                return GetTokenValue(Token);
            }
        }

        #endregion
    }

    public class GSCoreCollection : List<GSCore>
    {
        public long[] getIntegerArray()
        {
            long[] result = new long[Count];
            for (int i = 0; i < Count; i++)
            {
                result[i] = this[i].getIntegerValue();
            }
            return result;
        }

        public double[] getDoubleArray()
        {
            double[] result = new double[Count];
            for (int i = 0; i < Count; i++)
            {
                result[i] = this[i].getDoubleValue();
            }
            return result;
        }
        public string[] getStringArray()
        {
            string[] result = new string[Count];
            for (int i = 0; i < Count; i++)
            {
                result[i] = this[i].getStringValue();
            }
            return result;
        }

        public bool[] getBooleanArray()
        {
            bool[] result = new bool[Count];
            for (int i = 0; i < Count; i++)
            {
                result[i] = this[i].getBooleanValue();
            }
            return result;
        }

        public bool IsFirstToken()
        {
            if (Count > 0 && this[0] is GSToken)
                return true;
            return false;
        }

        public string getFirstToken()
        {
            if (Count > 0)
            {
                if (this[0] is GSToken)
                    return (this[0] as GSToken).Token;
                else
                    return this[0].getStringValue();
            }
            return String.Empty;
        }

        public GSCoreDataType getArithmeticDataType()
        {
            int strings = 0;
            int numbers = 0;
            int doubles = 0;
            int bools = 0;

            foreach (GSCore item in this)
            {
                if (item is GSNumber)
                {
                    if ((item as GSNumber).IsInteger)
                        numbers++;
                    else 
                        doubles++;
                }
                else if (item is GSBoolean)
                    bools++;
                else if (item is GSString)
                    strings++;
            }

            if (strings > 0)
                return GSCoreDataType.String;
            if (doubles > 0)
                return GSCoreDataType.Double;
            if (numbers > 0)
                return GSCoreDataType.Integer;
            if (bools > 0)
                return GSCoreDataType.Boolean;

            return GSCoreDataType.Void;
        }

        public GSCoreCollection getSublist(int fromIndex)
        {
            GSCoreCollection args = new GSCoreCollection();
            for (int n = 1; n < Count; n++)
                args.Add(this[n]);
            return args;
        }
    }

    public enum GSCoreDataType
    {
        String,
        Integer,
        Double,
        Boolean,
        Void
    }

}
