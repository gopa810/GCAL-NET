using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GCAL.Base
{
    public class GCCalendarEventList
    {

        public static string GetEventClassText(int i)
        {
            switch (i)
            {
                case 0: return GCStrings.Localized("Appearance Days of the Lord and His Incarnations");
                case 1: return GCStrings.Localized("Events in the Pastimes of the Lord and His Associates");
                case 2: return GCStrings.Localized("Appearance and Disappearance Days of Recent Acaryas");
                case 3: return GCStrings.Localized("Appearance and Disappearance Days of Mahaprabhu's Associates and Other Acaryas");
                case 4: return GCStrings.Localized("ISKCON's Historical Events");
                case 5: return GCStrings.Localized("Bengal-specific Holidays");
                case 6: return GCStrings.Localized("My Personal Events");
                default: break;
            }
            return "";
        }

        public static void Export(string pszFile, int format)
        {
            TFile f = new TFile();
            String strc;

            switch (format)
            {
                case 1:
                    for (int nk = 0; nk < GCCalendarEventList.Count(); nk++)
                    {
                        GCCalendarEvent pce = GCCalendarEventList.EventAtIndex(nk);
                        strc = string.Format("{0}\n\t{1} Tithi,{2} Paksa,{3} Masa\n", pce.strText,
                            GCTithi.GetName(pce.nTithi), GCPaksa.GetName(pce.nTithi / 15), GCMasa.GetName(pce.nMasa));
                        f.WriteString(strc);
                        if (pce.nFastType != 0)
                        {
                            strc = string.Format("\t{0}\n", GCStrings.GetFastingName(0x200 + pce.nFastType));
                            f.WriteString(strc);
                        }
                    }

                    foreach (GCCalendarEventSankranti se in SanBasedList)
                    {
                        f.WriteString(string.Format("{0}\n\t\t{1} Sankranti, {2}\n", se.Text, GCRasi.GetName(se.Rasi), se.DayOffset));
                    }
                    break;
                case 2:
                    {
                        f.WriteString("<xml>\n");
                        for (int nk = 0; nk < GCCalendarEventList.Count(); nk++)
                        {
                            GCCalendarEvent pce = GCCalendarEventList.EventAtIndex(nk);
                            f.WriteString("\t<event>\n");
                            strc = string.Format("\t\t<name>{0}</name>\n", pce.strText);
                            f.WriteString(strc);
                            strc = string.Format("\t\t<tithi>{0}</tithi>\n", GCTithi.GetName(pce.nTithi));
                            f.WriteString(strc);
                            strc = string.Format("\t\t<paksa>{0}</paksa>\n", GCPaksa.GetName(pce.nTithi / 15));
                            f.WriteString(strc);
                            strc = string.Format("\t\t<masa>{0}</masa>\n", GCMasa.GetName(pce.nMasa));
                            f.WriteString(strc);
                            f.WriteString("\t</event>\n");
                        }

                        foreach (GCCalendarEventSankranti se in SanBasedList)
                        {
                            f.WriteString("\t<event>\n\t\t<name>");
                            f.WriteString(se.Text);
                            f.WriteString("</name>\n\t\t<sankranti>" + GCRasi.GetName(se.Rasi) + "</sankranti>\n\t\t<rel>" + se.DayOffset + "</rel>\n\t</event>\n");
                        }
                        f.WriteString("</xml>\n");
                    }
                    break;
                default:
                    break;
            }

            f.WriteFile(pszFile);

        }

        public static int OpenFile(string pszFile)
        {
            TFileRichList F = new TFileRichList();
            int tithi, masa;
            GCCalendarEvent pce;
            int nRet = -1;

            if (!TFile.FileExists(pszFile))
            {
                TFile.CreateFileFromResource("events.rl", pszFile);
            }

            if (F.ReadFile(pszFile))
            {
                GCGlobal.customEventList = new GCCalendarEventList();

                nRet++;
                while (F.ReadLine() > 0)
                {
                    if (int.Parse(F.GetTag()) == 13)
                    {
                        nRet++;
                        pce = GCGlobal.customEventList.add();
                        if (pce != null)
                        {
                            pce.strText = F.GetField(0);
                            pce.nMasa = masa = int.Parse(F.GetField(1));
                            pce.nTithi = tithi = int.Parse(F.GetField(2));
                            pce.nClass = int.Parse(F.GetField(3));
                            pce.nVisible = int.Parse(F.GetField(4));
                            pce.nFastType = int.Parse(F.GetField(5));
                            pce.strFastSubject = F.GetField(6);
                            pce.nUsed = int.Parse(F.GetField(7));
                            pce.nSpec = int.Parse(F.GetField(8));
                            string str10 = F.GetField(9);
                            if (str10.Length == 0)
                                pce.nStartYear = -10000;
                            else
                                pce.nStartYear = int.Parse(str10);

                        }

                    }
                }

            }

            InitDefaultSankrantiEvents();

            return nRet;

        }

        /// <summary>
        /// Saving file as text
        /// </summary>
        /// <param name="pszFile"></param>
        /// <returns></returns>
        public static int SaveFile(string pszFile)
        {
            if (!GCGlobal.customEventListModified)
                return 0;

            TFile csd = new TFile();
            String str;
            GCCalendarEvent pce;
            int nRet = -1;

            nRet++;
            for (int i = 0; i < list.Count(); i++)
            {
                pce = list[i];
                if (pce.nDeleted == 0)
                {
                    nRet++;
                    // write to file
                    str = string.Format("13 {0}|{1}|{2}|{3}|{4}|{5}|{6}|{7}|{8}\n", pce.strText,
                        pce.nMasa, pce.nTithi, pce.nClass, pce.nVisible,
                        pce.nFastType, pce.strFastSubject, pce.nUsed, pce.nSpec);
                    csd.WriteString(str);
                }
            }
            // close file
            csd.WriteFile(pszFile);

            return nRet;
        }

        public static int Count()
        {
            return list.Count;
        }

        public static GCCalendarEvent EventAtIndex(int index)
        {
            return list[index];
        }

        public static int SetOldStyleFasting(int bOldStyle)
        {
            GCCalendarEvent pce = null;

            int[,] locMatrix =
	        {
		        // visnu tattva and sakti tattva
		        {/*tithi*/ 28, /*masa*/  0, /*fast*/ 4, /*new fast*/ 7, /*class*/ 0},
		        {/*tithi*/ 29, /*masa*/  3, /*fast*/ 1, 7, 0},
		        {/*tithi*/ 22, /*masa*/  4, /*fast*/ 1, 0, 0},
		        {/*tithi*/ 26, /*masa*/  4, /*fast*/ 1, 7, 0},
		        {/*tithi*/ 21, /*masa*/  9, /*fast*/ 1, 7, 0},
		        {/*tithi*/ 26, /*masa*/  9, /*fast*/ 1, 7, 0},
		        {/*tithi*/ 27, /*masa*/  9, /*fast*/ 1, 7, 0},
		        {/*tithi*/  7, /*masa*/  4, /*fast*/ 5, 7, 0},
		        {/*tithi*/ 29, /*masa*/ 10, /*fast*/ 3, 7, 0},
		        {/*tithi*/ 23, /*masa*/ 11, /*fast*/ 2, 7, 0},
		        // acaryas
		        {/*tithi*/  8, /*masa*/  4, /*fast*/ 1, 0, 2},
		        {/*tithi*/ 27, /*masa*/  4, /*fast*/ 1, 0, 2},
		        {/*tithi*/ 14, /*masa*/  2, /*fast*/ 1, 0, 2},
		        {/*tithi*/ 25, /*masa*/  6, /*fast*/ 1, 0, 2},
		        {/*tithi*/ 18, /*masa*/  6, /*fast*/ 1, 0, 2},
		        {/*tithi*/  3, /*masa*/  8, /*fast*/ 1, 0, 2},
		        {/*tithi*/  4, /*masa*/ 10, /*fast*/ 1, 0, 2},
		        {-1,-1,-1,-1, -1}
	        };

            int i, idx;
            int ret = 0;
            if (bOldStyle != 0) idx = 2;
            else idx = 3;

            for (i = 0; locMatrix[i, 0] >= 0; i++)
            {
                for (int n = 0; n < list.Count(); n++)
                {
                    pce = list[n];
                    if (pce.nMasa == locMatrix[i, 1] &&
                        pce.nTithi == locMatrix[i, 0] &&
                        pce.nClass == locMatrix[i, 4])
                    {
                        if (pce.nFastType != locMatrix[i, idx])
                        {
                            ret++;
                            pce.nFastType = locMatrix[i, idx];
                        }
                        break;
                    }
                }
            }

            return ret;
        }


        public static List<GCCalendarEvent> list = new List<GCCalendarEvent>();
        public static List<GCCalendarEventSankranti> SanBasedList = new List<GCCalendarEventSankranti>();


        public static void InitDefaultSankrantiEvents()
        {
            GCCalendarEventSankranti se = new GCCalendarEventSankranti();

            SanBasedList.Clear();

            se.DayOffset = 0;
            se.Rasi = SankrantiId.MAKARA_SANKRANTI;
            se.Text = GCStrings.Localized("Ganga Sagara Mela");
            SanBasedList.Add(se);
            
            se.DayOffset = 0;
            se.Rasi = SankrantiId.MESHA_SANKRANTI;
            se.Text = GCStrings.Localized("Tulasi Jala Dan begins");
            SanBasedList.Add(se);

            se.DayOffset = -1;
            se.Rasi = SankrantiId.VRSABHA_SANKRANTI;
            se.Text = GCStrings.Localized("Tulasi Jala Dan ends");
            SanBasedList.Add(se);

        }

        public GCCalendarEvent add()
        {
            GCCalendarEvent p = new GCCalendarEvent();
            list.Add(p);
            return p;
        }

        public GCCalendarEvent add(GCCalendarEvent ce)
        {
            list.Add(ce);
            return ce;
        }

        public void clear()
        {
            list.Clear();
        }

        public void Remove(GCCalendarEvent ce)
        {
            list.Remove(ce);
        }
    }
}
