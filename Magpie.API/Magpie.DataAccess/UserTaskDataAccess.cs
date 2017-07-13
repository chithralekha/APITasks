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
    public sealed class UserTaskDataAccess
    {
        private static volatile UserTaskDataAccess instance;
        private static object syncRoot = new Object();

        private UserTaskDataAccess() { }

        public static UserTaskDataAccess Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                            instance = new UserTaskDataAccess();
                    }
                }

                return instance;
            }
        }

        #region Enums

        private enum UserTaskWithRaciRolesIndices
        {
            UserTaskId,
            Code,
            Title,
            ControlId,
            ControlCode,
            ControlTitle,
            TaskDefinitionId,
            WorkingSetId,
            Created,
            CreatedByUserId,
            TaskStateId,
            TaskState,
            Description,
            Due,
            DueStatusId,
            DueStatus,
            Completed,
            Link,
            RaciRoleId,
            RaciRole,
            RaciRoleUserId,
            RaciRoleUserName,
            RaciRoleUserFirstName,
            RaciRoleUserLastName,
            RaciRoleUserEmail,
        }

        private enum UserTaskCommentsGetIndices
        {
            Id,
            UserTaskId,
            Text,
            LastModified,
            LastModifiedByUserId,
            LastModifiedByUserEmail,
            LastModifiedByUserFirstName,
            LastModifiedByUserLastName,
            LastModifiedByUserName
        }
        #endregion

        public IEnumerable<UserTask> GetItems(string ConnectionString)
        {
            #region Preconditions

            if (string.IsNullOrWhiteSpace(ConnectionString))
                throw new InvalidOperationException();

            #endregion

            var userTasks = GetItems(ConnectionString, null);
            return userTasks;
        }

        public UserTask GetItem(string ConnectionString, int Id)
        {
            #region Preconditions

            if (string.IsNullOrWhiteSpace(ConnectionString))
                throw new InvalidOperationException();

            if (Id <= 0)
                throw new ArgumentOutOfRangeException();

            #endregion

            var userTasks = GetItems(ConnectionString, new UserTaskFilter { UserTaskId = Id });

            if (userTasks.Count() != 1)
                throw new Exception();

            var userTask = userTasks.First();
            return userTask;
        }

        public IEnumerable<UserTask> GetItems(string ConnectionString, int FilterId, int? OverrideWorkingSetId)
        {
            #region Preconditions

            if (string.IsNullOrWhiteSpace(ConnectionString))
                throw new InvalidOperationException();

            if (FilterId <= 0)
                throw new ArgumentOutOfRangeException();

            if (OverrideWorkingSetId != null && OverrideWorkingSetId <= 0)
                throw new ArgumentOutOfRangeException();

            #endregion

            try
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    using (SqlCommand command = new SqlCommand())
                    {
                        string storedProcedureName = "usp_GetUserTasksWithRaciRolesByFilter";

                        var userTasks = new List<UserTask>();


                        command.CommandType = CommandType.StoredProcedure;
                        command.Connection = connection;
                        command.CommandText = storedProcedureName;

                        command.Parameters.AddWithValue("@FilterId", FilterId);

                        SqlParameter outPutCount = new SqlParameter("@Count", SqlDbType.Int);
                        outPutCount.Direction = ParameterDirection.Output;
                        command.Parameters.Add(outPutCount);

                        if (OverrideWorkingSetId != null)
                        {
                            command.Parameters.AddWithValue("@OverrideWorkingSetId", OverrideWorkingSetId);
                        }

                        connection.Open();

                        var reader = command.ExecuteReader();

                        if (reader.HasRows)
                        {
                            UserTask currentUserTask = null;

                            while (reader.Read())
                            {
                                var newId = reader.GetInt32((int)UserTaskWithRaciRolesIndices.UserTaskId);

                                if (currentUserTask != null && newId == currentUserTask.Id)
                                {
                                    #region RACI Roles

                                    if (!reader.IsDBNull((int)UserTaskWithRaciRolesIndices.RaciRoleId))
                                    {
                                        var raciRoleId = reader.GetInt32((int)UserTaskWithRaciRolesIndices.RaciRoleId);
                                        var raciRole = reader.GetString((int)UserTaskWithRaciRolesIndices.RaciRole);
                                        var raciRoleUserId = reader.GetString((int)UserTaskWithRaciRolesIndices.RaciRoleUserId);
                                        var raciRoleUserName = reader.GetString((int)UserTaskWithRaciRolesIndices.RaciRoleUserName);

                                        string raciRoleFirstName = null;
                                        string raciRoleLastName = null;
                                        string raciRoleEmail = null;

                                        if (!reader.IsDBNull((int)UserTaskWithRaciRolesIndices.RaciRoleUserFirstName))
                                            raciRoleFirstName = reader.GetString((int)UserTaskWithRaciRolesIndices.RaciRoleUserFirstName);

                                        if (!reader.IsDBNull((int)UserTaskWithRaciRolesIndices.RaciRoleUserLastName))
                                            raciRoleLastName = reader.GetString((int)UserTaskWithRaciRolesIndices.RaciRoleUserLastName);

                                        if (!reader.IsDBNull((int)UserTaskWithRaciRolesIndices.RaciRoleUserEmail))
                                            raciRoleEmail = reader.GetString((int)UserTaskWithRaciRolesIndices.RaciRoleUserEmail);

                                        var raciRoleUser = new User
                                        {
                                            Email = raciRoleEmail,
                                            FirstName = raciRoleFirstName,
                                            Id = raciRoleUserId,
                                            LastName = raciRoleLastName,
                                            UserName = raciRoleUserName
                                        };

                                        switch (raciRole)
                                        {
                                            case "Responsible":
                                                {
                                                    currentUserTask.RaciTeam.ResponsibleUser = raciRoleUser;
                                                }
                                                break;
                                            case "Accountable":
                                                {
                                                    currentUserTask.RaciTeam.AccountableUser = raciRoleUser;
                                                }
                                                break;
                                            case "Consulted":
                                                {
                                                    ((List<User>)currentUserTask.RaciTeam.ConsultedUsers).Add(raciRoleUser);
                                                }
                                                break;
                                            case "Informed":
                                                {
                                                    ((List<User>)currentUserTask.RaciTeam.InformedUsers).Add(raciRoleUser);
                                                }
                                                break;
                                            default:
                                                throw new Exception("Unexpected RACI Role");
                                        }
                                    }

                                    #endregion
                                }
                                else
                                {
                                    var ut = new UserTask();
                                    currentUserTask = ut;

                                    ut.Id = reader.GetInt32((int)UserTaskWithRaciRolesIndices.UserTaskId);
                                    ut.Code = reader.GetString((int)UserTaskWithRaciRolesIndices.Code);
                                    ut.Title = reader.GetString((int)UserTaskWithRaciRolesIndices.Title);
                                    ut.ControlId = reader.GetInt32((int)UserTaskWithRaciRolesIndices.ControlId);
                                    ut.ControlCode = reader.GetString((int)UserTaskWithRaciRolesIndices.ControlCode);
                                    ut.ControlTitle = reader.GetString((int)UserTaskWithRaciRolesIndices.ControlTitle);

                                    if (!reader.IsDBNull((int)UserTaskWithRaciRolesIndices.TaskDefinitionId))
                                        ut.TaskDefinitionId = reader.GetInt32((int)UserTaskWithRaciRolesIndices.TaskDefinitionId);

                                    ut.WorkingSetId = reader.GetInt32((int)UserTaskWithRaciRolesIndices.WorkingSetId);
                                    ut.Created = reader.GetDateTime((int)UserTaskWithRaciRolesIndices.Created);
                                    ut.CreatedByUserId = reader.GetString((int)UserTaskWithRaciRolesIndices.CreatedByUserId);

                                    ut.TaskState = new TaskState { Id = reader.GetInt32((int)UserTaskWithRaciRolesIndices.TaskStateId), Name = reader.GetString((int)UserTaskWithRaciRolesIndices.TaskState) };

                                    if (!reader.IsDBNull((int)UserTaskWithRaciRolesIndices.Description))
                                        ut.Description = reader.GetString((int)UserTaskWithRaciRolesIndices.Description);

                                    if (!reader.IsDBNull((int)UserTaskWithRaciRolesIndices.Due))
                                        ut.Due = reader.GetDateTime((int)UserTaskWithRaciRolesIndices.Due);

                                    ut.DueStatus = new DueStatus { Id = reader.GetInt32((int)UserTaskWithRaciRolesIndices.DueStatusId), Status = reader.GetString((int)UserTaskWithRaciRolesIndices.DueStatus) };

                                    if (!reader.IsDBNull((int)UserTaskWithRaciRolesIndices.Completed))
                                        ut.Completed = reader.GetDateTime((int)UserTaskWithRaciRolesIndices.Completed);

                                    if (!reader.IsDBNull((int)UserTaskWithRaciRolesIndices.Link))
                                        ut.Link = reader.GetString((int)UserTaskWithRaciRolesIndices.Link);

                                    ut.RaciTeam = new RaciTeam
                                    {
                                        ConsultedUsers = new List<User>(),
                                        InformedUsers = new List<User>()
                                    };

                                    #region RACI Roles

                                    if (!reader.IsDBNull((int)UserTaskWithRaciRolesIndices.RaciRoleId))
                                    {
                                        var raciRoleId = reader.GetInt32((int)UserTaskWithRaciRolesIndices.RaciRoleId);
                                        var raciRole = reader.GetString((int)UserTaskWithRaciRolesIndices.RaciRole);
                                        var raciRoleUserId = reader.GetString((int)UserTaskWithRaciRolesIndices.RaciRoleUserId);
                                        var raciRoleUserName = reader.GetString((int)UserTaskWithRaciRolesIndices.RaciRoleUserName);

                                        string raciRoleFirstName = null;
                                        string raciRoleLastName = null;
                                        string raciRoleEmail = null;

                                        if (!reader.IsDBNull((int)UserTaskWithRaciRolesIndices.RaciRoleUserFirstName))
                                            raciRoleFirstName = reader.GetString((int)UserTaskWithRaciRolesIndices.RaciRoleUserFirstName);

                                        if (!reader.IsDBNull((int)UserTaskWithRaciRolesIndices.RaciRoleUserLastName))
                                            raciRoleLastName = reader.GetString((int)UserTaskWithRaciRolesIndices.RaciRoleUserLastName);

                                        if (!reader.IsDBNull((int)UserTaskWithRaciRolesIndices.RaciRoleUserEmail))
                                            raciRoleEmail = reader.GetString((int)UserTaskWithRaciRolesIndices.RaciRoleUserEmail);

                                        var raciRoleUser = new User
                                        {
                                            Email = raciRoleEmail,
                                            FirstName = raciRoleFirstName,
                                            Id = raciRoleUserId,
                                            LastName = raciRoleLastName,
                                            UserName = raciRoleUserName
                                        };

                                        switch (raciRole)
                                        {
                                            case "Responsible":
                                                {
                                                    ut.RaciTeam.ResponsibleUser = raciRoleUser;
                                                }
                                                break;
                                            case "Accountable":
                                                {
                                                    ut.RaciTeam.AccountableUser = raciRoleUser;
                                                }
                                                break;
                                            case "Consulted":
                                                {
                                                    ((List<User>)ut.RaciTeam.ConsultedUsers).Add(raciRoleUser);
                                                }
                                                break;
                                            case "Informed":
                                                {
                                                    ((List<User>)ut.RaciTeam.InformedUsers).Add(raciRoleUser);
                                                }
                                                break;
                                            default:
                                                throw new Exception("Unexpected RACI Role");
                                        }
                                    }

                                    #endregion

                                    userTasks.Add(ut);
                                }
                            }
                        }

                        reader.Close();

                        return userTasks;
                    }
                }
            }
            catch (SqlException ex)
            {
                string res = ex.ToString();
                throw;
            }
        }

        public IEnumerable<UserTask> GetItems(string ConnectionString, int FilterId)
        {
            #region Preconditions

            if (string.IsNullOrWhiteSpace(ConnectionString))
                throw new InvalidOperationException();

            if (FilterId <= 0)
                throw new ArgumentOutOfRangeException();
            #endregion

            return GetItems(ConnectionString, FilterId, null);
        }

        public IEnumerable<UserTask> GetItems(string ConnectionString, UserTaskFilter Filter)
        {
            #region Preconditions

            if (string.IsNullOrWhiteSpace(ConnectionString))
                throw new InvalidOperationException();

            #endregion

            try
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    var userTasks = new List<UserTask>();
                    var comments = new List<Comment>();
                    var ut = new UserTask();
                    using (SqlCommand command = new SqlCommand())
                    {
                        string storedProcedureName = "usp_GetUserTasksWithRaciRoles";
                        command.CommandType = CommandType.StoredProcedure;
                        command.Connection = connection;
                        command.CommandText = storedProcedureName;

                        if (Filter != null)
                        {
                            if (Filter.ControlId != null)
                                command.Parameters.AddWithValue("@ControlId", Filter.ControlId);

                            if (Filter.ControlSetId != null)
                                command.Parameters.AddWithValue("@ControlSetId", Filter.ControlSetId);

                            if (!string.IsNullOrWhiteSpace(Filter.ResponsibleUserId))
                                command.Parameters.AddWithValue("@ResponsibleUserId", Filter.ResponsibleUserId);

                            if (Filter.TaskStateId != null)
                                command.Parameters.AddWithValue("@TaskStateId", Filter.TaskStateId);

                            if (!string.IsNullOrWhiteSpace(Filter.UserTaskCode))
                                command.Parameters.AddWithValue("@UserTaskCode", Filter.UserTaskCode);

                            if (Filter.UserTaskId != null)
                                command.Parameters.AddWithValue("@UserTaskId", Filter.UserTaskId);

                            if (Filter.WorkingSetId != null)
                                command.Parameters.AddWithValue("@WorkingSetId", Filter.WorkingSetId);

                            //###############
                            // ADD THESE!!!!!!
                            //AssignedStatusId
                            //DueEndDate
                            //DueStartDate
                            //DueStatusId
                            //IncludeRelations
                            //###############
                        }

                        connection.Open();

                        var reader = command.ExecuteReader();

                        if (reader.HasRows)
                        {
                            UserTask currentUserTask = null;

                            while (reader.Read())
                            {
                                var newId = reader.GetInt32((int)UserTaskWithRaciRolesIndices.UserTaskId);

                                if (currentUserTask != null && newId == currentUserTask.Id)
                                {
                                    #region RACI Roles

                                    if (!reader.IsDBNull((int)UserTaskWithRaciRolesIndices.RaciRoleId))
                                    {
                                        var raciRoleId = reader.GetInt32((int)UserTaskWithRaciRolesIndices.RaciRoleId);
                                        var raciRole = reader.GetString((int)UserTaskWithRaciRolesIndices.RaciRole);
                                        var raciRoleUserId = reader.GetString((int)UserTaskWithRaciRolesIndices.RaciRoleUserId);
                                        var raciRoleUserName = reader.GetString((int)UserTaskWithRaciRolesIndices.RaciRoleUserName);

                                        string raciRoleFirstName = null;
                                        string raciRoleLastName = null;
                                        string raciRoleEmail = null;

                                        if (!reader.IsDBNull((int)UserTaskWithRaciRolesIndices.RaciRoleUserFirstName))
                                            raciRoleFirstName = reader.GetString((int)UserTaskWithRaciRolesIndices.RaciRoleUserFirstName);

                                        if (!reader.IsDBNull((int)UserTaskWithRaciRolesIndices.RaciRoleUserLastName))
                                            raciRoleLastName = reader.GetString((int)UserTaskWithRaciRolesIndices.RaciRoleUserLastName);

                                        if (!reader.IsDBNull((int)UserTaskWithRaciRolesIndices.RaciRoleUserEmail))
                                            raciRoleEmail = reader.GetString((int)UserTaskWithRaciRolesIndices.RaciRoleUserEmail);

                                        var raciRoleUser = new User
                                        {
                                            Email = raciRoleEmail,
                                            FirstName = raciRoleFirstName,
                                            Id = raciRoleUserId,
                                            LastName = raciRoleLastName,
                                            UserName = raciRoleUserName
                                        };

                                        switch (raciRole)
                                        {
                                            case "Responsible":
                                                {
                                                    currentUserTask.RaciTeam.ResponsibleUser = raciRoleUser;
                                                }
                                                break;
                                            case "Accountable":
                                                {
                                                    currentUserTask.RaciTeam.AccountableUser = raciRoleUser;
                                                }
                                                break;
                                            case "Consulted":
                                                {
                                                    ((List<User>)currentUserTask.RaciTeam.ConsultedUsers).Add(raciRoleUser);
                                                }
                                                break;
                                            case "Informed":
                                                {
                                                    ((List<User>)currentUserTask.RaciTeam.InformedUsers).Add(raciRoleUser);
                                                }
                                                break;
                                            default:
                                                throw new Exception("Unexpected RACI Role");
                                        }
                                    }

                                    #endregion
                                }
                                else
                                {

                                    currentUserTask = ut;

                                    ut.Id = reader.GetInt32((int)UserTaskWithRaciRolesIndices.UserTaskId);
                                    ut.Code = reader.GetString((int)UserTaskWithRaciRolesIndices.Code);
                                    ut.Title = reader.GetString((int)UserTaskWithRaciRolesIndices.Title);
                                    ut.ControlId = reader.GetInt32((int)UserTaskWithRaciRolesIndices.ControlId);
                                    ut.ControlCode = reader.GetString((int)UserTaskWithRaciRolesIndices.ControlCode);
                                    ut.ControlTitle = reader.GetString((int)UserTaskWithRaciRolesIndices.ControlTitle);

                                    if (!reader.IsDBNull((int)UserTaskWithRaciRolesIndices.TaskDefinitionId))
                                        ut.TaskDefinitionId = reader.GetInt32((int)UserTaskWithRaciRolesIndices.TaskDefinitionId);

                                    ut.WorkingSetId = reader.GetInt32((int)UserTaskWithRaciRolesIndices.WorkingSetId);
                                    ut.Created = reader.GetDateTime((int)UserTaskWithRaciRolesIndices.Created);
                                    ut.CreatedByUserId = reader.GetString((int)UserTaskWithRaciRolesIndices.CreatedByUserId);

                                    ut.TaskState = new TaskState { Id = reader.GetInt32((int)UserTaskWithRaciRolesIndices.TaskStateId), Name = reader.GetString((int)UserTaskWithRaciRolesIndices.TaskState) };

                                    if (!reader.IsDBNull((int)UserTaskWithRaciRolesIndices.Description))
                                        ut.Description = reader.GetString((int)UserTaskWithRaciRolesIndices.Description);

                                    if (!reader.IsDBNull((int)UserTaskWithRaciRolesIndices.Due))
                                        ut.Due = reader.GetDateTime((int)UserTaskWithRaciRolesIndices.Due);

                                    ut.DueStatus = new DueStatus { Id = reader.GetInt32((int)UserTaskWithRaciRolesIndices.DueStatusId), Status = reader.GetString((int)UserTaskWithRaciRolesIndices.DueStatus) };

                                    if (!reader.IsDBNull((int)UserTaskWithRaciRolesIndices.Completed))
                                        ut.Completed = reader.GetDateTime((int)UserTaskWithRaciRolesIndices.Completed);

                                    if (!reader.IsDBNull((int)UserTaskWithRaciRolesIndices.Link))
                                        ut.Link = reader.GetString((int)UserTaskWithRaciRolesIndices.Link);

                                    ut.RaciTeam = new RaciTeam
                                    {
                                        ConsultedUsers = new List<User>(),
                                        InformedUsers = new List<User>()
                                    };

                                    #region RACI Roles

                                    if (!reader.IsDBNull((int)UserTaskWithRaciRolesIndices.RaciRoleId))
                                    {
                                        var raciRoleId = reader.GetInt32((int)UserTaskWithRaciRolesIndices.RaciRoleId);
                                        var raciRole = reader.GetString((int)UserTaskWithRaciRolesIndices.RaciRole);
                                        var raciRoleUserId = reader.GetString((int)UserTaskWithRaciRolesIndices.RaciRoleUserId);
                                        var raciRoleUserName = reader.GetString((int)UserTaskWithRaciRolesIndices.RaciRoleUserName);

                                        string raciRoleFirstName = null;
                                        string raciRoleLastName = null;
                                        string raciRoleEmail = null;

                                        if (!reader.IsDBNull((int)UserTaskWithRaciRolesIndices.RaciRoleUserFirstName))
                                            raciRoleFirstName = reader.GetString((int)UserTaskWithRaciRolesIndices.RaciRoleUserFirstName);

                                        if (!reader.IsDBNull((int)UserTaskWithRaciRolesIndices.RaciRoleUserLastName))
                                            raciRoleLastName = reader.GetString((int)UserTaskWithRaciRolesIndices.RaciRoleUserLastName);

                                        if (!reader.IsDBNull((int)UserTaskWithRaciRolesIndices.RaciRoleUserEmail))
                                            raciRoleEmail = reader.GetString((int)UserTaskWithRaciRolesIndices.RaciRoleUserEmail);

                                        var raciRoleUser = new User
                                        {
                                            Email = raciRoleEmail,
                                            FirstName = raciRoleFirstName,
                                            Id = raciRoleUserId,
                                            LastName = raciRoleLastName,
                                            UserName = raciRoleUserName
                                        };

                                        switch (raciRole)
                                        {
                                            case "Responsible":
                                                {
                                                    ut.RaciTeam.ResponsibleUser = raciRoleUser;
                                                }
                                                break;
                                            case "Accountable":
                                                {
                                                    ut.RaciTeam.AccountableUser = raciRoleUser;
                                                }
                                                break;
                                            case "Consulted":
                                                {
                                                    ((List<User>)ut.RaciTeam.ConsultedUsers).Add(raciRoleUser);
                                                }
                                                break;
                                            case "Informed":
                                                {
                                                    ((List<User>)ut.RaciTeam.InformedUsers).Add(raciRoleUser);
                                                }
                                                break;
                                            default:
                                                throw new Exception("Unexpected RACI Role");
                                        }
                                    }

                                    #endregion


                                }
                            }
                        }

                        reader.Close();
                    }

                    if (Filter != null && Filter.UserTaskId != null)
                    {
                        using (SqlCommand command = new SqlCommand())
                        {
                            command.CommandType = CommandType.StoredProcedure;
                            command.Connection = connection;
                            command.CommandText = "usp_UserTaskCommentsGet";

                            command.Parameters.AddWithValue("@UserTaskId", Filter.UserTaskId);
                            var reader = command.ExecuteReader();

                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    var commentId = reader.GetInt32((int)UserTaskCommentsGetIndices.Id);
                                    var commentText = reader.GetString((int)UserTaskCommentsGetIndices.Text);
                                    var commentLastModified = reader.GetDateTime((int)UserTaskCommentsGetIndices.LastModified);
                                    var commentLastModifiedByUserId = reader.GetString((int)UserTaskCommentsGetIndices.LastModifiedByUserId);
                                    var email = reader.IsDBNull((int)UserTaskCommentsGetIndices.LastModifiedByUserEmail) ? null : reader.GetString((int)UserTaskCommentsGetIndices.LastModifiedByUserEmail);
                                    var firstName = reader.IsDBNull((int)UserTaskCommentsGetIndices.LastModifiedByUserFirstName) ? null : reader.GetString((int)UserTaskCommentsGetIndices.LastModifiedByUserFirstName);
                                    var LastName = reader.IsDBNull((int)UserTaskCommentsGetIndices.LastModifiedByUserLastName) ? null : reader.GetString((int)UserTaskCommentsGetIndices.LastModifiedByUserLastName);
                                    var userName = reader.IsDBNull((int)UserTaskCommentsGetIndices.LastModifiedByUserName) ? null : reader.GetString((int)UserTaskCommentsGetIndices.LastModifiedByUserName);

                                    var comment = new Comment
                                    {
                                        Id = commentId,
                                        Text = commentText,
                                        LastModified = commentLastModified,
                                        LastModifiedByUser = new User
                                        {
                                            Email = email,
                                            FirstName = firstName,
                                            Id = commentLastModifiedByUserId,
                                            LastName = LastName,
                                            UserName = userName
                                        }
                                    };
                                    comments.Add(comment);
                                }
                            }
                            reader.Close();

                            ut.Comments = comments;
                            userTasks.Add(ut);

                            connection.Close();
                        }
                    }

                    return userTasks;
                }
            }
            catch (SqlException ex)
            {
                string res = ex.ToString();
                throw;
            }
        }

        public int? Create(string ConnectionString, UserTask UserTask)
        {
            #region Preconditions

            if (string.IsNullOrWhiteSpace(ConnectionString))
                throw new InvalidOperationException();

            if (UserTask == null)
                throw new ArgumentOutOfRangeException();

            #endregion

            try
            {
                string storedProcedureName = "usp_UserTaskCreate";

                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    int? newId = null;
                    using (SqlCommand command = new SqlCommand())
                    {

                        command.CommandType = CommandType.StoredProcedure;
                        command.Connection = connection;
                        command.CommandText = storedProcedureName;

                        command.Parameters.AddWithValue("@Code", UserTask.Code);
                        command.Parameters.AddWithValue("@Title", UserTask.Title);
                        command.Parameters.AddWithValue("@ControlId", UserTask.ControlId);

                        if (UserTask.TaskDefinitionId != null)
                            command.Parameters.AddWithValue("@TaskDefinitionId", UserTask.TaskDefinitionId);

                        command.Parameters.AddWithValue("@WorkingSetId", UserTask.WorkingSetId);
                        command.Parameters.AddWithValue("@CreatedByUserId", UserTask.CreatedByUserId);
                        command.Parameters.AddWithValue("@TaskStateId", UserTask.TaskState.Id);
                        command.Parameters.AddWithValue("@IsRootTask", 1);
                        command.Parameters.AddWithValue("@IsRecurring", 0);
                        command.Parameters.AddWithValue("@RecurranceFrequencyId", 1);
                        command.Parameters.AddWithValue("@NumberOfOccurrences", 2);
                        if (!string.IsNullOrWhiteSpace(UserTask.Description))
                            command.Parameters.AddWithValue("@Description", UserTask.Description);

                        if (UserTask.Due != null && UserTask.Due.Value.Year < 20000 && UserTask.Due.Value.Year > 1753)
                            command.Parameters.AddWithValue("@Due", UserTask.Due);

                        if (UserTask.Completed != null && UserTask.Completed.Value.Year < 20000 && UserTask.Completed.Value.Year > 1753)
                            command.Parameters.AddWithValue("@Completed", UserTask.Completed);

                        if (!string.IsNullOrWhiteSpace(UserTask.Link))
                            command.Parameters.AddWithValue("@Link", UserTask.Link);

                        SqlParameter outPutVal = new SqlParameter("@NewId", SqlDbType.Int);
                        outPutVal.Direction = ParameterDirection.Output;
                        command.Parameters.Add(outPutVal);

                        connection.Open();
                        command.ExecuteNonQuery();
                        connection.Close();

                        if (outPutVal.Value != DBNull.Value)
                            newId = Convert.ToInt32(outPutVal.Value);


                    }
                    using (SqlCommand command = new SqlCommand())
                    {
                        bool changes = false;
                        command.CommandType = CommandType.StoredProcedure;
                        command.Connection = connection;
                        command.CommandText = "usp_UserTaskRaciRolesUpdate";

                        command.Parameters.AddWithValue("@UserTaskId", newId);

                        if (UserTask.RaciTeam != null)
                        {
                            if (UserTask.RaciTeam.ResponsibleUser != null)
                            {
                                changes = true;
                                command.Parameters.AddWithValue("@ResponsibleUserId", UserTask.RaciTeam.ResponsibleUser.Id);
                            }

                            if (UserTask.RaciTeam.AccountableUser != null)
                            {
                                changes = true;
                                command.Parameters.AddWithValue("@AccountableUserId", UserTask.RaciTeam.AccountableUser.Id);
                            }

                            var dataTableConsultedUsers = new DataTable();
                            dataTableConsultedUsers.Columns.Add("UserId", typeof(string));
                            changes = true;

                            if (UserTask.RaciTeam.ConsultedUsers != null && UserTask.RaciTeam.ConsultedUsers.Count() > 0)
                            {
                                foreach (var item in UserTask.RaciTeam.ConsultedUsers)
                                {
                                    dataTableConsultedUsers.Rows.Add(item.Id);
                                }
                            }

                            SqlParameter controlSetIdsParameterConsultedUsers;
                            controlSetIdsParameterConsultedUsers = command.Parameters.AddWithValue("@ConsultedUserIds", dataTableConsultedUsers);
                            controlSetIdsParameterConsultedUsers.SqlDbType = SqlDbType.Structured;
                            controlSetIdsParameterConsultedUsers.TypeName = "dbo.UserIdTVPType";

                            var dataTableInformedUsers = new DataTable();
                            dataTableInformedUsers.Columns.Add("UserId", typeof(string));
                            changes = true;

                            if (UserTask.RaciTeam.InformedUsers != null && UserTask.RaciTeam.InformedUsers.Count() > 0)
                            {
                                foreach (var item in UserTask.RaciTeam.InformedUsers)
                                {
                                    dataTableInformedUsers.Rows.Add(item.Id);
                                }
                            }

                            SqlParameter controlSetIdsParameterInformedUsers;
                            controlSetIdsParameterInformedUsers = command.Parameters.AddWithValue("@InformedUserIds", dataTableInformedUsers);
                            controlSetIdsParameterInformedUsers.SqlDbType = SqlDbType.Structured;
                            controlSetIdsParameterInformedUsers.TypeName = "dbo.UserIdTVPType";
                        }
                        if (changes)
                        {
                            connection.Open();
                            command.ExecuteNonQuery();
                            connection.Close();
                        }

                    }
                    using (SqlCommand command = new SqlCommand())
                    {
                        bool changes = false;
                        var text = "";
                        var lastModifiedByUserId = "";
                        command.CommandType = CommandType.StoredProcedure;
                        command.Connection = connection;
                        command.CommandText = "usp_UserTaskCommentCreate";

                        command.Parameters.AddWithValue("@UserTaskId", newId);

                        if (UserTask.Comments != null)
                        {

                            if (UserTask.Comments != null && UserTask.Comments.Count() > 0)
                            {
                                foreach (var item in UserTask.Comments)
                                {
                                    if (item.Id == 0)
                                    {
                                        if (!string.IsNullOrWhiteSpace(item.Text))
                                        {
                                            text = item.Text;
                                            lastModifiedByUserId = item.LastModifiedByUser.Id;
                                            changes = true;
                                            break;
                                        }
                                    }
                                }
                            }

                            command.Parameters.AddWithValue("@Text", text);
                            command.Parameters.AddWithValue("@LastModified", System.DateTime.Now);
                            command.Parameters.AddWithValue("@LastModifiedByUserId", lastModifiedByUserId);

                            SqlParameter outPutVal = new SqlParameter("@NewId", SqlDbType.Int);
                            outPutVal.Direction = ParameterDirection.Output;
                            command.Parameters.Add(outPutVal);
                        }
                        if (changes)
                        {
                            connection.Open();
                            command.ExecuteNonQuery();
                            connection.Close();
                        }

                    }

                    return newId;
                }
            }
            catch (Exception e)
            {
                string res = e.ToString();
                throw;
            }
        }

        public bool Update(string ConnectionString, UserTask UserTask)
        {
            #region Preconditions

            if (string.IsNullOrWhiteSpace(ConnectionString))
                throw new InvalidOperationException();

            if (UserTask == null)
                throw new ArgumentNullException();

            #endregion

            try
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    using (SqlCommand command = new SqlCommand())
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Connection = connection;
                        command.CommandText = "usp_UserTaskUpdate";

                        command.Parameters.AddWithValue("@UserTaskId", UserTask.Id);
                        command.Parameters.AddWithValue("@Code", UserTask.Code);
                        command.Parameters.AddWithValue("@Title", UserTask.Title);
                        command.Parameters.AddWithValue("@ControlId", UserTask.ControlId);
                        command.Parameters.AddWithValue("@TaskDefinitionId", UserTask.TaskDefinitionId);
                        command.Parameters.AddWithValue("@WorkingSetId", UserTask.WorkingSetId);
                        command.Parameters.AddWithValue("@TaskStateId", UserTask.TaskState.Id);
                        command.Parameters.AddWithValue("@Description", UserTask.Description);
                        command.Parameters.AddWithValue("@Due", UserTask.Due);
                        if(UserTask.TaskState.Id == 3)
                        {
                            command.Parameters.AddWithValue("@Completed", System.DateTime.Now);
                        }
                        else
                        {
                            command.Parameters.AddWithValue("@Completed", DBNull.Value);
                        }
                        
                        command.Parameters.AddWithValue("@Link", UserTask.Link);

                        connection.Open();
                        command.ExecuteNonQuery();
                        connection.Close();
                    }

                    using (SqlCommand command = new SqlCommand())
                    {
                        bool changes = false;
                        command.CommandType = CommandType.StoredProcedure;
                        command.Connection = connection;
                        command.CommandText = "usp_UserTaskRaciRolesUpdate";

                        command.Parameters.AddWithValue("@UserTaskId", UserTask.Id);

                        if (UserTask.RaciTeam != null)
                        {
                            if (UserTask.RaciTeam.ResponsibleUser != null)
                            {
                                changes = true;
                                command.Parameters.AddWithValue("@ResponsibleUserId", UserTask.RaciTeam.ResponsibleUser.Id);
                            }

                            if (UserTask.RaciTeam.AccountableUser != null)
                            {
                                changes = true;
                                command.Parameters.AddWithValue("@AccountableUserId", UserTask.RaciTeam.AccountableUser.Id);
                            }

                            var dataTableConsultedUsers = new DataTable();
                            dataTableConsultedUsers.Columns.Add("UserId", typeof(string));
                            changes = true;

                            if (UserTask.RaciTeam.ConsultedUsers != null && UserTask.RaciTeam.ConsultedUsers.Count() > 0)
                            {
                                foreach (var item in UserTask.RaciTeam.ConsultedUsers)
                                {
                                    dataTableConsultedUsers.Rows.Add(item.Id);
                                }
                            }

                            SqlParameter controlSetIdsParameterConsultedUsers;
                            controlSetIdsParameterConsultedUsers = command.Parameters.AddWithValue("@ConsultedUserIds", dataTableConsultedUsers);
                            controlSetIdsParameterConsultedUsers.SqlDbType = SqlDbType.Structured;
                            controlSetIdsParameterConsultedUsers.TypeName = "dbo.UserIdTVPType";

                            var dataTableInformedUsers = new DataTable();
                            dataTableInformedUsers.Columns.Add("UserId", typeof(string));
                            changes = true;

                            if (UserTask.RaciTeam.InformedUsers != null && UserTask.RaciTeam.InformedUsers.Count() > 0)
                            {
                                foreach (var item in UserTask.RaciTeam.InformedUsers)
                                {
                                    dataTableInformedUsers.Rows.Add(item.Id);
                                }
                            }

                            SqlParameter controlSetIdsParameterInformedUsers;
                            controlSetIdsParameterInformedUsers = command.Parameters.AddWithValue("@InformedUserIds", dataTableInformedUsers);
                            controlSetIdsParameterInformedUsers.SqlDbType = SqlDbType.Structured;
                            controlSetIdsParameterInformedUsers.TypeName = "dbo.UserIdTVPType";
                        }
                        if (changes)
                        {
                            connection.Open();
                            command.ExecuteNonQuery();
                            connection.Close();
                        }

                    }
                    using (SqlCommand command = new SqlCommand())
                    {
                        bool changes = false;
                        var text = "";
                        var lastModifiedByUserId = "";
                        command.CommandType = CommandType.StoredProcedure;
                        command.Connection = connection;
                        command.CommandText = "usp_UserTaskCommentCreate";

                        command.Parameters.AddWithValue("@UserTaskId", UserTask.Id);

                        if (UserTask.Comments != null)
                        {

                            if (UserTask.Comments != null && UserTask.Comments.Count() > 0)
                            {
                                foreach (var item in UserTask.Comments)
                                {
                                    if (item.Id == 0)
                                    {
                                        if (!string.IsNullOrWhiteSpace(item.Text))
                                        {
                                            text = item.Text;
                                            lastModifiedByUserId = item.LastModifiedByUser.Id;
                                            changes = true;
                                            break;
                                        }
                                    }
                                }
                            }

                            command.Parameters.AddWithValue("@Text", text);
                            command.Parameters.AddWithValue("@LastModified", System.DateTime.Now);
                            command.Parameters.AddWithValue("@LastModifiedByUserId", lastModifiedByUserId);

                            SqlParameter outPutVal = new SqlParameter("@NewId", SqlDbType.Int);
                            outPutVal.Direction = ParameterDirection.Output;
                            command.Parameters.Add(outPutVal);
                        }
                        if (changes)
                        {
                            connection.Open();
                            command.ExecuteNonQuery();
                            connection.Close();
                        }

                    }

                    return true;
                }
            }
            catch (Exception ex)
            {

                throw;
            }
        }


        public void Delete(string ConnectionString, int Id)
        {
            #region Preconditions

            if (string.IsNullOrWhiteSpace(ConnectionString))
                throw new InvalidOperationException();

            if (Id <= 0)
                throw new ArgumentOutOfRangeException();

            #endregion

            try
            {
                string storedProcedureName = "usp_UserTaskDelete";

                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    using (SqlCommand command = new SqlCommand())
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Connection = connection;
                        command.CommandText = storedProcedureName;

                        command.Parameters.AddWithValue("@UserTaskId", Id);

                        connection.Open();
                        command.ExecuteNonQuery();
                        connection.Close();
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
