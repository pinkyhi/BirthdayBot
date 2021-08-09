using AutoMapper;
using BirthdayBot.BLL.Inputs.Start;
using BirthdayBot.BLL.Menus.Notes;
using BirthdayBot.BLL.Menus.Settings;
using BirthdayBot.BLL.Resources;
using BirthdayBot.Core.Resources;
using BirthdayBot.Core.Types;
using BirthdayBot.DAL.Entities;
using BirthdayBot.DAL.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json;
using RapidBots.Types.Core;
using System;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace BirthdayBot.BLL.Commands.Notes
{
    public class NoteConfirm : ICommand
    {
        private readonly BotClient botClient;
        private readonly ClientSettings clientSettings;

        public NoteConfirm(BotClient botClient, ClientSettings clientSetting)
        {
            this.clientSettings = clientSetting;
            this.botClient = botClient;
        }

        public string Key => CommandKeys.NoteDateConfirm;

        public async Task Execute(Update update, TelegramUser user = null, IServiceScope actionScope = null)
        {
            var repository = actionScope.ServiceProvider.GetService<IRepository>();
            var actionsManager = actionScope.ServiceProvider.GetService<ActionManager>();
            var resources = actionScope.ServiceProvider.GetService<IStringLocalizer<SharedResources>>();

            TUser dbUser = (user as TUser) ?? await repository.GetAsync<TUser>(false, u => u.Id == update.CallbackQuery.From.Id, include: u => u.Include(x => x.Notes));

            if (dbUser?.Notes == null)
            {
                var tempUser = await repository.GetAsync<TUser>(false, u => u.Id == update.CallbackQuery.From.Id, include: u => u.Include(x => x.Notes));
                dbUser.Notes = tempUser.Notes;
            }

            dbUser.Notes.Add(JsonConvert.DeserializeObject<Note>(dbUser.MiddlewareData));
            dbUser.MiddlewareData = null;
            dbUser.CurrentStatus = null;
            await repository.UpdateAsync(dbUser);

            await botClient.AnswerCallbackQueryAsync(update.CallbackQuery.Id);
            try
            {
                await botClient.DeleteMessageAsync(update.CallbackQuery.Message.Chat.Id, update.CallbackQuery.Message.MessageId);
            }
            catch
            { }
            var openerMessage = await botClient.SendTextMessageAsync(update.Message?.Chat?.Id ?? update.CallbackQuery.Message.Chat.Id, resources["MENU_OPENER_TEXT"], replyMarkup: new ReplyKeyboardRemove());
            await botClient.DeleteMessageAsync(openerMessage.Chat.Id, openerMessage.MessageId);

            dbUser.CurrentStatus = null;
            dbUser.MiddlewareData = null;
            await repository.UpdateAsync(dbUser);

            if (dbUser?.Notes == null)
            {
                var tempDbUser = await repository.GetAsync<TUser>(false, u => u.Id == update.Message.From.Id, include: u => u.Include(x => x.Notes));
                dbUser.Notes = tempDbUser.Notes;
            }

            NotesMenu notesMenu = new NotesMenu(resources);
            await botClient.SendTextMessageAsync(update.CallbackQuery.Message.Chat.Id, notesMenu.GetDefaultTitle(actionScope), replyMarkup: notesMenu.GetMarkup(0, dbUser.Notes, actionScope));

            return;
        }
    }
}
