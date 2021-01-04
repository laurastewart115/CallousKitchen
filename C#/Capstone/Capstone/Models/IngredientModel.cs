using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using System.Web;

namespace Capstone.Models
{
    // Author Peter Szadurski /
    // Includes ingredient normalization, it was used when we didn't have edamam's starup plan

    public static class IgnoredIngredientSections
    {
        // class is for removing parts of a string like "sticks" or "cloves" that may make searching for an ingredient harder
        // also ignores units of measurement

        public static string[] Ignored = { "cups", "cup", "grams", "gram", "ounce", "ounce", "Tbsp", "sticks",
            "stick", "litre" ,  "stick", "frozen", "thawed", "pounds", "pound", "tablespoons",  "tablespoon", "teaspoons", "teaspoon", "large", "small", "bunch", "of", "chopped", "ltr", "g", "ml" };
    }
    [DataContract]
    [Serializable]
    public class IngredientModel
    {
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public string OriginalName { get; set; }
        [DataMember]
        public int Score { get; set; } // The ranking of how well an ingredient matches with an inventory item

        public IngredientModel(string name)
        {
            Name = name;
            OriginalName = name;
            Score = 0;
            // check for "of" and ","
            if (Name.ToLower().ToLower().Contains(" of ")){
                Name = Regex.Split(Name ," of ")[1];
            }
            if (Name.ToLower().ToLower().Contains(", "))
            {
                Name = Regex.Split(Name, ", ")[0];
            }

            // Run Name by the filter
            foreach (var x in IgnoredIngredientSections.Ignored)
            {
                if (Regex.IsMatch(Name, @"(^|\s)" + x + @"(\s|$)"))
                {
                    Name = Name.Replace(" " + x, string.Empty);
                    Name = Name.Replace(" " + x + " ", string.Empty);
                    Name = Name.Replace(x + " ", string.Empty);
                }

            }

            // remove numbers

            Name = Regex.Replace(Name, @"[\d-]", string.Empty);

            // remove slashes, decimals, and commas

            Name = Name.Replace(@"\", string.Empty);
            Name = Name.Replace(@"/", string.Empty);
            Name = Name.Replace(@".", string.Empty);
            Name = Name.Replace(@",", string.Empty);
            Name = Name.Trim();
            // string is cleaned up

            List<Classes.Food> foods = new List<Classes.Food>();

            // Highlight Ingredients
            //if (foods.Where(x=> x.Categories.Contains("Salt")))
        }

        /*    public IngredientModel(string name)
            {
                Name = name;

            }
        */
    }/*
    [DataContract]
    [Serializable]
    public class SerializableIngredientModel
    {
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public int Score { get; set; }


        public SerializableIngredientModel(IngredientModel m)
        {
            Name = m.Name;
            Score = m.Score;
        }
    }
    */
}