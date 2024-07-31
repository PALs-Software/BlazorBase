namespace BlazorBase.Abstractions.CRUD.Arguments;

public class BasePrimitiveInputArgs
{
    public record OnFormatValueArgs(Dictionary<string, object> InputAttributes)
    {
        public OnFormatValueArgs(Dictionary<string, object> inputAttributes, string? feedbackClass, string? inputClass, string? feedback, bool isReadOnly) : this(inputAttributes)
        {
            FeedbackClass = feedbackClass;
            InputClass = inputClass;
            Feedback = feedback;

            IsReadOnly = isReadOnly;
        }

        public string? FeedbackClass { get; set; }
        public string? InputClass { get; set; }
        public string? Feedback { get; set; }

        public bool IsReadOnly { get; set; }
    }

    public record OnBeforeConvertTypeArgs(object? OldValue)
    {
        public OnBeforeConvertTypeArgs(object? newValue, object? oldValue) : this(oldValue) => NewValue = newValue;
        public object? NewValue { get; set; }
    }

    public record OnValueChangedArgs(object? OldValue, bool IsValid)
    {
        public OnValueChangedArgs(object? newValue, object? oldValue, bool isValid) : this(oldValue, isValid) => NewValue = newValue;
        public object? NewValue { get; set; }
    }

    #region Validation
    public record OnBeforeValidateArgs()
    {
        public bool IsValid { get; set; }
        public bool IsHandled { get; set; }
        public string? ErrorMessage { get; set; }
    }
    public record OnAfterValidateArgs()
    {
        public OnAfterValidateArgs(bool isValid, string? errorMessage) : this()
        {
            IsValid = isValid;
            ErrorMessage = errorMessage;
        }

        public bool IsValid { get; set; }
        public string? ErrorMessage { get; set; }
    }
    #endregion
}
