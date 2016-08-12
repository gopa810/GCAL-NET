using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GCAL.Base.Documents
{
    public class GSExecutor
    {
        private List<Dictionary<string, GSCore>> stackVars = new List<Dictionary<string, GSCore>>();


        public GSExecutor()
        {
            stackVars.Add(new Dictionary<string, GSCore>());
        }

        public GSCore ExecuteToken(string token, GSCoreCollection args)
        {
            GSCore result = null;
            if (token.Equals("add") || token.Equals("+"))
                result = execAdd(evaluateAllArgs(args));
            else if (token.Equals("sub") || token.Equals("-"))
                result = execSub(evaluateAllArgs(args));
            else if (token.Equals("mul") || token.Equals("*"))
                result = execMul(evaluateAllArgs(args));
            else if (token.Equals("div") || token.Equals("/"))
                result = execDiv(evaluateAllArgs(args));
            else if (token.Equals("and") || token.Equals("&"))
                result = execAnd(args);
            else if (token.Equals("or") || token.Equals("|"))
                result = execOr(args);
            else if (token.Equals("not") || token.Equals("!"))
                result = execNot(args);
            else if (token.Equals("set") && args.Count > 1)
                result = execSet(args[0], args[1]);
            else if ((token.Equals("gt") || token.Equals(">")) && args.Count > 1)
                result = execGt(args);
            else if ((token.Equals("ge") || token.Equals(">=")) && args.Count > 1)
                result = execGe(args);
            else if ((token.Equals("eq") || token.Equals("==")) && args.Count > 1)
                result = execEq(args);
            else if ((token.Equals("ne") || token.Equals("!=")) && args.Count > 1)
                result = execNe(args);
            else if ((token.Equals("le") || token.Equals("<=")) && args.Count > 1)
                result = execLe(args);
            else if ((token.Equals("lt") || token.Equals("<")) && args.Count > 1)
                result = execLt(args);

            return result;
        }

        public GSCoreCollection evaluateAllArgs(GSCoreCollection args)
        {
            GSCoreCollection coll = new GSCoreCollection();
            foreach (GSCore item in args)
            {
                coll.Add(item.Execute());
            }
            return coll;
        }

        private Dictionary<string, GSCore> getLastVars()
        {
            return stackVars[stackVars.Count - 1];
        }

        private GSCore execGt(GSCoreCollection arg1)
        {
            GSBoolean bv = new GSBoolean();
            GSCoreDataType dt = arg1.getArithmeticDataType();

            if (dt == GSCoreDataType.Double)
            {
                bv.Value = (arg1[0].getDoubleValue() > arg1[1].getDoubleValue());
            }
            else if (dt == GSCoreDataType.Integer)
            {
                bv.Value = (arg1[0].getIntegerValue() > arg1[1].getIntegerValue());
            }
            else if (dt == GSCoreDataType.String)
            {
                bv.Value = (arg1[0].getStringValue().CompareTo(arg1[1].getStringValue()) > 0);
            }

            return bv;
        }

        private GSCore execGe(GSCoreCollection arg1)
        {
            GSBoolean bv = new GSBoolean();
            GSCoreDataType dt = arg1.getArithmeticDataType();

            if (dt == GSCoreDataType.Double)
            {
                bv.Value = (arg1[0].getDoubleValue() >= arg1[1].getDoubleValue());
            }
            else if (dt == GSCoreDataType.Integer)
            {
                bv.Value = (arg1[0].getIntegerValue() >= arg1[1].getIntegerValue());
            }
            else if (dt == GSCoreDataType.String)
            {
                bv.Value = (arg1[0].getStringValue().CompareTo(arg1[1].getStringValue()) >= 0);
            }

            return bv;
        }

        private GSCore execEq(GSCoreCollection arg1)
        {
            GSBoolean bv = new GSBoolean();
            GSCoreDataType dt = arg1.getArithmeticDataType();

            if (dt == GSCoreDataType.Double)
            {
                bv.Value = (arg1[0].getDoubleValue() == arg1[1].getDoubleValue());
            }
            else if (dt == GSCoreDataType.Integer)
            {
                bv.Value = (arg1[0].getIntegerValue() == arg1[1].getIntegerValue());
            }
            else if (dt == GSCoreDataType.String)
            {
                bv.Value = (arg1[0].getStringValue().CompareTo(arg1[1].getStringValue()) == 0);
            }

            return bv;
        }

        private GSCore execNe(GSCoreCollection arg1)
        {
            GSBoolean bv = new GSBoolean();
            GSCoreDataType dt = arg1.getArithmeticDataType();

            if (dt == GSCoreDataType.Double)
            {
                bv.Value = (arg1[0].getDoubleValue() != arg1[1].getDoubleValue());
            }
            else if (dt == GSCoreDataType.Integer)
            {
                bv.Value = (arg1[0].getIntegerValue() != arg1[1].getIntegerValue());
            }
            else if (dt == GSCoreDataType.String)
            {
                bv.Value = (arg1[0].getStringValue().CompareTo(arg1[1].getStringValue()) != 0);
            }

            return bv;
        }

        private GSCore execLe(GSCoreCollection arg1)
        {
            GSBoolean bv = new GSBoolean();
            GSCoreDataType dt = arg1.getArithmeticDataType();

            if (dt == GSCoreDataType.Double)
            {
                bv.Value = (arg1[0].getDoubleValue() <= arg1[1].getDoubleValue());
            }
            else if (dt == GSCoreDataType.Integer)
            {
                bv.Value = (arg1[0].getIntegerValue() <= arg1[1].getIntegerValue());
            }
            else if (dt == GSCoreDataType.String)
            {
                bv.Value = (arg1[0].getStringValue().CompareTo(arg1[1].getStringValue()) <= 0);
            }

            return bv;
        }

        private GSCore execLt(GSCoreCollection arg1)
        {
            GSBoolean bv = new GSBoolean();
            GSCoreDataType dt = arg1.getArithmeticDataType();

            if (dt == GSCoreDataType.Double)
            {
                bv.Value = (arg1[0].getDoubleValue() < arg1[1].getDoubleValue());
            }
            else if (dt == GSCoreDataType.Integer)
            {
                bv.Value = (arg1[0].getIntegerValue() < arg1[1].getIntegerValue());
            }
            else if (dt == GSCoreDataType.String)
            {
                bv.Value = (arg1[0].getStringValue().CompareTo(arg1[1].getStringValue()) < 0);
            }

            return bv;
        }

        private GSCore execSet(GSCore keyElem, GSCore valueElem)
        {
            string key;
            if (keyElem is GSToken)
                key = (keyElem as GSToken).Token;
            else
                key = keyElem.getStringValue();
            GSCore value = valueElem.Execute();
            if (getLastVars().ContainsKey(key))
                getLastVars()[key] = value;
            else
                getLastVars().Add(key, value);
            return value;
        }

        private GSCore execNot(GSCoreCollection args)
        {
            bool result = true;

            if (args.Count > 0)
                result = !args[0].getBooleanValue();
            return new GSBoolean() { Value = result };
        }

        private GSCore execOr(GSCoreCollection args)
        {
            bool result = false;
            foreach (GSCore item in args)
            {
                if (item.getBooleanValue() == true)
                {
                    result = true;
                    break;
                }
            }
            return new GSBoolean() { Value = result };
        }

        private GSCore execAnd(GSCoreCollection args)
        {
            bool result = true;
            foreach (GSCore item in args)
            {
                if (item.getBooleanValue() == false)
                {
                    result = false;
                    break;
                }
            }
            return new GSBoolean() { Value = result };
        }

        private GSCore execDiv(GSCoreCollection args)
        {
            GSCoreDataType dataType = args.getArithmeticDataType();

            switch (dataType)
            {
                case GSCoreDataType.Double:
                    {
                        double[] arr = args.getDoubleArray();
                        double sum = arr[0];
                        for (int i = 1; i < arr.Length; i++)
                            sum /= arr[i];
                        return new GSNumber() { DoubleValue = sum };
                    }
                case GSCoreDataType.Integer:
                case GSCoreDataType.Boolean:
                    {
                        long[] arr = args.getIntegerArray();
                        long sum = arr[0];
                        for (int i = 1; i < arr.Length; i++)
                            sum /= arr[i];
                        return new GSNumber() { IntegerValue = sum };
                    }
                default:
                    break;
            }

            return new GSString();
        }

        private GSCore execMul(GSCoreCollection args)
        {
            GSCoreDataType dataType = args.getArithmeticDataType();

            switch (dataType)
            {
                case GSCoreDataType.Double:
                    {
                        double[] arr = args.getDoubleArray();
                        double sum = 1.0;
                        for (int i = 0; i < arr.Length; i++)
                            sum *= arr[i];
                        return new GSNumber() { DoubleValue = sum };
                    }
                case GSCoreDataType.Integer:
                case GSCoreDataType.Boolean:
                    {
                        long[] arr = args.getIntegerArray();
                        long sum = 1;
                        for (int i = 0; i < arr.Length; i++)
                            sum *= arr[i];
                        return new GSNumber() { IntegerValue = sum };
                    }
                default:
                    break;
            }

            return new GSString();
        }

        private GSCore execSub(GSCoreCollection args)
        {
            GSCoreDataType dataType = args.getArithmeticDataType();

            switch (dataType)
            {
                case GSCoreDataType.Double:
                    {
                        double[] arr = args.getDoubleArray();
                        double sum = arr[0];
                        for (int i = 1; i < arr.Length; i++)
                            sum -= arr[i];
                        return new GSNumber() { DoubleValue = sum };
                    }
                case GSCoreDataType.Integer:
                case GSCoreDataType.Boolean:
                    {
                        long[] arr = args.getIntegerArray();
                        long sum = arr[0];
                        for (int i = 1; i < arr.Length; i++)
                            sum -= arr[i];
                        return new GSNumber() { IntegerValue = sum };
                    }
                default:
                    break;
            }

            return new GSString();
        }

        private GSCore execAdd(GSCoreCollection args)
        {
            GSCoreDataType dataType = args.getArithmeticDataType();

            switch (dataType)
            {
                case GSCoreDataType.String:
                case GSCoreDataType.Void:
                    {
                        string[] arr = args.getStringArray();
                        StringBuilder sb = new StringBuilder();
                        foreach (string s in arr)
                        {
                            if (sb.Length > 0)
                                sb.Append(' ');
                            sb.Append(s);
                        }
                        return new GSString() { Value = sb.ToString() };
                    }
                case GSCoreDataType.Double:
                    {
                        double[] arr = args.getDoubleArray();
                        double sum = 0;
                        for (int i = 0; i < arr.Length; i++)
                            sum += arr[i];
                        return new GSNumber() { DoubleValue = sum };
                    }
                case GSCoreDataType.Integer:
                case GSCoreDataType.Boolean:
                    {
                        long[] arr = args.getIntegerArray();
                        long sum = 0;
                        for (int i = 0; i < arr.Length; i++)
                            sum += arr[i];
                        return new GSNumber() { IntegerValue = sum };
                    }
                default:
                    break;
            }

            return new GSString();
        }


        // this function is also in GSCore object
        // evaluates property into value
        public GSCore GetTokenValue(string Token)
        {
            // find in variables
            GSCore obj = FindObject(Token);
            if (obj != null)
                return obj;

            // find built-in property
            if (Token.Equals("name"))
                return new GSString() { Value = "Executor" };

            // return default empty string
            return new GSString();
        }

        public GSCore EvaluateToken(string Token)
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


        /// <summary>
        /// Looks for object with given name in the stack of variables
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public GSCore FindObject(string name)
        {
            for (int i = stackVars.Count - 1; i >= 0; i--)
            {
                if (stackVars[i].ContainsKey(name))
                {
                    return stackVars[i][name];
                }
            }

            return null;
        }

        public void PopStack()
        {
            if (stackVars.Count > 1)
                stackVars.RemoveAt(stackVars.Count - 1);
        }

        public void PushStack()
        {
            stackVars.Add(new Dictionary<string, GSCore>());
        }
    }
}
