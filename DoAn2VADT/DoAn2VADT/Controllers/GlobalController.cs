using AspNetCoreHero.ToastNotification.Abstractions;
using DoAn2VADT.Database;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;

namespace DoAn2VADT.Controllers
{
    [SessionExpire]
    public class GlobalController : Controller
    {
        public readonly AppDbContext _context;
        public INotyfService _notyfService;
        public IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<GlobalController> _logger;
        public GlobalController(AppDbContext context, INotyfService notyfService, IHttpContextAccessor httpContextAccessor, ILogger<GlobalController> logger)
        {
            _notyfService = notyfService;
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;            
        }

    }
}
