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

            eventGroupPairs = new int[] {
                GCDS.CAL_FEST_0,
                GCDS.CAL_FEST_1,
                GCDS.CAL_FEST_2,
                GCDS.CAL_FEST_3,
                GCDS.CAL_FEST_4,
                GCDS.CAL_FEST_5,
                GCDS.CAL_FEST_6
            };

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
                new CheckBoxValuePair(checkBox27, GCDS.CAL_SUN_RISE),
                new CheckBoxValuePair(checkBox28, GCDS.CAL_SUN_NOON),
                new CheckBoxValuePair(checkBox29, GCDS.CAL_SUN_SET),
                new CheckBoxValuePair(checkBox30, GCDS.CAL_SUN_LONG),
                new CheckBoxValuePair(checkBox31, GCDS.CAL_MOON_RISE),
                new CheckBoxValuePair(checkBox32, GCDS.CAL_MOON_SET),
                new CheckBoxValuePair(checkBox33, GCDS.CAL_MOON_LONG),
            };


            comboBox4.SelectedIndex = GCDisplaySettings.getValue(GCDS.CAL_HEADER_MASA);

            comboBox5.SelectedIndex = GCDisplaySettings.getValue(GCDS.CATURMASYA_SYSTEM);

            foreach(GCFestivalBook fb in GCFestivalBookCollection.Books)
            {
                checkedListBox1.Items.Add( fb, fb.Visible);
            }

            foreach (CheckBoxValuePair cvp in displayPairs)
            {
                cvp.checkBox.Checked = (GCDisplaySettings.getValue(cvp.dispValue) != 0);
            }
        }

        public void Save()
        {
            int i;


            GCDisplaySettings.setValue(GCDS.CAL_HEADER_MASA, comboBox4.SelectedIndex);

            GCDisplaySettings.setValue(GCDS.CATURMASYA_SYSTEM, comboBox5.SelectedIndex);
            GCDisplaySettings.setBoolValue(GCDS.CATURMASYA_PURNIMA, comboBox5.SelectedIndex == 0);
            GCDisplaySettings.setBoolValue(GCDS.CATURMASYA_PRATIPAT, comboBox5.SelectedIndex == 1);
            GCDisplaySettings.setBoolValue(GCDS.CATURMASYA_EKADASI, comboBox5.SelectedIndex == 2);

            for (i = 0; i < eventGroupPairs.Length; i++)
            {
                GCDisplaySettings.setBoolValue(eventGroupPairs[i], 
                    checkedListBox1.GetItemChecked(i));
            }

            foreach (CheckBoxValuePair cvp in displayPairs)
            {
                GCDisplaySettings.setBoolValue(cvp.dispValue, cvp.checkBox.Checked);
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
