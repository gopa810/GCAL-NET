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
    public partial class DlgSetMyLocation : Form
    {
        public DlgSetMyLocation()
        {
            InitializeComponent();

            locationEnterPanel1.LocationRef = GCGlobal.myLocation;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            GCGlobal.myLocation = locationEnterPanel1.LocationRef;
        }
    }
}
