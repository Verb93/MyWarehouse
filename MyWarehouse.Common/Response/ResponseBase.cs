namespace MyWarehouse.Common.Response;

public class ResponseBase<T>
{
    public bool Result { get; set; }
    public T? Data { get; set; }
    public string? ErrorMessage { get; set; }
    public ErrorCode ErrorCode { get; set; }

    public static ResponseBase<T> Success(T data)
    {
        return new ResponseBase<T> { Result = true, Data = data };
    }

    public static ResponseBase<T> Fail(string errorMessage, ErrorCode errorCode)
    {
        return new ResponseBase<T> {Result = false, ErrorMessage = errorMessage, ErrorCode = errorCode };
    }


}
