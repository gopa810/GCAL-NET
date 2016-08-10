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
    public partial class DlgGetStartDate : Form
    {
        public static GregorianDateTime lastDate = new GregorianDateTime();

        public DlgGetStartDate()
        {
            InitializeComponent();
        }

        public GregorianDateTime GregorianTime
        {
            get { return startDatePanel1.GregorianTime; }
            set { startDatePanel1.GregorianTime = value; }
        }

        public GCEarthData EarthLocation
        {
            get { return startDatePanel1.EarthLocation; }
            set { startDatePanel1.EarthLocation = value; }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (!startDatePanel1.CorrectDates)
            {
                MessageBox.Show("Year must be between 1500 and 3999 and Gaurabda year must be between 0 and 2500.", "Wrong values", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            else
            {
                this.DialogResult = DialogResult.Yes;
            }
        }
    }
}
