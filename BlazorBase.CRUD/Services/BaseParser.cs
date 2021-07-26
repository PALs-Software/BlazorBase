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
            bool success = false;
            errorMessage = null;

            try
            {
                Type conversionType = Nullable.GetUnderlyingType(outputType) ?? outputType;

                if (conversionType.IsEnum)
                    outputValue = Enum.Parse(outputType, inputValue, true);
                else if (conversionType == typeof(Guid))
                    outputValue = Convert.ChangeType(Guid.Parse(inputValue), conversionType);
                else if (conversionType == typeof(DateTimeOffset))
                    outputValue = Convert.ChangeType(DateTimeOffset.Parse(inputValue), conversionType);
                else if (conversionType == typeof(DateTime))
                    outputValue = Convert.ChangeType(inputValue, conversionType, CultureInfo.CurrentUICulture);
                else
                    outputValue = Convert.ChangeType(inputValue, conversionType, CultureInfo.InvariantCulture);
                
                success = true;
            }
            catch
            {
                outputValue = default;
                errorMessage = Localizer["The Value {0} can not be converted to the format {1}", inputValue, outputType.Name];
            }
          
            return success;
        }

    }
}
