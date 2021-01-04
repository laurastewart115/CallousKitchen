using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Capstone.Models
{
    // Author: Peter
    public class FoodFactsModel
    {
        [JsonProperty(PropertyName = "product")]
        public FoodFactsProductModel Product { get; set; }
        public string Code { get; set; }
    }
}