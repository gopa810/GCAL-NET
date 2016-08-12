using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace GCAL.Base.Documents
{
    /// <summary>
    /// This is representation of LISP program in the memory
    /// Main separators are space, (, )
    /// tokens are writen without parenthesses
    /// string constants are writen with starting '
    /// number constants are writen without parenthesses and are automaticaly recognized
    /// example:
    /// 
    /// (add (mul 12.0 a) 25)
    /// 
    /// if token is on first position in the list, then it is regarded as command
    /// if token is within the list, it is regarded as variable or object
    /// </summary>
    public class GSScript: GSList
    {
        public void readTextTemplate(String text)
        {
            Parts.Clear();
            Parent = this;

            StringReader sr = new StringReader(text);
            String line = null;
            GSList currentList = this;

            while ((line = sr.ReadLine()) != null)
            {
                if (line.StartsWith("#"))
                {
                    readTextTemplateLine(line.Substring(1), ref currentList);
                }
                else
                {
                    GSList list = currentList.createAndAddSublist();
                    list.Add(new GSToken(){ Token = "print" });
                    list.Add(new GSString(){ Value = line });
                }
            }
        }

        public void readTextTemplateLine(string line, ref GSList currentList)
        {
            // if line starts with #, then 
            if (line.TrimStart().StartsWith("#"))
                return;

            int mode = 0;
            StringBuilder part = new StringBuilder();
            foreach (char C in line)
            {
                if (mode == 0)
                {
                    if (char.IsWhiteSpace(C))
                    {
                    }
                    else if (C == '\'')
                    {
                        mode = 1;
                    }
                    else if (C == '\"')
                    {
                        mode = 3;
                    }
                    else if (C == '(')
                    {
                        currentList = currentList.createAndAddSublist();
                    }
                    else if (C == ')')
                    {
                        currentList = currentList.Parent;
                    }
                    else
                    {
                        part.Append(C);
                        mode = 2;
                    }
                }
                else if (mode == 1)
                {
                    if (char.IsWhiteSpace(C) || C == ')')
                    {
                        currentList.Add(new GSString() { Value = part.ToString() });
                        part.Clear();
                        mode = 0;
                        if (C == ')')
                            currentList = currentList.Parent;
                    }
                    else
                    {
                        part.Append(C);
                    }
                }
                else if (mode == 2)
                {
                    if (char.IsWhiteSpace(C) || C == ')')
                    {
                        double d;
                        int i;
                        string value = part.ToString();
                        if (double.TryParse(value, out d))
                        {
                            currentList.Add(new GSNumber() { DoubleValue = d });
                        }
                        else if (int.TryParse(value, out i))
                        {
                            currentList.Add(new GSNumber() { IntegerValue = i });
                        }
                        else
                        {
                            currentList.Add(new GSToken() { Token = part.ToString() });
                        }
                        part.Clear();
                        mode = 0;
                        if (C == ')')
                            currentList = currentList.Parent;
                    }
                    else
                    {
                        part.Append(C);
                    }
                }
                else if (mode == 3)
                {
                    if (C == '\"')
                    {
                        double d;
                        int i;
                        string value = part.ToString();
                        if (double.TryParse(value, out d))
                        {
                            currentList.Add(new GSNumber() { DoubleValue = d });
                        }
                        else if (int.TryParse(value, out i))
                        {
                            currentList.Add(new GSNumber() { IntegerValue = i });
                        }
                        else
                        {
                            currentList.Add(new GSToken() { Token = part.ToString() });
                        }
                        part.Clear();
                        mode = 0;
                    }
                    else if (C == '\\')
                    {
                        mode = 4;
                    }
                    else
                    {
                        part.Append(C);
                    }
                }
                else if (mode == 4)
                {
                    part.Append(C);
                    mode = 3;
                }
            }
        }

        public GSCore ExecuteScript(GSExecutor exec)
        {
            setExecutor(exec);

            return Execute();
        }
    }
}
