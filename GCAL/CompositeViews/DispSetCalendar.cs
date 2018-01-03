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

namespace GCAL.CompositeViews
{
    public partial class DispSetCalendar : UserControl
    {
        private class CheckBoxValuePair
        {
            public CheckBox checkBox;
            public int dispValue;
            public CheckBoxValuePair(CheckBox cb, int dv)
            {
                checkBox = cb;
                dispValue = dv;
            }
        }

        private CheckBoxValuePair[] displayPairs;

        private int[] eventGroupPairs;

        public DispSetCalendar()
        {
            int i;

            InitializeComponent();


            displayPairs = new CheckBoxValuePair[] {
                new CheckBoxValuePair(checkBox1, 50),
                new CheckBoxValuePair(checkBox2, 35),
                new CheckBoxValuePair(checkBox3, 20),
                new CheckBoxValuePair(checkBox4, 21),
                new CheckBoxValuePair(checkBox5, 8),
                new CheckBoxValuePair(checkBox6, 7),
                new CheckBoxValuePair(checkBox7, 11),
                new CheckBoxValuePair(checkBox8, 0),
                new CheckBoxValuePair(checkBox9, 1),
                new CheckBoxValuePair(checkBox10, 12),
                new CheckBoxValuePair(checkBox11, 42),
                new CheckBoxValuePair(checkBox12, 17),
                new CheckBoxValuePair(checkBox13, 16),
                new CheckBoxValuePair(checkBox14, 39),
                new CheckBoxValuePair(checkBox15, 36),
                new CheckBoxValuePair(checkBox16, 37),
                new CheckBoxValuePair(checkBox17, 38),
                new CheckBoxValuePair(checkBox18, 41),
                new CheckBoxValuePair(checkBox20, GCDS.CAL_COL_SUNRISE),
                new CheckBoxValuePair(checkBox21, GCDS.CAL_COL_NOON),
                new CheckBoxValuePair(checkBox22, GCDS.CAL_COL_SUNSET),
                new CheckBoxValuePair(checkBox27, GCDS.CAL_SUN_RISE),
                new CheckBoxValuePair(checkBox28, GCDS.CAL_SUN_SANDHYA),
                new CheckBoxValuePair(checkBox29, GCDS.CAL_BRAHMA_MUHURTA),
                new CheckBoxValuePair(checkBox30, GCDS.CAL_SUN_LONG),
                new CheckBoxValuePair(checkBox31, GCDS.CAL_MOON_RISE),
                new CheckBoxValuePair(checkBox32, GCDS.CAL_MOON_SET),
                new CheckBoxValuePair(checkBox33, GCDS.CAL_MOON_LONG),
                new CheckBoxValuePair(checkBox19, GCDS.CAL_COREEVENTS),
            };


            comboBox4.SelectedIndex = GCDisplaySettings.Current.getValue(GCDS.CAL_HEADER_MASA);

            foreach (CheckBoxValuePair cvp in displayPairs)
            {
                cvp.checkBox.Checked = (GCDisplaySettings.Current.getValue(cvp.dispValue) != 0);
            }
        }

        public void Save()
        {
            int i;


            GCDisplaySettings.Current.setValue(GCDS.CAL_HEADER_MASA, comboBox4.SelectedIndex);

            foreach (CheckBoxValuePair cvp in displayPairs)
            {
                GCDisplaySettings.Current.setBoolValue(cvp.dispValue, cvp.checkBox.Checked);
            }

        }
    }

    /// <summary>
    /// Controller for DispSetCalendar
    /// </summary>
    public class DispSetCalendarDelegate : GVCore
    {
        public DispSetCalendarDelegate(DispSetCalendar v)
        {
            View = v;
        }

        public override Base.Scripting.GSCore ExecuteMessage(string token, Base.Scripting.GSCoreCollection args)
        {
            if (token.Equals(GVControlContainer.MsgViewWillHide))
            {
                (View as DispSetCalendar).Save();
                return GVCore.Void;
            }
            else
                return base.ExecuteMessage(token, args);
        }
    }

}
