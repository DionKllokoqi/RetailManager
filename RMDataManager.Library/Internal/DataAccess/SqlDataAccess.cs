using Dapper;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RMDataManager.Library.Internal.DataAccess
{
    internal class SqlDataAccess : IDisposable
    {
        private IDbConnection _connection;
        private IDbTransaction _transaction;
        private bool _isClosed = false;

        /// <summary>
        /// Get Connection String of the current project that is using this class library.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public string GetConnectionString(string name)
        {
            return ConfigurationManager.ConnectionStrings[name].ConnectionString;
        }

        /// <summary>
        /// Load Data from the Database via this method.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="U"></typeparam>
        /// <param name="storedProcedure"></param>
        /// <param name="parameters"></param>
        /// <param name="connectionStringName"></param>
        /// <returns></returns>
        public List<T> LoadData<T, U>(string storedProcedure, U parameters, string connectionStringName)
        {
            string connectionString = GetConnectionString(connectionStringName);

            using (IDbConnection cnn = new SqlConnection(connectionString))
            {
                // query database, T being the type of model that we want each row
                // to be, passing the stored procedure name and generic parameters
                List<T> rows = cnn.Query<T>(storedProcedure, parameters, 
                    commandType: CommandType.StoredProcedure).ToList();
                return rows;
            }
        }

        /// <summary>
        /// Save Data to the database via this method.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="storedProcedure"></param>
        /// <param name="parameters"></param>
        /// <param name="connectionStringName"></param>
        public void SaveData<T>(string storedProcedure, T parameters, string connectionStringName)
        {
            string connectionString = GetConnectionString(connectionStringName);

            using (IDbConnection cnn = new SqlConnection(connectionString))
            {
                cnn.Execute(storedProcedure, parameters, commandType: CommandType.StoredProcedure);
            }
        }

        public void StartTransaction(string connectionStringName)
        {
            string connectionString = GetConnectionString(connectionStringName);

            _connection = new SqlConnection(connectionString);
            _connection.Open();

            _transaction = _connection.BeginTransaction();

            _isClosed = false;
        }

        public List<T> LoadDataInTransaction<T, U>(string storedProcedure, U parameters)
        {
            List<T> rows = _connection.Query<T>(storedProcedure, parameters,
                commandType: CommandType.StoredProcedure, transaction: _transaction).ToList();

            return rows;
        }

        public void SaveDataInTransaction<T>(string storedProcedure, T parameters)
        {
            _connection.Execute(storedProcedure, parameters, 
                commandType: CommandType.StoredProcedure, transaction: _transaction);
        }

        public void CommitTransaction()
        {
            _transaction?.Commit();
            _connection?.Close();

            _isClosed = true;
        }

        public void RollbackTransaction()
        {
            _transaction?.Rollback();
            _connection?.Close();

            _isClosed = true;
        }

        public void Dispose()
        {
            if (!_isClosed)
            {
                try
                {
                    CommitTransaction();
                }
                catch
                {
                    // ToDo - Log this issue
                } 
            }

            _transaction = null;
            _connection = null;
        }

        // Open connection / transaction method 
        // Load using the transaction
        // Save using the transaction
        // Close connection / stop transaction method
        // Dispose method
    }
}
