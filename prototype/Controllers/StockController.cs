using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

using Microsoft.AspNetCore.Http;
using prototype.Models;

namespace prototype.Controllers
{
    public class StockController : Controller
    {
        public readonly IIBFC configuration;

        public StockController(IIBFC _config)
        {
            configuration = _config;
        }

        [ResponseCache(NoStore = true)]
        public IActionResult Index()
        {
            ViewBag.sTotalPrice = String.Format("{0:N}", configuration.SumStockInfo());
            return View(configuration.GetStockInfos());
        }

        [ResponseCache(NoStore = true)]
        public IActionResult AddOrEdit(string id)
        {
            if (id == null)
            {
                return View(new StockInfo());
            }   
            else
            {
                return View(configuration.GetStockInfoById(id));
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddOrEdit(StockInfo s)
        {
            if (ModelState.IsValid)
            {
                string TimeStamp = HttpContext.Session.GetString("LoginTimeStamp");
                if (s.Id == null)
                {
                    bool flag = await configuration.AddStockInfo(s, "tse");
                    if (!flag)
                    {
                        await configuration.AddStockInfo(s, "otc");
                    }
                }
                else
                {
                    await configuration.UpdateStockInfo(s);
                }
                return RedirectToAction("Index", "Stock");

            }
            return View(new StockInfo());
        }

        public IActionResult Delete(string id)
        {
            if (id != null)
            {
                configuration.DeleteStockInfo(id);
            }
            return RedirectToAction("Index", "Stock");
        }
    }
}