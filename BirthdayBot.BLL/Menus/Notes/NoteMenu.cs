using BirthdayBot.BLL.Resources;
using BirthdayBot.Core.Resources;
using BirthdayBot.DAL.Entities;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using RapidBots.Constants;
using RapidBots.Types.Menus;
using Telegram.Bot.Types.ReplyMarkups;

namespace BirthdayBot.BLL.Menus.Notes
{
    public class NoteMenu : IMenu
    {
        private readonly IStringLocalizer<SharedResources> resources;
        private readonly int fromPage;
        private readonly Note note;

        public NoteMenu(IStringLocalizer<SharedResources> resources, int fromPage, Note note)
        {
            this.resources = resources;
            this.fromPage = fromPage;
            this.note = note;
        }

        public string GetDefaultTitle(IServiceScope actionScope = null, params string[] values)
        {
            return resources["NOTE_TEXT", note.Title, note.Date, note.IsStrong ? resources["STRONG_NOTIFICATION_TEXT"] : resources["COMMON_NOTIFICATION_TEXT"]];
        }

        public IReplyMarkup GetMarkup(IServiceScope actionScope = null)
        {
            InlineKeyboardButton removeBut = new InlineKeyboardButton() { CallbackData = QueryHelpers.AddQueryString(CommandKeys.RemoveNote, "property", note.Id.ToString()), Text = resources["REMOVE_BUTTON"] };
            InlineKeyboardButton changeNoteNotifBut = null;
            if (note.IsStrong)
            {
                changeNoteNotifBut = new InlineKeyboardButton() { CallbackData = CommandKeys.ChangeNoteNotification, Text = resources["STRONG_NOTIFICATION_BUTTON"] };
            }
            else
            {
                changeNoteNotifBut = new InlineKeyboardButton() { CallbackData = CommandKeys.ChangeNoteNotification, Text = resources["COMMON_NOTIFICATION_BUTTON"] };
            }

            InlineKeyboardButton back = new InlineKeyboardButton() { CallbackData = QueryHelpers.AddQueryString(CommandKeys.Notes, CallbackParams.Page, $"{fromPage}"), Text = resources["BACK_BUTTON"] };

            InlineKeyboardMarkup result = new InlineKeyboardMarkup(new InlineKeyboardButton[][] {
                new[]
                {
                    changeNoteNotifBut,
                    removeBut
                },
                new[]
                {
                    back
                }
            });
            return result;
        }
    }
}
