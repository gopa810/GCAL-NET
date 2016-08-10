using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GCAL.Base
{
    public class GCRatedEvent
    {
        public string Title = "";
        public string Key = "";

        public bool StartEqual = true;
        public int StartType = 0;
        public int StartData = 0;

        public bool EndEqual = true;
        public int EndType = 0;
        public int EndData = 0;

        public double Rating = 0.0;
        public bool Rejected = false;
        public string Note = null;

        public GCRatedEvent()
        {
        }

        public GCRatedEvent(string key, bool startEqual, int startType, int startData, bool endEqual, int endType, int endData, double rating)
        {
            Key = key;
            StartEqual = startEqual;
            StartType = startType;
            StartData = startData;
            EndEqual = endEqual;
            EndType = endType;
            EndData = endData;
            Rating = rating;
        }

        public bool IsActive()
        {
            return (Rating != 0.0 || Note != null);
        }


        public bool MeetsStart(TDayEvent de)
        {
            if (StartEqual && de.nType == StartType && de.nData == StartData)
            {
                return true;
            }
            else if (!StartEqual && de.nType == StartType && de.nData != StartData)
            {
                return true;
            }

            return false;
        }

        public bool MeetsEnd(TDayEvent de)
        {
            if (EndEqual && de.nType == EndType && de.nData == EndData)
            {
                return true;
            }
            else if (!EndEqual && de.nType == EndType && de.nData != EndData)
            {
                return true;
            }

            return false;
        }
    }
}
