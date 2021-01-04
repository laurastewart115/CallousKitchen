using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Capstone
{
    [ServiceContract]
    public interface IBarcodeService
    {
        [OperationContract]
        Task<string> GetBarcodeData(string barcode);
        [OperationContract]
        Task<bool> AddFood(int kitchenId, string name, int quantity);
        [OperationContract]
        Task<bool> EditItem(int id, int quantity);
        [OperationContract]
        Task<bool> RemoveItem(int id);
        [OperationContract]
        string TestMethod();
    }
}
