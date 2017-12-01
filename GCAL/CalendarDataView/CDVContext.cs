using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace GCAL.CalendarDataView
{
    public class CDVContext
    {
        public Graphics g;

        public static Dictionary<UInt32, Brush> Brushes = new Dictionary<uint, Brush>();
        public static Dictionary<UInt64, Pen> Pens = new Dictionary<ulong, Pen>();
        public static Dictionary<string, Font> Fonts = new Dictionary<string, Font>();

        public static StringFormat StringFormatCenterCenter;

        static CDVContext()
        {
            StringFormatCenterCenter = new StringFormat();
            StringFormatCenterCenter.Alignment = StringAlignment.Center;
            StringFormatCenterCenter.LineAlignment = StringAlignment.Center;
        }

        public static Brush GetBrush(UInt32 color)
        {
            if (Brushes.ContainsKey(color))
                return Brushes[color];

            Color c = Color.FromArgb((int)color);
            Brushes[color] = new SolidBrush(c);
            return Brushes[color];
        }

        public static Pen GetPen(int width, UInt32 color)
        {
            if (width < 1) width = 1;
            if (width > 100) width = 100;
            ulong key = ((ulong)width) << 32 | color;
            if (Pens.ContainsKey(key))
                return Pens[key];

            Pens[key] = new Pen(Color.FromArgb((int)color), width / 10f);
            return Pens[key];
        }

        public static Font GetFont(string familyName, int size, bool bold, bool italic, bool underline)
        {
            string key = string.Format("{0}-{1}-{2}{3}{4}", familyName, size, bold ? 1 : 0, italic ? 1 : 0, underline ? 1 : 0);
            if (Fonts.ContainsKey(key))
                return Fonts[key];

            Fonts[key] = new Font(new FontFamily(familyName), (float)size,
                (bold ? FontStyle.Bold : FontStyle.Regular)
                | (italic ? FontStyle.Italic : FontStyle.Regular)
                | (underline ? FontStyle.Underline : FontStyle.Regular));

            return Fonts[key];
        }
    }
}
