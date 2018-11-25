using System;
using System.IO;
using System.Threading.Tasks;
using JobsBot.Bot;
using Newtonsoft.Json;
using Nito.AsyncEx;

namespace JobsBot
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                var settings = File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "settings.json"));
                var configuration = JsonConvert.DeserializeObject<BotConfiguration>(settings);
                var bot = new BotClient(configuration);
                bot.Start();
                Console.WriteLine("Started bot");
                Console.In.ReadLineAsync().GetAwaiter().GetResult();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }
}
