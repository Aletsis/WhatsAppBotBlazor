using Microsoft.VisualBasic;

namespace WhatsAppBot.Models;

public class WebhookPayload
{
    public string Object { get; set; }
    public List<Entry> Entry { get; set; } = new();
}

public class Entry
{
    public List<Change> Changes { get; set; } = new();
}

public class Change
{
    public Value Value { get; set; }
}

public class Value
{
    public List<Message> Messages { get; set; } = new();
}

public class Message
{
    public string From { get; set; }
    public MessageText? Text { get; set; }
    public Interactive? Interactive { get; set; }
    public string Type { get; set; }
}

public class MessageText
{
    public string Body { get; set; }
}
public class Interactive
{
    public string Type { get; set; }
    public ButtonReply? Button_Reply { get; set; }
    public ListReply? List_Reply { get; set; }
}
public class ButtonReply
{
    public string Id { get; set; }
    public string Title { get; set; }
}

public class ListReply
{
    public string Id { get; set; }
    public string Title { get; set; }
}