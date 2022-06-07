using Antikythera.Utils;

using Konata.Core;
using Konata.Core.Events.Model;
using Konata.Core.Message;

using System;
using System.Diagnostics;

namespace Antikythera.Functions;

public class Commands
{
    [Command(CommandType.Common, "/help")]
    private MessageBuilder? OnCommandHelp()
        => new MessageBuilder()
            .Text("[Antikythera Help]\n")
            .Text("/ping\n Pong!\n\n")
            .Text("/help\n 打印本帮助消息。\n\n")
            .Text("/status\n 显示 Bot 状态。\n\n");

    [Command(CommandType.Status, "/status")]
    private MessageBuilder? OnCommandStatus(BotStatus stat)
        => new MessageBuilder()
            // Core descriptions
            .Text($"[Antikythera]\n")
            .Text($"[分支名:{BuildStamp.Branch}]\n")
            .Text($"[版本号:{BuildStamp.Version}]\n")
            .Text($"[提交哈希:{BuildStamp.CommitHash[..12]}]\n")
            .Text($"[构建时间:{BuildStamp.BuildTime}]\n\n")

            // System status
            .Text($"启动至今已处理 {stat.GroupMessageReceived} 条消息。\n")
            .Text($"回收器内存: {GC.GetTotalAllocatedBytes().Bytes2MiB(2)} MiB " +
                  $"({Math.Round((double)GC.GetTotalAllocatedBytes() / GC.GetTotalMemory(false) * 100, 2)}%)\n")
            .Text($"总内存: {Process.GetCurrentProcess().WorkingSet64.Bytes2MiB(2)} MiB\n\n")

            // Copyrights
            .Text("Powered by ProjectHDS, Konata.Core and Kagami Project.");


    [Command(CommandType.Common, "/ping")]
    private MessageBuilder? OnCommandPing() => new MessageBuilder().Text("Pong!");

    [Command(CommandType.Full, "/jrrp")]
    private MessageBuilder? OnCommandJrrp(Bot bot, GroupMessageEvent evt)
    {
        var date = new DateTimeOffset(DateTime.Now).LocalDateTime;
        var year = date.Year;
        var month = date.Month;
        var day = date.Day;
        year |= day;
        var seed = int.MaxValue / 2 - (month | year);
        var random = new Random(seed);
        var rp = random.Next(0, 100);
        return new MessageBuilder().Text($"{evt.MemberCard} 的今日人品为：{rp}");
    }
}
