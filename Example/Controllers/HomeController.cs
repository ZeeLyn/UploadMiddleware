using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Example.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;

namespace Example.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;


        public IConfiguration Configuration { get; }
        public HomeController(ILogger<HomeController> logger, IConfiguration configuration)
        {
            _logger = logger;
            Configuration = configuration;
        }

        public async Task<IActionResult> Index()
        {
            //await using var writeStream = new FileStream("D:\\temp\\temp.txt", FileMode.Create, FileAccess.Write, FileShare.ReadWrite, 1024);
            ////writeStream.SetLength(100);
            //writeStream.Write(System.Text.Encoding.UTF8.GetBytes("Hello"));

            //await using var write2Stream = new FileStream("D:\\temp\\temp.txt", FileMode.Create, FileAccess.Write, FileShare.ReadWrite, 1024);
            ////write2Stream.SetLength(100);

            //var end = write2Stream.Seek(2, SeekOrigin.Begin);
            ////var sw = new StreamWriter(write2Stream);
            ////sw.Write("****");
            //var buffer = System.Text.Encoding.UTF8.GetBytes("****");
            //write2Stream.Write(buffer);
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
