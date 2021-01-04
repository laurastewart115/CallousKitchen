using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using CallousFrontEnd.Models;
using AccountService;
using System.Diagnostics;
//using AccountServiceOther;

namespace CallousFrontEnd.Controllers
{
    // Author: Peter Szadurski
    public class UserController : Controller
    {

        AccountService.AccountServiceMvcClient Client = new AccountService.AccountServiceMvcClient();
        // Author: Peter Szadurski
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateUser(Capstone.Classes.User user, IFormCollection form)
        {
            List<string> allergiesList = new List<string>();

            // Process the allergy checkboxes

            foreach (var a in Allergies.GetAllergies())
            {
                if (form[a] == "on")
                {
                    allergiesList.Add(a);
                }
            }

            // Process the "Other" textbox
            if (form["other"] != "" || form["other"].Count() != 0)
            {
                allergiesList.Add(form["other"]);
            }


            SerializableUser serializableUser = Client.CreateAccount(user.Email, user.Password);
            // Log in the user if the process is successful
            if (serializableUser.Id > 0)
            {
                Client.EditUserDietaryRestrictions(serializableUser.Id, form["Diet"] == "Vegan", form["Diet"] == "Vegetarian", allergiesList.ToArray());
                UserSessionModel userSession = new UserSessionModel { Id = serializableUser.Id, Username = user.Email };
                HttpContext.Session.SetInt32("UserId", serializableUser.Id);
                HttpContext.Session.SetString("Username", user.Email);
                return AccountView(userSession);
            }
            return RedirectToAction("LoginView");
        }
        [HttpGet]
        public ActionResult CreateUserView()
        {
            return View("CreateUser");
        }


        // Author: Peter
        // ActionResult for Settings View
        [HttpGet]
        public ActionResult Settings()
        {
            int userId = HttpContext.Session.GetInt32("UserId").GetValueOrDefault();
            ViewBag.UserId = userId;
            var user = Client.GetSerializableUser(userId);
            // Setup the checkboxes so it matches the user data
            string[] selected = new string[3];
            if (user.Vegan)
            {
                selected[2] = "checked";
            }
            else if (user.Vegetarian)
            {
                selected[1] = "checked";
            }
            else
            {
                selected[0] = "checked";
            }

            // Setup the allergies so it matches the user data
            List<string> aM = new List<string>();
            foreach (var a in Allergies.GetAllergies())
            {
                if (user.Allergies.Contains(a))
                {
                    aM.Add("checked");
                }
                else
                {
                    aM.Add("");
                }
            }

            // set other

            if (!Allergies.GetAllergies().Contains(user.Allergies.LastOrDefault()))
            {
                ViewBag.Other = user.Allergies.LastOrDefault();
            }


            ViewBag.Checked = aM;
            ViewBag.Selected = selected;
            return View("Settings", user);
        }

        // Author: Peter
        // Settings form submit request
        [HttpPost]
        public ActionResult Settings(SerializableUser user, IFormCollection form)
        {
            ViewBag.UserId = user.Id;

            // Process allergy checkboxes
            List<string> allergiesList = new List<string>();
            foreach (var a in Allergies.GetAllergies())
            {
                if (form[a] == "on")
                {
                    allergiesList.Add(a);
                }
            }


            // Process "Other"
            if (form["other"] != "" || form["other"].Count() != 0)
            {
                allergiesList.Add(form["other"]);
            }

            // Update viewbag to tell the user if the changes were saved or not
            try
            {
                Client.EditUserDietaryRestrictions(user.Id, form["Diet"] == "Vegan", form["Diet"] == "Vegetarian", allergiesList.ToArray());
                ViewBag.Result = "Settings Changed";
            }
            catch
            {
                ViewBag.Result = "There was a problem saving data.";
            }

            user = Client.GetSerializableUser(user.Id);

            // set other
            if (!Allergies.GetAllergies().Contains(user.Allergies.LastOrDefault()))
            {
                ViewBag.Other = user.Allergies.LastOrDefault();
            }

            // Setup the settings view with the new data

            string[] selected = new string[3];
            if (user.Vegan)
            {
                selected[2] = "checked";
            }
            else if (user.Vegetarian)
            {
                selected[1] = "checked";
            }
            else
            {
                selected[0] = "checked";
            }
            List<string> aM = new List<string>();
            foreach (var a in Allergies.GetAllergies())
            {
                if (user.Allergies.Contains(a))
                {
                    aM.Add("checked");
                }
                else
                {
                    aM.Add("");
                }
            }


            ViewBag.Checked = aM;
            ViewBag.Selected = selected;
            return View("Settings", user);

        }



        // Author: Peter, Kevin
        [HttpPost]
        public ActionResult Login(LoginModel login)
        {

            SerializableUser serializableUser = Client.LoginAccount(login.Username, login.Password);
            if (serializableUser.Id != -1)
            {
                UserSessionModel user = new UserSessionModel { Id = serializableUser.Id, Username = login.Username };
                System.Diagnostics.Debug.WriteLine("UserId: " + serializableUser.Id);
                

                // Any time the webapp needs to get a the user, its done through session variables.
                HttpContext.Session.SetInt32("UserId", serializableUser.Id);
                HttpContext.Session.SetString("Username", login.Username);
                ViewBag.UserSession = user;


                if (login.Remember)
                {
                    //Setup the cookie
                    HttpContext.Response.Cookies.Append("Username", login.Username);
                    HttpContext.Response.Cookies.Append("Password", login.Password);
                }

                return AccountView(user);
            }
            return RedirectToAction("LoginView");
        }
        
        // Author: Peter, Kevin
        public ActionResult LoginView()
        {
            if (HttpContext.Request.Cookies.ContainsKey("Username"))
            {
                string username = HttpContext.Request.Cookies["Username"];
                string password = HttpContext.Request.Cookies["Password"];
                return Login(new LoginModel { Username = username, Password = password, Remember = true });

            }
            return View("Login");
        }

        // Author Peter Szadurski
        
        [HttpPost]
        public ActionResult AccountView(UserSessionModel userSession)
        {
            TempData["userId"] = userSession.Id;
            return RedirectToAction("HomeView");
        }

        // Author: Peter
        // This is the view that gets called if the "Kitchen" button is clicked
        public ActionResult KitchenView()
        {
            TempData["userId"] = HttpContext.Session.GetInt32("UserId").GetValueOrDefault();
            return HomeView();
        }

        // Author: Peter
        // This is the view that gets caled upon a successful login
        public ActionResult HomeView()
        {
            if (TempData["userId"] != null)
            {
                int userId = (int)TempData["userId"];
                TempData.Keep();
                SerializableUser user = Client.GetSerializableUser(userId);
                bool confirmed = user.IsConfirmed;
                ViewBag.UserId = user.Id;

                ViewBag.IsVegan = user.Vegan.ToString();
                ViewBag.IsVeg = user.Vegetarian.ToString();
                //user.Kitchens = Client.GetKitchens(userSession.Id).ToList();
                ViewBag.IsConfirmed = confirmed;

                user.Kitchens = Client.GetKitchens(userId);
                return View("Account", user);
            }
            else
            {
                return View("Login");
            }
        }

        // Author Peter, Kevin
        public ActionResult Logout()
        {
            // destory all cookies
            if (HttpContext.Request.Cookies.ContainsKey("Username"))
            {
                HttpContext.Response.Cookies.Delete("Username");
                HttpContext.Response.Cookies.Delete("Password");
            }
            ViewData.Clear();
            return RedirectToAction("LoginView");

        }


        // Author Peter Szadurski
        // This is the kithen partialview that gets rendered on the accounts page
        [HttpPost]
        public ActionResult KitchenPartialView(List<SerializableKitchen> kitchens)
        {
            int userId = HttpContext.Session.GetInt32("UserId").GetValueOrDefault();
            ViewBag.UserId = userId;
            var user = Client.GetSerializableUser(userId);
            bool confirmed = user.IsConfirmed;
            
            // Viewbags needed to check user diet and if the user is confirmed
            ViewBag.IsVegan = user.Vegan.ToString();
            ViewBag.IsVeg = user.Vegetarian.ToString();
            ViewBag.IsConfirmed = confirmed;
            
            // Non-HardCoded StorageList
            ViewBag.StoragesList = Client.GetStorages();
            KitchenModel kM = new KitchenModel();
            kM.Kitchens = kitchens;
            kM.Storages = Client.GetStorages().ToList();



            return PartialView("UserKitchenPartialView", kM);
        }

        // Author Peter Szadurski
        // Deprecated, multi-kitchens are not currently accessable
        [HttpPost]
        public ActionResult AddEditKitchen(KitchenUser kitchenUser)
        {
            int userId = HttpContext.Session.GetInt32("UserId").GetValueOrDefault();
            try
            {
                Client.AddKitchen(userId, kitchenUser.kitchen.Name);
                ViewBag.UserId = userId;

                List<SerializableKitchen> kitchens = Client.GetKitchens(userId).ToList();
            }
            catch
            {
                Debug.WriteLine("eat failed");
            }
            UserSessionModel user = new UserSessionModel
            {
                Id = HttpContext.Session.GetInt32("UserId").GetValueOrDefault(),
                Username = HttpContext.Session.GetString("Username")
            };
            return AccountView(user);
        }

        // Author Peter Szadurski
        // Deprecated, multi-kitchens are not currently accessable
        [HttpGet]
        public ActionResult AddEditKitchenView(KitchenUser kitchenUser)
        {
            ViewBag.UserId = kitchenUser.UserId;

            UserSessionModel user = new UserSessionModel { Id = HttpContext.Session.GetInt32("UserId").GetValueOrDefault(), Username = HttpContext.Session.GetString("Username") };

            return AccountView(user);

        }

        //Author: Peter
        /// <summary>
        /// Used for either submitting food adding or food editing
        /// </summary>
        /// <param name="foodKitchen"></param>
        /// <returns>Brigns the user back to the account view</returns>
        [HttpPost]
        public ActionResult AddEditFood(FoodKitchen foodKitchen)
        {

            int userId = HttpContext.Session.GetInt32("UserId").GetValueOrDefault();
            ViewBag.UserId = userId;


            try
            {
                if (foodKitchen.Food.Id == 0) // add food
                {
                    Client.AddFoodComplete(userId, foodKitchen.KitchenId, foodKitchen.Food.Name, foodKitchen.Food.Storage.ToString(), foodKitchen.Food.ExpiryDate, foodKitchen.Food.Quantity, foodKitchen.Food.QuantityClassifier, foodKitchen.Food.Vegan, foodKitchen.Food.Vegetarian, new string[0], new string[0], foodKitchen.Food.Favourite);
                }
                else // edit food
                {
                    Client.EditFood(foodKitchen.Food.Id, foodKitchen.Food.Name, foodKitchen.Food.Quantity, foodKitchen.Food.QuantityClassifier, foodKitchen.Food.Storage.ToString(), foodKitchen.Food.ExpiryDate);
                }
            }
            catch
            {
            }


            UserSessionModel user = new UserSessionModel
            {
                Id = HttpContext.Session.GetInt32("UserId").GetValueOrDefault(),
                Username = HttpContext.Session.GetString("Username")
            };
            return AccountView(user);

        }

        // Author: Peter
        // Partial view used for the recipe search modal
        [HttpGet]
        public ActionResult RecipeSearchView()
        {
            int Id = HttpContext.Session.GetInt32("UserId").GetValueOrDefault();
            int kId = Client.GetKitchens(Id).FirstOrDefault().Id;
            ViewBag.KitchenId = kId;

            return PartialView("_RecipeSearchPartial");
        }

        // Author: Peter
        // Method to used search recipes and return formated results
        [HttpPost]
        public ActionResult SearchRecipes(string search)
        {
            search = System.Web.HttpUtility.UrlEncode(search);
            int Id = HttpContext.Session.GetInt32("UserId").GetValueOrDefault();
            List<SerializableRecipeModel> recs = new List<SerializableRecipeModel>();
            try
            {
                recs = Client.SearchRecipesUser(search, 100, Id).ToList();
            }
            // Used to just prevent the app from crashing when Edamam is out of results, Partial page already handles null results
            catch { }
            return PartialView("_RecipeResultView", recs.ToArray()) ;
        }

        // Author: Peter
        // Method to auto used search recipes and return formated results
        [HttpPost]
        public ActionResult FeelingLucky(string search)
        {


            search = System.Web.HttpUtility.UrlEncode(search);
            int Id = HttpContext.Session.GetInt32("UserId").GetValueOrDefault();
            List<SerializableRecipeModel> recs = new List<SerializableRecipeModel>();
            try
            {
                recs = Client.FeelingLuckyUser(100, Id).ToList();
            }
            // Used to just prevent the app from crashing when Edamam is out of results, Partial page already handles null results
            catch
            {

            }
            return PartialView("_RecipeResultView", recs.ToArray());
        }
        // Author: Peter
        // Method used by the shopping list modal
        [HttpPost]
        public ActionResult ShoppingList()
        {
            int Id = HttpContext.Session.GetInt32("UserId").GetValueOrDefault();
            var kitchen = Client.GetKitchens(Id).FirstOrDefault();
            ViewBag.kID = kitchen.Id;
            var sL = kitchen.Inventory.OrderByDescending(x => x.Favourite).ToList();
            return PartialView("_ShoppingListPartial",sL);
        }

        // Author: Peter
        // Method used by to save the shopping list. Uses Ajax forms in order to just update the modal instead of refreshing the whole page
        [HttpPost]
        async public Task<string> SetShoppingList(List<SerializableFood> shoppingList, IFormCollection form)
        {
            int Id = HttpContext.Session.GetInt32("UserId").GetValueOrDefault();
            int kID = Convert.ToInt32(form["kID"]);
            var sL = Client.GetKitchens(Id).FirstOrDefault().Inventory.OrderByDescending(x => x.Favourite).ToList();

            await Client.EditShoppingListMultipleAsync(kID, shoppingList.ToArray());

            return "Shopping List Updated";
        }

        // Author: Peter
        // Method used by the Add/Edit food modal

        [HttpGet]
        public ActionResult AddEditFoodView(int kId, int fId)
        {
            ViewBag.KitchenId = kId;
            FoodKitchen foodKitchen = new FoodKitchen();
            foodKitchen.KitchenId = kId;
            ViewBag.StorageTypesList = Client.GetStorages();
            List<string> qClassifiers = new List<string> { "item", "g", "mg", "kg",
                "mL", "L", "oz", "fl. oz.", "gallon", "lb" };
            ViewBag.Classifier = qClassifiers;
            ViewBag.VegVegan = DropdownModel.VegVeganDropdown();
            if (fId != 0)
            {
                foodKitchen.Food = Client.GetFood(fId);

            }
            else
            {
                foodKitchen.Food = new Food();
            }
            return PartialView("AddEditFoodPartial", foodKitchen);
        }

        // Author: Peter
        [HttpPost]
        public ActionResult EatFood(Food food)
        {
            try
            {

                Client.EatFood(food.Id, food.Quantity);


            }
            catch
            {
                Debug.WriteLine("eat failed");
            }
            ViewBag.UserId = HttpContext.Session.GetInt32("UserId").GetValueOrDefault();


            UserSessionModel user = new UserSessionModel
            {
                Id = HttpContext.Session.GetInt32("UserId").GetValueOrDefault(),
                Username = HttpContext.Session.GetString("Username")
            };
            return AccountView(user);
        }

        // Author: Peter
        // Changes passowrd, uses ajax for faster validation
        [HttpPost]
        public string ChangePassword(int userId,string old, string newPass)
        {
            SerializableUser user = Client.GetSerializableUser(userId);

            
            if(Client.LoginAccount(user.Email, old).Id != -1)
            {
                Client.EditUserPassword(userId, newPass);
                return "Password Changed!";
            }
            else
            {
                return "Old password does not match.";
            }
        }

        // Author: Peter
        // Method used by eat food modal
        [HttpGet]
        public ActionResult EatFoodView(int fId)
        {
            var food = Client.GetFood(fId);

            return PartialView("EatFoodPartial", food);
        }

        // Author: Peter
        // It deltes the food matching the id
        [HttpDelete]
        public ActionResult DeleteFood(int fId)
        {
            try
            {
                Client.RemoveItem(fId);
            }
            catch
            {
                Debug.WriteLine("remove failed");
            }

            // Setup for UserKitchenPartialView
            int userId = HttpContext.Session.GetInt32("UserId").GetValueOrDefault();
            ViewBag.UserId = userId;
            var user = Client.GetSerializableUser(userId);
            ViewBag.IsVegan = user.Vegan.ToString();
            ViewBag.IsVeg = user.Vegetarian.ToString();
            ViewBag.IsConfirmed = user.IsConfirmed;

            KitchenModel kM = new KitchenModel();




            return PartialView("UserKitchenPartialView", user.Kitchens.ToList());
        }

        // Author: Peter
        // Returns Open Food Facts barcode data
        [HttpGet]
        public string GetBarcodeData(string barcode)
        {
            // chop off leading zeros

            //  barcode = barcode.TrimStart('0');
            string test = Client.GetBarcodeData(barcode);
            return test;
        }

    }


}
