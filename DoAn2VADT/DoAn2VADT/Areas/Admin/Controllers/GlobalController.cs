using AspNetCoreHero.ToastNotification.Abstractions;
using DoAn2VADT.Database;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;
using Microsoft.AspNetCore.Hosting;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;

namespace DoAn2VADT.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class GlobalController : Controller
    {
        public readonly AppDbContext _context;
        public INotyfService _notyfService;
        public IHostingEnvironment _environment;
        public IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<GlobalController> _logger;
        public GlobalController(AppDbContext context, IHostingEnvironment environment ,INotyfService notyfService, IHttpContextAccessor httpContextAccessor, ILogger<GlobalController> logger)
        {
            _notyfService = notyfService;
            _context = context;
            _environment = environment;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

    }
}
