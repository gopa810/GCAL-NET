using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GCAL.Base
{
    public class GCCalendarEventSankranti
    {
        public int DayOffset { get; set; }
        public int Rasi { get; set; }
        public string Text { get; set; }

        public GCCalendarEventSankranti()
        {
            DayOffset = 0;
            Rasi = SankrantiId.MAKARA_SANKRANTI;
        }
    }
}
