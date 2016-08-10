using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using GCAL.Base;

namespace GCAL.CompositeViews
{
    public partial class DateTimePanel : UserControl
    {
        private TTimeZone timeZone = null;

        public DateTimePanel()
        {
            InitializeComponent();

            for (int i = 1; i < 32; i++)
                cbDay.Items.Add(i.ToString());
            for (int j = 1; j < 12; j++)
                cbMonth.Items.Add(GregorianDateTime.GetMonthName(j));
        }

        public GregorianDateTime DateTime
        {
            get
            {
                GregorianDateTime vc = new GregorianDateTime();
                vc.day = cbDay.SelectedIndex + 1;
                vc.month = cbMonth.SelectedIndex + 1;
                vc.year = Convert.ToInt32(nddYear.Value);
                vc.shour = Convert.ToDouble(nddHour.Value) / 24.0 + Convert.ToDouble(nddMinute.Value) / 1440.0;
                vc.InitWeekDay();
                if (timeZone != null)
                    vc.TimezoneHours = timeZone.OffsetMinutes / 60.0;
                return vc;
            }
            set
            {
                if (value != null)
                {
                    int i;
                    value.NormalizeValues();
                    cbDay.SelectedIndex = value.day - 1;
                    cbMonth.SelectedIndex = value.month - 1;
                    if (value.year >= nddYear.Minimum && value.year < nddYear.Maximum)
                        nddYear.Value = value.year;
                    double d = value.shour * 24.0;
                    i = GCMath.IntFloor(d);
                    if ( i < 24)
                        nddHour.Value = GCMath.IntFloor(d);
                    d -= Math.Floor(d);
                    d *= 60;
                    i = GCMath.IntFloor(d);
                    if (i < 60)
                        nddMinute.Value = i;
                }
            }
        }

        public TTimeZone Timezone
        {
            set
            {
                timeZone = value;
                if (value != null)
                    labelTimezoneName.Text = value.name;
            }
        }
    }
}
