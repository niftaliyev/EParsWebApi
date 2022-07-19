using System;
using System.Collections.Generic;
using Telegram.Bot;

namespace WebApi.Services
{
    public static class TelegramBotService
    {
       // public static TelegramBotClient Client = new TelegramBotClient("2093531413:AAHMSMt0m3GSpM8svr6_hTsTF4OZIU_JGcI");

        //EmlakCrawler
        public static TelegramBotClient Client = new TelegramBotClient("5486861849:AAHhXVeEzx5gS6yeCAHUuRXD1EXD7VBQplc");
        public static HashSet<long> chatIds = new HashSet<long>();

        [Obsolete]
        static TelegramBotService()
        {
            Client.OnMessage += Client_OnMessage;
            Client.StartReceiving();
        }

        [Obsolete]
        private static void Client_OnMessage(object sender, Telegram.Bot.Args.MessageEventArgs e)
        {
            var id = e.Message.Chat.Id;
            var text = e.Message.Text;
            if (text.ToLower().Trim() == "stop")
            {
                chatIds.Remove(id);
                Client.SendTextMessageAsync(id, "Siz qrupdan ayrıldınız");

            }

            if (!chatIds.Contains(id))
            {

                if (text.Trim() == "123")
                {
                    chatIds.Add(id);
                    Client.SendTextMessageAsync(id, "Siz qrupa qoçuldunuz");
                }
               
            }

            else
            {
                if (text.Trim() == "123") 
                    Client.SendTextMessageAsync(id, "Siz qrupda varsınız , yenidən qoşulmağa ehtiyac yoxdur !");
                else
                    Client.SendTextMessageAsync(id, "Yazdığınız səhvdir !");
            }
        }

        public static void Sender(string message)
        {
            foreach (var id in chatIds)
            {
                Client.SendTextMessageAsync(id,message);
            }

        }
    }
}
