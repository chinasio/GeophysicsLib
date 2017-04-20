﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace GeophysicsLib
{
    public static class IGRFMode
    {
        #region 变量定义
        /// <summary>
        /// 全局临时变量
        /// MAXMOD：IGRF文件中的model数最大值
        /// </summary>
        private const int MAXMOD = 30;
        private const int MAXDEG = 13;
        private const int MAXCOEFF = MAXDEG * (MAXDEG + 2) + 1;
        private static double[] gh1 = new double[MAXCOEFF];
        private static double[] gh2 = new double[MAXCOEFF];
        private static double[] gha = new double[MAXCOEFF];
        private static double[] ghb = new double[MAXCOEFF];
        private static double dtemp = 0, ftemp = 0, htemp = 0, itemp = 0;
        private static double xtemp = 0, ytemp = 0, ztemp = 0;
        //private List<string> igrf_cof = new List<string>();
        /// <summary>
        /// IGRF参数值:
        /// X Y Z 
        /// D、F、H、I 
        /// </summary>
        private static double x = 0, y = 0, z = 0;
        private static double d = 0, f = 0, h = 0, i = 0;
        #endregion
        /// <summary>
        /// 获取正常场总场值F
        /// </summary>
        /// <param name="year">年</param>
        /// <param name="month">月</param>
        /// <param name="day">日</param>
        /// <param name="latitude">纬度（°）</param>
        /// <param name="longitude">经度（°）</param>
        /// <param name="elevation">海报高度（m）</param>
        /// <returns>总场值F</returns>
        public static double getF(int year, int month, int day, double latitude, double longitude, double elevation)
        {
            double dates = julday(year, month, day);
            elevation = elevation / 1000.0;
            igrf_calculate(igrf_cof, 1, dates, latitude, longitude, elevation);

            return f;
        }
        /// <summary>
        /// 获取正常场X分量值
        /// </summary>
        /// <param name="year">年</param>
        /// <param name="month">月</param>
        /// <param name="day">日</param>
        /// <param name="latitude">纬度（°）</param>
        /// <param name="longitude">经度（°）</param>
        /// <param name="elevation">海报高度（m）</param>
        /// <returns>X分量值</returns>
        public static double getX(int year, int month, int day, double latitude, double longitude, double elevation)
        {
            double dates = julday(year, month, day);
            elevation = elevation / 1000.0;
            igrf_calculate(igrf_cof, 1, dates, latitude, longitude, elevation);

            return x;
        }
        /// <summary>
        /// 获取正常场Y分量值
        /// </summary>
        /// <param name="year">年</param>
        /// <param name="month">月</param>
        /// <param name="day">日</param>
        /// <param name="latitude">纬度（°）</param>
        /// <param name="longitude">经度（°）</param>
        /// <param name="elevation">海报高度（m）</param>
        /// <returns>Y分量值</returns>
        public static double getY(int year, int month, int day, double latitude, double longitude, double elevation)
        {
            double dates = julday(year, month, day);
            elevation = elevation / 1000.0;
            igrf_calculate(igrf_cof, 1, dates, latitude, longitude, elevation);

            return y;
        }
        /// <summary>
        /// 获取正常场Z分量值
        /// </summary>
        /// <param name="year">年</param>
        /// <param name="month">月</param>
        /// <param name="day">日</param>
        /// <param name="latitude">纬度（°）</param>
        /// <param name="longitude">经度（°）</param>
        /// <param name="elevation">海报高度（m）</param>
        /// <returns>Z分量值</returns>
        public static double getZ(int year, int month, int day, double latitude, double longitude, double elevation)
        {
            double dates = julday(year, month, day);
            elevation = elevation / 1000.0;
            igrf_calculate(igrf_cof, 1, dates, latitude, longitude, elevation);

            return z;
        }
        /// <summary>
        /// 获取正常场H分量值
        /// </summary>
        /// <param name="year">年</param>
        /// <param name="month">月</param>
        /// <param name="day">日</param>
        /// <param name="latitude">纬度（°）</param>
        /// <param name="longitude">经度（°）</param>
        /// <param name="elevation">海报高度（m）</param>
        /// <returns>H分量值</returns>
        public static double getH(int year, int month, int day, double latitude, double longitude, double elevation)
        {
            double dates = julday(year, month, day);
            elevation = elevation / 1000.0;
            igrf_calculate(igrf_cof, 1, dates, latitude, longitude, elevation);

            return h;
        }
        #region 函数定义
        /// <summary>
        /// 将年月日转为年，用x.xx年的形式表示月和日
        /// </summary>
        /// <param name="year">年</param>
        /// <param name="month">月</param>
        /// <param name="day">日</param>
        /// <returns></returns>
        private static double julday(int year, int month, int day)
        {
            int[] days = new int[12] { 0, 31, 59, 90, 120, 151, 181, 212, 243, 273, 304, 334 };
            int leap_year = 0;
            if ((((year % 4) == 0) && (((year % 100) != 0) || ((year % 400) == 0))))
            {
                leap_year = 1;
            }

            double day_in_year = (days[month - 1] + day + (month > 2 ? leap_year : 0));

            return ((double)year + (day_in_year / (365.0 + leap_year)));
        }
        
        /// <summary>
        /// Extrapolates linearly a spherical harmonic model with a rate-of-change model.
        /// </summary>
        /// <param name="date">计算时间（decimal year）</param>
        /// <param name="dte1">模型时间（decimal year）</param>
        /// <param name="nmax1">maximum degree and order of base model</param>
        /// <param name="nmax2">maximum degree and order of rate-of-change model</param>
        /// <param name="gh">Schmidt quasi-normal internal spherical harmonic coefficients of rate-of-change model</param>
        /// <returns>gha</returns>
        private static int extrapsh(double date, double dte1, int nmax1, int nmax2, int gh)
        {
            int nmax;
            int k, l;
            int ii;
            double factor = date - dte1;

            if (nmax1 == nmax2)
            {
                k = nmax1 * (nmax1 + 2);
                nmax = nmax1;
            }
            else
            {
                if (nmax1 > nmax2)
                {
                    k = nmax2 * (nmax2 + 2);
                    l = nmax1 * (nmax1 + 2);
                    switch (gh)
                    {
                        case 3: for (ii = k + 1; ii <= l; ++ii)
                            {
                                gha[ii] = gh1[ii];
                            }
                            break;
                        case 4: for (ii = k + 1; ii <= l; ++ii)
                            {
                                ghb[ii] = gh1[ii];
                            }
                            break;
                        default: ;
                            break;
                    }
                    nmax = nmax1;
                }
                else
                {
                    k = nmax1 * (nmax1 + 2);
                    l = nmax2 * (nmax2 + 2);
                    switch (gh)
                    {
                        case 3: for (ii = k + 1; ii <= l; ++ii)
                            {
                                gha[ii] = factor * gh2[ii];
                            }
                            break;
                        case 4: for (ii = k + 1; ii <= l; ++ii)
                            {
                                ghb[ii] = factor * gh2[ii];
                            }
                            break;
                        default: ;
                            break;
                    }
                    nmax = nmax2;
                }
            }
            switch (gh)
            {
                case 3: for (ii = 1; ii <= k; ++ii)
                    {
                        gha[ii] = gh1[ii] + factor * gh2[ii];
                    }
                    break;
                case 4: for (ii = 1; ii <= k; ++ii)
                    {
                        ghb[ii] = gh1[ii] + factor * gh2[ii];
                    }
                    break;
                default:
                    break;
            }
            return (nmax);
        }
        /****************************************************************************/
        /*                                                                          */
        /*                           Subroutine interpsh                            */
        /*                                                                          */
        /****************************************************************************/
        /*                                                                          */
        /*     Interpolates linearly, in time, between two spherical harmonic       */
        /*     models.                                                              */
        /*                                                                          */
        /*     Input:                                                               */
        /*           date     - date of resulting model (in decimal year)           */
        /*           dte1     - date of earlier model                               */
        /*           nmax1    - maximum degree and order of earlier model           */
        /*           gh1      - Schmidt quasi-normal internal spherical             */
        /*                      harmonic coefficients of earlier model              */
        /*           dte2     - date of later model                                 */
        /*           nmax2    - maximum degree and order of later model             */
        /*           gh2      - Schmidt quasi-normal internal spherical             */
        /*                      harmonic coefficients of internal model             */
        /*                                                                          */
        /*     Output:                                                              */
        /*           gha or b - coefficients of resulting model                     */
        /*           nmax     - maximum degree and order of resulting model         */
        /*                                                                          */
        private static int interpsh(double date, double dte1, int nmax1, double dte2, int nmax2, int gh)
        {
            int nmax = 0;
            int k, l = 0;
            int ii = 0;
            double factor = (date - dte1) / (dte2 - dte1);
            if (nmax1 == nmax2)
            {
                k = nmax1 * (nmax1 + 2);
                nmax = nmax1;
            }
            else
            {
                if (nmax1 > nmax2)
                {
                    k = nmax2 * (nmax2 + 2);
                    l = nmax1 * (nmax1 + 2);
                    switch (gh)
                    {
                        case 3: for (ii = k + 1; ii <= l; ++ii)
                            {
                                gha[ii] = gh1[ii] + factor * (-gh1[ii]);
                            }
                            break;
                        case 4: for (ii = k + 1; ii <= l; ++ii)
                            {
                                ghb[ii] = gh1[ii] + factor * (-gh1[ii]);
                            }
                            break;
                        default:
                            break;
                    }
                    nmax = nmax1;
                }
                else
                {
                    k = nmax1 * (nmax1 + 2);
                    l = nmax2 * (nmax2 + 2);
                    switch (gh)
                    {
                        case 3: for (ii = k + 1; ii <= l; ++ii)
                            {
                                gha[ii] = factor * gh2[ii];
                            }
                            break;
                        case 4: for (ii = k + 1; ii <= l; ++ii)
                            {
                                ghb[ii] = factor * gh2[ii];
                            }
                            break;
                        default:
                            break;
                    }
                    nmax = nmax2;
                }
            }
            switch (gh)
            {
                case 3: for (ii = 1; ii <= k; ++ii)
                    {
                        gha[ii] = gh1[ii] + factor * (gh2[ii] - gh1[ii]);
                    }
                    break;
                case 4: for (ii = 1; ii <= k; ++ii)
                    {
                        ghb[ii] = gh1[ii] + factor * (gh2[ii] - gh1[ii]);
                    }
                    break;
                default:
                    break;
            }
            return (nmax);
        }
        /****************************************************************************/
        /*                                                                          */
        /*                           Subroutine shval3                              */
        /*                                                                          */
        /****************************************************************************/
        /*                                                                          */
        /*     Calculates field components from spherical harmonic (sh)             */
        /*     models.                                                              */
        /*                                                                          */
        /*     Input:                                                               */
        /*           igdgc     - indicates coordinate system used; set equal        */
        /*                       to 1 if geodetic, 2 if geocentric                  */
        /*           latitude  - north latitude, in degrees                         */
        /*           longitude - east longitude, in degrees                         */
        /*           elev      - WGS84 altitude above ellipsoid (igdgc=1), or       */
        /*                       radial distance from earth's center (igdgc=2)      */
        /*           a2,b2     - squares of semi-major and semi-minor axes of       */
        /*                       the reference spheroid used for transforming       */
        /*                       between geodetic and geocentric coordinates        */
        /*                       or components                                      */
        /*           nmax      - maximum degree and order of coefficients           */
        /*           iext      - external coefficients flag (=0 if none)            */
        /*           ext1,2,3  - the three 1st-degree external coefficients         */
        /*                       (not used if iext = 0)                             */
        /*                                                                          */
        /*     Output:                                                              */
        /*           x         - northward component                                */
        /*           y         - eastward component                                 */
        /*           z         - vertically-downward component                      */
        /*                                                                          */
         private static int shval3(int igdgc, double flat, double flon, double elev, int nmax, int gh, int iext, double ext1, double ext2, double ext3)
        {
            double earths_radius = 6371.2;
            double dtr = 0.01745329;
            double slat;
            double clat;
            double ratio;
            double aa, bb, cc, dd;
            double sd;
            double cd;
            double r;
            double a2;
            double b2;
            double rr = 0;
            double fm, fn = 0;
            double[] sl = new double[14];
            double[] cl = new double[14];
            double[] p = new double[119];
            double[] q = new double[119];
            int ii, j, k, l, m, n;
            int npq;
            int ios;
            double argument;
            double power;
            a2 = 40680631.59;            /* WGS84 */
            b2 = 40408299.98;            /* WGS84 */
            ios = 0;
            r = elev ;  //将m转为km
            argument = flat * dtr;
            slat = Math.Sin(argument);
            if ((90.0 - flat) < 0.001)
            {
                aa = 89.999;            /*  300 ft. from North pole  */
            }
            else
            {
                if ((90.0 + flat) < 0.001)
                {
                    aa = -89.999;        /*  300 ft. from South pole  */
                }
                else
                {
                    aa = flat;
                }
            }
            argument = aa * dtr;
            clat = Math.Cos(argument);
            argument = flon * dtr;
            sl[1] = Math.Sin(argument);
            cl[1] = Math.Cos(argument);
            switch (gh)
            {
                case 3: x = 0;
                    y = 0;
                    z = 0;
                    break;
                case 4: xtemp = 0;
                    ytemp = 0;
                    ztemp = 0;
                    break;
                default:
                    break;
            }
            sd = 0.0;
            cd = 1.0;
            l = 1;
            n = 0;
            m = 1;
            npq = (nmax * (nmax + 3)) / 2;
            if (igdgc == 1)
            {
                aa = a2 * clat * clat;
                bb = b2 * slat * slat;
                cc = aa + bb;
                argument = cc;
                dd = Math.Sqrt(argument);
                argument = elev * (elev + 2.0 * dd) + (a2 * aa + b2 * bb) / cc;
                r = Math.Sqrt(argument);
                cd = (elev + dd) / r;
                sd = (a2 - b2) / dd * slat * clat / r;
                aa = slat;
                slat = slat * cd - clat * sd;
                clat = clat * cd + aa * sd;
            }
            ratio = earths_radius / r;
            argument = 3.0;
            aa = Math.Sqrt(argument);
            p[1] = 2.0 * slat;
            p[2] = 2.0 * clat;
            p[3] = 4.5 * slat * slat - 1.5;
            p[4] = 3.0 * aa * clat * slat;
            q[1] = -clat;
            q[2] = slat;
            q[3] = -3.0 * clat * slat;
            q[4] = aa * (slat * slat - clat * clat);
            for (k = 1; k <= npq; ++k)
            {
                if (n < m)
                {
                    m = 0;
                    n = n + 1;
                    argument = ratio;
                    power = n + 2;
                    rr = Math.Pow(argument, power);
                    fn = n;
                }
                fm = m;
                if (k >= 5)
                {
                    if (m == n)
                    {
                        argument = (1.0 - 0.5 / fm);
                        aa = Math.Sqrt(argument);
                        j = k - n - 1;
                        p[k] = (1.0 + 1.0 / fm) * aa * clat * p[j];
                        q[k] = aa * (clat * q[j] + slat / fm * p[j]);
                        sl[m] = sl[m - 1] * cl[1] + cl[m - 1] * sl[1];
                        cl[m] = cl[m - 1] * cl[1] - sl[m - 1] * sl[1];
                    }
                    else
                    {
                        argument = fn * fn - fm * fm;
                        aa = Math.Sqrt(argument);
                        argument = ((fn - 1.0) * (fn - 1.0)) - (fm * fm);
                        bb = Math.Sqrt(argument) / aa;
                        cc = (2.0 * fn - 1.0) / aa;
                        ii = k - n;
                        j = k - 2 * n + 1;
                        p[k] = (fn + 1.0) * (cc * slat / fn * p[ii] - bb / (fn - 1.0) * p[j]);
                        q[k] = cc * (slat * q[ii] - clat / fn * p[ii]) - bb * q[j];
                    }
                }
                switch (gh)
                {
                    case 3: aa = rr * gha[l];
                        break;
                    case 4: aa = rr * ghb[l];
                        break;
                    default:
                        break;
                }
                if (m == 0)
                {
                    switch (gh)
                    {
                        case 3: x = x + aa * q[k];
                            z = z - aa * p[k];
                            break;
                        case 4: xtemp = xtemp + aa * q[k];
                            ztemp = ztemp - aa * p[k];
                            break;
                        default:
                            break;
                    }
                    l = l + 1;
                }
                else
                {
                    switch (gh)
                    {
                        case 3: bb = rr * gha[l + 1];
                            cc = aa * cl[m] + bb * sl[m];
                            x = x + cc * q[k];
                            z = z - cc * p[k];
                            if (clat > 0)
                            {
                                y = y + (aa * sl[m] - bb * cl[m]) *
                                  fm * p[k] / ((fn + 1.0) * clat);
                            }
                            else
                            {
                                y = y + (aa * sl[m] - bb * cl[m]) * q[k] * slat;
                            }
                            l = l + 2;
                            break;
                        case 4: bb = rr * ghb[l + 1];
                            cc = aa * cl[m] + bb * sl[m];
                            xtemp = xtemp + cc * q[k];
                            ztemp = ztemp - cc * p[k];
                            if (clat > 0)
                            {
                                ytemp = ytemp + (aa * sl[m] - bb * cl[m]) *
                                  fm * p[k] / ((fn + 1.0) * clat);
                            }
                            else
                            {
                                ytemp = ytemp + (aa * sl[m] - bb * cl[m]) *
                                  q[k] * slat;
                            }
                            l = l + 2;
                            break;
                        default:
                            break;
                    }
                }
                m = m + 1;
            }
            if (iext != 0)
            {
                aa = ext2 * cl[1] + ext3 * sl[1];
                switch (gh)
                {
                    case 3: x = x - ext1 * clat + aa * slat;
                        y = y + ext2 * sl[1] - ext3 * cl[1];
                        z = z + ext1 * slat + aa * clat;
                        break;
                    case 4: xtemp = xtemp - ext1 * clat + aa * slat;
                        ytemp = ytemp + ext2 * sl[1] - ext3 * cl[1];
                        ztemp = ztemp + ext1 * slat + aa * clat;
                        break;
                    default:
                        break;
                }
            }
            switch (gh)
            {
                case 3: aa = x;
                    x = x * cd + z * sd;
                    z = z * cd - aa * sd;
                    break;
                case 4: aa = xtemp;
                    xtemp = xtemp * cd + ztemp * sd;
                    ztemp = ztemp * cd - aa * sd;
                    break;
                default:
                    break;
            }
            return (ios);
        }
         /****************************************************************************/
         /*                                                                          */
         /*                           Subroutine dihf                                */
         /*                                                                          */
         /****************************************************************************/
         /*                                                                          */
         /*     Computes the geomagnetic d, i, h, and f from x, y, and z.            */
         /*                                                                          */
         /*     Input:                                                               */
         /*           x  - northward component                                       */
         /*           y  - eastward component                                        */
         /*           z  - vertically-downward component                             */
         /*                                                                          */
         /*     Output:                                                              */
         /*           d  - declination                                               */
         /*           i  - inclination                                               */
         /*           h  - horizontal intensity                                      */
         /*           f  - total intensity                                           */
         private static int dihf(int gh)
         {
             int ios;
             int j;
             double sn;
             double h2;
             double hpx;
             double argument, argument2;

             ios = gh;
             sn = 0.0001;

             switch (gh)
             {
                 case 3: for (j = 1; j <= 1; ++j)
                     {
                         h2 = x * x + y * y;
                         argument = h2;
                         h = Math.Sqrt(argument);       /* calculate horizontal intensity */
                         argument = h2 + z * z;
                         f = Math.Sqrt(argument);      /* calculate total intensity */
                         if (f < sn)
                         {
                             d = Math.Log(-1);       /* If d and i cannot be determined, */
                             i = Math.Log(-1);        /*       set equal to NaN         */
                         }
                         else
                         {
                             argument = z;
                             argument2 = h;
                             i = Math.Atan2(argument, argument2);
                             if (h < sn)
                             {
                                 d = Math.Log(-1);
                             }
                             else
                             {
                                 hpx = h + x;
                                 if (hpx < sn)
                                 {
                                     d = Math.PI;
                                 }
                                 else
                                 {
                                     argument = y;
                                     argument2 = hpx;
                                     d = 2.0 * Math.Atan2(argument, argument2);
                                 }
                             }
                         }
                     }
                     break;
                 case 4: for (j = 1; j <= 1; ++j)
                     {
                         h2 = xtemp * xtemp + ytemp * ytemp;
                         argument = h2;
                         htemp = Math.Sqrt(argument);
                         argument = h2 + ztemp * ztemp;
                         ftemp = Math.Sqrt(argument);
                         if (ftemp < sn)
                         {
                             dtemp = Math.Log(-1);     /* If d and i cannot be determined, */
                             itemp = Math.Log(-1);     /*       set equal to 999.0         */
                         }
                         else
                         {
                             argument = ztemp;
                             argument2 = htemp;
                             itemp = Math.Atan2(argument, argument2);
                             if (htemp < sn)
                             {
                                 dtemp = Math.Log(-1);
                             }
                             else
                             {
                                 hpx = htemp + xtemp;
                                 if (hpx < sn)
                                 {
                                     dtemp = Math.PI;
                                 }
                                 else
                                 {
                                     argument = ytemp;
                                     argument2 = hpx;
                                     dtemp = 2.0 * Math.Atan2(argument, argument2);
                                 }
                             }
                         }
                     }
                     break;
                 default:
                     break;
             }
             return ios;
         }
         /****************************************************************************/
         /*                                                                          */
         /*                           Subroutine getshc                              */
         /*                                                                          */
         /****************************************************************************/
         /*                                                                          */
         /*     Reads spherical harmonic coefficients from the specified             */
         /*     model into an array.                                                 */
         /*                                                                          */
         /*     Input:                                                               */
         /*           stream     - Logical unit number                               */
         /*           iflag      - Flag for SV equal to ) or not equal to 0          */
         /*                        for designated read statements                    */
         /*           strec      - Starting record number to read from model         */
         /*           nmax_of_gh - Maximum degree and order of model                 */
         /*                                                                          */
         /*     Output:                                                              */
         /*           gh1 or 2   - Schmidt quasi-normal internal spherical           */
         /*                        harmonic coefficients                             */
         private static void getshc(List<string> igrf_array, int iflag, int line_num, int nmax_of_gh, int gh)
         {
             int ii, m, n, mm, nn;
             double g, hh;
             double trash;
             int num = line_num;
             string line = null;

             if (igrf_array.Count < 1)
             {
                 return;
             }
             else
             {
                 ii = 0;

                 for (nn = 1; nn <= nmax_of_gh; ++nn)
                 {
                     for (mm = 0; mm <= nn; ++mm)
                     {
                         line = igrf_array[num];

                         if (line.Substring(0, 4) == "IGRF" || line.Substring(0, 4) == "DGRF")
                         {
                             num++;
                             line = igrf_array[num];
                         }
                         line = System.Text.RegularExpressions.Regex.Replace(line, "\\s+", " ");
                         string[] array = line.Split(' ');
                         if (iflag == 1)
                         {
                             n = Convert.ToInt32(array[0]);
                             m = Convert.ToInt32(array[1]);
                             g = Convert.ToDouble(array[2]);
                             hh = Convert.ToDouble(array[3]);
                             trash = Convert.ToDouble(array[5]);
                         }
                         else
                         {
                             n = Convert.ToInt32(array[0]);
                             m = Convert.ToInt32(array[1]);
                             g = Convert.ToDouble(array[4]);
                             hh = Convert.ToDouble(array[5]);
                             trash = Convert.ToDouble(array[2]);

                         }
                         if ((nn != n) || (mm != m))
                         {
                             return;
                         }
                         ii = ii + 1;
                         switch (gh)
                         {
                             case 1: gh1[ii] = g;
                                 break;
                             case 2: gh2[ii] = g;
                                 break;
                             default:
                                 break;
                         }
                         if (m != 0)
                         {
                             ii = ii + 1;
                             switch (gh)
                             {
                                 case 1: gh1[ii] = hh;
                                     break;
                                 case 2: gh2[ii] = hh;
                                     break;
                                 default:
                                     break;
                             }
                         }
                         num++;
                     }
                 }
             }

         }

         private static double igrf_calculate(List<string> igrf_array, int igdgc, double tdate, double lat, double lon, double elev)
         {
             /* Control variables */
             const int IEXT = 0;
             const int EXT_COEFF1 = 0;
             const int EXT_COEFF2 = 0;
             const int EXT_COEFF3 = 0;
             double RAD2DEG = 180 / Math.PI;

             int warn_H = 0, warn_H_strong = 0, warn_P = 0;

             /* Which model (Index) */
             int nmodel = 0;             /* Number of models in file */
             int[] max1 = new int[MAXMOD];
             int[] max2 = new int[MAXMOD];
             int[] max3 = new int[MAXMOD];
             int[] irec_pos = new int[MAXMOD];


             int nmax = 0;

             double[] epoch = new double[MAXMOD];
             double[] yrmin = new double[MAXMOD];
             double[] yrmax = new double[MAXMOD];
             double[] altmin = new double[MAXMOD];
             double[] altmax = new double[MAXMOD];


             double minyr = 0.0;
             double maxyr = 0.0;

             double minalt;
             double maxalt;
             double sdate = -1;
             double latitude = 200;
             double longitude = 200;
             double ddot;
             double fdot;
             double hdot;
             double idot;
             double xdot;
             double ydot;
             double zdot;
             double warn_H_val, warn_H_strong_val;

             warn_H = 0;
             warn_H_val = 99999.0;
             warn_H_strong = 0;
             warn_H_strong_val = 99999.0;
             warn_P = 0;

             string line = null;
             int modelI = -1;
             int line_num = 0;
             for (int ii = 0; ii < igrf_array.Count; ii++)
             {
                 line_num++;
                 line = igrf_array[ii];
                 if (line.Substring(0, 4) == "IGRF" || line.Substring(0, 4) == "DGRF")
                 {
                     modelI++;
                     line = System.Text.RegularExpressions.Regex.Replace(line, "\\s+", " ");
                     string[] array = line.Split(' ');
                     epoch[modelI] = Convert.ToDouble(array[1]);
                     max1[modelI] = Convert.ToInt32(array[2]);
                     max2[modelI] = Convert.ToInt32(array[3]);
                     max3[modelI] = Convert.ToInt32(array[4]);
                     yrmin[modelI] = Convert.ToDouble(array[5]);
                     yrmax[modelI] = Convert.ToDouble(array[6]);
                     altmin[modelI] = Convert.ToDouble(array[7]);
                     altmax[modelI] = Convert.ToDouble(array[8]);
                     irec_pos[modelI] = line_num;
                     if (modelI == 0)                    /*If first model */
                     {
                         minyr = yrmin[0];
                         maxyr = yrmax[0];
                     }
                     else
                     {
                         if (yrmin[modelI] < minyr)
                         {
                             minyr = yrmin[modelI];
                         }
                         if (yrmax[modelI] > maxyr)
                         {
                             maxyr = yrmax[modelI];
                         }
                     }
                 }

             }
             nmodel = modelI + 1;
            
             sdate = tdate;
             for (modelI = 0; modelI < nmodel; modelI++)
                 if (sdate < yrmax[modelI]) break;
             if (modelI == nmodel) modelI--;           /* if beyond end of last model use last model */

             /* Get altitude min and max for selected model. */
             minalt = altmin[modelI];
             maxalt = altmax[modelI];

             latitude = lat;
             longitude = lon;
             if (max2[modelI] == 0)
             {
                 getshc(igrf_array, 1, irec_pos[modelI], max1[modelI], 1);
                 getshc(igrf_array, 1, irec_pos[modelI + 1], max1[modelI + 1], 2);
                 nmax = interpsh(sdate, yrmin[modelI], max1[modelI],
                                 yrmin[modelI + 1], max1[modelI + 1], 3);
                 nmax = interpsh(sdate + 1, yrmin[modelI], max1[modelI],
                                 yrmin[modelI + 1], max1[modelI + 1], 4);
             }
             else
             {
                 getshc(igrf_array, 1, irec_pos[modelI], max1[modelI], 1);
                 getshc(igrf_array, 0, irec_pos[modelI], max2[modelI], 2);
                 nmax = extrapsh(sdate, epoch[modelI], max1[modelI], max2[modelI], 3);
                 nmax = extrapsh(sdate + 1, epoch[modelI], max1[modelI], max2[modelI], 4);
             }

             double TF = 0;
             /* Do the first calculations */
             shval3(igdgc, latitude, longitude, elev, nmax, 3,
                    IEXT, EXT_COEFF1, EXT_COEFF2, EXT_COEFF3);
             dihf(3);
             shval3(igdgc, latitude, longitude, elev, nmax, 4,
                    IEXT, EXT_COEFF1, EXT_COEFF2, EXT_COEFF3);
             dihf(4);


             ddot = ((dtemp - d) * RAD2DEG);
             if (ddot > 180.0) ddot -= 360.0;
             if (ddot <= -180.0) ddot += 360.0;
             ddot *= 60.0;

             idot = ((itemp - i) * RAD2DEG) * 60;
             d = d * (RAD2DEG); i = i * (RAD2DEG);
             hdot = htemp - h; xdot = xtemp - x;
             ydot = ytemp - y; zdot = ztemp - z;
             fdot = ftemp - f;

             /* deal with geographic and magnetic poles */

             if (h < 100.0) /* at magnetic poles */
             {
                 d = Math.Log(-1);
                 ddot = Math.Log(-1);
                 /* while rest is ok */
             }

             if (h < 1000.0)
             {
                 warn_H = 0;
                 warn_H_strong = 1;
                 if (h < warn_H_strong_val) warn_H_strong_val = h;
             }
             else if (h < 5000.0 && warn_H_strong == 0)
             {
                 warn_H = 1;
                 if (h < warn_H_val) warn_H_val = h;
             }

             if (90.0 - Math.Abs(latitude) <= 0.001) /* at geographic poles */
             {
                 x = Math.Log(-1);
                 y = Math.Log(-1);
                 d = Math.Log(-1);
                 xdot = Math.Log(-1);
                 ydot = Math.Log(-1);
                 ddot = Math.Log(-1);
                 warn_P = 1;
                 warn_H = 0;
                 warn_H_strong = 0;
                 /* while rest is ok */
             }


             return TF;

         }
        #endregion

         #region IGFR
         private static List<string> igrf_cof = new List<string>(new string[]{
"IGRF00  1900.00 10  0  0 1900.00 1905.00   -1.0  600.0           IGRF00   0",
"1  0 -31543.00      0.00      0.00      0.00                         IGRF00   1",
"1  1  -2298.00   5922.00      0.00      0.00                         IGRF00   2",
"2  0   -677.00      0.00      0.00      0.00                         IGRF00   3",
"2  1   2905.00  -1061.00      0.00      0.00                         IGRF00   4",
"2  2    924.00   1121.00      0.00      0.00                         IGRF00   5",
"3  0   1022.00      0.00      0.00      0.00                         IGRF00   6",
"3  1  -1469.00   -330.00      0.00      0.00                         IGRF00   7",
"3  2   1256.00      3.00      0.00      0.00                         IGRF00   8",
"3  3    572.00    523.00      0.00      0.00                         IGRF00   9",
"4  0    876.00      0.00      0.00      0.00                         IGRF00  10",
"4  1    628.00    195.00      0.00      0.00                         IGRF00  11",
"4  2    660.00    -69.00      0.00      0.00                         IGRF00  12",
"4  3   -361.00   -210.00      0.00      0.00                         IGRF00  13",
"4  4    134.00    -75.00      0.00      0.00                         IGRF00  14",
"5  0   -184.00      0.00      0.00      0.00                         IGRF00  15",
"5  1    328.00   -210.00      0.00      0.00                         IGRF00  16",
"5  2    264.00     53.00      0.00      0.00                         IGRF00  17",
"5  3      5.00    -33.00      0.00      0.00                         IGRF00  18",
"5  4    -86.00   -124.00      0.00      0.00                         IGRF00  19",
"5  5    -16.00      3.00      0.00      0.00                         IGRF00  20",
"6  0     63.00      0.00      0.00      0.00                         IGRF00  21",
"6  1     61.00     -9.00      0.00      0.00                         IGRF00  22",
"6  2    -11.00     83.00      0.00      0.00                         IGRF00  23",
"6  3   -217.00      2.00      0.00      0.00                         IGRF00  24",
"6  4    -58.00    -35.00      0.00      0.00                         IGRF00  25",
"6  5     59.00     36.00      0.00      0.00                         IGRF00  26",
"6  6    -90.00    -69.00      0.00      0.00                         IGRF00  27",
"7  0     70.00      0.00      0.00      0.00                         IGRF00  28",
"7  1    -55.00    -45.00      0.00      0.00                         IGRF00  29",
"7  2      0.00    -13.00      0.00      0.00                         IGRF00  30",
"7  3     34.00    -10.00      0.00      0.00                         IGRF00  31",
"7  4    -41.00     -1.00      0.00      0.00                         IGRF00  32",
"7  5    -21.00     28.00      0.00      0.00                         IGRF00  33",
"7  6     18.00    -12.00      0.00      0.00                         IGRF00  34",
"7  7      6.00    -22.00      0.00      0.00                         IGRF00  35",
"8  0     11.00      0.00      0.00      0.00                         IGRF00  36",
"8  1      8.00      8.00      0.00      0.00                         IGRF00  37",
"8  2     -4.00    -14.00      0.00      0.00                         IGRF00  38",
"8  3     -9.00      7.00      0.00      0.00                         IGRF00  39",
"8  4      1.00    -13.00      0.00      0.00                         IGRF00  40",
"8  5      2.00      5.00      0.00      0.00                         IGRF00  41",
"8  6     -9.00     16.00      0.00      0.00                         IGRF00  42",
"8  7      5.00     -5.00      0.00      0.00                         IGRF00  43",
"8  8      8.00    -18.00      0.00      0.00                         IGRF00  44",
"9  0      8.00      0.00      0.00      0.00                         IGRF00  45",
"9  1     10.00    -20.00      0.00      0.00                         IGRF00  46",
"9  2      1.00     14.00      0.00      0.00                         IGRF00  47",
"9  3    -11.00      5.00      0.00      0.00                         IGRF00  48",
"9  4     12.00     -3.00      0.00      0.00                         IGRF00  49",
"9  5      1.00     -2.00      0.00      0.00                         IGRF00  50",
"9  6     -2.00      8.00      0.00      0.00                         IGRF00  51",
"9  7      2.00     10.00      0.00      0.00                         IGRF00  52",
"9  8     -1.00     -2.00      0.00      0.00                         IGRF00  53",
"9  9     -1.00      2.00      0.00      0.00                         IGRF00  54",
"10  0     -3.00      0.00      0.00      0.00                         IGRF00  55",
"10  1     -4.00      2.00      0.00      0.00                         IGRF00  56",
"10  2      2.00      1.00      0.00      0.00                         IGRF00  57",
"10  3     -5.00      2.00      0.00      0.00                         IGRF00  58",
"10  4     -2.00      6.00      0.00      0.00                         IGRF00  59",
"10  5      6.00     -4.00      0.00      0.00                         IGRF00  60",
"10  6      4.00      0.00      0.00      0.00                         IGRF00  61",
"10  7      0.00     -2.00      0.00      0.00                         IGRF00  62",
"10  8      2.00      4.00      0.00      0.00                         IGRF00  63",
"10  9      2.00      0.00      0.00      0.00                         IGRF00  64",
"10 10      0.00     -6.00      0.00      0.00                         IGRF00  65",
"IGRF05  1905.00 10  0  0 1905.00 1910.00   -1.0  600.0           IGRF05   0",
"1  0 -31464.00      0.00      0.00      0.00                         IGRF05   1",
"1  1  -2298.00   5909.00      0.00      0.00                         IGRF05   2",
"2  0   -728.00      0.00      0.00      0.00                         IGRF05   3",
"2  1   2928.00  -1086.00      0.00      0.00                         IGRF05   4",
"2  2   1041.00   1065.00      0.00      0.00                         IGRF05   5",
"3  0   1037.00      0.00      0.00      0.00                         IGRF05   6",
"3  1  -1494.00   -357.00      0.00      0.00                         IGRF05   7",
"3  2   1239.00     34.00      0.00      0.00                         IGRF05   8",
"3  3    635.00    480.00      0.00      0.00                         IGRF05   9",
"4  0    880.00      0.00      0.00      0.00                         IGRF05  10",
"4  1    643.00    203.00      0.00      0.00                         IGRF05  11",
"4  2    653.00    -77.00      0.00      0.00                         IGRF05  12",
"4  3   -380.00   -201.00      0.00      0.00                         IGRF05  13",
"4  4    146.00    -65.00      0.00      0.00                         IGRF05  14",
"5  0   -192.00      0.00      0.00      0.00                         IGRF05  15",
"5  1    328.00   -193.00      0.00      0.00                         IGRF05  16",
"5  2    259.00     56.00      0.00      0.00                         IGRF05  17",
"5  3     -1.00    -32.00      0.00      0.00                         IGRF05  18",
"5  4    -93.00   -125.00      0.00      0.00                         IGRF05  19",
"5  5    -26.00     11.00      0.00      0.00                         IGRF05  20",
"6  0     62.00      0.00      0.00      0.00                         IGRF05  21",
"6  1     60.00     -7.00      0.00      0.00                         IGRF05  22",
"6  2    -11.00     86.00      0.00      0.00                         IGRF05  23",
"6  3   -221.00      4.00      0.00      0.00                         IGRF05  24",
"6  4    -57.00    -32.00      0.00      0.00                         IGRF05  25",
"6  5     57.00     32.00      0.00      0.00                         IGRF05  26",
"6  6    -92.00    -67.00      0.00      0.00                         IGRF05  27",
"7  0     70.00      0.00      0.00      0.00                         IGRF05  28",
"7  1    -54.00    -46.00      0.00      0.00                         IGRF05  29",
"7  2      0.00    -14.00      0.00      0.00                         IGRF05  30",
"7  3     33.00    -11.00      0.00      0.00                         IGRF05  31",
"7  4    -41.00      0.00      0.00      0.00                         IGRF05  32",
"7  5    -20.00     28.00      0.00      0.00                         IGRF05  33",
"7  6     18.00    -12.00      0.00      0.00                         IGRF05  34",
"7  7      6.00    -22.00      0.00      0.00                         IGRF05  35",
"8  0     11.00      0.00      0.00      0.00                         IGRF05  36",
"8  1      8.00      8.00      0.00      0.00                         IGRF05  37",
"8  2     -4.00    -15.00      0.00      0.00                         IGRF05  38",
"8  3     -9.00      7.00      0.00      0.00                         IGRF05  39",
"8  4      1.00    -13.00      0.00      0.00                         IGRF05  40",
"8  5      2.00      5.00      0.00      0.00                         IGRF05  41",
"8  6     -8.00     16.00      0.00      0.00                         IGRF05  42",
"8  7      5.00     -5.00      0.00      0.00                         IGRF05  43",
"8  8      8.00    -18.00      0.00      0.00                         IGRF05  44",
"9  0      8.00      0.00      0.00      0.00                         IGRF05  45",
"9  1     10.00    -20.00      0.00      0.00                         IGRF05  46",
"9  2      1.00     14.00      0.00      0.00                         IGRF05  47",
"9  3    -11.00      5.00      0.00      0.00                         IGRF05  48",
"9  4     12.00     -3.00      0.00      0.00                         IGRF05  49",
"9  5      1.00     -2.00      0.00      0.00                         IGRF05  50",
"9  6     -2.00      8.00      0.00      0.00                         IGRF05  51",
"9  7      2.00     10.00      0.00      0.00                         IGRF05  52",
"9  8      0.00     -2.00      0.00      0.00                         IGRF05  53",
"9  9     -1.00      2.00      0.00      0.00                         IGRF05  54",
"10  0     -3.00      0.00      0.00      0.00                         IGRF05  55",
"10  1     -4.00      2.00      0.00      0.00                         IGRF05  56",
"10  2      2.00      1.00      0.00      0.00                         IGRF05  57",
"10  3     -5.00      2.00      0.00      0.00                         IGRF05  58",
"10  4     -2.00      6.00      0.00      0.00                         IGRF05  59",
"10  5      6.00     -4.00      0.00      0.00                         IGRF05  60",
"10  6      4.00      0.00      0.00      0.00                         IGRF05  61",
"10  7      0.00     -2.00      0.00      0.00                         IGRF05  62",
"10  8      2.00      4.00      0.00      0.00                         IGRF05  63",
"10  9      2.00      0.00      0.00      0.00                         IGRF05  64",
"10 10      0.00     -6.00      0.00      0.00                         IGRF05  65",
"IGRF10  1910.00 10  0  0 1910.00 1915.00   -1.0  600.0           IGRF10   0",
"1  0 -31354.00      0.00      0.00      0.00                         IGRF10   1",
"1  1  -2297.00   5898.00      0.00      0.00                         IGRF10   2",
"2  0   -769.00      0.00      0.00      0.00                         IGRF10   3",
"2  1   2948.00  -1128.00      0.00      0.00                         IGRF10   4",
"2  2   1176.00   1000.00      0.00      0.00                         IGRF10   5",
"3  0   1058.00      0.00      0.00      0.00                         IGRF10   6",
"3  1  -1524.00   -389.00      0.00      0.00                         IGRF10   7",
"3  2   1223.00     62.00      0.00      0.00                         IGRF10   8",
"3  3    705.00    425.00      0.00      0.00                         IGRF10   9",
"4  0    884.00      0.00      0.00      0.00                         IGRF10  10",
"4  1    660.00    211.00      0.00      0.00                         IGRF10  11",
"4  2    644.00    -90.00      0.00      0.00                         IGRF10  12",
"4  3   -400.00   -189.00      0.00      0.00                         IGRF10  13",
"4  4    160.00    -55.00      0.00      0.00                         IGRF10  14",
"5  0   -201.00      0.00      0.00      0.00                         IGRF10  15",
"5  1    327.00   -172.00      0.00      0.00                         IGRF10  16",
"5  2    253.00     57.00      0.00      0.00                         IGRF10  17",
"5  3     -9.00    -33.00      0.00      0.00                         IGRF10  18",
"5  4   -102.00   -126.00      0.00      0.00                         IGRF10  19",
"5  5    -38.00     21.00      0.00      0.00                         IGRF10  20",
"6  0     62.00      0.00      0.00      0.00                         IGRF10  21",
"6  1     58.00     -5.00      0.00      0.00                         IGRF10  22",
"6  2    -11.00     89.00      0.00      0.00                         IGRF10  23",
"6  3   -224.00      5.00      0.00      0.00                         IGRF10  24",
"6  4    -54.00    -29.00      0.00      0.00                         IGRF10  25",
"6  5     54.00     28.00      0.00      0.00                         IGRF10  26",
"6  6    -95.00    -65.00      0.00      0.00                         IGRF10  27",
"7  0     71.00      0.00      0.00      0.00                         IGRF10  28",
"7  1    -54.00    -47.00      0.00      0.00                         IGRF10  29",
"7  2      1.00    -14.00      0.00      0.00                         IGRF10  30",
"7  3     32.00    -12.00      0.00      0.00                         IGRF10  31",
"7  4    -40.00      1.00      0.00      0.00                         IGRF10  32",
"7  5    -19.00     28.00      0.00      0.00                         IGRF10  33",
"7  6     18.00    -13.00      0.00      0.00                         IGRF10  34",
"7  7      6.00    -22.00      0.00      0.00                         IGRF10  35",
"8  0     11.00      0.00      0.00      0.00                         IGRF10  36",
"8  1      8.00      8.00      0.00      0.00                         IGRF10  37",
"8  2     -4.00    -15.00      0.00      0.00                         IGRF10  38",
"8  3     -9.00      6.00      0.00      0.00                         IGRF10  39",
"8  4      1.00    -13.00      0.00      0.00                         IGRF10  40",
"8  5      2.00      5.00      0.00      0.00                         IGRF10  41",
"8  6     -8.00     16.00      0.00      0.00                         IGRF10  42",
"8  7      5.00     -5.00      0.00      0.00                         IGRF10  43",
"8  8      8.00    -18.00      0.00      0.00                         IGRF10  44",
"9  0      8.00      0.00      0.00      0.00                         IGRF10  45",
"9  1     10.00    -20.00      0.00      0.00                         IGRF10  46",
"9  2      1.00     14.00      0.00      0.00                         IGRF10  47",
"9  3    -11.00      5.00      0.00      0.00                         IGRF10  48",
"9  4     12.00     -3.00      0.00      0.00                         IGRF10  49",
"9  5      1.00     -2.00      0.00      0.00                         IGRF10  50",
"9  6     -2.00      8.00      0.00      0.00                         IGRF10  51",
"9  7      2.00     10.00      0.00      0.00                         IGRF10  52",
"9  8      0.00     -2.00      0.00      0.00                         IGRF10  53",
"9  9     -1.00      2.00      0.00      0.00                         IGRF10  54",
"10  0     -3.00      0.00      0.00      0.00                         IGRF10  55",
"10  1     -4.00      2.00      0.00      0.00                         IGRF10  56",
"10  2      2.00      1.00      0.00      0.00                         IGRF10  57",
"10  3     -5.00      2.00      0.00      0.00                         IGRF10  58",
"10  4     -2.00      6.00      0.00      0.00                         IGRF10  59",
"10  5      6.00     -4.00      0.00      0.00                         IGRF10  60",
"10  6      4.00      0.00      0.00      0.00                         IGRF10  61",
"10  7      0.00     -2.00      0.00      0.00                         IGRF10  62",
"10  8      2.00      4.00      0.00      0.00                         IGRF10  63",
"10  9      2.00      0.00      0.00      0.00                         IGRF10  64",
"10 10      0.00     -6.00      0.00      0.00                         IGRF10  65",
"IGRF15  1915.00 10  0  0 1915.00 1920.00   -1.0  600.0           IGRF15   0",
"1  0 -31212.00      0.00      0.00      0.00                         IGRF15   1",
"1  1  -2306.00   5875.00      0.00      0.00                         IGRF15   2",
"2  0   -802.00      0.00      0.00      0.00                         IGRF15   3",
"2  1   2956.00  -1191.00      0.00      0.00                         IGRF15   4",
"2  2   1309.00    917.00      0.00      0.00                         IGRF15   5",
"3  0   1084.00      0.00      0.00      0.00                         IGRF15   6",
"3  1  -1559.00   -421.00      0.00      0.00                         IGRF15   7",
"3  2   1212.00     84.00      0.00      0.00                         IGRF15   8",
"3  3    778.00    360.00      0.00      0.00                         IGRF15   9",
"4  0    887.00      0.00      0.00      0.00                         IGRF15  10",
"4  1    678.00    218.00      0.00      0.00                         IGRF15  11",
"4  2    631.00   -109.00      0.00      0.00                         IGRF15  12",
"4  3   -416.00   -173.00      0.00      0.00                         IGRF15  13",
"4  4    178.00    -51.00      0.00      0.00                         IGRF15  14",
"5  0   -211.00      0.00      0.00      0.00                         IGRF15  15",
"5  1    327.00   -148.00      0.00      0.00                         IGRF15  16",
"5  2    245.00     58.00      0.00      0.00                         IGRF15  17",
"5  3    -16.00    -34.00      0.00      0.00                         IGRF15  18",
"5  4   -111.00   -126.00      0.00      0.00                         IGRF15  19",
"5  5    -51.00     32.00      0.00      0.00                         IGRF15  20",
"6  0     61.00      0.00      0.00      0.00                         IGRF15  21",
"6  1     57.00     -2.00      0.00      0.00                         IGRF15  22",
"6  2    -10.00     93.00      0.00      0.00                         IGRF15  23",
"6  3   -228.00      8.00      0.00      0.00                         IGRF15  24",
"6  4    -51.00    -26.00      0.00      0.00                         IGRF15  25",
"6  5     49.00     23.00      0.00      0.00                         IGRF15  26",
"6  6    -98.00    -62.00      0.00      0.00                         IGRF15  27",
"7  0     72.00      0.00      0.00      0.00                         IGRF15  28",
"7  1    -54.00    -48.00      0.00      0.00                         IGRF15  29",
"7  2      2.00    -14.00      0.00      0.00                         IGRF15  30",
"7  3     31.00    -12.00      0.00      0.00                         IGRF15  31",
"7  4    -38.00      2.00      0.00      0.00                         IGRF15  32",
"7  5    -18.00     28.00      0.00      0.00                         IGRF15  33",
"7  6     19.00    -15.00      0.00      0.00                         IGRF15  34",
"7  7      6.00    -22.00      0.00      0.00                         IGRF15  35",
"8  0     11.00      0.00      0.00      0.00                         IGRF15  36",
"8  1      8.00      8.00      0.00      0.00                         IGRF15  37",
"8  2     -4.00    -15.00      0.00      0.00                         IGRF15  38",
"8  3     -9.00      6.00      0.00      0.00                         IGRF15  39",
"8  4      2.00    -13.00      0.00      0.00                         IGRF15  40",
"8  5      3.00      5.00      0.00      0.00                         IGRF15  41",
"8  6     -8.00     16.00      0.00      0.00                         IGRF15  42",
"8  7      6.00     -5.00      0.00      0.00                         IGRF15  43",
"8  8      8.00    -18.00      0.00      0.00                         IGRF15  44",
"9  0      8.00      0.00      0.00      0.00                         IGRF15  45",
"9  1     10.00    -20.00      0.00      0.00                         IGRF15  46",
"9  2      1.00     14.00      0.00      0.00                         IGRF15  47",
"9  3    -11.00      5.00      0.00      0.00                         IGRF15  48",
"9  4     12.00     -3.00      0.00      0.00                         IGRF15  49",
"9  5      1.00     -2.00      0.00      0.00                         IGRF15  50",
"9  6     -2.00      8.00      0.00      0.00                         IGRF15  51",
"9  7      2.00     10.00      0.00      0.00                         IGRF15  52",
"9  8      0.00     -2.00      0.00      0.00                         IGRF15  53",
"9  9     -1.00      2.00      0.00      0.00                         IGRF15  54",
"10  0     -3.00      0.00      0.00      0.00                         IGRF15  55",
"10  1     -4.00      2.00      0.00      0.00                         IGRF15  56",
"10  2      2.00      1.00      0.00      0.00                         IGRF15  57",
"10  3     -5.00      2.00      0.00      0.00                         IGRF15  58",
"10  4     -2.00      6.00      0.00      0.00                         IGRF15  59",
"10  5      6.00     -4.00      0.00      0.00                         IGRF15  60",
"10  6      4.00      0.00      0.00      0.00                         IGRF15  61",
"10  7      0.00     -2.00      0.00      0.00                         IGRF15  62",
"10  8      1.00      4.00      0.00      0.00                         IGRF15  63",
"10  9      2.00      0.00      0.00      0.00                         IGRF15  64",
"10 10      0.00     -6.00      0.00      0.00                         IGRF15  65",
"IGRF20  1920.00 10  0  0 1920.00 1925.00   -1.0  600.0           IGRF20   0",
"1  0 -31060.00      0.00      0.00      0.00                         IGRF20   1",
"1  1  -2317.00   5845.00      0.00      0.00                         IGRF20   2",
"2  0   -839.00      0.00      0.00      0.00                         IGRF20   3",
"2  1   2959.00  -1259.00      0.00      0.00                         IGRF20   4",
"2  2   1407.00    823.00      0.00      0.00                         IGRF20   5",
"3  0   1111.00      0.00      0.00      0.00                         IGRF20   6",
"3  1  -1600.00   -445.00      0.00      0.00                         IGRF20   7",
"3  2   1205.00    103.00      0.00      0.00                         IGRF20   8",
"3  3    839.00    293.00      0.00      0.00                         IGRF20   9",
"4  0    889.00      0.00      0.00      0.00                         IGRF20  10",
"4  1    695.00    220.00      0.00      0.00                         IGRF20  11",
"4  2    616.00   -134.00      0.00      0.00                         IGRF20  12",
"4  3   -424.00   -153.00      0.00      0.00                         IGRF20  13",
"4  4    199.00    -57.00      0.00      0.00                         IGRF20  14",
"5  0   -221.00      0.00      0.00      0.00                         IGRF20  15",
"5  1    326.00   -122.00      0.00      0.00                         IGRF20  16",
"5  2    236.00     58.00      0.00      0.00                         IGRF20  17",
"5  3    -23.00    -38.00      0.00      0.00                         IGRF20  18",
"5  4   -119.00   -125.00      0.00      0.00                         IGRF20  19",
"5  5    -62.00     43.00      0.00      0.00                         IGRF20  20",
"6  0     61.00      0.00      0.00      0.00                         IGRF20  21",
"6  1     55.00      0.00      0.00      0.00                         IGRF20  22",
"6  2    -10.00     96.00      0.00      0.00                         IGRF20  23",
"6  3   -233.00     11.00      0.00      0.00                         IGRF20  24",
"6  4    -46.00    -22.00      0.00      0.00                         IGRF20  25",
"6  5     44.00     18.00      0.00      0.00                         IGRF20  26",
"6  6   -101.00    -57.00      0.00      0.00                         IGRF20  27",
"7  0     73.00      0.00      0.00      0.00                         IGRF20  28",
"7  1    -54.00    -49.00      0.00      0.00                         IGRF20  29",
"7  2      2.00    -14.00      0.00      0.00                         IGRF20  30",
"7  3     29.00    -13.00      0.00      0.00                         IGRF20  31",
"7  4    -37.00      4.00      0.00      0.00                         IGRF20  32",
"7  5    -16.00     28.00      0.00      0.00                         IGRF20  33",
"7  6     19.00    -16.00      0.00      0.00                         IGRF20  34",
"7  7      6.00    -22.00      0.00      0.00                         IGRF20  35",
"8  0     11.00      0.00      0.00      0.00                         IGRF20  36",
"8  1      7.00      8.00      0.00      0.00                         IGRF20  37",
"8  2     -3.00    -15.00      0.00      0.00                         IGRF20  38",
"8  3     -9.00      6.00      0.00      0.00                         IGRF20  39",
"8  4      2.00    -14.00      0.00      0.00                         IGRF20  40",
"8  5      4.00      5.00      0.00      0.00                         IGRF20  41",
"8  6     -7.00     17.00      0.00      0.00                         IGRF20  42",
"8  7      6.00     -5.00      0.00      0.00                         IGRF20  43",
"8  8      8.00    -19.00      0.00      0.00                         IGRF20  44",
"9  0      8.00      0.00      0.00      0.00                         IGRF20  45",
"9  1     10.00    -20.00      0.00      0.00                         IGRF20  46",
"9  2      1.00     14.00      0.00      0.00                         IGRF20  47",
"9  3    -11.00      5.00      0.00      0.00                         IGRF20  48",
"9  4     12.00     -3.00      0.00      0.00                         IGRF20  49",
"9  5      1.00     -2.00      0.00      0.00                         IGRF20  50",
"9  6     -2.00      9.00      0.00      0.00                         IGRF20  51",
"9  7      2.00     10.00      0.00      0.00                         IGRF20  52",
"9  8      0.00     -2.00      0.00      0.00                         IGRF20  53",
"9  9     -1.00      2.00      0.00      0.00                         IGRF20  54",
"10  0     -3.00      0.00      0.00      0.00                         IGRF20  55",
"10  1     -4.00      2.00      0.00      0.00                         IGRF20  56",
"10  2      2.00      1.00      0.00      0.00                         IGRF20  57",
"10  3     -5.00      2.00      0.00      0.00                         IGRF20  58",
"10  4     -2.00      6.00      0.00      0.00                         IGRF20  59",
"10  5      6.00     -4.00      0.00      0.00                         IGRF20  60",
"10  6      4.00      0.00      0.00      0.00                         IGRF20  61",
"10  7      0.00     -2.00      0.00      0.00                         IGRF20  62",
"10  8      1.00      4.00      0.00      0.00                         IGRF20  63",
"10  9      3.00      0.00      0.00      0.00                         IGRF20  64",
"10 10      0.00     -6.00      0.00      0.00                         IGRF20  65",
"IGRF25  1925.00 10  0  0 1925.00 1930.00   -1.0  600.0           IGRF25   0",
"1  0 -30926.00      0.00      0.00      0.00                         IGRF25   1",
"1  1  -2318.00   5817.00      0.00      0.00                         IGRF25   2",
"2  0   -893.00      0.00      0.00      0.00                         IGRF25   3",
"2  1   2969.00  -1334.00      0.00      0.00                         IGRF25   4",
"2  2   1471.00    728.00      0.00      0.00                         IGRF25   5",
"3  0   1140.00      0.00      0.00      0.00                         IGRF25   6",
"3  1  -1645.00   -462.00      0.00      0.00                         IGRF25   7",
"3  2   1202.00    119.00      0.00      0.00                         IGRF25   8",
"3  3    881.00    229.00      0.00      0.00                         IGRF25   9",
"4  0    891.00      0.00      0.00      0.00                         IGRF25  10",
"4  1    711.00    216.00      0.00      0.00                         IGRF25  11",
"4  2    601.00   -163.00      0.00      0.00                         IGRF25  12",
"4  3   -426.00   -130.00      0.00      0.00                         IGRF25  13",
"4  4    217.00    -70.00      0.00      0.00                         IGRF25  14",
"5  0   -230.00      0.00      0.00      0.00                         IGRF25  15",
"5  1    326.00    -96.00      0.00      0.00                         IGRF25  16",
"5  2    226.00     58.00      0.00      0.00                         IGRF25  17",
"5  3    -28.00    -44.00      0.00      0.00                         IGRF25  18",
"5  4   -125.00   -122.00      0.00      0.00                         IGRF25  19",
"5  5    -69.00     51.00      0.00      0.00                         IGRF25  20",
"6  0     61.00      0.00      0.00      0.00                         IGRF25  21",
"6  1     54.00      3.00      0.00      0.00                         IGRF25  22",
"6  2     -9.00     99.00      0.00      0.00                         IGRF25  23",
"6  3   -238.00     14.00      0.00      0.00                         IGRF25  24",
"6  4    -40.00    -18.00      0.00      0.00                         IGRF25  25",
"6  5     39.00     13.00      0.00      0.00                         IGRF25  26",
"6  6   -103.00    -52.00      0.00      0.00                         IGRF25  27",
"7  0     73.00      0.00      0.00      0.00                         IGRF25  28",
"7  1    -54.00    -50.00      0.00      0.00                         IGRF25  29",
"7  2      3.00    -14.00      0.00      0.00                         IGRF25  30",
"7  3     27.00    -14.00      0.00      0.00                         IGRF25  31",
"7  4    -35.00      5.00      0.00      0.00                         IGRF25  32",
"7  5    -14.00     29.00      0.00      0.00                         IGRF25  33",
"7  6     19.00    -17.00      0.00      0.00                         IGRF25  34",
"7  7      6.00    -21.00      0.00      0.00                         IGRF25  35",
"8  0     11.00      0.00      0.00      0.00                         IGRF25  36",
"8  1      7.00      8.00      0.00      0.00                         IGRF25  37",
"8  2     -3.00    -15.00      0.00      0.00                         IGRF25  38",
"8  3     -9.00      6.00      0.00      0.00                         IGRF25  39",
"8  4      2.00    -14.00      0.00      0.00                         IGRF25  40",
"8  5      4.00      5.00      0.00      0.00                         IGRF25  41",
"8  6     -7.00     17.00      0.00      0.00                         IGRF25  42",
"8  7      7.00     -5.00      0.00      0.00                         IGRF25  43",
"8  8      8.00    -19.00      0.00      0.00                         IGRF25  44",
"9  0      8.00      0.00      0.00      0.00                         IGRF25  45",
"9  1     10.00    -20.00      0.00      0.00                         IGRF25  46",
"9  2      1.00     14.00      0.00      0.00                         IGRF25  47",
"9  3    -11.00      5.00      0.00      0.00                         IGRF25  48",
"9  4     12.00     -3.00      0.00      0.00                         IGRF25  49",
"9  5      1.00     -2.00      0.00      0.00                         IGRF25  50",
"9  6     -2.00      9.00      0.00      0.00                         IGRF25  51",
"9  7      2.00     10.00      0.00      0.00                         IGRF25  52",
"9  8      0.00     -2.00      0.00      0.00                         IGRF25  53",
"9  9     -1.00      2.00      0.00      0.00                         IGRF25  54",
"10  0     -3.00      0.00      0.00      0.00                         IGRF25  55",
"10  1     -4.00      2.00      0.00      0.00                         IGRF25  56",
"10  2      2.00      1.00      0.00      0.00                         IGRF25  57",
"10  3     -5.00      2.00      0.00      0.00                         IGRF25  58",
"10  4     -2.00      6.00      0.00      0.00                         IGRF25  59",
"10  5      6.00     -4.00      0.00      0.00                         IGRF25  60",
"10  6      4.00      0.00      0.00      0.00                         IGRF25  61",
"10  7      0.00     -2.00      0.00      0.00                         IGRF25  62",
"10  8      1.00      4.00      0.00      0.00                         IGRF25  63",
"10  9      3.00      0.00      0.00      0.00                         IGRF25  64",
"10 10      0.00     -6.00      0.00      0.00                         IGRF25  65",
"IGRF30  1930.00 10  0  0 1930.00 1935.00   -1.0  600.0           IGRF30   0",
"1  0 -30805.00      0.00      0.00      0.00                         IGRF30   1",
"1  1  -2316.00   5808.00      0.00      0.00                         IGRF30   2",
"2  0   -951.00      0.00      0.00      0.00                         IGRF30   3",
"2  1   2980.00  -1424.00      0.00      0.00                         IGRF30   4",
"2  2   1517.00    644.00      0.00      0.00                         IGRF30   5",
"3  0   1172.00      0.00      0.00      0.00                         IGRF30   6",
"3  1  -1692.00   -480.00      0.00      0.00                         IGRF30   7",
"3  2   1205.00    133.00      0.00      0.00                         IGRF30   8",
"3  3    907.00    166.00      0.00      0.00                         IGRF30   9",
"4  0    896.00      0.00      0.00      0.00                         IGRF30  10",
"4  1    727.00    205.00      0.00      0.00                         IGRF30  11",
"4  2    584.00   -195.00      0.00      0.00                         IGRF30  12",
"4  3   -422.00   -109.00      0.00      0.00                         IGRF30  13",
"4  4    234.00    -90.00      0.00      0.00                         IGRF30  14",
"5  0   -237.00      0.00      0.00      0.00                         IGRF30  15",
"5  1    327.00    -72.00      0.00      0.00                         IGRF30  16",
"5  2    218.00     60.00      0.00      0.00                         IGRF30  17",
"5  3    -32.00    -53.00      0.00      0.00                         IGRF30  18",
"5  4   -131.00   -118.00      0.00      0.00                         IGRF30  19",
"5  5    -74.00     58.00      0.00      0.00                         IGRF30  20",
"6  0     60.00      0.00      0.00      0.00                         IGRF30  21",
"6  1     53.00      4.00      0.00      0.00                         IGRF30  22",
"6  2     -9.00    102.00      0.00      0.00                         IGRF30  23",
"6  3   -242.00     19.00      0.00      0.00                         IGRF30  24",
"6  4    -32.00    -16.00      0.00      0.00                         IGRF30  25",
"6  5     32.00      8.00      0.00      0.00                         IGRF30  26",
"6  6   -104.00    -46.00      0.00      0.00                         IGRF30  27",
"7  0     74.00      0.00      0.00      0.00                         IGRF30  28",
"7  1    -54.00    -51.00      0.00      0.00                         IGRF30  29",
"7  2      4.00    -15.00      0.00      0.00                         IGRF30  30",
"7  3     25.00    -14.00      0.00      0.00                         IGRF30  31",
"7  4    -34.00      6.00      0.00      0.00                         IGRF30  32",
"7  5    -12.00     29.00      0.00      0.00                         IGRF30  33",
"7  6     18.00    -18.00      0.00      0.00                         IGRF30  34",
"7  7      6.00    -20.00      0.00      0.00                         IGRF30  35",
"8  0     11.00      0.00      0.00      0.00                         IGRF30  36",
"8  1      7.00      8.00      0.00      0.00                         IGRF30  37",
"8  2     -3.00    -15.00      0.00      0.00                         IGRF30  38",
"8  3     -9.00      5.00      0.00      0.00                         IGRF30  39",
"8  4      2.00    -14.00      0.00      0.00                         IGRF30  40",
"8  5      5.00      5.00      0.00      0.00                         IGRF30  41",
"8  6     -6.00     18.00      0.00      0.00                         IGRF30  42",
"8  7      8.00     -5.00      0.00      0.00                         IGRF30  43",
"8  8      8.00    -19.00      0.00      0.00                         IGRF30  44",
"9  0      8.00      0.00      0.00      0.00                         IGRF30  45",
"9  1     10.00    -20.00      0.00      0.00                         IGRF30  46",
"9  2      1.00     14.00      0.00      0.00                         IGRF30  47",
"9  3    -12.00      5.00      0.00      0.00                         IGRF30  48",
"9  4     12.00     -3.00      0.00      0.00                         IGRF30  49",
"9  5      1.00     -2.00      0.00      0.00                         IGRF30  50",
"9  6     -2.00      9.00      0.00      0.00                         IGRF30  51",
"9  7      3.00     10.00      0.00      0.00                         IGRF30  52",
"9  8      0.00     -2.00      0.00      0.00                         IGRF30  53",
"9  9     -2.00      2.00      0.00      0.00                         IGRF30  54",
"10  0     -3.00      0.00      0.00      0.00                         IGRF30  55",
"10  1     -4.00      2.00      0.00      0.00                         IGRF30  56",
"10  2      2.00      1.00      0.00      0.00                         IGRF30  57",
"10  3     -5.00      2.00      0.00      0.00                         IGRF30  58",
"10  4     -2.00      6.00      0.00      0.00                         IGRF30  59",
"10  5      6.00     -4.00      0.00      0.00                         IGRF30  60",
"10  6      4.00      0.00      0.00      0.00                         IGRF30  61",
"10  7      0.00     -2.00      0.00      0.00                         IGRF30  62",
"10  8      1.00      4.00      0.00      0.00                         IGRF30  63",
"10  9      3.00      0.00      0.00      0.00                         IGRF30  64",
"10 10      0.00     -6.00      0.00      0.00                         IGRF30  65",
"IGRF35  1935.00 10  0  0 1935.00 1940.00   -1.0  600.0           IGRF35   0",
"1  0 -30715.00      0.00      0.00      0.00                         IGRF35   1",
"1  1  -2306.00   5812.00      0.00      0.00                         IGRF35   2",
"2  0  -1018.00      0.00      0.00      0.00                         IGRF35   3",
"2  1   2984.00  -1520.00      0.00      0.00                         IGRF35   4",
"2  2   1550.00    586.00      0.00      0.00                         IGRF35   5",
"3  0   1206.00      0.00      0.00      0.00                         IGRF35   6",
"3  1  -1740.00   -494.00      0.00      0.00                         IGRF35   7",
"3  2   1215.00    146.00      0.00      0.00                         IGRF35   8",
"3  3    918.00    101.00      0.00      0.00                         IGRF35   9",
"4  0    903.00      0.00      0.00      0.00                         IGRF35  10",
"4  1    744.00    188.00      0.00      0.00                         IGRF35  11",
"4  2    565.00   -226.00      0.00      0.00                         IGRF35  12",
"4  3   -415.00    -90.00      0.00      0.00                         IGRF35  13",
"4  4    249.00   -114.00      0.00      0.00                         IGRF35  14",
"5  0   -241.00      0.00      0.00      0.00                         IGRF35  15",
"5  1    329.00    -51.00      0.00      0.00                         IGRF35  16",
"5  2    211.00     64.00      0.00      0.00                         IGRF35  17",
"5  3    -33.00    -64.00      0.00      0.00                         IGRF35  18",
"5  4   -136.00   -115.00      0.00      0.00                         IGRF35  19",
"5  5    -76.00     64.00      0.00      0.00                         IGRF35  20",
"6  0     59.00      0.00      0.00      0.00                         IGRF35  21",
"6  1     53.00      4.00      0.00      0.00                         IGRF35  22",
"6  2     -8.00    104.00      0.00      0.00                         IGRF35  23",
"6  3   -246.00     25.00      0.00      0.00                         IGRF35  24",
"6  4    -25.00    -15.00      0.00      0.00                         IGRF35  25",
"6  5     25.00      4.00      0.00      0.00                         IGRF35  26",
"6  6   -106.00    -40.00      0.00      0.00                         IGRF35  27",
"7  0     74.00      0.00      0.00      0.00                         IGRF35  28",
"7  1    -53.00    -52.00      0.00      0.00                         IGRF35  29",
"7  2      4.00    -17.00      0.00      0.00                         IGRF35  30",
"7  3     23.00    -14.00      0.00      0.00                         IGRF35  31",
"7  4    -33.00      7.00      0.00      0.00                         IGRF35  32",
"7  5    -11.00     29.00      0.00      0.00                         IGRF35  33",
"7  6     18.00    -19.00      0.00      0.00                         IGRF35  34",
"7  7      6.00    -19.00      0.00      0.00                         IGRF35  35",
"8  0     11.00      0.00      0.00      0.00                         IGRF35  36",
"8  1      7.00      8.00      0.00      0.00                         IGRF35  37",
"8  2     -3.00    -15.00      0.00      0.00                         IGRF35  38",
"8  3     -9.00      5.00      0.00      0.00                         IGRF35  39",
"8  4      1.00    -15.00      0.00      0.00                         IGRF35  40",
"8  5      6.00      5.00      0.00      0.00                         IGRF35  41",
"8  6     -6.00     18.00      0.00      0.00                         IGRF35  42",
"8  7      8.00     -5.00      0.00      0.00                         IGRF35  43",
"8  8      7.00    -19.00      0.00      0.00                         IGRF35  44",
"9  0      8.00      0.00      0.00      0.00                         IGRF35  45",
"9  1     10.00    -20.00      0.00      0.00                         IGRF35  46",
"9  2      1.00     15.00      0.00      0.00                         IGRF35  47",
"9  3    -12.00      5.00      0.00      0.00                         IGRF35  48",
"9  4     11.00     -3.00      0.00      0.00                         IGRF35  49",
"9  5      1.00     -3.00      0.00      0.00                         IGRF35  50",
"9  6     -2.00      9.00      0.00      0.00                         IGRF35  51",
"9  7      3.00     11.00      0.00      0.00                         IGRF35  52",
"9  8      0.00     -2.00      0.00      0.00                         IGRF35  53",
"9  9     -2.00      2.00      0.00      0.00                         IGRF35  54",
"10  0     -3.00      0.00      0.00      0.00                         IGRF35  55",
"10  1     -4.00      2.00      0.00      0.00                         IGRF35  56",
"10  2      2.00      1.00      0.00      0.00                         IGRF35  57",
"10  3     -5.00      2.00      0.00      0.00                         IGRF35  58",
"10  4     -2.00      6.00      0.00      0.00                         IGRF35  59",
"10  5      6.00     -4.00      0.00      0.00                         IGRF35  60",
"10  6      4.00      0.00      0.00      0.00                         IGRF35  61",
"10  7      0.00     -1.00      0.00      0.00                         IGRF35  62",
"10  8      2.00      4.00      0.00      0.00                         IGRF35  63",
"10  9      3.00      0.00      0.00      0.00                         IGRF35  64",
"10 10      0.00     -6.00      0.00      0.00                         IGRF35  65",
"IGRF40  1940.00 10  0  0 1940.00 1945.00   -1.0  600.0           IGRF40   0",
"1  0 -30654.00      0.00      0.00      0.00                         IGRF40   1",
"1  1  -2292.00   5821.00      0.00      0.00                         IGRF40   2",
"2  0  -1106.00      0.00      0.00      0.00                         IGRF40   3",
"2  1   2981.00  -1614.00      0.00      0.00                         IGRF40   4",
"2  2   1566.00    528.00      0.00      0.00                         IGRF40   5",
"3  0   1240.00      0.00      0.00      0.00                         IGRF40   6",
"3  1  -1790.00   -499.00      0.00      0.00                         IGRF40   7",
"3  2   1232.00    163.00      0.00      0.00                         IGRF40   8",
"3  3    916.00     43.00      0.00      0.00                         IGRF40   9",
"4  0    914.00      0.00      0.00      0.00                         IGRF40  10",
"4  1    762.00    169.00      0.00      0.00                         IGRF40  11",
"4  2    550.00   -252.00      0.00      0.00                         IGRF40  12",
"4  3   -405.00    -72.00      0.00      0.00                         IGRF40  13",
"4  4    265.00   -141.00      0.00      0.00                         IGRF40  14",
"5  0   -241.00      0.00      0.00      0.00                         IGRF40  15",
"5  1    334.00    -33.00      0.00      0.00                         IGRF40  16",
"5  2    208.00     71.00      0.00      0.00                         IGRF40  17",
"5  3    -33.00    -75.00      0.00      0.00                         IGRF40  18",
"5  4   -141.00   -113.00      0.00      0.00                         IGRF40  19",
"5  5    -76.00     69.00      0.00      0.00                         IGRF40  20",
"6  0     57.00      0.00      0.00      0.00                         IGRF40  21",
"6  1     54.00      4.00      0.00      0.00                         IGRF40  22",
"6  2     -7.00    105.00      0.00      0.00                         IGRF40  23",
"6  3   -249.00     33.00      0.00      0.00                         IGRF40  24",
"6  4    -18.00    -15.00      0.00      0.00                         IGRF40  25",
"6  5     18.00      0.00      0.00      0.00                         IGRF40  26",
"6  6   -107.00    -33.00      0.00      0.00                         IGRF40  27",
"7  0     74.00      0.00      0.00      0.00                         IGRF40  28",
"7  1    -53.00    -52.00      0.00      0.00                         IGRF40  29",
"7  2      4.00    -18.00      0.00      0.00                         IGRF40  30",
"7  3     20.00    -14.00      0.00      0.00                         IGRF40  31",
"7  4    -31.00      7.00      0.00      0.00                         IGRF40  32",
"7  5     -9.00     29.00      0.00      0.00                         IGRF40  33",
"7  6     17.00    -20.00      0.00      0.00                         IGRF40  34",
"7  7      5.00    -19.00      0.00      0.00                         IGRF40  35",
"8  0     11.00      0.00      0.00      0.00                         IGRF40  36",
"8  1      7.00      8.00      0.00      0.00                         IGRF40  37",
"8  2     -3.00    -14.00      0.00      0.00                         IGRF40  38",
"8  3    -10.00      5.00      0.00      0.00                         IGRF40  39",
"8  4      1.00    -15.00      0.00      0.00                         IGRF40  40",
"8  5      6.00      5.00      0.00      0.00                         IGRF40  41",
"8  6     -5.00     19.00      0.00      0.00                         IGRF40  42",
"8  7      9.00     -5.00      0.00      0.00                         IGRF40  43",
"8  8      7.00    -19.00      0.00      0.00                         IGRF40  44",
"9  0      8.00      0.00      0.00      0.00                         IGRF40  45",
"9  1     10.00    -21.00      0.00      0.00                         IGRF40  46",
"9  2      1.00     15.00      0.00      0.00                         IGRF40  47",
"9  3    -12.00      5.00      0.00      0.00                         IGRF40  48",
"9  4     11.00     -3.00      0.00      0.00                         IGRF40  49",
"9  5      1.00     -3.00      0.00      0.00                         IGRF40  50",
"9  6     -2.00      9.00      0.00      0.00                         IGRF40  51",
"9  7      3.00     11.00      0.00      0.00                         IGRF40  52",
"9  8      1.00     -2.00      0.00      0.00                         IGRF40  53",
"9  9     -2.00      2.00      0.00      0.00                         IGRF40  54",
"10  0     -3.00      0.00      0.00      0.00                         IGRF40  55",
"10  1     -4.00      2.00      0.00      0.00                         IGRF40  56",
"10  2      2.00      1.00      0.00      0.00                         IGRF40  57",
"10  3     -5.00      2.00      0.00      0.00                         IGRF40  58",
"10  4     -2.00      6.00      0.00      0.00                         IGRF40  59",
"10  5      6.00     -4.00      0.00      0.00                         IGRF40  60",
"10  6      4.00      0.00      0.00      0.00                         IGRF40  61",
"10  7      0.00     -1.00      0.00      0.00                         IGRF40  62",
"10  8      2.00      4.00      0.00      0.00                         IGRF40  63",
"10  9      3.00      0.00      0.00      0.00                         IGRF40  64",
"10 10      0.00     -6.00      0.00      0.00                         IGRF40  65",
"DGRF45  1945.00 10  0  0 1945.00 1950.00   -1.0  600.0           DGRF45   0",
"1  0 -30594.00      0.00      0.00      0.00                         DGRF45   1",
"1  1  -2285.00   5810.00      0.00      0.00                         DGRF45   2",
"2  0  -1244.00      0.00      0.00      0.00                         DGRF45   3",
"2  1   2990.00  -1702.00      0.00      0.00                         DGRF45   4",
"2  2   1578.00    477.00      0.00      0.00                         DGRF45   5",
"3  0   1282.00      0.00      0.00      0.00                         DGRF45   6",
"3  1  -1834.00   -499.00      0.00      0.00                         DGRF45   7",
"3  2   1255.00    186.00      0.00      0.00                         DGRF45   8",
"3  3    913.00    -11.00      0.00      0.00                         DGRF45   9",
"4  0    944.00      0.00      0.00      0.00                         DGRF45  10",
"4  1    776.00    144.00      0.00      0.00                         DGRF45  11",
"4  2    544.00   -276.00      0.00      0.00                         DGRF45  12",
"4  3   -421.00    -55.00      0.00      0.00                         DGRF45  13",
"4  4    304.00   -178.00      0.00      0.00                         DGRF45  14",
"5  0   -253.00      0.00      0.00      0.00                         DGRF45  15",
"5  1    346.00    -12.00      0.00      0.00                         DGRF45  16",
"5  2    194.00     95.00      0.00      0.00                         DGRF45  17",
"5  3    -20.00    -67.00      0.00      0.00                         DGRF45  18",
"5  4   -142.00   -119.00      0.00      0.00                         DGRF45  19",
"5  5    -82.00     82.00      0.00      0.00                         DGRF45  20",
"6  0     59.00      0.00      0.00      0.00                         DGRF45  21",
"6  1     57.00      6.00      0.00      0.00                         DGRF45  22",
"6  2      6.00    100.00      0.00      0.00                         DGRF45  23",
"6  3   -246.00     16.00      0.00      0.00                         DGRF45  24",
"6  4    -25.00     -9.00      0.00      0.00                         DGRF45  25",
"6  5     21.00    -16.00      0.00      0.00                         DGRF45  26",
"6  6   -104.00    -39.00      0.00      0.00                         DGRF45  27",
"7  0     70.00      0.00      0.00      0.00                         DGRF45  28",
"7  1    -40.00    -45.00      0.00      0.00                         DGRF45  29",
"7  2      0.00    -18.00      0.00      0.00                         DGRF45  30",
"7  3      0.00      2.00      0.00      0.00                         DGRF45  31",
"7  4    -29.00      6.00      0.00      0.00                         DGRF45  32",
"7  5    -10.00     28.00      0.00      0.00                         DGRF45  33",
"7  6     15.00    -17.00      0.00      0.00                         DGRF45  34",
"7  7     29.00    -22.00      0.00      0.00                         DGRF45  35",
"8  0     13.00      0.00      0.00      0.00                         DGRF45  36",
"8  1      7.00     12.00      0.00      0.00                         DGRF45  37",
"8  2     -8.00    -21.00      0.00      0.00                         DGRF45  38",
"8  3     -5.00    -12.00      0.00      0.00                         DGRF45  39",
"8  4      9.00     -7.00      0.00      0.00                         DGRF45  40",
"8  5      7.00      2.00      0.00      0.00                         DGRF45  41",
"8  6    -10.00     18.00      0.00      0.00                         DGRF45  42",
"8  7      7.00      3.00      0.00      0.00                         DGRF45  43",
"8  8      2.00    -11.00      0.00      0.00                         DGRF45  44",
"9  0      5.00      0.00      0.00      0.00                         DGRF45  45",
"9  1    -21.00    -27.00      0.00      0.00                         DGRF45  46",
"9  2      1.00     17.00      0.00      0.00                         DGRF45  47",
"9  3    -11.00     29.00      0.00      0.00                         DGRF45  48",
"9  4      3.00     -9.00      0.00      0.00                         DGRF45  49",
"9  5     16.00      4.00      0.00      0.00                         DGRF45  50",
"9  6     -3.00      9.00      0.00      0.00                         DGRF45  51",
"9  7     -4.00      6.00      0.00      0.00                         DGRF45  52",
"9  8     -3.00      1.00      0.00      0.00                         DGRF45  53",
"9  9     -4.00      8.00      0.00      0.00                         DGRF45  54",
"10  0     -3.00      0.00      0.00      0.00                         DGRF45  55",
"10  1     11.00      5.00      0.00      0.00                         DGRF45  56",
"10  2      1.00      1.00      0.00      0.00                         DGRF45  57",
"10  3      2.00    -20.00      0.00      0.00                         DGRF45  58",
"10  4     -5.00     -1.00      0.00      0.00                         DGRF45  59",
"10  5     -1.00     -6.00      0.00      0.00                         DGRF45  60",
"10  6      8.00      6.00      0.00      0.00                         DGRF45  61",
"10  7     -1.00     -4.00      0.00      0.00                         DGRF45  62",
"10  8     -3.00     -2.00      0.00      0.00                         DGRF45  63",
"10  9      5.00      0.00      0.00      0.00                         DGRF45  64",
"10 10     -2.00     -2.00      0.00      0.00                         DGRF45  65",
"DGRF50  1950.00 10  0  0 1950.00 1955.00   -1.0  600.0           DGRF50   0",
"1  0 -30554.00      0.00      0.00      0.00                         DGRF50   1",
"1  1  -2250.00   5815.00      0.00      0.00                         DGRF50   2",
"2  0  -1341.00      0.00      0.00      0.00                         DGRF50   3",
"2  1   2998.00  -1810.00      0.00      0.00                         DGRF50   4",
"2  2   1576.00    381.00      0.00      0.00                         DGRF50   5",
"3  0   1297.00      0.00      0.00      0.00                         DGRF50   6",
"3  1  -1889.00   -476.00      0.00      0.00                         DGRF50   7",
"3  2   1274.00    206.00      0.00      0.00                         DGRF50   8",
"3  3    896.00    -46.00      0.00      0.00                         DGRF50   9",
"4  0    954.00      0.00      0.00      0.00                         DGRF50  10",
"4  1    792.00    136.00      0.00      0.00                         DGRF50  11",
"4  2    528.00   -278.00      0.00      0.00                         DGRF50  12",
"4  3   -408.00    -37.00      0.00      0.00                         DGRF50  13",
"4  4    303.00   -210.00      0.00      0.00                         DGRF50  14",
"5  0   -240.00      0.00      0.00      0.00                         DGRF50  15",
"5  1    349.00      3.00      0.00      0.00                         DGRF50  16",
"5  2    211.00    103.00      0.00      0.00                         DGRF50  17",
"5  3    -20.00    -87.00      0.00      0.00                         DGRF50  18",
"5  4   -147.00   -122.00      0.00      0.00                         DGRF50  19",
"5  5    -76.00     80.00      0.00      0.00                         DGRF50  20",
"6  0     54.00      0.00      0.00      0.00                         DGRF50  21",
"6  1     57.00     -1.00      0.00      0.00                         DGRF50  22",
"6  2      4.00     99.00      0.00      0.00                         DGRF50  23",
"6  3   -247.00     33.00      0.00      0.00                         DGRF50  24",
"6  4    -16.00    -12.00      0.00      0.00                         DGRF50  25",
"6  5     12.00    -12.00      0.00      0.00                         DGRF50  26",
"6  6   -105.00    -30.00      0.00      0.00                         DGRF50  27",
"7  0     65.00      0.00      0.00      0.00                         DGRF50  28",
"7  1    -55.00    -35.00      0.00      0.00                         DGRF50  29",
"7  2      2.00    -17.00      0.00      0.00                         DGRF50  30",
"7  3      1.00      0.00      0.00      0.00                         DGRF50  31",
"7  4    -40.00     10.00      0.00      0.00                         DGRF50  32",
"7  5     -7.00     36.00      0.00      0.00                         DGRF50  33",
"7  6      5.00    -18.00      0.00      0.00                         DGRF50  34",
"7  7     19.00    -16.00      0.00      0.00                         DGRF50  35",
"8  0     22.00      0.00      0.00      0.00                         DGRF50  36",
"8  1     15.00      5.00      0.00      0.00                         DGRF50  37",
"8  2     -4.00    -22.00      0.00      0.00                         DGRF50  38",
"8  3     -1.00      0.00      0.00      0.00                         DGRF50  39",
"8  4     11.00    -21.00      0.00      0.00                         DGRF50  40",
"8  5     15.00     -8.00      0.00      0.00                         DGRF50  41",
"8  6    -13.00     17.00      0.00      0.00                         DGRF50  42",
"8  7      5.00     -4.00      0.00      0.00                         DGRF50  43",
"8  8     -1.00    -17.00      0.00      0.00                         DGRF50  44",
"9  0      3.00      0.00      0.00      0.00                         DGRF50  45",
"9  1     -7.00    -24.00      0.00      0.00                         DGRF50  46",
"9  2     -1.00     19.00      0.00      0.00                         DGRF50  47",
"9  3    -25.00     12.00      0.00      0.00                         DGRF50  48",
"9  4     10.00      2.00      0.00      0.00                         DGRF50  49",
"9  5      5.00      2.00      0.00      0.00                         DGRF50  50",
"9  6     -5.00      8.00      0.00      0.00                         DGRF50  51",
"9  7     -2.00      8.00      0.00      0.00                         DGRF50  52",
"9  8      3.00    -11.00      0.00      0.00                         DGRF50  53",
"9  9      8.00     -7.00      0.00      0.00                         DGRF50  54",
"10  0     -8.00      0.00      0.00      0.00                         DGRF50  55",
"10  1      4.00     13.00      0.00      0.00                         DGRF50  56",
"10  2     -1.00     -2.00      0.00      0.00                         DGRF50  57",
"10  3     13.00    -10.00      0.00      0.00                         DGRF50  58",
"10  4     -4.00      2.00      0.00      0.00                         DGRF50  59",
"10  5      4.00     -3.00      0.00      0.00                         DGRF50  60",
"10  6     12.00      6.00      0.00      0.00                         DGRF50  61",
"10  7      3.00     -3.00      0.00      0.00                         DGRF50  62",
"10  8      2.00      6.00      0.00      0.00                         DGRF50  63",
"10  9     10.00     11.00      0.00      0.00                         DGRF50  64",
"10 10      3.00      8.00      0.00      0.00                         DGRF50  65",
"DGRF55  1955.00 10  0  0 1955.00 1960.00   -1.0  600.0           DGRF55   0",
"1  0 -30500.00      0.00      0.00      0.00                         DGRF55   1",
"1  1  -2215.00   5820.00      0.00      0.00                         DGRF55   2",
"2  0  -1440.00      0.00      0.00      0.00                         DGRF55   3",
"2  1   3003.00  -1898.00      0.00      0.00                         DGRF55   4",
"2  2   1581.00    291.00      0.00      0.00                         DGRF55   5",
"3  0   1302.00      0.00      0.00      0.00                         DGRF55   6",
"3  1  -1944.00   -462.00      0.00      0.00                         DGRF55   7",
"3  2   1288.00    216.00      0.00      0.00                         DGRF55   8",
"3  3    882.00    -83.00      0.00      0.00                         DGRF55   9",
"4  0    958.00      0.00      0.00      0.00                         DGRF55  10",
"4  1    796.00    133.00      0.00      0.00                         DGRF55  11",
"4  2    510.00   -274.00      0.00      0.00                         DGRF55  12",
"4  3   -397.00    -23.00      0.00      0.00                         DGRF55  13",
"4  4    290.00   -230.00      0.00      0.00                         DGRF55  14",
"5  0   -229.00      0.00      0.00      0.00                         DGRF55  15",
"5  1    360.00     15.00      0.00      0.00                         DGRF55  16",
"5  2    230.00    110.00      0.00      0.00                         DGRF55  17",
"5  3    -23.00    -98.00      0.00      0.00                         DGRF55  18",
"5  4   -152.00   -121.00      0.00      0.00                         DGRF55  19",
"5  5    -69.00     78.00      0.00      0.00                         DGRF55  20",
"6  0     47.00      0.00      0.00      0.00                         DGRF55  21",
"6  1     57.00     -9.00      0.00      0.00                         DGRF55  22",
"6  2      3.00     96.00      0.00      0.00                         DGRF55  23",
"6  3   -247.00     48.00      0.00      0.00                         DGRF55  24",
"6  4     -8.00    -16.00      0.00      0.00                         DGRF55  25",
"6  5      7.00    -12.00      0.00      0.00                         DGRF55  26",
"6  6   -107.00    -24.00      0.00      0.00                         DGRF55  27",
"7  0     65.00      0.00      0.00      0.00                         DGRF55  28",
"7  1    -56.00    -50.00      0.00      0.00                         DGRF55  29",
"7  2      2.00    -24.00      0.00      0.00                         DGRF55  30",
"7  3     10.00     -4.00      0.00      0.00                         DGRF55  31",
"7  4    -32.00      8.00      0.00      0.00                         DGRF55  32",
"7  5    -11.00     28.00      0.00      0.00                         DGRF55  33",
"7  6      9.00    -20.00      0.00      0.00                         DGRF55  34",
"7  7     18.00    -18.00      0.00      0.00                         DGRF55  35",
"8  0     11.00      0.00      0.00      0.00                         DGRF55  36",
"8  1      9.00     10.00      0.00      0.00                         DGRF55  37",
"8  2     -6.00    -15.00      0.00      0.00                         DGRF55  38",
"8  3    -14.00      5.00      0.00      0.00                         DGRF55  39",
"8  4      6.00    -23.00      0.00      0.00                         DGRF55  40",
"8  5     10.00      3.00      0.00      0.00                         DGRF55  41",
"8  6     -7.00     23.00      0.00      0.00                         DGRF55  42",
"8  7      6.00     -4.00      0.00      0.00                         DGRF55  43",
"8  8      9.00    -13.00      0.00      0.00                         DGRF55  44",
"9  0      4.00      0.00      0.00      0.00                         DGRF55  45",
"9  1      9.00    -11.00      0.00      0.00                         DGRF55  46",
"9  2     -4.00     12.00      0.00      0.00                         DGRF55  47",
"9  3     -5.00      7.00      0.00      0.00                         DGRF55  48",
"9  4      2.00      6.00      0.00      0.00                         DGRF55  49",
"9  5      4.00     -2.00      0.00      0.00                         DGRF55  50",
"9  6      1.00     10.00      0.00      0.00                         DGRF55  51",
"9  7      2.00      7.00      0.00      0.00                         DGRF55  52",
"9  8      2.00     -6.00      0.00      0.00                         DGRF55  53",
"9  9      5.00      5.00      0.00      0.00                         DGRF55  54",
"10  0     -3.00      0.00      0.00      0.00                         DGRF55  55",
"10  1     -5.00     -4.00      0.00      0.00                         DGRF55  56",
"10  2     -1.00      0.00      0.00      0.00                         DGRF55  57",
"10  3      2.00     -8.00      0.00      0.00                         DGRF55  58",
"10  4     -3.00     -2.00      0.00      0.00                         DGRF55  59",
"10  5      7.00     -4.00      0.00      0.00                         DGRF55  60",
"10  6      4.00      1.00      0.00      0.00                         DGRF55  61",
"10  7     -2.00     -3.00      0.00      0.00                         DGRF55  62",
"10  8      6.00      7.00      0.00      0.00                         DGRF55  63",
"10  9     -2.00     -1.00      0.00      0.00                         DGRF55  64",
"10 10      0.00     -3.00      0.00      0.00                         DGRF55  65",
"DGRF60  1960.00 10  0  0 1960.00 1965.00   -1.0  600.0           DGRF60   0",
"1  0 -30421.00      0.00      0.00      0.00                         DGRF60   1",
"1  1  -2169.00   5791.00      0.00      0.00                         DGRF60   2",
"2  0  -1555.00      0.00      0.00      0.00                         DGRF60   3",
"2  1   3002.00  -1967.00      0.00      0.00                         DGRF60   4",
"2  2   1590.00    206.00      0.00      0.00                         DGRF60   5",
"3  0   1302.00      0.00      0.00      0.00                         DGRF60   6",
"3  1  -1992.00   -414.00      0.00      0.00                         DGRF60   7",
"3  2   1289.00    224.00      0.00      0.00                         DGRF60   8",
"3  3    878.00   -130.00      0.00      0.00                         DGRF60   9",
"4  0    957.00      0.00      0.00      0.00                         DGRF60  10",
"4  1    800.00    135.00      0.00      0.00                         DGRF60  11",
"4  2    504.00   -278.00      0.00      0.00                         DGRF60  12",
"4  3   -394.00      3.00      0.00      0.00                         DGRF60  13",
"4  4    269.00   -255.00      0.00      0.00                         DGRF60  14",
"5  0   -222.00      0.00      0.00      0.00                         DGRF60  15",
"5  1    362.00     16.00      0.00      0.00                         DGRF60  16",
"5  2    242.00    125.00      0.00      0.00                         DGRF60  17",
"5  3    -26.00   -117.00      0.00      0.00                         DGRF60  18",
"5  4   -156.00   -114.00      0.00      0.00                         DGRF60  19",
"5  5    -63.00     81.00      0.00      0.00                         DGRF60  20",
"6  0     46.00      0.00      0.00      0.00                         DGRF60  21",
"6  1     58.00    -10.00      0.00      0.00                         DGRF60  22",
"6  2      1.00     99.00      0.00      0.00                         DGRF60  23",
"6  3   -237.00     60.00      0.00      0.00                         DGRF60  24",
"6  4     -1.00    -20.00      0.00      0.00                         DGRF60  25",
"6  5     -2.00    -11.00      0.00      0.00                         DGRF60  26",
"6  6   -113.00    -17.00      0.00      0.00                         DGRF60  27",
"7  0     67.00      0.00      0.00      0.00                         DGRF60  28",
"7  1    -56.00    -55.00      0.00      0.00                         DGRF60  29",
"7  2      5.00    -28.00      0.00      0.00                         DGRF60  30",
"7  3     15.00     -6.00      0.00      0.00                         DGRF60  31",
"7  4    -32.00      7.00      0.00      0.00                         DGRF60  32",
"7  5     -7.00     23.00      0.00      0.00                         DGRF60  33",
"7  6     17.00    -18.00      0.00      0.00                         DGRF60  34",
"7  7      8.00    -17.00      0.00      0.00                         DGRF60  35",
"8  0     15.00      0.00      0.00      0.00                         DGRF60  36",
"8  1      6.00     11.00      0.00      0.00                         DGRF60  37",
"8  2     -4.00    -14.00      0.00      0.00                         DGRF60  38",
"8  3    -11.00      7.00      0.00      0.00                         DGRF60  39",
"8  4      2.00    -18.00      0.00      0.00                         DGRF60  40",
"8  5     10.00      4.00      0.00      0.00                         DGRF60  41",
"8  6     -5.00     23.00      0.00      0.00                         DGRF60  42",
"8  7     10.00      1.00      0.00      0.00                         DGRF60  43",
"8  8      8.00    -20.00      0.00      0.00                         DGRF60  44",
"9  0      4.00      0.00      0.00      0.00                         DGRF60  45",
"9  1      6.00    -18.00      0.00      0.00                         DGRF60  46",
"9  2      0.00     12.00      0.00      0.00                         DGRF60  47",
"9  3     -9.00      2.00      0.00      0.00                         DGRF60  48",
"9  4      1.00      0.00      0.00      0.00                         DGRF60  49",
"9  5      4.00     -3.00      0.00      0.00                         DGRF60  50",
"9  6     -1.00      9.00      0.00      0.00                         DGRF60  51",
"9  7     -2.00      8.00      0.00      0.00                         DGRF60  52",
"9  8      3.00      0.00      0.00      0.00                         DGRF60  53",
"9  9     -1.00      5.00      0.00      0.00                         DGRF60  54",
"10  0      1.00      0.00      0.00      0.00                         DGRF60  55",
"10  1     -3.00      4.00      0.00      0.00                         DGRF60  56",
"10  2      4.00      1.00      0.00      0.00                         DGRF60  57",
"10  3      0.00      0.00      0.00      0.00                         DGRF60  58",
"10  4     -1.00      2.00      0.00      0.00                         DGRF60  59",
"10  5      4.00     -5.00      0.00      0.00                         DGRF60  60",
"10  6      6.00      1.00      0.00      0.00                         DGRF60  61",
"10  7      1.00     -1.00      0.00      0.00                         DGRF60  62",
"10  8     -1.00      6.00      0.00      0.00                         DGRF60  63",
"10  9      2.00      0.00      0.00      0.00                         DGRF60  64",
"10 10      0.00     -7.00      0.00      0.00                         DGRF60  65",
"DGRF65  1965.00 10  0  0 1965.00 1970.00   -1.0  600.0           DGRF65   0",
"1  0 -30334.00      0.00      0.00      0.00                         DGRF65   1",
"1  1  -2119.00   5776.00      0.00      0.00                         DGRF65   2",
"2  0  -1662.00      0.00      0.00      0.00                         DGRF65   3",
"2  1   2997.00  -2016.00      0.00      0.00                         DGRF65   4",
"2  2   1594.00    114.00      0.00      0.00                         DGRF65   5",
"3  0   1297.00      0.00      0.00      0.00                         DGRF65   6",
"3  1  -2038.00   -404.00      0.00      0.00                         DGRF65   7",
"3  2   1292.00    240.00      0.00      0.00                         DGRF65   8",
"3  3    856.00   -165.00      0.00      0.00                         DGRF65   9",
"4  0    957.00      0.00      0.00      0.00                         DGRF65  10",
"4  1    804.00    148.00      0.00      0.00                         DGRF65  11",
"4  2    479.00   -269.00      0.00      0.00                         DGRF65  12",
"4  3   -390.00     13.00      0.00      0.00                         DGRF65  13",
"4  4    252.00   -269.00      0.00      0.00                         DGRF65  14",
"5  0   -219.00      0.00      0.00      0.00                         DGRF65  15",
"5  1    358.00     19.00      0.00      0.00                         DGRF65  16",
"5  2    254.00    128.00      0.00      0.00                         DGRF65  17",
"5  3    -31.00   -126.00      0.00      0.00                         DGRF65  18",
"5  4   -157.00    -97.00      0.00      0.00                         DGRF65  19",
"5  5    -62.00     81.00      0.00      0.00                         DGRF65  20",
"6  0     45.00      0.00      0.00      0.00                         DGRF65  21",
"6  1     61.00    -11.00      0.00      0.00                         DGRF65  22",
"6  2      8.00    100.00      0.00      0.00                         DGRF65  23",
"6  3   -228.00     68.00      0.00      0.00                         DGRF65  24",
"6  4      4.00    -32.00      0.00      0.00                         DGRF65  25",
"6  5      1.00     -8.00      0.00      0.00                         DGRF65  26",
"6  6   -111.00     -7.00      0.00      0.00                         DGRF65  27",
"7  0     75.00      0.00      0.00      0.00                         DGRF65  28",
"7  1    -57.00    -61.00      0.00      0.00                         DGRF65  29",
"7  2      4.00    -27.00      0.00      0.00                         DGRF65  30",
"7  3     13.00     -2.00      0.00      0.00                         DGRF65  31",
"7  4    -26.00      6.00      0.00      0.00                         DGRF65  32",
"7  5     -6.00     26.00      0.00      0.00                         DGRF65  33",
"7  6     13.00    -23.00      0.00      0.00                         DGRF65  34",
"7  7      1.00    -12.00      0.00      0.00                         DGRF65  35",
"8  0     13.00      0.00      0.00      0.00                         DGRF65  36",
"8  1      5.00      7.00      0.00      0.00                         DGRF65  37",
"8  2     -4.00    -12.00      0.00      0.00                         DGRF65  38",
"8  3    -14.00      9.00      0.00      0.00                         DGRF65  39",
"8  4      0.00    -16.00      0.00      0.00                         DGRF65  40",
"8  5      8.00      4.00      0.00      0.00                         DGRF65  41",
"8  6     -1.00     24.00      0.00      0.00                         DGRF65  42",
"8  7     11.00     -3.00      0.00      0.00                         DGRF65  43",
"8  8      4.00    -17.00      0.00      0.00                         DGRF65  44",
"9  0      8.00      0.00      0.00      0.00                         DGRF65  45",
"9  1     10.00    -22.00      0.00      0.00                         DGRF65  46",
"9  2      2.00     15.00      0.00      0.00                         DGRF65  47",
"9  3    -13.00      7.00      0.00      0.00                         DGRF65  48",
"9  4     10.00     -4.00      0.00      0.00                         DGRF65  49",
"9  5     -1.00     -5.00      0.00      0.00                         DGRF65  50",
"9  6     -1.00     10.00      0.00      0.00                         DGRF65  51",
"9  7      5.00     10.00      0.00      0.00                         DGRF65  52",
"9  8      1.00     -4.00      0.00      0.00                         DGRF65  53",
"9  9     -2.00      1.00      0.00      0.00                         DGRF65  54",
"10  0     -2.00      0.00      0.00      0.00                         DGRF65  55",
"10  1     -3.00      2.00      0.00      0.00                         DGRF65  56",
"10  2      2.00      1.00      0.00      0.00                         DGRF65  57",
"10  3     -5.00      2.00      0.00      0.00                         DGRF65  58",
"10  4     -2.00      6.00      0.00      0.00                         DGRF65  59",
"10  5      4.00     -4.00      0.00      0.00                         DGRF65  60",
"10  6      4.00      0.00      0.00      0.00                         DGRF65  61",
"10  7      0.00     -2.00      0.00      0.00                         DGRF65  62",
"10  8      2.00      3.00      0.00      0.00                         DGRF65  63",
"10  9      2.00      0.00      0.00      0.00                         DGRF65  64",
"10 10      0.00     -6.00      0.00      0.00                         DGRF65  65",
"DGRF70  1970.00 10  0  0 1970.00 1975.00   -1.0  600.0           DGRF70   0",
"1  0 -30220.00      0.00      0.00      0.00                         DGRF70   1",
"1  1  -2068.00   5737.00      0.00      0.00                         DGRF70   2",
"2  0  -1781.00      0.00      0.00      0.00                         DGRF70   3",
"2  1   3000.00  -2047.00      0.00      0.00                         DGRF70   4",
"2  2   1611.00     25.00      0.00      0.00                         DGRF70   5",
"3  0   1287.00      0.00      0.00      0.00                         DGRF70   6",
"3  1  -2091.00   -366.00      0.00      0.00                         DGRF70   7",
"3  2   1278.00    251.00      0.00      0.00                         DGRF70   8",
"3  3    838.00   -196.00      0.00      0.00                         DGRF70   9",
"4  0    952.00      0.00      0.00      0.00                         DGRF70  10",
"4  1    800.00    167.00      0.00      0.00                         DGRF70  11",
"4  2    461.00   -266.00      0.00      0.00                         DGRF70  12",
"4  3   -395.00     26.00      0.00      0.00                         DGRF70  13",
"4  4    234.00   -279.00      0.00      0.00                         DGRF70  14",
"5  0   -216.00      0.00      0.00      0.00                         DGRF70  15",
"5  1    359.00     26.00      0.00      0.00                         DGRF70  16",
"5  2    262.00    139.00      0.00      0.00                         DGRF70  17",
"5  3    -42.00   -139.00      0.00      0.00                         DGRF70  18",
"5  4   -160.00    -91.00      0.00      0.00                         DGRF70  19",
"5  5    -56.00     83.00      0.00      0.00                         DGRF70  20",
"6  0     43.00      0.00      0.00      0.00                         DGRF70  21",
"6  1     64.00    -12.00      0.00      0.00                         DGRF70  22",
"6  2     15.00    100.00      0.00      0.00                         DGRF70  23",
"6  3   -212.00     72.00      0.00      0.00                         DGRF70  24",
"6  4      2.00    -37.00      0.00      0.00                         DGRF70  25",
"6  5      3.00     -6.00      0.00      0.00                         DGRF70  26",
"6  6   -112.00      1.00      0.00      0.00                         DGRF70  27",
"7  0     72.00      0.00      0.00      0.00                         DGRF70  28",
"7  1    -57.00    -70.00      0.00      0.00                         DGRF70  29",
"7  2      1.00    -27.00      0.00      0.00                         DGRF70  30",
"7  3     14.00     -4.00      0.00      0.00                         DGRF70  31",
"7  4    -22.00      8.00      0.00      0.00                         DGRF70  32",
"7  5     -2.00     23.00      0.00      0.00                         DGRF70  33",
"7  6     13.00    -23.00      0.00      0.00                         DGRF70  34",
"7  7     -2.00    -11.00      0.00      0.00                         DGRF70  35",
"8  0     14.00      0.00      0.00      0.00                         DGRF70  36",
"8  1      6.00      7.00      0.00      0.00                         DGRF70  37",
"8  2     -2.00    -15.00      0.00      0.00                         DGRF70  38",
"8  3    -13.00      6.00      0.00      0.00                         DGRF70  39",
"8  4     -3.00    -17.00      0.00      0.00                         DGRF70  40",
"8  5      5.00      6.00      0.00      0.00                         DGRF70  41",
"8  6      0.00     21.00      0.00      0.00                         DGRF70  42",
"8  7     11.00     -6.00      0.00      0.00                         DGRF70  43",
"8  8      3.00    -16.00      0.00      0.00                         DGRF70  44",
"9  0      8.00      0.00      0.00      0.00                         DGRF70  45",
"9  1     10.00    -21.00      0.00      0.00                         DGRF70  46",
"9  2      2.00     16.00      0.00      0.00                         DGRF70  47",
"9  3    -12.00      6.00      0.00      0.00                         DGRF70  48",
"9  4     10.00     -4.00      0.00      0.00                         DGRF70  49",
"9  5     -1.00     -5.00      0.00      0.00                         DGRF70  50",
"9  6      0.00     10.00      0.00      0.00                         DGRF70  51",
"9  7      3.00     11.00      0.00      0.00                         DGRF70  52",
"9  8      1.00     -2.00      0.00      0.00                         DGRF70  53",
"9  9     -1.00      1.00      0.00      0.00                         DGRF70  54",
"10  0     -3.00      0.00      0.00      0.00                         DGRF70  55",
"10  1     -3.00      1.00      0.00      0.00                         DGRF70  56",
"10  2      2.00      1.00      0.00      0.00                         DGRF70  57",
"10  3     -5.00      3.00      0.00      0.00                         DGRF70  58",
"10  4     -1.00      4.00      0.00      0.00                         DGRF70  59",
"10  5      6.00     -4.00      0.00      0.00                         DGRF70  60",
"10  6      4.00      0.00      0.00      0.00                         DGRF70  61",
"10  7      1.00     -1.00      0.00      0.00                         DGRF70  62",
"10  8      0.00      3.00      0.00      0.00                         DGRF70  63",
"10  9      3.00      1.00      0.00      0.00                         DGRF70  64",
"10 10     -1.00     -4.00      0.00      0.00                         DGRF70  65",
"DGRF75  1975.00 10  0  0 1975.00 1980.00   -1.0  600.0           DGRF75   0",
"1  0 -30100.00      0.00      0.00      0.00                         DGRF75   1",
"1  1  -2013.00   5675.00      0.00      0.00                         DGRF75   2",
"2  0  -1902.00      0.00      0.00      0.00                         DGRF75   3",
"2  1   3010.00  -2067.00      0.00      0.00                         DGRF75   4",
"2  2   1632.00    -68.00      0.00      0.00                         DGRF75   5",
"3  0   1276.00      0.00      0.00      0.00                         DGRF75   6",
"3  1  -2144.00   -333.00      0.00      0.00                         DGRF75   7",
"3  2   1260.00    262.00      0.00      0.00                         DGRF75   8",
"3  3    830.00   -223.00      0.00      0.00                         DGRF75   9",
"4  0    946.00      0.00      0.00      0.00                         DGRF75  10",
"4  1    791.00    191.00      0.00      0.00                         DGRF75  11",
"4  2    438.00   -265.00      0.00      0.00                         DGRF75  12",
"4  3   -405.00     39.00      0.00      0.00                         DGRF75  13",
"4  4    216.00   -288.00      0.00      0.00                         DGRF75  14",
"5  0   -218.00      0.00      0.00      0.00                         DGRF75  15",
"5  1    356.00     31.00      0.00      0.00                         DGRF75  16",
"5  2    264.00    148.00      0.00      0.00                         DGRF75  17",
"5  3    -59.00   -152.00      0.00      0.00                         DGRF75  18",
"5  4   -159.00    -83.00      0.00      0.00                         DGRF75  19",
"5  5    -49.00     88.00      0.00      0.00                         DGRF75  20",
"6  0     45.00      0.00      0.00      0.00                         DGRF75  21",
"6  1     66.00    -13.00      0.00      0.00                         DGRF75  22",
"6  2     28.00     99.00      0.00      0.00                         DGRF75  23",
"6  3   -198.00     75.00      0.00      0.00                         DGRF75  24",
"6  4      1.00    -41.00      0.00      0.00                         DGRF75  25",
"6  5      6.00     -4.00      0.00      0.00                         DGRF75  26",
"6  6   -111.00     11.00      0.00      0.00                         DGRF75  27",
"7  0     71.00      0.00      0.00      0.00                         DGRF75  28",
"7  1    -56.00    -77.00      0.00      0.00                         DGRF75  29",
"7  2      1.00    -26.00      0.00      0.00                         DGRF75  30",
"7  3     16.00     -5.00      0.00      0.00                         DGRF75  31",
"7  4    -14.00     10.00      0.00      0.00                         DGRF75  32",
"7  5      0.00     22.00      0.00      0.00                         DGRF75  33",
"7  6     12.00    -23.00      0.00      0.00                         DGRF75  34",
"7  7     -5.00    -12.00      0.00      0.00                         DGRF75  35",
"8  0     14.00      0.00      0.00      0.00                         DGRF75  36",
"8  1      6.00      6.00      0.00      0.00                         DGRF75  37",
"8  2     -1.00    -16.00      0.00      0.00                         DGRF75  38",
"8  3    -12.00      4.00      0.00      0.00                         DGRF75  39",
"8  4     -8.00    -19.00      0.00      0.00                         DGRF75  40",
"8  5      4.00      6.00      0.00      0.00                         DGRF75  41",
"8  6      0.00     18.00      0.00      0.00                         DGRF75  42",
"8  7     10.00    -10.00      0.00      0.00                         DGRF75  43",
"8  8      1.00    -17.00      0.00      0.00                         DGRF75  44",
"9  0      7.00      0.00      0.00      0.00                         DGRF75  45",
"9  1     10.00    -21.00      0.00      0.00                         DGRF75  46",
"9  2      2.00     16.00      0.00      0.00                         DGRF75  47",
"9  3    -12.00      7.00      0.00      0.00                         DGRF75  48",
"9  4     10.00     -4.00      0.00      0.00                         DGRF75  49",
"9  5     -1.00     -5.00      0.00      0.00                         DGRF75  50",
"9  6     -1.00     10.00      0.00      0.00                         DGRF75  51",
"9  7      4.00     11.00      0.00      0.00                         DGRF75  52",
"9  8      1.00     -3.00      0.00      0.00                         DGRF75  53",
"9  9     -2.00      1.00      0.00      0.00                         DGRF75  54",
"10  0     -3.00      0.00      0.00      0.00                         DGRF75  55",
"10  1     -3.00      1.00      0.00      0.00                         DGRF75  56",
"10  2      2.00      1.00      0.00      0.00                         DGRF75  57",
"10  3     -5.00      3.00      0.00      0.00                         DGRF75  58",
"10  4     -2.00      4.00      0.00      0.00                         DGRF75  59",
"10  5      5.00     -4.00      0.00      0.00                         DGRF75  60",
"10  6      4.00     -1.00      0.00      0.00                         DGRF75  61",
"10  7      1.00     -1.00      0.00      0.00                         DGRF75  62",
"10  8      0.00      3.00      0.00      0.00                         DGRF75  63",
"10  9      3.00      1.00      0.00      0.00                         DGRF75  64",
"10 10     -1.00     -5.00      0.00      0.00                         DGRF75  65",
"DGRF80  1980.00 10  0  0 1980.00 1985.00   -1.0  600.0           DGRF80   0",
"1  0 -29992.00      0.00      0.00      0.00                         DGRF80   1",
"1  1  -1956.00   5604.00      0.00      0.00                         DGRF80   2",
"2  0  -1997.00      0.00      0.00      0.00                         DGRF80   3",
"2  1   3027.00  -2129.00      0.00      0.00                         DGRF80   4",
"2  2   1663.00   -200.00      0.00      0.00                         DGRF80   5",
"3  0   1281.00      0.00      0.00      0.00                         DGRF80   6",
"3  1  -2180.00   -336.00      0.00      0.00                         DGRF80   7",
"3  2   1251.00    271.00      0.00      0.00                         DGRF80   8",
"3  3    833.00   -252.00      0.00      0.00                         DGRF80   9",
"4  0    938.00      0.00      0.00      0.00                         DGRF80  10",
"4  1    782.00    212.00      0.00      0.00                         DGRF80  11",
"4  2    398.00   -257.00      0.00      0.00                         DGRF80  12",
"4  3   -419.00     53.00      0.00      0.00                         DGRF80  13",
"4  4    199.00   -297.00      0.00      0.00                         DGRF80  14",
"5  0   -218.00      0.00      0.00      0.00                         DGRF80  15",
"5  1    357.00     46.00      0.00      0.00                         DGRF80  16",
"5  2    261.00    150.00      0.00      0.00                         DGRF80  17",
"5  3    -74.00   -151.00      0.00      0.00                         DGRF80  18",
"5  4   -162.00    -78.00      0.00      0.00                         DGRF80  19",
"5  5    -48.00     92.00      0.00      0.00                         DGRF80  20",
"6  0     48.00      0.00      0.00      0.00                         DGRF80  21",
"6  1     66.00    -15.00      0.00      0.00                         DGRF80  22",
"6  2     42.00     93.00      0.00      0.00                         DGRF80  23",
"6  3   -192.00     71.00      0.00      0.00                         DGRF80  24",
"6  4      4.00    -43.00      0.00      0.00                         DGRF80  25",
"6  5     14.00     -2.00      0.00      0.00                         DGRF80  26",
"6  6   -108.00     17.00      0.00      0.00                         DGRF80  27",
"7  0     72.00      0.00      0.00      0.00                         DGRF80  28",
"7  1    -59.00    -82.00      0.00      0.00                         DGRF80  29",
"7  2      2.00    -27.00      0.00      0.00                         DGRF80  30",
"7  3     21.00     -5.00      0.00      0.00                         DGRF80  31",
"7  4    -12.00     16.00      0.00      0.00                         DGRF80  32",
"7  5      1.00     18.00      0.00      0.00                         DGRF80  33",
"7  6     11.00    -23.00      0.00      0.00                         DGRF80  34",
"7  7     -2.00    -10.00      0.00      0.00                         DGRF80  35",
"8  0     18.00      0.00      0.00      0.00                         DGRF80  36",
"8  1      6.00      7.00      0.00      0.00                         DGRF80  37",
"8  2      0.00    -18.00      0.00      0.00                         DGRF80  38",
"8  3    -11.00      4.00      0.00      0.00                         DGRF80  39",
"8  4     -7.00    -22.00      0.00      0.00                         DGRF80  40",
"8  5      4.00      9.00      0.00      0.00                         DGRF80  41",
"8  6      3.00     16.00      0.00      0.00                         DGRF80  42",
"8  7      6.00    -13.00      0.00      0.00                         DGRF80  43",
"8  8     -1.00    -15.00      0.00      0.00                         DGRF80  44",
"9  0      5.00      0.00      0.00      0.00                         DGRF80  45",
"9  1     10.00    -21.00      0.00      0.00                         DGRF80  46",
"9  2      1.00     16.00      0.00      0.00                         DGRF80  47",
"9  3    -12.00      9.00      0.00      0.00                         DGRF80  48",
"9  4      9.00     -5.00      0.00      0.00                         DGRF80  49",
"9  5     -3.00     -6.00      0.00      0.00                         DGRF80  50",
"9  6     -1.00      9.00      0.00      0.00                         DGRF80  51",
"9  7      7.00     10.00      0.00      0.00                         DGRF80  52",
"9  8      2.00     -6.00      0.00      0.00                         DGRF80  53",
"9  9     -5.00      2.00      0.00      0.00                         DGRF80  54",
"10  0     -4.00      0.00      0.00      0.00                         DGRF80  55",
"10  1     -4.00      1.00      0.00      0.00                         DGRF80  56",
"10  2      2.00      0.00      0.00      0.00                         DGRF80  57",
"10  3     -5.00      3.00      0.00      0.00                         DGRF80  58",
"10  4     -2.00      6.00      0.00      0.00                         DGRF80  59",
"10  5      5.00     -4.00      0.00      0.00                         DGRF80  60",
"10  6      3.00      0.00      0.00      0.00                         DGRF80  61",
"10  7      1.00     -1.00      0.00      0.00                         DGRF80  62",
"10  8      2.00      4.00      0.00      0.00                         DGRF80  63",
"10  9      3.00      0.00      0.00      0.00                         DGRF80  64",
"10 10      0.00     -6.00      0.00      0.00                         DGRF80  65",
"DGRF85  1985.00 10  0  0 1985.00 1990.00   -1.0  600.0           DGRF85   0",
"1  0 -29873.00      0.00      0.00      0.00                         DGRF85   1",
"1  1  -1905.00   5500.00      0.00      0.00                         DGRF85   2",
"2  0  -2072.00      0.00      0.00      0.00                         DGRF85   3",
"2  1   3044.00  -2197.00      0.00      0.00                         DGRF85   4",
"2  2   1687.00   -306.00      0.00      0.00                         DGRF85   5",
"3  0   1296.00      0.00      0.00      0.00                         DGRF85   6",
"3  1  -2208.00   -310.00      0.00      0.00                         DGRF85   7",
"3  2   1247.00    284.00      0.00      0.00                         DGRF85   8",
"3  3    829.00   -297.00      0.00      0.00                         DGRF85   9",
"4  0    936.00      0.00      0.00      0.00                         DGRF85  10",
"4  1    780.00    232.00      0.00      0.00                         DGRF85  11",
"4  2    361.00   -249.00      0.00      0.00                         DGRF85  12",
"4  3   -424.00     69.00      0.00      0.00                         DGRF85  13",
"4  4    170.00   -297.00      0.00      0.00                         DGRF85  14",
"5  0   -214.00      0.00      0.00      0.00                         DGRF85  15",
"5  1    355.00     47.00      0.00      0.00                         DGRF85  16",
"5  2    253.00    150.00      0.00      0.00                         DGRF85  17",
"5  3    -93.00   -154.00      0.00      0.00                         DGRF85  18",
"5  4   -164.00    -75.00      0.00      0.00                         DGRF85  19",
"5  5    -46.00     95.00      0.00      0.00                         DGRF85  20",
"6  0     53.00      0.00      0.00      0.00                         DGRF85  21",
"6  1     65.00    -16.00      0.00      0.00                         DGRF85  22",
"6  2     51.00     88.00      0.00      0.00                         DGRF85  23",
"6  3   -185.00     69.00      0.00      0.00                         DGRF85  24",
"6  4      4.00    -48.00      0.00      0.00                         DGRF85  25",
"6  5     16.00     -1.00      0.00      0.00                         DGRF85  26",
"6  6   -102.00     21.00      0.00      0.00                         DGRF85  27",
"7  0     74.00      0.00      0.00      0.00                         DGRF85  28",
"7  1    -62.00    -83.00      0.00      0.00                         DGRF85  29",
"7  2      3.00    -27.00      0.00      0.00                         DGRF85  30",
"7  3     24.00     -2.00      0.00      0.00                         DGRF85  31",
"7  4     -6.00     20.00      0.00      0.00                         DGRF85  32",
"7  5      4.00     17.00      0.00      0.00                         DGRF85  33",
"7  6     10.00    -23.00      0.00      0.00                         DGRF85  34",
"7  7      0.00     -7.00      0.00      0.00                         DGRF85  35",
"8  0     21.00      0.00      0.00      0.00                         DGRF85  36",
"8  1      6.00      8.00      0.00      0.00                         DGRF85  37",
"8  2      0.00    -19.00      0.00      0.00                         DGRF85  38",
"8  3    -11.00      5.00      0.00      0.00                         DGRF85  39",
"8  4     -9.00    -23.00      0.00      0.00                         DGRF85  40",
"8  5      4.00     11.00      0.00      0.00                         DGRF85  41",
"8  6      4.00     14.00      0.00      0.00                         DGRF85  42",
"8  7      4.00    -15.00      0.00      0.00                         DGRF85  43",
"8  8     -4.00    -11.00      0.00      0.00                         DGRF85  44",
"9  0      5.00      0.00      0.00      0.00                         DGRF85  45",
"9  1     10.00    -21.00      0.00      0.00                         DGRF85  46",
"9  2      1.00     15.00      0.00      0.00                         DGRF85  47",
"9  3    -12.00      9.00      0.00      0.00                         DGRF85  48",
"9  4      9.00     -6.00      0.00      0.00                         DGRF85  49",
"9  5     -3.00     -6.00      0.00      0.00                         DGRF85  50",
"9  6     -1.00      9.00      0.00      0.00                         DGRF85  51",
"9  7      7.00      9.00      0.00      0.00                         DGRF85  52",
"9  8      1.00     -7.00      0.00      0.00                         DGRF85  53",
"9  9     -5.00      2.00      0.00      0.00                         DGRF85  54",
"10  0     -4.00      0.00      0.00      0.00                         DGRF85  55",
"10  1     -4.00      1.00      0.00      0.00                         DGRF85  56",
"10  2      3.00      0.00      0.00      0.00                         DGRF85  57",
"10  3     -5.00      3.00      0.00      0.00                         DGRF85  58",
"10  4     -2.00      6.00      0.00      0.00                         DGRF85  59",
"10  5      5.00     -4.00      0.00      0.00                         DGRF85  60",
"10  6      3.00      0.00      0.00      0.00                         DGRF85  61",
"10  7      1.00     -1.00      0.00      0.00                         DGRF85  62",
"10  8      2.00      4.00      0.00      0.00                         DGRF85  63",
"10  9      3.00      0.00      0.00      0.00                         DGRF85  64",
"10 10      0.00     -6.00      0.00      0.00                         DGRF85  65",
"DGRF90  1990.00 10  0  0 1990.00 1995.00   -1.0  600.0           DGRF90   0",
"1  0 -29775.00      0.00      0.00      0.00                         DGRF90   1",
"1  1  -1848.00   5406.00      0.00      0.00                         DGRF90   2",
"2  0  -2131.00      0.00      0.00      0.00                         DGRF90   3",
"2  1   3059.00  -2279.00      0.00      0.00                         DGRF90   4",
"2  2   1686.00   -373.00      0.00      0.00                         DGRF90   5",
"3  0   1314.00      0.00      0.00      0.00                         DGRF90   6",
"3  1  -2239.00   -284.00      0.00      0.00                         DGRF90   7",
"3  2   1248.00    293.00      0.00      0.00                         DGRF90   8",
"3  3    802.00   -352.00      0.00      0.00                         DGRF90   9",
"4  0    939.00      0.00      0.00      0.00                         DGRF90  10",
"4  1    780.00    247.00      0.00      0.00                         DGRF90  11",
"4  2    325.00   -240.00      0.00      0.00                         DGRF90  12",
"4  3   -423.00     84.00      0.00      0.00                         DGRF90  13",
"4  4    141.00   -299.00      0.00      0.00                         DGRF90  14",
"5  0   -214.00      0.00      0.00      0.00                         DGRF90  15",
"5  1    353.00     46.00      0.00      0.00                         DGRF90  16",
"5  2    245.00    154.00      0.00      0.00                         DGRF90  17",
"5  3   -109.00   -153.00      0.00      0.00                         DGRF90  18",
"5  4   -165.00    -69.00      0.00      0.00                         DGRF90  19",
"5  5    -36.00     97.00      0.00      0.00                         DGRF90  20",
"6  0     61.00      0.00      0.00      0.00                         DGRF90  21",
"6  1     65.00    -16.00      0.00      0.00                         DGRF90  22",
"6  2     59.00     82.00      0.00      0.00                         DGRF90  23",
"6  3   -178.00     69.00      0.00      0.00                         DGRF90  24",
"6  4      3.00    -52.00      0.00      0.00                         DGRF90  25",
"6  5     18.00      1.00      0.00      0.00                         DGRF90  26",
"6  6    -96.00     24.00      0.00      0.00                         DGRF90  27",
"7  0     77.00      0.00      0.00      0.00                         DGRF90  28",
"7  1    -64.00    -80.00      0.00      0.00                         DGRF90  29",
"7  2      2.00    -26.00      0.00      0.00                         DGRF90  30",
"7  3     26.00      0.00      0.00      0.00                         DGRF90  31",
"7  4     -1.00     21.00      0.00      0.00                         DGRF90  32",
"7  5      5.00     17.00      0.00      0.00                         DGRF90  33",
"7  6      9.00    -23.00      0.00      0.00                         DGRF90  34",
"7  7      0.00     -4.00      0.00      0.00                         DGRF90  35",
"8  0     23.00      0.00      0.00      0.00                         DGRF90  36",
"8  1      5.00     10.00      0.00      0.00                         DGRF90  37",
"8  2     -1.00    -19.00      0.00      0.00                         DGRF90  38",
"8  3    -10.00      6.00      0.00      0.00                         DGRF90  39",
"8  4    -12.00    -22.00      0.00      0.00                         DGRF90  40",
"8  5      3.00     12.00      0.00      0.00                         DGRF90  41",
"8  6      4.00     12.00      0.00      0.00                         DGRF90  42",
"8  7      2.00    -16.00      0.00      0.00                         DGRF90  43",
"8  8     -6.00    -10.00      0.00      0.00                         DGRF90  44",
"9  0      4.00      0.00      0.00      0.00                         DGRF90  45",
"9  1      9.00    -20.00      0.00      0.00                         DGRF90  46",
"9  2      1.00     15.00      0.00      0.00                         DGRF90  47",
"9  3    -12.00     11.00      0.00      0.00                         DGRF90  48",
"9  4      9.00     -7.00      0.00      0.00                         DGRF90  49",
"9  5     -4.00     -7.00      0.00      0.00                         DGRF90  50",
"9  6     -2.00      9.00      0.00      0.00                         DGRF90  51",
"9  7      7.00      8.00      0.00      0.00                         DGRF90  52",
"9  8      1.00     -7.00      0.00      0.00                         DGRF90  53",
"9  9     -6.00      2.00      0.00      0.00                         DGRF90  54",
"10  0     -3.00      0.00      0.00      0.00                         DGRF90  55",
"10  1     -4.00      2.00      0.00      0.00                         DGRF90  56",
"10  2      2.00      1.00      0.00      0.00                         DGRF90  57",
"10  3     -5.00      3.00      0.00      0.00                         DGRF90  58",
"10  4     -2.00      6.00      0.00      0.00                         DGRF90  59",
"10  5      4.00     -4.00      0.00      0.00                         DGRF90  60",
"10  6      3.00      0.00      0.00      0.00                         DGRF90  61",
"10  7      1.00     -2.00      0.00      0.00                         DGRF90  62",
"10  8      3.00      3.00      0.00      0.00                         DGRF90  63",
"10  9      3.00     -1.00      0.00      0.00                         DGRF90  64",
"10 10      0.00     -6.00      0.00      0.00                         DGRF90  65",
"DGRF95  1995.00 10  0  0 1995.00 2000.00   -1.0  600.0           DGRF95   0",
"1  0 -29692.00      0.00      0.00      0.00                         DGRF95   1",
"1  1  -1784.00   5306.00      0.00      0.00                         DGRF95   2",
"2  0  -2200.00      0.00      0.00      0.00                         DGRF95   3",
"2  1   3070.00  -2366.00      0.00      0.00                         DGRF95   4",
"2  2   1681.00   -413.00      0.00      0.00                         DGRF95   5",
"3  0   1335.00      0.00      0.00      0.00                         DGRF95   6",
"3  1  -2267.00   -262.00      0.00      0.00                         DGRF95   7",
"3  2   1249.00    302.00      0.00      0.00                         DGRF95   8",
"3  3    759.00   -427.00      0.00      0.00                         DGRF95   9",
"4  0    940.00      0.00      0.00      0.00                         DGRF95  10",
"4  1    780.00    262.00      0.00      0.00                         DGRF95  11",
"4  2    290.00   -236.00      0.00      0.00                         DGRF95  12",
"4  3   -418.00     97.00      0.00      0.00                         DGRF95  13",
"4  4    122.00   -306.00      0.00      0.00                         DGRF95  14",
"5  0   -214.00      0.00      0.00      0.00                         DGRF95  15",
"5  1    352.00     46.00      0.00      0.00                         DGRF95  16",
"5  2    235.00    165.00      0.00      0.00                         DGRF95  17",
"5  3   -118.00   -143.00      0.00      0.00                         DGRF95  18",
"5  4   -166.00    -55.00      0.00      0.00                         DGRF95  19",
"5  5    -17.00    107.00      0.00      0.00                         DGRF95  20",
"6  0     68.00      0.00      0.00      0.00                         DGRF95  21",
"6  1     67.00    -17.00      0.00      0.00                         DGRF95  22",
"6  2     68.00     72.00      0.00      0.00                         DGRF95  23",
"6  3   -170.00     67.00      0.00      0.00                         DGRF95  24",
"6  4     -1.00    -58.00      0.00      0.00                         DGRF95  25",
"6  5     19.00      1.00      0.00      0.00                         DGRF95  26",
"6  6    -93.00     36.00      0.00      0.00                         DGRF95  27",
"7  0     77.00      0.00      0.00      0.00                         DGRF95  28",
"7  1    -72.00    -69.00      0.00      0.00                         DGRF95  29",
"7  2      1.00    -25.00      0.00      0.00                         DGRF95  30",
"7  3     28.00      4.00      0.00      0.00                         DGRF95  31",
"7  4      5.00     24.00      0.00      0.00                         DGRF95  32",
"7  5      4.00     17.00      0.00      0.00                         DGRF95  33",
"7  6      8.00    -24.00      0.00      0.00                         DGRF95  34",
"7  7     -2.00     -6.00      0.00      0.00                         DGRF95  35",
"8  0     25.00      0.00      0.00      0.00                         DGRF95  36",
"8  1      6.00     11.00      0.00      0.00                         DGRF95  37",
"8  2     -6.00    -21.00      0.00      0.00                         DGRF95  38",
"8  3     -9.00      8.00      0.00      0.00                         DGRF95  39",
"8  4    -14.00    -23.00      0.00      0.00                         DGRF95  40",
"8  5      9.00     15.00      0.00      0.00                         DGRF95  41",
"8  6      6.00     11.00      0.00      0.00                         DGRF95  42",
"8  7     -5.00    -16.00      0.00      0.00                         DGRF95  43",
"8  8     -7.00     -4.00      0.00      0.00                         DGRF95  44",
"9  0      4.00      0.00      0.00      0.00                         DGRF95  45",
"9  1      9.00    -20.00      0.00      0.00                         DGRF95  46",
"9  2      3.00     15.00      0.00      0.00                         DGRF95  47",
"9  3    -10.00     12.00      0.00      0.00                         DGRF95  48",
"9  4      8.00     -6.00      0.00      0.00                         DGRF95  49",
"9  5     -8.00     -8.00      0.00      0.00                         DGRF95  50",
"9  6     -1.00      8.00      0.00      0.00                         DGRF95  51",
"9  7     10.00      5.00      0.00      0.00                         DGRF95  52",
"9  8     -2.00     -8.00      0.00      0.00                         DGRF95  53",
"9  9     -8.00      3.00      0.00      0.00                         DGRF95  54",
"10  0     -3.00      0.00      0.00      0.00                         DGRF95  55",
"10  1     -6.00      1.00      0.00      0.00                         DGRF95  56",
"10  2      2.00      0.00      0.00      0.00                         DGRF95  57",
"10  3     -4.00      4.00      0.00      0.00                         DGRF95  58",
"10  4     -1.00      5.00      0.00      0.00                         DGRF95  59",
"10  5      4.00     -5.00      0.00      0.00                         DGRF95  60",
"10  6      2.00     -1.00      0.00      0.00                         DGRF95  61",
"10  7      2.00     -2.00      0.00      0.00                         DGRF95  62",
"10  8      5.00      1.00      0.00      0.00                         DGRF95  63",
"10  9      1.00     -2.00      0.00      0.00                         DGRF95  64",
"10 10      0.00     -7.00      0.00      0.00                         DGRF95  65",
"DGRF2000  2000.00 13  0  0 2000.00 2005.00   -1.0  600.0         DGRF2000   0",
"1  0 -29619.40      0.00      0.00      0.00                       DGRF2000   1",
"1  1  -1728.20   5186.10      0.00      0.00                       DGRF2000   2",
"2  0  -2267.70      0.00      0.00      0.00                       DGRF2000   3",
"2  1   3068.40  -2481.60      0.00      0.00                       DGRF2000   4",
"2  2   1670.90   -458.00      0.00      0.00                       DGRF2000   5",
"3  0   1339.60      0.00      0.00      0.00                       DGRF2000   6",
"3  1  -2288.00   -227.60      0.00      0.00                       DGRF2000   7",
"3  2   1252.10    293.40      0.00      0.00                       DGRF2000   8",
"3  3    714.50   -491.10      0.00      0.00                       DGRF2000   9",
"4  0    932.30      0.00      0.00      0.00                       DGRF2000  10",
"4  1    786.80    272.60      0.00      0.00                       DGRF2000  11",
"4  2    250.00   -231.90      0.00      0.00                       DGRF2000  12",
"4  3   -403.00    119.80      0.00      0.00                       DGRF2000  13",
"4  4    111.30   -303.80      0.00      0.00                       DGRF2000  14",
"5  0   -218.80      0.00      0.00      0.00                       DGRF2000  15",
"5  1    351.40     43.80      0.00      0.00                       DGRF2000  16",
"5  2    222.30    171.90      0.00      0.00                       DGRF2000  17",
"5  3   -130.40   -133.10      0.00      0.00                       DGRF2000  18",
"5  4   -168.60    -39.30      0.00      0.00                       DGRF2000  19",
"5  5    -12.90    106.30      0.00      0.00                       DGRF2000  20",
"6  0     72.30      0.00      0.00      0.00                       DGRF2000  21",
"6  1     68.20    -17.40      0.00      0.00                       DGRF2000  22",
"6  2     74.20     63.70      0.00      0.00                       DGRF2000  23",
"6  3   -160.90     65.10      0.00      0.00                       DGRF2000  24",
"6  4     -5.90    -61.20      0.00      0.00                       DGRF2000  25",
"6  5     16.90      0.70      0.00      0.00                       DGRF2000  26",
"6  6    -90.40     43.80      0.00      0.00                       DGRF2000  27",
"7  0     79.00      0.00      0.00      0.00                       DGRF2000  28",
"7  1    -74.00    -64.60      0.00      0.00                       DGRF2000  29",
"7  2      0.00    -24.20      0.00      0.00                       DGRF2000  30",
"7  3     33.30      6.20      0.00      0.00                       DGRF2000  31",
"7  4      9.10     24.00      0.00      0.00                       DGRF2000  32",
"7  5      6.90     14.80      0.00      0.00                       DGRF2000  33",
"7  6      7.30    -25.40      0.00      0.00                       DGRF2000  34",
"7  7     -1.20     -5.80      0.00      0.00                       DGRF2000  35",
"8  0     24.40      0.00      0.00      0.00                       DGRF2000  36",
"8  1      6.60     11.90      0.00      0.00                       DGRF2000  37",
"8  2     -9.20    -21.50      0.00      0.00                       DGRF2000  38",
"8  3     -7.90      8.50      0.00      0.00                       DGRF2000  39",
"8  4    -16.60    -21.50      0.00      0.00                       DGRF2000  40",
"8  5      9.10     15.50      0.00      0.00                       DGRF2000  41",
"8  6      7.00      8.90      0.00      0.00                       DGRF2000  42",
"8  7     -7.90    -14.90      0.00      0.00                       DGRF2000  43",
"8  8     -7.00     -2.10      0.00      0.00                       DGRF2000  44",
"9  0      5.00      0.00      0.00      0.00                       DGRF2000  45",
"9  1      9.40    -19.70      0.00      0.00                       DGRF2000  46",
"9  2      3.00     13.40      0.00      0.00                       DGRF2000  47",
"9  3     -8.40     12.50      0.00      0.00                       DGRF2000  48",
"9  4      6.30     -6.20      0.00      0.00                       DGRF2000  49",
"9  5     -8.90     -8.40      0.00      0.00                       DGRF2000  50",
"9  6     -1.50      8.40      0.00      0.00                       DGRF2000  51",
"9  7      9.30      3.80      0.00      0.00                       DGRF2000  52",
"9  8     -4.30     -8.20      0.00      0.00                       DGRF2000  53",
"9  9     -8.20      4.80      0.00      0.00                       DGRF2000  54",
"10  0     -2.60      0.00      0.00      0.00                       DGRF2000  55",
"10  1     -6.00      1.70      0.00      0.00                       DGRF2000  56",
"10  2      1.70      0.00      0.00      0.00                       DGRF2000  57",
"10  3     -3.10      4.00      0.00      0.00                       DGRF2000  58",
"10  4     -0.50      4.90      0.00      0.00                       DGRF2000  59",
"10  5      3.70     -5.90      0.00      0.00                       DGRF2000  60",
"10  6      1.00     -1.20      0.00      0.00                       DGRF2000  61",
"10  7      2.00     -2.90      0.00      0.00                       DGRF2000  62",
"10  8      4.20      0.20      0.00      0.00                       DGRF2000  63",
"10  9      0.30     -2.20      0.00      0.00                       DGRF2000  64",
"10 10     -1.10     -7.40      0.00      0.00                       DGRF2000  65",
"11  0      2.70      0.00      0.00      0.00                       DGRF2000  66",
"11  1     -1.70      0.10      0.00      0.00                       DGRF2000  67",
"11  2     -1.90      1.30      0.00      0.00                       DGRF2000  68",
"11  3      1.50     -0.90      0.00      0.00                       DGRF2000  69",
"11  4     -0.10     -2.60      0.00      0.00                       DGRF2000  70",
"11  5      0.10      0.90      0.00      0.00                       DGRF2000  71",
"11  6     -0.70     -0.70      0.00      0.00                       DGRF2000  72",
"11  7      0.70     -2.80      0.00      0.00                       DGRF2000  73",
"11  8      1.70     -0.90      0.00      0.00                       DGRF2000  74",
"11  9      0.10     -1.20      0.00      0.00                       DGRF2000  75",
"11 10      1.20     -1.90      0.00      0.00                       DGRF2000  76",
"11 11      4.00     -0.90      0.00      0.00                       DGRF2000  77",
"12  0     -2.20      0.00      0.00      0.00                       DGRF2000  78",
"12  1     -0.30     -0.40      0.00      0.00                       DGRF2000  79",
"12  2      0.20      0.30      0.00      0.00                       DGRF2000  80",
"12  3      0.90      2.50      0.00      0.00                       DGRF2000  81",
"12  4     -0.20     -2.60      0.00      0.00                       DGRF2000  82",
"12  5      0.90      0.70      0.00      0.00                       DGRF2000  83",
"12  6     -0.50      0.30      0.00      0.00                       DGRF2000  84",
"12  7      0.30      0.00      0.00      0.00                       DGRF2000  85",
"12  8     -0.30      0.00      0.00      0.00                       DGRF2000  86",
"12  9     -0.40      0.30      0.00      0.00                       DGRF2000  87",
"12 10     -0.10     -0.90      0.00      0.00                       DGRF2000  88",
"12 11     -0.20     -0.40      0.00      0.00                       DGRF2000  89",
"12 12     -0.40      0.80      0.00      0.00                       DGRF2000  90",
"13  0     -0.20      0.00      0.00      0.00                       DGRF2000  91",
"13  1     -0.90     -0.90      0.00      0.00                       DGRF2000  92",
"13  2      0.30      0.20      0.00      0.00                       DGRF2000  93",
"13  3      0.10      1.80      0.00      0.00                       DGRF2000  94",
"13  4     -0.40     -0.40      0.00      0.00                       DGRF2000  95",
"13  5      1.30     -1.00      0.00      0.00                       DGRF2000  96",
"13  6     -0.40     -0.10      0.00      0.00                       DGRF2000  97",
"13  7      0.70      0.70      0.00      0.00                       DGRF2000  98",
"13  8     -0.40      0.30      0.00      0.00                       DGRF2000  99",
"13  9      0.30      0.60      0.00      0.00                       DGRF2000 100",
"13 10     -0.10      0.30      0.00      0.00                       DGRF2000 101",
"13 11      0.40     -0.20      0.00      0.00                       DGRF2000 102",
"13 12      0.00     -0.50      0.00      0.00                       DGRF2000 103",
"13 13      0.10     -0.90      0.00      0.00                       DGRF2000 104",
"DGRF2005  2005.00 13  0  0 2005.00 2010.00   -1.0  600.0         DGRF2005   0",
"1  0 -29554.63      0.00      0.00      0.00                       IGRF2005   1",
"1  1  -1669.05   5077.99      0.00      0.00                       IGRF2005   2",
"2  0  -2337.24      0.00      0.00      0.00                       IGRF2005   3",
"2  1   3047.69  -2594.50      0.00      0.00                       IGRF2005   4",
"2  2   1657.76   -515.43      0.00      0.00                       IGRF2005   5",
"3  0   1336.30      0.00      0.00      0.00                       IGRF2005   6",
"3  1  -2305.83   -198.86      0.00      0.00                       IGRF2005   7",
"3  2   1246.39    269.72      0.00      0.00                       IGRF2005   8",
"3  3    672.51   -524.72      0.00      0.00                       IGRF2005   9",
"4  0    920.55      0.00      0.00      0.00                       IGRF2005  10",
"4  1    797.96    282.07      0.00      0.00                       IGRF2005  11",
"4  2    210.65   -225.23      0.00      0.00                       IGRF2005  12",
"4  3   -379.86    145.15      0.00      0.00                       IGRF2005  13",
"4  4    100.00   -305.36      0.00      0.00                       IGRF2005  14",
"5  0   -227.00      0.00      0.00      0.00                       IGRF2005  15",
"5  1    354.41     42.72      0.00      0.00                       IGRF2005  16",
"5  2    208.95    180.25      0.00      0.00                       IGRF2005  17",
"5  3   -136.54   -123.45      0.00      0.00                       IGRF2005  18",
"5  4   -168.05    -19.57      0.00      0.00                       IGRF2005  19",
"5  5    -13.55    103.85      0.00      0.00                       IGRF2005  20",
"6  0     73.60      0.00      0.00      0.00                       IGRF2005  21",
"6  1     69.56    -20.33      0.00      0.00                       IGRF2005  22",
"6  2     76.74     54.75      0.00      0.00                       IGRF2005  23",
"6  3   -151.34     63.63      0.00      0.00                       IGRF2005  24",
"6  4    -14.58    -63.53      0.00      0.00                       IGRF2005  25",
"6  5     14.58      0.24      0.00      0.00                       IGRF2005  26",
"6  6    -86.36     50.94      0.00      0.00                       IGRF2005  27",
"7  0     79.88      0.00      0.00      0.00                       IGRF2005  28",
"7  1    -74.46    -61.14      0.00      0.00                       IGRF2005  29",
"7  2     -1.65    -22.57      0.00      0.00                       IGRF2005  30",
"7  3     38.73      6.82      0.00      0.00                       IGRF2005  31",
"7  4     12.30     25.35      0.00      0.00                       IGRF2005  32",
"7  5      9.37     10.93      0.00      0.00                       IGRF2005  33",
"7  6      5.42    -26.32      0.00      0.00                       IGRF2005  34",
"7  7      1.94     -4.64      0.00      0.00                       IGRF2005  35",
"8  0     24.80      0.00      0.00      0.00                       IGRF2005  36",
"8  1      7.62     11.20      0.00      0.00                       IGRF2005  37",
"8  2    -11.73    -20.88      0.00      0.00                       IGRF2005  38",
"8  3     -6.88      9.83      0.00      0.00                       IGRF2005  39",
"8  4    -18.11    -19.71      0.00      0.00                       IGRF2005  40",
"8  5     10.17     16.22      0.00      0.00                       IGRF2005  41",
"8  6      9.36      7.61      0.00      0.00                       IGRF2005  42",
"8  7    -11.25    -12.76      0.00      0.00                       IGRF2005  43",
"8  8     -4.87     -0.06      0.00      0.00                       IGRF2005  44",
"9  0      5.58      0.00      0.00      0.00                       IGRF2005  45",
"9  1      9.76    -20.11      0.00      0.00                       IGRF2005  46",
"9  2      3.58     12.69      0.00      0.00                       IGRF2005  47",
"9  3     -6.94     12.67      0.00      0.00                       IGRF2005  48",
"9  4      5.01     -6.72      0.00      0.00                       IGRF2005  49",
"9  5    -10.76     -8.16      0.00      0.00                       IGRF2005  50",
"9  6     -1.25      8.10      0.00      0.00                       IGRF2005  51",
"9  7      8.76      2.92      0.00      0.00                       IGRF2005  52",
"9  8     -6.66     -7.73      0.00      0.00                       IGRF2005  53",
"9  9     -9.22      6.01      0.00      0.00                       IGRF2005  54",
"10  0     -2.17      0.00      0.00      0.00                       IGRF2005  55",
"10  1     -6.12      2.19      0.00      0.00                       IGRF2005  56",
"10  2      1.42      0.10      0.00      0.00                       IGRF2005  57",
"10  3     -2.35      4.46      0.00      0.00                       IGRF2005  58",
"10  4     -0.15      4.76      0.00      0.00                       IGRF2005  59",
"10  5      3.06     -6.58      0.00      0.00                       IGRF2005  60",
"10  6      0.29     -1.01      0.00      0.00                       IGRF2005  61",
"10  7      2.06     -3.47      0.00      0.00                       IGRF2005  62",
"10  8      3.77     -0.86      0.00      0.00                       IGRF2005  63",
"10  9     -0.21     -2.31      0.00      0.00                       IGRF2005  64",
"10 10     -2.09     -7.93      0.00      0.00                       IGRF2005  65",
"11  0      2.95      0.00      0.00      0.00                       IGRF2005  66",
"11  1     -1.60      0.26      0.00      0.00                       IGRF2005  67",
"11  2     -1.88      1.44      0.00      0.00                       IGRF2005  68",
"11  3      1.44     -0.77      0.00      0.00                       IGRF2005  69",
"11  4     -0.31     -2.27      0.00      0.00                       IGRF2005  70",
"11  5      0.29      0.90      0.00      0.00                       IGRF2005  71",
"11  6     -0.79     -0.58      0.00      0.00                       IGRF2005  72",
"11  7      0.53     -2.69      0.00      0.00                       IGRF2005  73",
"11  8      1.80     -1.08      0.00      0.00                       IGRF2005  74",
"11  9      0.16     -1.58      0.00      0.00                       IGRF2005  75",
"11 10      0.96     -1.90      0.00      0.00                       IGRF2005  76",
"11 11      3.99     -1.39      0.00      0.00                       IGRF2005  77",
"12  0     -2.15      0.00      0.00      0.00                       IGRF2005  78",
"12  1     -0.29     -0.55      0.00      0.00                       IGRF2005  79",
"12  2      0.21      0.23      0.00      0.00                       IGRF2005  80",
"12  3      0.89      2.38      0.00      0.00                       IGRF2005  81",
"12  4     -0.38     -2.63      0.00      0.00                       IGRF2005  82",
"12  5      0.96      0.61      0.00      0.00                       IGRF2005  83",
"12  6     -0.30      0.40      0.00      0.00                       IGRF2005  84",
"12  7      0.46      0.01      0.00      0.00                       IGRF2005  85",
"12  8     -0.35      0.02      0.00      0.00                       IGRF2005  86",
"12  9     -0.36      0.28      0.00      0.00                       IGRF2005  87",
"12 10      0.08     -0.87      0.00      0.00                       IGRF2005  88",
"12 11     -0.49     -0.34      0.00      0.00                       IGRF2005  89",
"12 12     -0.08      0.88      0.00      0.00                       IGRF2005  90",
"13  0     -0.16      0.00      0.00      0.00                       IGRF2005  91",
"13  1     -0.88     -0.76      0.00      0.00                       IGRF2005  92",
"13  2      0.30      0.33      0.00      0.00                       IGRF2005  93",
"13  3      0.28      1.72      0.00      0.00                       IGRF2005  94",
"13  4     -0.43     -0.54      0.00      0.00                       IGRF2005  95",
"13  5      1.18     -1.07      0.00      0.00                       IGRF2005  96",
"13  6     -0.37     -0.04      0.00      0.00                       IGRF2005  97",
"13  7      0.75      0.63      0.00      0.00                       IGRF2005  98",
"13  8     -0.26      0.21      0.00      0.00                       IGRF2005  99",
"13  9      0.35      0.53      0.00      0.00                       IGRF2005 100",
"13 10     -0.05      0.38      0.00      0.00                       IGRF2005 101",
"13 11      0.41     -0.22      0.00      0.00                       IGRF2005 102",
"13 12     -0.10     -0.57      0.00      0.00                       IGRF2005 103",
"13 13     -0.18     -0.82      0.00      0.00                       IGRF2005 104",
"DGRF2010  2010.00 13  0  0 2010.00 2015.00    -1.0 600.0         DGRF2010   0",
"1  0 -29496.57      0.00      0.00      0.00                       DGRF2010   1",
"1  1  -1586.42   4944.26      0.00      0.00                       DGRF2010   2",
"2  0  -2396.06      0.00      0.00      0.00                       DGRF2010   3",
"2  1   3026.34  -2708.54      0.00      0.00                       DGRF2010   4",
"2  2   1668.17   -575.73      0.00      0.00                       DGRF2010   5",
"3  0   1339.85      0.00      0.00      0.00                       DGRF2010   6",
"3  1  -2326.54   -160.40      0.00      0.00                       DGRF2010   7",
"3  2   1232.10    251.75      0.00      0.00                       DGRF2010   8",
"3  3    633.73   -537.03      0.00      0.00                       DGRF2010   9",
"4  0    912.66      0.00      0.00      0.00                       DGRF2010  10",
"4  1    808.97    286.48      0.00      0.00                       DGRF2010  11",
"4  2    166.58   -211.03      0.00      0.00                       DGRF2010  12",
"4  3   -356.83    164.46      0.00      0.00                       DGRF2010  13",
"4  4     89.40   -309.72      0.00      0.00                       DGRF2010  14",
"5  0   -230.87      0.00      0.00      0.00                       DGRF2010  15",
"5  1    357.29     44.58      0.00      0.00                       DGRF2010  16",
"5  2    200.26    189.01      0.00      0.00                       DGRF2010  17",
"5  3   -141.05   -118.06      0.00      0.00                       DGRF2010  18",
"5  4   -163.17     -0.01      0.00      0.00                       DGRF2010  19",
"5  5     -8.03    101.04      0.00      0.00                       DGRF2010  20",
"6  0     72.78      0.00      0.00      0.00                       DGRF2010  21",
"6  1     68.69    -20.90      0.00      0.00                       DGRF2010  22",
"6  2     75.92     44.18      0.00      0.00                       DGRF2010  23",
"6  3   -141.40     61.54      0.00      0.00                       DGRF2010  24",
"6  4    -22.83    -66.26      0.00      0.00                       DGRF2010  25",
"6  5     13.10      3.02      0.00      0.00                       DGRF2010  26",
"6  6    -78.09     55.40      0.00      0.00                       DGRF2010  27",
"7  0     80.44      0.00      0.00      0.00                       DGRF2010  28",
"7  1    -75.00    -57.80      0.00      0.00                       DGRF2010  29",
"7  2     -4.55    -21.20      0.00      0.00                       DGRF2010  30",
"7  3     45.24      6.54      0.00      0.00                       DGRF2010  31",
"7  4     14.00     24.96      0.00      0.00                       DGRF2010  32",
"7  5     10.46      7.03      0.00      0.00                       DGRF2010  33",
"7  6      1.64    -27.61      0.00      0.00                       DGRF2010  34",
"7  7      4.92     -3.28      0.00      0.00                       DGRF2010  35",
"8  0     24.41      0.00      0.00      0.00                       DGRF2010  36",
"8  1      8.21     10.84      0.00      0.00                       DGRF2010  37",
"8  2    -14.50    -20.03      0.00      0.00                       DGRF2010  38",
"8  3     -5.59     11.83      0.00      0.00                       DGRF2010  39",
"8  4    -19.34    -17.41      0.00      0.00                       DGRF2010  40",
"8  5     11.61     16.71      0.00      0.00                       DGRF2010  41",
"8  6     10.85      6.96      0.00      0.00                       DGRF2010  42",
"8  7    -14.05    -10.74      0.00      0.00                       DGRF2010  43",
"8  8     -3.54      1.64      0.00      0.00                       DGRF2010  44",
"9  0      5.50      0.00      0.00      0.00                       DGRF2010  45",
"9  1      9.45    -20.54      0.00      0.00                       DGRF2010  46",
"9  2      3.45     11.51      0.00      0.00                       DGRF2010  47",
"9  3     -5.27     12.75      0.00      0.00                       DGRF2010  48",
"9  4      3.13     -7.14      0.00      0.00                       DGRF2010  49",
"9  5    -12.38     -7.42      0.00      0.00                       DGRF2010  50",
"9  6     -0.76      7.97      0.00      0.00                       DGRF2010  51",
"9  7      8.43      2.14      0.00      0.00                       DGRF2010  52",
"9  8     -8.42     -6.08      0.00      0.00                       DGRF2010  53",
"9  9    -10.08      7.01      0.00      0.00                       DGRF2010  54",
"10  0     -1.94      0.00      0.00      0.00                       DGRF2010  55",
"10  1     -6.24      2.73      0.00      0.00                       DGRF2010  56",
"10  2      0.89     -0.10      0.00      0.00                       DGRF2010  57",
"10  3     -1.07      4.71      0.00      0.00                       DGRF2010  58",
"10  4     -0.16      4.44      0.00      0.00                       DGRF2010  59",
"10  5      2.45     -7.22      0.00      0.00                       DGRF2010  60",
"10  6     -0.33     -0.96      0.00      0.00                       DGRF2010  61",
"10  7      2.13     -3.95      0.00      0.00                       DGRF2010  62",
"10  8      3.09     -1.99      0.00      0.00                       DGRF2010  63",
"10  9     -1.03     -1.97      0.00      0.00                       DGRF2010  64",
"10 10     -2.80     -8.31      0.00      0.00                       DGRF2010  65",
"11  0      3.05      0.00      0.00      0.00                       DGRF2010  66",
"11  1     -1.48      0.13      0.00      0.00                       DGRF2010  67",
"11  2     -2.03      1.67      0.00      0.00                       DGRF2010  68",
"11  3      1.65     -0.66      0.00      0.00                       DGRF2010  69",
"11  4     -0.51     -1.76      0.00      0.00                       DGRF2010  70",
"11  5      0.54      0.85      0.00      0.00                       DGRF2010  71",
"11  6     -0.79     -0.39      0.00      0.00                       DGRF2010  72",
"11  7      0.37     -2.51      0.00      0.00                       DGRF2010  73",
"11  8      1.79     -1.27      0.00      0.00                       DGRF2010  74",
"11  9      0.12     -2.11      0.00      0.00                       DGRF2010  75",
"11 10      0.75     -1.94      0.00      0.00                       DGRF2010  76",
"11 11      3.75     -1.86      0.00      0.00                       DGRF2010  77",
"12  0     -2.12      0.00      0.00      0.00                       DGRF2010  78",
"12  1     -0.21     -0.87      0.00      0.00                       DGRF2010  79",
"12  2      0.30      0.27      0.00      0.00                       DGRF2010  80",
"12  3      1.04      2.13      0.00      0.00                       DGRF2010  81",
"12  4     -0.63     -2.49      0.00      0.00                       DGRF2010  82",
"12  5      0.95      0.49      0.00      0.00                       DGRF2010  83",
"12  6     -0.11      0.59      0.00      0.00                       DGRF2010  84",
"12  7      0.52      0.00      0.00      0.00                       DGRF2010  85",
"12  8     -0.39      0.13      0.00      0.00                       DGRF2010  86",
"12  9     -0.37      0.27      0.00      0.00                       DGRF2010  87",
"12 10      0.21     -0.86      0.00      0.00                       DGRF2010  88",
"12 11     -0.77     -0.23      0.00      0.00                       DGRF2010  89",
"12 12      0.04      0.87      0.00      0.00                       DGRF2010  90",
"13  0     -0.09      0.00      0.00      0.00                       DGRF2010  91",
"13  1     -0.89     -0.87      0.00      0.00                       DGRF2010  92",
"13  2      0.31      0.30      0.00      0.00                       DGRF2010  93",
"13  3      0.42      1.66      0.00      0.00                       DGRF2010  94",
"13  4     -0.45     -0.59      0.00      0.00                       DGRF2010  95",
"13  5      1.08     -1.14      0.00      0.00                       DGRF2010  96",
"13  6     -0.31     -0.07      0.00      0.00                       DGRF2010  97",
"13  7      0.78      0.54      0.00      0.00                       DGRF2010  98",
"13  8     -0.18      0.10      0.00      0.00                       DGRF2010  99",
"13  9      0.38      0.49      0.00      0.00                       DGRF2010 100",
"13 10      0.02      0.44      0.00      0.00                       DGRF2010 101",
"13 11      0.42     -0.25      0.00      0.00                       DGRF2010 102",
"13 12     -0.26     -0.53      0.00      0.00                       DGRF2010 103",
"13 13     -0.26     -0.79      0.00      0.00                       DGRF2010 104",
"IGRF2015  2015.00 13  8  0 2015.00 2020.00    -1.0 600.0         IGRF2015   0",
"1  0 -29442.00      0.00     10.30      0.00                       IGRF2015   1",
"1  1  -1501.00   4797.10     18.10    -26.60                       IGRF2015   2",
"2  0  -2445.10      0.00     -8.70      0.00                       IGRF2015   3",
"2  1   3012.90  -2845.60     -3.30    -27.40                       IGRF2015   4",
"2  2   1676.70   -641.90      2.10    -14.10                       IGRF2015   5",
"3  0   1350.70      0.00      3.40      0.00                       IGRF2015   6",
"3  1  -2352.30   -115.30     -5.50      8.20                       IGRF2015   7",
"3  2   1225.60    244.90     -0.70     -0.40                       IGRF2015   8",
"3  3    582.00   -538.40    -10.10      1.80                       IGRF2015   9",
"4  0    907.60      0.00     -0.70      0.00                       IGRF2015  10",
"4  1    813.70    283.30      0.20     -1.30                       IGRF2015  11",
"4  2    120.40   -188.70     -9.10      5.30                       IGRF2015  12",
"4  3   -334.90    180.90      4.10      2.90                       IGRF2015  13",
"4  4     70.40   -329.50     -4.30     -5.20                       IGRF2015  14",
"5  0   -232.60      0.00     -0.20      0.00                       IGRF2015  15",
"5  1    360.10     47.30      0.50      0.60                       IGRF2015  16",
"5  2    192.40    197.00     -1.30      1.70                       IGRF2015  17",
"5  3   -140.90   -119.30     -0.10     -1.20                       IGRF2015  18",
"5  4   -157.50     16.00      1.40      3.40                       IGRF2015  19",
"5  5      4.10    100.20      3.90      0.00                       IGRF2015  20",
"6  0     70.00      0.00     -0.30      0.00                       IGRF2015  21",
"6  1     67.70    -20.80     -0.10      0.00                       IGRF2015  22",
"6  2     72.70     33.20     -0.70     -2.10                       IGRF2015  23",
"6  3   -129.90     58.90      2.10     -0.70                       IGRF2015  24",
"6  4    -28.90    -66.70     -1.20      0.20                       IGRF2015  25",
"6  5     13.20      7.30      0.30      0.90                       IGRF2015  26",
"6  6    -70.90     62.60      1.60      1.00                       IGRF2015  27",
"7  0     81.60      0.00      0.30      0.00                       IGRF2015  28",
"7  1    -76.10    -54.10     -0.20      0.80                       IGRF2015  29",
"7  2     -6.80    -19.50     -0.50      0.40                       IGRF2015  30",
"7  3     51.80      5.70      1.30     -0.20                       IGRF2015  31",
"7  4     15.00     24.40      0.10     -0.30                       IGRF2015  32",
"7  5      9.40      3.40     -0.60     -0.60                       IGRF2015  33",
"7  6     -2.80    -27.40     -0.80      0.10                       IGRF2015  34",
"7  7      6.80     -2.20      0.20     -0.20                       IGRF2015  35",
"8  0     24.20      0.00      0.20      0.00                       IGRF2015  36",
"8  1      8.80     10.10      0.00     -0.30                       IGRF2015  37",
"8  2    -16.90    -18.30     -0.60      0.30                       IGRF2015  38",
"8  3     -3.20     13.30      0.50      0.10                       IGRF2015  39",
"8  4    -20.60    -14.60     -0.20      0.50                       IGRF2015  40",
"8  5     13.40     16.20      0.40     -0.20                       IGRF2015  41",
"8  6     11.70      5.70      0.10     -0.30                       IGRF2015  42",
"8  7    -15.90     -9.10     -0.40      0.30                       IGRF2015  43",
"8  8     -2.00      2.10      0.30      0.00                       IGRF2015  44",
"9  0      5.40      0.00      0.00      0.00                       IGRF2015  45",
"9  1      8.80    -21.60      0.00      0.00                       IGRF2015  46",
"9  2      3.10     10.80      0.00      0.00                       IGRF2015  47",
"9  3     -3.30     11.80      0.00      0.00                       IGRF2015  48",
"9  4      0.70     -6.80      0.00      0.00                       IGRF2015  49",
"9  5    -13.30     -6.90      0.00      0.00                       IGRF2015  50",
"9  6     -0.10      7.80      0.00      0.00                       IGRF2015  51",
"9  7      8.70      1.00      0.00      0.00                       IGRF2015  52",
"9  8     -9.10     -4.00      0.00      0.00                       IGRF2015  53",
"9  9    -10.50      8.40      0.00      0.00                       IGRF2015  54",
"10  0     -1.90      0.00      0.00      0.00                       IGRF2015  55",
"10  1     -6.30      3.20      0.00      0.00                       IGRF2015  56",
"10  2      0.10     -0.40      0.00      0.00                       IGRF2015  57",
"10  3      0.50      4.60      0.00      0.00                       IGRF2015  58",
"10  4     -0.50      4.40      0.00      0.00                       IGRF2015  59",
"10  5      1.80     -7.90      0.00      0.00                       IGRF2015  60",
"10  6     -0.70     -0.60      0.00      0.00                       IGRF2015  61",
"10  7      2.10     -4.20      0.00      0.00                       IGRF2015  62",
"10  8      2.40     -2.80      0.00      0.00                       IGRF2015  63",
"10  9     -1.80     -1.20      0.00      0.00                       IGRF2015  64",
"10 10     -3.60     -8.70      0.00      0.00                       IGRF2015  65",
"11  0      3.10      0.00      0.00      0.00                       IGRF2015  66",
"11  1     -1.50     -0.10      0.00      0.00                       IGRF2015  67",
"11  2     -2.30      2.00      0.00      0.00                       IGRF2015  68",
"11  3      2.00     -0.70      0.00      0.00                       IGRF2015  69",
"11  4     -0.80     -1.10      0.00      0.00                       IGRF2015  70",
"11  5      0.60      0.80      0.00      0.00                       IGRF2015  71",
"11  6     -0.70     -0.20      0.00      0.00                       IGRF2015  72",
"11  7      0.20     -2.20      0.00      0.00                       IGRF2015  73",
"11  8      1.70     -1.40      0.00      0.00                       IGRF2015  74",
"11  9     -0.20     -2.50      0.00      0.00                       IGRF2015  75",
"11 10      0.40     -2.00      0.00      0.00                       IGRF2015  76",
"11 11      3.50     -2.40      0.00      0.00                       IGRF2015  77",
"12  0     -1.90      0.00      0.00      0.00                       IGRF2015  78",
"12  1     -0.20     -1.10      0.00      0.00                       IGRF2015  79",
"12  2      0.40      0.40      0.00      0.00                       IGRF2015  80",
"12  3      1.20      1.90      0.00      0.00                       IGRF2015  81",
"12  4     -0.80     -2.20      0.00      0.00                       IGRF2015  82",
"12  5      0.90      0.30      0.00      0.00                       IGRF2015  83",
"12  6      0.10      0.70      0.00      0.00                       IGRF2015  84",
"12  7      0.50     -0.10      0.00      0.00                       IGRF2015  85",
"12  8     -0.30      0.30      0.00      0.00                       IGRF2015  86",
"12  9     -0.40      0.20      0.00      0.00                       IGRF2015  87",
"12 10      0.20     -0.90      0.00      0.00                       IGRF2015  88",
"12 11     -0.90     -0.10      0.00      0.00                       IGRF2015  89",
"12 12      0.00      0.70      0.00      0.00                       IGRF2015  90",
"13  0      0.00      0.00      0.00      0.00                       IGRF2015  91",
"13  1     -0.90     -0.90      0.00      0.00                       IGRF2015  92",
"13  2      0.40      0.40      0.00      0.00                       IGRF2015  93",
"13  3      0.50      1.60      0.00      0.00                       IGRF2015  94",
"13  4     -0.50     -0.50      0.00      0.00                       IGRF2015  95",
"13  5      1.00     -1.20      0.00      0.00                       IGRF2015  96",
"13  6     -0.20     -0.10      0.00      0.00                       IGRF2015  97",
"13  7      0.80      0.40      0.00      0.00                       IGRF2015  98",
"13  8     -0.10     -0.10      0.00      0.00                       IGRF2015  99",
"13  9      0.30      0.40      0.00      0.00                       IGRF2015 100",
"13 10      0.10      0.50      0.00      0.00                       IGRF2015 101",
"13 11      0.50     -0.30      0.00      0.00                       IGRF2015 102",
"13 12     -0.40     -0.40      0.00      0.00                       IGRF2015 103",
"13 13     -0.30     -0.80      0.00      0.00                       IGRF2015 104"});
#endregion
    }
}
