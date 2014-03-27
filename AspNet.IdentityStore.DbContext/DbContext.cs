using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AspNet.IdentityStore
{
    public sealed class DbContext : IDbContext
    {
        ConnectionStringSettings connectionStringSettings;

        SqlConnection sqlConnection;
        int sqlConnectionCount = 0;

        SqlTransaction sqlTransaction;
        int sqlTransactionCount = 0;

        public DbContext()
            : this("DefaultConnection")
        { }

        public DbContext(string connectionStringName)
        {
            connectionStringSettings = ConfigurationManager.ConnectionStrings[connectionStringName];
            if (null == connectionStringSettings)
                throw new ArgumentException("Invalid connection string name.", "connectionStringName");
        }

        void OpenConnection()
        {
            RaiseObjectDisposedException();

            if (null == sqlConnection)
            {
                sqlConnection = new SqlConnection(connectionStringSettings.ConnectionString);
                try
                {
                    sqlConnection.Open();
                }
                catch
                {
                    sqlConnection.Dispose();
                    sqlConnection = null;
                    throw;
                }
            }
            ++sqlConnectionCount;
        }

        internal void CloseConnection(bool disposing = false)
        {
            RaiseObjectDisposedException();

            if (0 == sqlConnectionCount && !disposing)
                throw new ApplicationException("Context already is closed.");

            if (1 == sqlConnectionCount || disposing)
            {
                if (null != sqlTransaction)
                {
                    sqlTransaction.Rollback();
                    sqlTransaction.Dispose();
                    sqlTransaction = null;
                    sqlTransactionCount = 0;
                }
                if (null != sqlConnection)
                {
                    sqlConnection.Close();
                    sqlConnection.Dispose();
                    sqlConnection = null;
                }
                if (disposing)
                {
                    sqlConnectionCount = 0;
                }
            }
            --sqlConnectionCount;
        }

        void BeginTransaction()
        {
            OpenConnection();
            if (null == sqlTransaction)
            {
                sqlTransaction = sqlConnection.BeginTransaction();
            }
            else
            {
                sqlTransaction.Save("savePoint_" + sqlTransactionCount);
            }
            ++sqlTransactionCount;
        }

        internal void Commit()
        {
            RaiseNoActiveTransaction();

            --sqlTransactionCount;

            if (0 == sqlTransactionCount)
            {
                sqlTransaction.Commit();
                sqlTransaction.Dispose();
                sqlTransaction = null;
            }

            CloseConnection();
        }

        internal void Rollback()
        {
            RaiseNoActiveTransaction();

            --sqlTransactionCount;

            if (0 == sqlTransactionCount)
            {
                sqlTransaction.Rollback();
                sqlTransaction.Dispose();
                sqlTransaction = null;
            }
            else
            {
                sqlTransaction.Rollback("savePoint_" + sqlTransactionCount);
            }


            CloseConnection();
        }

        IDbConnectionContext IDbContext.Open()
        {
            OpenConnection();
            return new DbConnectionContext(this, false);
        }

        IDbConnectionContext IDbContext.BeginTransaction()
        {
            BeginTransaction();
            return new DbConnectionContext(this, true);
        }

        internal System.Data.IDbConnection Connection
        {
            get
            {
                RaiseNoActiveConnection();
                return sqlConnection;
            }
        }

        internal System.Data.IDbTransaction Transaction
        {
            get
            {
                RaiseNoActiveConnection();
                return sqlTransaction;
            }
        }

        void RaiseNoActiveTransaction()
        {
            RaiseObjectDisposedException();
            if (0 == sqlTransactionCount)
                throw new ApplicationException("No active transaction is present.");
        }

        void RaiseNoActiveConnection()
        {
            RaiseObjectDisposedException();
            if (null == sqlConnection)
                throw new ApplicationException("No active connection is present.");
        }

        void RaiseObjectDisposedException()
        {
            if (sqlConnectionCount < 0)
                throw new ObjectDisposedException(typeof(DbContext).Name);
        }

        public void Dispose()
        {
            CloseConnection(true);
            GC.SuppressFinalize(this);
        }
    }
}