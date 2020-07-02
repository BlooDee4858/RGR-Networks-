using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AnimalVictorineBot
{
    public class Program :ModuleBase<SocketCommandContext>
    {
        static void Main(string[] args) => new Program().RunBotAsync().GetAwaiter().GetResult();
        public static DiscordSocketClient _client;
        private CommandService _commands;
        private IServiceProvider _services;
        static string botToken = "";
        static string channelId = "";
        public async Task RunBotAsync()
        {
            _client = new DiscordSocketClient();
            _commands = new CommandService();
            _services = new ServiceCollection()
                .AddSingleton(_client)
                .AddSingleton(_commands)
                .BuildServiceProvider();
            string serialized = "";
            dynamic json = "";
            Console.WriteLine("Загрузка конфига...");
            if (File.Exists(Directory.GetCurrentDirectory() + "\\config.json"))
            {
                serialized = File.ReadAllText(Directory.GetCurrentDirectory() + "\\config.json");
                json = JObject.Parse(serialized);
                botToken = json.token;
                channelId = json.channelId;
                Console.WriteLine("Конфиг загружен...");
            }
            else
            {
                Console.WriteLine("Создаем новый конфиг...");
                app_config config = new app_config()
                {
                    token = "no",
                    channelId = "no",
                };
                json = JsonConvert.SerializeObject(config, Formatting.Indented);
                File.WriteAllText(Directory.GetCurrentDirectory() + "\\config.json", json);
                Console.WriteLine("Конфиг загружен...");
            }
            _client.Log += Log;
            _client.MessageReceived += _client_MessageReceived;
            _client.Ready += init;
            await RegisterCommandsAsync();
            await _client.LoginAsync(TokenType.Bot, botToken);
            await _client.StartAsync();
            await Task.Delay(-1);
        }
        public bool cardExists(string id = "")
        {
            if (File.Exists(Directory.GetCurrentDirectory() + "\\Users\\" + id + ".json"))
                return true;
            return false;
        }
        public async void createNewUser(string id)
        {
            Users_Data config = new Users_Data()
            {
                xp = "1",
            };
            dynamic json = JsonConvert.SerializeObject(config, Formatting.Indented);
            File.WriteAllText(Directory.GetCurrentDirectory() + "\\Users\\" + id + ".json", json);
        }
        private async Task _client_MessageReceived(SocketMessage arg)
        {
            if (arg.Channel.Id.ToString() != channelId) return;
            if (arg.Content != "1" && arg.Content != "2" && arg.Content != "3" && arg.Content != "4")
            {
                return;
            }
            if (arg.Content == correctAnswer)
            {
                IUser user = null;
                List<IMessage> messages = await arg.Channel.GetMessagesAsync().Flatten().ToListAsync();
                foreach (var k in messages)
                {
                    if (k.Id == arg.Id)
                    {
                        user = k.Author;
                        break;
                    }
                }
                if (!cardExists(user.Id.ToString()))
                {
                    createNewUser(user.Id.ToString());
                }
                else
                {
                    dynamic json = JObject.Parse(File.ReadAllText(Directory.GetCurrentDirectory() + "\\Users\\" + user.Id.ToString() + ".json"));
                    string xp = json.xp;
                    int xp_ = Int32.Parse(xp);
                    xp_++;
                    Users_Data config = new Users_Data()
                    {
                        xp = xp_.ToString(),
                    };
                    json = JsonConvert.SerializeObject(config, Formatting.Indented);
                    File.WriteAllText(Directory.GetCurrentDirectory() + "\\Users\\" + user.Id.ToString() + ".json", json);

                }
                await newanimal();
            }
            else
            {
                EmbedBuilder b = new EmbedBuilder();
                await arg.Channel.SendMessageAsync("", false, b
                    .WithTitle("Ошибка!")
                    .WithDescription("Неверный ответ!")
                    .WithColor(Color.Red)
                    .Build());
            }
        }
        [Command("rating", RunMode = RunMode.Async)]
        public async Task top()
        {
            string[] filePaths = Directory.GetFiles(Directory.GetCurrentDirectory() + "\\Users\\");
            List<string> ids = new List<string>();
            List<string> money = new List<string>();
            List<string> new_money = new List<string>();
            List<int> sorted_money = new List<int>();
            List<string> user_names = new List<string>();
            List<string> new_names = new List<string>();
            foreach (string s in filePaths)
            {
                string file_name = Path.GetFileNameWithoutExtension(s);
                ids.Add(file_name);
                string readed = File.ReadAllText(s);
                dynamic json = JObject.Parse(readed);
                var k = json.xp;
                money.Add(k.ToString());
            }
            foreach (string q in money)
            {
                Int32.TryParse(q, out int result);
                sorted_money.Add(result);
            }
            sorted_money.Sort();
            foreach (int u in sorted_money)
            {
                new_money.Add(u.ToString());
            }
            int count = sorted_money.Count;
            for (int i = 1; i < 6; i++)
            {
                user_names.Add(ids[money.IndexOf(sorted_money[count - i].ToString())]);
            }
            foreach (string s in user_names)
            {
                foreach (var b in _client.Guilds.FirstOrDefault().Users)
                {
                    if (b.Id.ToString() == s)
                    {
                        int gg = ids.IndexOf(s);
                        new_names.Add(b.Username);
                    }
                }
            }
            EmbedBuilder builder = new EmbedBuilder();
            string d = "";
            int g = 0;
            for (int i = 1; i < 6; i++)
            {
                d += i + " - " + new_names[g] + " - " + sorted_money[count - i].ToString() + "\r\n";
                g++;
                if (g == 5) g = 0;
            }
            var a = await Context.Channel.SendMessageAsync("", false, builder
                .WithTitle("Топ 5")
                .WithDescription(d)
                .WithColor(Color.Blue)
                .Build());
        }
        static string correctAnswer = "";
        static int iter = 0;
        private async Task newanimal()
        {
            iter++;
            SocketGuildChannel channel = null;
            foreach (var s in _client.Guilds.FirstOrDefault().Channels)
            {
                if (s.Id.ToString() == channelId)
                {
                    channel = s;
                    break;
                }
            }
            Random r = new Random();
            int h = r.Next(1, 11);
            List<string> answers = new List<string>();
            List<string> paths = new List<string>();
            paths = Directory.GetFiles(Directory.GetCurrentDirectory() + "\\Animals\\").ToList();
            foreach (var j in paths)
            {
                answers.Add(Path.GetFileNameWithoutExtension(j));
            }
            string answ1 = "";
            string answ2 = "";
            string answ3 = "";
            string answ4 = "";
            string pathtocorrect = "";
            switch (iter)
            {
                case 1:
                    answ1 = answers[5]; //correct
                    answ2 = answers[3];
                    answ3 = answers[6];
                    answ4 = answers[11];
                    correctAnswer = "1";
                    pathtocorrect = paths[5];
                    break;
                case 2:
                    answ1 = answers[4];
                    answ2 = answers[3]; //correct
                    answ3 = answers[8];
                    answ4 = answers[9];
                    correctAnswer = "2";
                    pathtocorrect = paths[3];
                    break;

                case 3:
                    answ1 = answers[0]; //correct
                    answ2 = answers[2];
                    answ3 = answers[6];
                    answ4 = answers[7];
                    correctAnswer = "1";
                    pathtocorrect = paths[0];
                    break;

                case 4:
                    answ1 = answers[2];
                    answ2 = answers[11];
                    answ3 = answers[4];
                    answ4 = answers[8]; //correct
                    correctAnswer = "4";
                    pathtocorrect = paths[8];
                    break;

                case 5:
                    answ1 = answers[10]; 
                    answ2 = answers[7];
                    answ3 = answers[11]; //correct
                    answ4 = answers[2];
                    correctAnswer = "3";
                    pathtocorrect = paths[11];
                    break;

            }
            if (iter == 5) iter = 0;
            EmbedBuilder builder = new EmbedBuilder();
            await (channel as SocketTextChannel).SendFileAsync(pathtocorrect,
                "",
                embed: builder
                .WithTitle("Викторина - Угадай животное (выберите цифру)")
                .WithColor(Color.Purple)
                .WithDescription($"1. {answ1}\r\n2. {answ2}\r\n3. {answ3}\r\n4. {answ4}")
                .Build());
        }
        private async Task init()
        {
            newanimal();
        }

        public async Task RegisterCommandsAsync()
        {
            _client.MessageReceived += HandleCommandAsync;
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
        }
        private async Task HandleCommandAsync(SocketMessage arg)
        {
            var message = arg as SocketUserMessage;

            if (message is null || message.Author.IsBot) return;

            int argPos = 0;
            if (message.HasStringPrefix("!", ref argPos) || message.HasMentionPrefix(_client.CurrentUser, ref argPos))
            {
                var context = new SocketCommandContext(_client, message);

                var result = await _commands.ExecuteAsync(context, argPos, _services);
            }
        }
        private Task Log(LogMessage arg)
        {
            Console.WriteLine(arg);
            return Task.CompletedTask;
        }
    }
}
