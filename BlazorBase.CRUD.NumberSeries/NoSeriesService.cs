using BlazorBase.CRUD.Models;
using BlazorBase.CRUD.Services;
using Microsoft.Extensions.Localization;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace BlazorBase.CRUD.NumberSeries
{
    public class NoSeriesService
    {
        protected IStringLocalizer<NoSeriesService> Localizer { get; set; }
        public NoSeriesService(IStringLocalizer<NoSeriesService> localizer)
        {
            Localizer = localizer;
        }

        public string GetMaxSeriesNo(string noSeries)
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

        public bool IsValidNoSeries(string noSeries)
        {
            var digits = noSeries.Where(entry => char.IsDigit(entry)).ToList();
            return digits.Count != 0;
        }

        public bool NoSeriesAreEqualExceptOfDigits(string noSeries1, string noSeries2)
        {
            if (noSeries1.Length != (noSeries2?.Length ?? -1))
                return false;

            for (int i = 0; i < noSeries1.Length; i++)
                if (noSeries1[i] != noSeries2[i])
                    return char.IsDigit(noSeries1[i]) && char.IsDigit(noSeries2[i]);

            return true;
        }

        public async Task<string> GetNextNoAsync(BaseService service, string noSeriesId)
        {
            var noSeries = await service.GetAsync<NoSeries>(noSeriesId);
            if (noSeries == null)
                throw new CRUDException(Localizer["Cant get next number in series, because number series can not be found with the key {0}", noSeriesId]);

            if (String.IsNullOrEmpty(noSeries.LastNoUsed))
            {
                noSeries.LastNoUsed = noSeries.StartingNo;
                noSeries.LastNoUsedNumeric = long.Parse(new String(noSeries.LastNoUsed.Where(entry => char.IsDigit(entry)).ToArray()));
                noSeries.EndingNoNumeric = long.Parse(new String(noSeries.EndingNo.Where(entry => char.IsDigit(entry)).ToArray()));
                noSeries.NoOfDigits = noSeries.StartingNo.Where(entry => char.IsDigit(entry)).Count();
            }
            else
                IncreaseNo(noSeries);

            service.UpdateEntry(noSeries);
            return noSeries.LastNoUsed;
        }

        protected void IncreaseNo(NoSeries noSeries)
        {
            if (noSeries.LastNoUsedNumeric + 1 > noSeries.EndingNoNumeric)
                throw new CRUDException(Localizer["The defined maximum of the no series is reached, please create a new number series"]);

            noSeries.LastNoUsedNumeric++;
            var lastNoUsed = noSeries.LastNoUsedNumeric.ToString().PadLeft(noSeries.NoOfDigits, '0');

            var result = String.Empty;
            foreach (var item in noSeries.LastNoUsed)
            {
                if (char.IsDigit(item))
                {
                    result += lastNoUsed[0];
                    lastNoUsed = lastNoUsed.Substring(1);
                }
                else
                    result += item;
            }

            noSeries.LastNoUsed = result;
        }
    }
}
