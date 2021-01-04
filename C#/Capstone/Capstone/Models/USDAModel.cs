using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Capstone.Models
{
    // Author: Perer
    // Unused, taken from an experimental build to get usda barcode data
    public class USDAModel
    {
        [JsonProperty(PropertyName = "description")]
        public string Name { get; set; }
        [JsonProperty(PropertyName = "gtinUpc")]
        public string Code { get; set; }
    }
}