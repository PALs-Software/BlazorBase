using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorBase.CRUD.Services
{
    public class BaseParser
    {
        protected IStringLocalizer<BaseParser> Localizer { get; set; }

        public BaseParser(IStringLocalizer<BaseParser> localizer) {
            Localizer = localizer;
        }        

        public bool TryParseValueFromString<T>(string inputValue, out object outputValue, out string errorMessage)
        {
            return TryParseValueFromString(typeof(T), inputValue, out outputValue, out errorMessage);
        }

        public bool TryParseValueFromString(Type outputType, string inputValue, out object outputValue, out string errorMessage)
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
                success = BindConverter.TryConvertToBool(inputValue, CultureInfo.CurrentCulture, out var parsedValue);
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
            else if (outputType.IsEnum)
            {
                success = Enum.TryParse(outputType, inputValue, true, out var parsedValue);
                outputValue = parsedValue;
            }
            else
                throw new Exception("Type is not supported!");

            if (success)
            {
                errorMessage = null;
                return true;
            }
            else
            {
                outputValue = default;
                errorMessage = Localizer["The Value {0} can not be converted to the format {1}", inputValue, outputType.Name];
                return false;
            }
        }
    }
}
