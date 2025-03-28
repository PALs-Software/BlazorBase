﻿using System.Threading.Tasks;
using Blazorise;
using Blazorise.Utilities;
using Microsoft.AspNetCore.Components;

namespace BlazorBase.CRUD.Components.Modals;

/// <summary>
/// A generic close button for dismissing content like modals and alerts.
/// </summary>
public partial class BaseModalCloseButton : BaseComponent
{
    #region Members

    #endregion

    #region Methods

    /// <inheritdoc/>
    protected override void BuildClasses(ClassBuilder builder)
    {
        builder.Append(ClassProvider.CloseButton());

        base.BuildClasses(builder);
    }

    /// <summary>
    /// Handles the item onclick event.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    protected async Task ClickHandler()
    {
        // We must have priority over what get's closed once we click on close button.
        // For example, there can be Alert placed inside of Modal, and Close Button inside of Alert.
        // And we don't want to close both Alert and Modal in that case.
        if (IsAutoClose)
        {
            if (ParentAlert != null)
            {
                ParentAlert.Hide();
            }
            else if (ParentModal != null)
            {
                await ParentModal.HideAsync();
            }
        }

        await Clicked.InvokeAsync(null);
    }

    #endregion

    #region Properties

    /// <summary>
    /// Returns true if parent component should be automatically closed.
    /// </summary>
    protected bool IsAutoClose
        => AutoClose.GetValueOrDefault(Options?.AutoCloseParent ?? true);

    /// <summary>
    /// Holds the information about the Blazorise global options.
    /// </summary>
    [Inject] protected BlazoriseOptions? Options { get; set; }

    /// <summary>
    /// Occurs when the button is clicked.
    /// </summary>
    [Parameter] public EventCallback Clicked { get; set; }

    /// <summary>
    /// If true, the parent <see cref="Alert"/> or <see cref="Modal"/> with be automatically closed
    /// when <see cref="CloseButton"/> button is placed inside of them.
    /// </summary>
    [Parameter] public bool? AutoClose { get; set; }

    /// <summary>
    /// Cascaded <see cref="Alert"/> component in which this <see cref="CloseButton"/> is placed.
    /// </summary>
    [CascadingParameter] protected Alert? ParentAlert { get; set; }

    /// <summary>
    /// Cascaded <see cref="Modal"/> component in which this <see cref="CloseButton"/> is placed.
    /// </summary>
    [CascadingParameter] protected BaseModal? ParentModal { get; set; }

    /// <summary>
    /// Specifies the content to be rendered inside this <see cref="CloseButton"/>.
    /// </summary>
    [Parameter] public RenderFragment? ChildContent { get; set; }

    #endregion
}
