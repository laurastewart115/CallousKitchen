using Capstone.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CallousFrontEnd.Models
{
    // Author: Peter
    // Model for keeping track of a userId with a kitchen

    public class KitchenUser
    {
        public int UserId { get; set; }
        public Kitchen kitchen { get; set; }
    }
}
