using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace CallousFrontEnd.Controllers
{
    public class BarcodeController : Controller
    {
        //BarcodeService.BarcodeServiceClient Client = new BarcodeService.BarcodeServiceClient();

        public IActionResult Index() { 
            return View();
        }
      /*  public async Task<string> ReadBarcode(string barcode)
        {
        //    string test = await Client.GetBarcodeDataAsync(barcode);
          //   return await Client.GetBarcodeDataAsync(barcode);
        }
      */

       // public string test() {
//return Client.TestMethod();
        }
    }
//}