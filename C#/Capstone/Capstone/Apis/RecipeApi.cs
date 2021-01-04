using Capstone.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;

namespace Capstone.Apis
{
    // Author Peter Szadurski
    public class RecipeApi
    {
        private ApiHelper ApiHelper = new ApiHelper();

        private readonly string AppiKey = "2f1c9181989a25dd44b03abe15600a3f";
        private readonly string AppId = "ffa3c67d";
        public async Task<SerializableRecipeModel[]> GetRecipe(string search, int count, string[] diets)
        {

            string dietsString = "";
            // Fix the diets array
            if (diets != null)
            {
                for (int i = 0; i < diets.Length; i++)
                {
                    diets[i] = diets[i].ToLower();
                    if (diets[i] != "vegan" && diets[i]
                        != "vegetarian" && diets[i] != "paleo" && diets[i] != "low-sugar")
                    {
                        diets[i] = diets[i] += "-free";
                    }
                    dietsString += "&health=" + diets[i];
                }

            }


            List<SerializableRecipeModel> results = new List<SerializableRecipeModel>();
            search = System.Web.HttpUtility.UrlEncode(search);
            string url = $"https://api.edamam.com/search?q={search}&app_id={AppId}&app_key={AppiKey}&from=0&to={count}" + dietsString;
            using (HttpResponseMessage response = await ApiHelper.ApiClient.GetAsync(url))
            {
                if (response.IsSuccessStatusCode)
                {
                    RecipeQueryModel RQModel = await response.Content.ReadAsAsync<RecipeQueryModel>();
                    for (int i = 0; i < RQModel.Hits.Count(); i++)
                    {
                        System.Diagnostics.Debug.WriteLine("Scanning");
                        results.Add(new SerializableRecipeModel(RQModel.Hits[i].Recipe));

                    }
                    return results.ToArray();
                }
                else
                {
                    return null;
                }
            }
        }
    }
}