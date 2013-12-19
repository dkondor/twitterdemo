using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;

namespace Elte.GeoVisualizer.Lib.DataSources
{
    public class SqlQuery : DataSource
    {
        private string connectionString;
        private SqlConnection connection;
        private SqlTransaction transaction;
        private SqlCommand command;
        private SqlDataReader dataReader;

        public string ConnectionString
        {
            get { return connectionString; }
            set { connectionString = value; }
        }

        public SqlCommand Command
        {
            get { return command; }
            set { command = value; }
        }

        private void InitializeMembers()
        {
            this.connectionString = null;
            this.connection = null;
            this.transaction = null;
            this.command = null;
            this.dataReader = null;
        }

        public override void Open()
        {
            if (connection != null)
            {
                throw new InvalidOperationException();  // TODO
            }

            connection = new SqlConnection(connectionString);
            connection.Open();

            transaction = connection.BeginTransaction(IsolationLevel.ReadUncommitted);

            command.Connection = connection;
            command.Transaction = transaction;

            dataReader = command.ExecuteReader();
        }

        public override void Close()
        {
            dataReader.Close();
            dataReader.Dispose();
            dataReader = null;

            transaction.Commit();
            transaction.Dispose();
            transaction = null;

            connection.Close();
            connection.Dispose();
            connection = null;
        }

        public override string[] GetColumnNames()
        {
            var res = new string[dataReader.FieldCount];

            for (int i = 0; i < dataReader.FieldCount; i++)
            {
                dataReader.GetName(i);
            }

            return res;
        }

        public override bool ReadNext(object[] values)
        {
            if (dataReader.Read())
            {
                dataReader.GetValues(values);
                return true;
            }
            else
            {
                return false;
            }            
        }

    }
}
