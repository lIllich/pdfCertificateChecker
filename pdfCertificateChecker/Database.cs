using Oracle.ManagedDataAccess.Client;
using System.Data;

namespace pdfCertificateChecker
{
    internal class Database
    {
        private OracleConnection? connection;

        public void InitConnection(string schema, string password, string db)
        {
            try
            {
                OracleConnectionStringBuilder connectionStringBuilder = new OracleConnectionStringBuilder
                {
                    DataSource = db,
                    UserID = schema,
                    Password = password
                };

                connection = new OracleConnection(connectionStringBuilder.ConnectionString);
            }
            catch (OracleException ex)
            {
                throw new ApplicationException("Oracle Exception: " + ex.Message, ex);
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Exception: " + ex.Message, ex);
            }
        }

        public OracleDataReader ExecuteQuery(string query, OracleParameter[]? parameters = null)
        {
            try
            {
                if (connection.State != ConnectionState.Open) connection.Open();

                using (OracleCommand command = new OracleCommand(query, connection))
                {
                    if (parameters != null) command.Parameters.AddRange(parameters);
                    return command.ExecuteReader();
                }
            }
            catch (OracleException ex)
            {
                throw new ApplicationException("Oracle Exception: " + ex.Message, ex);
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Exception: " + ex.Message, ex);
            }
        }

        public void ExecuteNonQuery(string query)
        {
            try
            {
                using (OracleCommand command = new OracleCommand(query, connection))
                {
                    if (connection.State != ConnectionState.Open) connection.Open();
                    command.ExecuteNonQuery();
                }
            }
            catch (OracleException ex)
            {
                throw new ApplicationException("Oracle Exception: " + ex.Message, ex);
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Exception: " + ex.Message, ex);
            }
        }
    }
}