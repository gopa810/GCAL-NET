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
    public partial class CAboutDlg : Form
    {
        public CAboutDlg()
        {
            InitializeComponent();

            label1.Text = GCStrings.FullVersionText;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
