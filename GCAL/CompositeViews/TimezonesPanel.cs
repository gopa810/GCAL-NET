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
    public partial class TimezonesPanel : UserControl
    {
        public TTimeZone SelectedTimeZone { get; set; }

        public TimezonesPanel()
        {
            InitializeComponent();


            UpdateTimezoneList();

        }


        public void UpdateTimezoneList()
        {
            listView1.BeginUpdate();
            listView1.Items.Clear();
            foreach (TTimeZone timezone in TTimeZone.gzone)
            {
                ListViewItem lvi = new ListViewItem(timezone.name);
                lvi.SubItems.Add(TTimeZone.GetTimeZoneOffsetText(timezone.OffsetMinutes / 60.0));
                lvi.Tag = timezone;
                listView1.Items.Add(lvi);
            }
            listView1.EndUpdate();
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 0)
                return;

            if (SelectedTimeZone != null)
            {
                SaveTimeZoneData();
            }

            TTimeZone tz = listView1.SelectedItems[0].Tag as TTimeZone;
            if (tz == null)
                return;
            SelectedTimeZone = tz;

            textBox1.Text = tz.name;
            offsetNoDST.ValueMinutes = tz.OffsetMinutes;
            if (tz.val == 0)
            {
                checkBox1.Checked = false;
                offsetDST.ValueMinutes = 0;
                dstStartPanel.Value = 0;
                dstEndPanel.Value = 0;
            }
            else
            {
                checkBox1.Checked = true;
                offsetDST.ValueMinutes = tz.OffsetMinutes + tz.BiasMinutes;
                dstStartPanel.Value = (UInt16)((tz.val & 0xffff0000) >> 16);
                dstEndPanel.Value = (UInt16)(tz.val & 0xffff);
            }
        }

        public void SaveTimeZoneData()
        {
            bool changed = false;
            if (SelectedTimeZone != null)
            {
                if (!SelectedTimeZone.name.Equals(textBox1.Text))
                {
                    SelectedTimeZone.name = textBox1.Text;
                    changed = true;
                }
                if (SelectedTimeZone.OffsetMinutes != offsetNoDST.ValueMinutes)
                {
                    SelectedTimeZone.OffsetMinutes = offsetNoDST.ValueMinutes;
                    changed = true;
                }
                if (SelectedTimeZone.BiasMinutes != offsetDST.ValueMinutes - offsetNoDST.ValueMinutes)
                {
                    SelectedTimeZone.BiasMinutes = offsetDST.ValueMinutes - offsetNoDST.ValueMinutes;
                    changed = true;
                }
                if (checkBox1.Checked)
                {
                    UInt32 val = (dstStartPanel.Value << 16) | dstEndPanel.Value;
                    if (SelectedTimeZone.val != val)
                    {
                        SelectedTimeZone.val = val;
                        changed = true;
                    }
                }
                else
                {
                    if (SelectedTimeZone.val != 0)
                    {
                        SelectedTimeZone.val = 0;
                        changed = true;
                    }
                }
            }

            if (changed)
                TTimeZone.Modified = true;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "Text Files (*.txt)|*.txt||";
            sfd.DefaultExt = ".txt";

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                TTimeZone.SaveFile(sfd.FileName);
            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            panel2.Visible = checkBox1.Checked;
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            if (textBox2.Text.Trim().Length == 0)
            {
                UpdateTimezoneList();
                return;
            }

            string[] ps = textBox2.Text.Trim().ToLower().Split(' ');
            int A, B;

            listView1.BeginUpdate();
            listView1.Items.Clear();
            foreach (TTimeZone timezone in TTimeZone.gzone)
            {
                A = B = 0;
                foreach (string s in ps)
                {
                    A++;
                    if (timezone.name.IndexOf(s, 0, StringComparison.CurrentCultureIgnoreCase) >= 0)
                        B++;
                }
                if (A == B)
                {
                    ListViewItem lvi = new ListViewItem(timezone.name);
                    lvi.SubItems.Add(TTimeZone.GetTimeZoneOffsetText(timezone.OffsetMinutes / 60.0));
                    lvi.Tag = timezone;
                    listView1.Items.Add(lvi);
                }
            }
            listView1.EndUpdate();
        }


    }
}
