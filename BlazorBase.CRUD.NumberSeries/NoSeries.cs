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
        public string Id { get; set; }

        [Visible]
        public string Description { get; set; }

        [Visible]
        [Required]
        [CheckValidSeriesNo(onlyCheckHasDigits: true)]
        public string StartingNo { get; set; }

        [Visible]
        [Required]
        [CheckValidSeriesNo]
        public string EndingNo { get; set; }

        [Visible]
        [Editable(false)]
        public string LastNoUsed { get; set; }

        public long EndingNoNumeric { get; set; }
        public long LastNoUsedNumeric { get; set; }
        public int NoOfDigits{ get; set; }

        public override Task OnAfterPropertyChanged(string propertyName, object newValue, bool isValid, EventServices eventServices)
        {
            if (!isValid)
                return base.OnAfterPropertyChanged(propertyName, newValue, isValid, eventServices);

            switch (propertyName)
            {
                case nameof(StartingNo):
                    GenerateEndingNo((string)newValue);
                    break;
            }

            return base.OnAfterPropertyChanged(propertyName, newValue, isValid, eventServices);
        }

        private void GenerateEndingNo(string newValue)
        {
            EndingNo = NoSeriesManager.GetMaxSeriesNo(newValue);
            ForcePropertyRepaint(nameof(EndingNo));
        }

        public void IncreaseNo()
        {
            if (LastNoUsedNumeric + 1 > EndingNoNumeric)
                throw new CRUDException($"The defined maximum of the no series is reached, please create a new number series");

            LastNoUsedNumeric++;
            var lastNoUsed = LastNoUsedNumeric.ToString().PadLeft(NoOfDigits, '0');

            var result = String.Empty;
            foreach (var item in LastNoUsed)
            {
                if (char.IsDigit(item)) { 
                    result += lastNoUsed[0];
                    lastNoUsed = lastNoUsed.Substring(1);
                }
                else
                    result += item;
            }

            LastNoUsed = result;
        }

        public class CheckValidSeriesNoAttribute : ValidationAttribute
        {
            public bool OnlyCheckHasDigits { get; init; }
            public CheckValidSeriesNoAttribute(bool onlyCheckHasDigits = false)
            {
                OnlyCheckHasDigits = onlyCheckHasDigits;
            }

            protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            {
                var localizer = (IStringLocalizer<NoSeries>)validationContext.Items[typeof(IStringLocalizer<NoSeries>)];

                var newValue = value as string;
                var model = (NoSeries)validationContext.ObjectInstance;

                if (String.IsNullOrEmpty(newValue))
                    return ValidationResult.Success;

                if (!NoSeriesManager.IsValidNoSeries(newValue))
                    return new ValidationResult(localizer["The no series must contain at least one digit"], new List<string>() { validationContext.MemberName });

                if (!OnlyCheckHasDigits)
                {
                    var otherNo = validationContext.MemberName == nameof(StartingNo) ? model.EndingNo : model.StartingNo;
                    if (!NoSeriesManager.NoSeriesAreEqualExceptOfDigits(newValue, otherNo))
                        return new ValidationResult(localizer["The numbers of the start and end numbers must be in the same position and the remaining characters of the numbers must be identical"], new List<string>() { validationContext.MemberName });
                }

                return ValidationResult.Success;
            }
        }
    }
}
