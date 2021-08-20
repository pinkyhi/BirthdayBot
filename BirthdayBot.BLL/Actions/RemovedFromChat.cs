using Microsoft.Extensions.DependencyInjection;
using RapidBots.Types.Core;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace BirthdayBot.BLL.Actions
{
    public class RemovedFromChat : RapidBots.Types.Core.Action
    {
        public override Task Execute(Update update, TelegramUser user = null, IServiceScope actionScope = null)
        {
            return new Task(() => { });
        }

        public override bool ValidateUpdate(Update update)
        {
            return false;
        }
    }
}
