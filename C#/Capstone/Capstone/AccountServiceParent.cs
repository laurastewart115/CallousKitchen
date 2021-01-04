using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Capstone.Apis;
using Capstone.Classes;

namespace Capstone
{
    //Authors: Kevin and Peter
    //This is where all the methods that the app and the web app use stay
    //We were initially going to use different services for different things but we found out that would be a pain so now there's just one
    public class AccountServiceParent
    {
        //private  static HttpClient client = ApiHelper.ApiClient;


        //The entityframework database
        private CallousHipposDb db = new CallousHipposDb();

        public object KeyDerivationPrf { get; private set; }

        //Author: Mostly Peter with some help from Kevin
        //Returns the created user or an error message
        public SerializableUser CreateAccount(string email, string pass)
        {
            if (IsValidEmail(email))
            {
                if (db.Users.Where(x => x.Email == email).Count() != 0)
                {
                    //If the email exists, return -1
                    return new SerializableUser(-1);
                }
                else
                {
                    User user = new User(email, pass);
                    Guid guid = Guid.NewGuid();
                    // check for guid collisions, very unlikely 
                    while (db.Users.Where(x => x.EmailConfirmKey == guid).Count() > 0)
                    {
                        guid = Guid.NewGuid();
                    }
                    user.EmailConfirmKey = guid;
                    user.IsConfirmed = false;

                    User returnedUser = db.Users.Add(user);
                    returnedUser.Kitchens = new List<Kitchen>();
                    returnedUser.Kitchens.Add(db.Kitchens.Add(new Kitchen { Name = "Kitchen" }));
                    db.SaveChanges();
                    EmailClient emailClient = new EmailClient();
                    emailClient.SendConfirmEmail(user.Email, user.EmailConfirmKey);
                    return new SerializableUser(returnedUser);
                }
            }
            //If the email is not valid send -2
            return new SerializableUser(-2);
        }

        //Author: Kevin and Peter
        //Return the user with the correct credentials or -1 if not found
        public SerializableUser LoginAccount(string email, string pass)
        {
            //Firstordefault returns null if not found and the constructor returns -1 if null
            return (new SerializableUser(db.Users.Where(x => x.Email == email && x.Password == pass).FirstOrDefault()));
        }

        public string ConfirmEmail(string key)
        {
            Guid keyGuid;
            Guid.TryParse(key, out keyGuid);
            Guid blankGuid = new Guid();
            if (keyGuid != blankGuid)
            {
                var user = db.Users.Where(x => x.EmailConfirmKey == keyGuid).FirstOrDefault();
                user.IsConfirmed = true;
                user.EmailConfirmKey = blankGuid; // can't set a null guid, closest thing
                db.SaveChanges();
                return "Success";
            }

            return "Failed";
        }

        //Testing method, remove in production
        public bool AnotherTest()
        {
            DemoDb("67X7C@&Aej*hS%");
            return true;
        }

        //Author: Kevin Gadelha
        //Returns true if success
        public async Task<bool> EditUserDietaryRestrictions(int id, bool vegan, bool vegetarian, List<string> allergies)
        {
            //Update the user's stuff
            var user = db.Users.Where(x => x.Id == id).FirstOrDefault();
            user.Vegan = vegan;
            user.Vegetarian = vegetarian;
            user.Allergies = string.Join("|", allergies);
            await db.SaveChangesAsync();
            return true;
        }

        //Author:Kevin Gadelha
        public async Task<bool> EditUserPassword(int id, string password)
        {
            //Update the password
            var user = db.Users.Where(x => x.Id == id).FirstOrDefault();
            user.Password = password;
            await db.SaveChangesAsync();
            return true;
        }

        //Author:Kevin Gadelha
        //Return false if exists
        public async Task<bool> EditUserEmail(int id, string email)
        {
            if (db.Users.Where(x => x.Email == email).Count() != 0)
            {
                return false;
            }
            else
            {
                //Update email
                var user = db.Users.Where(x => x.Id == id).FirstOrDefault();
                user.Email = email;
                await db.SaveChangesAsync();
                return true;
            }
        }

        //Author: Peter
        //Adds a kitchen for a user
        public int AddKitchen(int userId, string name)
        {
            Kitchen kitchen = db.Kitchens.Add(new Kitchen() { Name = name });
            db.Users.Where(x => x.Id == userId).FirstOrDefault().Kitchens.Add(kitchen);
            db.SaveChanges();
            return kitchen.Id;
        }
        //Author: Kevin and Peter
        //Gets the inventory for a user's first kitchen, which is the only one
        //Shorthand method
        public List<SerializableFood> GetPrimaryInventory(int userId)
        {
            return db.Users.Where(x => x.Id == userId).FirstOrDefault()?.Kitchens?.FirstOrDefault()?.Inventory?
                .Select(o => new SerializableFood(o)).ToList();
        }
        //Author: Kevin and Peter
        //Gets all the kitchens a user has
        public List<SerializableKitchen> GetKitchens(int userId)
        {
            return db.Users.Where(x => x.Id == userId).FirstOrDefault()?.Kitchens?
                .Select(o => new SerializableKitchen(o)).ToList();
        }
        //Author: Kevin and Peter
        //Gets the inventory for a specified kitchen
        public List<SerializableFood> GetInventory(int kitchenId)
        {
            return db.Kitchens.Where(x => x.Id == kitchenId).FirstOrDefault()?.Inventory?
                .Select(o => new SerializableFood(o)).ToList();
        }

        //Author: Peter
        // Used to get all the storage enums so I don't have to hardcode it on the Web App
        public List<Storage> GetStorages()
        {
            List<Storage> storages = new List<Storage>();
            storages.Add(Storage.Fridge);
            storages.Add(Storage.Freezer);
            storages.Add(Storage.Pantry);
            storages.Add(Storage.Cupboard);
            storages.Add(Storage.Cellar);
            storages.Add(Storage.Other);

            return storages;
        }

        //Author: Kevin Gadelha
        //Gets the user info from their id
        public SerializableUser GetSerializableUser(int id)
        {
            SerializableUser user = new SerializableUser(db.Users.Where(x => x.Id == id).FirstOrDefault());
            return user;
        }

        public string DoesThisEvenWork()
        {

            return "yes";
        }

        public User GetUser(int id)
        {
            return (db.Users.Where(x => x.Id == id).FirstOrDefault());
        }

        public Task<string> GetBarcodeData(string barcode)
        {
            OpenFoodFacts openFoodFacts = new OpenFoodFacts();
            return openFoodFacts.LoadBarcode(barcode);
        }

        public Task<Models.SerializableFoodFactsProductModel> GetAllOpenFoodFacts(string barcode)
        {
            //ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            OpenFoodFacts openFoodFacts = new OpenFoodFacts();
            return openFoodFacts.LoadAllBarcodeData(barcode);

        }

        //From when the shopping list was more limited
        public List<string> GenerateShoppingList(int kichenId)
        {
            List<string> shoppingList;
            shoppingList = db.Kitchens.Where(x => x.Id == kichenId).FirstOrDefault().Inventory.Where(i => i.Favourite == true && i.Quantity < 3).Select(n => n.Name).ToList();
            return shoppingList;
        }

        //Author: Kevin, Modified by Peter
        //Search recipes from user's account info
        public Models.SerializableRecipeModel[] SearchRecipesUser(string search, int count, int userId)
        {
            var user = db.Users.Where(x => x.Id == userId).FirstOrDefault();
            //The diets include things that can't be in the recipe or recipe requirements
            List<string> diets = new List<string>();
            if (!String.IsNullOrWhiteSpace(user.Allergies))
            {
                diets = user.Allergies.Split('|').ToList();
            }
            if (user.Vegan)
                diets.Add("vegan");
            if (user.Vegetarian)
                diets.Add("vegetarian");
            //Use ranked by default
            return SearchRecipesRanked(search, count, diets, user.Kitchens.FirstOrDefault().Id);
        }

        // Author Peter Szadurski
        public Task<Models.SerializableRecipeModel[]> SearchRecipes(string search, int count, List<string> diets)
        {
            RecipeApi recipeApi = new RecipeApi();
            return recipeApi.GetRecipe(search, count, diets.ToArray());
        }
        // Author Peter Szadurski

        public Models.SerializableRecipeModel[] SearchRecipesRanked(string search, int count, List<string> diets, int kitchenId)
        {
            RecipeApi recipeApi = new RecipeApi();
            Models.SerializableRecipeModel[] recipes = recipeApi.GetRecipe(search, count, diets.ToArray()).Result;
            List<Food> foods = db.Kitchens.Where(x => x.Id == kitchenId).FirstOrDefault().Inventory;

            foreach (var r in recipes)
            {
                /*foreach (var i in r.Ingredients)
                {
                    foreach (var f in foods)
                    {
                        if (f.Name.ToLower() == i.Name.ToLower())
                        {
                            i.Score = 2;
                            break;
                        }
                        else if (f.Name.ToLower().Contains(i.Name.ToLower()))
                        {
                            i.Score = 1;
                        }
                    }
                    r.Score += i.Score;
                }
                */
                foreach (var i in r.EdamanIngredients)
                {
                    // Keeps the current best score for an ingredient
                    int tempScore = 0;
                    // Ranks Score based off normalized ingredients match
                    // Full match is worth 2 points, partial partial is worth 1 point.
                    // If the food is at least a partial match, expiry rate will be used to increase score
                    // it will increase in score by 1 point of it will expire in 3 days, and add 1 for each day below 3, capping out at +3 extra score.
                    foreach (var f in foods)
                    {

                        if (f.Name.ToLower() == i.Name.ToLower())
                        {
                            i.Score = 2;

                        }
                        else if (i.Score != 2 && (f.Name.ToLower().Contains(i.Name.ToLower()) || i.Name.ToLower().Contains(f.Name.ToLower())))
                        {
                            i.Score = 1;
                        }
                        if (i.Score > 0 && f.ExpiryDate.HasValue)
                        {
                            DateTime date1 = f.ExpiryDate.Value;
                            DateTime date2 = DateTime.Now.AddDays(3);
                            TimeSpan time = date2 - date1;
                            if (time.Days > 0)
                                i.Score += time.Days;
                        }
                        if (i.Score > tempScore)
                        {
                            tempScore = i.Score;
                        }

                    }
                    // Adds score to the recipe
                    r.Score += tempScore;
                }

            }
            recipes = recipes.OrderByDescending(x => x.Score).ToArray();

            return recipes;
        }

        //Author: Peter modified by Kevin
        public Models.SerializableRecipeModel[] FeelingLucky(int count, List<string> diets, int kitchenId)
        {
            string searchString = "";
            List<Food> foods = db.Kitchens.Where(x => x.Id == kitchenId).FirstOrDefault().Inventory;
            Models.SerializableRecipeModel[] recipes = null;
            int take = 5;
            int itemNumber = 1;
            while (recipes == null || recipes.Count() == 0)
            {
                //Try with several items and decrease the amount on each failure
                if (take > 0)
                {
                    var expiringFoods = foods.OrderByDescending(x => x.ExpiryDate).Take(take).ToList();
                    foreach (var f in expiringFoods)
                    {
                        searchString += f.Name + "+";
                    }
                    recipes = SearchRecipesRanked(searchString, count, diets, kitchenId);
                    take--;
                }
                //Try all individual items
                else if (itemNumber < foods.Count)
                {
                    recipes = SearchRecipesRanked(foods[itemNumber].Name, count, diets, kitchenId);
                    itemNumber++;
                }
                //If all else fails, give up
                else
                {
                    break;
                }
            }
            return recipes ?? new Models.SerializableRecipeModel[0];
        }

        //Author:Kevin, modified by Peter
        //Feeling lucky  but taking into account a user's dietary requirements
        public Models.SerializableRecipeModel[] FeelingLuckyUser(int count, int userId)
        {
            var user = db.Users.Where(x => x.Id == userId).FirstOrDefault();
            List<string> diets = new List<string>();
            if (!String.IsNullOrWhiteSpace(user.Allergies))
            {
                diets = user.Allergies.Split('|').ToList();
            }
            if (user.Vegan)
                diets.Add("vegan");
            if (user.Vegetarian)
                diets.Add("vegetarian");
            return FeelingLucky(count, diets, user.Kitchens.FirstOrDefault().Id);
        }

        //Author: Kevin and Peter
        //Add food to a user's kitchen
        public async Task<bool> AddFood(int userId, int kitchenId, string name, int quantity, DateTime? expiryDate)
        {
            //Don't let user add food if they're not confirmed
            if (!db.Users.Where(x => x.Id == userId).FirstOrDefault().IsConfirmed)
                return false;
            //Some default values are added
            db.Kitchens.Where(x => x.Id == kitchenId).FirstOrDefault().Inventory
                .Add(db.Foods.Add(new Food() { Name = name, Quantity = quantity, ExpiryDate = expiryDate, Vegetarian = -1, Vegan = -1, Calories = -1 }));
            await db.SaveChangesAsync();
            return true;
        }
        //Author:Kevin
        //Same as the above method but with more parameters
        public async Task<bool> AddFoodComplete(int userId, int kitchenId, string name, string storage, DateTime? expiryDate, double quantity, string quantityClassifier, int vegan, int vegetarian, List<string> ingredients, List<string> traces, bool favourite)
        {
            //Don't let user add food if they're not confirmed
            if (!db.Users.Where(x => x.Id == userId).FirstOrDefault().IsConfirmed)
                return false;
            var food = new Food(name, (Storage)Enum.Parse(typeof(Storage), storage, true), expiryDate, quantity, quantityClassifier, vegan, vegetarian, ingredients, traces, favourite);
            db.Kitchens.Where(x => x.Id == kitchenId).FirstOrDefault().Inventory
                .Add(db.Foods.Add(food));
            await db.SaveChangesAsync();
            return true;
        }
        //Author:Kevin and Peter
        //Simplified edit
        public async Task<bool> EatFood(int id, double quantity)
        {
            var item = db.Foods.Where(x => x.Id == id).FirstOrDefault();
            //If the food has run out and it's not a favorite, remove it
            if (quantity == 0 && !item.Favourite)
            {
                db.Foods.Remove(item);
            }
            else
            {
                //Update the quantity
                item.Quantity = quantity;
            }
            await db.SaveChangesAsync();
            return true;
        }
        //Author:Kevin and Peter
        //Self explanatory
        public async Task<bool> EditFood(int id, string name, double quantity, string quantityClassifier, string storage, DateTime? expiryDate)
        {
            var item = db.Foods.Where(x => x.Id == id).FirstOrDefault();
            //Assume that if the user is editing the food they don't want to delete by having the quantity be zero
            item.Name = name;
            item.Quantity = quantity;
            item.QuantityClassifier = quantityClassifier;
            //Convert the string parameter to an enum
            item.Storage = (Storage)Enum.Parse(typeof(Storage), storage, true);
            //Assume that the user editing the food means they are resetting their initial quantity
            item.InitialQuantity = quantity;
            item.ExpiryDate = expiryDate;
            await db.SaveChangesAsync();
            return true;
        }
        //Author:Kevin
        //Update whether food is favorite
        public async Task<bool> FavouriteFood(int foodId, bool favourite)
        {
            var item = db.Foods.Where(x => x.Id == foodId).FirstOrDefault();
            item.Favourite = favourite;
            await db.SaveChangesAsync();
            return true;
        }
        //Author:Kevin
        //Update whether food is on the user's shopping list
        public async Task<bool> ShoppingListFood(int foodId, bool onShoppingList)
        {
            var item = db.Foods.Where(x => x.Id == foodId).FirstOrDefault();
            item.OnShoppingList = onShoppingList;
            await db.SaveChangesAsync();
            return true;
        }
        public async Task<bool> EditShoppingListMultiple(int kId, List<SerializableFood> shoppingList)
        {
            var foods = db.Kitchens.Where(x => x.Id == kId).FirstOrDefault()?.Inventory;
            for (int i = 0; i < foods.Count(); i++)
            {
                foods[i].OnShoppingList = shoppingList[i].OnShoppingList;
            }
            await db.SaveChangesAsync();
            return true;
        }
        //Author: Kevin
        //Take everything off the shopping list
        //Used when the user's done shopping
        public async Task<bool> ClearShoppingList(int kitchenId)
        {
            var kitchen = db.Kitchens.Where(x => x.Id == kitchenId).FirstOrDefault();
            foreach (Food food in kitchen.Inventory)
            {
                food.OnShoppingList = false;
            }
            await db.SaveChangesAsync();
            return true;
        }
        //Author: Kevin and Peter
        //Delete the food
        public async Task<bool> RemoveItem(int id)
        {
            var item = db.Foods.Where(x => x.Id == id).FirstOrDefault();
            db.Foods.Remove(item);
            await db.SaveChangesAsync();
            return true;
        }

        //I'm just going to leave this test method here since I keep needing it
        public List<SerializableUser> Test()
        {
            return db.Users.ToList().Select(o => new SerializableUser(o)).ToList();
        }

        //returns true if email is valid, false if invalid
        public bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            //Normalize the domain
            try
            {
                //checks an email for matches with regular expression
                //in this case it looks for "@" and domain name
                //if there were matches - delegates domain name proccessing to DomainMapper function
                //replaces email part that's complies with regex (@gmail.com) with DomainMapper return string

                email = Regex.Replace(email, @"(@)(.+)$", DomainMapper,
                                      RegexOptions.None, TimeSpan.FromMilliseconds(200));

                // Examines the domain part of the email and normalizes it.
                string DomainMapper(Match match)
                {
                    // Use IdnMapping class to convert Unicode domain names.
                    var idn = new IdnMapping();

                    // Pull out and process domain name (throws ArgumentException on invalid)
                    //Domain part of an email can have international characters and they are converted to PunyCode
                    //bücher.com --> xn--bcher-kva.com
                    var domainName = idn.GetAscii(match.Groups[2].Value);

                    //I am not sure how it works, but Group[1] (not 0) is linked to first group (@)
                    //Group[2] is linked to domain name


                    //return "@" and proccessed domain name
                    return match.Groups[1].Value + domainName;
                }
            }
            catch (RegexMatchTimeoutException e)
            {
                return false;
            }
            catch (ArgumentException e)
            {
                return false;
            }


            //check if normalized email is matched to regex which checks for correct email address
            try
            {
                return Regex.IsMatch(email,
                    @"^(?("")("".+?(?<!\\)""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z])@))" +
                    @"(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-0-9a-z]*[0-9a-z]*\.)+[a-z0-9][\-a-z0-9]{0,22}[a-z0-9]))$",
                    RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250));
            }
            catch (RegexMatchTimeoutException)
            {
                return false;
            }
        }
        //Never used since it's we can't figure out how to hash a password the same way in android
        public string HashPassword(string password)
        {
            //I am hashing using Rcf2898DeriveBytes method


            //There is another algorithm used in ASP.NET Core - PBKDF2

            //it detects the operating system and tries to choose optimal implementation
            //according to microsoft it offers 10x througput compared to  Rcf2898DeriveBytes
            //it supports more hashing algorithms - HMACSHA256, HMACSHA512



            //creating a salt value which is later appended to a password
            byte[] salt;
            new RNGCryptoServiceProvider().GetBytes(salt = new byte[16]);

            //use password+salt combination to get hash with help of Rcf2898DeriveBytes method
            var hashvalue = new Rfc2898DeriveBytes(password, salt, 10000);
            byte[] hash = hashvalue.GetBytes(20);


            //combine salt+hash (it will be needed to verify a user password from his input)
            byte[] hashBytes = new byte[36];
            Array.Copy(salt, 0, hashBytes, 0, 16);
            Array.Copy(hash, 0, hashBytes, 16, 20);

            //final (combined) password which will be saved into the database
            string finalPassword = Convert.ToBase64String(hashBytes);
            return finalPassword;
        }

        public bool IsValidPassword(string databaseHashPassword, string userPassword)
        {
            //get hashBytes from database
            byte[] hashBytes = Convert.FromBase64String(databaseHashPassword);
            byte[] salt = new byte[16];
            //extract salt from combined password
            Array.Copy(hashBytes, 0, salt, 0, 16);

            //get hashvalue from input and salt
            var hashvalue = new Rfc2898DeriveBytes(userPassword, salt, 10000);
            byte[] hash = hashvalue.GetBytes(20);


            //compare hashvalue in database to hashvalue generated from salt and user password
            for (int i = 0; i < 20; i++)
            {
                if (hashBytes[i + 16] != hash[i])
                {
                    return false;
                }
            }
            return true;
        }
        public Food GetFood(int id)
        {
            return db.Foods.Where(x => x.Id == id).FirstOrDefault();
        }


        // Author: Peter
        // Sets up the database with demo data, it has a password so it can't be called by accident
        public bool DemoDb(string pass)
        {
            if (pass != "DiscoFever")
            {
                return false;
            }
            else
            {
                // Truncate
                db.CaloriesInDays.RemoveRange(db.CaloriesInDays);
                db.Foods.RemoveRange(db.Foods);
                db.Kitchens.RemoveRange(db.Kitchens);
                db.Foods.RemoveRange(db.Foods);
                db.Users.RemoveRange(db.Users);
                db.SaveChanges();
                db.Database.ExecuteSqlCommand("DBCC CHECKIDENT (Users, RESEED, 1);");
                db.Database.ExecuteSqlCommand("DBCC CHECKIDENT (Foods, RESEED, 1);");
                db.Database.ExecuteSqlCommand("DBCC CHECKIDENT (Kitchens, RESEED, 1);");
                db.Database.ExecuteSqlCommand("DBCC CHECKIDENT (CaloriesInDays, RESEED, 1);");
                db.SaveChanges();

                //Create User
                User demoUser = new User("demo@example.com", "pass");
                demoUser.EmailConfirmKey = new Guid();
                demoUser.IsConfirmed = true;
                db.Users.Add(demoUser);
                db.SaveChanges();

                // Kitchens
                demoUser.Kitchens = new List<Kitchen>();
                demoUser.Kitchens.Add(db.Kitchens.Add(new Kitchen { Name = "Kitchen", Inventory = new List<Food>() }));
                db.SaveChanges();

                // Add Food

                var kitch = demoUser.Kitchens.FirstOrDefault().Inventory;

                kitch.Add(db.Foods.Add(new Food() { Name = "Butter", Quantity = 200, ExpiryDate = DateTime.Now.AddDays(7), Vegetarian = 1, Vegan = 0, Calories = -1, Storage = Storage.Fridge, QuantityClassifier = "g", Ingredients = "", Traces = "" }));
                kitch.Add(db.Foods.Add(new Food() { Name = "Garlic", Quantity = 7, ExpiryDate = DateTime.Now.AddDays(3), Vegetarian = 1, Vegan = 1, Calories = -1, Storage = Storage.Fridge, QuantityClassifier = "item", Ingredients = "", Traces = "" }));
                kitch.Add(db.Foods.Add(new Food() { Name = "Steak", Quantity = 12, ExpiryDate = DateTime.Now.AddDays(3), Vegetarian = 0, Vegan = 0, Calories = -1, Storage = Storage.Fridge, QuantityClassifier = "oz", Ingredients = "", Traces = "" }));
                kitch.Add(db.Foods.Add(new Food() { Name = "Egg", Quantity = 11, ExpiryDate = DateTime.Now.AddDays(14), Vegetarian = 1, Vegan = 0, Calories = -1, Storage = Storage.Fridge, Favourite = true, QuantityClassifier = "item", Ingredients = "", Traces = "" }));
                kitch.Add(db.Foods.Add(new Food() { Name = "Apple", Quantity = 3, ExpiryDate = DateTime.Now.AddDays(4), Vegetarian = 1, Vegan = 1, Calories = -1, Storage = Storage.Pantry, QuantityClassifier = "item", Ingredients = "", Traces = "" }));
                kitch.Add(db.Foods.Add(new Food() { Name = "Salt", Quantity = 10000, ExpiryDate = null, Vegetarian = 1, Vegan = 1, Calories = -1, Storage = Storage.Cupboard, Favourite = true, QuantityClassifier = "g", Ingredients = "", Traces = "" }));
                kitch.Add(db.Foods.Add(new Food() { Name = "Pepper", Quantity = 1256, ExpiryDate = null, Vegetarian = 1, Vegan = 1, Calories = -1, Storage = Storage.Cupboard, Favourite = true, QuantityClassifier = "g", Ingredients = "", Traces = "" }));
                kitch.Add(db.Foods.Add(new Food() { Name = "Cinnamon", Quantity = 300, ExpiryDate = null, Vegetarian = 1, Vegan = 1, Calories = -1, Storage = Storage.Pantry, Favourite = true, QuantityClassifier = "g", Ingredients = "", Traces = "" }));
                kitch.Add(db.Foods.Add(new Food() { Name = "Heavy Cream", Quantity = 0.5, ExpiryDate = DateTime.Now.AddDays(20), Vegetarian = 1, Vegan = 0, Calories = -1, Storage = Storage.Fridge, Favourite = true, QuantityClassifier = "L", Ingredients = "", Traces = "" }));
                kitch.Add(db.Foods.Add(new Food() { Name = "Whole Milk", Quantity = 2, ExpiryDate = DateTime.Now.AddDays(14), Vegetarian = 1, Vegan = 0, Calories = -1, Storage = Storage.Fridge, Favourite = false, QuantityClassifier = "L", Ingredients = "", Traces = "" }));
                kitch.Add(db.Foods.Add(new Food() { Name = "Almond Milk", Quantity = 2, ExpiryDate = DateTime.Now.AddDays(14), Vegetarian = 1, Vegan = 1, Calories = -1, Storage = Storage.Fridge, Favourite = false, QuantityClassifier = "L", Ingredients = "", Traces = "" }));
                kitch.Add(db.Foods.Add(new Food() { Name = "Coldbrew Coffee", Quantity = 1, ExpiryDate = DateTime.Now.AddDays(14), Vegetarian = 1, Vegan = 1, Calories = -1, Storage = Storage.Fridge, Favourite = true, QuantityClassifier = "L", Ingredients = "", Traces = "" }));
                kitch.Add(db.Foods.Add(new Food() { Name = "Ground Beef", Quantity = 300, ExpiryDate = DateTime.Now.AddDays(25), Vegetarian = 0, Vegan = 0, Calories = -1, Storage = Storage.Freezer, Favourite = true, QuantityClassifier = "lb", Ingredients = "", Traces = "" }));
                kitch.Add(db.Foods.Add(new Food() { Name = "Orange Juice", Quantity = 1, ExpiryDate = DateTime.Now.AddDays(25), Vegetarian = 1, Vegan = 1, Calories = -1, Storage = Storage.Fridge, Favourite = true, QuantityClassifier = "gallon", Ingredients = "", Traces = "" }));
                kitch.Add(db.Foods.Add(new Food() { Name = "Red Wine", Quantity = 750, ExpiryDate = null, Vegetarian = 1, Vegan = 1, Calories = -1, Storage = Storage.Cellar, Favourite = true, QuantityClassifier = "mL", Ingredients = "", Traces = "" }));
                kitch.Add(db.Foods.Add(new Food() { Name = "Canned Chilli", Quantity = 3, ExpiryDate = null, Vegetarian = 1, Vegan = 1, Calories = -1, Storage = Storage.Pantry, Favourite = true, QuantityClassifier = "item", Ingredients = "", Traces = "" }));
                kitch.Add(db.Foods.Add(new Food() { Name = "Canned Beans", Quantity = 3, ExpiryDate = null, Vegetarian = 1, Vegan = 1, Calories = -1, Storage = Storage.Pantry, Favourite = true, QuantityClassifier = "item", Ingredients = "", Traces = "" }));


                db.SaveChanges();
                return true;
            }
        }
    }
}
