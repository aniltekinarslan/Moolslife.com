using System.Collections;
using System.Data;
using System.Data.SqlClient;
using System.Web.Configuration;
using System.Linq;

namespace MoolsPayment
{
    internal class DataAccessor
    {
        private SqlConnection Connection { get; set; }
        private const int CommandTimeout = 120;

        public DataAccessor() : this("SQL") { }

        public DataAccessor(string connectionStringName)
        {
            string connectionString = WebConfigurationManager.ConnectionStrings[connectionStringName].ConnectionString;
            Connection = new SqlConnection(connectionString);
        }

        public int Execute(string sql, CommandType commandType = CommandType.Text)
        {
            using (Connection)
            {
                SqlCommand command = new SqlCommand(sql, Connection);
                command.CommandType = commandType;
                command.CommandTimeout = CommandTimeout;
                Connection.Open();
                return command.ExecuteNonQuery();
            }
        }

        public int Execute(string sql, Hashtable parameters, CommandType commandType = CommandType.Text)
        {
            using (Connection)
            {
                SqlCommand command = new SqlCommand(sql, Connection);
                command.CommandType = commandType;
                command.CommandTimeout = CommandTimeout;

                foreach (string key in parameters.Keys)
                {
                    command.Parameters.Add(new SqlParameter(key, parameters[key]));
                }

                Connection.Open();
                return command.ExecuteNonQuery();
            }
        }

        public DataRow Single(string sql, CommandType commandType = CommandType.Text)
        {
            using (Connection)
            {
                SqlCommand command = new SqlCommand(sql, Connection);
                command.CommandType = commandType;
                command.CommandTimeout = CommandTimeout;

                SqlDataAdapter sqlAdap = new SqlDataAdapter(command);
                var dtResult = new DataTable();
                Connection.Open();
                sqlAdap.Fill(dtResult);
                if (dtResult.Rows.Count > 0)
                {
                    return dtResult.Rows[0];
                }
                return null;
            }
        }

        public DataRow Single(string sql, Hashtable parameters, CommandType commandType = CommandType.Text)
        {
            using (Connection)
            {
                SqlCommand command = new SqlCommand(sql, Connection);
                command.CommandType = commandType;
                command.CommandTimeout = CommandTimeout;

                SqlDataAdapter sqlAdap = new SqlDataAdapter(command);
                foreach (string key in parameters.Keys)
                {
                    sqlAdap.SelectCommand.Parameters.Add(new SqlParameter(key, parameters[key]));
                }
                var dtResult = new DataTable();
                Connection.Open();
                sqlAdap.Fill(dtResult);
                if (dtResult.Rows.Count > 0)
                {
                    return dtResult.Rows[0];
                }
                return null;
            }
        }

        public DataTable Many(string sql, CommandType commandType = CommandType.Text)
        {
            using (Connection)
            {
                SqlCommand command = new SqlCommand(sql, Connection);
                command.CommandType = commandType;
                command.CommandTimeout = CommandTimeout;

                SqlDataAdapter sqlAdap = new SqlDataAdapter(command);
                var dtResult = new DataTable();
                Connection.Open();
                sqlAdap.Fill(dtResult);
                if (dtResult.Rows.Count > 0)
                {
                    return dtResult;
                }
                return null;
            }
        }

        public DataTable Many(string sql, Hashtable parameters, CommandType commandType = CommandType.Text)
        {
            using (Connection)
            {
                SqlCommand command = new SqlCommand(sql, Connection);
                command.CommandType = commandType;
                command.CommandTimeout = CommandTimeout;

                SqlDataAdapter sqlAdap = new SqlDataAdapter(command);
                foreach (string key in parameters.Keys)
                {
                    sqlAdap.SelectCommand.Parameters.Add(new SqlParameter(key, parameters[key]));
                }
                Connection.Open();
                var dtResult = new DataTable();
                sqlAdap.Fill(dtResult);
                if (dtResult.Rows.Count > 0)
                {
                    return dtResult;
                }
                return null;
            }
        }
    }
}