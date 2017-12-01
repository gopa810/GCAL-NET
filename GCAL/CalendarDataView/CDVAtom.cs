using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace GCAL.CalendarDataView
{
    public class CDVAtom
    {
        public CDVAtom Parent { get; set; }

        private Rectangle p_bounds;

        public Size Size {  get { return p_bounds.Size; } set { p_bounds.Size = value; } }

        public Point Location {  get { return p_bounds.Location; } set { p_bounds.Location = value; } }

        public Rectangle Bounds {  get { return p_bounds; } set { p_bounds = value; } }

        private CDVParaStyle p_para_style = null;
        private CDVTextStyle p_text_style = null;

        public virtual void Offset(int x, int y)
        {
            p_bounds.Offset(x, y);
        }

        public CDVParaStyle ParaStyle
        {
            get
            {
                if (p_para_style == null && Parent != null)
                    return Parent.ParaStyle;
                return p_para_style;
            }
            set
            {
                p_para_style = value;
            }
        }

        public CDVTextStyle TextStyle
        {
            get
            {
                if (p_text_style == null && Parent != null)
                    return Parent.TextStyle;
                return p_text_style;
            }
            set
            {
                p_text_style = value;
            }
        }


        public CDVAtom(CDVAtom parent)
        {
            Parent = parent;
        }

        public virtual int GetMinimumWidth(CDVContext context)
        {
            return 10;
        }

        public virtual void MeasureRect(CDVContext context, Rectangle availableArea)
        {

        }

        public virtual void DrawInRect(CDVContext context)
        {

        }
    }
}
