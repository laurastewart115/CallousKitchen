using Capstone.Classes;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Capstone
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "ExpiryService" in code, svc and config file together.
    // NOTE: In order to launch WCF Test Client for testing this service, please select ExpiryService.svc or ExpiryService.svc.cs at the Solution Explorer and start debugging.
    public class ExpiryService : IExpiryService
    {
        public List<Product> products { get; set; }
        public string resultExpiry;
        public string resultStorage;

        DateTime dateToday = DateTime.Now;
        DateTime normalExpiryDate;

        string minExpiryRange;
        string maxExpiryRange;
        string normalExpiryRange;
        int inputSuggestedStorageTempDifference;
        TimeSpan SuggestedNTodayDateDif;
        float tempFoodLifecycleRelation;
        FinalResult finalResult = new FinalResult();
        int expiryDuration;


        public Tuple<DateTime, DateTime, Storage> GetDateStorage(string food, Storage storage)
        {
            products = getJsonData();
            checkJsonData(food, products);



            StorageTemperature storageCopy = (StorageTemperature)storage;

            inputSuggestedStorageTempDifference = (int)storageCopy - (int)finalResult.SuggestedStorage;
            
            //every 10C or 18F temp increase > food lifecycle halves
            tempFoodLifecycleRelation = (float)Math.Sqrt(10.0 / inputSuggestedStorageTempDifference);
            SuggestedNTodayDateDif = finalResult.SuggestedExpiry - dateToday;
            expiryDuration = (int)Math.Round((SuggestedNTodayDateDif.Days * 1.0 / tempFoodLifecycleRelation));

            finalResult.InputResultExpiry = dateToday.AddDays(expiryDuration);


            return Tuple.Create(finalResult.SuggestedExpiry, finalResult.InputResultExpiry, finalResult.SuggestedStorage); 

        }


        public List<Product> getJsonData()
        {
            string json = System.IO.File.ReadAllText(System.Web.Hosting.HostingEnvironment.ApplicationPhysicalPath + "expiryinfo.json");
            products = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Product>>(json);
            return products;
        }


        public FinalResult checkJsonData(string crop, List<Product> products)
        {
            var format = new NumberFormatInfo();
            format.NegativeSign = "-";
            format.NumberDecimalSeparator = ".";


            foreach (var item in products)
            {
                if (crop.ToLower().Equals(item.FOOD.ToLower()))
                {

                    if (item.STORAGELIFE.Contains("-"))
                    {
                        minExpiryRange = item.STORAGELIFE.Substring(0, item.STORAGELIFE.IndexOf("-"));
                        maxExpiryRange = item.STORAGELIFE.Substring(item.STORAGELIFE.IndexOf("-") + 1);
                        resultExpiry = Math.Round((double.Parse(minExpiryRange) + double.Parse(maxExpiryRange)) / 2).ToString();

                        if (Convert.ToDouble(resultExpiry) > 30)
                        {

                            normalExpiryDate = dateToday.AddDays(30);
                            resultExpiry = normalExpiryDate.ToShortDateString();
                        }
                        else
                        {
                            normalExpiryDate = dateToday.AddDays(Convert.ToDouble(resultExpiry));
                            resultExpiry = normalExpiryDate.ToShortDateString();
                        }

                    }
                    else
                    {
                        normalExpiryRange = item.STORAGELIFE;
                        if (Convert.ToDouble(normalExpiryRange) > 30)
                        {
                            normalExpiryDate = dateToday.AddDays(30);
                            resultExpiry = normalExpiryDate.ToShortDateString();
                        }
                        else
                        {
                            normalExpiryDate = dateToday.AddDays(Convert.ToDouble(normalExpiryRange));

                            resultExpiry = normalExpiryDate.ToShortDateString();
                        }
                    }


                    string minTemperatureRange = "0";
                    string maxTemperatureRange = "0";
                    string normalTemperatureRange = "0";
                    string finalTemperature = "0";

                    if (item.TEMPERATURE.Contains("-"))
                    {
                        minTemperatureRange = item.TEMPERATURE.Substring(0, item.TEMPERATURE.LastIndexOf("-"));
                        maxTemperatureRange = item.TEMPERATURE.Substring(item.TEMPERATURE.LastIndexOf("-") + 1);


                        finalTemperature = Math.Round((double.Parse(minTemperatureRange, format) + double.Parse(maxTemperatureRange, format)) / 2).ToString();


                    }
                    else
                    {
                        normalTemperatureRange = item.TEMPERATURE;

                        finalTemperature = normalTemperatureRange;
                    }


                    if (double.Parse(finalTemperature, format) >= -18 && double.Parse(finalTemperature, format) <= -2)
                    {
                        finalResult.SuggestedStorage = Storage.Freezer;
                    }
                    else
                    if (double.Parse(finalTemperature, format) >= -2 && double.Parse(finalTemperature, format) <= 7)
                    {
                        finalResult.SuggestedStorage = Storage.Fridge;

                    }
                    else
                    if (double.Parse(finalTemperature, format) > 7 && double.Parse(finalTemperature, format) <= 14)
                    {
                        finalResult.SuggestedStorage = Storage.Cellar;

                    }
                    else if (double.Parse(finalTemperature, format) > 14)
                    {
                        finalResult.SuggestedStorage = Storage.Pantry;

                    }
                    finalResult.SuggestedExpiry = DateTime.Parse(resultExpiry);
                    return finalResult;
                }
            }

            return finalResult;
        }

        public class Product
        {
            public string FOOD { get; set; }
            public string TEMPERATURE { get; set; }
            public string STORAGELIFE { get; set; }
        }
        public class FinalResult
        {
            public Storage SuggestedStorage { get; set; }
            public DateTime SuggestedExpiry { get; set; }
            public DateTime InputResultExpiry { get; set; }
        }
        public enum StorageTemperature
        {
   
            Fridge = 4,
            Freezer = -18,
            Cellar = 10,
            Pantry = 18
        }
    }


}
