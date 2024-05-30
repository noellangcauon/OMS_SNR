using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using System;
using System.Data.SqlClient;
using Dapper;
using System.Linq;

namespace SNR_BGC.DataAccess
{
    public class DbAccess : IDbAccess
    {
        private readonly IConfiguration _config;
        public DbAccess(IConfiguration config)
        {
            _config = config;
        }

        public async Task<IEnumerable<T>> ExecuteSP<T, U>(string storedProcedure,
       U parameters,
       string connectionId = "Myconnection")
        {
            using IDbConnection connection = new SqlConnection(_config.GetConnectionString(connectionId));

            return await connection.QueryAsync<T>(storedProcedure,
                parameters,
                commandType: CommandType.StoredProcedure);
        }

        public IEnumerable<T> ExecuteSP2<T, U>(string storedProcedure,
       U parameters,
       string connectionId = "Myconnection")
        {
            using IDbConnection connection = new SqlConnection(_config.GetConnectionString(connectionId));

            return connection.Query<T>(storedProcedure,
                parameters,
                commandType: CommandType.StoredProcedure);
        }

        public async Task<IEnumerable<T>> ExecuteQuery<T, U>(string query,
         U parameters,
         string connectionId = "Myconnection")
        {
            using IDbConnection connection = new SqlConnection(_config.GetConnectionString(connectionId));

            return await connection.QueryAsync<T>(query,
                parameters,
                commandType: CommandType.Text);
        }

        public async Task<T> ExecuteSingleSP<T, U>(string storedProcedure,
            U parameters,
            string connectionId = "Myconnection")
        {
            using IDbConnection connection = new SqlConnection(_config.GetConnectionString(connectionId));

            var result = await connection.QueryAsync<T>(storedProcedure,
                parameters,
                commandType: CommandType.StoredProcedure);

            return result.FirstOrDefault();
        }

        public async Task<T> ExecuteSingleQuery<T, U>(string storedProcedure,
            U parameters,
            string connectionId = "Myconnection")
        {
            using IDbConnection connection = new SqlConnection(_config.GetConnectionString(connectionId));

            var result = await connection.QueryAsync<T>(storedProcedure,
                parameters,
                commandType: CommandType.Text);

            return result.FirstOrDefault();
        }



        public async Task<int> SaveDataReturnId<T>(string storedProcedure, T parameters, string connectionId = "Myconnection")
        {
            using IDbConnection connection = new SqlConnection(_config.GetConnectionString(connectionId));
            connection.Open();

            using var transaction = connection.BeginTransaction();
            try
            {
                var result = await connection.QuerySingleAsync<int>(storedProcedure, parameters, transaction, commandType: CommandType.StoredProcedure);
                transaction.Commit();
                return result;
            }
            catch (Exception)
            {
                transaction.Rollback();
                throw;
            }
        }

        public async Task ExecuteModificationQuery<U>(string storedProcedure, U parameters, string connectionId = "Myconnection")
        {
            using IDbConnection connection = new SqlConnection(_config.GetConnectionString(connectionId));
            using var transaction = connection.BeginTransaction();
            try
            {
                await connection.ExecuteAsync(storedProcedure, parameters, commandType: CommandType.StoredProcedure);
            }
            catch (Exception) { transaction.Rollback(); throw; }
        }

        public async Task SaveData<T>(string storedProcedure, T parameters, string connectionId = "Myconnection")
        {
            using IDbConnection connection = new SqlConnection(_config.GetConnectionString(connectionId));
            connection.Open();
            using var transaction = connection.BeginTransaction();
            try
            {
                await connection.ExecuteAsync(storedProcedure, parameters, transaction, commandType: CommandType.StoredProcedure);
                transaction.Commit();
            }
            catch (Exception)
            {
                transaction.Rollback();
                throw;
            }
        }

        public async Task SaveDataRange<T>(string storedProcedure, List<T> parameters, string connectionId = "Myconnection")
        {
            using IDbConnection connection = new SqlConnection(_config.GetConnectionString(connectionId));
            using var transaction = connection.BeginTransaction();
            try
            {
                foreach (var item in parameters)
                    await connection.ExecuteAsync(storedProcedure, item, commandType: CommandType.StoredProcedure);

                transaction.Commit();
            }
            catch (Exception)
            {
                transaction.Rollback();
                throw;
            }
        }


    }
}
