using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Diagnostics;
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

<<<<<<< HEAD
        public int CreateAccountWithEmail(string userName, string pass, string email) {
            if (db.Users.Where(x => x.Email == email && x.Username == userName).Count() != 0)
            {
                return -1;
            }
            else {
                User user = new User { Username = userName, DietId = 1, Email = email, Password = pass, GuiltLevel = 1 };
                db.Users.Add(user);
                try
                {
                    db.SaveChanges();
                }
                catch (DbEntityValidationException e)
                {
                    foreach (var eve in e.EntityValidationErrors)
                    {
                        Debug.WriteLine($"Entity of type \"{eve.Entry.Entity.GetType().Name}\" in state \"{eve.Entry.State}\" has the following validation errors:");
                        foreach (var ve in eve.ValidationErrors)
                        {
                            Debug.WriteLine($"- Property: \"{ve.PropertyName}\", Value: \"{eve.Entry.CurrentValues.GetValue<object>(ve.PropertyName)}\", Error: \"{ve.ErrorMessage}\"");
                        }
                    }
                }
                return db.Users.Where(x => x.Username == userName).FirstOrDefault()?.UserId??-1;
            }
=======
        public bool CreateAccount(string username, string pass, string email , int guiltLevel, int dietId) {
            /*    if (db.Users.Where(x => x.Email == email && x.Username == username).Count() != 0)
                {
                    return false;
                }
                else {
                    User user = new User { Username = username, DietId = 1, Email = email, Password = pass, GuiltLevel = 1 };
                    db.Users.Add(user);
                    db.SaveChanges();
                    return true;
                }
                */
            return (db.CreateAccount(username, email, pass, dietId, guiltLevel).FirstOrDefault() != null);
>>>>>>> origin/C#Dev

        }

        //temporary solution for creating an account without an email
        public int CreateAccount(string userName, string pass)
        {
            return CreateAccountWithEmail(userName, pass, userName);
        }

        public int LoginAccount(string userName, string pass)
        {
<<<<<<< HEAD
            return (db.Users.Where(x => x.Username == userName && x.Password == pass).FirstOrDefault()?.UserId??-1);
=======
            // return db.AccountLogin(email, pass).FirstOrDefault().Value;
            return db.AccountLogin(email, pass).FirstOrDefault() != null;
>>>>>>> origin/C#Dev
        }
    }
}
