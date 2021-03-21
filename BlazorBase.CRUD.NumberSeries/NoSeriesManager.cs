using BlazorBase.CRUD.ViewModels;
using System;
using System.Linq;

namespace BlazorBase.CRUD.NumberSeries
{
    public static class NoSeriesManager
    {
        public static string GetMaxSeriesNo(string noSeries)
        {
            string result = String.Empty;

            foreach (var item in noSeries)
            {
                if (char.IsDigit(item))
                    result += '9';
                else
                    result += item;
            }

            return result;
        }

        public static bool IsValidNoSeries(string noSeries)
        {
            var digits = noSeries.Where(entry => char.IsDigit(entry)).ToList();
            return digits.Count != 0;
        }

        public static bool NoSeriesAreEqualExceptOfDigits(string noSeries1, string noSeries2)
        {
            if (noSeries1.Length != noSeries2.Length)
                return false;

            for (int i = 0; i < noSeries1.Length; i++)
                if (noSeries1[i] != noSeries2[i])
                    return char.IsDigit(noSeries1[i]) && char.IsDigit(noSeries2[i]);

            return true;
        }

    }
}
