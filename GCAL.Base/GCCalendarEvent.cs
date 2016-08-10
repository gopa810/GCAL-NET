using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GCAL.Base
{
    public class GCCalendarEvent
    {
        public int nTithi;
        public int nMasa;
        public int nClass;
        public int nFastType;
        public int nVisible;
        public int nStartYear;
        public int nUsed;
        public int nDeleted;
        public int nSpec;
        public string strFastSubject;
        public String strText;

        public GCCalendarEvent()
        {
            nClass = 0;
            nTithi = 0;
            nMasa = 0;
            nFastType = 0;
            nVisible = 1;
            nUsed = 1;
            nDeleted = 0;
            nSpec = 0;
        }

    }
}
