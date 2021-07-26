using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorBase.Modules
{
    public class ErrorHandler
    {
        protected IStringLocalizer<ErrorHandler> Localizer;

        public ErrorHandler(IStringLocalizer<ErrorHandler> localizer) {
            Localizer = localizer;
        }

        public string PrepareExceptionErrorMessage(Exception e)
        {
            if (e.InnerException == null)
                return e.Message;

            return e.Message + Environment.NewLine + Environment.NewLine + Localizer["Inner Exception:"] + PrepareExceptionErrorMessage(e.InnerException);
        }
    }
}
