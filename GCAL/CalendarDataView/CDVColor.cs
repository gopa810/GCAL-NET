using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace GCAL.CalendarDataView
{
    public struct CDVColor
    {
        public static readonly CDVColor Transparent = new CDVColor(Color.Transparent.A, Color.Transparent.R, Color.Transparent.G, Color.Transparent.B);
        public static readonly CDVColor Black = new CDVColor(Color.Black.A, 0, 0, 0);

        public byte A;
        public byte R;
        public byte G;
        public byte B;

        public CDVColor(byte alpha, byte red, byte green, byte blue)
        {
            A = alpha;
            R = red;
            G = green;
            B = blue;
        }

        public override bool Equals(object obj)
        {
            if (obj is CDVColor)
            {
                CDVColor c = (CDVColor)obj;
                return (A == c.A && R == c.R && G == c.G && B == c.B);
            }
            else
                return base.Equals(obj);
        }
    }
}
