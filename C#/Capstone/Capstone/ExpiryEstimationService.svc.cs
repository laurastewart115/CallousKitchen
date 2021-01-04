using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace Capstone
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "ExpiryEstimationService" in code, svc and config file together.
    // NOTE: In order to launch WCF Test Client for testing this service, please select ExpiryEstimationService.svc or ExpiryEstimationService.svc.cs at the Solution Explorer and start debugging.
    public class ExpiryEstimationService : IExpiryEstimationService
    {
        public List<Product> products { get; set; }
        Result result = new Result();
        DateTime dateToday = DateTime.Now;

        DateTime expiryDate;

        string minExpiryRange;
        string maxExpiryRange;
        string normalExpiryRange;
        public string resultExpiry;
        public string resultStorage;

        //fridge temperature
        double storageTemperature = 2.5;

        string minTemperatureRange = "0";
        string maxTemperatureRange = "0";
        string normalTemperatureRange = "0";
        string finalTemperature = "0";


        //Service function, takes product name - returns expiry date
        public DateTime GetDate(string productName)
        {
           return GetExpiryBasedOnDate(productName);

        }

        //return expiry result
        public DateTime GetExpiryBasedOnDate(string crop)
        {
            products = getJsonData();
            goThroughJsonData(crop, products, "1");
            return result.RESULTEXPIRY;
        }

        //Service function, takes product name and storage - returns expiry date
        public DateTime GetDateBasedOnStorage(string productName, string storage)
        {
            return GetExpiryBasedOnDateStorage(productName, storage).Item2;

        }



        //return expiry result with storage taken into account
        public Tuple<DateTime, DateTime> GetExpiryBasedOnDateStorage(string crop, string storage)
        {
            products = getJsonData();
            storage = storage.ToLower();
            goThroughJsonData(crop, products, storage);

            return new Tuple<DateTime, DateTime>(result.RESULTEXPIRY, result.RESULTEXPIRYSTORAGE);
        }






        //compare user product to products in data and make an expiry estimation
        public Result goThroughJsonData(string product, List<Product> products, string storage)
        {

            //specifying characters for negative sign and separator in expiry.json file
            var format = new NumberFormatInfo();
            format.NegativeSign = "-";
            format.NumberDecimalSeparator = ".";



            foreach (var item in products)
            {
                //if an item found in expiry.json
                if (item.CROP.ToLower().Contains(product.ToLower()))
                {
                    //if expiry item has not a single expiry date (29.03) but a range (29.03-31.03)
                    if (item.STORAGELIFE.Contains("-"))
                    {
                        minExpiryRange = item.STORAGELIFE.Substring(0, item.STORAGELIFE.IndexOf("-"));
                        maxExpiryRange = item.STORAGELIFE.Substring(item.STORAGELIFE.IndexOf("-") + 1);
                        resultExpiry = Math.Round((double.Parse(minExpiryRange) + double.Parse(maxExpiryRange)) / 2).ToString();

                        //if an item has vast expiry duration: ketchup, nuts give an 30 day expiry
                        if (Convert.ToDouble(resultExpiry) > 30)
                        {

                            expiryDate = dateToday.AddDays(30);
                            resultExpiry = expiryDate.ToShortDateString();
                        }
                        //if an item has an expiry duration less than 30 days
                        else
                        {
                            expiryDate = dateToday.AddDays(Convert.ToDouble(resultExpiry));
                            resultExpiry = expiryDate.ToShortDateString();
                        }

                    }
                    //if a expiry item has a single date
                    else
                    {
                        normalExpiryRange = item.STORAGELIFE;
                        //if an item has vast expiry duration: ketchup, nuts give an 30 day expiry
                        if (Convert.ToDouble(normalExpiryRange) > 30)
                        {
                            expiryDate = dateToday.AddDays(30);
                            resultExpiry = expiryDate.ToShortDateString();
                        }
                        //if an item has an expiry duration less than 30 days
                        else
                        {
                            expiryDate = dateToday.AddDays(Convert.ToDouble(normalExpiryRange));

                            resultExpiry = expiryDate.ToShortDateString();
                        }
                    }



                    if (!storage.Equals("1"))
                    {

                    

                    //if temperature is a range (9-16) get an average
                    if (item.TEMPERATURE.Contains("-"))
                    {
                        minTemperatureRange = item.TEMPERATURE.Substring(0, item.TEMPERATURE.LastIndexOf("-"));
                        maxTemperatureRange = item.TEMPERATURE.Substring(item.TEMPERATURE.LastIndexOf("-") + 1);


                        finalTemperature = Math.Round((double.Parse(minTemperatureRange, format) + double.Parse(maxTemperatureRange, format)) / 2).ToString();


                    }
                    //get temperature
                    else
                    {
                        normalTemperatureRange = item.TEMPERATURE;

                        finalTemperature = normalTemperatureRange;
                    }


                    


                    
                    //storage temperature are assigned as an average temperature for type of storage in celcius
                    if (storage.ToLower().Equals("freezer")){
                        storageTemperature = -10;
                    }
                    else if (storage.ToLower().Equals("fridge")){
                        storageTemperature = 2.5;
                    }
                    else if (storage.ToLower().Equals("cellar")){
                        storageTemperature = 10.5;
                    }
                    else if (storage.ToLower().Equals("pantry")){
                        storageTemperature = 17;
                    }
                    else if (storage.ToLower().Equals("cupboard")){
                        storageTemperature = 17;
                    }
                    //other - table, room temperature
                    else if (storage.ToLower().Equals("other"))
                    {
                        storageTemperature = 20;

                    }



                    
                    double tempDif;

                        //compare allocated storage in json/standard storage to storage provided by user
                        //difference in temperatures divide by 10 degrees and multiply by factor of 2 (every 10 degrees celcius product lifespan  halves ()

                        //+10 degrees - divide by 2
                        //+5 degrees - divide by sqrt(2)
                        //+2.5 degrees - divide by sqrt(sqrt(2))
                        //+1.25 degrees - divide by sqrt(sqrt(sqrt(2))
                        //-1.25 degrees - multiply by sqrt(sqrt(sqrt(2))
                        double ratio = 1;

                        tempDif = (double.Parse(finalTemperature, format) - storageTemperature);

                        //change ratio which will be used to calcute expiry (ex 1*sqrt(2)) every 1.25 degrees
                        if (tempDif > 0)
                        {
                            while (tempDif > 0)
                            {
                                if (tempDif > 1.24)
                                {
                                    tempDif = tempDif - 1.25;
                                    ratio = ratio * Math.Sqrt(Math.Sqrt(Math.Sqrt(2)));
                                }
                            }
                        }
                        else if (tempDif < 0)
                        {
                            while (tempDif < 0)
                            {
                                if (tempDif < 1.24)
                                {
                                    tempDif = tempDif + 1.25;
                                    ratio = ratio / Math.Sqrt(Math.Sqrt(Math.Sqrt(2)));
                                }
                            }
                        }
                    
                    //get the difference between expiry date calculated based on name only AND todays date
                    TimeSpan difference = expiryDate - dateToday;
                    double days = (int)difference.TotalDays;

                    //adjust date with ratio
                    days = 1.0 * days * ratio;
                    result.RESULTEXPIRYSTORAGE = dateToday.AddDays((int)days);



                    }

                    //if expiry estimated successfully - return it
                    result.RESULTEXPIRY = DateTime.Parse(resultExpiry);
                    return result;
                }
            }

            //if expiry estimation was not possible - assign standard 7 days
            result.RESULTEXPIRY = dateToday.AddDays(7);

            return result;
        }






        //receive products  from json file
        public List<Product> getJsonData()
        {
            string json = System.IO.File.ReadAllText(System.Web.Hosting.HostingEnvironment.ApplicationPhysicalPath + "expiryinfo.json");
            products = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Product>>(json);
            return products;
        }

        public class Product
        {
            public string CROP { get; set; }
            public string TEMPERATURE { get; set; }
            public string STORAGELIFE { get; set; }
        }
        public class Result
        {
            public string RESULTSTORAGE { get; set; }
            public DateTime RESULTEXPIRY { get; set; }

            public DateTime RESULTEXPIRYSTORAGE { get; set; }
        }

    }
}