using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Magpie.Model;

namespace Magpie.DataAccess
{
    public sealed class WorkingSetDataAccess
    {
        private static volatile WorkingSetDataAccess instance;
        private static object syncRoot = new Object();

        private WorkingSetDataAccess() { }

        public static WorkingSetDataAccess Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                            instance = new WorkingSetDataAccess();
                    }
                }

                return instance;
            }
        }

        #region Enums

        private enum WorkingSetsIndices
        {
            WorkingSetId,
            WorkingSetGuid,
            WorkingSetTemplateId,
            Name,
            Description,
            Deployed,
            DeployedByUserId,
            DeployedByUserName,
            WorkingSetCompliance
        }

        private enum WorkingSetTemplatesGetWithRelationshipsIndices
        {
            WorkingSetTemplateId,
            WorkingSetTemplateGuid,
            WorkingSetTemplateName,
            WorkingSetTemplateCreated,
            WorkingSetTemplateCreatedByUserId,
            WorkingSetTemplateCreatedByUserName,
            ControlSetId,
            ControlSetCode,
            ControlSetTitle,
            ControlSetClassificationId,
            ControlSetClassificationName,
            ControlSetDefinitionSourceId,
            ControlSetDefinitionSourceCode,
            ControlSetMagpieCoreDefinitionSourceGuid,
            ControlSetDefinitionSource,
            MagpieCoreControlSetGuid,
            ControlSetVersion
        }

        private enum WorkingSetTemplateAndComplianceInfoForSpecificWorkingSetGetIndices
        {
            WorkingSetTemplateId,
            WorkingSetTemplateGuid,
            WorkingSetTemplateName,
            WorkingSetTemplateCreated,
            WorkingSetTemplateCreatedByUserId,
            WorkingSetTemplateCreatedByUserName,
            ControlSetId,
            ControlSetCode,
            ControlSetTitle,
            ControlSetClassificationId,
            ControlSetClassificationName,
            ControlSetDefinitionSourceId,
            ControlSetDefinitionSourceCode,
            ControlSetMagpieCoreDefinitionSourceGuid,
            ControlSetDefinitionSource,
            MagpieCoreControlSetGuid,
            ControlSetVersion,
            ControlSetCompliance
        }

        private enum WorkingSetsDataPointIndices
        {
            WorkingSetId,
            Name,
            TotalTasks,
            TotalNew,
            TotalInProgress,
            TotalCompleted,
            TotalOnTime,
            TotalOverdue,
            TotalInJeopardy,
            TotalAssigned,
            TotalUnAssigned
        }

        #endregion

        public IEnumerable<WorkingSet> GetWorkingSets(string ConnectionString, int? Id = null)
        {
            #region Preconditions

            if (string.IsNullOrWhiteSpace(ConnectionString))
                throw new InvalidOperationException();

            if (Id != null && Id <= 0)
                throw new ArgumentOutOfRangeException();

            #endregion

            try
            {
                string storedProcedureName = "usp_WorkingSetsGet";

                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    using (SqlCommand command = new SqlCommand())
                    {
                        var workingSets = new List<WorkingSet>();

                        command.CommandType = CommandType.StoredProcedure;
                        command.Connection = connection;
                        command.CommandText = storedProcedureName;

                        if (Id != null)
                            command.Parameters.AddWithValue("@WorkingSetId", Id);

                        connection.Open();

                        var reader = command.ExecuteReader();

                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                var ws = new WorkingSet
                                {
                                    Deployed = reader.GetDateTime((int)WorkingSetsIndices.Deployed),
                                    DeployedByUser = new User
                                    {
                                        Id = reader.GetString((int)WorkingSetsIndices.DeployedByUserId),
                                        UserName = reader.GetString((int)WorkingSetsIndices.DeployedByUserName),
                                    },
                                    Description = (reader.IsDBNull((int)WorkingSetsIndices.Description)) ? null : reader.GetString((int)WorkingSetsIndices.Description),
                                    Name = reader.GetString((int)WorkingSetsIndices.Name),
                                    //WorkingSetCompliance = reader.GetInt32((int)WorkingSetsIndices.WorkingSetCompliance),
                                    WorkingSetGuid = reader.GetGuid((int)WorkingSetsIndices.WorkingSetGuid),
                                    Id = reader.GetInt32((int)WorkingSetsIndices.WorkingSetId),
                                    WorkingSetTemplate = new WorkingSetTemplate
                                    {
                                        WorkingSetTemplateId = reader.GetInt32((int)WorkingSetsIndices.WorkingSetTemplateId)
                                    }
                                };

                                workingSets.Add(ws);
                            }

                        }

                        reader.Close();

                        return workingSets;
                    }
                }
            }
            catch (Exception ex)
            {
                string res = ex.ToString();
                throw;
            }
        }

        private WorkingSetDataPoint GetWorkingSetDataPoint(string ConnectionString, int id, int? compliancePercent)
        {
            try
            {
                WorkingSetDataPoint workingSetDataPoint = new WorkingSetDataPoint();

                string storedProcedureName = "uspGetTaskStatisticsDeprecated";

                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    using (SqlCommand command = new SqlCommand())
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Connection = connection;
                        command.CommandText = storedProcedureName;

                        command.Parameters.AddWithValue("@WorkingSetId", id);

                        connection.Open();

                        var reader = command.ExecuteReader();

                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                workingSetDataPoint.WorkingSetId = reader.GetInt32((int)WorkingSetsIndices.WorkingSetId);

                                if (compliancePercent != null)
                                    workingSetDataPoint.CompliancePercent = compliancePercent.Value;

                                if (!reader.IsDBNull((int)WorkingSetsDataPointIndices.TotalTasks))
                                    workingSetDataPoint.TotalTasks = reader.GetInt32((int)WorkingSetsDataPointIndices.TotalTasks);
                                if (!reader.IsDBNull((int)WorkingSetsDataPointIndices.TotalInProgress))
                                    workingSetDataPoint.TotalInProgress = reader.GetInt32((int)WorkingSetsDataPointIndices.TotalInProgress);
                                if (!reader.IsDBNull((int)WorkingSetsDataPointIndices.TotalCompleted))
                                    workingSetDataPoint.TotalCompleted = reader.GetInt32((int)WorkingSetsDataPointIndices.TotalCompleted);
                                if (!reader.IsDBNull((int)WorkingSetsDataPointIndices.TotalOnTime))
                                    workingSetDataPoint.TotalOnTime = reader.GetInt32((int)WorkingSetsDataPointIndices.TotalOnTime);
                                if (!reader.IsDBNull((int)WorkingSetsDataPointIndices.TotalOverdue))
                                    workingSetDataPoint.TotalOverdue = reader.GetInt32((int)WorkingSetsDataPointIndices.TotalOverdue);
                                if (!reader.IsDBNull((int)WorkingSetsDataPointIndices.TotalInJeopardy))
                                    workingSetDataPoint.TotalInJeopardy = reader.GetInt32((int)WorkingSetsDataPointIndices.TotalInJeopardy);
                            }
                        }
                        reader.Close();
                    }
                }

                return workingSetDataPoint;
            }

            catch (Exception ex)
            {
                string res = ex.ToString();
                throw;
            }
        }

        public int? Create(string ConnectionString, WorkingSet WorkingSet)
        {
            throw new NotImplementedException();
        }

        public bool Update(string ConnectionString, WorkingSet WorkingSet)
        {
            throw new NotImplementedException();
        }

        public void Delete(string ConnectionString, int Id)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<WorkingSetTemplate> GetWorkingSetTemplates(string ConnectionString, int? Id = null)
        {
            #region Preconditions

            if (string.IsNullOrWhiteSpace(ConnectionString))
                throw new InvalidOperationException();

            if (Id != null && Id <= 0)
                throw new ArgumentOutOfRangeException();

            #endregion

            try
            {
                var workingSetTemplates = new List<WorkingSetTemplate>();

                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    using (SqlCommand command = new SqlCommand())
                    {
                        string storedProcedureName = "usp_WorkingSetTemplatesGetWithRelationships";

                        command.CommandType = CommandType.StoredProcedure;
                        command.Connection = connection;
                        command.CommandText = storedProcedureName;

                        if (Id != null)
                            command.Parameters.AddWithValue("@WorkingSetTemplateId", Id);

                        connection.Open();

                        var reader = command.ExecuteReader();

                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                var id = reader.GetInt32((int)WorkingSetTemplatesGetWithRelationshipsIndices.WorkingSetTemplateId);

                                var wst = workingSetTemplates.Find(i => i.WorkingSetTemplateId == id);

                                if (wst == null)
                                {
                                    wst = new WorkingSetTemplate
                                    {
                                        ControlSets = new List<ControlSet>
                                        {
                                            new ControlSet
                                            {
                                                Code = reader.GetString((int)WorkingSetTemplatesGetWithRelationshipsIndices.ControlSetCode),
                                                Controls = null,
                                                ControlSetClassification = new ControlSetClassification
                                                {
                                                    Id = reader.GetInt32((int)WorkingSetTemplatesGetWithRelationshipsIndices.ControlSetClassificationId),
                                                    Name = reader.GetString((int)WorkingSetTemplatesGetWithRelationshipsIndices.ControlSetClassificationName),
                                                },
                                                DefinitionSource = new DefinitionSource
                                                {
                                                    Code = reader.GetString((int)WorkingSetTemplatesGetWithRelationshipsIndices.ControlSetDefinitionSourceCode),
                                                    MagpieCoreDefinitionSourceGuid = (reader.IsDBNull((int)WorkingSetTemplatesGetWithRelationshipsIndices.ControlSetMagpieCoreDefinitionSourceGuid)) ? (Guid?)null : reader.GetGuid((int)WorkingSetTemplatesGetWithRelationshipsIndices.ControlSetMagpieCoreDefinitionSourceGuid),
                                                    Id = reader.GetInt32((int)WorkingSetTemplatesGetWithRelationshipsIndices.ControlSetDefinitionSourceId),
                                                    Source = reader.GetString((int)WorkingSetTemplatesGetWithRelationshipsIndices.ControlSetDefinitionSource),
                                                },
                                                Id = reader.GetInt32((int)WorkingSetTemplatesGetWithRelationshipsIndices.ControlSetId),
                                                Title = reader.GetString((int)WorkingSetTemplatesGetWithRelationshipsIndices.ControlSetTitle),
                                                Version = reader.GetString((int)WorkingSetTemplatesGetWithRelationshipsIndices.ControlSetVersion),
                                                MagpieCoreControlSetGuid = (reader.IsDBNull((int)WorkingSetTemplatesGetWithRelationshipsIndices.MagpieCoreControlSetGuid)) ? (Guid?)null : reader.GetGuid((int)WorkingSetTemplatesGetWithRelationshipsIndices.MagpieCoreControlSetGuid)
                                            }
                                        },
                                        Created = reader.GetDateTime((int)WorkingSetTemplatesGetWithRelationshipsIndices.WorkingSetTemplateCreated),
                                        CreatedByUser = new User
                                        {
                                            Id = reader.GetString((int)WorkingSetTemplatesGetWithRelationshipsIndices.WorkingSetTemplateCreatedByUserId),
                                            UserName = reader.GetString((int)WorkingSetTemplatesGetWithRelationshipsIndices.WorkingSetTemplateCreatedByUserName),
                                        },
                                        Name = reader.GetString((int)WorkingSetTemplatesGetWithRelationshipsIndices.WorkingSetTemplateName),
                                        WorkingSetTemplateGuid = reader.GetGuid((int)WorkingSetTemplatesGetWithRelationshipsIndices.WorkingSetTemplateGuid),
                                        WorkingSetTemplateId = reader.GetInt32((int)WorkingSetTemplatesGetWithRelationshipsIndices.WorkingSetTemplateId)
                                    };

                                    workingSetTemplates.Add(wst);
                                }
                                else
                                {
                                    ((List<ControlSet>)wst.ControlSets).Add(
                                        new ControlSet
                                        {
                                            Code = reader.GetString((int)WorkingSetTemplatesGetWithRelationshipsIndices.ControlSetCode),
                                            Controls = null,
                                            ControlSetClassification = new ControlSetClassification
                                            {
                                                Id = reader.GetInt32((int)WorkingSetTemplatesGetWithRelationshipsIndices.ControlSetClassificationId),
                                                Name = reader.GetString((int)WorkingSetTemplatesGetWithRelationshipsIndices.ControlSetClassificationName),
                                            },
                                            DefinitionSource = new DefinitionSource
                                            {
                                                Code = reader.GetString((int)WorkingSetTemplatesGetWithRelationshipsIndices.ControlSetDefinitionSourceCode),
                                                MagpieCoreDefinitionSourceGuid = (reader.IsDBNull((int)WorkingSetTemplatesGetWithRelationshipsIndices.ControlSetMagpieCoreDefinitionSourceGuid)) ? (Guid?)null : reader.GetGuid((int)WorkingSetTemplatesGetWithRelationshipsIndices.ControlSetMagpieCoreDefinitionSourceGuid),
                                                Id = reader.GetInt32((int)WorkingSetTemplatesGetWithRelationshipsIndices.ControlSetDefinitionSourceId),
                                                Source = reader.GetString((int)WorkingSetTemplatesGetWithRelationshipsIndices.ControlSetDefinitionSource),
                                            },
                                            Id = reader.GetInt32((int)WorkingSetTemplatesGetWithRelationshipsIndices.ControlSetId),
                                            Title = reader.GetString((int)WorkingSetTemplatesGetWithRelationshipsIndices.ControlSetTitle),
                                            Version = reader.GetString((int)WorkingSetTemplatesGetWithRelationshipsIndices.ControlSetVersion),
                                            MagpieCoreControlSetGuid = (reader.IsDBNull((int)WorkingSetTemplatesGetWithRelationshipsIndices.MagpieCoreControlSetGuid)) ? (Guid?)null : reader.GetGuid((int)WorkingSetTemplatesGetWithRelationshipsIndices.MagpieCoreControlSetGuid)
                                        });
                                }

                            }
                        }

                        reader.Close();
                    }
                }

                return workingSetTemplates;
            }
            catch (Exception ex)
            {
                string res = ex.ToString();
                throw;
            }
        }

        public WorkingSetTemplate GetWorkingSetTemplateAndComplianceInfoForSpecificWorkingSetGet(string ConnectionString, int @WorkingSetTemplateId, int @WorkingSetId)
        {
            #region Preconditions

            if (string.IsNullOrWhiteSpace(ConnectionString))
                throw new InvalidOperationException();

            if (WorkingSetTemplateId <= 0)
                throw new ArgumentOutOfRangeException();

            if (WorkingSetId <= 0)
                throw new ArgumentOutOfRangeException();

            #endregion

            try
            {
                var workingSetTemplates = new List<WorkingSetTemplate>();

                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    using (SqlCommand command = new SqlCommand())
                    {
                        string storedProcedureName = "usp_WorkingSetTemplateAndComplianceInfoForSpecificWorkingSetGet";

                        command.CommandType = CommandType.StoredProcedure;
                        command.Connection = connection;
                        command.CommandText = storedProcedureName;

                        command.Parameters.AddWithValue("@WorkingSetTemplateId", WorkingSetTemplateId);
                        command.Parameters.AddWithValue("@WorkingSetId", WorkingSetId);

                        connection.Open();

                        var reader = command.ExecuteReader();

                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                var id = reader.GetInt32((int)WorkingSetTemplateAndComplianceInfoForSpecificWorkingSetGetIndices.WorkingSetTemplateId);

                                var wst = workingSetTemplates.Find(i => i.WorkingSetTemplateId == id);

                                if (wst == null)
                                {
                                    wst = new WorkingSetTemplate
                                    {
                                        ControlSets = new List<ControlSet>
                                        {
                                            new ControlSet
                                            {
                                                Code = reader.GetString((int)WorkingSetTemplateAndComplianceInfoForSpecificWorkingSetGetIndices.ControlSetCode),
                                                Controls = null,
                                                ControlSetClassification = new ControlSetClassification
                                                {
                                                    Id = reader.GetInt32((int)WorkingSetTemplateAndComplianceInfoForSpecificWorkingSetGetIndices.ControlSetClassificationId),
                                                    Name = reader.GetString((int)WorkingSetTemplateAndComplianceInfoForSpecificWorkingSetGetIndices.ControlSetClassificationName),
                                                },
                                                ControlSetCompliance = (reader.IsDBNull((int)WorkingSetTemplateAndComplianceInfoForSpecificWorkingSetGetIndices.ControlSetCompliance)) ? (int?)null : reader.GetInt32((int)WorkingSetTemplateAndComplianceInfoForSpecificWorkingSetGetIndices.ControlSetCompliance),
                                                DefinitionSource = new DefinitionSource
                                                {
                                                    Code = reader.GetString((int)WorkingSetTemplateAndComplianceInfoForSpecificWorkingSetGetIndices.ControlSetDefinitionSourceCode),
                                                    MagpieCoreDefinitionSourceGuid = (reader.IsDBNull((int)WorkingSetTemplateAndComplianceInfoForSpecificWorkingSetGetIndices.ControlSetMagpieCoreDefinitionSourceGuid)) ? (Guid?)null : reader.GetGuid((int)WorkingSetTemplateAndComplianceInfoForSpecificWorkingSetGetIndices.ControlSetMagpieCoreDefinitionSourceGuid),
                                                    Id = reader.GetInt32((int)WorkingSetTemplateAndComplianceInfoForSpecificWorkingSetGetIndices.ControlSetDefinitionSourceId),
                                                    Source = reader.GetString((int)WorkingSetTemplateAndComplianceInfoForSpecificWorkingSetGetIndices.ControlSetDefinitionSource),
                                                },
                                                Id = reader.GetInt32((int)WorkingSetTemplateAndComplianceInfoForSpecificWorkingSetGetIndices.ControlSetId),
                                                Title = reader.GetString((int)WorkingSetTemplateAndComplianceInfoForSpecificWorkingSetGetIndices.ControlSetTitle),
                                                Version = reader.GetString((int)WorkingSetTemplateAndComplianceInfoForSpecificWorkingSetGetIndices.ControlSetVersion),
                                                MagpieCoreControlSetGuid = (reader.IsDBNull((int)WorkingSetTemplateAndComplianceInfoForSpecificWorkingSetGetIndices.MagpieCoreControlSetGuid)) ? (Guid?)null : reader.GetGuid((int)WorkingSetTemplateAndComplianceInfoForSpecificWorkingSetGetIndices.MagpieCoreControlSetGuid)
                                            }
                                        },
                                        Created = reader.GetDateTime((int)WorkingSetTemplateAndComplianceInfoForSpecificWorkingSetGetIndices.WorkingSetTemplateCreated),
                                        CreatedByUser = new User
                                        {
                                            Id = reader.GetString((int)WorkingSetTemplateAndComplianceInfoForSpecificWorkingSetGetIndices.WorkingSetTemplateCreatedByUserId),
                                            UserName = reader.GetString((int)WorkingSetTemplateAndComplianceInfoForSpecificWorkingSetGetIndices.WorkingSetTemplateCreatedByUserName),
                                        },
                                        Name = reader.GetString((int)WorkingSetTemplateAndComplianceInfoForSpecificWorkingSetGetIndices.WorkingSetTemplateName),
                                        WorkingSetTemplateGuid = reader.GetGuid((int)WorkingSetTemplateAndComplianceInfoForSpecificWorkingSetGetIndices.WorkingSetTemplateGuid),
                                        WorkingSetTemplateId = reader.GetInt32((int)WorkingSetTemplateAndComplianceInfoForSpecificWorkingSetGetIndices.WorkingSetTemplateId)
                                    };

                                    workingSetTemplates.Add(wst);
                                }
                                else
                                {
                                    ((List<ControlSet>)wst.ControlSets).Add(
                                        new ControlSet
                                        {
                                            Code = reader.GetString((int)WorkingSetTemplateAndComplianceInfoForSpecificWorkingSetGetIndices.ControlSetCode),
                                            Controls = null,
                                            ControlSetClassification = new ControlSetClassification
                                            {
                                                Id = reader.GetInt32((int)WorkingSetTemplateAndComplianceInfoForSpecificWorkingSetGetIndices.ControlSetClassificationId),
                                                Name = reader.GetString((int)WorkingSetTemplateAndComplianceInfoForSpecificWorkingSetGetIndices.ControlSetClassificationName),
                                            },
                                            ControlSetCompliance = (reader.IsDBNull((int)WorkingSetTemplateAndComplianceInfoForSpecificWorkingSetGetIndices.ControlSetCompliance)) ? (int?)null : reader.GetInt32((int)WorkingSetTemplateAndComplianceInfoForSpecificWorkingSetGetIndices.ControlSetCompliance),
                                            DefinitionSource = new DefinitionSource
                                            {
                                                Code = reader.GetString((int)WorkingSetTemplateAndComplianceInfoForSpecificWorkingSetGetIndices.ControlSetDefinitionSourceCode),
                                                MagpieCoreDefinitionSourceGuid = (reader.IsDBNull((int)WorkingSetTemplateAndComplianceInfoForSpecificWorkingSetGetIndices.ControlSetMagpieCoreDefinitionSourceGuid)) ? (Guid?)null : reader.GetGuid((int)WorkingSetTemplateAndComplianceInfoForSpecificWorkingSetGetIndices.ControlSetMagpieCoreDefinitionSourceGuid),
                                                Id = reader.GetInt32((int)WorkingSetTemplateAndComplianceInfoForSpecificWorkingSetGetIndices.ControlSetDefinitionSourceId),
                                                Source = reader.GetString((int)WorkingSetTemplateAndComplianceInfoForSpecificWorkingSetGetIndices.ControlSetDefinitionSource),
                                            },
                                            Id = reader.GetInt32((int)WorkingSetTemplateAndComplianceInfoForSpecificWorkingSetGetIndices.ControlSetId),
                                            Title = reader.GetString((int)WorkingSetTemplateAndComplianceInfoForSpecificWorkingSetGetIndices.ControlSetTitle),
                                            Version = reader.GetString((int)WorkingSetTemplateAndComplianceInfoForSpecificWorkingSetGetIndices.ControlSetVersion),
                                            MagpieCoreControlSetGuid = (reader.IsDBNull((int)WorkingSetTemplateAndComplianceInfoForSpecificWorkingSetGetIndices.MagpieCoreControlSetGuid)) ? (Guid?)null : reader.GetGuid((int)WorkingSetTemplateAndComplianceInfoForSpecificWorkingSetGetIndices.MagpieCoreControlSetGuid)
                                        });
                                }

                            }
                        }

                        reader.Close();
                    }
                }

                if (workingSetTemplates.Count() != 1)
                    throw new Exception();

                var workingSetTemplate = (workingSetTemplates.Count() == 1) ? workingSetTemplates.First() : null;

                return workingSetTemplate;
            }
            catch (Exception ex)
            {
                string res = ex.ToString();
                throw;
            }
        }
    }
}
