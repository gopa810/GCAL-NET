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
    public partial class DlgGetDateTime : Form
    {
        public DlgGetDateTime()
        {
            InitializeComponent();
        }

        public DateTimePanel Panel
        {
            get { return dateTimePanel1; }
        }
    }
}
