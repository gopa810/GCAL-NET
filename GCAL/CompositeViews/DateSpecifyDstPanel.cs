using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using GCAL.Base;

namespace GCAL.CompositeViews
{
    public partial class DateSpecifyDstPanel : UserControl
    {
        public UInt32 Value
        {
            get
            {
                int[] a = new int[4];
                int val = 0;

                a[0] = comboBoxMonth.SelectedIndex + 1;
                a[1] = comboBoxType.SelectedIndex;
                if (a[1] == 0)
                {
                    a[3] = comboBoxDayOfWeek.SelectedIndex;
                    a[2] = comboBoxWeekOfMonth.SelectedIndex + 1;
                }
                else
                {
                    a[3] = comboBoxDayOfMonth.SelectedIndex + 1;
                }

                val = (a[0] << 12) | (a[1] << 10) | (a[2] << 4) | a[3];
                return (UInt32)val;
            }
            set
            {
                UInt32[] a = new UInt32[4];
                UInt32 val = value;
                a[3] = (val & 0xf);
                val >>= 4;
                a[2] = (val & 0x3f);
                val >>= 6;
                a[1] = (val & 0x3);
                val >>= 2;
                a[0] = (val & 0xf);

                //Debugger.Log(0, "", string.Format("Values: {0},{1},{2},{3}\n", a[0], a[1], a[2], a[3]));

                if (a[0] < 12)
                    comboBoxMonth.SelectedIndex = (int)a[0] - 1;
                if (a[1] < 2)
                    comboBoxType.SelectedIndex = (int)a[1];
                if (a[1] == 0)
                {
                    comboBoxDayOfWeek.SelectedIndex = (int)a[3];
                    comboBoxWeekOfMonth.SelectedIndex = (int)a[2] - 1;
                    comboBoxDayOfMonth.SelectedIndex = -1;
                }
                else if (a[1] == 1)
                {
                    comboBoxDayOfWeek.SelectedIndex = -1;
                    comboBoxWeekOfMonth.SelectedIndex = -1;
                    comboBoxDayOfMonth.SelectedIndex = (int)a[3] - 1;
                }
            }
        }

        public DateSpecifyDstPanel()
        {
            InitializeComponent();
            int i;

            for (i = 0; i < 7; i++)
            {
                comboBoxDayOfWeek.Items.Add(GCCalendar.GetWeekdayName(i));
            }

            for (i = 1; i < 32; i++)
            {
                comboBoxDayOfMonth.Items.Add(i.ToString());
            }

            for (i = 1; i < 13; i++)
            {
                comboBoxMonth.Items.Add(GregorianDateTime.GetMonthName(i));
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            bool en1 = (comboBoxType.SelectedIndex == 0);

            label2.Enabled = en1;
            label3.Enabled = en1;
            comboBoxDayOfWeek.Enabled = en1;
            comboBoxWeekOfMonth.Enabled = en1;

            label5.Enabled = !en1;
            comboBoxDayOfMonth.Enabled = !en1;
        }
    }
}
