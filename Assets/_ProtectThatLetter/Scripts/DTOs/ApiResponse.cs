using System;

[Serializable]
public class ApiResponse<T>
{
    public bool success;
    public string message;
    public T data;
    public string errorCode;
}

public static class ErrorCodes {
    public const string ERROR_DATABASE_OFFLINE = "ERROR_DATABASE_OFFLINE";
    public const string ERROR_INTERNAL_SERVER = "ERROR_INTERNAL_SERVER";
    public const string ERROR_INVALID_CODE = "ERROR_INVALID_CODE";
    public const string ERROR_EXPIRED_CODE = "ERROR_EXPIRED_CODE";
}
