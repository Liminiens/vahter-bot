using MihaZupan;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nito.AsyncEx;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace JobsBot.Bot
{
    public class BotClient
    {
        private readonly string[] _invalidLinks = { "bit.ly", "t.me" };
        private readonly BotConfiguration _configuration;
        private readonly ITelegramBotClient _client;

        public BotClient(BotConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            if (String.IsNullOrWhiteSpace(_configuration.Token))
            {
                throw new ArgumentException("Token must be not null or empty or whitespace", nameof(_configuration.Token));
            }
            if (!_configuration.ChatId.HasValue)
            {
                throw new ArgumentException("ChatId must be not null", nameof(_configuration.ChatId));
            }
            if (!_configuration.ChannelId.HasValue)
            {
                throw new ArgumentException("ChannelId must be not null", nameof(_configuration.ChannelId));
            }
            if (configuration.Proxy != null)
            {
                var proxy = new HttpToSocks5Proxy(
                    configuration.Proxy.Host,
                    configuration.Proxy.Port,
                    configuration.Proxy.Username,
                    configuration.Proxy.Password);
                _client = new TelegramBotClient(_configuration.Token, proxy);
            }
            else
            {
                _client = new TelegramBotClient(_configuration.Token);
            }
        }

        public Task<User> GetMeAsync() => _client.GetMeAsync();

        public void Start()
        {
            _client.OnReceiveError += (sender, args) => Console.WriteLine(args.ApiRequestException);
            _client.OnReceiveGeneralError += (sender, args) => Console.WriteLine(args.Exception);
            _client.OnMessage += (sender, args) => AsyncContext.Run(() => OnBotMessage(sender, args));
            _client.OnMessageEdited += (sender, args) => AsyncContext.Run(() => OnBotMessage(sender, args));
            _client.OnCallbackQuery += (sender, args) => AsyncContext.Run(() => OnCallbackQuery(sender, args));
            _client.StartReceiving();
        }

        private Task SendUnbanKeyboard(User user, string text)
        {
            var builder = new StringBuilder();
            builder.AppendFormat(
                "Забанен: Id \"{0}\", Last Name \"{1}\", First Name \"{2}\", Username \"{3}\"\n\n",
                user.Id,
                user.FirstName,
                user.LastName,
                user.Username);
            builder.Append(text);

            var inlineKeyboard = new InlineKeyboardMarkup(new[]
            {
                new []
                {
                    InlineKeyboardButton.WithCallbackData("Unban", user.Id.ToString()),
                    InlineKeyboardButton.WithCallbackData("Пёс с ним")
                }
            });

            return _client.SendTextMessageAsync(_configuration.ChannelId, builder.ToString(), replyMarkup: inlineKeyboard);
        }

        private Task Restrict(int userId)
        {
            return _client.RestrictChatMemberAsync(_configuration.ChatId, userId, DateTime.MaxValue, false, false, false, false);
        }

        private Task UnRestrict(int userId)
        {
            return _client.RestrictChatMemberAsync(_configuration.ChatId, userId, DateTime.UtcNow.AddMinutes(1), true, true, true, true);
        }

        private async void OnCallbackQuery(object sender, CallbackQueryEventArgs args)
        {
            try
            {
                var query = args.CallbackQuery;
                if (query.Data != "Пёс с ним")
                {
                    var id = Int32.Parse(query.Data);
                    await UnRestrict(id);
                    await _client.SendTextMessageAsync(_configuration.ChannelId, $"Разбанил {id}");
                }

                if (query.Message != null)
                {
                    await _client.DeleteMessageAsync(query.Message.Chat.Id, query.Message.MessageId);
                }

            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        private async void OnBotMessage(object sender, MessageEventArgs args)
        {
            try
            {
                var message = args.Message;
                var text = message.Text ?? message.Caption;
                if (message.Chat.Id == _configuration.ChatId && text != null)
                {
                    await OnChatMessage(message, text);
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        private async Task OnChatMessage(Message message, string text)
        {
            if (_invalidLinks.Any(text.Contains))
            {
                await Restrict(message.From.Id);
                await _client.DeleteMessageAsync(message.Chat.Id, message.MessageId);
                await SendUnbanKeyboard(message.From, text);
            }
        }
    }
}
