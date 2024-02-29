using System;

namespace BlazorBase.CRUD.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public class SkipExplicitNavigationLoadingOnCardOpenAttribute : Attribute
{
   public bool SkipCardSaveChangesEventsForThisPropertyToPreventLoading { get; set; }
}
