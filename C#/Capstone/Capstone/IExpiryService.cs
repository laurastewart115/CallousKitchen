using Capstone.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Capstone
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IExpiryService" in both code and config file together.
    [ServiceContract]
    public interface IExpiryService
    {
        [OperationContract]
        Tuple<DateTime, DateTime, Storage> GetDateStorage(string food, Storage storage);

    }
}
