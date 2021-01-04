using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace CallousFrontEnd.Controllers
{

    // Author Peter Szadurski
    // Controller for email confirmations
    public class EmailController : Controller
    {
        AccountService.AccountServiceMvcClient Client = new AccountService.AccountServiceMvcClient();

        [HttpGet]
        public async Task<ActionResult> Confirm(string Key)
        {
            string result = await Client.ConfirmEmailAsync(Key);
            ViewBag.Result = result;
            return View();
        }
    }
}
