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
    public partial class DisplaySettingsPanel : UserControl
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

        public DisplaySettingsPanel()
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
                new CheckBoxValuePair(checkBox19, 45),
                new CheckBoxValuePair(checkBox20, 29),
                new CheckBoxValuePair(checkBox21, 30),
                new CheckBoxValuePair(checkBox22, 31),
                new CheckBoxValuePair(checkBox23, 33),
                new CheckBoxValuePair(checkBox24, 32),
                new CheckBoxValuePair(checkBox25, 46),
                new CheckBoxValuePair(checkBox26, 47),
                new CheckBoxValuePair(checkBox27, GCDS.CAL_SUN_RISE),
                new CheckBoxValuePair(checkBox28, GCDS.CAL_SUN_NOON),
                new CheckBoxValuePair(checkBox29, GCDS.CAL_SUN_SET),
                new CheckBoxValuePair(checkBox30, GCDS.CAL_SUN_LONG),
                new CheckBoxValuePair(checkBox31, GCDS.CAL_MOON_RISE),
                new CheckBoxValuePair(checkBox32, GCDS.CAL_MOON_SET),
                new CheckBoxValuePair(checkBox33, GCDS.CAL_MOON_LONG),
                new CheckBoxValuePair(checkBox34, GCDS.APP_CHILDNAMES),
                new CheckBoxValuePair(checkBox35, GCDS.COREEVENTS_SUN),
                new CheckBoxValuePair(checkBox36, GCDS.COREEVENTS_TITHI),
                new CheckBoxValuePair(checkBox37, GCDS.COREEVENTS_NAKSATRA),
                new CheckBoxValuePair(checkBox38, GCDS.COREEVENTS_YOGA),
                new CheckBoxValuePair(checkBox39, GCDS.COREEVENTS_SANKRANTI),
                new CheckBoxValuePair(checkBox40, GCDS.COREEVENTS_CONJUNCTION),
                new CheckBoxValuePair(checkBox41, GCDS.COREEVENTS_MOON),
                new CheckBoxValuePair(checkBox42, GCDS.COREEVENTS_MOONRASI),
                new CheckBoxValuePair(checkBox43, GCDS.COREEVENTS_RAHUKALAM),
                new CheckBoxValuePair(checkBox44, GCDS.COREEVENTS_YAMAGHANTI),
                new CheckBoxValuePair(checkBox45, GCDS.COREEVENTS_GULIKALAM),
                new CheckBoxValuePair(checkBox46, GCDS.COREEVENTS_ASCENDENT),
                new CheckBoxValuePair(checkBox47, GCDS.COREEVENTS_ABHIJIT_MUHURTA),
            };

            // first day of week
            for (i = 0; i < 7; i++)
            {
                comboBox1.Items.Add(GCCalendar.GetWeekdayName(i));
            }
            comboBox1.SelectedIndex = GCDisplaySettings.getValue(GCDS.GENERAL_FIRST_DOW);

            // masa name format
            comboBox2.SelectedIndex = GCDisplaySettings.getValue(GCDS.GENERAL_MASA_FORMAT);

            // anniversary format
            comboBox3.SelectedIndex = GCDisplaySettings.getValue(GCDS.GENERAL_ANNIVERSARY_FMT);

            comboBox4.SelectedIndex = GCDisplaySettings.getValue(GCDS.CAL_HEADER_MASA);

            comboBox5.SelectedIndex = GCDisplaySettings.getValue(GCDS.CATURMASYA_SYSTEM);

            comboBox6.SelectedIndex = GCDisplaySettings.getValue(GCDS.COREEVENTS_SORT);

            for (i = 0; i < eventGroupPairs.Length; i++)
            {
                checkedListBox1.Items.Add( GCEventGroup.Groups[i], GCDisplaySettings.getBoolValue(eventGroupPairs[i]));
            }

            foreach (CheckBoxValuePair cvp in displayPairs)
            {
                cvp.checkBox.Checked = (GCDisplaySettings.getValue(cvp.dispValue) != 0);
            }
        }

        public void Save()
        {
            int i;

            GCDisplaySettings.setValue(GCDS.GENERAL_FIRST_DOW, comboBox1.SelectedIndex);

            // masa name format
            GCDisplaySettings.setValue(GCDS.GENERAL_MASA_FORMAT, comboBox2.SelectedIndex);

            // anniversary format
            GCDisplaySettings.setValue(GCDS.GENERAL_ANNIVERSARY_FMT, comboBox3.SelectedIndex);

            GCDisplaySettings.setValue(GCDS.CAL_HEADER_MASA, comboBox4.SelectedIndex);

            GCDisplaySettings.setValue(GCDS.CATURMASYA_SYSTEM, comboBox5.SelectedIndex);
            GCDisplaySettings.setBoolValue(GCDS.CATURMASYA_PURNIMA, comboBox5.SelectedIndex == 0);
            GCDisplaySettings.setBoolValue(GCDS.CATURMASYA_PRATIPAT, comboBox5.SelectedIndex == 1);
            GCDisplaySettings.setBoolValue(GCDS.CATURMASYA_EKADASI, comboBox5.SelectedIndex == 2);

            GCDisplaySettings.setValue(GCDS.COREEVENTS_SORT, comboBox6.SelectedIndex);

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
}
