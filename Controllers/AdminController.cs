using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using GlobalEvent.Models;
using GlobalEvent.Models.ManageViewModels;
using GlobalEvent.Services;

namespace GlobalEvent.Controllers
{
    [Authorize]
    public class AdminController : Controller
    {

    }
}
