namespace TTCS.Meta.Common
{
    public readonly struct ValidationResult
    {
        public bool IsValid { get; }
        public string Message { get; }

        private ValidationResult(bool isValid, string message)
        {
            IsValid = isValid;
            Message = message;
        }

        public static ValidationResult Valid()
        {
            return new ValidationResult(true, string.Empty);
        }

        public static ValidationResult Invalid(string message)
        {
            return new ValidationResult(false, message ?? "Validation failed.");
        }
    }
}
