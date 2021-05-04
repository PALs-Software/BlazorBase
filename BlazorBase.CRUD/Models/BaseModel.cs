using BlazorBase.CRUD.Attributes;
using BlazorBase.CRUD.Components;
using BlazorBase.CRUD.Enums;
using BlazorBase.CRUD.Extensions;
using BlazorBase.CRUD.Resources.ValidationAttributes;
using BlazorBase.CRUD.ViewModels;
using Blazorise;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.WebUtilities;
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
    public class BaseModel : ComponentBase, IBaseModel
    {
        #region Events
        public event EventHandler<string> OnForcePropertyRepaint;

        public event EventHandler OnReloadEntityFromDatabase;
        #endregion

        public BaseModel()
        {
            CreatedOn = DateTime.Now;
            ModifiedOn = CreatedOn;
        }

        #region Additional Properties
        [Editable(false)]
        [DateDisplayMode(DateInputMode = DateInputMode.DateTime)]
        [Visible(DisplayGroup = "Information", DisplayGroupOrder = 9999, DisplayOrder = 100, Collapsed = true)]
        public DateTime CreatedOn { get; set; }

        [Editable(false)]
        [DateDisplayMode(DateInputMode = DateInputMode.DateTime)]
        [Visible(DisplayGroup = "Information", DisplayOrder = 200)]
        public DateTime ModifiedOn { get; set; }
        #endregion

        #region Attribute Methods
        public List<PropertyInfo> GetVisibleProperties()
        {
            return GetType().GetVisibleProperties();
        }

        public object[] GetPrimaryKeys()
        {
            var keyProperties = GetKeyProperties();

            var keys = new object[keyProperties.Count];
            for (int i = 0; i < keyProperties.Count; i++)
                keys[i] = keyProperties.ElementAt(i).GetValue(this);

            return keys;
        }

        public string GetPrimaryKeysAsString()
        {
            return String.Join(", ", GetPrimaryKeys());
        }

        public List<PropertyInfo> GetKeyProperties()
        {
            return GetType().GetKeyProperties();
        }

        public Dictionary<string, string> GetNavigationQuery(string baseQuery = null)
        {
            var query = new Dictionary<string, string>();
            if (baseQuery != null)
                query = QueryHelpers.ParseQuery(baseQuery).ToDictionary(key => key.Key, val => val.Value.ToString());

            var keyProperties = GetKeyProperties();

            var primaryKeys = new List<object>();
            foreach (var keyProperty in keyProperties)
                query[keyProperty.Name] = keyProperty.GetValue(this).ToString();

            return query;
        }
        #endregion

        #region CRUD Methods
        public void ForcePropertyRepaint(string propertyName)
        {
            OnForcePropertyRepaint?.Invoke(this, propertyName);
        }

        public void ReloadEntityFromDatabase()
        {
            OnReloadEntityFromDatabase?.Invoke(this, EventArgs.Empty);
        }

        #region Events

        #region DbContext

        public virtual Task OnBeforeDbContextAddEntry(OnBeforeDbContextAddEntryArgs args) { return Task.CompletedTask; }
        public virtual Task OnAfterDbContextAddedEntry(OnAfterDbContextAddedEntryArgs args) { return Task.CompletedTask; }

        public virtual Task OnBeforeDbContextModifyEntry(OnBeforeDbContextModifyEntryArgs args)
        {
            ModifiedOn = DateTime.Now;
            return Task.CompletedTask;
        }
        public virtual Task OnAfterDbContextModifiedEntry(OnAfterDbContextModifiedEntryArgs args) { return Task.CompletedTask; }

        public virtual Task OnBeforeDbContextDeleteEntry(OnBeforeDbContextDeleteEntryArgs args) { return Task.CompletedTask; }
        public virtual Task OnAfterDbContextDeletedEntry(OnAfterDbContextDeletedEntryArgs args) { return Task.CompletedTask; }
        #endregion
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

        public virtual Task OnAfterSaveChanges(OnAfterSaveChangesArgs args)
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

        public bool TryValidate(out List<ValidationResult> validationResults, EventServices eventServices)
        {
            var validationContext = new ValidationContext(this, eventServices.ServiceProvider, new Dictionary<object, object>()
            {
                [eventServices.Localizer.GetType()] = eventServices.Localizer,
                [typeof(DbContext)] = eventServices.BaseService.DbContext
            });

            return TryValidate(out validationResults, validationContext);
        }

        #endregion

        #region ComponentBase
        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            base.BuildRenderTree(builder);

            if (ShowOnlySingleEntry)
                BuildCardComponent(builder);
            else
                BuildListComponent(builder);
        }

        protected virtual void BuildListComponent(RenderTreeBuilder builder)
        {
            builder.OpenComponent(0, typeof(BaseList<>).MakeGenericType(GetType()));
            builder.CloseComponent();
        }

        public virtual bool ShowOnlySingleEntry { get; }
        protected virtual void BuildCardComponent(RenderTreeBuilder builder)
        {
            builder.OpenComponent(0, typeof(BaseCard<>).MakeGenericType(GetType()));
            builder.AddAttribute(1, "ShowEntryByStart", ShowOnlySingleEntry);
            builder.AddAttribute(2, "EntryToBeShownByStart", new Func<EventServices, Task<IBaseModel>>(GetShowOnlySingleEntryInstance));
            builder.CloseComponent();
        }
        public virtual async Task<IBaseModel> GetShowOnlySingleEntryInstance(EventServices eventServices)
        {
            return await Task.Run(() =>
            {
                return (IBaseModel)eventServices.BaseService.Set(GetType()).FirstOrDefault();
            });
        }
        #endregion

        #region PageActions
        public virtual List<PageActionGroup> GeneratePageActionGroups() { return null; }
        #endregion

        #region Other
        public Type GetUnproxiedType()
        {
            var type = GetType();

            if (type.Namespace == "Castle.Proxies")
                return type.BaseType;

            return type;
        }

        public void TransferPropertiesTo(object target)
        {
            var sourceProperties = this.GetType().GetProperties();
            var targetProperties = target.GetType().GetProperties();

            foreach (var sourceProperty in sourceProperties)
            {
                var targetProperty = targetProperties.Where(entry => entry.Name == sourceProperty.Name).FirstOrDefault();

                if (targetProperty == null ||
                 (!sourceProperty.CanRead || !targetProperty.CanWrite) ||
                 (!targetProperty.PropertyType.IsAssignableFrom(sourceProperty.PropertyType)) ||
                 ((targetProperty.GetSetMethod().Attributes & MethodAttributes.Static) != 0))
                    continue;

                targetProperty.SetValue(target, sourceProperty.GetValue(this));
            }
        }
        #endregion
    }
}
