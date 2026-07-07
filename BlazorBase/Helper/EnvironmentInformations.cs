using System;

namespace BlazorBase.Helper;

public static class EnvironmentInformations
{
    private static bool? _IsDockerContainer;
    public static bool IsDockerContainer
    {
        get
        {
            _IsDockerContainer ??= Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true";
            return _IsDockerContainer.Value;
        }
    }
}
