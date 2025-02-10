﻿namespace DiscordBotV3;

using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Lavalink4NET;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

[RequireContext(ContextType.Guild)]
public sealed class CommandsModule : InteractionModuleBase<SocketInteractionContext>
{
    private readonly DiscordSocketClient _client;

    public Random random = new Random();

    public CommandsModule( DiscordSocketClient client)
    {
        ArgumentNullException.ThrowIfNull(client);
        _client = client;

        _client.ButtonExecuted += _client_ButtonExecuted;
    }

    public async Task _client_ButtonExecuted(SocketMessageComponent arg)
    {

        switch (arg.Data.CustomId)
        {
            case "addpidoras":
                List<string> listOfpidorasi = new SqliteHelper().GetPidorasi().ConvertAll(x => x.ToLower());
                SqliteHelper sqliteHelper = new SqliteHelper();

                if (listOfpidorasi.FirstOrDefault(x => x.Contains(arg.User.Mention.ToLower())) != null)
                {
                    await arg.RespondAsync($"В списке пидорасов ты уже есть", ephemeral: true);
                    break;
                }

                sqliteHelper.AddPidorasi(arg.User.Mention);
                
                await arg.UpdateAsync(x =>
                {
                    x.Content = $"{arg.User.Mention} добавился в список пидорасов, поздравим его";
                });
                break;
            default:
                break;
        }
    }

    public List<string> coolRoles = new List<string>()
    {
        "Крысюк",
        "Крутышки"
    };

    [SlashCommand("fantic","крыса")]
    public async Task Fantic()
    {
        await DeferAsync(false).ConfigureAwait(false);

        await FollowupAsync("https://tenor.com/view/rat-spin-gif-10300642414513246571\r\n");
    }


    [SlashCommand("roll", "ролим, даём макс значения")]    
    public async Task Roll(
        [Summary("Max","Максимальное значение ролла")]
        int Max,
        [Summary("Min","Минимальное значение ролла")]
        int? Min = null,
        [Summary("Reason","На что ролим?")]
        string? reason = null)
    {
        await DeferAsync(false).ConfigureAwait(false);

        Random rnd = new Random();
        if (Min > Max)
        {
            await FollowupAsync("🤓 минимум не может быть больше максимума");
        }


        if (Min != null)
        {
            await FollowupAsync($"{reason}\nРолим между {Min} и {Max}\nВыпало: {rnd.Next((int)Min,Max)}");
        }
        else if (Max < 0)
        {
            await FollowupAsync($"{reason}\nРолим между {Max} и 0\nВыпало: {rnd.Next(Max,0)}");
        }
        else
        {
            await FollowupAsync($"{reason}\nРолим между 0 и {Max}\nВыпало: {rnd.Next(Max)}");
        }
        
    }

    [SlashCommand("pidorasi", "список пидорасов")]
    public async Task Hate(
        [Summary("name","Добавляет имя в список")]
        string? name = null
        )
    {        
        
        await DeferAsync(false).ConfigureAwait(false);
       
        
        SqliteHelper sqliteHelper = new SqliteHelper();

        List<string> listOfpidorasi = new SqliteHelper().GetPidorasi();

        if (name == null)
        {
            var seppuku = new ComponentBuilder()
                    .WithButton("Добавить себя", "addpidoras");

            var messsageComp = seppuku.Build();

            string pidorasi = "";
            for (int i = 0; i < listOfpidorasi.Count; i++)
            {
                pidorasi += i.ToString() + ". " + listOfpidorasi[i] + "\n";
            }
            await FollowupAsync($"Список пидорасов:\n" +
                pidorasi +
                "Список активно дополняется, ВЫ можете дополнить его",allowedMentions:AllowedMentions.None,components:messsageComp);
        }
        else
        {
            List<string> listPidorasiLower = listOfpidorasi.ConvertAll(x => x.ToLower());
            
            var user = Context.User as SocketGuildUser;
            if (user == null)
            {
                return;
            }

            // Проверка на роли у пользователя, который вызвал комманду
            if (user.Roles.Any(r => r.Name == "Крысюк" || r.Name == "Крутышки"))
            {
                // Проверка на дубликат
                if (listPidorasiLower.FirstOrDefault(x => x.Contains(name.ToLower())) != null)
                {
                    await FollowupAsync($"В списке пидорасов, {name} уже присутствует <:ratgelupa:1276532532896202852>");
                    return;
                }

                sqliteHelper.AddPidorasi(name);
                await FollowupAsync($"Список пидорасов пополнился {name}");
            }else
            {
                var seppuku = new ComponentBuilder()
                    .WithButton("Добавить себя", "addpidoras");
                
                var messsageComp = seppuku.Build();

                await FollowupAsync("Нет прав <:gagaga:1276389397927039038>, хотите добавить себя? <:ratgelupa:1276532532896202852>", components:messsageComp);
            }
        }

    }    


    [SlashCommand("meme", "Смотрим мемы")]
    public async Task Meme(
        [Summary("tag","Смотрим рандомный мем по определённому тегу")]
        string tag       
        )
    {
        await DeferAsync(false).ConfigureAwait(false);
        SqliteHelper sqliteHelper = new SqliteHelper();

        string link = sqliteHelper.GetMemes(tag);
       
        if (link == "")
        {
            await FollowupAsync("Нет такого тега, посмотри теги по gettags");
        }
        
        await FollowupAsync(link);
    }

    [SlashCommand("addmeme", "Добавляем мемы")]
    [RequireOwner(Group = "CoolDudes")]
    [RequireRole("Крутышки", Group = "CoolDudes")]
    [RequireRole("Крысюк", Group = "CoolDudes")]
    public async Task Meme(
        [Summary("tag","Добавляем мем с определённым тегом")]
        string tag,
        [Summary("link","Добавляем ссылку на мем в список, tag - куда добавится мем")]
        string link
        )
    {
        await DeferAsync(false).ConfigureAwait(false);
        SqliteHelper sqliteHelper = new SqliteHelper();

        if (sqliteHelper.AddMeme(tag, link))
        {
            await FollowupAsync("Мем успешно добавлен");
        }
        else
        {
            await FollowupAsync("Произошла ошибка");
        }
    }


    [SlashCommand("gettags", "Получить список тегов для мемов")]
    public async Task GetTags()
    {
        await DeferAsync(false).ConfigureAwait(false);
        SqliteHelper sqliteHelper = new SqliteHelper();

        string tags = "";
        List<string> listOfTags = new SqliteHelper().GetTags();
        List<string> listOfTagsFiltered = listOfTags.Distinct().ToList();
        foreach (string pid in listOfTagsFiltered)
        {
            tags += pid + "\n";
        }

        await FollowupAsync($"Список тегов в мемах:\n" + tags);


    }

    [SlashCommand("talk", "silence, Miha is talking")]
    [RequireOwner]
    public async Task Talk(
        [Summary("text","silence, Miha is talking")]
        string text)
    {
        await DeferAsync(false).ConfigureAwait(false);

        await FollowupAsync(text, allowedMentions: AllowedMentions.All).ConfigureAwait(false);
        
    }

    [SlashCommand("hanadiscus", "Тебе сколько лет?")]
    public async Task hanadiscus()
    {
        await DeferAsync(false).ConfigureAwait(false);

        await FollowupAsync("Тебе сколько лет вообще?").ConfigureAwait(false);
    }

    [SlashCommand("emote", "Смотрим что за эмоут тут у нас")]
    public async Task GetEmoteData(
        [Summary("Emote","Даём эмоут на вход")]
        string text)
    {
        await DeferAsync(false).ConfigureAwait(false);

        var emote = Emote.Parse(text);

        EmbedBuilder builder = new EmbedBuilder();

        builder.WithTitle("Emote");
        builder.ThumbnailUrl = emote.Url;
        builder.AddField("Имя",$"{emote.Name}");
        builder.AddField("ID", $"{emote.Id}");
        builder.AddField("Анимированный", $"{emote.Animated}");
        builder.AddField("Ссылка", $"{emote.Url}");

        Embed embed = builder.Build();

        await FollowupAsync(embed: embed).ConfigureAwait(false);
    }


    [SlashCommand("test", "тестим на проде")]
    [RequireOwner]
    public async Task Test()
    {
        await DeferAsync(true).ConfigureAwait(false);

        HttpHelper httpHelper = new HttpHelper();

        string? anek = httpHelper.Anek();

        if (anek != null)
        {
            await FollowupAsync(anek).ConfigureAwait(false);
        }
        await FollowupAsync("ERROR", ephemeral:true).ConfigureAwait(false);

    }

    [SlashCommand("deletetag", "Удаляем мемы с определённым тегом")]
    [RequireOwner(Group = "CoolDudes")]
    [RequireRole("Крысюк", Group = "CoolDudes")]
    public async Task DelTag(
        [Summary("Tag","Тег который нужно удалить полностью")]
        string tag
        )
    {
        await DeferAsync(false).ConfigureAwait(false);

        SqliteHelper sqliteHelper = new SqliteHelper();

        if (sqliteHelper.DelTag(tag))
        {
            await FollowupAsync($"Мемы с тегом {tag} удалены");
        }
        else
        {
            await FollowupAsync("Такого тега и так нет");
        }
    }

    [SlashCommand("deletememe", "Удаляем мем с определённой ссылкой")]
    [RequireOwner(Group = "CoolDudes")]
    [RequireRole("Крысюк", Group = "CoolDudes")]
    public async Task DelMeme(
        [Summary("Link","Ссылка на мем, который нужно удалить")]
        string link
        )
    {
        await DeferAsync(false).ConfigureAwait(false);

        SqliteHelper sqliteHelper = new SqliteHelper();

        if (sqliteHelper.DelMeme(link))
        {
            await FollowupAsync($"Мем {link} удалён");
        }
        else
        {
            await FollowupAsync("Такой ссылки нет");
        }
    }

    

    /*
    [SlashCommand("online", "Смотрим онлайн")]
    public async Task Online()       
    {
        await DeferAsync(false).ConfigureAwait(false);

        ByondHelper byondHelper = new ByondHelper();

        if (byondHelper.IsServerOnline())
        {
            await FollowupAsync(byondHelper.QueryServerStatus());
        }
        else
        {
            await FollowupAsync("Сервер мёртв ");
        }
    }*/

    public bool CheckPrivelege(string roleName)
    {
        var match = coolRoles
            .FirstOrDefault(coolRoles => coolRoles.Contains(roleName));
        if (match != null)
        {
            return true;
        }
        return false;
    }
    
}
