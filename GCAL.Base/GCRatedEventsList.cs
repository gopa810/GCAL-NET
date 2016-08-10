using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;

namespace GCAL.Base
{
    public class GCRatedEventsList
    {
        private static List<GCRatedEvent> list = new List<GCRatedEvent>();
        public static string Name = "Untitled";
        public static int Revision = 0;
        public static string FileName = string.Empty;
        public static bool ShowOnlyPositive = false;
        public static bool ShowOnlyAboveLevel = false;
        public static double AboveLevelValue = -1000.0;
        public static bool ShowPeriodLongerThan = false;
        public static double PeriodLongerValue = -1.0;
        public static string Description = string.Empty;


        public static List<GCRatedEvent> List
        {
            get
            {
                if (list.Count == 0)
                {
                    list.Add(new GCRatedEvent("sun.day",
                        true, CoreEventType.CCTYPE_S_RISE, 0, 
                        true, CoreEventType.CCTYPE_S_SET, 0, 0.0));
                    list.Add(new GCRatedEvent("sun.morning",
                        true, CoreEventType.CCTYPE_S_RISE, 0, 
                        true, CoreEventType.CCTYPE_S_NOON, 0, 0.0));
                    list.Add(new GCRatedEvent("sun.afternoon",
                        true, CoreEventType.CCTYPE_S_NOON, 0,
                        true, CoreEventType.CCTYPE_S_SET, 0, 0.0));
                    list.Add(new GCRatedEvent("sun.night",
                        true, CoreEventType.CCTYPE_S_SET, 0,
                        true, CoreEventType.CCTYPE_S_RISE, 0, 0.0));
                    list.Add(new GCRatedEvent("sun.beforemidn",
                        true, CoreEventType.CCTYPE_S_SET, 0, 
                        true, CoreEventType.CCTYPE_S_MIDNIGHT, 0, 0.0));
                    list.Add(new GCRatedEvent("sun.aftermidn",
                        true, CoreEventType.CCTYPE_S_MIDNIGHT, 0, 
                        true, CoreEventType.CCTYPE_S_RISE, 0, 0.0));

                    for (int i = 0; i < 4; i++)
                    {
                        list.Add(new GCRatedEvent("sun.sand." + i,
                            true, CoreEventType.CCTYPE_KALA_START, KalaType.KT_SANDHYA_SUNRISE + i, 
                            true, CoreEventType.CCTYPE_KALA_END, KalaType.KT_SANDHYA_SUNRISE + i, 0.0));
                    }

                    for (int i = 0; i < 27; i++)
                    {
                        list.Add(new GCRatedEvent("naks." + i, 
                            true, CoreEventType.CCTYPE_NAKS, i,
                            false, CoreEventType.CCTYPE_NAKS, i, 0.0));
                        for (int j = 0; j < 4; j++)
                        {
                            list.Add(new GCRatedEvent("naks." + i + "." + j, 
                                true, CoreEventType.CCTYPE_NAKS_PADA1 + j, i, 
                                false, CoreEventType.CCTYPE_NAKS_PADA1 + j, i, 0.0));
                        }
                    }

                    for (int i = 0; i < 15; i++)
                    {
                        list.Add(new GCRatedEvent("tithi." + i, 
                            true, CoreEventType.CCTYPE_TITHI_BASE, i, 
                            true, CoreEventType.CCTYPE_TITHI_BASE, i, 0.0));
                        list.Add(new GCRatedEvent("tithi." + i + ".0",
                            true, CoreEventType.CCTYPE_TITHI, i,
                            false, CoreEventType.CCTYPE_TITHI, i, 0.0));
                        list.Add(new GCRatedEvent("tithi." + i +".1",
                            true, CoreEventType.CCTYPE_TITHI, i + 15,
                            false, CoreEventType.CCTYPE_TITHI, i + 15, 0.0));
                    }

                    for (int i = 0; i < 30; i++)
                    {
                        list.Add(new GCRatedEvent("muhurta." + i,
                            true, CoreEventType.CCTYPE_DAY_MUHURTA, i,
                            false, CoreEventType.CCTYPE_DAY_MUHURTA, i, 0.0));
                    }

                    for (int i = 0; i < 12; i++)
                    {
                        list.Add(new GCRatedEvent("sun.rasi." + i, 
                            true, CoreEventType.CCTYPE_SANK, i, 
                            false, CoreEventType.CCTYPE_SANK, i, 0.0));
                    }

                    for (int i = 0; i < 12; i++)
                    {
                        list.Add(new GCRatedEvent("moon.rasi." + i, 
                            true, CoreEventType.CCTYPE_M_RASI, i, 
                            false, CoreEventType.CCTYPE_M_RASI, i, 0.0));
                    }

                    for (int i = 0; i < 27; i++)
                    {
                        list.Add(new GCRatedEvent("yoga." + i,
                            true, CoreEventType.CCTYPE_YOGA, i,
                            false, CoreEventType.CCTYPE_YOGA, i, 0.0));
                    }


                    for (int i = 0; i < 12; i++)
                    {
                        list.Add(new GCRatedEvent("ascendent." + i,
                            true, CoreEventType.CCTYPE_ASCENDENT, i,
                            false, CoreEventType.CCTYPE_ASCENDENT, i, 0.0));
                    }

                    for (int i = 0; i < 7; i++)
                    {
                        list.Add(new GCRatedEvent("weekday." + i,
                            true, CoreEventType.CCTYPE_DAY_OF_WEEK, i,
                            false, CoreEventType.CCTYPE_DAY_OF_WEEK, i, 0.0));
                    }

                    list.Add(new GCRatedEvent("spec.rahu",
                        true, CoreEventType.CCTYPE_KALA_START, KalaType.KT_RAHU_KALAM,
                        true, CoreEventType.CCTYPE_KALA_END, KalaType.KT_RAHU_KALAM, 0.0));
                    list.Add(new GCRatedEvent("spec.yama",
                        true, CoreEventType.CCTYPE_KALA_START, KalaType.KT_YAMA_GHANTI,
                        true, CoreEventType.CCTYPE_KALA_END, KalaType.KT_YAMA_GHANTI, 0.0));
                    list.Add(new GCRatedEvent("spec.guli",
                        true, CoreEventType.CCTYPE_KALA_START, KalaType.KT_GULI_KALAM,
                        true, CoreEventType.CCTYPE_KALA_END, KalaType.KT_GULI_KALAM, 0.0));
                    list.Add(new GCRatedEvent("spec.vish",
                        true, CoreEventType.CCTYPE_KALA_START, KalaType.KT_VISHAGATI,
                        true, CoreEventType.CCTYPE_KALA_END, KalaType.KT_VISHAGATI, 0.0));
                    list.Add(new GCRatedEvent("spec.abhijit",
                        true, CoreEventType.CCTYPE_KALA_START, KalaType.KT_ABHIJIT,
                        true, CoreEventType.CCTYPE_KALA_END, KalaType.KT_ABHIJIT, 0.0));

                }

                return list;
            }
        }

        public static bool HasItemWithType(int type)
        {
            foreach(GCRatedEvent re in List)
            {
                if ((re.StartType == type || re.EndType == type) 
                    && (re.Rating != 0.0 || re.Rejected != false))
                    return true;
            }

            return false;
        }

        public static bool HasItemTypeRange(int typeFrom, int typeTo)
        {
            foreach (GCRatedEvent re in List)
            {
                if ((re.StartType >= typeFrom && re.StartType <= typeTo)
                     && (re.Rating != 0.0 || re.Rejected != false))
                    return true;
            }

            return false;
        }

        public static bool HasItemWithType(int type, int data)
        {
            foreach (GCRatedEvent re in List)
            {
                if (((re.StartType == type && re.StartData == data)
                    || (re.EndType == type && re.EndData == data)) && (re.Rating != 0.0 || re.Rejected != false))
                    return true;
            }

            return false;
        }

        public static GCRatedEvent EventWithKey(string key)
        {
            List<GCRatedEvent> L = List;

            foreach (GCRatedEvent re in L)
            {
                if (re.Key.Equals(key))
                    return re;
            }

            return null;
        }

        public static bool FileExists(string fileName)
        {
            return File.Exists(fileName);
        }

        public static bool LoadFile(string fileName)
        {
            XmlDocument doc = new XmlDocument();

            try
            {
                Clear();
                doc.Load(fileName);
                FileName = fileName;
            }
            catch
            {
                return false;
            }

            string key, rating, note;

            foreach (XmlElement root in doc.ChildNodes)
            {
                if (root.Name.Equals("rated"))
                {
                    foreach (XmlElement xe in root.ChildNodes)
                    {
                        if (xe.Name.Equals("event"))
                        {
                            key = xe.HasAttribute("key") ? xe.GetAttribute("key") : null;
                            if (key == null)
                                continue;
                            GCRatedEvent ge = EventWithKey(key);
                            if (ge == null)
                                continue;

                            rating = xe.HasAttribute("rating") ? xe.GetAttribute("rating") : null;
                            if (rating != null)
                            {
                                double d = 0.0;
                                double.TryParse(rating, out d);
                                ge.Rating = d;
                            }

                            note = xe.InnerText;
                            if (note != null && note.Length > 0)
                                ge.Note = note;
                        }
                        else if (xe.Name.Equals("name"))
                        {
                            Name = xe.InnerText;
                            if (xe.HasAttribute("revision"))
                            {
                                if (!int.TryParse(xe.GetAttribute("revision"), out Revision))
                                    Revision = 1;
                            }
                            else
                            {
                                Revision = 1;
                            }
                        }
                        else if (xe.Name.Equals("showonlypositive"))
                        {
                            bool.TryParse(xe.InnerText, out ShowOnlyPositive);
                        }
                        else if (xe.Name.Equals("showonlyabove"))
                        {
                            bool.TryParse(xe.InnerText, out ShowOnlyAboveLevel);
                        }
                        else if (xe.Name.Equals("showonlyabovevalue"))
                        {
                            double.TryParse(xe.InnerText, out AboveLevelValue);
                        }
                        else if (xe.Name.Equals("showonlylonger"))
                        {
                            bool.TryParse(xe.InnerText, out ShowPeriodLongerThan);
                        }
                        else if (xe.Name.Equals("showonlylongervalue"))
                        {
                            double.TryParse(xe.InnerText, out PeriodLongerValue);
                        }
                    }
                }
            }


            return true;
        }

        public static bool SaveFile(string fileName)
        {
            XmlDocument doc = new XmlDocument();
            XmlElement xe, root;

            root = doc.CreateElement("rated");
            doc.AppendChild(root);

            List<GCRatedEvent> L = List;

            foreach (GCRatedEvent re in L)
            {
                if (re.Rating != 0.0)
                {
                    xe = doc.CreateElement("event");
                    root.AppendChild(xe);
                    xe.SetAttribute("key", re.Key);
                    xe.SetAttribute("rating", re.Rating.ToString());
                    if (re.Note != null && re.Note.Length > 0)
                        xe.InnerText = re.Note;
                }
            }

            xe = doc.CreateElement("name");
            xe.InnerText = Name;
            root.AppendChild(xe);

            xe = doc.CreateElement("showonlypositive");
            xe.InnerText = ShowOnlyPositive.ToString();
            root.AppendChild(xe);

            xe = doc.CreateElement("showonlyabove");
            xe.InnerText = ShowOnlyAboveLevel.ToString();
            root.AppendChild(xe);

            xe = doc.CreateElement("showonlyabovevalue");
            xe.InnerText = AboveLevelValue.ToString();
            root.AppendChild(xe);

            xe = doc.CreateElement("showonlylonger");
            xe.InnerText = ShowPeriodLongerThan.ToString();
            root.AppendChild(xe);

            xe = doc.CreateElement("showonlylongervalue");
            xe.InnerText = PeriodLongerValue.ToString();
            root.AppendChild(xe);

            xe.SetAttribute("revision", Revision.ToString());

            try
            {
                doc.Save(fileName);
                FileName = fileName;
            }
            catch
            {
                return false;
            }

            return true;
        }

        public static void Clear()
        {
            List<GCRatedEvent> L = List;

            Name = "Untitled";
            Revision = 0;
            FileName = string.Empty;

            foreach (GCRatedEvent re in L)
            {
                re.Rating = 0.0;
                re.Note = null;
            }
        }
    }
}
