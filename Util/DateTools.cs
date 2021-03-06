﻿using System;
using System.Linq;

namespace HlidacStatu.Util
{
    public static class DateTools
    {
        public enum DateTimePart
        {
            Year = 1,
            Month = 2,
            Day = 3,
            Hour = 4,
            Minute = 5,
            Second = 6
        }



        // https://cs.wikipedia.org/wiki/Přestupný_rok
        static int[] prestupneRoky = new int[] { 1904, 2000, 2096, 2196, 2296, 2396, 1908, 2004, 2104, 2204, 2304, 2400, 1912, 2008, 2108, 2208, 2308, 2404, 1916, 2012, 2112, 2212, 2312, 2408, 1920, 2016, 2116, 2216, 2316, 2412, 1924, 2020, 2120, 2220, 2320, 2416, 1928, 2024, 2124, 2224, 2324, 2420, 1932, 2028, 2128, 2228, 2328, 2424, 1936, 2032, 2132, 2232, 2332, 2428, 1940, 2036, 2136, 2236, 2336, 2432, 1944, 2040, 2140, 2240, 2340, 2436, 1948, 2044, 2144, 2244, 2344, 2440, 1952, 2048, 2148, 2248, 2348, 2444, 1956, 2052, 2152, 2252, 2352, 2448, 1960, 2056, 2156, 2256, 2356, 2452, 1964, 2060, 2160, 2260, 2360, 2456, 1968, 2064, 2164, 2264, 2364, 2460, 1972, 2068, 2168, 2268, 2368, 2464, 1976, 2072, 2172, 2272, 2372, 2468, 1980, 2076, 2176, 2276, 2376, 2472, 1984, 2080, 2180, 2280, 2380, 2476, 1988, 2084, 2184, 2284, 2384, 2480, 1992, 2088, 2188, 2288, 2388, 2484, 1996, 2092, 2192, 2292, 2392, 2488 };
        public static bool IsPrestupnyRok(int rok)
        {
            return prestupneRoky.Contains(rok);
        }


        public static string DateDiffShort(DateTime first, DateTime sec, string beforeTemplate = "{0}", string afterTemplate = "{0}")
        {
            if (first < DateTime.MinValue)
                first = DateTime.MinValue;
            if (sec < DateTime.MinValue)
                sec = DateTime.MinValue;
            if (first > DateTime.MaxValue)
                first = DateTime.MaxValue;
            if (sec > DateTime.MaxValue)
                sec = DateTime.MaxValue;

            bool after = first > sec;
            Devmasters.Core.DateTimeSpan dateDiff = Devmasters.Core.DateTimeSpan.CompareDates(first, sec);
            string txtDiff = string.Empty;
            if (dateDiff.Years > 0)
            {
                txtDiff = HlidacStatu.Util.PluralForm.Get(dateDiff.Years, "rok;{0} roky;{0} let");
            }
            else if (dateDiff.Months > 3)
            {
                txtDiff = HlidacStatu.Util.PluralForm.Get(dateDiff.Months, "měsíc;{0} měsíce;{0} měsíců");
            }
            else
            {
                txtDiff = Devmasters.Core.Lang.Plural.GetWithZero(dateDiff.Days, "dnes", "den", "{0} dny", "{0} dnů");
            }

            if (after)
                return string.Format(afterTemplate, txtDiff);
            else
                return string.Format(beforeTemplate, txtDiff);

        }

        public static string FormatIntervalSinglePart(TimeSpan ts, DateTimePart minDatePart, string numFormat = "N1")
        {

            var end = DateTime.Now;
            Devmasters.Core.DateTimeSpan dts = Devmasters.Core.DateTimeSpan.CompareDates(end - ts, end);
            string s = "";
            if (dts.Years > 0 && minDatePart > DateTimePart.Year)
            {
                s += " " + HlidacStatu.Util.PluralForm.Get(dts.Years, "{0} rok;{0} roky;{0} let");
            }
            else if (dts.Years > 0)
            {
                decimal part = dts.Years + dts.Months / 12m;
                if (part % 1 > 0)
                    s += string.Format(" {0:" + numFormat + "} let", part);
                else
                    s += HlidacStatu.Util.PluralForm.Get((int)part, " {0} rok; {0} roky; {0} let"); ;
                return s;
            }

            if (dts.Months > 0 && minDatePart > DateTimePart.Month)
            {
                s += " " + HlidacStatu.Util.PluralForm.Get(dts.Months, "{0} měsíc;{0} měsíce;{0} měsíců");
            }
            else if (dts.Months > 0)
            {
                decimal part = dts.Months + dts.Days / 30m;
                if (part % 1 > 0)
                    s += string.Format(" {0:" + numFormat + "} měsíců", part);
                else
                    s += HlidacStatu.Util.PluralForm.Get((int)part, " {0} měsíc; {0} měsíce; {0} měsíců"); ;
                return s;
            }

            if (dts.Days > 0 && minDatePart > DateTimePart.Day)
            {
                s = " " + HlidacStatu.Util.PluralForm.Get(dts.Days, " {0} den;{0} dny;{0} dnů");
            }
            else if (dts.Days > 0)
            {
                decimal part = dts.Days + dts.Hours / 24m;
                if (part % 1 > 0)
                    s = " " + string.Format(" {0:" + numFormat + "} dní", part);
                else
                    s = " " + HlidacStatu.Util.PluralForm.Get((int)part, " {0} den;{0} dny;{0} dnů"); ;
                return s;
            }

            if (dts.Hours > 0 && minDatePart > DateTimePart.Hour)
            {
                s += " " + HlidacStatu.Util.PluralForm.Get(dts.Hours, " {0} hodinu;{0} hodiny;{0} hodin");
            }
            else if (dts.Hours > 0)
            {
                decimal part = dts.Hours + dts.Minutes / 60m;
                if (part % 1 > 0)
                    s += string.Format(" {0:" + numFormat + "} hodin", part);
                else
                    s += " " + HlidacStatu.Util.PluralForm.Get((int)part, " {0} hodinu;{0} hodiny;{0} hodin");
                return s;
            }

            if (dts.Minutes > 0 && minDatePart > DateTimePart.Minute)
            {
                s += " " + HlidacStatu.Util.PluralForm.Get(dts.Minutes, "minutu;{0} minuty;{0} minut");
            }
            else if (dts.Minutes > 0)
            {
                decimal part = dts.Minutes + dts.Seconds / 60m;
                if (part % 1 > 0)
                    s += string.Format(" {0:" + numFormat + "} minut", part);
                else
                    s += " " + HlidacStatu.Util.PluralForm.Get((int)part, "minutu;{0} minuty;{0} minut"); ;
                return s;
            }

            if (dts.Seconds > 0 && minDatePart > DateTimePart.Second)
            {
                s += " " + HlidacStatu.Util.PluralForm.Get(dts.Seconds, "sekundu;{0} sekundy;{0} sekund");
            }
            else
            {
                decimal part = dts.Seconds + dts.Milliseconds / 1000m;
                if (part % 1 > 0)
                    s += string.Format(" {0:" + numFormat + "} sekund", part);
                else
                    s += " " + HlidacStatu.Util.PluralForm.Get((int)part, "sekundu;{0} sekundy;{0} sekund"); ;
                return s;
            }

            //if (dts.Milliseconds > 0)
            //    s += " " + HlidacStatu.Util.PluralForm.Get(dts.Milliseconds, "{0} ms;{0} ms;{0} ms");

            return s.Trim();

        }

        public static string FormatInterval(TimeSpan ts)
        {
            return FormatInterval(ts, System.Globalization.CultureInfo.CurrentUICulture);
        }
        public static string FormatInterval(TimeSpan ts, System.Globalization.CultureInfo culture)
        {
            var end = DateTime.Now;
            Devmasters.Core.DateTimeSpan dts = Devmasters.Core.DateTimeSpan.CompareDates(end - ts, end);
            string s = "";
            if (dts.Years > 0)
            {
                s += " " + HlidacStatu.Util.PluralForm.Get(dts.Years, "rok;{0} roky;{0} let", culture);
            }
            if (dts.Months > 0)
            {
                s += " " + HlidacStatu.Util.PluralForm.Get(dts.Months, "měsíc;{0} měsíce;{0} měsíců", culture);
            }
            if (dts.Days > 0)
            {
                s += " " + HlidacStatu.Util.PluralForm.Get(dts.Days, "den;{0} dny;{0} dnů", culture);
            }
            if (dts.Hours > 0)
            {
                s += " " + HlidacStatu.Util.PluralForm.Get(dts.Hours, "hodinu;{0} hodiny;{0} hodin", culture);
            }
            if (dts.Minutes > 0)
            {
                s += " " + HlidacStatu.Util.PluralForm.Get(dts.Minutes, "minutu;{0} minuty;{0} minut", culture);
            }
            if (dts.Seconds > 0)
            {
                s += " " + HlidacStatu.Util.PluralForm.Get(dts.Seconds, "sekundu;{0} sekundy;{0} sekund", culture);
            }
            if (dts.Milliseconds > 0)
                s += " " + HlidacStatu.Util.PluralForm.Get(dts.Milliseconds, "{0} ms;{0} ms;{0} ms", culture);

            return s.Trim();

        }


        public static bool IsDateInInterval(DateTime? from, DateTime? to, DateTime? date)
        {
            if (date == null)
                return false;
            else if (
                        (from <= date && date <= to)
                        || (from == null && date <= to)
                        || (date <= to && to == null)
                    )
                return true;
            else
                return false;
        }

        //based on sql dbo.IsSomehowInInterval
        public static bool IsOverlappingIntervals(DateTime? dateIntervalFrom,
                                            DateTime? dateIntervalTo,
                                            DateTime? dateRelFrom,
                                            DateTime? dateRelTo)
        {
            int oks = 0;

            if (dateIntervalFrom is null && dateIntervalTo is null)
                return true;
            if (dateRelFrom is null && dateRelTo is null)
                return true;

            if (dateRelFrom is null)
                dateRelFrom = new DateTime(1900, 01, 01);
            if (dateRelTo is null)
                dateRelTo = DateTime.Now.AddDays(10);


            if (IsDateInInterval(dateRelFrom, dateRelTo, dateIntervalFrom))
                oks = oks + 1;

            if (IsDateInInterval(dateRelFrom, dateRelTo, dateIntervalTo))
                oks = oks + 1;

            if (dateIntervalFrom <= dateRelFrom && dateRelTo <= dateIntervalTo
                && dateIntervalFrom != null && dateRelFrom != null && dateRelTo != null && dateIntervalTo != null
                )
                oks = oks + 1;

            if (oks == 0
                && dateIntervalFrom is null
                && dateIntervalTo > dateRelTo
                )
                oks = oks + 1;

            if (oks == 0
                && dateIntervalTo is null
                && dateIntervalFrom < dateRelFrom
                )
                oks = oks + 1;

            if (oks > 0)
                return true;
            else
                return false; ;

        }
    }
}
