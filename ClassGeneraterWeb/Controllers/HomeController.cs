using ClassGeneraterWeb.Models;
using ClassGeneraterWeb.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;

namespace ClassGeneraterWeb.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IGeneraterClassService _generaterClassService;

        public HomeController(
            ILogger<HomeController> logger,
            IGeneraterClassService generaterClassService)
        {
            _logger = logger;
            _generaterClassService = generaterClassService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpPost]
        public bool CheckConnectionString(CheckConnection checkConnection)
        {
            bool isSuccess = false;

            // 加上timeout 1秒，只做測試連線，避免測試連線時間過長
            checkConnection.ConnectionString += "Connection Timeout = 1;";

            try
            {
                using SqlConnection conn = new SqlConnection(checkConnection.ConnectionString);
                conn.Open();
                isSuccess = conn.State == ConnectionState.Open;
            }
            catch
            {
                isSuccess = false;
            }

            return isSuccess;
        }

        [HttpPost]
        public GeneraterClassViewModel GeneraterClassAction(GeneraterClassAction generaterClassAction)
        {
            if (!ModelState.IsValid)
                return new GeneraterClassViewModel() { IsSuccess = false, ErrorMessage = ModelState.Keys.SelectMany(key => ModelState[key].Errors).Select(x => x.ErrorMessage).FirstOrDefault() };

            (string pocoClass, string ErrorMessage) = _generaterClassService.GeneraterClass(generaterClassAction);

            if (string.IsNullOrEmpty(pocoClass))
                return new GeneraterClassViewModel() { IsSuccess = false, ErrorMessage = ErrorMessage };

            return new GeneraterClassViewModel() { IsSuccess = true, PocoClass = pocoClass };
        }
    }
}