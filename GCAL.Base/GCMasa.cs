using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GCAL.Base
{
    public class GCMasa
    {
        private static string[] p_masaNameGaudiya = new string[] {
            "Madhusudana", // GCStrings.Localized("Madhusudana");
            "Trivikrama", // GCStrings.Localized("Trivikrama");
            "Vamana", // GCStrings.Localized("Vamana");
            "Sridhara", // GCStrings.Localized("Sridhara");
            "Hrsikesa", // GCStrings.Localized("Hrsikesa");
            "Padmanabha", // GCStrings.Localized("Padmanabha");
            "Damodara", // GCStrings.Localized("Damodara");
            "Kesava", // GCStrings.Localized("Kesava");
            "Narayana", // GCStrings.Localized("Narayana");
            "Madhava", // GCStrings.Localized("Madhava");
            "Govinda", // GCStrings.Localized("Govinda");
            "Visnu", // GCStrings.Localized("Visnu");
            "Purusottama-adhika", // GCStrings.Localized("Purusottama-adhika");
        };

        private static string[] p_masaNameVedic = new string[] {
            "Vaisakha", // GCStrings.Localized("Vaisakha");
            "Jyestha", // GCStrings.Localized("Jyestha");
            "Asadha", // GCStrings.Localized("Asadha");
            "Sravana", // GCStrings.Localized("Sravana");
            "Bhadra", // GCStrings.Localized("Bhadra");
            "Asvina", // GCStrings.Localized("Asvina");
            "Kartika", // GCStrings.Localized("Kartika");
            "Margasirsa", // GCStrings.Localized("Margasirsa");
            "Pausa", // GCStrings.Localized("Pausa");
            "Magha", // GCStrings.Localized("Magha");
            "Phalguna", // GCStrings.Localized("Phalguna");
            "Caitra", // GCStrings.Localized("Caitra");
            "Adhika", // GCStrings.Localized("Adhika");
        };

        /// <summary>
        /// Returns Gaudiya style of name of the month
        /// </summary>
        /// <param name="masaID">0..Madhusudana, 12..Purusottama-adhika</param>
        /// <returns></returns>
        private static string GetGaudiyaName(int masaID)
        {
            return GCStrings.Localized(p_masaNameGaudiya[masaID % 13]);
        }

        /// <summary>
        /// Returns Vedic style of name of the month
        /// </summary>
        /// <param name="masaID"></param>
        /// <returns></returns>
        private static string GetVedicName(int masaID)
        {
            return GCStrings.Localized(p_masaNameVedic[masaID % 13]);
        }

        /// <summary>
        /// Returns combined name of month according user settings
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public static string GetName(int i)
        {
            switch (GCDisplaySettings.getValue(49))
            {
                case 0: return GetGaudiyaName(i);
                case 1: return string.Format("{0} ({1})", GetGaudiyaName(i), GetVedicName(i));
                case 2: return GetVedicName(i);
                case 3: return string.Format("{0} ({1})", GetVedicName(i), GetGaudiyaName(i));
                default: return GetGaudiyaName(i);
            }
        }
    }
}
