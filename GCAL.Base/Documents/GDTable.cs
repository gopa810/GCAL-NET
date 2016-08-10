using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GCAL.Base.Documents
{
    public class GDTable: GDNode
    {
        public List<GDTableRow> rows = new List<GDTableRow>();

        public GDTableRow NewRow()
        {
            GDTableRow row = new GDTableRow();
            rows.Add(row);
            return row;
        }
    }
}
