using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorBase.CRUD.ViewModels
{
    public class InputValidation
    {
        public InputValidation(bool showValidation = true, bool isValid = false, string feedback = "")
        {
            ShowValidation = showValidation;
            IsValid = isValid;
            Feedback = feedback;
        }

        public bool ShowValidation { get; set; }
        public bool IsValid { get; set; }
        public string Feedback { get; set; }
    }
}
