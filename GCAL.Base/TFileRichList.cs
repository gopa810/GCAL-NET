using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GCAL.Base
{
    public class TFileRichList: TFile
    {
        public int m_nLineTop;
	    public StringBuilder m_szTemp = new StringBuilder();
        public string m_szTag;
	    public List<string> m_pFields;

        public int ReadLine()
        {
	        int rc = 0;
            string line;

            line = base.ReadLine();

	        if (line == null)
		        return 0;

            if (m_pFields == null)
                m_pFields = new List<string>();
            if (m_pFields.Count > 0)
                m_pFields.Clear();

	        // analyza riadku
            rc = line.IndexOf(' ');
            if (rc > 0)
            {
                m_szTag = line.Substring(0, rc);
                line = line.Substring(rc + 1);
            }

            int mode = 0;
            StringBuilder sb = new StringBuilder();
            foreach (char c in line)
            {
                if (mode == 0)
                {
                    if (c == '|')
                    {
                        m_pFields.Add(sb.ToString());
                        sb.Clear();
                    }
                    else if (c == '\\')
                    {
                        mode = 1;
                    }
                    else
                    {
                        sb.Append(c);
                    }
                }
                else if (mode == 1)
                {
                    sb.Append(c);
                    mode = 0;
                }
            }

            if (sb.Length > 0)
                m_pFields.Add(sb.ToString());

	        return 1;
        }

        public string GetTag()
        {
            return m_szTag;
        }

        public string GetField(int i)
        {
            if (i < 0 || i >= m_pFields.Count)
                return "";
            return m_pFields[i];
        }

        public void Clear()
        {
            if (m_pFields != null)
                m_pFields.Clear();
        }

        public int AddInt(int i)
        {
            return AddText(i.ToString());
        }

        public int AddTag(int i)
        {
            m_szTemp.Clear();
            m_szTemp.AppendFormat("{0} ", i);
            m_nLineTop = 0;
            return m_szTemp.Length;
        }

        public int AddReal(double r)
        {
            return AddText(r.ToString());
        }

        public int AddText(string szText)
        {
            int cnt = 0;
            if (m_nLineTop != 0)
                m_szTemp.Append('|');

            m_szTemp.Append(szText.Replace("\\", "\\\\").Replace("|", "\\|"));
            m_nLineTop++;
            return m_szTemp.Length;
        }

        public int WriteLine()
        {
            main.AppendLine(m_szTemp.ToString());
            m_szTemp.Clear();
            m_nLineTop = 0;
            return 1;
        }

    }
}
