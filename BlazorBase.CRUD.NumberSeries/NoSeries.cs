using BlazorBase.Abstractions.CRUD.Arguments;
using BlazorBase.Abstractions.CRUD.Attributes;
using BlazorBase.Abstractions.CRUD.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
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
        public string Id { get; set; } = default!;

        [Visible]
        public string? Description { get; set; }

        [Visible]
        [Required]
        [CheckValidSeriesNo(onlyCheckHasDigits: true)]
        public string StartingNo { get; set; } = default!;

        [Visible]
        [Required]
        [CheckValidSeriesNo]
        public string EndingNo { get; set; } = default!;

        [Visible]
        [CheckValidSeriesNo]
        public string? LastNoUsed { get; set; }

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
                    var noSeriesService = args.EventServices.ServiceProvider.GetRequiredService<NoSeriesService>();
                    GenerateEndingNo(noSeriesService, (string?)args.NewValue ?? String.Empty);
                    break;

                case nameof(LastNoUsed):
                    if (!String.IsNullOrEmpty(LastNoUsed))
                        LastNoUsedNumeric = long.Parse(new String(LastNoUsed.Where(char.IsDigit).ToArray()));
                    break;

                case nameof(EndingNo):
                    if (!String.IsNullOrEmpty(LastNoUsed))
                        EndingNoNumeric = long.Parse(new String(EndingNo.Where(char.IsDigit).ToArray()));
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

            protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
            {
                var localizer = (IStringLocalizer?)validationContext.Items[typeof(IStringLocalizer)];

                var newValue = value as string;
                var model = (NoSeries)validationContext.ObjectInstance;

                if (String.IsNullOrEmpty(newValue))
                    return ValidationResult.Success;

                var noSeriesService = validationContext.GetRequiredService<NoSeriesService>();
                if (!noSeriesService.IsValidNoSeries(newValue))
                    return new ValidationResult(localizer?["The no series must contain at least one digit"] ?? "The no series must contain at least one digit", new List<string>() { validationContext.MemberName ?? String.Empty });

                if (!OnlyCheckHasDigits)
                {
                    var otherNo = validationContext.MemberName == nameof(StartingNo) ? model.EndingNo : model.StartingNo;
                    if (!noSeriesService.NoSeriesAreEqualExceptOfDigits(newValue, otherNo))
                        return new ValidationResult(localizer?["The numbers of the start and end numbers must be in the same position and the remaining characters of the numbers must be identical"] ?? "The numbers of the start and end numbers must be in the same position and the remaining characters of the numbers must be identical", new List<string>() { validationContext.MemberName ?? String.Empty });
                }

                return ValidationResult.Success;
            }
        }
    }
}
