using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Windows.Forms;

using GCAL.Base;

namespace GCAL
{
    public partial class FrameSearch : Form
    {
        bool stopSearching = false;
        bool stopped = false;
        string currentText = string.Empty;
        string newText = null;
        bool finished = true;
        bool resultsChanged = false;
        int progress = 0;
        public CLocationRef EarthLocation = null;
        public StringBuilder results = new StringBuilder();

        public FrameSearch()
        {
            InitializeComponent();

            EarthLocation = GCGlobal.myLocation;

            backSearch.RunWorkerAsync();

            labelLocationInfo.Text = EarthLocation.GetFullName();

            timer1.Start();
        }

        private static FrameSearch sharedSearchWindow = null;

        public static void ShowFrame()
        {
            if (sharedSearchWindow != null)
            {
                sharedSearchWindow.Show();
                sharedSearchWindow.BringToFront();
            }
            else
            {
                sharedSearchWindow = new FrameSearch();
                sharedSearchWindow.Show();
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            if (newText == null)
                newText = textBox1.Text;
            else
            {
                if (newText.Equals(textBox1.Text))
                    return;

                lock (newText)
                {
                    newText = textBox1.Text;
                }
            }
        }

        private void backSearch_DoWork(object sender, DoWorkEventArgs e)
        {
            while (true)
            {
                if (stopSearching)
                    break;

                if (newText != null)
                {
                    lock (newText)
                    {
                        lock (currentText)
                        {
                            Debugger.Log(0, "", "Set current text to " + newText + "\n");
                            currentText = newText;
                            newText = null;
                            lock (results)
                            {
                                results.Clear();
                                finished = false;
                            }
                        }
                    }
                }

                if (currentText.Length > 0)
                {
                    doSearch();
                }

            }

            stopped = true;
        }

        /// <summary>
        /// If newText is changed during the processing of this method
        /// then running of this method is aborted and new search starts in backSearch_DoWork
        /// by assigning newText value into currentText
        /// </summary>
        private void doSearch()
        {
            TResultCalendar cal = new TResultCalendar();
            string dayText;
            cal.CalculateCalendar(EarthLocation, new GregorianDateTime(), 700);

            lock (results)
            {
                finished = false;
                results.Clear();
            }

            for (int i = 0; i < cal.m_PureCount; i++)
            {
                VAISNAVADAY vd = cal.GetDay(i);

                dayText = TResultCalendar.formatPlainTextDay(vd);
                if (dayText.IndexOf(currentText, 0, StringComparison.CurrentCultureIgnoreCase) >= 0)
                {
                    lock (results)
                    {
                        results.AppendLine();
                        results.AppendLine(dayText);
                        resultsChanged = true;
                    }
                }
                progress = i;
                if (newText != null)
                    break;
                if (stopSearching)
                    break;

            }

            finished = true;
        }

        private void backSearch_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {

        }

        private void FrameSearch_FormClosing(object sender, FormClosingEventArgs e)
        {
            stopSearching = true;
            int a = 0;

            FrameSearch.sharedSearchWindow = null;

            timer1.Stop();

            while (stopped == false && a < 100)
            {
                System.Threading.Thread.Sleep(100);
                a++;
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (finished)
            {
                if (progressBar1.Visible)
                    progressBar1.Visible = false;
            }
            else
            {
                if (!progressBar1.Visible)
                    progressBar1.Visible = true;
                progressBar1.Value = progress;
            } 
            
            if (resultsChanged || finished)
            {
                richTextBox1.Text = results.ToString();
                richTextBox1.Refresh();
                resultsChanged = false;
                finished = false;
            }


        }
    }
}
