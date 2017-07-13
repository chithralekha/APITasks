using Magpie.DataAccess;
using Magpie.Model;
using System;

namespace Magpie.Repository
{
    public class TenantRepository
    {

        public TenantRepository(string ConnectionString)
        {
            #region Preconditions

            if (string.IsNullOrWhiteSpace(ConnectionString))
                throw new ArgumentNullException();

            #endregion

            connectionString = ConnectionString;
        }

        protected string connectionString;

        public Tenant GetTenant()
        {
            #region Preconditions

            if (string.IsNullOrWhiteSpace(connectionString))
                throw new InvalidOperationException();

            #endregion

            Tenant tenant = TenantDataAccess.Instance.GetTenant(connectionString);
            return tenant;
        }
    }
}