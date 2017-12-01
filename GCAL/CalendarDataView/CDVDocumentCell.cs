using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GCAL.CalendarDataView
{
    public class CDVDocumentCell
    {
        public string Key = "";
        public string PrevKey = "";
        public string NextKey = "";
        public CDVAtom Item = null;

        public void MoveAfter(CDVDocumentCell after)
        {
            Item.Offset(0, after.Item.Bounds.Bottom - Item.Bounds.Location.Y);
        }

        public void MoveBefore(CDVDocumentCell before)
        {
            Item.Offset(0, before.Item.Bounds.Top - Item.Size.Height - Item.Bounds.Location.Y);
        }
    }
}
