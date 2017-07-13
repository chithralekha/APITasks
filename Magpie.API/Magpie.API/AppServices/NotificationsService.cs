using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Net.Mail;
using System.Text;
using Magpie.Model;

namespace Magpie.API.AppServices
{
    public class NotificationsService
    {
        private static string connectionStringName = "MagpieClient";
        private static string storedProcedureGetUserTaskNotificationSettings = "uspGetUserTaskNotificationSettingsDeprecated";

        public enum Events { TaskCreated, TaskStarted, TaskCompleted }

        public void NotifyUsers(UserTask userTask)
        {
            var userTaskNotificationSettings = GetUserTaskNotificationSettings(userTask.Id);

            foreach (var utns in userTaskNotificationSettings)
            {
                ProcessNotifications(userTask, utns);
            }
        }

        protected void ProcessNotifications(UserTask userTask, UserTaskNotificationSetting utns)
        {
            var taskEvent = GetEvent(userTask);

            switch (taskEvent)
            {
                case Events.TaskCreated:
                    {
                        if (utns.TaskStartedNotifyByEmail != null && utns.TaskStartedNotifyByEmail.Value)
                        {
                            SendEmail(userTask, taskEvent, utns);
                        }

                        if (utns.TaskStartedNotifyByText != null && utns.TaskStartedNotifyByText.Value)
                        {
                            SendText(userTask, taskEvent, utns);
                        }
                    }
                    break;
                case Events.TaskStarted:
                    {
                        if (utns.TaskStartedNotifyByEmail != null && utns.TaskStartedNotifyByEmail.Value)
                        {
                            SendEmail(userTask, taskEvent, utns);
                        }

                        if (utns.TaskStartedNotifyByText != null && utns.TaskStartedNotifyByText.Value)
                        {
                            SendText(userTask, taskEvent, utns);
                        }
                    }
                    break;
                case Events.TaskCompleted:
                    {
                        if (utns.TaskCompletedNotifyByEmail != null && utns.TaskCompletedNotifyByEmail.Value)
                        {
                            SendEmail(userTask, taskEvent, utns);
                        }

                        if (utns.TaskCompletedNotifyByText != null && utns.TaskCompletedNotifyByText.Value)
                        {
                            SendText(userTask, taskEvent, utns);
                        }
                    }
                    break;
                default:
                    break;
            }

        }

        protected void SendEmail(UserTask userTask, Events taskEvent, UserTaskNotificationSetting utns)
        {
            //#########################
            //#########################
            // READ THIS!!!!!
            // NB: If you provide credentials for basic authentication, they are sent to the server in clear text.This can present a security issue because your credentials can be seen, and then used by others.
            //#########################
            //#########################

            if (string.IsNullOrWhiteSpace(utns.Email))
            {
                throw new Exception("NotificationsNoEmail");
            }

            MailAddress to = new MailAddress(utns.Email);

            string notificationsFromMailAddress = System.Configuration.ConfigurationManager.AppSettings["notificationsFromMailAddress"];

            if (string.IsNullOrWhiteSpace(notificationsFromMailAddress))
                throw new Exception("Configuration: notificationsFromMailAddress invalid");

            MailAddress from = new MailAddress(notificationsFromMailAddress);

            MailMessage message = new MailMessage(from, to);
            message.Priority = MailPriority.High;
            message.Subject = string.Format("Notification from Magpie for Task: {0}", userTask.Title);

            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("Notification from Magpie for Task: {0}", userTask.Title);
            sb.AppendLine();
            sb.AppendLine();
            sb.AppendFormat("{0}, You are receiving this notification because your role for this task is {1}", utns.UserName, utns.RaciRole);
            sb.AppendLine();
            sb.AppendLine();
            sb.AppendFormat("The task status has change to {0}", taskEvent.ToString());
            sb.AppendLine();
            sb.AppendLine();
            sb.AppendFormat("To change your notification settings go to Admin and select notifications");

            message.Body = sb.ToString();

            string notificationsSmtpClientHost = System.Configuration.ConfigurationManager.AppSettings["notificationsSmtpClientHost"];
            string notificationsSmtpClientPort = System.Configuration.ConfigurationManager.AppSettings["notificationsSmtpClientPort"];
            string notificationsCredentialsUserName  = System.Configuration.ConfigurationManager.AppSettings["notificationsCredentialsUserName"];
            string notificationsCredentialsPassword = System.Configuration.ConfigurationManager.AppSettings["notificationsCredentialsPassword"];

            if (string.IsNullOrWhiteSpace(notificationsSmtpClientHost))
                throw new Exception("Configuration: notificationsSmtpClientHost invalid");

            if (string.IsNullOrWhiteSpace(notificationsSmtpClientPort))
                throw new Exception("Configuration: notificationsSmtpClientPort invalid");

            if (string.IsNullOrWhiteSpace(notificationsCredentialsUserName))
                throw new Exception("Configuration: notificationsCredentialsUserName invalid");

            if (string.IsNullOrWhiteSpace(notificationsCredentialsPassword))
                throw new Exception("Configuration: notificationsCredentialsPassword invalid");

            int port = Int32.Parse(notificationsSmtpClientPort);

            SmtpClient smtpClient = new SmtpClient(notificationsSmtpClientHost, port);
            smtpClient.Credentials = new System.Net.NetworkCredential(notificationsCredentialsUserName, notificationsCredentialsPassword);

            smtpClient.Send(message);
        }

        protected void SendText(UserTask userTask, Events taskEvent, UserTaskNotificationSetting utns)
        {
            //#########################
            //#########################
            // READ THIS!!!!!
            // NB: If you provide credentials for basic authentication, they are sent to the server in clear text.This can present a security issue because your credentials can be seen, and then used by others.
            //#########################
            //#########################

            if (string.IsNullOrWhiteSpace(utns.PhoneNumber))
            {
                throw new Exception("NotificationsNoPhoneNumber");
            }

            if (utns.PhoneCarrierId == null)
            {
                throw new Exception("NotificationsNoPhoneCarrier");
            }

            //#####################################
            // MOVE THIS TO DB APPSETTINGS
            var textEmail = utns.EmailForTexts.Replace("number", utns.PhoneNumber);
            //#####################################

            MailAddress to = new MailAddress(textEmail);

            string notificationsFromMailAddress = System.Configuration.ConfigurationManager.AppSettings["notificationsFromMailAddress"];

            if (string.IsNullOrWhiteSpace(notificationsFromMailAddress))
                throw new Exception("Configuration: notificationsFromMailAddress invalid");

            MailAddress from = new MailAddress(notificationsFromMailAddress);

            MailMessage message = new MailMessage(from, to);
            message.Priority = MailPriority.High;
            message.Subject = string.Format("Notification from Magpie for Task: {0}", userTask.Title);

            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("Notification from Magpie for Task: {0}", userTask.Title);
            sb.AppendLine();
            sb.AppendLine();
            sb.AppendFormat("{0}, You are receiving this notification because your role for this task is {1}", utns.UserName, utns.RaciRole);
            sb.AppendLine();
            sb.AppendLine();
            sb.AppendFormat("The task status has change to {0}", taskEvent.ToString());
            sb.AppendLine();
            sb.AppendLine();
            sb.AppendFormat("To change your notification settings go to Admin and select notifications");

            message.Body = sb.ToString();

            string notificationsSmtpClientHost = System.Configuration.ConfigurationManager.AppSettings["notificationsSmtpClientHost"];
            string notificationsSmtpClientPort = System.Configuration.ConfigurationManager.AppSettings["notificationsSmtpClientPort"];
            string notificationsCredentialsUserName = System.Configuration.ConfigurationManager.AppSettings["notificationsCredentialsUserName"];
            string notificationsCredentialsPassword = System.Configuration.ConfigurationManager.AppSettings["notificationsCredentialsPassword"];

            if (string.IsNullOrWhiteSpace(notificationsSmtpClientHost))
                throw new Exception("Configuration: notificationsSmtpClientHost invalid");

            if (string.IsNullOrWhiteSpace(notificationsSmtpClientPort))
                throw new Exception("Configuration: notificationsSmtpClientPort invalid");

            if (string.IsNullOrWhiteSpace(notificationsCredentialsUserName))
                throw new Exception("Configuration: notificationsCredentialsUserName invalid");

            if (string.IsNullOrWhiteSpace(notificationsCredentialsPassword))
                throw new Exception("Configuration: notificationsCredentialsPassword invalid");

            int port = Int32.Parse(notificationsSmtpClientPort);

            SmtpClient smtpClient = new SmtpClient(notificationsSmtpClientHost, port);
            smtpClient.Credentials = new System.Net.NetworkCredential(notificationsCredentialsUserName, notificationsCredentialsPassword);

            smtpClient.Send(message);
        }

        private Events GetEvent(UserTask userTask)
        {
            Events? taskEvent = null;

            switch (userTask.TaskState.Name)
            {
                case "New":
                    {
                        taskEvent = AppServices.NotificationsService.Events.TaskCreated;
                    }
                    break;
                case "In Progress":
                    {
                        taskEvent = AppServices.NotificationsService.Events.TaskStarted;
                    }
                    break;
                case "Completed":
                    {
                        taskEvent = AppServices.NotificationsService.Events.TaskCompleted;
                    }
                    break;
                default:
                    {
                        throw new NotImplementedException();
                    }
            }

            return taskEvent.Value;
        }

        private IEnumerable<UserTaskNotificationSetting> GetUserTaskNotificationSettings(int? UserTaskId = null)
        {
            try
            {
                string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings[connectionStringName].ConnectionString;

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    using (SqlCommand command = new SqlCommand(storedProcedureGetUserTaskNotificationSettings, connection))
                    {
                        List<UserTaskNotificationSetting> userTaskNotificationSettings = new List<UserTaskNotificationSetting>();

                        command.CommandType = CommandType.StoredProcedure;

                        command.Parameters.Add("@UserTaskId", SqlDbType.VarChar).Value = UserTaskId;

                        connection.Open();

                        var reader = command.ExecuteReader();

                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                var utns = new UserTaskNotificationSetting();

                                utns.UserTaskId = reader.GetInt32((int)UserTaskNotificationSettingIndices.UserTaskId);
                                utns.UserTaskTitle = reader.GetString((int)UserTaskNotificationSettingIndices.UserTaskTitle);
                                utns.RaciRoleId = reader.GetInt32((int)UserTaskNotificationSettingIndices.RaciRoleId);
                                utns.RaciRole = reader.GetString((int)UserTaskNotificationSettingIndices.RaciRole);
                                utns.UserId = reader.GetString((int)UserTaskNotificationSettingIndices.UserId);

                                if (!reader.IsDBNull((int)UserTaskNotificationSettingIndices.UserName))
                                    utns.UserName = reader.GetString((int)UserTaskNotificationSettingIndices.UserName);

                                if (!reader.IsDBNull((int)UserTaskNotificationSettingIndices.PhoneNumber))
                                    utns.PhoneNumber = reader.GetString((int)UserTaskNotificationSettingIndices.PhoneNumber);

                                if (!reader.IsDBNull((int)UserTaskNotificationSettingIndices.Email))
                                    utns.Email = reader.GetString((int)UserTaskNotificationSettingIndices.Email);

                                if (!reader.IsDBNull((int)UserTaskNotificationSettingIndices.PhoneCarrierId))
                                    utns.PhoneCarrierId = reader.GetInt32((int)UserTaskNotificationSettingIndices.PhoneCarrierId);

                                if (!reader.IsDBNull((int)UserTaskNotificationSettingIndices.EmailForTexts))
                                    utns.EmailForTexts = reader.GetString((int)UserTaskNotificationSettingIndices.EmailForTexts);

                                if (!reader.IsDBNull((int)UserTaskNotificationSettingIndices.TaskCreatedNotifyByEmail))
                                    utns.TaskCreatedNotifyByEmail = reader.GetBoolean((int)UserTaskNotificationSettingIndices.TaskCreatedNotifyByEmail);

                                if (!reader.IsDBNull((int)UserTaskNotificationSettingIndices.TaskCreatedNotifyByText))
                                    utns.TaskCreatedNotifyByText = reader.GetBoolean((int)UserTaskNotificationSettingIndices.TaskCreatedNotifyByText);

                                if (!reader.IsDBNull((int)UserTaskNotificationSettingIndices.TaskStartedNotifyByEmail))
                                    utns.TaskStartedNotifyByEmail = reader.GetBoolean((int)UserTaskNotificationSettingIndices.TaskStartedNotifyByEmail);

                                if (!reader.IsDBNull((int)UserTaskNotificationSettingIndices.TaskStartedNotifyByText))
                                    utns.TaskStartedNotifyByText = reader.GetBoolean((int)UserTaskNotificationSettingIndices.TaskStartedNotifyByText);

                                if (!reader.IsDBNull((int)UserTaskNotificationSettingIndices.TaskCompletedNotifyByEmail))
                                    utns.TaskCompletedNotifyByEmail = reader.GetBoolean((int)UserTaskNotificationSettingIndices.TaskCompletedNotifyByEmail);

                                if (!reader.IsDBNull((int)UserTaskNotificationSettingIndices.TaskCompletedNotifyByText))
                                    utns.TaskCompletedNotifyByText = reader.GetBoolean((int)UserTaskNotificationSettingIndices.TaskCompletedNotifyByText);

                                if (!reader.IsDBNull((int)UserTaskNotificationSettingIndices.IncidentReportCreatedNotifyByEmail))
                                    utns.IncidentReportCreatedNotifyByEmail = reader.GetBoolean((int)UserTaskNotificationSettingIndices.IncidentReportCreatedNotifyByEmail);

                                if (!reader.IsDBNull((int)UserTaskNotificationSettingIndices.IncidentReportCreatedNotifyByText))
                                    utns.IncidentReportCreatedNotifyByText = reader.GetBoolean((int)UserTaskNotificationSettingIndices.IncidentReportCreatedNotifyByText);

                                userTaskNotificationSettings.Add(utns);
                            }
                        }

                        reader.Close();

                        return userTaskNotificationSettings;
                    }
                }
            }
            catch (SqlException ex)
            {
                string res = ex.ToString();
                throw;
            }
        }

        #region Enums

        protected class UserTaskNotificationSetting
        {
            public int UserTaskId { get; set; }
            public string UserTaskTitle { get; set; }
            public int RaciRoleId { get; set; }
            public string RaciRole { get; set; }
            public string UserId { get; set; }
            public string UserName { get; set; }
            public string PhoneNumber { get; set; }
            public string Email { get; set; }
            public int? PhoneCarrierId { get; set; }
            public string EmailForTexts { get; set; }
            public bool? TaskCreatedNotifyByEmail { get; set; }
            public bool? TaskCreatedNotifyByText { get; set; }
            public bool? TaskStartedNotifyByEmail { get; set; }
            public bool? TaskStartedNotifyByText { get; set; }
            public bool? TaskCompletedNotifyByEmail { get; set; }
            public bool? TaskCompletedNotifyByText { get; set; }
            public bool? IncidentReportCreatedNotifyByEmail { get; set; }
            public bool? IncidentReportCreatedNotifyByText { get; set; }
        }

        private enum UserTaskNotificationSettingIndices
        {
            UserTaskId = 0,
            UserTaskTitle,
            RaciRoleId,
            RaciRole,
            UserId,
            UserName,
            PhoneNumber,
            Email,
            PhoneCarrierId,
            EmailForTexts,
            TaskCreatedNotifyByEmail,
            TaskCreatedNotifyByText,
            TaskStartedNotifyByEmail,
            TaskStartedNotifyByText,
            TaskCompletedNotifyByEmail,
            TaskCompletedNotifyByText,
            IncidentReportCreatedNotifyByEmail,
            IncidentReportCreatedNotifyByText
        }

        #endregion
    }
}
