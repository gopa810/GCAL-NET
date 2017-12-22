using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using GCAL.Views;
using GCAL.CompositeViews;
using GCAL.Base;
using GCAL.Base.Scripting;

namespace GCAL
{
    public partial class DlgSelectLocation : Form
    {
        public GCLocation SelectedLocation = null;

        public DlgSelectLocation()
        {
            InitializeComponent();


            SelectLocationInputPanel panel1 = new SelectLocationInputPanel();
            panel1.OnLocationSelected += Panel1_OnLocationSelected;
            SelectLocationInputPanelController pc1 = new SelectLocationInputPanelController(panel1);

            pc1.ShowInContainer(gvControlContainer1, GVControlAlign.Center);
        }

        private void Panel1_OnLocationSelected(object sender, EventArgs e)
        {
            if (sender is GCLocation)
            {
                DialogResult = DialogResult.OK;
                SelectedLocation = (GCLocation)sender;
            }
            else
            {
                DialogResult = DialogResult.Cancel;
                SelectedLocation = null;
            }

            Close();
        }
    }
}
