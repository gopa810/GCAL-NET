using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GCAL.Base.Documents
{
    /// <summary>
    /// This is list of elements in LISP program. Therefore LISP is
    /// called programing language for lists.
    /// </summary>
    public class GSList: GSCore
    {
        public GSCoreCollection Parts = new GSCoreCollection();

        public GSList Parent = null;

        public void Add(GSCore obj)
        {
            Parts.Add(obj);
        }

        public int Count
        {
            get
            {
                return Parts.Count;
            }
        }

        public GSCore this[int index]
        {
            get
            {
                return Parts[index];
            }
        }

        public GSList createAndAddSublist()
        {
            GSList list = new GSList() { Parent = this };
            Parts.Add(list);
            return list;
        }

        public string getFirstToken()
        {
            if (Parts.Count > 0)
                return Parts[0].getStringValue();
            return String.Empty;
        }

        public override string getStringValue()
        {
            GSCore result = Execute();
            return result.getStringValue();
        }

        public override long getIntegerValue()
        {
            GSCore result = Execute();
            return result.getIntegerValue();
        }

        public override bool getBooleanValue()
        {
            GSCore result = Execute();
            return result.getBooleanValue();
        }

        public override double getDoubleValue()
        {
            GSCore result = Execute();
            return result.getDoubleValue();
        }


        public override void setExecutor(GSExecutor exec)
        {
            base.setExecutor(exec);

            foreach (GSCore item in Parts)
            {
                item.setExecutor(exec);
            }
        }

        public override GSCore Execute()
        {
            if (Parts.IsFirstToken())
            {
                GSCore res = null;
                try
                {
                    res = Executor.ExecuteToken(Parts.getFirstToken(),
                        Parts.getSublist(1));
                }
                catch
                {
                    res = new GSString();
                }
                finally
                {
                }
                return res;
            }
            else
            {
                GSCore result = null;
                foreach (GSCore item in Parts)
                {
                    result = item.Execute();
                }
                if (result == null)
                    return new GSString();
                return result;
            }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("(");
            bool firstSpace = false;
            foreach (GSCore item in Parts)
            {
                if (firstSpace)
                    sb.Append(" ");
                sb.Append(item.ToString());
                firstSpace = true;
            }
            sb.Append(")");
            return sb.ToString();
        }
    }
}
