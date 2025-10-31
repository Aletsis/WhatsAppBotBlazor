using Microsoft.VisualBasic;
using MudBlazor;

namespace WhatsAppBot.Models;

public class WebhookPayload
{
    public string Object { get; set; }
    public List<Entry> Entry { get; set; } = new();
}

public class Entry
{
    public string Id { get; set; }
    public List<Change> Changes { get; set; } = new();
}

public class Change
{
    public Value Value { get; set; }
    public string Field { get; set; }
}

public class Value
{
    public string Messaging_Product { get; set; }
    public Metadata Metadata { get; set; } = new();
    public List<Message> Messages { get; set; } = new();
    public List<Contact> Contacts { get; set; } = new();
}

public class Metadata
{
    public string Display_Phone_Number { get; set; }
    public string Phone_Number_Id { get; set; }
}

public class Contact
{
    public string Wa_Id { get; set; }
    public Profile Profile { get; set; }
}

public class Profile
{
    public string Name { get; set; }
}

public class Message
{
    public string From { get; set; }
    public string Id { get; set; }
    public MessageText? Text { get; set; }
    public Interactive? Interactive { get; set; }
    public string? Type { get; set; }
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