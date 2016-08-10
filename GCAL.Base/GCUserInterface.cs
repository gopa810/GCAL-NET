using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace GCAL.Base
{
    public class GCUserInterface
    {
        public static int CalculateCalendar(TResultCalendar daybuff, CLocationRef loc, GregorianDateTime date, int nDaysCount)
        {
            bool bCalcMoon = (GCDisplaySettings.getValue(4) > 0 || GCDisplaySettings.getValue(5) > 0);

            //GCUserInterface.CreateProgressWindow();

            if (daybuff.CalculateCalendar(loc, date, nDaysCount) == 0)
                return 0;

            //GCUserInterface.CloseProgressWindow();

            return 1;
        }

        public static void CreateProgressWindow()
        {
            if (dcp != null)
            {
                dcp.ShowWindow();
            }
        }

        public static int SetProgressWindowRange(int nMin, int nMax)
        {
            if (dcp != null)
            {
                return dcp.SetRange(nMin, nMax);
            }
            return 0;
        }

        public static void SetProgressWindowPos(double nPos)
        {
            if (dcp != null)
                dcp.SetPos(nPos);
        }

        public static void CloseProgressWindow()
        {
            if (dcp != null)
                dcp.CloseWindow();
        }

        public static void ShowHelp(string pszFile)
        {
            if (windowController != null)
                windowController.ShowHelp(pszFile);
        }

        public static IMainWindow windowController;
        public static ICalculationProgressWindow dcp;
        public static int dstSelectionMethod = 2;
        private static int _ShowMode = 1;
        public static int ShowMode
        {
            get {
                return _ShowMode; 
            }
            set { 
                _ShowMode = value; 
            }
        }
    }

    public interface ICalculationProgressWindow
    {
        void ShowWindow();
        int SetRange(int a, int b);
        int SetPos(double c);
        int CloseWindow();
    }

    public interface IMainWindow
    {
        Rectangle GetMainRectangle();
        void SetMainRectangle(Rectangle rc);
        void ShowHelp(string file);
        int GetShowMode();
        void SetShowMode(int i);
        void ShowTipAtStartup();
    }
}
