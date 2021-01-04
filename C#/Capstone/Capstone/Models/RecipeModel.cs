using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace Capstone.Models
{
    // Author Peter Szadurski
    public class RecipeModel
    {
        [JsonProperty(PropertyName = "label")]
        public string Name { get; set; }
        [JsonProperty(PropertyName = "url")]
        public string Url { get; set; }
        [JsonProperty(PropertyName = "image")]
        public string Image { get; set; }
        [JsonProperty(PropertyName = "yield")]
        public string Yield { get; set; }
        [JsonProperty(PropertyName = "healthLabels")]
        public string[] HealthLabels { get; set; }
        [JsonProperty(PropertyName = "source")]
        public string Source { get; set; }
        [JsonProperty(PropertyName = "ingredientLines")]
        public string[] Ingredients { get; set; }

        [JsonProperty(PropertyName = "ingredients")]
        public EdamanIngredient[] EdamanIngredients { get; set; }


    }

    [DataContract]
    [Serializable]
    public class SerializableRecipeModel
    {
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public string Url { get; set; }
        [DataMember]
        public string Image { get; set; }
        [DataMember]
        public double Yield { get; set; }
        [DataMember]
        public string[] HealthLabels { get; set; }
        [DataMember]
        public string Source { get; set; }
        [DataMember]
        public int Score { get; set; }
        [DataMember]
        public IngredientModel[] Ingredients { get; set; }
        [DataMember]
        public EdamanIngredientScored[] EdamanIngredients { get; set; }



    public SerializableRecipeModel(RecipeModel m)
        {
            Name = m.Name;
            Url = m.Url;
            Image = m.Image;
            Yield = Convert.ToDouble(m.Yield);
            HealthLabels = m.HealthLabels;
            Source = m.Source;
            List<IngredientModel> ingreds = new List<IngredientModel>();
            foreach (var i in m.Ingredients)
            {
                ingreds.Add(new IngredientModel(i));
            }
            Ingredients = ingreds.ToArray();
            Score = 0;

            List<EdamanIngredientScored> edams = new List<EdamanIngredientScored>();
            foreach (var i in m.EdamanIngredients)
            {
                EdamanIngredientScored edam = new EdamanIngredientScored();
                edam.Name = i.Name;
                edam.Score = 0;
                edams.Add(edam);
            }
            EdamanIngredients = edams.ToArray();
        }
    }
}