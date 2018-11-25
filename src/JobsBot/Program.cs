using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using JobsBot.Bot;
using Newtonsoft.Json;
using Nito.AsyncEx;

namespace JobsBot
{
    class Program
    {
        static async Task Main(string[] args)
        {
            try
            {
                var settings = File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "settings.json"));
                var configuration = JsonConvert.DeserializeObject<BotConfiguration>(settings);
                var bot = new BotClient(configuration);
                var self = await bot.GetMeAsync();
                Console.WriteLine($"Self id: {self.Id}");
                bot.Start();
                Console.WriteLine("Started bot");
                await Task.Delay(Timeout.Infinite);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }
}
