using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace Capstone.Models
{
    // Author: Peter

    public class FoodFactsProductModel
    {
        [JsonProperty(PropertyName = "product_name")]
        public string Name { get; set; }
        [JsonProperty(PropertyName = "image_thumb_url")]
        public string Image_Url { get; set; }
        [JsonProperty(PropertyName = "Serving_size")]
        public string ServingSize { get; set; }

        [JsonProperty(PropertyName = "nutriments")]
        public Dictionary<string, string> Nutrients { get; set; }

        [JsonProperty(PropertyName = "serving_quantity")]
        public int ServingQuantity { get; set; }
        [JsonProperty(PropertyName = "ingredients_ids_debug")]
        public string[] Ingredients { get; set; }
        //[JsonProperty(PropertyName = "nutrition_grades")]


    }

    [DataContract]
    [Serializable]
    public class SerializableFoodFactsProductModel
    {
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public string Image_Url { get; set; }
        [DataMember]
        public string ServingSize { get; set; }
        [DataMember]
        public Dictionary<string, string> Nutrients { get; set; }
        [DataMember]
        public int ServingQuantity { get; set; }
        [DataMember]
        public string[] Ingredients { get; set; }


        public SerializableFoodFactsProductModel(FoodFactsProductModel model)
        {
            this.Name = model.Name;
            this.Image_Url = model.Image_Url;
            this.ServingSize = model.ServingSize;
            this.Nutrients = model.Nutrients;
            this.ServingQuantity = model.ServingQuantity;
            this.Ingredients = model.Ingredients;
        }
    }
}