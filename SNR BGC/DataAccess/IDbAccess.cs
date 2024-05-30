using System.Collections.Generic;
using System.Threading.Tasks;

namespace SNR_BGC.DataAccess
{
    public interface IDbAccess
    {
        Task ExecuteModificationQuery<U>(string storedProcedure, U parameters, string connectionId = "Myconnection");
        Task<IEnumerable<T>> ExecuteQuery<T, U>(string query, U parameters, string connectionId = "Myconnection");
        Task<T> ExecuteSingleQuery<T, U>(string storedProcedure, U parameters, string connectionId = "Myconnection");
        Task<T> ExecuteSingleSP<T, U>(string storedProcedure, U parameters, string connectionId = "Myconnection");
        Task<IEnumerable<T>> ExecuteSP<T, U>(string storedProcedure, U parameters, string connectionId = "Myconnection");
        IEnumerable<T> ExecuteSP2<T, U>(string storedProcedure, U parameters, string connectionId = "Myconnection");
       
        Task SaveData<T>(string storedProcedure, T parameters, string connectionId = "Myconnection");
        Task SaveDataRange<T>(string storedProcedure, List<T> parameters, string connectionId = "Myconnection");
        Task<int> SaveDataReturnId<T>(string storedProcedure, T parameters, string connectionId = "Myconnection");
        // Task<List<T>> ExecuteSPList<T, U>(string storedProcedure, U parameters, string connectionId = "Default");

    }
}
