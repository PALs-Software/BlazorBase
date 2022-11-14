using Blazorise;
using Blazorise.Utilities;

namespace BlazorBase.Components;

public class FullHeightTabs : Tabs
{
    protected override void BuildContentClasses(ClassBuilder builder)
    {
        base.BuildContentClasses(builder);
        builder.Append("h-100");
    }
}
