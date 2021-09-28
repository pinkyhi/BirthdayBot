using BirthdayBot.BLL.Inputs.People.Personal;
using BirthdayBot.BLL.Resources;
using BirthdayBot.Core.Resources;
using BirthdayBot.DAL.Entities;
using BirthdayBot.DAL.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using RapidBots.Types.Core;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using RapidBots.Types.Attributes;
using Telegram.Bot.Types.Enums;

namespace BirthdayBot.BLL.Commands.People.Personal
{
    [ChatType(ChatType.Private)]
    public class AddByContact : Command
    {
        private readonly BotClient botClient;

        public AddByContact(BotClient botClient)
        {
            this.botClient = botClient;
        }

        public override string Key => CommandKeys.AddByContact;

        public override async Task Execute(Update update, TelegramUser user = null, IServiceScope actionScope = null)
        {
            var repository = actionScope.ServiceProvider.GetService<IRepository>();
            var actionsManager = actionScope.ServiceProvider.GetService<ActionManager>();
            var resources = actionScope.ServiceProvider.GetService<IStringLocalizer<SharedResources>>();

            TUser dbUser = user as TUser ?? await repository.GetAsync<TUser>(false, u => u.Id == update.CallbackQuery.From.Id);
            dbUser.CurrentStatus = actionsManager.FindInputStatusByType<AddByContactInput>();
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

            dbUser.CurrentStatus = actionsManager.FindInputStatusByType<AddByContactInput>();
            await repository.UpdateAsync(dbUser);
            await botClient.SendTextMessageAsync(update.Message?.Chat?.Id ?? update.CallbackQuery.Message.Chat.Id, resources["ADD_BY_CONTACT_INPUT"], replyMarkup: new ReplyKeyboardMarkup(backBut) { ResizeKeyboard = true }, parseMode: Telegram.Bot.Types.Enums.ParseMode.Html);
        }
    }
}
