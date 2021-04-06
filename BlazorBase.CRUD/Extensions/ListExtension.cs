using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorBase.CRUD.Extensions
{
    public static class ListExtension
    {
        public static string FormatResultsToString(this List<ValidationResult> results) {
            var formattedResult = String.Empty;
            foreach (var result in results)
                formattedResult += $"- {result.ErrorMessage}{Environment.NewLine}";

            return formattedResult;
        }
    }
}
