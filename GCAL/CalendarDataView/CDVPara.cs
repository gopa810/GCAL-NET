using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace GCAL.CalendarDataView
{
    public class CDVPara: CDVAtom
    {


        public CDVOrientation Orientation = CDVOrientation.Horizontal;
        public List<CDVAtom> Words = new List<CDVAtom>();
        private Rectangle border = Rectangle.Empty;

        public CDVPara(CDVAtom owner): base(owner)
        {

        }

        public override int GetMinimumWidth(CDVContext context)
        {
            int minWidth = 0;
            foreach(CDVAtom atom in Words)
            {
                minWidth = Math.Max(minWidth, atom.GetMinimumWidth(context));
            }

            return minWidth;
        }

        public override void DrawInRect(CDVContext context)
        {
            if (ParaStyle.BackgroundColor != CDVColor.Transparent)
            {
                context.g.FillRectangle(CDVContext.GetBrush(ParaStyle.BackgroundColor), border);
            }

            if (ParaStyle.BorderWidth > 0)
            {
                context.g.DrawRectangle(CDVContext.GetPen(ParaStyle.BorderWidth, ParaStyle.BorderColor), border);
            }

            foreach (CDVAtom atom in Words)
            {
                atom.DrawInRect(context);
            }
        }

        public override void MeasureRect(CDVContext context, Rectangle availableArea)
        {
            Location = availableArea.Location;
            Size = new Size(availableArea.Width, 4);
            Rectangle clientArea = new Rectangle(availableArea.X + ParaStyle.Margin.Left + ParaStyle.Padding.Left,
                availableArea.Y + ParaStyle.Margin.Top + ParaStyle.Padding.Top, availableArea.Width - ParaStyle.Margin.Left 
                - ParaStyle.Margin.Right - ParaStyle.Padding.Left - ParaStyle.Padding.Right, availableArea.Height - ParaStyle.Margin.Bottom - ParaStyle.Margin.Top
                - ParaStyle.Padding.Top - ParaStyle.Padding.Bottom);
            availableArea = clientArea;
            int bottom = clientArea.Top;
            List<CDVAtom> line = new List<CDVAtom>();
            foreach(CDVAtom atom in Words)
            {
                atom.MeasureRect(context, availableArea);
                if (Orientation == CDVOrientation.Vertical || atom.Bounds.Right > availableArea.Right)
                {
                    atom.Location = new Point(clientArea.X, bottom);
                    if (line.Count > 0)
                    {
                        int remainingWidth = availableArea.Width - line[line.Count - 1].Bounds.Right;
                        int correction = (ParaStyle.Align == CDVAlign.Left ? 0 : ParaStyle.Align == CDVAlign.Center ? remainingWidth / 2 : remainingWidth);
                        for (int i = 0; i < line.Count; i++)
                        {
                            line[i].Location.Offset(correction, 0);
                        }
                        line.Clear();
                    }
                }
                line.Add(atom);

                if (bottom < atom.Bounds.Bottom)
                    bottom = atom.Bounds.Bottom;
            }

            Size = new Size(Size.Width, bottom - clientArea.Top + ParaStyle.Margin.Bottom + ParaStyle.Margin.Top
                + ParaStyle.Padding.Top + ParaStyle.Padding.Bottom);

            border = new Rectangle(availableArea.X + ParaStyle.Margin.Left,
                availableArea.Y + ParaStyle.Margin.Top, availableArea.Width - ParaStyle.Margin.Left
                - ParaStyle.Margin.Right, availableArea.Height - ParaStyle.Margin.Bottom - ParaStyle.Margin.Top);

        }

        public override void Offset(int x, int y)
        {
            foreach(CDVAtom atom in Words)
            {
                atom.Offset(x, y);
            }

            border.Offset(x, y);
            base.Offset(x, y);
        }
    }
}
