using System;
using MessageSender.Models;
using Microsoft.Extensions.Configuration;
using Telegram.Bot;

namespace MessageSender.Services
{
    public class TelegramSender : ITelegramSender
    {
        private readonly ITelegramBotClient _bot;
        public TelegramSender(IConfiguration config)
        {
            var apiKey = config["Telegram:ApiKey"];
            _bot = new TelegramBotClient(apiKey);
        }
        public void SendMessage(string message, string chat)
        {
            var t = _bot.SendTextMessageAsync(chat, message).Result;
            Console.WriteLine($"Message: {message} was sent to {chat}");
        }
    }

    public interface ITelegramSender
    {
        public void SendMessage(string message, string chat);
    }
}