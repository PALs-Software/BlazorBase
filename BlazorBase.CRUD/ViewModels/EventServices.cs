using BlazorBase.CRUD.Services;
using BlazorBase.MessageHandling.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorBase.CRUD.ViewModels
{
    public class EventServices
    {
        public IServiceProvider ServiceProvider { get; init; }
        public IStringLocalizer Localizer { get; init; }
        public BaseService BaseService { get; init; }
        public IMessageHandler MessageHandler { get; init; }
    }
}
