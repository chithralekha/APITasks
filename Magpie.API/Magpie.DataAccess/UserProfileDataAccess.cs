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
    public sealed class UserProfileDataAccess
    {
        private static volatile UserProfileDataAccess instance;
        private static object syncRoot = new Object();

        private UserProfileDataAccess() { }

        public static UserProfileDataAccess Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                            instance = new UserProfileDataAccess();
                    }
                }

                return instance;
            }
        }

        #region Enums

        private enum UserProfilesGetIndices
        {
            UserId,
            Email,
            EmailConfirmed,
            PasswordHash,
            SecurityStamp,
            PhoneNumber,
            PhoneNumberConfirmed,
            TwoFactorEnabled,
            LockoutEndDateUtc,
            LockoutEnabled,
            AccessFailedCount,
            UserName,
            FirstName,
            LastName,
            IsActive,
            ManagerId,
            ManagerEmail,
            ManagerUserName,
            ManagerFirstName,
            ManagerLastName,
            PhoneCarrierId,
            Carrier,
            EmailForTexts,
            RoleId,
            Role,
            RoleTypeId,
            RoleType
        }

        private enum UserRolesGetIndices
        {
            UserId,
            RoleId,
            Role,
            RoleTypeId,
            RoleType
        }

        private enum UserNotificationsGetIndices
        {
            UserId,
            RaciRoleId,
            RaciRole,
            TaskCreatedNotifyByEmail,
            TaskCreatedNotifyByText,
            TaskStartedNotifyByEmail,
            TaskStartedNotifyByText,
            TaskCompletedNotifyByEmail,
            TaskCompletedNotifyByText,
            TaskInJeopardyNotifyByEmail,
            TaskInJeopardyNotifyByText,
            TaskOverdueNotifyByEmail,
            TaskOverdueNotifyByText,
            IncidentReportCreatedNotifyByEmail,
            IncidentReportCreatedNotifyByText
        }

        private enum RolesGetIndices
        {
            Id,
            Role,
            RoleTypeId,
            RoleType
        }

        private enum RoleTypesGetIndices
        {
            RoleTypeId,
            RoleType
        }

        private enum PhoneCarriersGetIndices
        {
            Id,
            Carrier,
            EmailForTexts
        }

        private enum udt_UserNotificationsIndices
        {
            RaciRoleId,
            TaskCreatedNotifyByEmail,
            TaskCreatedNotifyByText,
            TaskStartedNotifyByEmail,
            TaskStartedNotifyByText,
            TaskCompletedNotifyByEmail,
            TaskCompletedNotifyByText,
            TaskInJeopardyNotifyByEmail,
            TaskInJeopardyNotifyByText,
            TaskOverdueNotifyByEmail,
            TaskOverdueNotifyByText,
            IncidentReportCreatedNotifyByEmail,
            IncidentReportCreatedNotifyByText
        }

        #endregion

        public IEnumerable<Role> GetRoles(string ConnectionString)
        {
            #region Preconditions

            if (string.IsNullOrWhiteSpace(ConnectionString))
                throw new InvalidOperationException();

            #endregion

            try
            {
                string storedProcedureName = "usp_RolesGet";

                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    using (SqlCommand command = new SqlCommand())
                    {
                        var roles = new List<Role>();

                        command.CommandType = CommandType.StoredProcedure;
                        command.Connection = connection;
                        command.CommandText = storedProcedureName;

                        #region No system roles for now...

                        var dataTable = new DataTable();
                        dataTable.Columns.Add("Id", typeof(long));
                        dataTable.Rows.Add(2);

                        SqlParameter controlSetIdsParameter;
                        controlSetIdsParameter = command.Parameters.AddWithValue("@RoleTypes", dataTable);
                        controlSetIdsParameter.SqlDbType = SqlDbType.Structured;
                        controlSetIdsParameter.TypeName = "dbo.IdTVPType";

                        #endregion

                        connection.Open();

                        var reader = command.ExecuteReader();

                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                var r = new Role
                                {
                                    Id = Int32.Parse(reader.GetString((int)RolesGetIndices.Id)),
                                    Name = reader.GetString((int)RolesGetIndices.Role),
                                    RoleType = new RoleType
                                    {
                                        Id = reader.GetInt32((int)RolesGetIndices.RoleTypeId),
                                        Name = reader.GetString((int)RolesGetIndices.RoleType)
                                    }
                                };

                                roles.Add(r);
                            }
                        }

                        reader.Close();

                        return roles;
                    }
                }
            }
            catch (Exception ex)
            {
                string res = ex.ToString();
                throw;
            }
        }

        public IEnumerable<RoleType> GetRoleTypes(string ConnectionString, bool? IncludeSystemRoles = false)
        {
            #region Preconditions

            if (string.IsNullOrWhiteSpace(ConnectionString))
                throw new InvalidOperationException();

            #endregion

            try
            {
                string storedProcedureName = "usp_RoleTypesGet";

                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    using (SqlCommand command = new SqlCommand())
                    {
                        var roleTypes = new List<RoleType>();

                        command.CommandType = CommandType.StoredProcedure;
                        command.Connection = connection;
                        command.CommandText = storedProcedureName;

                        connection.Open();

                        var reader = command.ExecuteReader();

                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                var rt = new RoleType
                                {
                                    Id = Int32.Parse(reader.GetString((int)RolesGetIndices.Id)),
                                    Name = reader.GetString((int)RolesGetIndices.Role)
                                };

                                roleTypes.Add(rt);
                            }
                        }

                        reader.Close();

                        return roleTypes;
                    }
                }
            }
            catch (Exception ex)
            {
                string res = ex.ToString();
                throw;
            }
        }

        public IEnumerable<PhoneCarrier> GetPhoneCarriers(string ConnectionString)
        {
            #region Preconditions

            if (string.IsNullOrWhiteSpace(ConnectionString))
                throw new InvalidOperationException();

            #endregion

            try
            {
                string storedProcedureName = "usp_PhoneCarriersGet";

                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    using (SqlCommand command = new SqlCommand())
                    {
                        var phoneCarriers = new List<PhoneCarrier>();

                        command.CommandType = CommandType.StoredProcedure;
                        command.Connection = connection;
                        command.CommandText = storedProcedureName;

                        connection.Open();

                        var reader = command.ExecuteReader();

                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                var pc = new PhoneCarrier
                                {
                                    Id = reader.GetInt32((int)PhoneCarriersGetIndices.Id),
                                    Carrier = reader.GetString((int)PhoneCarriersGetIndices.Carrier),
                                    EmailForTexts = reader.GetString((int)PhoneCarriersGetIndices.EmailForTexts)
                                };

                                phoneCarriers.Add(pc);
                            }
                        }

                        reader.Close();

                        return phoneCarriers;
                    }
                }
            }
            catch (Exception ex)
            {
                string res = ex.ToString();
                throw;
            }
        }

        public IEnumerable<User> GetUserProfiles(string ConnectionString, string Id = null)
        {
            #region Preconditions

            if (string.IsNullOrWhiteSpace(ConnectionString))
                throw new InvalidOperationException();

            #endregion

            try
            {
                var users = new List<User>();

                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    //################################
                    //################################
                    //################################
                    //################################
                    User FIX_THIS_TEMP_SINGLEUSER = null;
                    //################################
                    //################################
                    //################################
                    //################################

                    using (SqlCommand command = new SqlCommand())
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Connection = connection;
                        command.CommandText = "usp_UserProfilesGet";

                        if (Id != null)
                            command.Parameters.AddWithValue("@UserId", Id);

                        #region TVPs

                        var dataTableRoleIds = new DataTable();
                        dataTableRoleIds.Columns.Add("Id", typeof(long));

                        for (int i = 1; i < 20; i++)
                        {
                            dataTableRoleIds.Rows.Add(i);
                        }

                        SqlParameter parameterUserRoleIds;
                        parameterUserRoleIds = command.Parameters.AddWithValue("@Roles", dataTableRoleIds);
                        parameterUserRoleIds.SqlDbType = SqlDbType.Structured;
                        parameterUserRoleIds.TypeName = "dbo.IdTVPType";

                        var dataTableRoleTypes = new DataTable();
                        dataTableRoleTypes.Columns.Add("Id", typeof(long));

                        //################################
                        //################################
                        //################################
                        //################################
                        // REMOVE HARD-CODED ID!!!!
                        dataTableRoleTypes.Rows.Add(1);
                        dataTableRoleTypes.Rows.Add(2);
                        //################################
                        //################################
                        //################################
                        //################################

                        SqlParameter parameterRoleTypes;
                        parameterRoleTypes = command.Parameters.AddWithValue("@RoleTypes", dataTableRoleTypes);
                        parameterRoleTypes.SqlDbType = SqlDbType.Structured;
                        parameterRoleTypes.TypeName = "dbo.IdTVPType";

                        #endregion

                        connection.Open();

                        var reader = command.ExecuteReader();

                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                var id = reader.GetString((int)UserProfilesGetIndices.UserId);

                                var u = users.Find(i => i.Id == id);

                                if (u == null)
                                {
                                    u = new User
                                    {
                                        Id = reader.GetString((int)UserProfilesGetIndices.UserId),
                                        Email = (reader.IsDBNull((int)UserProfilesGetIndices.Email)) ? null : reader.GetString((int)UserProfilesGetIndices.Email),
                                        EmailConfirmed = reader.GetBoolean((int)UserProfilesGetIndices.EmailConfirmed),
                                        //PasswordHash = (reader.IsDBNull((int)UserProfilesGetIndices.PasswordHash)) ? null : reader.GetString((int)UserProfilesGetIndices.PasswordHash),
                                        //SecurityStamp = (reader.IsDBNull((int)UserProfilesGetIndices.SecurityStamp)) ? null : reader.GetString((int)UserProfilesGetIndices.SecurityStamp),
                                        PhoneNumber = (reader.IsDBNull((int)UserProfilesGetIndices.PhoneNumber)) ? null : reader.GetString((int)UserProfilesGetIndices.PhoneNumber),
                                        PhoneNumberConfirmed = reader.GetBoolean((int)UserProfilesGetIndices.PhoneNumberConfirmed),
                                        TwoFactorEnabled = reader.GetBoolean((int)UserProfilesGetIndices.TwoFactorEnabled),
                                        LockoutEndDateUtc = (reader.IsDBNull((int)UserProfilesGetIndices.LockoutEndDateUtc)) ? (DateTime?)null : reader.GetDateTime((int)UserProfilesGetIndices.LockoutEndDateUtc),
                                        LockoutEnabled = reader.GetBoolean((int)UserProfilesGetIndices.LockoutEnabled),
                                        AccessFailedCount = reader.GetInt32((int)UserProfilesGetIndices.AccessFailedCount),
                                        UserName = reader.GetString((int)UserProfilesGetIndices.UserName),
                                        FirstName = (reader.IsDBNull((int)UserProfilesGetIndices.FirstName)) ? null : reader.GetString((int)UserProfilesGetIndices.FirstName),
                                        LastName = (reader.IsDBNull((int)UserProfilesGetIndices.LastName)) ? null : reader.GetString((int)UserProfilesGetIndices.LastName),
                                        IsActive = (reader.IsDBNull((int)UserProfilesGetIndices.IsActive)) ? false : reader.GetBoolean((int)UserProfilesGetIndices.IsActive),
                                        Manager = reader.IsDBNull((int)UserProfilesGetIndices.ManagerId) ? null : new User
                                        {
                                            Id = reader.GetString((int)UserProfilesGetIndices.ManagerId),
                                            Email = reader.IsDBNull((int)UserProfilesGetIndices.ManagerEmail) ? null : reader.GetString((int)UserProfilesGetIndices.ManagerEmail),
                                            UserName = reader.GetString((int)UserProfilesGetIndices.ManagerUserName),
                                            FirstName = reader.IsDBNull((int)UserProfilesGetIndices.ManagerFirstName) ? null : reader.GetString((int)UserProfilesGetIndices.ManagerFirstName),
                                            LastName = reader.IsDBNull((int)UserProfilesGetIndices.ManagerLastName) ? null : reader.GetString((int)UserProfilesGetIndices.ManagerLastName)
                                        },
                                        PhoneCarrier = reader.IsDBNull((int)UserProfilesGetIndices.PhoneCarrierId) ? null : new PhoneCarrier
                                        {
                                            Id = reader.GetInt32((int)UserProfilesGetIndices.PhoneCarrierId),
                                            Carrier = reader.GetString((int)UserProfilesGetIndices.Carrier),
                                            EmailForTexts = reader.GetString((int)UserProfilesGetIndices.EmailForTexts)
                                        },
                                        Roles = new List<Role>
                                        {
                                            new Role
                                            {
                                                //Id = 
                                                //Name = 
                                                //RoleType = 
                                            }
                                        },
                                        UserNotifications = new List<UserNotification>()
                                    };

                                    throw new NotImplementedException("don't just sit there staring at me, fix the roles error line 436!!!!!!!");

                                    users.Add(u);

                                    //################################
                                    //################################
                                    //################################
                                    //################################
                                    if (!string.IsNullOrWhiteSpace(Id))
                                        FIX_THIS_TEMP_SINGLEUSER = u;
                                    //################################
                                    //################################
                                    //################################
                                    //################################
                                }
                                else
                                {
                                    ((List<Role>)u.Roles).Add(
                                        new Role
                                        {
                                            Id = Int32.Parse(reader.GetString((int)UserProfilesGetIndices.RoleId)),
                                            Name = reader.GetString((int)UserProfilesGetIndices.Role),
                                            RoleType = new RoleType
                                            {
                                                Id = reader.GetInt32((int)UserProfilesGetIndices.RoleTypeId),
                                                Name = reader.GetString((int)UserProfilesGetIndices.RoleType)
                                            }
                                        });
                                }
                            }
                        }

                        reader.Close();
                    }

                    if (FIX_THIS_TEMP_SINGLEUSER != null)
                    {
                        using (SqlCommand commandUserNotifications = new SqlCommand())
                        {
                            commandUserNotifications.CommandType = CommandType.StoredProcedure;
                            commandUserNotifications.Connection = connection;
                            commandUserNotifications.CommandText = "usp_UserNotificationsGet";

                            commandUserNotifications.Parameters.AddWithValue("@UserId", Id);

                            var reader = commandUserNotifications.ExecuteReader();

                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    var un = new UserNotification
                                    {
                                        RaciRole = new RaciRole
                                        {
                                            Id = reader.GetInt32((int)UserNotificationsGetIndices.RaciRoleId),
                                            Role = reader.GetString((int)UserNotificationsGetIndices.RaciRole)
                                        },
                                        TaskCreatedNotifyByEmail = reader.GetBoolean((int)UserNotificationsGetIndices.TaskCreatedNotifyByEmail),
                                        TaskCreatedNotifyByText = reader.GetBoolean((int)UserNotificationsGetIndices.TaskCreatedNotifyByText),
                                        TaskStartedNotifyByEmail = reader.GetBoolean((int)UserNotificationsGetIndices.TaskStartedNotifyByEmail),
                                        TaskStartedNotifyByText = reader.GetBoolean((int)UserNotificationsGetIndices.TaskStartedNotifyByText),
                                        TaskCompletedNotifyByEmail = reader.GetBoolean((int)UserNotificationsGetIndices.TaskCompletedNotifyByEmail),
                                        TaskCompletedNotifyByText = reader.GetBoolean((int)UserNotificationsGetIndices.TaskCompletedNotifyByText),
                                        TaskInJeopardyNotifyByEmail = reader.GetBoolean((int)UserNotificationsGetIndices.TaskInJeopardyNotifyByEmail),
                                        TaskInJeopardyNotifyByText = reader.GetBoolean((int)UserNotificationsGetIndices.TaskInJeopardyNotifyByText),
                                        TaskOverdueNotifyByEmail = reader.GetBoolean((int)UserNotificationsGetIndices.TaskOverdueNotifyByEmail),
                                        TaskOverdueNotifyByText = reader.GetBoolean((int)UserNotificationsGetIndices.TaskOverdueNotifyByText),
                                        IncidentReportCreatedNotifyByEmail = reader.GetBoolean((int)UserNotificationsGetIndices.IncidentReportCreatedNotifyByEmail),
                                        IncidentReportCreatedNotifyByText = reader.GetBoolean((int)UserNotificationsGetIndices.IncidentReportCreatedNotifyByText)
                                    };

                                    ((List<UserNotification>)FIX_THIS_TEMP_SINGLEUSER.UserNotifications).Add(un);
                                    break;
                                }
                            }

                            reader.Close();
                        }
                    }
                }

                return users;
            }
            catch (Exception ex)
            {
                string res = ex.ToString();
                throw;
            }
        }

        public string Create(string ConnectionString, User User)
        {
            #region Preconditions

            if (string.IsNullOrWhiteSpace(ConnectionString))
                throw new InvalidOperationException();

            if (User == null)
                throw new ArgumentNullException();

            #endregion

            try
            {
                string storedProcedureName = "usp_UserProfileCreate";

                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    using (SqlCommand command = new SqlCommand())
                    {
                        string newId = null;

                        command.CommandType = CommandType.StoredProcedure;
                        command.Connection = connection;
                        command.CommandText = storedProcedureName;

                        if (!string.IsNullOrWhiteSpace(User.Email))
                            command.Parameters.AddWithValue("@Email", User.Email);

                        command.Parameters.AddWithValue("@EmailConfirmed", User.EmailConfirmed);
                        command.Parameters.AddWithValue("@IsActive", User.IsActive);

                        //command.Parameters.AddWithValue("@PasswordHash", User.PasswordHash);
                        //command.Parameters.AddWithValue("@SecurityStamp", User.SecurityStamp);

                        if (!string.IsNullOrWhiteSpace(User.PhoneNumber))
                            command.Parameters.AddWithValue("@PhoneNumber", User.PhoneNumber);

                        command.Parameters.AddWithValue("@PhoneNumberConfirmed", User.PhoneNumberConfirmed);
                        command.Parameters.AddWithValue("@TwoFactorEnabled", User.TwoFactorEnabled);

                        if (User.LockoutEndDateUtc != null)
                            command.Parameters.AddWithValue("@LockoutEndDateUtc", User.LockoutEndDateUtc);

                        command.Parameters.AddWithValue("@LockoutEnabled", User.LockoutEnabled);
                        command.Parameters.AddWithValue("@AccessFailedCount", User.AccessFailedCount);

                        command.Parameters.AddWithValue("@UserName", User.UserName);

                        if (!string.IsNullOrWhiteSpace(User.FirstName))
                            command.Parameters.AddWithValue("@FirstName", User.FirstName);

                        if (!string.IsNullOrWhiteSpace(User.LastName))
                            command.Parameters.AddWithValue("@LastName", User.LastName);

                        if (!string.IsNullOrWhiteSpace(User.Manager.Id))
                            command.Parameters.AddWithValue("@ManagerId", User.Manager.Id);

                        if (User.PhoneCarrier != null)
                            command.Parameters.AddWithValue("@PhoneCarrierId", User.PhoneCarrier.Id);

                        #region RoleIds

                        var dataTableUserRoleIds = new DataTable();
                        dataTableUserRoleIds.Columns.Add("Id", typeof(long));

                        foreach (var r in User.Roles)
                        {
                            dataTableUserRoleIds.Rows.Add(r.Id);
                        }

                        SqlParameter parameterUserRoleIds;
                        parameterUserRoleIds = command.Parameters.AddWithValue("@UserRoleIds", dataTableUserRoleIds);
                        parameterUserRoleIds.SqlDbType = SqlDbType.Structured;
                        parameterUserRoleIds.TypeName = "dbo.IdTVPType";

                        #endregion

                        #region UserNotificationIds

                        var dataTableUserNotifications = new DataTable();

                        foreach (var field in Enum.GetNames(typeof(udt_UserNotificationsIndices)))
                        {
                            if (string.Compare(field, udt_UserNotificationsIndices.RaciRoleId.ToString(), true) == 0)
                            {
                                dataTableUserNotifications.Columns.Add(field, typeof(long));
                                continue;
                            }

                            dataTableUserNotifications.Columns.Add(field, typeof(bool));
                        }

                        foreach (var u in User.UserNotifications)
                        {
                            var row = dataTableUserNotifications.NewRow();
                            row[(int)udt_UserNotificationsIndices.RaciRoleId] = u.RaciRole.Id;
                            row[(int)udt_UserNotificationsIndices.TaskCreatedNotifyByEmail] = u.TaskCreatedNotifyByEmail;
                            row[(int)udt_UserNotificationsIndices.TaskCreatedNotifyByText] = u.TaskCreatedNotifyByText;
                            row[(int)udt_UserNotificationsIndices.TaskStartedNotifyByEmail] = u.TaskStartedNotifyByEmail;
                            row[(int)udt_UserNotificationsIndices.TaskStartedNotifyByText] = u.TaskStartedNotifyByText;
                            row[(int)udt_UserNotificationsIndices.TaskCompletedNotifyByEmail] = u.TaskCompletedNotifyByEmail;
                            row[(int)udt_UserNotificationsIndices.TaskCompletedNotifyByText] = u.TaskCompletedNotifyByText;
                            row[(int)udt_UserNotificationsIndices.TaskInJeopardyNotifyByEmail] = u.TaskInJeopardyNotifyByEmail;
                            row[(int)udt_UserNotificationsIndices.TaskInJeopardyNotifyByText] = u.TaskInJeopardyNotifyByText;
                            row[(int)udt_UserNotificationsIndices.TaskOverdueNotifyByEmail] = u.TaskOverdueNotifyByEmail;
                            row[(int)udt_UserNotificationsIndices.TaskOverdueNotifyByText] = u.TaskOverdueNotifyByText;
                            row[(int)udt_UserNotificationsIndices.IncidentReportCreatedNotifyByEmail] = u.IncidentReportCreatedNotifyByEmail;
                            row[(int)udt_UserNotificationsIndices.IncidentReportCreatedNotifyByText] = u.IncidentReportCreatedNotifyByText;
                            dataTableUserNotifications.Rows.Add(row);
                        }

                        SqlParameter parameterUserNotifications;
                        parameterUserNotifications = command.Parameters.AddWithValue("@UserNotifications", dataTableUserNotifications);
                        parameterUserNotifications.SqlDbType = SqlDbType.Structured;
                        parameterUserNotifications.TypeName = "dbo.udt_UserNotifications";

                        #endregion

                        SqlParameter outPutVal = new SqlParameter("@NewId", SqlDbType.NVarChar);
                        outPutVal.Direction = ParameterDirection.Output;
                        outPutVal.Size = 128;
                        command.Parameters.Add(outPutVal);

                        connection.Open();
                        command.ExecuteNonQuery();
                        connection.Close();

                        if (outPutVal.Value != DBNull.Value)
                            newId = outPutVal.Value.ToString();

                        return newId;
                    }
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        public bool Update(string ConnectionString, User User)
        {
            #region Preconditions

            if (string.IsNullOrWhiteSpace(ConnectionString))
                throw new InvalidOperationException();

            if (User == null)
                throw new ArgumentNullException();

            #endregion

            try
            {
                string storedProcedureName = "usp_UserProfileUpdate";

                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    using (SqlCommand command = new SqlCommand())
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Connection = connection;
                        command.CommandText = storedProcedureName;

                        command.Parameters.AddWithValue("@UserId", User.Id);

                        if (!string.IsNullOrWhiteSpace(User.Email))
                            command.Parameters.AddWithValue("@Email", User.Email);

                        command.Parameters.AddWithValue("@EmailConfirmed", User.EmailConfirmed);
                        command.Parameters.AddWithValue("@IsActive", User.IsActive);

                        //command.Parameters.AddWithValue("@PasswordHash", User.PasswordHash);
                        //command.Parameters.AddWithValue("@SecurityStamp", User.SecurityStamp);

                        if (!string.IsNullOrWhiteSpace(User.PhoneNumber))
                            command.Parameters.AddWithValue("@PhoneNumber", User.PhoneNumber);

                        command.Parameters.AddWithValue("@PhoneNumberConfirmed", User.PhoneNumberConfirmed);
                        command.Parameters.AddWithValue("@TwoFactorEnabled", User.TwoFactorEnabled);

                        if (User.LockoutEndDateUtc != null)
                            command.Parameters.AddWithValue("@LockoutEndDateUtc", User.LockoutEndDateUtc);

                        command.Parameters.AddWithValue("@LockoutEnabled", User.LockoutEnabled);
                        command.Parameters.AddWithValue("@AccessFailedCount", User.AccessFailedCount);

                        command.Parameters.AddWithValue("@UserName", User.UserName);

                        if (!string.IsNullOrWhiteSpace(User.FirstName))
                            command.Parameters.AddWithValue("@FirstName", User.FirstName);

                        if (!string.IsNullOrWhiteSpace(User.LastName))
                            command.Parameters.AddWithValue("@LastName", User.LastName);

                        if (!string.IsNullOrWhiteSpace(User.Manager.Id))
                            command.Parameters.AddWithValue("@ManagerId", User.Manager.Id);

                        if (User.PhoneCarrier != null)
                            command.Parameters.AddWithValue("@PhoneCarrierId", User.PhoneCarrier.Id);

                        #region RoleIds

                        var dataTableUserRoleIds = new DataTable();
                        dataTableUserRoleIds.Columns.Add("Id", typeof(long));

                        foreach (var r in User.Roles)
                        {
                            dataTableUserRoleIds.Rows.Add(r.Id);
                        }

                        SqlParameter parameterUserRoleIds;
                        parameterUserRoleIds = command.Parameters.AddWithValue("@UserRoleIds", dataTableUserRoleIds);
                        parameterUserRoleIds.SqlDbType = SqlDbType.Structured;
                        parameterUserRoleIds.TypeName = "dbo.IdTVPType";

                        #endregion

                        #region UserNotificationIds

                        var dataTableUserNotifications = new DataTable();

                        foreach (var field in Enum.GetNames(typeof(udt_UserNotificationsIndices)))
                        {
                            if (string.Compare(field, udt_UserNotificationsIndices.RaciRoleId.ToString(), true) == 0)
                            {
                                dataTableUserNotifications.Columns.Add(field, typeof(long));
                                continue;
                            }

                            dataTableUserNotifications.Columns.Add(field, typeof(bool));
                        }

                        foreach (var u in User.UserNotifications)
                        {
                            var row = dataTableUserNotifications.NewRow();
                            row[(int)udt_UserNotificationsIndices.RaciRoleId] = u.RaciRole.Id;
                            row[(int)udt_UserNotificationsIndices.TaskCreatedNotifyByEmail] = u.TaskCreatedNotifyByEmail;
                            row[(int)udt_UserNotificationsIndices.TaskCreatedNotifyByText] = u.TaskCreatedNotifyByText;
                            row[(int)udt_UserNotificationsIndices.TaskStartedNotifyByEmail] = u.TaskStartedNotifyByEmail;
                            row[(int)udt_UserNotificationsIndices.TaskStartedNotifyByText] = u.TaskStartedNotifyByText;
                            row[(int)udt_UserNotificationsIndices.TaskCompletedNotifyByEmail] = u.TaskCompletedNotifyByEmail;
                            row[(int)udt_UserNotificationsIndices.TaskCompletedNotifyByText] = u.TaskCompletedNotifyByText;
                            row[(int)udt_UserNotificationsIndices.TaskInJeopardyNotifyByEmail] = u.TaskInJeopardyNotifyByEmail;
                            row[(int)udt_UserNotificationsIndices.TaskInJeopardyNotifyByText] = u.TaskInJeopardyNotifyByText;
                            row[(int)udt_UserNotificationsIndices.TaskOverdueNotifyByEmail] = u.TaskOverdueNotifyByEmail;
                            row[(int)udt_UserNotificationsIndices.TaskOverdueNotifyByText] = u.TaskOverdueNotifyByText;
                            row[(int)udt_UserNotificationsIndices.IncidentReportCreatedNotifyByEmail] = u.IncidentReportCreatedNotifyByEmail;
                            row[(int)udt_UserNotificationsIndices.IncidentReportCreatedNotifyByText] = u.IncidentReportCreatedNotifyByText;
                            dataTableUserNotifications.Rows.Add(row);
                        }

                        SqlParameter parameterUserNotifications;
                        parameterUserNotifications = command.Parameters.AddWithValue("@UserNotifications", dataTableUserNotifications);
                        parameterUserNotifications.SqlDbType = SqlDbType.Structured;
                        parameterUserNotifications.TypeName = "dbo.udt_UserNotifications";

                        #endregion

                        connection.Open();
                        command.ExecuteNonQuery();
                        connection.Close();

                        return true;
                    }
                }
            }
            catch (Exception ex)
            {

                throw;
            }
        }
    }
}
