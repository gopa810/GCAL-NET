using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GCAL.CalendarDataView
{
    public class CDVDocument: CDVAtom
    {
        Dictionary<string, CDVTextStyle> textStyles = new Dictionary<string, CDVTextStyle>();
        Dictionary<string, CDVParaStyle> paraStyles = new Dictionary<string, CDVParaStyle>();

        public Dictionary<string, CDVDocumentCell> Cells = new Dictionary<string, CDVDocumentCell>();

        public CDVDocument(): base(null)
        {
            TextStyle = new CDVTextStyle();
            TextStyle.StyleName = "Normal";
            textStyles["Normal"] = TextStyle;

            ParaStyle = new CDVParaStyle();
            ParaStyle.StyleName = "Normal";
            paraStyles["Normal"] = ParaStyle;
        }

        public void AddTextStyle(string name, CDVTextStyle ts)
        {
            textStyles[name] = ts;
        }

        public void AddParaStyle(string name, CDVParaStyle ps)
        {
            paraStyles[name] = ps;
        }

        public bool ContainsKey(string key)
        {
            return Cells.ContainsKey(key);
        }

        public CDVDocumentCell GetItemForKey(string key)
        {
            return Cells[key];
        }

        public void InsertKey(string key, string prevKey, string nextKey, CDVAtom cellContent)
        {
            CDVDocumentCell cell = new CDVDocumentCell();
            cell.Key = key;
            cell.NextKey = nextKey;
            cell.PrevKey = prevKey;
            cell.Item = cellContent;
            cellContent.Parent = this;
        }
    }
}
