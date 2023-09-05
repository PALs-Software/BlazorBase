using System;

namespace BlazorBase.CRUD.Attributes;

public class SkipExplicitNavigationLoadingOnCardOpenAttribute : Attribute
{
   public bool SkipCardSaveChangesEventsForThisPropertyToPreventLoading { get; set; }
}
