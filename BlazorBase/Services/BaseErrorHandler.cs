using Microsoft.Extensions.Localization;
using System;

namespace BlazorBase.Services;
public class BaseErrorHandler
{
    protected readonly IStringLocalizer<BaseErrorHandler> Localizer;

    public BaseErrorHandler(IStringLocalizer<BaseErrorHandler> localizer)
    {
        Localizer = localizer;
    }

    public string PrepareExceptionErrorMessage(Exception e)
    {
        if (e.InnerException == null)
            return e.Message;

        return e.Message + Environment.NewLine + Environment.NewLine + Localizer["Inner Exception:"] + PrepareExceptionErrorMessage(e.InnerException);
    }
}
