using BlazorBase.CRUD.Attributes;
using BlazorBase.CRUD.Components;
using BlazorBase.CRUD.Enums;
using BlazorBase.CRUD.Extensions;
using BlazorBase.CRUD.Resources.ValidationAttributes;
using BlazorBase.CRUD.ViewModels;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static BlazorBase.CRUD.Models.IBaseModel;

namespace BlazorBase.CRUD.Models
{
    public class BaseModel<TModel> : ComponentBase where TModel : class, IBaseModel, new()
    {
        public event EventHandler<string> OnForcePropertyRepaint;

        public BaseModel()
        {
            CreatedOn = DateTime.Now;
            ModifiedOn = CreatedOn;

            PageActionGroups = InitializePageActions();
        }

        #region Additional Properties
        [Visible(displayOrder:9999, hideInGUITypes: GUIType.ListPart)]
        [Editable(false)]
        public DateTime CreatedOn { get; set; }

        [Visible(displayOrder: 9999, hideInGUITypes: GUIType.ListPart)]
        [Editable(false)]
        public DateTime ModifiedOn { get; set; }
        #endregion

        #region [] Extension Methods
        public object this[PropertyInfo property]
        {
            get
            {
                return property.GetValue(this);
            }
            set
            {
                property.SetValue(this, value);
            }
        }

        public object this[string property]
        {
            get
            {
                return GetType().GetProperty(property).GetValue(this);
            }
            set
            {
                GetType().GetProperty(property).SetValue(this, value);
            }
        }
        #endregion

        #region Attribute Methods
        public List<PropertyInfo> GetVisibleProperties()
        {
            return GetType().GetVisibleProperties();
        }

        public object[] GetPrimaryKeys()
        {
            var keyProperties = GetType().GetProperties().Where(property =>
                (!property.PropertyType.IsSubclassOf(typeof(IBaseModel))) &&
                property.IsKey()
            ).ToList();

            var keys = new object[keyProperties.Count];
            for (int i = 0; i < keyProperties.Count; i++)
                keys[i] = keyProperties.ElementAt(i).GetValue(this);

            return keys;
        }

        public string GetPrimaryKeysAsString()
        {
            return String.Join(", ", GetPrimaryKeys());
        }
        #endregion

        #region CRUD Methods

        public void ForcePropertyRepaint(string propertyName)
        {
            OnForcePropertyRepaint?.Invoke(this, propertyName);
        }

        #region Events
        public virtual Task OnBeforeConvertPropertyType(OnBeforeConvertPropertyTypeArgs args)
        {
            return Task.CompletedTask;
        }

        public virtual Task OnBeforePropertyChanged(OnBeforePropertyChangedArgs args)
        {
            return Task.CompletedTask;
        }

        public virtual Task OnAfterPropertyChanged(OnAfterPropertyChangedArgs args)
        {
            return Task.CompletedTask;
        }

        public virtual Task OnCreateNewEntryInstance(OnCreateNewEntryInstanceArgs args)
        {
            return Task.CompletedTask;
        }

        public virtual Task OnBeforeAddEntry(OnBeforeAddEntryArgs args)
        {
            return Task.CompletedTask;
        }

        public virtual Task OnAfterAddEntry(OnAfterAddEntryArgs args)
        {
            return Task.CompletedTask;
        }

        public virtual Task OnBeforeUpdateEntry(OnBeforeUpdateEntryArgs args)
        {
            return Task.CompletedTask;
        }

        public virtual Task OnAfterUpdateEntry(OnAfterUpdateEntryArgs args)
        {
            return Task.CompletedTask;
        }

        public virtual Task OnBeforeRemoveEntry(OnBeforeRemoveEntryArgs args)
        {
            return Task.CompletedTask;
        }

        public virtual Task OnAfterRemoveEntry(OnAfterRemoveEntryArgs args)
        {
            return Task.CompletedTask;
        }
        #endregion

        #region List Events 
        public virtual Task OnBeforeConvertListPropertyType(OnBeforeConvertListPropertyTypeArgs args)
        {
            return Task.CompletedTask;
        }
        public virtual Task OnBeforeListPropertyChanged(OnBeforeListPropertyChangedArgs args)
        {
            return Task.CompletedTask;
        }

        public virtual Task OnAfterListPropertyChanged(OnAfterListPropertyChangedArgs args)
        {
            return Task.CompletedTask;
        }

        public virtual Task OnCreateNewListEntryInstance(OnCreateNewListEntryInstanceArgs args)
        {
            return Task.CompletedTask;
        }

        public virtual Task OnBeforeAddListEntry(OnBeforeAddListEntryArgs args)
        {
            return Task.CompletedTask;
        }

        public virtual Task OnAfterAddListEntry(OnAfterAddListEntryArgs args)
        {
            return Task.CompletedTask;
        }

        public virtual Task OnBeforeUpdateListEntry(OnBeforeUpdateListEntryArgs args)
        {
            return Task.CompletedTask;
        }

        public virtual Task OnAfterUpdateListEntry(OnAfterUpdateListEntryArgs args)
        {
            return Task.CompletedTask;
        }

        public virtual Task OnBeforeRemoveListEntry(OnBeforeRemoveListEntryArgs args)
        {
            return Task.CompletedTask;
        }

        public virtual Task OnAfterRemoveListEntry(OnAfterRemoveListEntryArgs args)
        {
            return Task.CompletedTask;
        }
        #endregion
        #endregion

        #region Validation Methods
        public bool TryValidate(out List<ValidationResult> validationResults, ValidationContext validationContext)
        {
            validationResults = new List<ValidationResult>();
            return Validator.TryValidateObject(this, validationContext, validationResults, true);
        }

        public bool TryValidateProperty(out List<ValidationResult> validationResults, ValidationContext propertyValidationContext, PropertyInfo propertyInfo)
        {
            validationResults = new List<ValidationResult>();
            var attributes = propertyInfo.GetCustomAttributes<Attribute>().OfType<ValidationAttribute>();
            foreach (var attribute in attributes.Where(attribute => attribute.ErrorMessage == null))
            {
                var attributeName = attribute.GetType().Name;
                if (ValidationAttributesTranslations.ResourceManager.GetString(attributeName) != null)
                {
                    attribute.ErrorMessageResourceType = typeof(ValidationAttributesTranslations);
                    attribute.ErrorMessageResourceName = attributeName;
                }
            }

            return Validator.TryValidateValue(propertyInfo.GetValue(this), propertyValidationContext, validationResults, attributes);
        }
        #endregion

        #region ComponentBase
        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            base.BuildRenderTree(builder);

            BuildComponent(builder);
        }

        protected virtual void BuildComponent(RenderTreeBuilder builder) {
            builder.OpenComponent<BaseList<TModel>>(0);
            builder.CloseComponent();
        }

        #endregion

        #region PageActions
        [NotMapped]
        public List<PageActionGroup> PageActionGroups { get; set; }
        public virtual List<PageActionGroup> InitializePageActions() { return null; }

        [NotMapped]
        public static List<PageActionGroup> ListPageActions { get; set; }
        #endregion

        #region Other
        public Type GetUnproxiedType()
        {
            var type = GetType();

            if (type.Namespace == "Castle.Proxies")
                return type.BaseType;

            return type;
        }
        #endregion
    }
}
