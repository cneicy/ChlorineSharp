using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

using Konata.Core;
using Konata.Core.Events.Model;
using Konata.Core.Interfaces.Api;
using Konata.Core.Message;
using Konata.Core.Message.Model;

using Serilog;

namespace Antikythera;

public class TemplateReply
{
    private Dictionary<string, string[]>? Dictionary { get; set; }

    private void LoadCommands()
    {
        if (File.Exists("commands.json"))
        {
            var commandsJson = JsonSerializer.Deserialize<Dictionary<string, string[]>>(File.ReadAllText("commands.json"));
            commandsJson ??= new Dictionary<string, string[]>();
            Dictionary = commandsJson;
            return;
        }

        Dictionary = new();
    }

    public async Task<bool> GetReply(Bot bot, GroupMessageEvent evt)
    {
        if (Dictionary is null)
        {
            LoadCommands();
        }

        foreach (var key in Dictionary!.Keys)
        {
            if (evt.Chain.GetChain<TextChain>()?.Content.Equals("/" + key) is true)
            {
                var mb = new MessageBuilder();
                foreach (var s in Dictionary[key])
                {
                    mb.Add(TextChain.Create(s));
                }

                try
                {
                    await bot.SendGroupMessage(evt.GroupUin, mb);
                    return true;
                }
                catch (Exception e)
                {
                    Log.Logger.Error(e, e.Message);
                    await bot.SendGroupMessage(evt.GroupUin, TextChain.Create(e.Message));
                }
            }
        }
        return false;
    }
}