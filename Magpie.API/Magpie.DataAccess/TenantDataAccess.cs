using Magpie.Model;
using System;
using System.Data;
using System.Data.SqlClient;

namespace Magpie.DataAccess
{
    public sealed class TenantDataAccess
    {
        private static volatile TenantDataAccess instance;
        private static object syncRoot = new Object();

        private TenantDataAccess() { }

        public static TenantDataAccess Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                            instance = new TenantDataAccess();
                    }
                }

                return instance;
            }
        }

        #region Enums

        private enum TenantPropertiesIndices
        {
            Id,
            Name
        }

        #endregion

        public Tenant GetTenant(string ConnectionString)
        {
            #region Preconditions

            if (string.IsNullOrWhiteSpace(ConnectionString))
                throw new InvalidOperationException();

            #endregion

            try
            {
                string storedProcedureName = "uspGetTenantDeprecated";

                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    using (SqlCommand command = new SqlCommand())
                    {
                        Tenant tenant = new Tenant();


                        command.CommandType = CommandType.StoredProcedure;
                        command.Connection = connection;
                        command.CommandText = storedProcedureName;

                        connection.Open();

                        var reader = command.ExecuteReader();

                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                tenant.Id = reader.GetGuid((int)TenantPropertiesIndices.Id);
                                tenant.Name = reader.GetString((int)TenantPropertiesIndices.Name);
                            }
                        }

                        reader.Close();

                        return tenant;
                    }
                }
            }
            catch (Exception ex)
            {
                string res = ex.ToString();
                throw;
            }
        }
    }
}



