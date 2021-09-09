using BirthdayBot.BLL.Resources;
using BirthdayBot.Core.Resources;
using BirthdayBot.DAL.Entities;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using RapidBots.Constants;
using RapidBots.Types.Menus;
using System.Collections.Generic;
using Telegram.Bot.Types.ReplyMarkups;

namespace BirthdayBot.BLL.Menus.Notes
{
    public class NotesMenu : PaginationMenu
    {
        private readonly IStringLocalizer<SharedResources> resources;

        public NotesMenu(IStringLocalizer<SharedResources> resources) : base(8, 1, CommandKeys.Notes)
        {
            this.resources = resources;
        }

        public override string GetDefaultTitle(IServiceScope actionScope = null, params string[] values)
        {
            return resources["NOTES_TEXT", values];
        }

        public IReplyMarkup GetMarkup(int page, List<Note> source, IServiceScope actionScope = null)
        {
            var addButton = new InlineKeyboardButton() { Text = resources["ADD_BUTTON"], CallbackData = CommandKeys.AddNote };
            var result = new List<List<InlineKeyboardButton>>() { new List<InlineKeyboardButton>() { addButton } };
            var pageButtons = this.GetPage(page, source, x => 
            {
                var qParams = new Dictionary<string, string>();
                qParams.Add("property", $"{x.Id}");
                qParams.Add(CallbackParams.Page, page.ToString());
                return new InlineKeyboardButton() { Text = string.Concat(x.Title, " ", x.Date.ToShortDateString()), CallbackData = QueryHelpers.AddQueryString(CommandKeys.OpenNote, qParams) };
            });

            var backBut = new InlineKeyboardButton() { CallbackData = CommandKeys.Start, Text = resources["BACK_BUTTON"] };

            result.AddRange(pageButtons);
            result.Add(new List<InlineKeyboardButton>() { backBut });
            return new InlineKeyboardMarkup(result);
        }
    }
}
