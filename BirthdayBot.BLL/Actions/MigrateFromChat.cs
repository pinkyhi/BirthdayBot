using AutoMapper;
using BirthdayBot.Core.Const;
using BirthdayBot.Core.Resources;
using BirthdayBot.DAL.Entities;
using BirthdayBot.DAL.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using RapidBots.Types.Core;
using System;
using System.Globalization;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace BirthdayBot.BLL.Actions
{
    public class MigrateFromChat : RapidBots.Types.Core.Action
    {
        private readonly BotClient botClient;

        public MigrateFromChat(BotClient botClient)
        {
            this.botClient = botClient;
        }

        public async override Task Execute(Update update, TelegramUser user = null, IServiceScope actionScope = null)
        {
            var resources = actionScope.ServiceProvider.GetService<IStringLocalizer<SharedResources>>();

            lock (Lockers.ChatMigrateLocker)
            {
                var repository = actionScope.ServiceProvider.GetService<IRepository>();
                var mapper = actionScope.ServiceProvider.GetService<IMapper>();

                string telegramUserLanguageCode = update.MyChatMember?.From?.LanguageCode;

                if (!string.IsNullOrEmpty(telegramUserLanguageCode))
                {
                    CultureInfo.CurrentCulture = new CultureInfo(telegramUserLanguageCode);
                    CultureInfo.CurrentUICulture = new CultureInfo(telegramUserLanguageCode);
                }

                long migrateFromChatId = update.Message.Chat.Id;

                var migrateFromChat = repository.Get<DAL.Entities.Chat>(true, x => x.Id == migrateFromChatId, x => x.Include(x => x.ChatMembers));
                var migrateToChat = repository.Get<DAL.Entities.Chat>(true, x => x.Id == update.Message.MigrateToChatId, x => x.Include(x => x.ChatMembers));
                if (migrateFromChat != null && migrateToChat != null)
                {
                    repository.Delete(migrateToChat);
                    repository.Delete(migrateFromChat);
                    migrateFromChat.Id = update.Message.MigrateToChatId;
                    migrateFromChat.Type = migrateToChat.Type;
                    migrateFromChat.Title = migrateToChat.Title;
                    migrateFromChat.ChatMembers.ForEach(x => x.ChatId = update.Message.MigrateToChatId);
                    repository.Add(migrateFromChat);
                }
                else if (migrateFromChat != null && migrateToChat == null)
                {
                    repository.Delete(migrateFromChat);
                    migrateFromChat.Id = update.Message.MigrateToChatId;

                    var newChat = botClient.GetChatAsync(update.Message.MigrateToChatId).Result;
                    migrateFromChat.Type = newChat.Type;
                    migrateFromChat.Title = newChat.Title;
                    migrateFromChat.ChatMembers.ForEach(x => x.ChatId = update.Message.MigrateToChatId);
                    repository.Add(migrateFromChat);
                }
            }
            await botClient.SendTextMessageAsync(update.Message.MigrateToChatId, resources["MIGRATED_TO_CHAT"], parseMode: Telegram.Bot.Types.Enums.ParseMode.Html);
        }

        public override bool ValidateUpdate(Update update)
        {
            if (update.Message?.MigrateToChatId != null && update.Message.MigrateToChatId != 0)
            {
                return true;
            }
            return false;
        }
    }
}
