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
    /// <summary>
    /// Besides normal initialization of the control within Windows Forms system,
    /// programmer should initialize first usage of this control in this way:
    /// 
    /// control.DataSource = [objectRef]
    ///    where objectRef is reference to object conforming to interface CDVDataSource
    ///    
    /// control.InitWithKey(Key)
    ///    where key is some string that should be recognizable by mother-application
    ///    control itself does not know what is behind this key, it serves just as identification
    ///    of the piece of document that is to be displayed
    ///    mother-application should be able to provide also "next value" for given "value" as well as
    ///    "previous value", so mother application should have well defined system of succession of values
    ///    so it can provide next key, previous key for given key and also display data for any KEY provided by control
    /// </summary>
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

        public CDVAtom GetDocument()
        {
            return Document;
        }

        public void OnCDVDataAvailable(CDVDocumentCell data)
        {
            Document.Cells[data.Key] = data;
            if (MainAtom == null)
            {
                MainAtom = data;
                MainAtomPosition = 0;
            }

            // TODO: Invalidate only if expected documentpart that should be displayed
            // and user was waiting for it
            Invalidate();
        }

        public void InitWithKey(string key)
        {
            if (Document.ContainsKey(key))
            {
                CDVDocumentCell atom = Document.GetItemForKey(key);
                Size atomSize = atom.Item.Size;
                Point atomLoc = atom.Item.Location;
                Point newAtomLoc = new Point(0, Height / 2 - atomSize.Height / 2);
                atom.Item.Offset(newAtomLoc.X - atomLoc.X, newAtomLoc.Y - atomLoc.Y);
                MainAtom = atom;
                MainAtomPosition = 50;
            }
            else
            {
                MainAtom = null;
                MainAtomPosition = -1;
                RequestAsyncKey(key);
            }

            Invalidate();
        }

        private void RequestAsyncKey(string key)
        {
            if (DataSource != null)
            {
                CDVDocumentCell cell = new CDVDocumentCell();
                cell.Key = key;
                DataSource.AsyncRequestData(this, cell);
            }
        }

        private RectangleF BoundsFloat
        {
            get
            {
                return new RectangleF(0, 0, Size.Width, Size.Height);
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
            Rectangle clientArea = this.ClientRectangle;

            // drawing main atom
            MainAtom.Item.MeasureRect(ctx, clientArea.Width);
            MainAtom.Item.DrawInRect(ctx);
            /*
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
                RequestAsyncKey(last.NextKey);
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
                RequestAsyncKey(last.PrevKey);
            }
            */
        }
    }
}
