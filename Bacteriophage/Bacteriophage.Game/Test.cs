using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bacteriophage
{
    class Test
    {
        public static float RoundToInterval(float value, int interval, IntervalRounding rounding)
        {
            // Guard against the divide by zero exception 
            if (interval == 0) return float.MinValue;

            // For example, round 18 to the nearest interval of 5 
            // Quotient = 3.6, Remainder = 0.6, Whole = 3
            float quotient = value / (float)interval;
            float remainder = quotient - (float)Math.Floor(quotient);
            float whole = quotient - remainder;

            // Rounding to the nearest interval and remainder 
            // is over half way to the next interval value

            if (rounding == IntervalRounding.Nearest && remainder >= 0.5f ||
                // Or we force rounding to the next highest interval
                // and we have a remainder. In otherwords, our value
                // isn’t on an interval already 
                rounding == IntervalRounding.Up && remainder > 0f)
            {
                return (float)interval * (whole + 1);
            }
            else
            {
                return (float)interval * whole;
            }
        }

        public enum IntervalRounding
        {
            Nearest = 0, 
            Up = 1,
            Down = 2
        }
    }
}
