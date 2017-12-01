using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GCAL.CalendarDataView
{
    public class CDVParaStyle
    {
        public string StyleName = "";


        public CDVAlign Align = CDVAlign.Left;
        public CDVBorder Padding = new CDVBorder(0);
        public CDVBorder Margin = new CDVBorder(0);
        public int RoundCorner = 0;
        public int BorderWidth = 0;
        public UInt32 BorderColor = 0;
        public UInt32 BackgroundColor = CDVColor.Transparent;
    }
}
