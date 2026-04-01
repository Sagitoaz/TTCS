namespace TTCS.Meta.Inventory
{
    public readonly struct UseItemResult
    {
        public bool Success { get; }
        public string Message { get; }
        public int QuantityUsed { get; }

        public UseItemResult(bool success, string message, int quantityUsed)
        {
            Success = success;
            Message = message;
            QuantityUsed = quantityUsed;
        }

        public static UseItemResult Failed(string message)
        {
            return new UseItemResult(false, message, 0);
        }

        public static UseItemResult Ok(int quantityUsed, string message = "")
        {
            return new UseItemResult(true, message, quantityUsed);
        }
    }
}
