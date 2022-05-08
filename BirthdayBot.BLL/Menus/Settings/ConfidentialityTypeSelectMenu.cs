using BirthdayBot.Core.Enums;
using BirthdayBot.Core.Resources;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using RapidBots.Types.Menus;
using System.Collections.Generic;
using Telegram.Bot.Types.ReplyMarkups;

namespace BirthdayBot.BLL.Menus.Settings
{
    public class ConfidentialityTypeSelectMenu : IMenu
    {
        private readonly IStringLocalizer<SharedResources> resources;
        private readonly ConfidentialType currentType;

        public ConfidentialityTypeSelectMenu(IStringLocalizer<SharedResources> resources, ConfidentialType currentType)
        {
            this.resources = resources;
            this.currentType = currentType;
        }

        public string GetDefaultTitle(IServiceScope actionScope = null, params string[] values)
        {
            return resources["CONFIDENTIALITY_TYPE_INPUT"];
        }

        public IReplyMarkup GetMarkup(IServiceScope actionScope = null)
        {
            List<KeyboardButton> types0row = new List<KeyboardButton>()
            {
                currentType == ConfidentialType.Public ? new KeyboardButton(resources["PUBLIC_CONFIDENTIALITY_TYPE"] + "✔") : new KeyboardButton(resources["PUBLIC_CONFIDENTIALITY_TYPE"])
            };
            List<KeyboardButton> types1row = new List<KeyboardButton>()
            {
                currentType == ConfidentialType.Private ? new KeyboardButton(resources["PRIVATE_CONFIDENTIALITY_TYPE"] + "✔") : new KeyboardButton(resources["PRIVATE_CONFIDENTIALITY_TYPE"])
            };
            List<KeyboardButton> types2row = new List<KeyboardButton>()
            {
                currentType == ConfidentialType.Mutual ? new KeyboardButton(resources["MUTUAL_CONFIDENTIALITY_TYPE"] + "✔") : new KeyboardButton(resources["MUTUAL_CONFIDENTIALITY_TYPE"])
            };
            return new ReplyKeyboardMarkup(new List<List<KeyboardButton>>() { types0row, types1row, types2row }) { ResizeKeyboard = true, OneTimeKeyboard = true };
        }
    }
}
