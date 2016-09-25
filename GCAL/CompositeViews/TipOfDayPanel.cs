using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using GCAL.Base;
using GCAL.Base.Scripting;
using GCAL.Views;

namespace GCAL.CompositeViews
{
    public partial class TipOfDayPanel : UserControl
    {
        public TipOfDayPanelController Controller { get; set; }

        public TipOfDayPanel()
        {
            InitializeComponent();
        }

        private void buttonClose_Click(object sender, EventArgs e)
        {
            Controller.RemoveFromContainer();
        }

        private void buttonNext_Click(object sender, EventArgs e)
        {

        }
    }

    public class TipOfDayPanelController : GVCore
    {
        public TipOfDayPanelController(TipOfDayPanel v)
        {
            View = v;
            v.Controller = this;
        }
    }
}
