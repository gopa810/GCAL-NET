using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;

namespace GCAL.Base
{
    public class TFile
    {
        public int readLinesPos = -1;
        public string[] readLines = new string[] { };

        public StringBuilder main = new StringBuilder();

        public int WriteString(string str)
        {
            main.Append(str);
            return str.Length;
        }

        public bool ReadString(StringBuilder str)
        {
            string s = ReadLine();
            if (s == null)
                return false;
            str.Clear();
            str.Append(s);
            return true;
        }

        public bool ReadFile(string szname)
        {
            if (File.Exists(szname))
            {
                readLines = File.ReadAllLines(szname);
                readLinesPos = 0;
                return true;
            }
            return false;
        }

        public void WriteFile(string szname)
        {
            File.WriteAllText(szname, main.ToString());
        }

        public string ReadLine()
        {
            if (readLinesPos >= readLines.Length)
                return null;
            readLinesPos++;
            return readLines[readLinesPos - 1];
        }

        public bool ReadPropertyLine(StringBuilder strA, StringBuilder strB)
        {
	        strA.Clear();
	        strB.Clear();

            string s = ReadLine();
            if (s == null)
                return false;

            int idx = s.IndexOf("=");
            if (idx > 0)
            {
                strA.Append(s.Substring(0, idx));
                strB.Append(s.Substring(idx+1));
            }
            else
            {
                strA.Append(s);
            }

            return true;
        }

        public bool WriteString2(int i, string psz)
        {
            main.AppendFormat("{0}\r\n{1}\r\n", i, psz);
            return true;
        }

        public bool WriteString2(int i, int n)
        {
            main.AppendFormat("{0}\r\n{1}\r\n", i, n);
            return true;
        }

        public bool WriteString2(int i, double a)
        {
            main.AppendFormat("{0}\r\n{1}\r\n", i, a);
            return true;
        }

        public bool ReadString2(StringBuilder strA, StringBuilder strB)
        {
            if (!ReadString(strA))
                return false;
            if (!ReadString(strB))
                return false;
            string s = strB.ToString().TrimEnd();
            strB.Clear();
            strB.Append(s);
            return true;
        }

        public TFile()
        {
        }

        public static bool FileExists(string fileName)
        {
            return File.Exists(fileName);
        }

        public static void CreateFileFromResource(string resourceName, string fileName)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resource = "GCAL.Base.Resources." + resourceName;

            string[] ss = assembly.GetManifestResourceNames();

            using (Stream stream = assembly.GetManifestResourceStream(resource))
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    string result = reader.ReadToEnd();
                    File.WriteAllText(fileName, result);
                }
            }
        }

        public static void DeleteFile(string fileName)
        {
            File.Delete(fileName);
        }

    }
}
