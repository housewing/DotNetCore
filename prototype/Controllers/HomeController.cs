using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using prototype.Models;

using Microsoft.AspNetCore.Http;

namespace prototype.Controllers
{
    public class HomeController : Controller
    {
        private readonly IIBFC configuration;
        private readonly IHttpContextAccessor accessor;

        public HomeController(IIBFC _config, IHttpContextAccessor _accessor)
        {
            configuration = _config;
            accessor = _accessor;
        }

        //[ServiceFilter(typeof(SessionTimeoutAttribute))]
        [ResponseCache(NoStore = true)]
        public IActionResult Index()
        {
            if (accessor.HttpContext.Session.GetString("LoginTimeStamp") == null)
            {
                return RedirectToAction("LoginMsg", "Home");
            }
            return View(configuration.GetIndexs());
        }

        [HttpPost]
        public IActionResult Index(string detail)
        {
            configuration.SendMail(detail);
            return RedirectToAction("LoginMsg", "Home");
        }

        [ResponseCache(NoStore = true)]
        public IActionResult Index1()
        {
            return View(configuration.GetIndex1s());
        }

        [HttpPost]
        public IActionResult Index1(string i1l)
        {
            bool flag = configuration.UpdateIndex1(i1l);
            return RedirectToAction("Index", "Home");
        }

        [ResponseCache(NoStore = true)]
        public IActionResult Index2()
        {
            return View(configuration.GetIndex2s());
        }

        [HttpPost]
        public IActionResult Index2(string i2l)
        {
            bool flag = configuration.UpdateIndex2(i2l);
            return RedirectToAction("Index", "Home");
        }

        public IActionResult LoginMsg()
        {
            HttpContext.Session.Clear();
            return View();
        }

        [HttpPost]
        public IActionResult LoginMsg(LoginMsg loginMsg)
        {
            if (ModelState.IsValid)
            {
                string ts = DateTimeOffset.Now.ToUnixTimeMilliseconds().ToString();
                accessor.HttpContext.Session.SetString("LoginTimeStamp", ts);
                configuration.Init(loginMsg);
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
