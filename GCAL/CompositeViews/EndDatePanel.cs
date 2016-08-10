using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using GCAL.Base;

namespace GCAL
{
    public partial class EndDatePanel : UserControl
    {
        private GregorianDateTime startDate;
        private GregorianDateTime endDate;

        public EndDatePanel()
        {
            InitializeComponent();
        }

        public GCEarthData EarthLocation { get; set; }

        public GregorianDateTime StartDate
        {
            get
            {
                if (startDate == null)
                {
                    startDate = new GregorianDateTime();
                }
                return startDate;
            }
            set
            {
                startDate = value;
                if (startDate != null)
                {
                    label3.Text = startDate.ToString();
                }
            }
        }

        public GregorianDateTime EndDate
        {
            get
            {
                if (endDate == null)
                {
                    endDate = new GregorianDateTime();
                }

                return endDate;
            }
            set
            {
                endDate = value;
                if (endDate != null)
                {
                    label4.Text = endDate.ToString();
                }
            }
        }

        public int Days
        {
            get
            {
                return EndDate.GetJulianInteger() - StartDate.GetJulianInteger();
            }
        }


        public int PeriodLength
        {
            get
            {
                int a, b;
                b = GCCalendar.LimitForPeriodUnit(PeriodType);
                int.TryParse(textBox1.Text, out a);
                if (a <= 0) a = 1;
                if (a > b) a = b;
                return a;
            }
            set
            {
                textBox1.Text = value.ToString();
            }
        }

        public GCCalendar.PeriodUnit PeriodType
        {
            get
            {
                if (radioButton1.Checked) return GCCalendar.PeriodUnit.Days;
                if (radioButton2.Checked) return GCCalendar.PeriodUnit.Weeks;
                if (radioButton3.Checked) return GCCalendar.PeriodUnit.Months;
                if (radioButton4.Checked) return GCCalendar.PeriodUnit.Tithis;
                if (radioButton5.Checked) return GCCalendar.PeriodUnit.Years;
                if (radioButton6.Checked) return GCCalendar.PeriodUnit.Masas;
                if (radioButton7.Checked) return GCCalendar.PeriodUnit.Gaurabda;
                return GCCalendar.PeriodUnit.Days;
            }
            set
            {
                switch(value)
                {
                    case GCCalendar.PeriodUnit.Days:
                        radioButton1.Checked = true;
                        break;
                    case GCCalendar.PeriodUnit.Weeks:
                        radioButton2.Checked = true;
                        break;
                    case GCCalendar.PeriodUnit.Months:
                        radioButton3.Checked = true;
                        break;
                    case GCCalendar.PeriodUnit.Tithis:
                        radioButton4.Checked = true;
                        break;
                    case GCCalendar.PeriodUnit.Years:
                        radioButton5.Checked = true;
                        break;
                    case GCCalendar.PeriodUnit.Masas:
                        radioButton6.Checked = true;
                        break;
                    case GCCalendar.PeriodUnit.Gaurabda:
                        radioButton7.Checked = true;
                        break;
                    default:
                        radioButton5.Checked = true;
                        break;
                }
            }
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            textBox1.Text = "1";
        }

        private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            textBox1.Text = "3";
        }

        private void linkLabel3_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            textBox1.Text = "7";
        }

        private void linkLabel4_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            textBox1.Text = "21";
        }

        private void button1_Click(object sender, EventArgs e)
        {
            int a = PeriodLength;
            if (a < GCCalendar.LimitForPeriodUnit(PeriodType))
                a++;
            textBox1.Text = a.ToString();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            int a = PeriodLength;
            if (a > 1)
                a--;
            textBox1.Text = a.ToString();
        }

        private void periodType_Changed(object sender, EventArgs e)
        {
            RecalculateEndDate();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            RecalculateEndDate();
        }

        private void RecalculateEndDate()
        {
            GregorianDateTime vcEnd;

            if (GCCalendar.CalcEndDate(EarthLocation, StartDate, out vcEnd,
                PeriodType, PeriodLength))
            {
                EndDate = vcEnd;
            }
        }


    }
}
