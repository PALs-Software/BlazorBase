using Blazorise;
using Blazorise.States;
using Blazorise.Utilities;
using Microsoft.AspNetCore.Components;

namespace BlazorBase.CRUD.Components.Modals;

/// <summary>
/// Internal component to render modal backdrop or background.
/// </summary>
public partial class BaseModalBackdrop : BaseComponent
{
    #region Members

    private ModalState? parentModalState;

    #endregion

    #region Methods

    /// <inheritdoc/>
    protected override void OnInitialized()
    {
        base.OnInitialized();

        ParentModal?.NotifyCloseActivatorIdInitialized( ElementId );
    }

    /// <inheritdoc/>
    protected override void BuildClasses( ClassBuilder builder )
    {
        builder.Append( ClassProvider.ModalBackdrop() );
        builder.Append( ClassProvider.ModalBackdropFade() );
        builder.Append( ClassProvider.ModalBackdropVisible( parentModalState?.Visible ?? true ) );

        base.BuildClasses( builder );
    }

    #endregion

    #region Properties

    /// <inheritdoc/>
    protected override bool ShouldAutoGenerateId => true;

    /// <summary>
    /// Gets or sets the cascaded parent modal component.
    /// </summary>
    [CascadingParameter] protected BaseModal? ParentModal { get; set; }

    /// <summary>
    /// Cascaded <see cref="Modal"/> component state object.
    /// </summary>
    [CascadingParameter]
    protected ModalState? ParentModalState
    {
        get => parentModalState;
        set
        {
            if ( parentModalState == value )
                return;

            parentModalState = value;

            DirtyClasses();
        }
    }

    #endregion
}
