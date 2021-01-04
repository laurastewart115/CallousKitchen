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
    //All the user info
    public class User
    {
		public User()
		{
		}

		public User(string email, string password)
		{
			Email = email;
			Password = password;
            //Defaults
            Vegetarian = false;
            Vegan = false;
            CalorieTracker = false;
        }

		public User(string email, string password, bool vegetarian, bool vegan, bool calorieTracker, List<Kitchen> kitchens, List<string> allergies)
		{
			Email = email;
			Password = password;
			Vegetarian = vegetarian;
			Vegan = vegan;
			CalorieTracker = calorieTracker;
			Kitchens = kitchens;
            //Convert list to string
			Allergies = string.Join("|", allergies);
		}

		[Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        //Ran into an issue with duplicate emails
        //This was the fix
        //Need this max length for the index to work
        //Never found out where the issue came from
        [Index("UniqueEmail", IsUnique = true)]
        [StringLength(250)]
        public string Email { get; set; }
        //Not hashed
        //It was hard to figure out how to hash something the same in C# and kotlin since the two languages are so different
        public string Password { get; set; }
        //There is no need for an unknown since we assume they aren't if they don't set it
        public bool Vegetarian { get; set; }
        public bool Vegan { get; set; }
        public bool CalorieTracker { get; set; }
        public virtual List<Kitchen> Kitchens { get; set; }
        //Ended up not using this
        public virtual List<CaloriesInDay> CaloriesInDays { get; set; }
        //While it's not pretty, the suggested course of action I found online was to use a delimited string to save a list of strings
        //This is because entityframework is too dumb to generate a table for a list of strings
        public string Allergies { get; set; }
        public Guid EmailConfirmKey { get; set; }
        public bool IsConfirmed { get; set; }
    }
	[DataContract]
	public class SerializableUser
	{
		[DataMember]
        public int Id { get; set; }
        [DataMember]
        public string Email { get; set; }
        [DataMember]
        public bool Vegetarian { get; set; }
        [DataMember]
        public bool Vegan { get; set; }
        [DataMember]
        public List<SerializableKitchen> Kitchens { get; set; }
        [DataMember]
        public List<string> Allergies { get; set; }
        [DataMember]
        public bool IsConfirmed { get; set; }
        public SerializableUser(User user)
		{
            if (user != null)
            {
                Id = user.Id;
                Email = user.Email;
                //Hopefully not serializing the password won't cause problems and will protect security
                //Password = user.Password;
                Vegetarian = user.Vegetarian;
                Vegan = user.Vegan;
                Kitchens = user?.Kitchens?.Select(o => new SerializableKitchen(o))?.ToList();
                Allergies = new List<string>();
                if (!String.IsNullOrWhiteSpace(user.Allergies))
                    Allergies = user.Allergies?.Split('|')?.ToList() ?? new List<string>();
                IsConfirmed = user.IsConfirmed;
            }
            //-1 is the default if something went wrong, like the user having the wrong password
            else
                Id = -1;
		}

        //Made this in case I wanted to send a unique error id like -2 in specefic cases
		public SerializableUser(int id)
		{
			Id = id;
		}
	}
}