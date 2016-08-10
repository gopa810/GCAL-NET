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
    public partial class DlgChooseLocation : Form
    {
        public DlgChooseLocation()
        {
            InitializeComponent();
        }

        public CLocation SelectedLocation
        {
            get
            {
                return chooseLocationPanel1.SelectedLocation;
            }
        }
    }
}
