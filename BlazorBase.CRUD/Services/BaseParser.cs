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
        public static List<Type> DecimalTypes { get; } = new List<Type>(){
            typeof(decimal),
            typeof(decimal?),
            typeof(double),
            typeof(double?),
            typeof(float),
            typeof(float?),
            typeof(int),
            typeof(int?),
            typeof(long),
            typeof(long?)
        };

        protected IStringLocalizer<BaseParser> Localizer { get; set; }

        public BaseParser(IStringLocalizer<BaseParser> localizer)
        {
            Localizer = localizer;
        }

        public bool TryParseValueFromString<T>(string? inputValue, out object? outputValue, out string? errorMessage)
        {
            return TryParseValueFromString(typeof(T), inputValue, out outputValue, out errorMessage);
        }

        public bool TryParseValueFromString(Type outputType, string? inputValue, out object? outputValue, out string? errorMessage)
        {
            bool success = false;
            errorMessage = null;

            try
            {
                var underlyingType = Nullable.GetUnderlyingType(outputType);
                var isNullable = underlyingType != null;
                Type conversionType = underlyingType ?? outputType;

                if (isNullable && conversionType != typeof(string) && String.IsNullOrEmpty(inputValue))
                    outputValue = null;
                else if (conversionType.IsEnum)
                    outputValue = Enum.Parse(outputType, inputValue!, true);
                else if (conversionType == typeof(Guid))
                    outputValue = Convert.ChangeType(Guid.Parse(inputValue!), conversionType);
                else if (conversionType == typeof(DateTimeOffset))
                    outputValue = Convert.ChangeType(DateTimeOffset.Parse(inputValue!), conversionType);
                else if (conversionType == typeof(TimeSpan))
                    outputValue = Convert.ChangeType(TimeSpan.Parse(inputValue!), conversionType);
                else if (conversionType == typeof(bool) && String.IsNullOrEmpty(inputValue))
                    outputValue = isNullable ? null : false;
                else
                    outputValue = Convert.ChangeType(inputValue, conversionType, CultureInfo.InvariantCulture);

                success = true;
            }
            catch
            {
                outputValue = default;
                errorMessage = Localizer["The Value {0} can not be converted to the format {1}", inputValue ?? "null", outputType.Name];
            }

            return success;
        }

    }
}
