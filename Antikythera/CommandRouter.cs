using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using Konata.Core;
using Konata.Core.Events.Model;
using Konata.Core.Interfaces.Api;
using Konata.Core.Message;
using Konata.Core.Message.Model;

using Serilog;

namespace Antikythera;

public class CommandRouter<T>
{
    private BotStatus BotStatus { get; set; } = new();

    private readonly T _instance;

    private ConcurrentDictionary<(CommandType, string), MethodInfo> Methods { get; set; } = new();

    public CommandRouter()
    {
        var type = typeof(T);
        _instance = Activator.CreateInstance<T>();
        var methods = type.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance)
            .Where(m => m.GetCustomAttributes().FirstOrDefault(attr => attr is CommandAttribute) is not null);
        foreach (var methodInfo in methods)
        {
            var attr = methodInfo.GetCustomAttributes<CommandAttribute>();
            foreach (var attribute in attr)
            {
                Methods.TryAdd((attribute.CommandType, attribute.Command), methodInfo);
            }
        }
    }

    public async Task Invoke(Bot bot, GroupMessageEvent evt)
    {
        BotStatus.GroupMessageReceived++;

        if (evt.MemberUin == bot.Uin)
        {
            return;
        }

        var textChain = evt.Chain.GetChain<TextChain>();

        if (textChain is null)
        {
            return;
        }

        MessageBuilder? messageBuilder = null;

        foreach (var (commandType, pattern) in Methods.Keys)
        {
            switch (commandType)
            {
                case CommandType.Common:
                    if (pattern == textChain.Content)
                    {
                        if (Methods.TryGetValue((commandType, pattern), out var method))
                        {
                            messageBuilder = (MessageBuilder?)method.Invoke(_instance, Array.Empty<object>());
                        }
                    }
                    break;
                case CommandType.Status:
                    if (pattern == textChain.Content)
                    {
                        if (Methods.TryGetValue((commandType, pattern), out var method))
                        {
                            messageBuilder = (MessageBuilder?)method.Invoke(_instance, new object?[] { BotStatus });
                        }
                    }
                    break;
                case CommandType.Regex:
                    throw new NotImplementedException();
                case CommandType.Full:
                    if (pattern == textChain.Content)
                    {
                        if (Methods.TryGetValue((commandType, pattern), out var method))
                        {
                            messageBuilder = (MessageBuilder?)method.Invoke(_instance, new object?[] { bot, evt });
                        }
                    }
                    break;
                default:
                    return;
            }

            if (messageBuilder is null)
            {
                continue;
            }

            try
            {
                await bot.SendGroupMessage(evt.GroupUin, messageBuilder);
                messageBuilder = null;
            }
            catch (Exception e)
            {
                Log.Logger.Error(e, e.Message);
                await bot.SendGroupMessage(evt.GroupUin, TextChain.Create(e.Message));
            }
        }
    }


}

public class CommandAttribute : Attribute
{
    public CommandType CommandType { get; set; }
    public string Command { get; set; }

    public CommandAttribute(CommandType commandType, string command)
    {
        CommandType = commandType;
        Command = command;
    }
}

public enum CommandType
{
    Common,
    Status,
    Regex,
    Full
}

public class BotStatus
{
    public uint GroupMessageReceived { get; set; }
}