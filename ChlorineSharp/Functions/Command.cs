using Konata.Core;
using Konata.Core.Events.Model;
using Konata.Core.Message;

using System;
using System.Diagnostics;
using ChlorineSharp.Utils;

namespace ChlorineSharp.Functions;

public class Commands
{
    [Command(CommandType.Common, "/help")]
    private MessageBuilder? OnCommandHelp()
        => new MessageBuilder()
            .Text("[ChlorineSharp Help]\n\n")
            .Text("/info\n   显示Bot资料\n")
            .Text("/help\n   显示Bot帮助信息\n")
            .Text("/jrrp\n   今日人品\n")
            .Text("/ask\n   教你提问\n")
            .Text("/pastebin\n   PasteBin使用方法\n")
            .Text("/log\n   log相关\n")
            .Text("/rules\n   显示群规\n")
            .Text("/crtcmds\n   CraftTweaker 部分常用指令使用方法\n")
            .Text("/mtcmds\n   MineTweaker 部分常用指令使用方法\n")
            .Text("/whyvsc\n   为什么要使用 VisualStudio Code?\n")
            .Text("/links\n   实用链接\n")
            .Text("/status\n   显示 Bot 状态。");

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
        var qq = evt.MemberUin;
        var seed = int.MaxValue / 2 - (month | year) + (int)qq;
        var random = new Random(seed);
        var rp = random.Next(0, 100);
        return new MessageBuilder().Text($"{evt.MemberCard} 的今日人品为：{rp}");
    }
}
