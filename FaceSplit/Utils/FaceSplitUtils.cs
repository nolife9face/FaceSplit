﻿using System;

namespace FaceSplit
{
    public class FaceSplitUtils
    {
        /// <summary>
        /// Take a time from string and parse it into a double containing the number of seconds.
        /// </summary>
        /// <param name="timeString">The time from string.</param>
        /// <returns>The time into a double.</returns>
        public static double TimeParse(string timeString)
        {
            double num = 0.0;
            foreach (string str in timeString.Split(new char[] { ':' }))
            {
                double num2;
                if (double.TryParse(str, out num2))
                    num = (num * 60.0) + num2;
            }
            return num;
        }

        /// <summary>
        /// Take a number of seconds from double and transform it into a string.
        /// </summary>
        /// <param name="secs">The number of seconds</param>
        /// <returns>The time into a string.</returns>
        public static string TimeFormat(double secs)
        {
            TimeSpan span = TimeSpan.FromSeconds(Math.Truncate(secs * 100) / 100);
            if (span.TotalHours >= 1.0)
                return string.Format("{0}:{1:00}:{2:00.00}", Math.Floor(span.TotalHours), span.Minutes, span.Seconds + span.Milliseconds / 1000.0);

            if (span.TotalMinutes >= 1.0)
                return string.Format("{0}:{1:00.00}", Math.Floor(span.TotalMinutes), span.Seconds + span.Milliseconds / 1000.0);

            return string.Format("{0:0.00}", span.TotalSeconds);
        }

        /// <summary>
        /// Take a time in a string format and remove amount of decimals asked 
        /// if it's possible.
        /// </summary>
        /// <param name="time">The time in a string format.</param>
        /// <param name="cutLenght">The amount of decimals we remove.</param>
        /// <returns></returns>
        public static string CutDecimals(string time, int cutLenght)
        {
            if (!string.IsNullOrEmpty(time.Trim()) && time.IndexOf(':') != -1 && time.IndexOf(',') != -1)
            {
                time = time.Remove(time.IndexOf(","), cutLenght + 1);    
            }
            return time;
        }
    }
}
