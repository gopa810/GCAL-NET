using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace GCAL.CalendarDataView
{
    public partial class CalendarDataView : UserControl, CDVDataTarget
    {
        public CDVDocument Document { get; set; }

        public CDVDataSource DataSource { get; set; }

        public CDVDocumentCell MainAtom { get; set; }

        /// <summary>
        /// value from 0 to 100 (percentual position of center of the view in the main atom)
        /// </summary>
        public int MainAtomPosition { get; set; }

        public CalendarDataView()
        {
            InitializeComponent();

            Document = new CDVDocument();
        }

        public void OnCDVDataAvailable(CDVDocumentCell data)
        {
            throw new NotImplementedException();
        }

        public void InitWithKey(string key)
        {
            if (Document.ContainsKey(key))
            {
                CDVDocumentCell atom = Document.GetItemForKey(key);
                Size atomSize = atom.Item.Size;
                Point atomLoc = atom.Item.Location;
                Point newAtomLoc = new Point(0, Height / 2 - atomSize.Height / 2);
                Size newOffset = new Size(newAtomLoc.X - atomLoc.X, newAtomLoc.Y - atomLoc.Y);
                atom.Item.Offset(newOffset);
                MainAtom = atom;
                MainAtomPosition = 50;
            }
            else
            {
                if (DataSource != null)
                {
                    CDVDocumentCell cell = new CDVDocumentCell();
                    cell.Key = key;
                    DataSource.AsyncRequestData(this, cell);
                    MainAtom = null;
                    MainAtomPosition = -1;
                }
            }

            Invalidate();
        }

        private RectangleF BoundsFloat
        {
            get
            {
                return new RectangleF(Bounds.X, Bounds.Y, Bounds.Width, Bounds.Height);
            }
        }

        /// <summary>
        /// Assumption is that at least MainAtom is correctly placed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CalendarDataView_Paint(object sender, PaintEventArgs e)
        {
            if (MainAtom == null)
            {
                e.Graphics.DrawString("Please wait a moment...", SystemFonts.MenuFont, Brushes.Black, BoundsFloat, CDVContext.StringFormatCenterCenter);
                return;
            }

            CDVContext ctx = new CDVContext();
            ctx.g = e.Graphics;
            CDVDocumentCell last = MainAtom;

            // drawing main atom
            MainAtom.Item.DrawInRect(ctx);

            // drawing after
            while (Document.ContainsKey(last.NextKey) && last.Item.Bounds.Bottom < Height)
            {
                // drawing next atom
                CDVDocumentCell nc = Document.GetItemForKey(last.NextKey);
                nc.MoveAfter(last);
                nc.Item.DrawInRect(ctx);
                last = nc;
            }

            if (!Document.ContainsKey(last.NextKey))
            {
                // here place order to datasource for nextkey
            }

            last = MainAtom;

            while(Document.ContainsKey(last.PrevKey) && last.Item.Bounds.Top > 0)
            {
                CDVDocumentCell nc = Document.GetItemForKey(last.PrevKey);
                nc.MoveBefore(last);
                nc.Item.DrawInRect(ctx);
                last = nc;
            }

            if (!Document.ContainsKey(last.PrevKey))
            {
                // here place order to datasource for nextkey
            }

        }
    }
}
