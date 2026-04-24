namespace Mnemo.Common
{
    public enum ErrorCode
    {
        // 400 BadRequest
        InvalidData,
        InvalidPassword,

        // 403 Forbidden
        ActionNotAllowed,
        SessionNotFinished,

        // 404 NotFound
        UserNotFound,
        EntryNotFound,
        StateNotFound,
        TaskNotFound,
        SessionNotFound,

        // 409 Conflict/Dublicate
        UsernameTaken,
        DuplicateEntry,
    }


    public class RequestResult<T>
    {
        public bool IsSuccess { get; }
        public T? Value { get; }
        public ErrorCode? ErrorCode { get; }
        public string? ErrorMessage { get; }


        private RequestResult(T value)
        {
            IsSuccess = true;
            Value = value;
        }

        private RequestResult(ErrorCode errorCode, string? errorMessage)
        {
            IsSuccess = false;
            ErrorCode = errorCode;
            ErrorMessage = errorMessage ?? errorCode.ToString();
        }


        public static RequestResult<T> Success(T value) => new RequestResult<T>(value);
        public static RequestResult<T> Failure(ErrorCode errorCode, string? errorMessage=null) => new RequestResult<T>(errorCode, errorMessage);
    }
}
