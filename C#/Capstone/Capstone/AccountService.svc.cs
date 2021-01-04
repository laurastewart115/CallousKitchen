using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Capstone.Apis;
using Capstone.Classes;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Security.Cryptography;
using System.ServiceModel;

namespace Capstone
{
	//Use inheritance to have two services with the same methods
	//The methods required in iaccountservice are found in parent
	public class AccountService : AccountServiceParent, IAccountService
	{
	}
}
