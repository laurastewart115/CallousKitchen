using AccountService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CallousFrontEnd.Models
{
    // Author: Peter
    // Model for keeping track of the kitchen Id with the food
    public class FoodKitchen
    {
        public int KitchenId { get; set; }
        public Food Food { get; set; }
    }
}
