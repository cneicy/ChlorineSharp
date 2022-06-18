using System.Threading.Tasks;
using Konata.Core;
using Konata.Core.Events.Model;
using Konata.Core.Interfaces.Api;
using Konata.Core.Message;
using Konata.Core.Message.Model;

namespace ChlorineSharp.Functions;

public class Welcome
{
    public static async Task OnJoin(Bot bot, GroupMemberIncreaseEvent evt)
    {
        var at = AtChain.Create(evt.MemberUin);
        if (at is null)
        {
            await bot.SendGroupMessage(evt.GroupUin, new MessageBuilder().Text("Argument error."));
        }
        else
        {
            var memberInfo = bot.GetGroupMemberInfo(evt.GroupUin, at.AtUin, true);
            if (memberInfo is null) await bot.SendGroupMessage(evt.GroupUin, new MessageBuilder().Text("No such member."));
        }
        await bot.SendGroupMessage(evt.GroupUin,
            new MessageBuilder()
            .Add(at)
            .Text("\n欢迎加入Minecraft魔改交流群，进群请先阅读所有置顶公告。提问请携带尽可能多的相关信息。\n")
            .Text("Discord服务器：https://discord.gg/sB9PhGcutE/\n")
            .Text("CRT等魔改类模组错误还需附带脚本内容和输出LOG。\n")
            .Text("详见/ask/pastebin/log\n")
            .Text("-----------------\n")
            .Text("能解决你大部分疑惑的视频:\n")
            .Text("https://b23.tv/Qu6aAY\n")
            .Text("https://b23.tv/d2brHg\n")
            .Text("-----------------\n")
            .Text("本群会不定期清理长期不发言的人。\n")
            .Text("本群允许分享整合包(私人整合包自建整合包领域服个人服务器包)。\n")
            .Text("-----------------\n")
            .Text("群内分享的代码片段、音效、材质等资源，使用协议和最终解释权归[发布者]，[商业使用]请提前咨询以避免踩雷。\n")
            .Text("-----------------\n"));
    }
}