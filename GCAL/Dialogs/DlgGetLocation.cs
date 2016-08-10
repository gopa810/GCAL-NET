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
    public partial class DlgGetLocation : Form
    {
        public DlgGetLocation()
        {
            InitializeComponent();

            locationEnterPanel1.LocationRef = GCGlobal.lastLocation;
        }


        public string Title
        {
            set
            {
                label1.Text = value;
            }
            get
            {
                return label1.Text;
            }
        }

        public CLocationRef LocationRef
        {
            get
            {
                return locationEnterPanel1.LocationRef;
            }
            set
            {
                locationEnterPanel1.LocationRef = value;
            }
        }

        /// <summary>
        /// On next button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonNext_Click(object sender, EventArgs e)
        {
            GCGlobal.lastLocation = locationEnterPanel1.LocationRef;
        }
    }
}
