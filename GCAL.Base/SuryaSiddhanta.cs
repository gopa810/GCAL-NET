using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;


namespace GCAL.Base
{
    /// <summary>
    /// This class implements algorithms for obtaining Sun's and Moon's coordinates
    /// according Surya Siddhanta
    /// </summary>
    public class GCSuryaSiddhanta
    {
        public SuryaChandraData CalculateSunMoon(GregorianDateTime Day, GCEarthData earth)
        {
            double DC;
            double dHourr;
            double Desantar;
            double dVelantar;
            double dAnomalySunSaur;
            double dMadhRavi;
            double dMandaphal;
            double dMadhChandr;
            double dMadhChandroch = 0.0;

            SuryaChandraData scd = new SuryaChandraData();

            DateToAhargan(Day, earth, out scd.dAhargan, out dHourr, out Desantar, out DC);

            /*Debugger.Log(0, "", "Year: " + Day.year + ", DC: " + DC + "\n");
            Debugger.Log(0, "", "Long:" + earth.longitudeDeg + ", Latitude:" + earth.latitudeDeg);
            */
            scd.dAhargan = AharganTrue(scd.dAhargan, Desantar, out dVelantar);
            dHourr = (dHourr + (dVelantar / 60.0));

            //======================================================= Graha-Spashta :
            Surya(scd.dAhargan, Desantar, out scd.dSurya, out dAnomalySunSaur, out dMadhRavi, out dMandaphal);
            Chandr(scd.dAhargan, Desantar, out scd.dChandra, out dMadhChandr, out dMadhChandroch);

            return scd;
        }

        public double AharganTrue(double dAhargan, double Desantar, out double dVelantar)
        {
            double dAharganOriginl = dAhargan;
            double dSurya;
            double dAnomalySunSaur;
            double dMadhRavi;
            double dMandaphal;
            double dAyan;

            //-------------------------------------------------------  First EOT :
            Surya(dAhargan, Desantar, out dSurya, out dAnomalySunSaur, out dMadhRavi, out dMandaphal);
            dAyan = Ayanamsh(dAhargan);
            EquationOfTime(dAhargan, out dVelantar, dMadhRavi, dAyan, dAnomalySunSaur);
            dAhargan = dAharganOriginl + (dVelantar / 1440.0);
            //-------------------------------------------------------  Repeat EOT :
            Surya(dAhargan, Desantar, out dSurya, out dAnomalySunSaur, out dMadhRavi, out dMandaphal);
            dAyan = Ayanamsh(dAhargan);
            EquationOfTime(dAhargan, out dVelantar, dMadhRavi, dAyan, dAnomalySunSaur);
            dAhargan = dAharganOriginl + (dVelantar / 1440.0);

            return dAhargan;
        }

        /*=================================================================
        =================================================================
        =================================================================
        =================================================================
        =================================================================
        =================================================================
        This AharganTrue module calls three other modules, of which I have already sent "Surya" module. Here are remaining two modules :---
        =================================================================
        =================================================================
        =================================================================
        =================================================================
        =================================================================
        =================================================================*/

        public double Ayanamsh(double dAhargan)
        {
            double dAyan = (((dAhargan / 365.258756481482) / 7200.0) - Fix((dAhargan / 365.258756481482) / 7200.0)) * 108.0;
            if (dAyan > 0 && dAyan <= 27.0)
                dAyan = -dAyan;
            else if (dAyan > 27.0 && dAyan <= 81.0)
                dAyan = dAyan - 54.0;
            else if (dAyan > 81.0 && dAyan <= 108.0)
                dAyan = 108.0 - dAyan;
            else if (dAyan < 0 && dAyan >= -27.0)
                dAyan = Math.Abs(dAyan);
            else if (dAyan < -27.0 && dAyan >= -81.0)
                dAyan = 54.0 + dAyan;
            else if (dAyan < -81.0 && dAyan >= -108.0)
                dAyan = -(108.0 + dAyan);

            return dAyan;
        }
        /*=================================================================
        =================================================================
        =================================================================
        =================================================================
        =================================================================
        =================================================================*/

        public void EquationOfTime(double dAhargan, out double dVelantar, double dMadhRavi, double dAyan, double dAnomalySunSaur)
        {
            double dQ;
            double dTrueObl;
            double dEcc;
            double dSunAnomaly;
            double dL0;
            double dY;
            double dCos2L0;
            double dSin2L0;
            double dSin4L0;
            double dSinM;
            double dSin2M;
            double dEtime;
    
            dQ = 57.2957795130823;
            dTrueObl = 24.0;
            dEcc = 0.018984000805087;
            dSunAnomaly = dAnomalySunSaur;

            dL0 = dMadhRavi + dAyan;
            if (dL0 >= 360.0)
                dL0 = dL0 - 360.0;
            dY = Math.Pow((GCMath.tanDeg((dTrueObl / dQ) / 2.0)), 2.0);
            dCos2L0 = GCMath.cosDeg(2.0 * (dL0 / dQ));
            dSin2L0 = GCMath.sinDeg(2.0 * (dL0 / dQ));
            dSin4L0 = GCMath.sinDeg(4.0 * (dL0 / dQ));
            dSinM = GCMath.sinDeg((dSunAnomaly / dQ));
            dSin2M = GCMath.sinDeg(2.0 * (dSunAnomaly / dQ));
            dEtime = (dY * dSin2L0) - 
                (0.5 * dY * dY * dSin4L0) - 
                (2.0 * dEcc * dSinM) - 
                (1.25 * dEcc * dEcc * dSin2M) + 
                (4.0 * dY * dEcc * dSinM * dCos2L0);
            dVelantar = (dEtime * dQ) * 4.0;                 //' in minutes
        }

        public double Latitude;
        public double Longitude;
        //public TTimeZone timeZone;

        public void DateToAhargan(GregorianDateTime Day,GCEarthData earth,  out double dAhargan, out double dHourr, out double Desantar, out double DC)
        {
            int lYear = Day.year;
            int iMonth = Day.month;
            int iDate = Day.day;
            int ihour = Day.GetHour();
            double dMin = (Day.shour * 24.0 - ihour) * 60.0;

            string sLongSign = String.Empty;
            string sLatSign = String.Empty;

            dHourr = ihour + (dMin / 60.0) + ((earth.longitudeDeg - Day.TimezoneHours) / 15.0); // = Desantara  Samskaara.
            //dHourIST = dHourr - ((dLong - dTZ) / 15#)
            /*If frmData.txtName.Text = "NOW" Then
                If Left(frmData.Combo2.Text, 6) = "Medini" Then
                    dHourr = dHourr - ((82.5 - dTZ) / 15#)
                    //dHourIST = dHourIST - ((82.5 - dTZ) / 15#)
                End If
            End If*/
            DC = iDate - 1 + (dHourr / 24.0); //' dHourr  &   DC are in  LMT ( not SunDial's).
            //If dHourIST > 24# Then dHourIST = dHourr - 24#

            AharganPhal(lYear, iMonth, DC, out dAhargan);
            dAhargan = dAhargan + 1826556.5;
            Desantar = (earth.longitudeDeg - 75.7684565);
        }



        public void AharganPhal(int lYear, int iMonth, double DC, out double dAhargana)
        {
            int iL = 0;
            int iLeap = 0;
            int iMonthDays = 0;

            iL = GregorianDateTime.IsLeapYear(lYear) ? 1 : 0;

            if (lYear < 1582)
            {
                iL = iL + 13;
            }
            if (lYear < 1701)
            {
                iL = iL + 3;
            }
            if (lYear < 1801)
            {
                iL = iL + 2;
            }
            if (lYear < 1901)
            {
                iL = iL + 1;
            }

            if (lYear > 2099) { iL = iL - 1; }
            if (lYear > 2199) { iL = iL - 1; }
            if (lYear > 2299) { iL = iL - 1; }
            if (lYear > 2499) { iL = iL - 1; }
            if (lYear > 2599) { iL = iL - 1; }
            if (lYear > 2699) { iL = iL - 1; }
            if (lYear > 2899) { iL = iL - 1; }
            if (lYear > 2999) { iL = iL - 1; }
            if (lYear > 3099) { iL = iL - 1; }
            if (lYear > 3299) { iL = iL - 1; }

            //==========================================================================
            if (iMonth == 1) { iMonthDays = 31; }
            if (iMonth == 2) { DC = DC + 31; iMonthDays = 28 + iLeap; }
            if (iMonth == 3) { DC = DC + 59 + iLeap; iMonthDays = 31; }
            if (iMonth == 4) { DC = DC + 90 + iLeap; iMonthDays = 30; }
            if (iMonth == 5) { DC = DC + 120 + iLeap; iMonthDays = 31; }
            if (iMonth == 6) { DC = DC + 151 + iLeap; iMonthDays = 30; }
            if (iMonth == 7) { DC = DC + 181 + iLeap; iMonthDays = 31; }
            if (iMonth == 8) { DC = DC + 212 + iLeap; iMonthDays = 31; }
            if (iMonth == 9) { DC = DC + 243 + iLeap; iMonthDays = 30; }
            if (iMonth == 10) { DC = DC + 273 + iLeap; iMonthDays = 31; }
            if (iMonth == 11) { DC = DC + 304 + iLeap; iMonthDays = 30; }
            if (iMonth == 12) { DC = DC + 334 + iLeap; iMonthDays = 31; }

            dAhargana = DC + iL + ((lYear - 1951) * 365) + 18625.5;
            // AD 1900 had no iLeap. This is dAhargana since  17:30, Jan1, 1900 AD,IST.
        }

        public double Fix(double d)
        {
            return (d < 0.0 ? Math.Ceiling(d) : Math.Floor(d));
        }

        /// <summary>
        /// Sun Module
        /// </summary>
        public void Surya(double dAhargan, double Desantar, out double dSurya, out double dAnomalySunSaur, out double dMadhRavi, out double dMandaphal)
        {
            int a;
            double RaviM;
            double RaviMK = 0.0;
            int SignRavi = 0;
            dMadhRavi = (((dAhargan / (1577917828.0 / 4320000.0)) - Fix(dAhargan / (1577917828.0 / 4320000.0))) * 360.0);
        LabelMadhRavi1:
            if (dMadhRavi < 0) { dMadhRavi = dMadhRavi + 360.0; }
            if (dMadhRavi < 0) { goto LabelMadhRavi1; }
            dMadhRavi = dMadhRavi - (Desantar / 360.0);
            if (dMadhRavi < 0) { dMadhRavi = dMadhRavi + 360.0; }
            if (dMadhRavi >= 360) { dMadhRavi = dMadhRavi - 360.0; }
            RaviM = ((((1955880000.0 + (dAhargan / (1577917828.0 / 4320000.0))) / 4320000000.0) * 387.0) - Fix(((1955880000.0 + (dAhargan / (1577917828.0 / 4320000.0))) / 4320000000.0) * 387.0)) * 360.0;
        LabelRaviM1:
            if (RaviM >= 360) { RaviM = RaviM - 360.0; }
            if (RaviM >= 360) { goto LabelRaviM1; }
        LabelRaviM2:
            if (RaviM < 0) { RaviM = RaviM + 360.0; }
            if (RaviM < 0) { goto LabelRaviM2; }
        LabelRaviMK1:
            RaviMK = RaviM - dMadhRavi;
            if (RaviMK < 0) { RaviMK = RaviMK + 360.0; }
            if (RaviMK < 0) { goto LabelRaviMK1; }
            dAnomalySunSaur = RaviMK;

            if (RaviMK <= 90) { SignRavi = 1; }
            if (RaviMK > 90)
            {
                if (RaviMK <= 180) { goto Label12310; } else { goto Label12320; }
            Label12310:
                RaviMK = 180 - RaviMK;
                SignRavi = 1;
            Label12320:
                a = 1;
            } //=======================
            if (RaviMK > 180)
            {
                if (RaviMK <= 270) { goto Label12330; } else { goto Label12340; }
            Label12330:
                RaviMK = RaviMK - 180;
                SignRavi = -1;
            Label12340:
                a = 1;
            }

            if (RaviMK > 270)
            {
                if (RaviMK <= 360) { goto Label12350; } else { goto Label12360; }
            Label12350:
                RaviMK = 360.0 - RaviMK;
                SignRavi = -1;
            Label12360:
                a = 1;
            }

            double dX = 0.0;
            double dKendraJyaGunak = 0.0;
            double dSphutaParidhi = 0.0;
            dKendraJyaGunak = (6383.39600310841 / (1.0 - ((1.0 - (1.0 / (8.0 * (Math.PI * Math.PI) / 9))) * GCMath.cosDeg(RaviMK / 57.2957795130823)))) / 6383.39600310841;
            dSphutaParidhi = (4904.05431317952 + (RaviMK * 2.04862282821986 / 90.0)) + ((13.9626340159546 - (3.32582160003707E-03 * RaviMK)) * dKendraJyaGunak);
            dX = dSphutaParidhi * (GCMath.sinDeg(RaviMK / 57.2957795130823)) / 129600;
            dMandaphal = ((GCMath.arcTan2Deg(dX, Math.Sqrt(-dX * dX + 1.0))) * 57.2957795130823) * SignRavi;
            dSurya = dMadhRavi + dMandaphal;
            if (dSurya < 0.0) { dSurya = dSurya + 360.0; }
            if (dSurya >= 360.0) { dSurya = dSurya - 360.0; }
        }

        /// <summary>
        /// Chandra Module
        /// </summary>
        public void Chandr(double dAhargan, double Desantar, out double dChandr, out double dMadhChandr, out double dMadhChandroch)
        {
            int a = 0;
            double ChandrochBeej = 0.0;
            dMadhChandroch = (((488203.0 * 1000 * ((1955880000 + (dAhargan / 365.258756481482)) / 4320000000)) - Fix(488203.0 * 1000 * ((1955880000 + (dAhargan / 365.258756481482)) / 4320000000))) * 360);
            dMadhChandr = ((dAhargan / (1577917828.0 / 57753336)) - Fix(dAhargan / (1577917828.0 / 57753336.0))) * 360.0;
            ChandrochBeej = -(780.380064202824 * (1955880000.0 + (dAhargan / (1577917828.0 / 4320000.0)))) / 1000000000000.0;
            dMadhChandr = dMadhChandr - ((Desantar / 360.0) * (57753336.0 / 4320000.0));
            dMadhChandroch = dMadhChandroch - ((Desantar / 360.0) * (488203.0 / 4320000.0)) + ChandrochBeej;
            if (dMadhChandr < 0) { dMadhChandr = dMadhChandr + 360; }
            if (dMadhChandroch < 0) { dMadhChandroch = dMadhChandroch + 360; }
            if (dMadhChandr >= 360) { dMadhChandr = dMadhChandr - 360; }
            if (dMadhChandroch >= 360) { dMadhChandroch = dMadhChandroch - 360; }

            double ChandrMK = 0.0;
            int SignChandr = 0;
            ChandrMK = dMadhChandroch - dMadhChandr;
            if (ChandrMK < 0) { ChandrMK = ChandrMK + 360.0; }
            if (ChandrMK <= 90) { SignChandr = 1; }
            if (ChandrMK > 90)
            {
                if (ChandrMK <= 180)
                { goto Label12370; }
                else { goto Label12380; }
            Label12370:
                ChandrMK = 180 - ChandrMK;
                SignChandr = 1;
            Label12380:
                a = 0;
            } 

            if (ChandrMK > 180)
            {
                if (ChandrMK <= 270) { goto Label12390; } else { goto Label12400; }
            Label12390:
                ChandrMK = ChandrMK - 180;
                SignChandr = -1;
            Label12400:
                a = 0;
            } //--------------------------------
            if (ChandrMK > 270) { goto Label12410; } else { goto Label12420; }
        Label12410:
            ChandrMK = 360.0 - ChandrMK;
            SignChandr = -1;
        Label12420:
            // Bhuj = ChandrMK
            double dMFChandr = 0.0;
            double dQ = 57.2957795130823;
            if (ChandrMK == 0.0) { ChandrMK = 0.00000000001; }
            dMFChandr = dQ * GCMath.arcTanDeg(((((32.0 - (0.3335309975909 * GCMath.sinDeg(ChandrMK / dQ))) / 360.0)
                * 3437.74677078494) * GCMath.sinDeg((ChandrMK + (5.046428631 * GCMath.sinDeg(ChandrMK / dQ))) / dQ))
                / (3437.74677078494 + ((((32.0 - (0.3335309975909 * GCMath.sinDeg(ChandrMK / dQ))) / 360.0)
                * 3437.74677078494) * GCMath.cosDeg((ChandrMK + (5.046428631 * GCMath.sinDeg(ChandrMK / dQ))) / dQ))));
            dChandr = dMadhChandr + (dMFChandr * SignChandr);
            if (dChandr < 0) { dChandr = dChandr + 360.0; }
            if (dChandr >= 360.0) { dChandr = dChandr - 360.0; }
        }


        public SuryaData Sunrise(double dAhargan, double dLat)
        {
            double dSaayanSurya, dBhujSaayan = 0.0, dAmsadiBhujSaayan, dAharganOriginal;
            double dSurya, dMandaphal, dAnomalySunSaur, dMadhRavi;
            int iSignSaayan = 0, iRasiBhujSaayan, iterate = 0;
            double dQ = 1.0;

            SuryaData sd = new SuryaData();
            sd.dSuryoday = 0;
            dAharganOriginal = dAhargan;
            sd.dVelantarSunrise = 0;

            do
            {
                if (sd.dSuryoday == 0) { sd.dSuryoday = 15; }
                dAhargan = Fix(dAhargan) + (sd.dSuryoday / 60.0);
                if (dAhargan > dAharganOriginal) { dAhargan = dAhargan - 1.0; }

                Surya(dAhargan, sd.Desantar, out dSurya, out dAnomalySunSaur, out dMadhRavi, out dMandaphal);
                sd.dAyan = Ayanamsh(dAhargan);
                dSaayanSurya = sd.dAyan + dSurya;

                if (dSaayanSurya > 360) { dSaayanSurya = dSaayanSurya - 360; }
                if (dSaayanSurya <= 90) { dBhujSaayan = dSaayanSurya; }
                if (dSaayanSurya > 90 && dSaayanSurya <= 180) { dBhujSaayan = 180 - dSaayanSurya; }
                if (dSaayanSurya > 180 && dSaayanSurya <= 270) { dBhujSaayan = dSaayanSurya - 180; }
                if (dSaayanSurya > 270 && dSaayanSurya <= 360) { dBhujSaayan = 360 - dSaayanSurya; }
                iSignSaayan = (dSaayanSurya <= 180 ? 1 : -1);
                iRasiBhujSaayan = Convert.ToInt32(Fix(dBhujSaayan / 30.0));
                dAmsadiBhujSaayan = dBhujSaayan - (iRasiBhujSaayan * 30);

                sd.dKranti = GCMath.sinDeg(30.0 / dQ) * GCMath.sinDeg(24.0 / dQ);
                sd.dKranti = dQ * GCMath.arcTan2Deg(sd.dKranti, Math.Sqrt(-sd.dKranti * sd.dKranti + 1));
                sd.dChar10 = GCMath.tanDeg(sd.dKranti / dQ) * GCMath.tanDeg(dLat / dQ);
                if (sd.dChar10 >= 1) { sd.dChar10 = 0.99999999999999; }
                sd.dChar10 = 10.0 * dQ * GCMath.arcTan2Deg(sd.dChar10, Math.Sqrt(-sd.dChar10 * sd.dChar10 + 1));
                sd.dKranti = GCMath.sinDeg(60.0 / dQ) * GCMath.sinDeg(24.0 / dQ);
                sd.dKranti = dQ * GCMath.arcTan2Deg(sd.dKranti, Math.Sqrt(-sd.dKranti * sd.dKranti + 1));
                sd.dChar8 = GCMath.tanDeg(sd.dKranti / dQ) * GCMath.tanDeg(dLat / dQ);
                if (sd.dChar8 >= 1) sd.dChar8 = 0.99999999999999;
                sd.dChar8 = (10.0 * dQ * GCMath.arcTan2Deg(sd.dChar8, Math.Sqrt(-sd.dChar8 * sd.dChar8 + 1))) - sd.dChar10;
                sd.dKranti = GCMath.sinDeg(90.0 / dQ) * GCMath.sinDeg(24.0 / dQ);
                sd.dKranti = dQ * GCMath.arcTan2Deg(sd.dKranti, Math.Sqrt(-sd.dKranti * sd.dKranti + 1));
                sd.dChar3 = GCMath.tanDeg(sd.dKranti / dQ) * GCMath.tanDeg(dLat / dQ);
                if (sd.dChar3 >= 1) sd.dChar3 = 0.99999999999999;
                sd.dChar3 = (10.0 * dQ * GCMath.arcTan2Deg(sd.dChar3, Math.Sqrt(-sd.dChar3 * sd.dChar3 + 1))) - sd.dChar10 - sd.dChar8;
                sd.dKranti = GCMath.sinDeg(dBhujSaayan / dQ) * GCMath.sinDeg(24.0 / dQ);
                sd.dKranti = dQ * GCMath.arcTan2Deg(sd.dKranti, Math.Sqrt(-sd.dKranti * sd.dKranti + 1));
                sd.dCharpal = GCMath.tanDeg(sd.dKranti / dQ) * GCMath.tanDeg(dLat / dQ);
                if (sd.dCharpal >= 1) sd.dCharpal = 0.99999999999999;
                sd.dCharpal = 10.0 * dQ * GCMath.arcTan2Deg(sd.dCharpal, Math.Sqrt(-sd.dCharpal * sd.dCharpal + 1));
                sd.dMishraMaan = 45.0 + ((sd.dCharpal / 60.0) * iSignSaayan);
                sd.dSuryoday = 60.0 - sd.dMishraMaan;
                iterate = iterate + 1;
                if (iterate == 5)
                {
                    //'-----------------------------------
                    double dTrueObl, dEcc, dSunAnomaly, dL0, dEtime;
                    dTrueObl = 24.0;
                    dEcc = 0.018984000805087;
                    dSunAnomaly = dAnomalySunSaur;
                    dL0 = (dMadhRavi + sd.dAyan);
                    if (dL0 >= 360.0) dL0 = dL0 - 360.0;
                    dEtime = (Math.Pow(GCMath.tanDeg((dTrueObl / dQ) / 2.0), 2.0) * GCMath.sinDeg(2.0 * (dL0 / dQ)))
                        - (0.5 * Math.Pow(GCMath.tanDeg((dTrueObl / dQ) / 2.0), 2.0)
                        * Math.Pow(GCMath.tanDeg((dTrueObl / dQ) / 2.0), 2.0) * (GCMath.sinDeg(4.0 * (dL0 / dQ))))
                        - (2.0 * dEcc * (GCMath.sinDeg((dSunAnomaly / dQ))))
                        - (1.25 * dEcc * dEcc * (GCMath.sinDeg(2.0 * (dSunAnomaly / dQ))))
                        + (4.0 * Math.Pow(GCMath.tanDeg((dTrueObl / dQ) / 2.0), 2) * dEcc
                        * (GCMath.sinDeg((dSunAnomaly / dQ))) * (GCMath.cosDeg(2.0 * (dL0 / dQ))));
                    sd.dVelantarSunrise = (dEtime * dQ) * 4.0;
                }
            } while (iterate < 5) ;

            return sd;
        }
    }

    public struct SuryaData
    {
        public double dChar10;
        public double dChar8;
        public double dChar3;
        public double dSuryoday; 
        public double dMishraMaan; 
        public double dCharpal;
        public double dKranti;
        public double Desantar;
        public double dVelantarSunrise;
        public double dAyan;
    }

    public struct SuryaChandraData
    {
        public double dSurya;
        public double dChandra;
        public double dAhargan;
    }
}
