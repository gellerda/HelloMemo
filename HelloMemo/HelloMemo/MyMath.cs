using System;
using System.Collections.Generic;
using System.Text;

namespace HelloMemo
{
    class MyMath
    {
        public static int CurrentDTinHours()
        {
            DateTime centuryBegin = new DateTime(2018, 1, 1);
            DateTime currentDT = DateTime.Now;

            long elapsedTicks = currentDT.Ticks - centuryBegin.Ticks;
            TimeSpan elapsedSpan = new TimeSpan(elapsedTicks);
            return (Int32)elapsedSpan.TotalHours;
        }

        public static double EMA(int oldValue, int newValue, double alpha)
        {
            return oldValue + (newValue - oldValue) * alpha;
        }
    }
}
