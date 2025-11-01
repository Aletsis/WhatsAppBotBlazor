namespace WhatsAppBot.Data.DTOs
{
    public class ConversationHistory
    {
        public string PhoneNumber { get; set; } = string.Empty;
        public DateTime LastMessageDate { get; set; }
        public int MessageCount { get; set; }
    }
}