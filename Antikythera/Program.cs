using Antikythera.Functions;

using Konata.Core;
using Konata.Core.Common;
using Konata.Core.Events.Model;
using Konata.Core.Interfaces;
using Konata.Core.Interfaces.Api;

using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Serilog;

namespace Antikythera;

public static class Antikythera
{
    private static Bot _bot = null!;

    public static async Task Main()
    {
        Log.Logger = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .MinimumLevel.Information()
            .WriteTo.Console()
            .WriteTo.File("log.log", rollingInterval: RollingInterval.Day, retainedFileCountLimit: null, flushToDiskInterval: new TimeSpan(0, 0, 5)).CreateLogger();
        var commandRouter = new CommandRouter<Commands>();
        var templateReply = new TemplateReply();

        _bot = BotFather.Create(GetConfig(), GetDevice(), GetKeyStore());
        {
            _bot.OnLog += (s, e) => Log.Logger.Debug(e.EventMessage);
            _bot.OnGroupMessage += (s, e) => Log.Logger.Information("[{0}({1})] {2}({3}) -> {4}({5}): {6}", s.Name, s.Uin, e.MemberCard, e.MemberUin, e.GroupName, e.GroupUin, e.Chain.ToString().Replace("\n", "\\n").Replace("\r", "\\r"));
            _bot.OnFriendMessage += (s, e) => Log.Logger.Information("[{0}({1})] ({2}) -> {3}({4}): {5}", s.Name, s.Uin, e.Message.Sender.Uin, s.Name, s.Uin, e.Chain.ToString().Replace("\n", "\\n").Replace("\r", "\\r"));
            _bot.OnBotOnline += (s, e) => Log.Logger.Information("[STATUS][{0}({1})] connected.", s.Name, s.Uin);
            _bot.OnBotOffline += (s, e) => Log.Logger.Information("[STATUS][{0}({1})] disconnected.", s.Name, s.Uin);
            _bot.OnGroupMessageRecall += (s, e) => Log.Logger.Information("[RECALL][({0})] [({1})] recall a message of [({2})]", e.GroupUin, e.OperatorUin, e.AffectedUin);
            _bot.OnFriendMessage += (s, e) => Log.Logger.Information("[RECALL][({0})] message [({1})] was recalled.", e.FriendUin, e.Chain.ToString().Replace("\n", "\\n").Replace("\r", "\\r"));
            //_bot.OnLog += (s, e) => Console.WriteLine(e.EventMessage);
            _bot.OnCaptcha += (s, e) =>
            {
                switch (e.Type)
                {
                    case CaptchaEvent.CaptchaType.Sms:
                        Console.WriteLine(e.Phone);
                        s.SubmitSmsCode(Console.ReadLine());
                        break;

                    case CaptchaEvent.CaptchaType.Slider:
                        Console.WriteLine(e.SliderUrl);
                        s.SubmitSliderTicket(Console.ReadLine());
                        break;

                    default:
                    case CaptchaEvent.CaptchaType.Unknown:
                        break;
                }
            };

            _bot.OnGroupMessage += async (s, e) =>
            {
                var reply = await templateReply.GetReply(s, e);
                if (reply)
                {
                    return;
                }
                await commandRouter.Invoke(s, e);
            };
        }

        var result = await _bot.Login();
        {
            if (result) UpdateKeyStore(_bot.KeyStore);
        }

        while (true)
        {
            switch (Console.ReadLine())
            {
                case "/stop":
                    await _bot.Logout();
                    _bot.Dispose();
                    return;
            }
        }
    }

    /// <summary>Get bot config.</summary>
    /// <returns></returns>
    private static BotConfig GetConfig()
    {
        return new BotConfig
        {
            EnableAudio = true,
            TryReconnect = true,
            HighwayChunkSize = 8192
        };
    }

    /// <summary>Load or create device.</summary>
    /// <returns></returns>
    private static BotDevice? GetDevice()
    {
        // Read device info from config file.
        if (File.Exists("device.json"))
        {
            return JsonSerializer.Deserialize<BotDevice>(File.ReadAllText("device.json"));
        }

        var device = BotDevice.Default();
        {
            var deviceJson = JsonSerializer.Serialize(device,
                new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText("device.json", deviceJson);
        }
        return device;
    }

    /// <summary>Load or create keystore.</summary>
    /// <returns></returns>
    private static BotKeyStore? GetKeyStore()
    {
        // Read keystore info from config file.
        if (File.Exists("keystore.json"))
        {
            return JsonSerializer.Deserialize<BotKeyStore>(File.ReadAllText("keystore.json"));
        }

        Console.WriteLine("Could not find keystore.json, " +
                          "please input your account and password:");

        // Create and return the bot keystore.
        Console.Write("Account: ");
        var account = Console.ReadLine();

        Console.Write("Password: ");
        var password = Console.ReadLine();

        Console.WriteLine("Bot created.");
        return UpdateKeyStore(new BotKeyStore(account, password));
    }

    /// <summary>Update keystore config file, self-return.</summary>
    /// <param name="keystore"></param>
    /// <returns></returns>
    private static BotKeyStore UpdateKeyStore(BotKeyStore keystore)
    {
        var deviceJson = JsonSerializer.Serialize(keystore,
            new JsonSerializerOptions { WriteIndented = true });

        File.WriteAllText("keystore.json", deviceJson);
        return keystore;
    }
}
