using Magpie.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Magpie.DataAccess
{
    public sealed class DocumentDataAccess
    { 
        private static volatile DocumentDataAccess instance;
        private static object syncRoot = new Object();

        private DocumentDataAccess() { }

        public static DocumentDataAccess Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                            instance = new DocumentDataAccess();
                    }
                }

                return instance;
            }
        }

        public int? Create(string ConnectionString, WorkingSetDocument workingSetDocument)
        {
            #region Preconditions

            if (string.IsNullOrWhiteSpace(ConnectionString))
                throw new InvalidOperationException();

            if (workingSetDocument == null)
                throw new ArgumentNullException();

            #endregion

            try
            {
                string storedProcedureName = "usp_WorkingSetDocumentsCreate";

                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    using (SqlCommand command = new SqlCommand())
                    {
                        int? newId = null;

                        command.CommandType = CommandType.StoredProcedure;
                        command.Connection = connection;
                        command.CommandText = storedProcedureName;

                        command.Parameters.AddWithValue("@WorkingSetId", workingSetDocument.WorkingSetId);
                        command.Parameters.AddWithValue("@DocumentTitle", workingSetDocument.DocumentTitle);
                        command.Parameters.AddWithValue("@DocumentUri", workingSetDocument.DocumentUri);

                        SqlParameter outPutVal = new SqlParameter("@NewId", SqlDbType.Int);
                        outPutVal.Direction = ParameterDirection.Output;
                        command.Parameters.Add(outPutVal);

                        connection.Open();
                        command.ExecuteNonQuery();
                        connection.Close();

                        if (outPutVal.Value != DBNull.Value)
                            newId = Convert.ToInt32(outPutVal.Value);

                        return newId;
                    }
                }
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
