using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace GCAL
{
    public partial class DlgGetMonthRange : Form
    {
        public DlgGetMonthRange()
        {
            InitializeComponent();
        }

        public int StartYear
        {
            get
            {
                return Convert.ToInt32(numericUpDown1.Value);
            }
            set
            {
                numericUpDown1.Value = value;
            }
        }

        public int NumberMonths
        {
            get
            {
                return Convert.ToInt32(numericUpDown2.Value);
            }
            set
            {
                numericUpDown2.Value = value;
            }
        }
    }
}
