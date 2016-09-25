using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;

using GCAL.Base;
using GCAL.Views;

namespace GCAL.CompositeViews
{
    public partial class LocationsPanel : UserControl
    {
        public GVCore Controller { get; set; }

        public LocationsPanel()
        {
            InitializeComponent();

            InitCountries();

            m_wndCountry.SelectedIndex = 0;
        }

        public void InitCountries()
        {
            int i, m, a;

            //	m_wndCtrs.DeleteAllItems();
            m_wndCountry.BeginUpdate();
            m_wndCountry.Items.Clear();
            m = TCountry.GetCountryCount();
            for (i = 0; i < m; i++)
            {
                a = m_wndCountry.Items.Add(TCountry.GetCountryByIndex(i));
            }
            m_wndCountry.Sorted = true;
            m_wndCountry.EndUpdate();
            m_wndCountry.Items.Insert(0, "<all countries>");

        }

        private void m_wndCountry_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateLocationList();
        }

        private void UpdateLocationList()
        {
            object obj = m_wndCountry.SelectedItem;
            string country = null;

            if (obj is TCountry)
            {
                country = (obj as TCountry).name;
            }

            m_wndLocs.BeginUpdate();
            m_wndLocs.Items.Clear();
            foreach (CLocation L in CLocationList.locationList)
            {
                if (country == null || L.countryName.Equals(country))
                {
                    ListViewItem lvi = ListItemFromLocation(L);
                    m_wndLocs.Items.Add(lvi);
                }
            }
            m_wndLocs.EndUpdate();
        }

        private ListViewItem ListItemFromLocation(CLocation L)
        {
            ListViewItem lvi = new ListViewItem(L.cityName);
            lvi.SubItems.Add(L.countryName);
            lvi.SubItems.Add(GCEarthData.GetTextLatitude(L.latitudeDeg));
            lvi.SubItems.Add(GCEarthData.GetTextLongitude(L.longitudeDeg));
            lvi.SubItems.Add(TTimeZone.GetTimeZoneName(L.timezoneId));
            lvi.Tag = L;

            return lvi;
        }

        /// <summary>
        /// new location
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            EditLocationPanel d = new EditLocationPanel();
            d.setLocation(null);
            d.PrefferedCountry = m_wndCountry.SelectedItem;
            d.OnEditLocationDone += new TBButtonPressed(OnEditLocationDone);
            EditLocationPanelController dc = new EditLocationPanelController(d);
            dc.ViewContainer = Controller.ViewContainer;

            Controller.ViewContainer.AddControl(dc, GVControlAlign.Center);
        }

        /// <summary>
        /// edit location
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button2_Click(object sender, EventArgs e)
        {
            if (m_wndLocs.SelectedItems.Count == 0)
                return;

            CLocation loc = m_wndLocs.SelectedItems[0].Tag as CLocation;

            EditLocationPanel d = new EditLocationPanel();
            d.setLocation(loc);
            d.PrefferedCountry = m_wndCountry.SelectedItem;
            d.OnEditLocationDone += new TBButtonPressed(OnEditLocationDone);
            EditLocationPanelController dc = new EditLocationPanelController(d);
            dc.ViewContainer = Controller.ViewContainer;

            Controller.ViewContainer.AddControl(dc, GVControlAlign.Center);
        }

        private void OnEditLocationDone(object sender, EventArgs e)
        {
            if (sender is EditLocationPanel)
            {
                CLocationList.m_bModified = true;
                UpdateLocationList();
            }
        }

        /// <summary>
        /// delete location
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button3_Click(object sender, EventArgs e)
        {
            if (m_wndLocs.SelectedItems.Count == 0)
                return;

            CLocation loc = m_wndLocs.SelectedItems[0].Tag as CLocation;

            string ask = string.Format("Do you want to remove location for city \"{0}\" ?", loc.cityName);

            if (MessageBox.Show(ask, "Confirm", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                CLocationList.locationList.Remove(loc);
                CLocationList.m_bModified = true;
                UpdateLocationList();
            }

        }

        /// <summary>
        /// Import
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button4_Click(object sender, EventArgs e)
        {
            DialogResult dr = MessageBox.Show("Import List of Locations:\r\n" +
                "1. First step is to find the file with the list of locations\r\n" +
                "2. Second step is to choose from importing methods (Add or Replace)\r\n" +
                "\r\nDo you want to continue with importing?", "Notification", MessageBoxButtons.YesNo, MessageBoxIcon.Information);

            if (dr != DialogResult.Yes)
                return;

            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "GLOC files (*.gloc)|*.gloc||";

            if (ofd.ShowDialog() != DialogResult.OK)
            {
                MessageBox.Show("Importing was cancelled.");
                return;
            }

            dr = MessageBox.Show("Do you want to ADD selected location file to the current database of locations?\r\n" +
                "Press YES for Adding, press NO for replacement of current database with imported file.\r\n" +
                "Press CANCEL for not importing at all.", "Importing method", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);

            if (dr == DialogResult.Cancel)
                return;

            if (CLocationList.ImportFile(ofd.FileName, (dr == DialogResult.No)) == false)
	        {
		        MessageBox.Show("Importing of file was not succesful.", "Importing progress");
		        return;
	        }

	        // opatovna inicializacia dialog boxu
	        m_wndLocs.Items.Clear(); // m_wndList.ResetContent();
	        m_wndCountry.Items.Clear();

	        // setting the current country
	        InitCountries();
	        m_wndCountry.SelectedIndex = 0;
        }

        /// <summary>
        /// Export 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button5_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();

            sfd.Filter = "XML file (*.xml)|*.xml|Text file (*.txt)|*.txt|Locations List (*.lox)|*.lox|Rich List Format (*.rl)|*.rl||";

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                CLocationList.SaveAs( sfd.FileName, sfd.FilterIndex);
            }
        }

        /// <summary>
        /// Show Google Maps
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button6_Click(object sender, EventArgs e)
        {
            if (m_wndLocs.SelectedItems.Count == 0)
                return;

	        CLocation loc = m_wndLocs.SelectedItems[0].Tag as CLocation;

		    if (loc != null)
		    {
			    string str = string.Format("<html><head><meta http-equiv=\"REFRESH\" content=\"0;url=http://maps.google.com/?ie=UTF8&ll={0},{1}&spn=0.774196,1.235962&z=10" +
						        "\"></head><body></body><html>", loc.latitudeDeg, loc.longitudeDeg);
			    string fileName = GCGlobal.getFileName(GlobalStringsEnum.GSTR_TEMFOLDER);
			    fileName += "temp.html";
                File.WriteAllText(fileName, str);
                System.Diagnostics.Process.Start(fileName);
		    }
        }

        /// <summary>
        /// Reset
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button7_Click(object sender, EventArgs e)
        {
	        if (MessageBox.Show("Are you sure to revert list of locations to the internal build-in list of locations?", "Confirm", MessageBoxButtons.YesNo) == DialogResult.Yes)
	        {
		        string fileName = GCGlobal.getFileName(GlobalStringsEnum.GSTR_LOCX_FILE);
		        CLocationList.RemoveAll();
		        CLocationList.InitInternal(fileName);

                UpdateLocationList();
	        }
        }

        private void onTextFilterChanged(object sender, EventArgs e)
        {
            if (textBox1.Text.Length == 0)
            {
                UpdateLocationList();
                return;
            }

            string[] sp = textBox1.Text.Trim().ToLower().Split(' ');

            if (sp.Length == 0)
            {
                UpdateLocationList();
                return;
            }
            int A, B;
            m_wndLocs.BeginUpdate();
            m_wndLocs.Items.Clear();
            foreach (CLocation L in CLocationList.locationList)
            {
                A = B = 0;
                foreach (string s in sp)
                {
                    A++;
                    if (s.StartsWith("#"))
                    {
                        if (L.countryName.IndexOf(s.Substring(1), 0, StringComparison.CurrentCultureIgnoreCase) >= 0)
                            B++;
                    }
                    else
                    {
                        if (L.cityName.IndexOf(s.Substring(1), 0, StringComparison.CurrentCultureIgnoreCase) >= 0)
                            B++;
                    }
                }

                if (A == B)
                {
                    ListViewItem lvi = ListItemFromLocation(L);
                    m_wndLocs.Items.Add(lvi);
                }
            }
            m_wndLocs.EndUpdate();
        }
    }

    public class LocationsPanelController : GVCore
    {
        public LocationsPanelController(LocationsPanel v)
        {
            View = v;
            v.Controller = this;
        }

        public override Base.Scripting.GSCore ExecuteMessage(string token, Base.Scripting.GSCoreCollection args)
        {
            return base.ExecuteMessage(token, args);
        }
    }

}
