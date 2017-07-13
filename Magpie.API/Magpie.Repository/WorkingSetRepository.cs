using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Magpie.DataAccess;
using Magpie.Model;

namespace Magpie.Repository
{
    public class WorkingSetRepository : IRepository<WorkingSet>
    {
        public WorkingSetRepository(string ConnectionString)
        {
            #region Preconditions

            if (string.IsNullOrWhiteSpace(ConnectionString))
                throw new ArgumentNullException();

            #endregion

            connectionString = ConnectionString;
        }

        protected string connectionString;

        public IEnumerable<WorkingSet> GetItems()
        {
            #region Preconditions

            if (string.IsNullOrWhiteSpace(connectionString))
                throw new InvalidOperationException();

            #endregion

            var workingSets = WorkingSetDataAccess.Instance.GetWorkingSets(connectionString);

            foreach (var workingSet in workingSets)
            {
                #region WorkingSets

                workingSet.WorkingSetTemplate = WorkingSetDataAccess.Instance.GetWorkingSetTemplateAndComplianceInfoForSpecificWorkingSetGet(connectionString, workingSet.WorkingSetTemplate.WorkingSetTemplateId, workingSet.Id);

                #endregion

                #region Compliance

                int sumControlSetCompliance = 0;
                int totalComplianceValues = 0;

                foreach (var cs in workingSet.WorkingSetTemplate.ControlSets)
                {
                    if (cs.ControlSetCompliance.HasValue)
                    {
                        sumControlSetCompliance += cs.ControlSetCompliance.Value;
                        totalComplianceValues++;
                    }
                }

                if (sumControlSetCompliance == 0 || totalComplianceValues == 0)
                    workingSet.WorkingSetCompliance = 0;
                else
                {
                    decimal workingSetCompliance = sumControlSetCompliance / totalComplianceValues;
                    workingSet.WorkingSetCompliance = Convert.ToInt32(workingSetCompliance);
                }

                #endregion

                #region WorkingSetUsers

                var workingSetUsers = UserDataAccess.Instance.GetWorkingSetUsers(connectionString, workingSet.Id);
                workingSet.Users = workingSetUsers;

                #endregion

                #region WorkingSetDataPoints

                //foreach (var ws in workingSets)
                //{
                //    ws.WorkingSetDataPoint = GetWorkingSetDataPoint(ConnectionString, ws.WorkingSetId, Decimal.ToInt32(ws.WorkingSetCompliance.Value));
                //}

                #endregion
            }

            return workingSets;
        }

        public WorkingSet GetItem(int Id)
        {
            #region Preconditions

            if (string.IsNullOrWhiteSpace(connectionString))
                throw new InvalidOperationException();

            if (Id <= 0)
                throw new ArgumentOutOfRangeException();

            #endregion

            var workingSets = WorkingSetDataAccess.Instance.GetWorkingSets(connectionString, Id);

            if (workingSets.Count() != 1)
                throw new Exception();

            var ws = workingSets.First();

            var wsts = WorkingSetDataAccess.Instance.GetWorkingSetTemplates(connectionString, ws.WorkingSetTemplate.WorkingSetTemplateId);

            if (wsts.Count() != 1)
                throw new Exception();

            ws.WorkingSetTemplate = wsts.First();

            return ws;
        }

        public bool Deploy(int Id)
        {
            throw new NotImplementedException();
        }

        public int? Add(WorkingSet item)
        {
            throw new NotImplementedException();
        }

        public bool Update(WorkingSet item)
        {
            throw new NotImplementedException();
        }

        public void Remove(int Id)
        {
            throw new NotImplementedException();
        }
    }
}
