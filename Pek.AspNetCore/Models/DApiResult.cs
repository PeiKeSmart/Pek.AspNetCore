namespace Pek.Models;

public class DApiResult
{
    public String msg { get; set; } = "OK";

    public ErrorCode error { get; set; } = ErrorCode.FAIL;

    public String errorCode { get; set; }

    public Object result { get; set; }

    public DApiResult ToObject()
    {
        return new DApiResult { msg = msg, error = error, errorCode = error.ToString(), result = result };
    }
}

public enum ErrorCode
{
    FAIL,
    SUCCESS
}