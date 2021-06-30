using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NegocieOnlineAPI.Database;
using NegocieOnlineAPI.Models;

namespace NegocieOnlineAPI.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Consulta(string CEP, string submit)
        {
            ViaCEPResult result = null;

            switch (submit)
            {
                case "botão consulta WS":
                    result = ViaCEP.ViaCEPAPI.Consulta(CEP);

                    if (result != null)
                    {
                        return View("ConsultaWS", result);
                    }
                    break;
                case "botão consulta DB":
                    result = DatabaseCRUD.Consulta(CEP);

                    if (result != null)
                    {
                        return View("ConsultaDB", result);
                    }
                    break;
            }

            return RedirectToAction("Error");
        }

        [HttpGet]
        public IActionResult InserirDB(ViaCEPResult result)
        {
            if (DatabaseCRUD.Insere(result))
            {
                return RedirectToAction("Index");
            }
            else
            {
                return View("ErrorDatabase");
            }            
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
