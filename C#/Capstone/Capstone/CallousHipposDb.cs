using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using Capstone.Classes;

namespace Capstone
{
	//Author: Kevin Gadelha
	public class CallousHipposDb : DbContext
	{
		//CallousHipposDb is the name of the connection string in web.config
		public CallousHipposDb() : base("name=CallousHipposDb")
		{
		}
		//Virtual dbset for entityframework
		public virtual DbSet<User> Users { get; set; }
		public virtual DbSet<CaloriesInDay> CaloriesInDays { get; set; }
		public virtual DbSet<Kitchen> Kitchens { get; set; }
		public virtual DbSet<Food> Foods { get; set; }
	}
}