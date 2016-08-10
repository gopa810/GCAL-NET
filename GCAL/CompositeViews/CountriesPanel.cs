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
    public partial class CountriesPanel : UserControl
    {
        public CountriesPanel()
        {
            InitializeComponent();


            InitCountryList();
        }

        public void InitCountryList()
        {
            listView1.Items.Clear();
            listView1.BeginUpdate();
            foreach (TCountry tc in TCountry.gcountries)
            {
                ListViewItem lvi = new ListViewItem(tc.abbreviatedName);
                lvi.SubItems.Add(tc.name);
                lvi.Tag = tc;
                listView1.Items.Add(lvi);
            }
            listView1.EndUpdate();
        }

        /// <summary>
        /// New
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            DlgEditCountry dlg = new DlgEditCountry();

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                TCountry.gcountries.Add(dlg.SelectedCountry);
                TCountry._modified = true;
                InitCountryList();
            }
        }

        /// <summary>
        /// Edit
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button2_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 0)
                return;

            TCountry sc = listView1.SelectedItems[0].Tag as TCountry;
            string prevname = (sc != null ? sc.name : "");

            DlgEditCountry dlg = new DlgEditCountry();

            dlg.SelectedCountry = sc;

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                if (sc.name.Equals(prevname))
                {
                    CLocationList.RenameCountry(prevname, sc.name);
                    CLocationList.m_bModified = true;
                }
                TCountry._modified = true;
                InitCountryList();
            }
        }

        /// <summary>
        /// Delete
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button3_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 0)
                return;

            TCountry sc = listView1.SelectedItems[0].Tag as TCountry;

            if (CLocationList.LocationCountForCountry(sc.name) > 0)
            {
                MessageBox.Show(string.Format("Country '{0}' cannot be removed now, because some locations are assigned to it."));
                return;
            }

            string ask = string.Format("Do you want to delete country with name '{0}' ?", sc.name);

            if (MessageBox.Show(ask, "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                TCountry.gcountries.Remove(sc);
                TCountry._modified = true;
            }

        }
    }
}
