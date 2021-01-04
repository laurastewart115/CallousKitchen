using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using Capstone;

namespace Capstone
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "AccountService" in code, svc and config file together.
    // NOTE: In order to launch WCF Test Client for testing this service, please select AccountService.svc or AccountService.svc.cs at the Solution Explorer and start debugging.
    public class AccountService : IAccountService
    {
        private CallousHippoEntities db = new CallousHippoEntities();

        public bool CreateAccount(string username, string pass, string email) {
            if (db.Users.Where(x => x.Email == email && x.Username == username).Count() != 0)
            {
                return false;
            }
            else {
                User user = new User { Username = username, DietId = 1, Email = email, Password = pass, GuiltLevel = 1 };
                db.Users.Add(user);
                db.SaveChanges();
                return true;
            }

        }
        public bool LoginAccount(string email, string pass)
        {
            return (db.Users.Where(x => x.Email == email && x.Password == pass).Count() == 1);
        }
    }
}
