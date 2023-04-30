namespace ProteiTelegramBot.Models;

public abstract class Notification
{
    public abstract string GetMessage();
}

public abstract class MergeRequestNotificationBase : Notification
{ 
    protected static string RemoveInvalidTelegramCharacters(string text)
    {
        var invalidChars = new[]
        {
            '_', '*', '[', ']', '(', ')', '~', '`', '>', '#', '+', '-', '=', '|', '{', '}', '.', '!'
        };

        foreach (var invalidChar in invalidChars)
        {
            text = text.Replace(invalidChar, default);
        }

        return text;
    }
}
public class MergeRequestOpenedNotification : MergeRequestNotificationBase
{
    private readonly string _url;
    private readonly string _title;
    private readonly string _creator;

    public MergeRequestOpenedNotification(string url,
        string title,
        string creator)
    {
        _url = url;
        _title = title;
        _creator = creator;
    }

    public override string GetMessage()
    {
        var messege =
            "***⚠️ Merge Request | Opened ***" +
            "\r\n" +
            $"[{RemoveInvalidTelegramCharacters(_title)}]({_url}) (opened by {_creator})";
        return messege;
    }

  
}

public class MergeRequestMergedNotification : MergeRequestNotificationBase
{
    private readonly string _url;
    private readonly string _title;

    public MergeRequestMergedNotification(string url,
        string title)
    {
        _url = url;
        _title = title;
    }

    public override string GetMessage()
    {
        var messege =
            "***✅ Merge Request | Merged ***" +
            "\r\n" +
            $"[{RemoveInvalidTelegramCharacters(_title)}]({_url})";
        return messege;
    }
}

public class PipelineNotification : MergeRequestNotificationBase
{
    private readonly string _url;
    private readonly string _name;
    private readonly string _status;


    public PipelineNotification(string url,
        string name, string status)
    {
        _url = url;
        _name = name;
        _status = status;
    }

    public override string GetMessage()
    {
        var message =
            "***⚠️ Pipeline failed ***" +
            "\r\n" +
            $"Project: [{RemoveInvalidTelegramCharacters(_name)}]({_url})"+
            "\r\n" +
            $"Status: {_status}";
        return message;
    }
}

