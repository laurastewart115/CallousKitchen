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
	//Used for multiple kitchens but we cut that so this is kind of useless
	//Since every user has only one kitchen
	public class Kitchen
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }
		//Name is kind of pointless since we never show the kitchen name since it's just a default
		public string Name { get; set; }
		public virtual List<Food> Inventory { get; set; }
	}
	[DataContract]
	public class SerializableKitchen
	{
		[DataMember]
		public int Id { get; set; }
		[DataMember]
		public string Name { get; set; }
		[DataMember]
		public List<SerializableFood> Inventory { get; set; }
		public SerializableKitchen(Kitchen kitchen)
		{
			Id = kitchen.Id;
			Name = kitchen.Name;
			//The foods in the kitchen also need to be serialized
			Inventory =  kitchen.Inventory?.Select(o => new SerializableFood(o)).ToList();
		}
	}
}