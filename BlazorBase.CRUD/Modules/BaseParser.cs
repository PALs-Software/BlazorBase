using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorBase.CRUD.Modules
{
    public static class BaseParser
    {
        public static bool TryParseValueFromString<T>(string inputValue, out object outputValue, out string errorMessage)
        {
            return TryParseValueFromString(typeof(T), inputValue, out outputValue, out errorMessage);
        }

        public static bool TryParseValueFromString(Type outputType, string inputValue, out object outputValue, out string errorMessage)
        {
            bool success;
            if (outputType == typeof(String))
            {
                success = BindConverter.TryConvertToString(inputValue, CultureInfo.CurrentCulture, out var parsedValue);
                outputValue = parsedValue;
            }
            else if (outputType == typeof(int))
            {
                success = BindConverter.TryConvertToInt(inputValue, CultureInfo.CurrentCulture, out var parsedValue);
                outputValue = parsedValue;
            }
            else if (outputType == typeof(decimal))
            {
                success = BindConverter.TryConvertToDecimal(inputValue, CultureInfo.CurrentCulture, out var parsedValue);
                outputValue = parsedValue;
            }
            else if (outputType == typeof(bool))
            {
                success = Boolean.TryParse(inputValue, out var parsedValue);
                //success = BindConverter.TryConvertToBool(inputValue, CultureInfo.CurrentCulture, out var parsedValue);
                outputValue = parsedValue;
            }
            else if (outputType == typeof(DateTime))
            {
                success = BindConverter.TryConvertToDateTime(inputValue, CultureInfo.CurrentCulture, out var parsedValue);
                outputValue = parsedValue;
            }
            else if (outputType == typeof(Guid))
            {
                success = Guid.TryParse(inputValue, out var parsedValue);
                outputValue = parsedValue;
            }
            else
                throw new Exception("Type not supported");

            if (success)
            {
                errorMessage = null;
                return true;
            }
            else
            {
                outputValue = default;
                errorMessage = $"Der Wert \"{inputValue}\" konnte nicht in das Format {outputType.Name} formatiert werden";
                return false;
            }
        }
    }
}
