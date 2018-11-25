using System;
using System.IO;
using System.Threading.Tasks;
using JobsBot.Bot;
using Newtonsoft.Json;

namespace JobsBot
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var settings = File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "settings.json"));
            var configuration = JsonConvert.DeserializeObject<BotConfiguration>(settings);
            var bot = new BotClient(configuration);
            bot.Start();
            await Console.In.ReadLineAsync();
        }
    }
}
