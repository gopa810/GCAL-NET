using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using GCAL.Base;

namespace GCAL
{
    public partial class DlgCalcEkadasiBoundaries : Form
    {
        public GregorianDateTime SelectedDate;
        public VAISNAVADAY SelectedVaisnavaDay;
        public GCMap SelectedMap;
        private bool stopRequested = false;

        /// <summary>
        /// 0 - none
        /// 1 - Ekadasi fast
        /// </summary>
        public int FindType = 0;

        public DlgCalcEkadasiBoundaries(GregorianDateTime sd)
        {
            InitializeComponent();


            SelectedDate = sd;

        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {

        }

        private void buttonSaveImage_Click(object sender, EventArgs e)
        {

        }

        private void buttonStop_Click(object sender, EventArgs e)
        {
            stopRequested = true;
            buttonStop.Enabled = false;
        }

        private void PerformeStop()
        {
            buttonStart.Enabled = true;
            buttonStop.Enabled = false;
            stopRequested = false;
        }

        private void buttonStart_Click(object sender, EventArgs e)
        {
            if (SelectedDate == null || SelectedVaisnavaDay == null || FindType < 1)
                return;
        }

        private void FunctionCalcBoundariesSync()
        {
            Quadrant q = new Quadrant(SelectedMap);
            QuadrantArray qa = new QuadrantArray(q, 10, 10);
        }
    }

    public class Quadrant
    {
        public double LongitudeStart = 0.0;
        public double LongitudeEnd = 0.0;
        public double LatitudeStart = 0.0;
        public double LatitudeEnd = 0.0;

        public QuandrantResult Result = null;

        public Quadrant(GCMap map)
        {
            LongitudeStart = map.LongitudeStart;
            LongitudeEnd = map.LongitudeEnd;
            LatitudeStart = map.LatitudeStart;
            LatitudeEnd = map.LatitudeEnd;
        }

        public Quadrant(Quadrant map)
        {
            LongitudeStart = map.LongitudeStart;
            LongitudeEnd = map.LongitudeEnd;
            LatitudeStart = map.LatitudeStart;
            LatitudeEnd = map.LatitudeEnd;
        }

        public QuadrantArray Split(int x, int y)
        {
            return new QuadrantArray(this, x, y);
        }
    }

    public class QuadrantArray: Quadrant
    {
        public Quadrant[,] Array = null;

        public QuadrantArray(Quadrant map): base(map)
        {

        }

        public QuadrantArray(Quadrant map, int longitudeParts, int latitudeParts): base(map)
        {
            double longitudeMin = map.LongitudeStart;
            double longitudeMax = map.LongitudeEnd;
            double latitudeMin = map.LatitudeStart;
            double latitudeMax = map.LatitudeEnd;

            if (longitudeParts < 2) longitudeParts = 2;
            if (latitudeParts < 2) latitudeParts = 2;

            double longitudeDiff = (longitudeMax - longitudeMin) / longitudeParts;
            double latitudeDiff = (latitudeMax - latitudeMin) / latitudeParts;

            Array = new Quadrant[longitudeParts, latitudeParts];

            for(int lo = 0; lo < longitudeParts; lo++)
            {
                for(int la = 0; la < latitudeParts; la++)
                {
                    Array[lo, la].LongitudeStart = longitudeMin + longitudeDiff * lo;
                    Array[lo, la].LongitudeEnd = Array[lo, la].LongitudeStart + longitudeDiff;
                    Array[lo, la].LatitudeStart = latitudeMin + latitudeDiff * lo;
                    Array[lo, la].LatitudeEnd = Array[lo, la].LatitudeStart + latitudeDiff;
                }
            }
        }
    }

    public class QuandrantResult
    {
        public static QuandrantResult Empty { get; set; }
        static QuandrantResult()
        {
            Empty = new QuandrantResult();
        }

        public Color color = Color.Transparent;
        public GregorianDateTime date = null;
    }
}
