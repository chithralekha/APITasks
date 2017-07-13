using System;
using Magpie.DataAccess;
using Magpie.Model;

namespace Magpie.Repository
{
    public class PolicyGenerationPropertiesRepository
    {

        public PolicyGenerationPropertiesRepository(string ConnectionString)
        {
            #region Preconditions

            if (string.IsNullOrWhiteSpace(ConnectionString))
                throw new ArgumentNullException();

            #endregion

            connectionString = ConnectionString;
        }

        protected string connectionString;

        public PolicyGenerationProperties GetPolicyGenerationProperties(Guid tenant)
        {
            #region Preconditions

            if (string.IsNullOrWhiteSpace(connectionString))
                throw new InvalidOperationException();

            #endregion

            PolicyGenerationProperties policyGenerationProperties = PolicyGenerationPropertiesDataAccess.Instance.GetPolicyGenerationProperties(connectionString, tenant); 
            return policyGenerationProperties;
        }
    }
}
