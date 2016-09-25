using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using GCAL.Base;
using GCAL.Base.Scripting;
using GCAL.Views;

namespace GCAL.CompositeViews
{
    public partial class EditLocationPanel : UserControl
    {
        public CLocation SelectedLocation;
        public EditLocationPanelController Controller { get; set; }
        public event TBButtonPressed OnEditLocationDone;

        public EditLocationPanel()
        {
            InitializeComponent();
        }

        public void setLocation(CLocation selloc)
        {
            SelectedLocation = selloc;

            TCountry findCountry = (PrefferedCountry is TCountry ? PrefferedCountry as TCountry : null);
            comboBox1.BeginUpdate();
            foreach (TCountry ct in TCountry.gcountries)
            {
                comboBox1.Items.Add(ct);
            }
            comboBox1.Sorted = true;
            comboBox1.EndUpdate();

            comboBox2.BeginUpdate();
            foreach (TTimeZone tz in TTimeZone.gzone)
            {
                comboBox2.Items.Add(tz);
            }
            comboBox2.EndUpdate();


            if (SelectedLocation == null)
            {
                textBox1.Text = "<new location>";
                textBox2.Text = "12N50";
                textBox3.Text = "50W13";
                comboBox1.SelectedIndex = 0;
                comboBox2.SelectedIndex = 0;
            }
            else
            {
                textBox1.Text = SelectedLocation.cityName;
                for (int i = 0; i < comboBox1.Items.Count; i++)
                {
                    TCountry tc = comboBox1.Items[i] as TCountry;
                    if (tc.name.Equals(SelectedLocation.countryName))
                    {
                        comboBox1.SelectedIndex = i;
                        break;
                    }
                }
                textBox2.Text = GCEarthData.GetTextLatitude(SelectedLocation.latitudeDeg);
                textBox3.Text = GCEarthData.GetTextLongitude(SelectedLocation.longitudeDeg);
                for (int j = 0; j < comboBox2.Items.Count; j++)
                {
                    TTimeZone tz = comboBox2.Items[j] as TTimeZone;
                    if (tz.TimezoneId == SelectedLocation.timezoneId)
                    {
                        comboBox2.SelectedIndex = j;
                        break;
                    }
                }
            }

        }

        private object _prefc;
        public object PrefferedCountry
        {
            get
            {
                return _prefc;
            }
            set
            {
                _prefc = value;
                if (value is TCountry)
                {
                    TCountry tc = value as TCountry;
                    int j = comboBox1.Items.IndexOf(value);
                    if (j >= 0)
                        comboBox1.SelectedIndex = j;
                    else
                        comboBox1.SelectedIndex = 0;

                    // selection of timezone
                    foreach (CLocation loc in CLocationList.locationList)
                    {
                        if (loc.countryName.Equals(tc.name))
                        {
                            for (int k = 0; k < comboBox2.Items.Count; k++)
                            {
                                if (loc.timezoneId == (comboBox2.Items[k] as TTimeZone).TimezoneId)
                                {
                                    comboBox2.SelectedIndex = k;
                                    break;
                                }
                            }
                            break;
                        }
                    }
                }
            }
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            textBox2.BackColor = IsLatitudeOK() ? Color.LightGreen : Color.LightCoral;
            button1.Enabled = IsLatitudeOK() && IsLongitudeOK();
        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {
            textBox3.BackColor = IsLongitudeOK() ? Color.LightGreen : Color.LightCoral;
            button1.Enabled = IsLatitudeOK() && IsLongitudeOK();
        }

        private bool IsLatitudeOK()
        {
            double d;
            return ToDouble(textBox2.Text, out d, 'N', 'S');
        }

        private bool IsLongitudeOK()
        {
            double d;
            return ToDouble(textBox3.Text, out d, 'E', 'W');
        }

        private bool ToDouble(string s, out double val, char poschar, char negchar)
        {
            int i;

            i = s.IndexOf(negchar);
            if (i >= 0)
                return ToDouble2(s, i, -1.0, out val);
            i = s.IndexOf(poschar);
            if (i >= 0)
                return ToDouble2(s, i, 1.0, out val);

            val = 0.0;
            return false;
        }

        private bool ToDouble2(string s, int i, double sig, out double val)
        {
            int a, b;
            if (int.TryParse(s.Substring(0, i), out a))
            {
                if (int.TryParse(s.Substring(i + 1), out b))
                {
                    val = sig * (a * 60.0 + b * 1.0) / 60.0;
                    return true;
                }
            }

            val = 0.0;
            return false;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (SelectedLocation == null)
                SelectedLocation = new CLocation();

            SelectedLocation.cityName = textBox1.Text;
            SelectedLocation.countryName = (comboBox1.SelectedItem as TCountry).name;
            ToDouble(textBox2.Text, out SelectedLocation.latitudeDeg, 'N', 'S');
            ToDouble(textBox3.Text, out SelectedLocation.longitudeDeg, 'E', 'W');
            TTimeZone tz = comboBox2.SelectedItem as TTimeZone;
            SelectedLocation.timezoneId = tz.TimezoneId;
            SelectedLocation.offsetUtcHours = tz.OffsetMinutes / 60;

            if (OnEditLocationDone != null)
                OnEditLocationDone(SelectedLocation, e);

            Controller.RemoveFromContainer();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Controller.RemoveFromContainer();
        }


    }

    public class EditLocationPanelController : GVCore
    {
        public EditLocationPanelController(EditLocationPanel v)
        {
            View = v;
            v.Controller = this;
        }
    }
}
