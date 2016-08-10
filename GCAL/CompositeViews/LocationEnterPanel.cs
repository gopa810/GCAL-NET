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
    public partial class LocationEnterPanel : UserControl
    {
        private CLocationRef loc = new CLocationRef();
        private bool b_upd = false;
        private double signOfLatitude = 1.0;
        private double signOfLongitude = 1.0;


        public LocationEnterPanel()
        {
            InitializeComponent();

            if (TTimeZone.gzone != null)
            {
                foreach (TTimeZone tz in TTimeZone.gzone)
                {
                    cbTimezones.Items.Add(tz);
                }
            }
        }

        /// <summary>
        /// Gets or sets main edited data in the panel.
        /// </summary>
        public CLocationRef LocationRef
        {
            get
            {
                loc = new CLocationRef();

                loc.locationName = textBox1.Text;
                loc.latitudeDeg = DoubleFromDialog(tbLatDeg, tbLatArc, signOfLatitude);
                loc.longitudeDeg = DoubleFromDialog(tbLongDeg, tbLongArc, signOfLongitude);

                TTimeZone tz = cbTimezones.SelectedItem as TTimeZone;
                if (tz != null)
                {
                    loc.offsetUtcHours = tz.OffsetMinutes / 60.0;
                    loc.timezoneId = tz.TimezoneId;
                    loc.timeZoneName = tz.name;
                }

                return loc;
            }
            set
            {
                if (value == null)
                    loc = new CLocationRef();
                else
                    loc = value;
                b_upd = true;
                textBox1.Text = loc.locationName;
                DialogLatitude = loc.latitudeDeg;
                DialogLongitude = loc.longitudeDeg;
                SelectTimezoneById(loc.timezoneId);
                b_upd = false;
            }
        }


        public double DoubleFromDialog(TextBox t1, TextBox t2, double sig)
        {
            int a, b;
            if (int.TryParse(t1.Text, out a) && int.TryParse(t2.Text, out b))
            {
                return sig * (a + b / 60.0);
            }

            return 0.0;
        }

        /// <summary>
        /// Select location from locations list
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button3_Click(object sender, EventArgs e)
        {
            DlgChooseLocation dlg = new DlgChooseLocation();

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                CLocation L = dlg.SelectedLocation;

                loc = new CLocationRef();

                loc.latitudeDeg = L.latitudeDeg;
                loc.longitudeDeg = L.longitudeDeg;
                loc.offsetUtcHours = L.offsetUtcHours;
                loc.timezoneId = L.timezoneId;
                loc.locationName = L.cityName + " [" + L.countryName + "]";

                b_upd = true;
                textBox1.Text = loc.locationName;
                DialogLatitude = loc.latitudeDeg;
                DialogLongitude = loc.longitudeDeg;
                SelectTimezoneById(loc.timezoneId);
                b_upd = false;
            }
        }

        public double DialogLatitude
        {
            get
            {
                return DoubleFromDialog(tbLatDeg, tbLatArc, signOfLatitude);
            }
            set
            {
                double a = value;
                if (a < 0.0)
                {
                    signOfLatitude = -1.0;
                    a = -a;
                }
                else
                {
                    signOfLatitude = 1.0;
                }
                tbLatDeg.Text = GCMath.IntFloor(a).ToString("00");
                tbLatArc.Text = GCMath.IntFloor((a - Math.Floor(a))*60.0).ToString("00");
                btnLatDir.Text = GCStrings.getLatitudeDirectionName(signOfLatitude);
            }
        }

        public double DialogLongitude
        {
            get
            {
                return DoubleFromDialog(tbLongDeg, tbLongArc, signOfLongitude);
            }
            set
            {
                double a = value;
                if (a < 0.0)
                {
                    signOfLongitude = -1.0;
                    a = -a;
                }
                else
                {
                    signOfLongitude = 1.0;
                }
                tbLongDeg.Text = GCMath.IntFloor(a).ToString("00");
                tbLongArc.Text = GCMath.IntFloor((a - Math.Floor(a)) * 60.0).ToString("00");
                btnLongDir.Text = GCStrings.getLongitudeDirectionName(signOfLongitude);
            }
        }

        private bool SelectTimezoneById(int id)
        {
            foreach (TTimeZone tz in cbTimezones.Items)
            {
                if (tz.TimezoneId == id)
                {
                    cbTimezones.SelectedItem = tz;
                    return true;
                }
            }
            return false;
        }

        public GCEarthData GetEarthData()
        {
            CLocationRef L = LocationRef;
            return L.EARTHDATA();
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            checkBox1.Checked = false;
            UpdateDSTInfo();
        }

        private void UpdateDSTInfo()
        {
            TTimeZone tz = cbTimezones.SelectedItem as TTimeZone;
            if (tz != null)
            {
                labelDstInfo.Text = tz.HumanDstText();
            }
        }

        private int UpdateDstByTimezone(double tzone)
        {
            if (!checkBox1.Checked)
                return 0;

            if (loc == null)
                return 0;

            double lon = (Math.Abs(DialogLongitude) + 7.5) / 15.0;
            if (DialogLongitude < 0.0)
                lon = -lon;

            foreach (TTimeZone tz in cbTimezones.Items)
            {
                if (tz.OffsetMinutes >= (int)lon * 60)
                {
                    cbTimezones.SelectedItem = tz;
                    break;
                }
            }

            UpdateDSTInfo();

            return 1;
        }

        // button longitude
        private void buttonLongDir_Click(object sender, EventArgs e)
        {
            signOfLongitude = -signOfLongitude;
            UpdatedLongitude();
            btnLongDir.Text = GCStrings.getLongitudeDirectionName(signOfLongitude);
        }

        private void UpdatedLongitude()
        {
            if (loc == null)
                return;

            if (GCUserInterface.dstSelectionMethod == 2 && b_upd == true)
            {
                loc.longitudeDeg = DoubleFromDialog(tbLongDeg, tbLongArc, signOfLongitude);
                UpdateDstByTimezone(loc.offsetUtcHours);
            }
        }

        // button latitude
        private void buttonLatDir_Click(object sender, EventArgs e)
        {
            signOfLatitude = -signOfLatitude;
            UpdatedLatitude();
            btnLatDir.Text = GCStrings.getLatitudeDirectionName(signOfLatitude);
        }

        private void UpdatedLatitude()
        {
            if (loc == null)
                return;

            if (GCUserInterface.dstSelectionMethod == 2 && b_upd == true)
            {
                loc.latitudeDeg = DoubleFromDialog(tbLatDeg, tbLatArc, signOfLatitude);
                UpdateDstByTimezone(loc.offsetUtcHours);
            }
        }

        private bool IsCorrect(TextBox tb, int min, int max)
        {
            int a;
            if (int.TryParse(tb.Text, out a))
            {
                return ((min <= a) && (a <= max));
            }
            else
            {
                return false;
            }
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            if (IsCorrect(tbLatDeg, 0, 89))
            {
                tbLatDeg.BackColor = SystemColors.Window;
                UpdatedLatitude();
            }
            else
            {
                tbLatDeg.BackColor = Color.LightCoral;
                labelDstInfo.Text = "Latitude degrees only between 0 and 89.";
            }
        }

        private void textBox4_TextChanged(object sender, EventArgs e)
        {
            if (IsCorrect(tbLatArc, 0, 59))
            {
                tbLatArc.BackColor = SystemColors.Window;
                UpdatedLatitude();
            }
            else
            {
                tbLatArc.BackColor = Color.LightCoral;
                labelDstInfo.Text = "Latitude minutes only between 0 and 59";
            }
        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {
            if (IsCorrect(tbLongDeg, 0, 179))
            {
                tbLongDeg.BackColor = SystemColors.Window;
                UpdatedLongitude();
            }
            else
            {
                tbLongDeg.BackColor = Color.LightCoral;
                labelDstInfo.Text = "Longitude degrees only between 0 and 179.";
            }
        }

        private void textBox5_TextChanged(object sender, EventArgs e)
        {
            if (IsCorrect(tbLongArc, 0, 59))
            {
                tbLongArc.BackColor = SystemColors.Window;
                UpdatedLongitude();
            }
            else
            {
                tbLongArc.BackColor = Color.LightCoral;
                labelDstInfo.Text = "Latitude minutes only between 0 and 59";
            }
        }
    }
}
