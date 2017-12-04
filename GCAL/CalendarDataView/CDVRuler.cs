using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace GCAL.CalendarDataView
{
    public class CDVRuler: CDVAtom
    {
        public int Value { get; set; }
        public int Width { get; set; }

        public CDVRuler(CDVAtom parent): base(parent)
        {
            Value = 0;
            Width = 6;
        }

        public override int GetMinimumWidth(CDVContext context)
        {
            return Width;
        }

        public override void MeasureRect(CDVContext context, Rectangle availableArea)
        {
            if (availableArea.Left > Value)
                Value = availableArea.Left;
            Bounds = new Rectangle(Value, 0, Width, 0);
        }

        public override void DrawInRect(CDVContext context)
        {
        }
    }
}
