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
	//Ended up never not getting the chance to put in calorie tracking stuff
	//If I had, this would have tracked the calories that each user consumed on a given day 
	public class CaloriesInDay
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }
		public DateTime Day { get; set; }
		public int Calories { get; set; }
	}

	//Every class has a serializble version because the key attribute confuses c#'s json serialization, so it has to be explicitly defined
	//Usually the exact same but sometimes has slight differences
	[DataContract]
	public class SerializableCaloriesInDay
	{
		[DataMember]
		public int Id { get; set; }
		[DataMember]
		public DateTime Day { get; set; }
		[DataMember]
		public int Calories { get; set; }
		public SerializableCaloriesInDay(CaloriesInDay caloriesInDay)
		{
			Id = caloriesInDay.Id;
			Day = caloriesInDay.Day;
			Calories = caloriesInDay.Calories;
		}
	}
}