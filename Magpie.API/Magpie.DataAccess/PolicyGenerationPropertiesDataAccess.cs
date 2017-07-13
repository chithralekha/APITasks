using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Magpie.Model;

namespace Magpie.DataAccess
{
    public sealed class PolicyGenerationPropertiesDataAccess
    {
        private static volatile PolicyGenerationPropertiesDataAccess instance;
        private static object syncRoot = new Object();

        private PolicyGenerationPropertiesDataAccess() { }

        public static PolicyGenerationPropertiesDataAccess Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                            instance = new PolicyGenerationPropertiesDataAccess();
                    }
                }

                return instance;
            }
        }

        #region Enums

        private enum PolicyGenerationPropertiesIndices
        {
            Key,
            Value
        }

        #endregion

        public PolicyGenerationProperties GetPolicyGenerationProperties(string ConnectionString, Guid tenant)
        {
            #region Preconditions

            if (string.IsNullOrWhiteSpace(ConnectionString))
                throw new InvalidOperationException();

            #endregion

            try
            {
                string storedProcedureName = "uspGetPolicyGenerationPropertiesDeprecated";

                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    using (SqlCommand command = new SqlCommand())
                    {
                        PolicyGenerationProperties policyGenerationProperties = new PolicyGenerationProperties
                        {
                            PolicyGenerationPropertiesEntries = new Dictionary<string, string>()
                        };

                        command.CommandType = CommandType.StoredProcedure;
                        command.Connection = connection;
                        command.CommandText = storedProcedureName;

                        command.Parameters.AddWithValue("@Tenant", tenant);

                        connection.Open();

                        var reader = command.ExecuteReader();

                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                string key = reader.GetString((int)PolicyGenerationPropertiesIndices.Key);

                                string value = null;

                                if (!reader.IsDBNull((int)PolicyGenerationPropertiesIndices.Value))
                                    value = reader.GetString((int)PolicyGenerationPropertiesIndices.Value);

                                policyGenerationProperties.PolicyGenerationPropertiesEntries.Add(key, value);
                            }
                        }

                        reader.Close();

                        return policyGenerationProperties;
                        
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


