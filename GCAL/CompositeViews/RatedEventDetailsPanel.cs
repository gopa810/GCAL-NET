using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;


using GCAL.Base;


namespace GCAL.CompositeViews
{
    public partial class RatedEventDetailsPanel : UserControl
    {
        public enum RatedEventChange
        {
            Note,
            Rating,
            Rejected
        }

        public class RatedEventDetailsArg : EventArgs
        {
            public RatedEventChange Change { get; set; }

            public RatedEventDetailsArg(RatedEventChange change)
            {
                Change = change;
            }
        }

        private GCRatedEvent _ret;

        public delegate void ValueChangedHandler(object sender, RatedEventDetailsArg ev);
        public event ValueChangedHandler OnValueChanged;

        public RatedEventDetailsPanel()
        {
            InitializeComponent();
        }

        public GCRatedEvent RatedEvent
        {
            set
            {
                _ret = value;
                if (value != null)
                {
                    numericUpDown3.Value = Convert.ToDecimal(_ret.Rating);
                    if (_ret.Note != null)
                        richTextBox2.Text = _ret.Note;
                    else
                        richTextBox2.Text = "";
                    checkBox1.Checked = _ret.Rejected;
                }
            }
            get
            {
                return _ret;
            }
        }

        private void richTextBox2_TextChanged(object sender, EventArgs e)
        {
            if (_ret != null)
            {
                _ret.Note = richTextBox2.Text;
                if (OnValueChanged != null)
                {
                    OnValueChanged(this, new RatedEventDetailsArg(RatedEventChange.Note));
                }
            }
        }

        private void numericUpDown3_ValueChanged(object sender, EventArgs e)
        {
            if (_ret != null)
            {
                _ret.Rating = Convert.ToDouble(numericUpDown3.Value);
                if (OnValueChanged != null)
                {
                    OnValueChanged(this, new RatedEventDetailsArg(RatedEventChange.Rating));
                }
            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (_ret != null)
            {
                _ret.Rejected = checkBox1.Checked;
                if (OnValueChanged != null)
                {
                    OnValueChanged(this, new RatedEventDetailsArg(RatedEventChange.Rejected));
                }
            }
        }
    }
}
