using System;
using Refactor.Common;

namespace Refactor.Model
{
    public class DisabilitySite
    {
        private Reading[] readings = new Reading[1000];
        private static readonly Dollars FUEL_TAX_CAP = new Dollars(0.10);
        private static readonly double TAX_RATE = 0.05;
        private Zone zone;
        private static readonly int CAP = 200;

        public DisabilitySite(Zone zone)
        {
            this.zone=zone;
        }


        public void AddReading(Reading newReading)
        {
            int i;
            for (i = 0; readings[i] != null; i++) ;
            readings[i] = newReading;
        }

        public Dollars Charge()
        {
            int i;
            for (i = 0; readings[i] != null; i++) ;
            int usage = readings[i - 1].Amount() - readings[i - 2].Amount();
            DateTime end = readings[i - 1].Date();
            DateTime start = readings[i - 2].Date();
            start = start.SetDate(start.GetDate() + 1); //set to begining of period
            return Charge(usage, start, end);
        }

        private Dollars Charge(int fullUsage, DateTime start, DateTime end)
        {
            Dollars result;
            double summerFraction;
            int usage = Math.Min(fullUsage, CAP);
            if (start.After(zone.SummerEnd()) || end.Before(zone.SummerStart()))
                summerFraction = 0;
            else if (!start.Before(zone.SummerStart()) && !start.After(zone.SummerEnd()) &&
                !end.Before(zone.SummerStart()) && !end.After(zone.SummerEnd()))
                summerFraction = 1;
            else
            {
                double summerDays;
                if (start.Before(zone.SummerStart()) || start.After(zone.SummerEnd()))
                {
                    // end is in the summer
                    summerDays = DayOfYear(end) - DayOfYear(zone.SummerStart()) + 1;
                }
                else
                {
                    // start is in summer
                    summerDays = DayOfYear(zone.SummerEnd()) - DayOfYear(start) + 1;
                }
                ;
                summerFraction = summerDays/(DayOfYear(end) - DayOfYear(start) + 1);
            }
            ;
            result = new Dollars((usage*zone.SummerRate()*summerFraction) +
                (usage*zone.WinterRate()*(1 - summerFraction)));
            result = result.Plus(new Dollars(Math.Max(fullUsage - usage, 0)*0.062));
            result = result.Plus(new Dollars(result.Times(TAX_RATE)));
            Dollars fuel = new Dollars(fullUsage*0.0175);
            result = result.Plus(fuel);
            result = new Dollars(result.Plus(fuel.Times(TAX_RATE).Min(FUEL_TAX_CAP)));
            return result;
        }

        private int DayOfYear(DateTime arg)
        {
//            return arg.DayOfYear;
            int result;
            switch (arg.Month)
            {
                case 0:
                    result = 0;
                    break;
                case 1:
                    result = 31;
                    break;
                case 2:
                    result = 59;
                    break;
                case 3:
                    result = 90;
                    break;
                case 4:
                    result = 120;
                    break;
                case 5:
                    result = 151;
                    break;
                case 6:
                    result = 181;
                    break;
                case 7:
                    result = 212;
                    break;
                case 8:
                    result = 243;
                    break;
                case 9:
                    result = 273;
                    break;
                case 10:
                    result = 304;
                    break;
                case 11:
                    result = 334;
                    break;
                default:
                    throw new ArgumentException();
            }
            result += arg.GetDate();
            //check leap year
            if ((arg.Year%4 == 0) && ((arg.Year%100 != 0) ||
                ((arg.Year + 1900)%400 == 0)))
            {
                result++;
            }
            return result;
        }
    }
}