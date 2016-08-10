using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using GCAL.CompositeViews;

namespace GCAL
{
    public partial class FrameOrganizer : Form
    {
        private static FrameOrganizer sharedWindow = null;
        private List<GVButton> buttons = new List<GVButton>();

        private DisplaySettingsPanel dsPanel = null;

        private EventsPanel evPanel = null;

        private LocationsPanel locPanel = null;

        private CountriesPanel couPanel = null;

        private TimezonesPanel timPanel = null;

        public FrameOrganizer()
        {
            InitializeComponent();

            buttons.Add(gvButtonDispSettings);
            buttons.Add(gvButtonEvents);
            buttons.Add(gvButtonLocations);
            buttons.Add(gvButtonCountries);
            buttons.Add(gvButtonTimezones);

            gvButtonDispSettings.Highlighted = true;
            SelectTab(0);
        }

        public static void ShowFrame()
        {
            if (sharedWindow == null)
                sharedWindow = new FrameOrganizer();

            sharedWindow.Show();

        }

        public void SelectTab(int nTab)
        {
            for (int i = 0; i < buttons.Count; i++)
            {
                buttons[i].Highlighted = false;
            }
            buttons[nTab].Highlighted = true;
            panel1.AutoScroll = false;
            foreach (Control cc in panel1.Controls)
            {
                cc.Visible = false;
            }

            switch (nTab)
            {
                case 0:
                    if (dsPanel == null)
                    {
                        dsPanel = new DisplaySettingsPanel();
                        dsPanel.Parent = panel1;
                        panel1.Controls.Add(dsPanel);
                    }
                    else
                    {
                        dsPanel.Visible = true;
                        dsPanel.AutoScrollOffset = new Point();
                    }
                    panel1.AutoScroll = true;
                    break;
                case 1:
                    if (evPanel == null)
                    {
                        evPanel = new EventsPanel();
                        evPanel.Parent = panel1;
                        evPanel.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
                        panel1.Controls.Add(evPanel);
                    }
                    else
                    {
                        evPanel.Visible = true;
                    }
                    evPanel.Location = new Point(0, 0);
                    evPanel.Size = panel1.Size;
                    panel1.AutoScroll = false;
                    break;
                case 2:
                    if (locPanel == null)
                    {
                        locPanel = new LocationsPanel();
                        locPanel.Parent = panel1;
                        locPanel.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
                        panel1.Controls.Add(locPanel);
                    }
                    else
                    {
                        locPanel.Visible = true;
                    }
                    locPanel.Location = new Point(0, 0);
                    locPanel.Size = panel1.Size;
                    panel1.AutoScroll = false;
                    break;
                case 3:
                    if (couPanel == null)
                    {
                        couPanel = new CountriesPanel();
                        couPanel.Parent = panel1;
                        couPanel.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
                        panel1.Controls.Add(couPanel);
                    }
                    else
                    {
                        couPanel.Visible = true;
                    }
                    couPanel.Location = new Point(0, 0);
                    couPanel.Size = panel1.Size;
                    panel1.AutoScroll = false;
                    break;
                case 4:
                    if (timPanel == null)
                    {
                        timPanel = new TimezonesPanel();
                        timPanel.Parent = panel1;
                        timPanel.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
                        panel1.Controls.Add(timPanel);
                    }
                    else
                    {
                        timPanel.Visible = true;
                    }
                    timPanel.Location = new Point(0, 0);
                    timPanel.Size = panel1.Size;
                    panel1.AutoScroll = false;
                    break;
                default:
                    break;
            }

            panel1.Refresh();
        }

        private void gvButtonDispSettings_Click(object sender, EventArgs e)
        {
            SelectTab(0);
        }

        private void gvButtonEvents_Click(object sender, EventArgs e)
        {
            SelectTab(1);
        }

        private void gvButtonLocations_Click(object sender, EventArgs e)
        {
            SelectTab(2);
        }

        private void gvButtonCountries_Click(object sender, EventArgs e)
        {
            SelectTab(3);
        }

        private void gvButtonTimezones_Click(object sender, EventArgs e)
        {
            SelectTab(4);
        }

        private void FrameOrganizer_Deactivate(object sender, EventArgs e)
        {
            if (dsPanel != null)
                dsPanel.Save();
            if (timPanel != null)
                timPanel.SaveTimeZoneData();
        }

        private void FrameOrganizer_FormClosing(object sender, FormClosingEventArgs e)
        {
            sharedWindow.Hide();
            e.Cancel = true;
        }

    }
}
