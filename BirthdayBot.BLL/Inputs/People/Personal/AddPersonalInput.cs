using BirthdayBot.BLL.Inputs.Start;
using BirthdayBot.BLL.Menus.Notes;
using BirthdayBot.BLL.Menus.People;
using BirthdayBot.BLL.Menus.Settings;
using BirthdayBot.Core.Enums;
using BirthdayBot.Core.Resources;
using BirthdayBot.DAL.Entities;
using BirthdayBot.DAL.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json;
using RapidBots.Types.Core;
using System;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace BirthdayBot.BLL.Inputs.People.Personal
{
    public class AddPersonalInput : IInput
    {
        private readonly BotClient botClient;

        public AddPersonalInput(BotClient botClient)
        {
            this.botClient = botClient;
        }

        public int Status => 10;

        public async Task Execute(Update update, TelegramUser user = null, IServiceScope actionScope = null)
        {
            //Initialisation
            var repository = actionScope.ServiceProvider.GetService<IRepository>();
            var actionsManager = actionScope.ServiceProvider.GetService<ActionManager>();
            var resources = actionScope.ServiceProvider.GetService<IStringLocalizer<SharedResources>>();
            TUser dbUser = user as TUser ?? await repository.GetAsync<TUser>(true, u => u.Id == update.Message.From.Id);
            if(dbUser.Subscriptions == null)
            {
                await repository.LoadCollectionAsync(dbUser, x => x.Subscriptions); 
            }

            // Logic
            try
            {
                string inputStr = update.Message.Text.Trim();
                if (inputStr.Equals(resources["BACK_BUTTON"]))
                {
                    var openerMessage = await botClient.SendTextMessageAsync(update.Message?.Chat?.Id ?? update.CallbackQuery.Message.Chat.Id, resources["MENU_OPENER_TEXT"], replyMarkup: new ReplyKeyboardRemove());
                    await botClient.DeleteMessageAsync(openerMessage.Chat.Id, openerMessage.MessageId);

                    dbUser.CurrentStatus = null;
                    dbUser.MiddlewareData = null;
                    await repository.UpdateAsync(dbUser);

                    PeopleMenu menu = new PeopleMenu(resources);

                    await botClient.SendTextMessageAsync(update.Message.Chat.Id, menu.GetDefaultTitle(actionScope), replyMarkup: menu.GetMarkup(0, dbUser.Subscriptions, actionScope));
                    return;
                }

                if (inputStr.Length > 65)
                {
                    throw new ArgumentException();
                }
                if (inputStr.StartsWith('@'))
                {
                    inputStr = inputStr.Substring(1);
                }

                var targets = await repository.GetRangeAsync<TUser>(true, x => x.Username.Contains(inputStr) && x.Id != dbUser.Id && x.RegistrationDate != null);
                if(targets.Count() < 1)
                {
                    await botClient.SendTextMessageAsync(update.Message.Chat.Id, resources["ADD_PERSONAL_INPUT_ERROR"]);
                    return;
                }
                else
                {
                    var target = targets.FirstOrDefault(x => x.Username.Equals(inputStr));
                    if (target != null)
                    {
                        if(dbUser.Subscriptions.FirstOrDefault(x => x.TargetId == target.Id) != null)
                        {
                            await botClient.SendTextMessageAsync(update.Message.Chat.Id, resources["ADD_PERSONAL_INPUT_DUPLICATE"]);
                            return;
                        }
                        dbUser.Subscriptions.Add(new Subscription() { IsStrong = false, Subscriber = dbUser, Target = target });
                        await repository.UpdateAsync(dbUser);
                        await botClient.SendTextMessageAsync(update.Message.Chat.Id, resources["ADD_PERSONAL_INPUT_SUCCESS"], replyMarkup: new ReplyKeyboardRemove());

                        PeopleMenu menu = new PeopleMenu(resources);

                        await botClient.SendTextMessageAsync(update.Message.Chat.Id, menu.GetDefaultTitle(actionScope), replyMarkup: menu.GetMarkup(0, dbUser.Subscriptions, actionScope));
                        return;
                    }
                    else
                    {
                        await botClient.SendTextMessageAsync(update.Message.Chat.Id, resources["ADD_PERSONAL_INPUT_MULTIPLE", string.Join(", ", targets.Select(x => x.Username).Take(10))]);
                        return;
                    }
                }
            }
            catch
            {
                await botClient.SendTextMessageAsync(update.Message.Chat.Id, resources["ADD_PESONAL_INPUT_ERROR"], parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown);
                return;
            }
        }
    }
}
