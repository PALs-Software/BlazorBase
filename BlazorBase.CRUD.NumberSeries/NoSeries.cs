using BlazorBase.CRUD.Attributes;
using BlazorBase.CRUD.EventArguments;
using BlazorBase.CRUD.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace BlazorBase.CRUD.NumberSeries
{
    [Route("/NoSeries")]
    [Authorize(Policy = nameof(NoSeries))]
    public class NoSeries : BaseModel
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
        public int NoOfDigits { get; set; }

        public override Task OnAfterPropertyChanged(OnAfterPropertyChangedArgs args)
        {
            if (!args.IsValid)
                return base.OnAfterPropertyChanged(args);

            switch (args.PropertyName)
            {
                case nameof(StartingNo):
                    var noSeriesService = (NoSeriesService)args.EventServices.ServiceProvider.GetService(typeof(NoSeriesService));
                    GenerateEndingNo(noSeriesService, (string)args.NewValue);
                    break;
            }

            return base.OnAfterPropertyChanged(args);
        }

        private void GenerateEndingNo(NoSeriesService service, string newValue)
        {
            EndingNo = service.GetMaxSeriesNo(newValue);
            ForcePropertyRepaint(nameof(EndingNo));
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
                var localizer = (IStringLocalizer)validationContext.Items[typeof(IStringLocalizer)];

                var newValue = value as string;
                var model = (NoSeries)validationContext.ObjectInstance;

                if (String.IsNullOrEmpty(newValue))
                    return ValidationResult.Success;

                var noSeriesService = (NoSeriesService)validationContext.GetService(typeof(NoSeriesService));
                if (!noSeriesService.IsValidNoSeries(newValue))
                    return new ValidationResult(localizer["The no series must contain at least one digit"], new List<string>() { validationContext.MemberName });

                if (!OnlyCheckHasDigits)
                {
                    var otherNo = validationContext.MemberName == nameof(StartingNo) ? model.EndingNo : model.StartingNo;
                    if (!noSeriesService.NoSeriesAreEqualExceptOfDigits(newValue, otherNo))
                        return new ValidationResult(localizer["The numbers of the start and end numbers must be in the same position and the remaining characters of the numbers must be identical"], new List<string>() { validationContext.MemberName });
                }

                return ValidationResult.Success;
            }
        }
    }
}
