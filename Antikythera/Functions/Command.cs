using Antikythera.Utils;
using Konata.Core;
using Konata.Core.Events.Model;
using Konata.Core.Interfaces.Api;
using Konata.Core.Message;
using Konata.Core.Message.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;

namespace Antikythera.Functions;
public static class Command
{
    private static uint _messageCounter;

    /// <summary>
    /// On group message
    /// </summary>
    /// <param name="bot"></param>
    /// <param name="group"></param>
    internal static async void OnGroupMessage(Bot bot, GroupMessageEvent group)
    {
        // Increase
        ++_messageCounter;

        if (group.MemberUin == bot.Uin) return;

        var textChain = group.Chain.GetChain<TextChain>();
        if (textChain is null) return;
        var commands = LoadCommands();

        try
        {
            MessageBuilder? reply = null;
            StringBuilder replyText = new StringBuilder();

            // simple commands
            foreach (var command in commands)
            {
                if (textChain.Content.StartsWith("/" + command.Key))
                {
                    foreach (string value in command.Value)
                        replyText.Append(value);
                    reply = new MessageBuilder(replyText.ToString());
                }
            }
            // ping/help it
            if (textChain.Content.StartsWith("/ping"))
                reply = OnCommandPing();
            if (textChain.Content.StartsWith("/help"))
                reply = OnCommandHelp(textChain);

            // complex commands
            if (textChain.Content.StartsWith("/status"))
                reply = OnCommandStatus(textChain);

            // Send reply message
            if (reply is not null)
                await bot.SendGroupMessage(group.GroupUin, reply);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            Console.WriteLine(e.StackTrace);

            // Send error print
            await bot.SendGroupMessage(group.GroupUin,
                Text($"{e.Message}\n{e.StackTrace}"));
        }
    }

    /// <summary>On help.</summary>
    /// <param name="chain"></param>
    /// <returns></returns>
    public static MessageBuilder OnCommandHelp(TextChain chain)
        => new MessageBuilder()
            .Text("[Antikythera Help]\n")
            .Text("/ping\n Pong!\n\n")
            .Text("/help\n 打印本帮助消息。\n\n")
            .Text("/status\n 显示 Bot 状态。\n\n");

    /// <summary>On status.</summary>
    /// <param name="chain"></param>
    /// <returns></returns>
    public static MessageBuilder OnCommandStatus(TextChain chain)
        => new MessageBuilder()
            // Core descriptions
            .Text($"[Antikythera]\n")
            .Text($"[分支名:{BuildStamp.Branch}]\n")
            .Text($"[版本号:{BuildStamp.Version}]\n")
            .Text($"[提交哈希:{BuildStamp.CommitHash[..12]}]\n")
            .Text($"[构建时间:{BuildStamp.BuildTime}]\n\n")

            // System status
            .Text($"启动至今已处理 {_messageCounter} 条消息。\n")
            .Text($"回收器内存: {GC.GetTotalAllocatedBytes().Bytes2MiB(2)} MiB " +
                  $"({Math.Round((double)GC.GetTotalAllocatedBytes() / GC.GetTotalMemory(false) * 100, 2)}%)\n")
            .Text($"总内存: {Process.GetCurrentProcess().WorkingSet64.Bytes2MiB(2)} MiB\n\n")

            // Copyrights
            .Text("Powered by ProjectHDS, Konata.Core and Kagami Project.");

    /// <summary>On ping.</summary>
    /// <returns></returns>
    public static MessageBuilder OnCommandPing() => Text("Pong!");

    private static MessageBuilder Text(string text)
        => new MessageBuilder().Text(text);

    private static Dictionary<string, string[]> LoadCommands()
    {
        var commands = new Dictionary<string, string[]>();
        var path = Path.Join("commands.json");
        if (File.Exists(path))
        {
            var commandsJson = JsonSerializer.Deserialize<Dictionary<string, string[]>>(File.ReadAllText(path));
            if (commandsJson is not null) return commandsJson;
        }
        return commands;
    }
}
