using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace Capstone.Models
{
    // Author: Peter
    [DataContract]
    [Serializable]
    public class EdamanIngredient
    {
        [JsonProperty(PropertyName = "food")]
        public string Name { get; set; }


    }

    [DataContract]
    [Serializable]
    public class EdamanIngredientScored
    {
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public int Score { get; set; }

    }
}