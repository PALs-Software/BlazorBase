using BlazorBase.CRUD.Attributes;
using BlazorBase.CRUD.Models;
using BlazorBase.CRUD.Modules;
using BlazorBase.CRUD.ViewModels;
using Microsoft.AspNetCore.Components;
using System;
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
        public string StartingNo { get; set; }

        [Visible]
        [Required]
        public string EndingNo { get; set; }

        [Visible]
        [Editable(false)]
        public string LastNoUsed { get; set; }

        public override Task OnBeforePropertyChanged(string propertyName, ref bool isHandled, ref string newValue, ref InputValidation inputValidation, EventServices services)
        {
            switch (propertyName)
            {
                case nameof(StartingNo):
                case nameof(EndingNo):
                    ValidateNo(ref newValue, propertyName == nameof(StartingNo) ? nameof(EndingNo) : nameof(StartingNo), ref isHandled, ref inputValidation, services);
                    break;
            }

            return base.OnBeforePropertyChanged(propertyName, ref isHandled, ref newValue, ref inputValidation, services);
        }

        private void ValidateNo(ref string newValue, string otherNoPropertyName, ref bool isHandled, ref InputValidation inputValidation, EventServices services)
        {
            if (String.IsNullOrEmpty(newValue))
                return;

            if (!NoSeriesManager.IsValidNoSeries(newValue, ref inputValidation, services))
            {
                isHandled = true;
                return;
            }

            var otherNo = otherNoPropertyName == nameof(StartingNo) ? StartingNo : EndingNo;
            if (String.IsNullOrEmpty(otherNo))
            {
                var maxSeriesNo = NoSeriesManager.GetMaxSeriesNo(newValue);
                if (otherNoPropertyName == nameof(StartingNo))
                    StartingNo = maxSeriesNo;
                else
                    EndingNo = maxSeriesNo;

                ForcePropertyRepaint(otherNoPropertyName);
                return;
            }

            if (!NoSeriesManager.NoSeriesAreEqualExceptOfDigits(newValue, otherNo))
            {
                inputValidation = new InputValidation(feedback: services.Localizer["The numbers of the start and end numbers must be in the same position and the remaining characters of the numbers must be identical"]);
                isHandled = true;
                return;
            }
        }

    }
}
