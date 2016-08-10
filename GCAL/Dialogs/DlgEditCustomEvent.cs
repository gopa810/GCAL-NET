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
    public partial class DlgEditCustomEvent : Form
    {
        private GCCalendarEvent cepriv = null;

        public GCCalendarEvent ce
        {
            get
            {
                return cepriv;
            }
            set
            {
                cepriv = value;

                if (ce != null)
                {
                    textBox1.Text = ce.strText;
                    textBox2.Text = ce.strFastSubject;
                    textBox3.Text = ce.nStartYear.ToString();

                    comboBox1.SelectedIndex = ce.nClass;
                    comboBox2.SelectedIndex = ce.nTithi;
                    comboBox3.SelectedIndex = ce.nMasa;
                    comboBox4.SelectedIndex = ce.nFastType;
                }
                else
                {
                    textBox1.Text = "<new event>";
                    textBox2.Text = "";
                    textBox3.Text = "-10000";

                    comboBox1.SelectedIndex = 6;
                    comboBox2.SelectedIndex = 0;
                    comboBox3.SelectedIndex = 0;
                    comboBox4.SelectedIndex = 0;
                }
            }
        }

        public DlgEditCustomEvent()
        {
            InitializeComponent();
            int i;
            for (i = 0; i < GCEventGroup.Groups.Length; i++)
            {
                comboBox1.Items.Add(GCEventGroup.Groups[i]);
            }

            for (i = 0; i < 30; i++)
            {
                comboBox2.Items.Add(string.Format("{0} - {1}", GCTithi.GetName(i), GCPaksa.GetName(i / 15)));
            }

            for (i = 0; i < 12; i++)
            {
                comboBox3.Items.Add(GCMasa.GetName(i));
            }

            for (i = 0; i < 5; i++)
            {
                comboBox4.Items.Add(GCStrings.GetFastingName(i + 0x200) ?? "no fast");
            }

        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (ce == null)
                ce = new GCCalendarEvent();

            ce.nClass = comboBox1.SelectedIndex;
            ce.nTithi = comboBox2.SelectedIndex;
            ce.nMasa = comboBox3.SelectedIndex;
            ce.nFastType = comboBox4.SelectedIndex;
            ce.strText = textBox1.Text;
            ce.strFastSubject = textBox2.Text;
            if (!int.TryParse(textBox3.Text, out ce.nStartYear))
                ce.nStartYear = -10000;
        }
    }
}
