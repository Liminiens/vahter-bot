namespace JobsBot.Bot
{
    public class BotConfiguration
    {
        public string Token { get; set; }
        public long? ChatId { get; set; }
        public long? ChannelId { get; set; }
        public BotProxy Proxy { get; set; }
    }
}
