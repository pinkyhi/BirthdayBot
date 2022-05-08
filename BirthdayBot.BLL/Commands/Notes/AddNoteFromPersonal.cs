using BirthdayBot.BLL.Inputs.Notes;
using BirthdayBot.BLL.Resources;
using RapidBots.Types.Attributes;
using Telegram.Bot.Types.Enums;
using BirthdayBot.Core.Resources;
using BirthdayBot.DAL.Entities;
using BirthdayBot.DAL.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json;
using RapidBots.Types.Core;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using RapidBots.Extensions;
using BirthdayBot.Core.Const;
using Microsoft.EntityFrameworkCore;

namespace BirthdayBot.BLL.Commands.Notes
{
    [ChatType(ChatType.Private)]
    [ExpectedParams("title")]
    public class AddNoteFromPersonal : Command
    {
        private readonly BotClient botClient;

        public AddNoteFromPersonal(BotClient botClient)
        {
            this.botClient = botClient;
        }

        public override string Key => CommandKeys.AddNote;

        public override async Task Execute(Update update, TelegramUser user = null, IServiceScope actionScope = null)
        {
            string title = update.GetParams()["title"];

            var repository = actionScope.ServiceProvider.GetService<IRepository>();
            var actionsManager = actionScope.ServiceProvider.GetService<ActionManager>();
            var resources = actionScope.ServiceProvider.GetService<IStringLocalizer<SharedResources>>();

            TUser dbUser = (user as TUser) ?? await repository.GetAsync<TUser>(false, u => u.Id == update.CallbackQuery.From.Id, include: u => u.Include(x => x.Notes));

            if (dbUser?.Notes == null)
            {
                await repository.LoadCollectionAsync(dbUser, x => x.Notes);
            }
            if(dbUser.Notes.Count < Limitations.NotesLimit)
            {
                dbUser.CurrentStatus = actionsManager.FindInputStatusByType<NoteTitleInput>();
                var newNote = new Note() { Title = title };
                newNote.Title = title;

                await botClient.SendTextMessageAsync(update.Message?.Chat?.Id ?? update.CallbackQuery.Message.Chat.Id, resources["TRANSFERED_NOTE_CREATION", title], parseMode: Telegram.Bot.Types.Enums.ParseMode.Html);

                dbUser.MiddlewareData = JsonConvert.SerializeObject(newNote);
                await repository.UpdateAsync(dbUser);

                try { await botClient.AnswerCallbackQueryAsync(update.CallbackQuery.Id); } catch { }
                try
                {
                    await botClient.DeleteMessageAsync(update.CallbackQuery.Message.Chat.Id, update.CallbackQuery.Message.MessageId);
                }
                catch
                { }

                // Output

                KeyboardButton backBut = new KeyboardButton() { Text = resources["BACK_BUTTON"] };

                dbUser.CurrentStatus = actionsManager.FindInputStatusByType<NoteYearInput>();
                await repository.UpdateAsync(dbUser);
                await botClient.SendTextMessageAsync(update.Message?.Chat?.Id ?? update.CallbackQuery.Message.Chat.Id, resources["NOTE_YEAR_INPUT"], parseMode: Telegram.Bot.Types.Enums.ParseMode.Html);
            }
            else
            {
                await botClient.AnswerCallbackQueryAsync(update.CallbackQuery.Id, resources["NOTES_LIMIT"], true);
            }
        }
    }
}
