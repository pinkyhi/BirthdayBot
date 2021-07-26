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
    public class GeopositionConfirmationMenu : IMenu
    {
        private readonly IStringLocalizer<SharedResources> resources;

        public GeopositionConfirmationMenu(IStringLocalizer<SharedResources> resources)
        {
            this.resources = resources;
        }

        public string GetDefaultTitle(IServiceScope actionScope = null, params string[] values)
        {
            return resources["GEOPOSITION_CONFIRMATION"];
        }

        public IReplyMarkup GetMarkup(IServiceScope actionScope = null)
        {
            InlineKeyboardButton geopositionConfirm = new InlineKeyboardButton() { CallbackData = CommandKeys.GeopositionConfirm, Text = resources["CONFIRM_BUTTON"]};
            InlineKeyboardButton geopositionReject = new InlineKeyboardButton() { CallbackData = CommandKeys.GeopositionReject, Text = resources["REJECT_BUTTON"] };

            InlineKeyboardMarkup result = new InlineKeyboardMarkup(new InlineKeyboardButton[][] {
                new[]
                {
                    geopositionConfirm,
                    geopositionReject
                }
            });
            return result;        
        }
    }
}
