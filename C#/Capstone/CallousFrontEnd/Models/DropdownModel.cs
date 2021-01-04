using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CallousFrontEnd.Models
{
    // Author: Peter
    // Model for the "Vegan" dropdown
    // Id matches the database
    public class DropdownModel
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public static List<DropdownModel> VegVeganDropdown()
        {
            List<DropdownModel> dM = new List<DropdownModel>();
            dM.Add(new DropdownModel { Id = -1, Name = "Unknown" });
            dM.Add(new DropdownModel { Id = 0, Name = "No" });
            dM.Add(new DropdownModel { Id = 1, Name = "Yes" });
            return dM;
        }
    }

}
