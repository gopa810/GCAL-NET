using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GCAL.Base;
using System.Drawing;

namespace GCAL
{
    public sealed class MainFrameContentType
    {
        public const int MW_MODE_CAL = 1;
        public const int MW_MODE_EVENTS = 2;
        public const int MW_MODE_MASALIST = 3;
        public const int MW_MODE_RATEDEVENTS = 4;
        public const int MW_MODE_APPDAY = 6;
        public const int MW_MODE_TODAY = 7;
    };

    public class FrameDelegate: IMainWindow
    {
        FrameMain frame;

        public FrameDelegate(FrameMain inframe)
        {
            frame = inframe;
        }

        public void ShowTipAtStartup()
        {
            if (Properties.Settings.Default.ShowStartupTips)
            {
                ShowTipOfTheDay();
            }

        }

        public void ShowTipOfTheDay()
        {
            CTipDlg dlg = new CTipDlg();
            dlg.Show();
        }

        public Rectangle GetMainRectangle()
        {
            return frame.Bounds;
        }

        public void SetMainRectangle(Rectangle rc)
        {
            frame.Bounds = rc;
        }

        public void ShowHelp(string file)
        {
        }

        public int GetShowMode()
        {
            return frame.m_nInfoType;
        }

        public void SetShowMode(int i)
        {
            frame.m_nInfoType = i;
        }
    }
}
