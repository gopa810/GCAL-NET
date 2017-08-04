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

        private void button2_Click(object sender, EventArgs e)
        {
            TResultCalendar cal = new TResultCalendar();
            GCLocation loc = new GCLocation();
            TTimeZone tz = new TTimeZone();
            loc.Longitude = 0;
            loc.Latitude = int.Parse(textBox1.Text);
            loc.Title = "Test";
            loc.TimeZone = tz;
            tz.BiasMinutes = 0;
            tz.Name = "TestTZ";
            tz.OffsetMinutes = 0;
            GregorianDateTime gc = new GregorianDateTime();
            gc.Set(SelectedDate);
            gc.PreviousDay();
            gc.PreviousDay();

            for (double ln = -180; ln < 180; ln += 5)
            {
                tz.OffsetMinutes = ((int)ln) / 15 * 60;
                cal.CalculateCalendar(loc, gc, 5);
                loc.Longitude = ln;
                for (int j = 0; j < cal.m_PureCount; j++)
                {
                    VAISNAVADAY vd = cal.GetDay(j);
                    if (vd.nFastID == FastType.FAST_EKADASI)
                    {
                        richTextBox1.AppendText(string.Format("Latitude {0}  Longitude {1}   Date {2}\n", loc.Latitude, loc.Longitude, vd.date.ToString()));
                    }
                }
            }
        }
    }

    public class Quadrant
    {
        public double LongitudeFrom = 0.0;
        public double LongitudeTo = 0.0;
        public double LatitudeFrom = 0.0;
        public double LatitudeTo = 0.0;

    }
}
