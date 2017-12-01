using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace GCAL.CalendarDataView
{
    public class CDVWord: CDVAtom
    {
        public string Text { get; set; }

        public CDVWord(CDVAtom owner): base(owner)
        {

        }

        public override int GetMinimumWidth(CDVContext context)
        {
            Font f = CDVContext.GetFont(TextStyle.Font, TextStyle.FontSize, TextStyle.Bold, TextStyle.Italic, TextStyle.Underline);
            SizeF sf = context.g.MeasureString(Text, f);
            return base.GetMinimumWidth(context);
        }

        public override void DrawInRect(CDVContext context)
        {
            Font f = CDVContext.GetFont(TextStyle.Font, TextStyle.FontSize, TextStyle.Bold, TextStyle.Italic, TextStyle.Underline);
            context.g.DrawString(Text, f, CDVContext.GetBrush(TextStyle.Color), Location);
        }

        public override void MeasureRect(CDVContext context, Rectangle availableArea)
        {
            Font f = CDVContext.GetFont(TextStyle.Font, TextStyle.FontSize, TextStyle.Bold, TextStyle.Italic, TextStyle.Underline);
            SizeF sf = context.g.MeasureString(Text, f);
            Bounds = new Rectangle(availableArea.X, availableArea.Y, (int)sf.Width + 1, (int)sf.Height + 1);
        }
    }
}
