using Capstone.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;

namespace Capstone.Apis
{
    // Author Peter Szadurski
    
    public class OpenFoodFacts
    {
        private ApiHelper ApiHelper = new ApiHelper();
        //apiHelper

        public async Task<string> LoadBarcode(string barcode)
        {
            string url = $"http://world.openfoodfacts.org/api/v0/product/{barcode}.json";
            using (HttpResponseMessage response = await ApiHelper.ApiClient.GetAsync(url))
            {
                if (response.IsSuccessStatusCode)
                {
                    FoodFactsModel FFModel = await response.Content.ReadAsAsync<FoodFactsModel>();
                    return FFModel.Product.Name;
                }
                else
                {
                    return "The request has failed";
                }


            }
        }
        public async Task<SerializableFoodFactsProductModel> LoadAllBarcodeData(string barcode)
        {

            string url = $"http://world.openfoodfacts.org/api/v0/product/{barcode}.json";
            using (HttpResponseMessage response = await ApiHelper.ApiClient.GetAsync(url))
            {
               // ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls;

                if (response.IsSuccessStatusCode)
                {
                    FoodFactsModel FFModel = await response.Content.ReadAsAsync<FoodFactsModel>();
                    SerializableFoodFactsProductModel serializedFoodFactsProductModel = new SerializableFoodFactsProductModel(FFModel.Product);
                    return serializedFoodFactsProductModel;
                }
                else
                {
                    return null;
                }
            }
        }

    }
}