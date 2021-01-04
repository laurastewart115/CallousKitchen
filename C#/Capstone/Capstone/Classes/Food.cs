using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace Capstone.Classes
{
	//Author: Kevin Gadelha
	//Represents a food item a user has
	public class Food
	{
		public Food()
		{
		}

		public Food(string name, DateTime? expiryDate, double quantity, int vegan, int vegetarian, List<string> ingredients, List<string> traces, int calories)
		{
			Name = name;
			ExpiryDate = expiryDate;
			Quantity = quantity;
			Vegan = vegan;
			Vegetarian = vegetarian;
			//Convert from a list to a delimitted string
			//Ingredients don't have piping symbols so this is safe
			Ingredients = string.Join("|",ingredients);
			Traces = string.Join("|", traces);
			Calories = calories;
		}

		public Food(string name, Storage storage, DateTime? expiryDate, double quantity, string quantityClassifier, int vegan, int vegetarian, List<string> ingredients, List<string> traces, bool favourite)
		{
			Name = name;
			Storage = storage;
			ExpiryDate = expiryDate;
			Quantity = quantity;
			InitialQuantity = quantity;
			QuantityClassifier = quantityClassifier;
			InitialQuantityClassifier = quantityClassifier;
			Vegan = vegan;
			Vegetarian = vegetarian;
			Ingredients = string.Join("|", ingredients);
			Traces = string.Join("|", traces);
			Favourite = favourite;
		}

		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }
		public string Name { get; set; }
		public Storage Storage { get; set; }
		//Explicitly declare expiry date to be nullable
		public Nullable<DateTime> ExpiryDate { get; set; }
		//Was supposed to be used for expiry date estimation
		//If the normal expirydate was null, this was supposed to be the fallback
		public Nullable<DateTime> CalculatedExpiryDate { get; set; }
		public double Quantity { get; set; }
		//I was too lazy to make this an enum in case we changed it later
		public string QuantityClassifier { get; set; }
		//Had this just in case we needed it for something, but ended up never using it
		public double InitialQuantity { get; set; }
		public string InitialQuantityClassifier { get; set; }
		//These are ints because I also need to track unknown values
		//1 is true, 0 is false and -1 is unknown
		//This is pretty intuitive for a developer
		public int Vegan { get; set; }
		public int Vegetarian { get; set; }
		//While it's not pretty, the suggested course of action I found online was to use a delimited string to save a list of strings
		//This is because entityframework is too dumb to generate a table for a list of strings
		public string Ingredients { get; set; }
		public string Traces { get; set; }
		//Ended up not doing calorie stuff
		public int Calories { get; set; }
		public bool Favourite { get; set; }
		public bool OnShoppingList { get; set; }
	}

	//Food can be stored in any of these
	public enum Storage
	{
		Fridge,
		Freezer,
		Pantry,
		Cupboard,
		Cellar,
		Other
	}

	//Only serialized the fields we're actually using
	[DataContract]
	public class SerializableFood
	{
		[DataMember]
		public int Id { get; set; }
		[DataMember]
		public string Name { get; set; }
		//Serialize storage as a string for the sake of convenience
		[DataMember]
		public string Storage { get; set; }
		[DataMember]
		public Nullable<DateTime> ExpiryDate { get; set; }
		[DataMember]
		public double Quantity { get; set; }
		[DataMember]
		public string QuantityClassifier { get; set; }
		[DataMember]
		public int Vegan { get; set; }
		[DataMember]
		public int Vegetarian { get; set; }
		//Convert to a list before serializing
		[DataMember]
		public List<string> Ingredients { get; set; }
		[DataMember]
		public List<string> Traces { get; set; }
		[DataMember]
		public bool Favourite { get; set; }
		[DataMember]
		public bool OnShoppingList { get; set; }
		public SerializableFood(Food food)
		{
			Id = food.Id;
			Name = food.Name;
			ExpiryDate = food.ExpiryDate;
			Quantity = food.Quantity;
			QuantityClassifier = food.QuantityClassifier;
			Storage = food.Storage.ToString();
			Vegan = food.Vegan;
			Vegetarian = food.Vegetarian;
			Favourite = food.Favourite;
			OnShoppingList = food.OnShoppingList;
			//An empty string should convert to an empty list, not a list with one empty string in it
			Ingredients = new List<string>();
			if (!String.IsNullOrWhiteSpace(food.Ingredients))
				Ingredients = food.Ingredients.Split('|').ToList();
			Traces = new List<string>();
			if (!String.IsNullOrWhiteSpace(food.Traces))
				Traces = food.Traces.Split('|').ToList();
		}
	}
}