using System.Reflection.Emit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MiHome.Net.Dto;
using MiHome.Net.Middleware;
using MiHome.Net.Service;

namespace FTFR.WEB.Pages {
    public class IndexModel : PageModel {
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(ILogger<IndexModel> logger) {
            _logger = logger;
        }

        public void OnGet() { }
    }
}
