using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GCAL.Base.Documents
{
    public class GDTableRow: GDNode
    {
        public List<GDTableCell> cells = new List<GDTableCell>();

        public GDTableCell NewCell()
        {
            GDTableCell cell = new GDTableCell();
            cells.Add(cell);
            return cell;
        }
    }
}
