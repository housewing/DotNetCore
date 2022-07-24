using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

using Microsoft.AspNetCore.Http;
using prototype.Models;

namespace prototype.Controllers
{
    public class MortgageController : Controller
    {
        public readonly IIBFC configuration;

        public MortgageController(IIBFC _config)
        {
            configuration = _config;
        }

        [ResponseCache(NoStore = true)]
        public IActionResult Index()
        {
            ViewBag.mTotalPrice = String.Format("{0:N}", configuration.SumMortgageInfo());
            return View(configuration.GetMortgageInfos());
        }

        [ResponseCache(NoStore = true)]
        public IActionResult AddOrEdit(string id)
        {
            if (id == null)
            {
                return View(new MortgageInfo());
            }
            else
            {
                return View(configuration.GetMortgageInfoById(id));
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddOrEdit(MortgageInfo m)
        {
            if (ModelState.IsValid)
            {
                if (m.Id == null)
                {
                    await configuration.AddMortgageInfo(m);
                }
                else
                {
                    await configuration.UpdateMortgageInfo(m);
                }
                return RedirectToAction("Index", "Mortgage");
            }
            return View(new MortgageInfo());
        }

        public IActionResult Delete(string id)
        {
            if (id != null)
            {
                configuration.DeleteMortgageInfo(id);
            }
            return RedirectToAction("Index", "Mortgage");
        }
    }
}