namespace Mnemo.Common
{
    public class RequestResult<T>
    {
        public bool IsSuccess { get; }
        public T? Value { get; }
        public string? ErrorCode { get; }
        // public string? ErrorMessage { get; }


        private RequestResult(T value)
        {
            IsSuccess = true;
            Value = value;
        }

        private RequestResult(string errorCode)
        {
            IsSuccess = false;
            ErrorCode = errorCode.ToUpper();
        }


        public static RequestResult<T> Success(T value) => new RequestResult<T>(value);
        public static RequestResult<T> Failure(string errorCode) => new RequestResult<T>(errorCode);
    }
}
