﻿using BirthdayBot.BLL.Menus.People;
using BirthdayBot.Core.Resources;
using BirthdayBot.DAL.Entities;
using BirthdayBot.DAL.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using RapidBots.Types.Core;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Types.Enums;
using BirthdayBot.Core.Const;

namespace BirthdayBot.BLL.Inputs.People.Personal
{

    public class AddByContactInput : Input
    {
        private readonly BotClient botClient;

        public AddByContactInput(BotClient botClient)
        {
            this.botClient = botClient;
        }

        public override int Status => 11;

        public override async Task Execute(Update update, TelegramUser user = null, IServiceScope actionScope = null)
        {
            //Initialisation
            var repository = actionScope.ServiceProvider.GetService<IRepository>();
            var actionsManager = actionScope.ServiceProvider.GetService<ActionManager>();
            var resources = actionScope.ServiceProvider.GetService<IStringLocalizer<SharedResources>>();
            TUser dbUser = user as TUser ?? await repository.GetAsync<TUser>(true, u => u.Id == update.Message.From.Id, include: u => u.Include(x => x.Subscriptions).ThenInclude(x => x.Target));
            if (dbUser.Subscriptions == null)
            {
                await repository.LoadCollectionAsync(dbUser, x => x.Subscriptions);
                await repository.LoadCollectionAsync(dbUser, x => x.Subscribers);
            }

            // Logic
            try
            {
                string inputStr = update.Message.Text?.Trim();
                
                if (inputStr?.Equals(resources["BACK_BUTTON"]) == true)
                {
                    var openerMessage = await botClient.SendTextMessageAsync(update.Message?.Chat?.Id ?? update.CallbackQuery.Message.Chat.Id, resources["MENU_OPENER_TEXT"], replyMarkup: new ReplyKeyboardRemove(), parseMode: Telegram.Bot.Types.Enums.ParseMode.Html);
                    await botClient.DeleteMessageAsync(openerMessage.Chat.Id, openerMessage.MessageId);

                    dbUser.CurrentStatus = null;
                    dbUser.MiddlewareData = null;
                    await repository.UpdateAsync(dbUser);

                    PeopleMenu menu = new PeopleMenu(resources);

                    await botClient.SendTextMessageAsync(update.Message.Chat.Id, menu.GetDefaultTitle(actionScope), replyMarkup: menu.GetMarkup(0, dbUser.Subscriptions, actionScope), parseMode: Telegram.Bot.Types.Enums.ParseMode.Html);
                    return;
                }
                if (update.Message.Contact == null)
                {
                    await botClient.SendTextMessageAsync(dbUser.Id, resources["NO_CONTACT_ERROR"], parseMode: ParseMode.Html);
                    return;
                }
                if (update.Message.Contact.UserId == 0)
                {
                    await botClient.SendTextMessageAsync(update.Message.Chat.Id, resources["EMPTY_CONTACT_ERROR"], parseMode: Telegram.Bot.Types.Enums.ParseMode.Html);
                    var openerMessage = await botClient.SendTextMessageAsync(update.Message?.Chat?.Id ?? update.CallbackQuery.Message.Chat.Id, resources["MENU_OPENER_TEXT"], replyMarkup: new ReplyKeyboardRemove(), parseMode: Telegram.Bot.Types.Enums.ParseMode.Html);
                    await botClient.DeleteMessageAsync(openerMessage.Chat.Id, openerMessage.MessageId);

                    dbUser.CurrentStatus = null;
                    dbUser.MiddlewareData = null;
                    await repository.UpdateAsync(dbUser);

                    AddPeopleMenu peopleMenu = new AddPeopleMenu(resources, "0");

                    await botClient.SendTextMessageAsync(update.Message.Chat.Id, peopleMenu.GetDefaultTitle(actionScope), replyMarkup: peopleMenu.GetMarkup(actionScope), parseMode: Telegram.Bot.Types.Enums.ParseMode.Html);
                    return;
                }

                var target = await repository.GetAsync<TUser>(true, x => x.Id == update.Message.Contact.UserId == true && x.Id != dbUser.Id && x.RegistrationDate != null);
                if (target == null)
                {
                    await botClient.SendTextMessageAsync(update.Message.Chat.Id, resources["CONTACT_NOT_FOUND"], parseMode: Telegram.Bot.Types.Enums.ParseMode.Html);
                    var openerMessage = await botClient.SendTextMessageAsync(update.Message?.Chat?.Id ?? update.CallbackQuery.Message.Chat.Id, resources["MENU_OPENER_TEXT"], parseMode: Telegram.Bot.Types.Enums.ParseMode.Html);
                    await botClient.DeleteMessageAsync(openerMessage.Chat.Id, openerMessage.MessageId);

                    PersonalNotFoundMenu nfMenu = new PersonalNotFoundMenu(resources, update.Message.Contact.FirstName ?? update.Message.Contact.PhoneNumber, dbUser.Id);
                    await botClient.SendTextMessageAsync(update.Message.Chat.Id, nfMenu.GetDefaultTitle(), replyMarkup: nfMenu.GetMarkup(actionScope), parseMode: Telegram.Bot.Types.Enums.ParseMode.Html);
                    return;
                }
                else
                {
                    if (dbUser.Subscriptions.FirstOrDefault(x => x.TargetId == target.Id) != null)
                    {
                        await botClient.SendTextMessageAsync(update.Message.Chat.Id, resources["ADD_PERSONAL_INPUT_DUPLICATE"], parseMode: Telegram.Bot.Types.Enums.ParseMode.Html);
                        var openerMessage = await botClient.SendTextMessageAsync(update.Message?.Chat?.Id ?? update.CallbackQuery.Message.Chat.Id, resources["MENU_OPENER_TEXT"], replyMarkup: new ReplyKeyboardRemove(), parseMode: Telegram.Bot.Types.Enums.ParseMode.Html);
                        await botClient.DeleteMessageAsync(openerMessage.Chat.Id, openerMessage.MessageId);

                        dbUser.CurrentStatus = null;
                        dbUser.MiddlewareData = null;
                        await repository.UpdateAsync(dbUser);

                        AddPeopleMenu peopleMenu = new AddPeopleMenu(resources, "0");

                        await botClient.SendTextMessageAsync(update.Message.Chat.Id, peopleMenu.GetDefaultTitle(actionScope), replyMarkup: peopleMenu.GetMarkup(actionScope), parseMode: Telegram.Bot.Types.Enums.ParseMode.Html);
                        return;
                    }
                    if (dbUser.Subscriptions.Count < Limitations.SubsLimit)
                    {
                        dbUser.Subscriptions.Add(new Subscription() { IsStrong = false, Subscriber = dbUser, Target = target });
                        dbUser.CurrentStatus = null;
                        await repository.UpdateAsync(dbUser);
                        await botClient.SendTextMessageAsync(update.Message.Chat.Id, resources["ADD_PERSONAL_INPUT_SUCCESS"], replyMarkup: new ReplyKeyboardRemove(), parseMode: Telegram.Bot.Types.Enums.ParseMode.Html);
                    }
                    else
                    {
                        dbUser.CurrentStatus = null;
                        await repository.UpdateAsync(dbUser);
                        await botClient.SendTextMessageAsync(update.Message.Chat.Id, resources["SUBSCRIPTIONS_LIMIT"], replyMarkup: new ReplyKeyboardRemove(), parseMode: Telegram.Bot.Types.Enums.ParseMode.Html);
                    }
                    PeopleMenu menu = new PeopleMenu(resources);

                    await botClient.SendTextMessageAsync(update.Message.Chat.Id, menu.GetDefaultTitle(actionScope), replyMarkup: menu.GetMarkup(0, dbUser.Subscriptions, actionScope), parseMode: Telegram.Bot.Types.Enums.ParseMode.Html);
                    return;

                }
            }
            catch
            {
                await botClient.SendTextMessageAsync(dbUser.Id, resources["NO_CONTACT_ERROR"], parseMode: ParseMode.Html);
                return;
            }
        }
    }
}