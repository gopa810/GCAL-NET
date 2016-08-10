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
    public partial class ChooseLocationPanel : UserControl
    {
        public static int nCurrentCountry = 0;

        public static CLocation lastLocation = null;

        public ChooseLocationPanel()
        {
            InitializeComponent();

            InitCountries();

            comboBox1.SelectedIndex = nCurrentCountry;

            SelectLocation(lastLocation);
        }

        public CLocation SelectedLocation
        {
            get
            {
                if (listView1.SelectedItems.Count > 0)
                {
                    lastLocation = listView1.SelectedItems[0].Tag as CLocation;
                    return lastLocation;
                }

                if (lastLocation != null)
                    return lastLocation;

                if (CLocationList.locationList != null)
                    return CLocationList.locationList[0];

                return null;
            }
            set
            {
                if (SelectLocation(value) == false)
                {
                    if (comboBox1.SelectedIndex > 0)
                    {
                        comboBox1.SelectedIndex = 0;
                        if (SelectLocation(value) == false)
                        {
                            SelectFirstLocation();
                        }
                    }
                }
            }
        }

        private bool SelectLocation(CLocation loc)
        {
            if (loc == null)
            {
                SelectFirstLocation();
                return true;
            }

            foreach (ListViewItem lvi in listView1.Items)
            {
                if (lvi.Tag == loc)
                {
                    listView1.SelectedItems.Clear();
                    lvi.Selected = true;
                    listView1.EnsureVisible(lvi.Index);
                    return true;
                }
            }

            return false;
        }

        private void InitCountries()
        {
            comboBox1.BeginUpdate();
            foreach (TCountry tc in TCountry.gcountries)
            {
                comboBox1.Items.Add(tc);
            }
            comboBox1.EndUpdate();
            comboBox1.Items.Insert(0, "<All Countries>");
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            InitCitiesForCountry(comboBox1.SelectedItem);
        }

        private void InitCitiesForCountry(object spec)
        {
            if (CLocationList.locationList != null)
            {
                TCountry tc = null;
                if (spec is TCountry)
                {
                    tc = spec as TCountry;
                } 
                
                listView1.BeginUpdate();
                listView1.Items.Clear();
                foreach (CLocation loc in CLocationList.locationList)
                {
                    if (tc == null || loc.countryName.Equals(tc.name))
                    {
                        ListViewItem lvi = new ListViewItem(loc.cityName);
                        lvi.SubItems.Add(loc.countryName);
                        lvi.SubItems.Add(GCEarthData.GetTextLatitude(loc.latitudeDeg));
                        lvi.SubItems.Add(GCEarthData.GetTextLongitude(loc.longitudeDeg));
                        lvi.SubItems.Add(TTimeZone.GetTimeZoneOffsetText(loc.offsetUtcHours));
                        lvi.Tag = loc;
                        listView1.Items.Add(lvi);
                    }
                }
                listView1.EndUpdate();

                if (SelectLocation(lastLocation))
                {
                    SelectFirstLocation();
                }
            }

        }

        private void SelectFirstLocation()
        {
            if (listView1.Items.Count > 0)
            {
                listView1.SelectedItems.Clear();
                listView1.Items[0].Selected = true;
                listView1.EnsureVisible(0);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            comboBox1.SelectedIndex = 0;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (lastLocation != null)
            {
                CLocation loc = lastLocation;
                InitCitiesForCountry(TCountry.GetCountryByName(loc.countryName));
                SelectLocation(loc);
            }
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                lastLocation = listView1.SelectedItems[0].Tag as CLocation;
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            string s = textBox1.Text.Trim();

            if (s.Length == 0)
                return;

            foreach (ListViewItem lvi in listView1.Items)
            {
                CLocation L = lvi.Tag as CLocation;
                if (L == null)
                    continue;

                if (L.cityName.IndexOf(s, StringComparison.CurrentCultureIgnoreCase) >= 0)
                {
                    listView1.EnsureVisible(lvi.Index);
                    lvi.Selected = true;
                    break;
                }
            }
        }
    }
}
