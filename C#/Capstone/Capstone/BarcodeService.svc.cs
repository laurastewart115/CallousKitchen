using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using Capstone.Apis;
using Capstone.Classes;

namespace Capstone
{
    // Author: Peter
    // Service to read open food facts
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "BarcodeService" in code, svc and config file together.
    // NOTE: In order to launch WCF Test Client for testing this service, please select BarcodeService.svc or BarcodeService.svc.cs at the Solution Explorer and start debugging.
    public class BarcodeService : IBarcodeService
    {
       private CallousHipposDb db = new CallousHipposDb();
        public Task<string> GetBarcodeData(string barcode)
        {
            OpenFoodFacts openFoodFacts = new OpenFoodFacts();
            return openFoodFacts.LoadBarcode(barcode);
        }
        public async Task<bool> AddFood(int kitchenId, string name, int quantity)
        {
            db.Kitchens.Where(x => x.Id == kitchenId).FirstOrDefault().Inventory
                .Add(db.Foods.Add(new Food() { Name = name, Quantity = quantity }));
            await db.SaveChangesAsync();
            return true;
        }
        public async Task<bool> EditItem(int id, int quantity)
        {
            var item = db.Foods.Where(x => x.Id == id).FirstOrDefault();
            item.Quantity = quantity;
            await db.SaveChangesAsync();
            return true;
        }
        public async Task<bool> RemoveItem(int id)
        {
            var item = db.Foods.Where(x => x.Id == id).FirstOrDefault();
            db.Foods.Remove(item);
            await db.SaveChangesAsync();
            return true;
        }

        public string TestMethod() {
            return "ok";
        }

    }
}
