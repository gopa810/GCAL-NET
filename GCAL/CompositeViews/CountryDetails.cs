using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using GCAL.Base;
using GCAL.Views;
using GCAL.Base.Scripting;

namespace GCAL.CompositeViews
{
    public partial class CountryDetails : UserControl
    {
        public CountryDetailsController Controller { get; set; }
        public event TBButtonPressed OnButtonSave;

        public CountryDetails()
        {
            InitializeComponent();

            comboBox1.BeginUpdate();
            foreach (string continentName in TCountry.gcontinents)
            {
                comboBox1.Items.Add(continentName);
            }
            comboBox1.EndUpdate();
        }

        private TCountry _sc;

        public TCountry SelectedCountry
        {
            get
            {
                return _sc;
            }
            set
            {
                _sc = value;
                if (_sc != null)
                {
                    textBox1.Enabled = false;
                    textBox1.Text = _sc.abbreviatedName;
                    textBox2.Text = _sc.name;
                    SelectedContinent = _sc.continent;
                }
            }
        }

        public UInt16 SelectedContinent
        {
            get
            {
                return (UInt16)(comboBox1.SelectedIndex + 1);
            }
            set
            {
                if (value > 0)
                    comboBox1.SelectedIndex = (int)value - 1;
            }
        }


        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            bool ok = IsCodeCorrect() && IsCodeUnique();

            textBox1.BackColor = ok ? Color.LightGreen : Color.LightCoral;

            buttonSave.Enabled = ok && IsNameCorrect();
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            bool ok = IsNameCorrect();

            textBox2.BackColor = ok ? Color.LightGreen : Color.LightCoral;

            buttonSave.Enabled = IsCodeCorrect() && IsCodeUnique() && ok;

        }


        private void buttonSave_Click(object sender, EventArgs e)
        {
            if (_sc == null)
            {
                TCountry.AddCountry(textBox1.Text, textBox2.Text, SelectedContinent);
            }
            else
            {
                _sc.abbreviatedName = textBox1.Text;
                _sc.name = textBox2.Text;
                _sc.code = TCountry.CodeFromString(textBox1.Text);
                _sc.continent = SelectedContinent;
            }
            TCountry._modified = true;

            Controller.RemoveFromContainer();
            if (OnButtonSave != null)
                OnButtonSave(this, e);
        }


        private bool IsCodeCorrect()
        {
            string s = textBox1.Text;

            if (s.Length != 2)
                return false;

            if (!Char.IsLetter(s[0]) || !Char.IsLetter(s[1]))
                return false;

            return true;
        }

        private bool IsCodeUnique()
        {
            if (textBox1.Enabled == false)
                return true;

            TCountry tc = TCountry.GetCountryByAcronym(textBox1.Text);
            return tc == null;
        }

        private bool IsNameCorrect()
        {
            return textBox2.Text.Trim().Length > 0;
        }

        private void buttonBack_Click(object sender, EventArgs e)
        {
            Controller.RemoveFromContainer();
        }
    }

    public class CountryDetailsController : GVCore
    {
        public CountryDetailsController(CountryDetails v)
        {
            View = v;
            v.Controller = this;
        }
    }
}
