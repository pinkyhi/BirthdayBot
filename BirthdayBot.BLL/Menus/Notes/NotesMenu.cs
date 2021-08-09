using BirthdayBot.BLL.Resources;
using BirthdayBot.Core.Resources;
using BirthdayBot.DAL.Entities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using RapidBots.Types.Menus;
using System;
using System.Collections.Generic;
using System.Text;
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
            var pageButtos = this.GetPage(page, source, x => string.Concat(x.Title, " ", x.Date.ToShortDateString()));
            var backBut = new InlineKeyboardButton() { CallbackData = CommandKeys.Start, Text = resources["BACK_BUTTON"] };

            result.AddRange(pageButtos);
            result.Add(new List<InlineKeyboardButton>() { backBut });
            return new InlineKeyboardMarkup(result);
            throw new Exception();
        }
    }
}
