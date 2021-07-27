using BirthdayBot.BLL.Resources;
using BirthdayBot.Core.Resources;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using RapidBots.Types.Menus;
using System;
using System.Collections.Generic;
using System.Text;
using Telegram.Bot.Types.ReplyMarkups;

namespace BirthdayBot.BLL.Menus
{
    public class BirthDateConfirmationMenu : IMenu
    {
        private readonly IStringLocalizer<SharedResources> resources;

        public BirthDateConfirmationMenu(IStringLocalizer<SharedResources> resources)
        {
            this.resources = resources;
        }

        public string GetDefaultTitle(IServiceScope actionScope = null, params string[] values)
        {
            return string.Format(resources["BIRTH_DATE_CONFIRMATION"], values);
        }

        public IReplyMarkup GetMarkup(IServiceScope actionScope = null)
        {
            InlineKeyboardButton birthDateConfirm = new InlineKeyboardButton() { CallbackData = CommandKeys.BirthDateConfirm, Text = resources["CONFIRM_BUTTON"]};
            InlineKeyboardButton birthDateReject = new InlineKeyboardButton() { CallbackData = CommandKeys.BirthDateReject, Text = resources["REJECT_BUTTON"] };

            InlineKeyboardMarkup result = new InlineKeyboardMarkup(new InlineKeyboardButton[][] {
                new[]
                {
                    birthDateConfirm,
                    birthDateReject
                }
            });
            return result;        
        }
    }
}
