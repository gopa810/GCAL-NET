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
    public partial class EventsPanel : UserControl
    {
        public bool[] b_masa;
        public bool[] b_class;
        public bool updatingList = false;
        public bool changingFilter = false;

        public EventsPanel()
        {
            InitializeComponent();
            int i;
            b_masa = new bool[13];
            b_class = new bool[GCEventGroup.Groups.Length + 1];

            b_masa[0] = true;
            b_class[0] = true;

            m_wndClass.Items.Add("<All Groups>");
            for (i = 0; i < GCEventGroup.Groups.Length; i++)
            {
                m_wndClass.Items.Add(GCEventGroup.Groups[i]);
                b_class[i + 1] = true;
            }

            m_wndMasa.Items.Add("<All Masas>");
            for (i = 0; i < 12; i++)
            {
                m_wndMasa.Items.Add(GCMasa.GetName(i));
                b_masa[i + 1] = true;
            }

            m_wndClass.SelectedIndex = 0;
            m_wndMasa.SelectedIndex = 0;


            FillListView();
        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        public void FillListView()
        {
            GCCalendarEvent p;
            ListViewItem lvi;
            m_wndList.BeginUpdate();
            updatingList = true;
            m_wndList.Items.Clear();

            for (int n = 0; n < GCCalendarEventList.Count(); n++)
            {
                p = GCCalendarEventList.EventAtIndex(n);
                if (p.nMasa >= 0 && p.nMasa < 12 && p.nClass >= 0 && p.nClass < 10 && b_class[p.nClass] && b_masa[p.nMasa] && p.nDeleted == 0)
                {
                    lvi = m_wndList.Items.Add(ListItemFromEvent(p));
                    lvi.Tag = p;
                    lvi.Checked = (p.nVisible != 0);
                }
            }
            updatingList = false;
            m_wndList.EndUpdate();
        }

        private ListViewItem ListItemFromEvent(GCCalendarEvent p)
        {
            ListViewItem lvi = new ListViewItem(p.strText);
            string time = "";

            lvi.SubItems.Add(GCEventGroup.Groups[p.nClass]);
            time = string.Format("{0} - {1} - {2}",
                GCMasa.GetName(p.nMasa),
                GCPaksa.GetName(p.nTithi / 15),
                GCTithi.GetName(p.nTithi % 15)
                );
            lvi.SubItems.Add(time);
            lvi.SubItems.Add(GCStrings.GetFastingName(p.nFastType + 0x200));
            lvi.SubItems.Add(p.strFastSubject);
            lvi.SubItems.Add(p.nStartYear > -7000 ? p.nStartYear.ToString() : "");

            return lvi;
        }

        private void m_wndClass_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (changingFilter)
                return;
            changingFilter = true;
            textBox1.Text = "";
            int si = m_wndClass.SelectedIndex;
            for (int i = 0; i < b_class.Length; i++)
            {
                b_class[i] = (si == 0) || (si == i + 1);
            }

            FillListView();
            changingFilter = false;
        }

        private void m_wndMasa_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (changingFilter)
                return;
            changingFilter = true;
            textBox1.Text = "";
            int si = m_wndMasa.SelectedIndex;
            for (int i = 0; i < b_masa.Length; i++)
            {
                b_masa[i] = (si == 0) || (si == i + 1);
            }
            FillListView();
            changingFilter = false;
        }

        private void m_wndList_ItemChecked(object sender, ItemCheckedEventArgs e)
        {
            if (updatingList)
                return;
            (e.Item.Tag as GCCalendarEvent).nVisible = e.Item.Checked ? 1 : 0;
            GCGlobal.customEventListModified = true;
        }

        private void m_wndList_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            if (updatingList)
                return;
            GCCalendarEvent ce = m_wndList.Items[e.Index].Tag as GCCalendarEvent;
            if (ce.nUsed == 0)
            {
                e.NewValue = CheckState.Checked;
            }
        }

        /// <summary>
        /// New Event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            DlgEditCustomEvent d = new DlgEditCustomEvent();

            if (d.ShowDialog() == DialogResult.OK)
            {
                GCCalendarEvent ce = d.ce;

                GCGlobal.customEventList.add(ce);
                GCGlobal.customEventListModified = true;
			    GCStrings.SetSpecFestivalName(ce.nSpec, ce.strText);
			    FillListView();
            }
        }

        /// <summary>
        /// Edit event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button2_Click(object sender, EventArgs e)
        {
            DlgEditCustomEvent d = new DlgEditCustomEvent();

            if (m_wndList.SelectedItems.Count == 0)
                return;


            GCCalendarEvent ce = m_wndList.SelectedItems[0].Tag as GCCalendarEvent;

            d.ce = ce;
            if (d.ShowDialog() == DialogResult.OK)
            {
                GCGlobal.customEventListModified = true;
                GCStrings.SetSpecFestivalName(ce.nSpec, ce.strText);
                FillListView();
            }
        }

        /// <summary>
        /// Delete Event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button3_Click(object sender, EventArgs e)
        {
            if (m_wndList.SelectedItems.Count == 0)
                return;


            GCCalendarEvent ce = m_wndList.SelectedItems[0].Tag as GCCalendarEvent;
            if (ce.nUsed == 0)
                return;

            string ask = string.Format("Do you want to remove event with title \"{0}\" ?", ce.strText);
            if (MessageBox.Show(ask, "Confirm", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                GCGlobal.customEventList.Remove(ce);
                GCGlobal.customEventListModified = true;
                FillListView();
            }
        }

        /// <summary>
        /// Export events
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button4_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "Text file (*.txt)|*.txt|XML File (*.xml)|*.xml||";
            sfd.FilterIndex = 0;
            sfd.CheckPathExists = true;
            sfd.OverwritePrompt = true;

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                GCCalendarEventList.Export(sfd.FileName, sfd.FilterIndex);
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            if (changingFilter)
                return;
            if (textBox1.Text.Length == 0)
            {
                FillListView();
                return;
            }

            changingFilter = true;
            string[] sp = textBox1.Text.Trim().ToLower().Split(' ');
            int A, B;
            ListViewItem lvi;

            m_wndList.BeginUpdate();
            updatingList = true;
            m_wndList.Items.Clear();

            foreach (GCCalendarEvent ce in GCCalendarEventList.list)
            {
                A = 0;
                B = 0;
                if (sp.Length > 0)
                {
                    foreach (string s in sp)
                    {
                        A++;
                        if (ce.strText.IndexOf(s, 0, StringComparison.CurrentCultureIgnoreCase) >= 0)
                        {
                            B++;
                        }
                    }
                }

                if (A == B)
                {
                    lvi = m_wndList.Items.Add(ListItemFromEvent(ce));
                    lvi.Tag = ce;
                    lvi.Checked = (ce.nVisible != 0);
                }

            }
            updatingList = false;
            m_wndList.EndUpdate();
            changingFilter = false;
        }
    }
}



















