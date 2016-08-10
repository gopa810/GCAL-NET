using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using GCAL.CompositeViews;
using GCAL.Base;

namespace GCAL
{
    public partial class DlgRatedEventSelection : Form
    {
        public class RatedEventNode
        {
            public string Title;
            public string Key;
            public GCRatedEvent EventRef;

            private List<RatedEventNode> Subnodes = null;


            public RatedEventNode()
            {
            }

            public RatedEventNode(string title)
            {
                Title = title;
                Key = null;
                EventRef = null;
            }

            public RatedEventNode(string title, string key)
            {
                Title = title;
                Key = key;
                if (key != null)
                {
                    EventRef = GCRatedEventsList.EventWithKey(key);
                    if (EventRef != null)
                        EventRef.Title = title;
                }
            }

            public List<RatedEventNode> List
            {
                get
                {
                    return Subnodes;
                }
            }

            public RatedEventNode this[int i]
            {
                get
                {
                    if (Subnodes == null || i < 0 || i >= Subnodes.Count)
                        return null;
                    return Subnodes[i];
                }
            }

            public int Count
            {
                get
                {
                    return Subnodes == null ? 0 : Subnodes.Count;
                }
            }

            public RatedEventNode Add(RatedEventNode re)
            {
                if (Subnodes == null)
                    Subnodes = new List<RatedEventNode>();
                Subnodes.Add(re);
                return re;
            }
        }

        public static List<RatedEventNode> EventsList = new List<RatedEventNode>();

        public DlgRatedEventSelection()
        {
            InitializeComponent();

            panelDetails.Location = new Point(0, 0);
            panelDisabled.Location = new Point(0, 0);
            panelDetails.Size = panel1.Size;
            panelDisabled.Size = panel1.Size;
            panelDetails.Visible = false;
            panelDisabled.Visible = false;

            if (EventsList.Count == 0)
            {
                InitEventsList();
            }

            InitEventsTree();

            if (GCGlobal.dialogLastRatedSpec != null && GCRatedEventsList.FileExists(GCGlobal.dialogLastRatedSpec))
            {
//                GCRatedEventsList.LoadFile(GCGlobal.dialogLastRatedSpec);
            }
            RefreshTextInfo();
            RefreshNodesAppearance();
        }

        public void InitEventsList()
        {
            RatedEventNode re, re1, re2;

            re = new RatedEventNode("Sun Events");
            EventsList.Add(re);
            re1 = re.Add(new RatedEventNode("Day", "sun.day"));
            re2 = re1.Add(new RatedEventNode("Morning", "sun.morning"));
            re2 = re1.Add(new RatedEventNode("Afternoon", "sun.afternoon"));

            re1 = re.Add(new RatedEventNode("Night", "sun.night"));
            re2 = re1.Add(new RatedEventNode("Before midnight", "sun.beforemidn"));
            re2 = re1.Add(new RatedEventNode("After midnight", "sun.aftermidn"));

            re1 = re.Add(new RatedEventNode("Sandhyas"));
            for(int i = 0; i < 4; i++)
            {
                re2 = re1.Add(new RatedEventNode(GCStrings.GetSandhyaName(i) + " sandhya", "sun.sand." + i));
            }

            re = new RatedEventNode("Day of a Week");
            EventsList.Add(re);
            for (int i = 0; i < 7; i++)
            {
                re1 = re.Add(new RatedEventNode(GCCalendar.GetWeekdayName(i), "weekday." + i));
            }

            re = new RatedEventNode("Naksatras");
            EventsList.Add(re);
            for (int i = 0; i < 27; i++)
            {
                re1 = re.Add(new RatedEventNode(GCNaksatra.GetName(i), "naks." + i));
                for (int j = 0; j < 4; j++)
                {
                    re2 = re1.Add(new RatedEventNode(GCNaksatra.GetPadaText(j), "naks." + i + "." + j));
                }
            }

            re = new RatedEventNode("Tithis");
            EventsList.Add(re);
            for (int i = 0; i < 14; i++)
            {
                re1 = re.Add(new RatedEventNode(GCTithi.GetName(i), "tithi." + i));
                for (int j = 0; j < 2; j++)
                {
                    re2 = re1.Add(new RatedEventNode(GCPaksa.GetName(j) + " Paksa", "tithi." + i + "." + j));
                }
            }
            re1 = re.Add(new RatedEventNode(GCTithi.GetName(14) + " / " + GCTithi.GetName(29), "tithi.14"));
            re2 = re1.Add(new RatedEventNode(GCTithi.GetName(14), "tithi.14.0"));
            re2 = re1.Add(new RatedEventNode(GCTithi.GetName(29), "tithi.14.1"));

            re = new RatedEventNode("Yogas");
            EventsList.Add(re);
            for (int i = 0; i < 27; i++)
            {
                re1 = re.Add(new RatedEventNode(GCYoga.GetName(i), "yoga." + i));
            }

            re = new RatedEventNode("Muhurtas");
            EventsList.Add(re);
            for (int i = 0; i < 30; i++)
            {
                re1 = re.Add(new RatedEventNode(GCStrings.GetMuhurtaName(i), "muhurta." + i));
            }

            re = new RatedEventNode("Rasi of the Sun");
            EventsList.Add(re);
            for (int i = 0; i < 12; i++)
            {
                re1 = re.Add(new RatedEventNode(string.Format("{0} ({1})",
                    GCRasi.GetName(i),
                    GCRasi.GetNameEn(i)), "sun.rasi." + i));
            }

            re = new RatedEventNode("Rasi of the Moon");
            EventsList.Add(re);
            for (int i = 0; i < 12; i++)
            {
                re1 = re.Add(new RatedEventNode(string.Format("{0} ({1})",
                    GCRasi.GetName(i),
                    GCRasi.GetNameEn(i)), "moon.rasi." + i));
            }

            re = new RatedEventNode("Ascendent");
            EventsList.Add(re);
            for (int i = 0; i < 12; i++)
            {
                re1 = re.Add(new RatedEventNode(string.Format("{0} ({1})",
                    GCRasi.GetName(i),
                    GCRasi.GetNameEn(i)), "ascendent." + i));
            }

            re = new RatedEventNode("Special Intervals");
            EventsList.Add(re);
            re1 = re.Add(new RatedEventNode("Rahu Kala", "spec.rahu"));
            re1 = re.Add(new RatedEventNode("Yamaghanti", "spec.yama"));
            re1 = re.Add(new RatedEventNode("Guli Kala", "spec.guli"));
            re1 = re.Add(new RatedEventNode("Vishagati", "spec.vish"));
            re1 = re.Add(new RatedEventNode("Abhijit Muhurta", "spec.abhijit"));
        }

        public void InitEventsTree()
        {
            InsertNodesIntoTree(treeView1.Nodes, EventsList);
        }

        public void InsertNodesIntoTree(TreeNodeCollection tnc, List<RatedEventNode> rel)
        {
            foreach (RatedEventNode re in rel)
            {
                TreeNode tn = new TreeNode(re.Title);
                tn.Tag = re;
                tnc.Add(tn);
                tn.SelectedImageIndex = tn.ImageIndex = GetNodeIcon(tn);
                if (re.Count > 0)
                {
                    InsertNodesIntoTree(tn.Nodes, re.List);
                }
            }
        }

        public void RefreshNodesAppearance()
        {
            RefreshNodesApp(treeView1.Nodes);
        }

        private void RefreshNodesApp(TreeNodeCollection tnc)
        {
            int idx = 0;
            foreach (TreeNode tn in tnc)
            {
                if (!tn.IsVisible)
                    continue;

                idx = GetNodeIcon(tn);
                tn.SelectedImageIndex = tn.ImageIndex = idx;

                if (tn.Nodes.Count > 0)
                {
                    RefreshNodesApp(tn.Nodes);
                }
            }
        }

        private static int GetNodeIcon(TreeNode tn)
        {
            int idx;
            RatedEventNode rn;
            if (tn.Tag != null && tn.Tag is RatedEventNode)
                rn = tn.Tag as RatedEventNode;
            else
                rn = null;
            if (rn.EventRef == null)
                idx = 0;
            else
            {
                if (rn.EventRef.Rejected)
                    idx = 3;
                else if (rn.EventRef.Rating > 0.0)
                    idx = 1;
                else if (rn.EventRef.Rating < 0.0)
                    idx = 2;
                else
                    idx = 0;
            }
            return idx;
        }

        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            GCRatedEventsList.Clear();
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Rated Events configuration file (*.rxml)|*.rxml||";

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                if (GCRatedEventsList.FileName != null
                    && GCRatedEventsList.FileExists(GCRatedEventsList.FileName))
                {
                    GCRatedEventsList.SaveFile(GCRatedEventsList.FileName);
                }
                GCRatedEventsList.LoadFile(ofd.FileName);
                RefreshNodesAppearance();
                RefreshTextInfo();
            }
        }

        private void RefreshTextInfo()
        {
            textBox1.Text = GCRatedEventsList.Name;
            textBox2.Text = GCRatedEventsList.Revision.ToString();
            checkBox2.Checked = GCRatedEventsList.ShowOnlyPositive;
            checkBox3.Checked = GCRatedEventsList.ShowOnlyAboveLevel;
            numericUpDown2.Value = Convert.ToDecimal(GCRatedEventsList.AboveLevelValue);
            checkBox4.Checked = GCRatedEventsList.ShowPeriodLongerThan;
            numericUpDown1.Value = Convert.ToDecimal(GCRatedEventsList.PeriodLongerValue);
            richTextBox1.Text = GCRatedEventsList.Description;
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string fileName = GCRatedEventsList.FileName;

            if (fileName == null || fileName.Length == 0)
            {
                SaveFileDialog sfd = new SaveFileDialog();
                sfd.DefaultExt = ".rxml";
                sfd.Filter = "Rated Events configuration file (*.rxml)|*.rxml||";

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    fileName = sfd.FileName;
                }
            }

            if (fileName != null)
            {
                SynchroShowValues();
                GCRatedEventsList.Revision++;
                GCRatedEventsList.Name = textBox1.Text;
                textBox2.Text = GCRatedEventsList.Revision.ToString();
                GCRatedEventsList.SaveFile(fileName);
                GCGlobal.dialogLastRatedSpec = fileName;
            }

        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.DefaultExt = ".rxml";
            sfd.Filter = "Rated Events configuration file (*.rxml)|*.rxml||";

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                GCRatedEventsList.FileName = sfd.FileName;
                saveToolStripMenuItem_Click(sender, e);
            }

        }

        private void ratedEventDetailsPanel1_OnValueChanged(object sender, RatedEventDetailsPanel.RatedEventDetailsArg ev)
        {
            if (ev.Change != RatedEventDetailsPanel.RatedEventChange.Note)
            {
                RefreshNodesAppearance();
            }
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (e.Node != null && e.Node.Tag != null && e.Node.Tag is RatedEventNode)
            {
                RatedEventNode ren = e.Node.Tag as RatedEventNode;
                if (ren.EventRef == null)
                {
                    panelDisabled.Visible = false;
                    panelDetails.Visible = false;
                }
                else
                {
                    panelDisabled.Visible = false;
                    panelDetails.Visible = true;

                    panelDetails.RatedEvent = ren.EventRef;
                }
            }
            else
            {
                panelDisabled.Visible = false;
                panelDetails.Visible = false;
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            SynchroShowValues();
        }

        private void SynchroShowValues()
        {
            GCRatedEventsList.ShowOnlyPositive = checkBox2.Checked;
            GCRatedEventsList.ShowOnlyAboveLevel = checkBox3.Checked;
            GCRatedEventsList.AboveLevelValue = Convert.ToDouble(numericUpDown2.Value);
            GCRatedEventsList.ShowPeriodLongerThan = checkBox4.Checked;
            GCRatedEventsList.PeriodLongerValue = Convert.ToDouble(numericUpDown1.Value);
            GCRatedEventsList.Description = richTextBox1.Text;
            GCRatedEventsList.Name = textBox1.Text;
        }
    }
}
