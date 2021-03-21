using BlazorBase.CRUD.Attributes;
using BlazorBase.CRUD.Models;
using BlazorBase.CRUD.Modules;
using BlazorBase.CRUD.ViewModels;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace BlazorBase.CRUD.NumberSeries
{
    [Route("/NoSeries")]
    public class NoSeries : BaseModel<NoSeries>, IBaseModel
    {
        [Key]
        [Required]
        [Visible]
        [StringLength(20)]
        public string Code { get; set; }

        [Visible]
        public string Description { get; set; }

        [Visible]
        [Required]
        [CheckValidSeriesNo]
        public string StartingNo { get; set; }

        [Visible]
        [Required]
        [CheckValidSeriesNo]
        public string EndingNo { get; set; }

        [Visible]
        [Editable(false)]
        public string LastNoUsed { get; set; }


        public override Task OnAfterPropertyChanged(string propertyName, object newValue, bool isValid, EventServices eventServices)
        {
            if (!isValid)
                return base.OnAfterPropertyChanged(propertyName, newValue, isValid, eventServices);

            switch (propertyName)
            {
                case nameof(StartingNo):
                case nameof(EndingNo):
                    CopyNo(propertyName, (string)newValue);
                    break;
            }

            return base.OnAfterPropertyChanged(propertyName, newValue, isValid, eventServices);
        }

        private void CopyNo(string propertyName, string newValue)
        {
            var isStartingNo = propertyName == nameof(StartingNo);
            var otherNo = isStartingNo ? EndingNo : StartingNo;
            if (!String.IsNullOrEmpty(otherNo))
                return;

            var maxSeriesNo = NoSeriesManager.GetMaxSeriesNo(newValue);
            if (isStartingNo)
                EndingNo = maxSeriesNo;
            else
                StartingNo = maxSeriesNo;

            ForcePropertyRepaint(isStartingNo ? nameof(EndingNo) : nameof(StartingNo));
        }

        public class CheckValidSeriesNoAttribute : ValidationAttribute
        {
            protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            {
                var localizer = (IStringLocalizer<NoSeries>)validationContext.Items[typeof(IStringLocalizer<NoSeries>)];

                var newValue = value as string;
                var model = (NoSeries)validationContext.ObjectInstance;

                if (String.IsNullOrEmpty(newValue))
                    return ValidationResult.Success;

                if (!NoSeriesManager.IsValidNoSeries(newValue))
                    return new ValidationResult(localizer["The no series must contain at least one digit"], new List<string>() { validationContext.MemberName });

                var otherNo = validationContext.MemberName == nameof(StartingNo) ? model.EndingNo : model.StartingNo;
                if (!NoSeriesManager.NoSeriesAreEqualExceptOfDigits(newValue, otherNo))
                    return new ValidationResult(localizer["The numbers of the start and end numbers must be in the same position and the remaining characters of the numbers must be identical"], new List<string>() { validationContext.MemberName });

                return ValidationResult.Success;
            }
        }
    }
}
