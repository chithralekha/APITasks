using System;
using Magpie.Model;
using Magpie.DataAccess;

namespace Magpie.Repository
{
    public class DocumentRepository
    {
        public DocumentRepository(string ConnectionString)
        {
            #region Preconditions

            if (string.IsNullOrWhiteSpace(ConnectionString))
                throw new ArgumentNullException();

            #endregion

            connectionString = ConnectionString;
        }

        protected string connectionString;

        public int? Add(WorkingSetDocument item)
        {
            #region Preconditions

            if (string.IsNullOrWhiteSpace(connectionString))
                throw new InvalidOperationException();

            if (item == null)
                throw new ArgumentNullException();

            #endregion

            var newId = DocumentDataAccess.Instance.Create(connectionString, item);
            return newId;
        }
       
    }
}
