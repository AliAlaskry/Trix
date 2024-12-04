using System;

[Flags]
public enum ErrorType : int
{
    OfflineMode = 2,
    NetwrokConnection = 4,
    RequestRedirectLimitOut = 8,
    AccessTokenExpired = 16,
    AccessTokenMissing = 32,
    AccessTokenInvalid = 64,
    GameEnded = 128,
    Unknonw = 256,
}

[Flags]
public enum ErrorWindowButtons : int
{
    Reconnect = ErrorType.NetwrokConnection | ErrorType.OfflineMode,
    Retry = ErrorType.RequestRedirectLimitOut,
    Quit = ErrorType.AccessTokenExpired | ErrorType.AccessTokenMissing | ErrorType.Unknonw | ErrorType.GameEnded | ErrorType.AccessTokenInvalid,
}

// replace 
// each _ with " " 
// each 1 with \n
[Flags]
public enum ErrorMessages : int
{
    Cannot_Play_Before_Connect1Connect_Again = ErrorType.OfflineMode,
    Check_Your_Internet_Connection = ErrorType.NetwrokConnection | ErrorType.RequestRedirectLimitOut,
    Your_Access_Token_Expired1Please_Back_To_App = ErrorType.AccessTokenExpired,
    Your_Access_Token_Missed1Please_Back_To_App = ErrorType.AccessTokenMissing,
    Your_Access_Token_Invalid1Please_Back_To_App = ErrorType.AccessTokenInvalid,
    Game_Ended1Please_Back_To_App = ErrorType.GameEnded,
    Error_Occurred = ErrorType.Unknonw,
}

[Serializable]
public class Error
{
    public Error(ErrorType type)
    {
        Debugging.Print("error occured " + type);
        if (DewaniaHostController.CurrentErrorType != -1 && (ErrorType)DewaniaHostController.CurrentErrorType == ErrorType)
        {
            Debugging.Print("the same current errro");
            return;
        }

        DewaniaHostController.CurrentErrorType = ((int)type);

        ErrorType = type;
        SetMessage();
    }

    #region Props
    public ErrorType ErrorType { get; private set; }
    public ErrorMessages MessageEnum { get; private set; }
    public string Message { get; private set; }
    #endregion

    #region Fns
    void SetMessage()
    {
        int errorCode = ((int)ErrorType);
        Array messagesArr = Enum.GetValues(typeof(ErrorMessages));
        foreach (var message in messagesArr)
        {
            ErrorMessages messageEnum = (ErrorMessages)Enum.Parse(typeof(ErrorMessages), message.ToString());
            int messageCode = ((int)messageEnum);
            if ((errorCode & messageCode) != 0)
            {
                MessageEnum = messageEnum;
                Message = MessageEnum.ToString();
                Message = Message.Replace('_', ' ').Replace('1', '\n');
                break;
            }
        }
    }

    public void Dispose()
    {
        Debugging.Print($"error of type = {ErrorType} disposed");
        DewaniaHostController.CurrentErrorType = -1;
    }
    #endregion
}
