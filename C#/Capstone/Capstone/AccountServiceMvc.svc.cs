using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.ServiceModel;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Capstone.Apis;
using Capstone.Classes;

namespace Capstone
{
	//Use inheritance to have two services with the same methods
	//The methods required in iaccountservice are found in parent
	public class AccountServiceMvc : AccountServiceParent, IAccountServiceMvc
	{
	}
}
