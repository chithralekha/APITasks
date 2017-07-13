using System;
using System.Collections.Generic;

namespace Magpie.Mapping
{
    public static class UserMapper
    {
        public static IEnumerable<Model.User> TranslateDTOUserListToModelUserList(IEnumerable<DTO.User> DTOUsers)
        {
            if (DTOUsers == null)
                return null;

            var modelUsers = new List<Model.User>();

            foreach (var item in DTOUsers)
            {
                modelUsers.Add(TranslateDTOUserToModelUser(item));
            }

            return modelUsers;
        }

        public static IEnumerable<Model.Role> TranslateDTORoleListToModelRoleList(IEnumerable<DTO.Role> DTORoles)
        {
            if (DTORoles == null)
                return null;

            var modelRoles = new List<Model.Role>();

            foreach (var item in DTORoles)
            {
                modelRoles.Add(TranslateDTORoleToModelRole(item));
            }

            return modelRoles;
        }

        public static IEnumerable<Model.UserNotification> TranslateDTOUserNotificationListToModelUserNotificationList(IEnumerable<DTO.UserNotification> DTOUserNotifications)
        {
            if (DTOUserNotifications == null)
                return null;

            var modelUserNotifications = new List<Model.UserNotification>();

            foreach (var item in DTOUserNotifications)
            {
                modelUserNotifications.Add(TranslateDTOUserNotificationToModelUserNotification(item));
            }

            return modelUserNotifications;
        }

        public static IEnumerable<Model.PhoneCarrier> TranslateDTOPhoneCarrierListToModelPhoneCarrierList(IEnumerable<DTO.PhoneCarrier> DTOPhoneCarriers)
        {
            if (DTOPhoneCarriers == null)
                return null;

            var modelPhoneCarriers = new List<Model.PhoneCarrier>();

            foreach (var item in DTOPhoneCarriers)
            {
                modelPhoneCarriers.Add(TranslateDTOPhoneCarrierToModelPhoneCarrier(item));
            }

            return modelPhoneCarriers;
        }

        public static Model.User TranslateDTOUserToModelUser(DTO.User u)
        {
            if (u == null)
                return null;

            return new Model.User
            {
                Id = u.Id,
                Email = u.Email,
                EmailConfirmed = u.EmailConfirmed,
                PasswordHash = u.PasswordHash,
                SecurityStamp = u.SecurityStamp,
                PhoneNumber = u.PhoneNumber,
                PhoneNumberConfirmed = u.PhoneNumberConfirmed,
                TwoFactorEnabled = u.TwoFactorEnabled,
                LockoutEndDateUtc = u.LockoutEndDateUtc,
                LockoutEnabled = u.LockoutEnabled,
                AccessFailedCount = u.AccessFailedCount,
                UserName = u.UserName,
                FirstName = u.FirstName,
                LastName = u.LastName,
                Manager = TranslateDTOUserToModelUser(u.Manager),
                PhoneCarrier = TranslateDTOPhoneCarrierToModelPhoneCarrier(u.PhoneCarrier),
                IsActive = u.IsActive,
                Roles = UserMapper.TranslateDTORoleListToModelRoleList(u.Roles),
                UserNotifications = UserMapper.TranslateDTOUserNotificationListToModelUserNotificationList(u.UserNotifications)
            };
        }

        public static Model.Role TranslateDTORoleToModelRole(DTO.Role r)
        {
            if (r == null)
                return null;

            return new Model.Role
            {
                Id = r.Id,
                Name = r.Name,
                RoleType = TranslateDTORoleTypeToModelRoleType(r.RoleType)
            };
        }

        public static Model.RoleType TranslateDTORoleTypeToModelRoleType(DTO.RoleType rt)
        {
            if (rt == null)
                return null;

            return new Model.RoleType
            {
                Id = rt.Id,
                Name = rt.Name
            };
        }

        public static Model.RaciRole TranslateDTORaciRoleToModelRaciRole(DTO.RaciRole rr)
        {
            if (rr == null)
                return null;

            return new Model.RaciRole
            {
                Id = rr.Id,
                Role = rr.Role
            };
        }

        public static Model.PhoneCarrier TranslateDTOPhoneCarrierToModelPhoneCarrier(DTO.PhoneCarrier pc)
        {
            if (pc == null)
                return null;

            return new Model.PhoneCarrier
            {
                Id = pc.Id,
                Carrier = pc.Carrier,
                EmailForTexts = pc.EmailForTexts
            };
        }

        public static Model.UserNotification TranslateDTOUserNotificationToModelUserNotification(DTO.UserNotification un)
        {
            if (un == null)
                return null;

            return new Model.UserNotification
            {
                RaciRole = TranslateDTORaciRoleToModelRaciRole(un.RaciRole),
                TaskCreatedNotifyByEmail = un.TaskCreatedNotifyByEmail,
                TaskCreatedNotifyByText = un.TaskCreatedNotifyByText,
                TaskStartedNotifyByEmail = un.TaskStartedNotifyByEmail,
                TaskStartedNotifyByText = un.TaskStartedNotifyByText,
                TaskCompletedNotifyByEmail = un.TaskCompletedNotifyByEmail,
                TaskCompletedNotifyByText = un.TaskCompletedNotifyByText,
                TaskInJeopardyNotifyByEmail = un.TaskInJeopardyNotifyByEmail,
                TaskInJeopardyNotifyByText = un.TaskInJeopardyNotifyByText,
                TaskOverdueNotifyByEmail = un.TaskOverdueNotifyByEmail,
                TaskOverdueNotifyByText = un.TaskOverdueNotifyByText,
                IncidentReportCreatedNotifyByEmail = un.IncidentReportCreatedNotifyByEmail,
                IncidentReportCreatedNotifyByText = un.IncidentReportCreatedNotifyByText
            };
        }

        public static IEnumerable<DTO.User> TranslateModelUserListToDTOUserList(IEnumerable<Model.User> ModelUsers)
        {
            if (ModelUsers == null)
                return null;

            var dtoUsers = new List<DTO.User>();

            foreach (var item in ModelUsers)
            {
                dtoUsers.Add(TranslateModelUserToDTOUser(item));
            }

            return dtoUsers;
        }

        public static IEnumerable<DTO.Role> TranslateModelRoleListToDTORoleList(IEnumerable<Model.Role> ModelRoles)
        {
            if (ModelRoles == null)
                return null;

            var dtoRoles = new List<DTO.Role>();

            foreach (var item in ModelRoles)
            {
                dtoRoles.Add(TranslateModelRoleToDTORole(item));
            }

            return dtoRoles;
        }

        public static IEnumerable<DTO.UserNotification> TranslateModelUserNotificationListToDTOUserNotificationList(IEnumerable<Model.UserNotification> ModelUserNotifications)
        {
            if (ModelUserNotifications == null)
                return null;

            var dtoUserNotifications = new List<DTO.UserNotification>();

            foreach (var item in ModelUserNotifications)
            {
                dtoUserNotifications.Add(TranslateModelUserNotificationToDTOUserNotification(item));
            }

            return dtoUserNotifications;
        }

        public static IEnumerable<DTO.PhoneCarrier> TranslateModelPhoneCarrierListToDTOPhoneCarrierList(IEnumerable<Model.PhoneCarrier> ModelPhoneCarriers)
        {
            if (ModelPhoneCarriers == null)
                return null;

            var dtoPhoneCarriers = new List<DTO.PhoneCarrier>();

            foreach (var item in ModelPhoneCarriers)
            {
                dtoPhoneCarriers.Add(TranslateModelPhoneCarrierToDTOPhoneCarrier(item));
            }

            return dtoPhoneCarriers;
        }

        public static DTO.User TranslateModelUserToDTOUser(Model.User u)
        {
            if (u == null)
                return null;

            return new DTO.User
            {
                Id = u.Id,
                Email = u.Email,
                EmailConfirmed = u.EmailConfirmed,
                PasswordHash = u.PasswordHash,
                SecurityStamp = u.SecurityStamp,
                PhoneNumber = u.PhoneNumber,
                PhoneNumberConfirmed = u.PhoneNumberConfirmed,
                TwoFactorEnabled = u.TwoFactorEnabled,
                LockoutEndDateUtc = u.LockoutEndDateUtc,
                LockoutEnabled = u.LockoutEnabled,
                AccessFailedCount = u.AccessFailedCount,
                UserName = u.UserName,
                FirstName = u.FirstName,
                LastName = u.LastName,
                Manager = TranslateModelUserToDTOUser(u.Manager),
                PhoneCarrier = TranslateModelPhoneCarrierToDTOPhoneCarrier(u.PhoneCarrier),
                IsActive = u.IsActive,
                Roles = UserMapper.TranslateModelRoleListToDTORoleList(u.Roles),
                UserNotifications = UserMapper.TranslateModelUserNotificationListToDTOUserNotificationList(u.UserNotifications)
            };
        }

        public static DTO.Role TranslateModelRoleToDTORole(Model.Role r)
        {
            if (r == null)
                return null;

            return new DTO.Role
            {
                Id = r.Id,
                Name = r.Name,
                RoleType = TranslateModelRoleTypeToDTORoleType(r.RoleType)
            };
        }

        public static DTO.RoleType TranslateModelRoleTypeToDTORoleType(Model.RoleType rt)
        {
            if (rt == null)
                return null;

            return new DTO.RoleType
            {
                Id = rt.Id,
                Name = rt.Name
            };
        }

        public static DTO.RaciRole TranslateModelRaciRoleToDTORaciRole(Model.RaciRole rr)
        {
            if (rr == null)
                return null;

            return new DTO.RaciRole
            {
                Id = rr.Id,
                Role = rr.Role
            };
        }

        public static DTO.PhoneCarrier TranslateModelPhoneCarrierToDTOPhoneCarrier(Model.PhoneCarrier pc)
        {
            if (pc == null)
                return null;

            return new DTO.PhoneCarrier
            {
                Id = pc.Id,
                Carrier = pc.Carrier,
                EmailForTexts = pc.EmailForTexts
            };
        }

        public static DTO.UserNotification TranslateModelUserNotificationToDTOUserNotification(Model.UserNotification un)
        {
            if (un == null)
                return null;

            return new DTO.UserNotification
            {
                RaciRole = TranslateModelRaciRoleToDTORaciRole(un.RaciRole),
                TaskCreatedNotifyByEmail = un.TaskCreatedNotifyByEmail,
                TaskCreatedNotifyByText = un.TaskCreatedNotifyByText,
                TaskStartedNotifyByEmail = un.TaskStartedNotifyByEmail,
                TaskStartedNotifyByText = un.TaskStartedNotifyByText,
                TaskCompletedNotifyByEmail = un.TaskCompletedNotifyByEmail,
                TaskCompletedNotifyByText = un.TaskCompletedNotifyByText,
                TaskInJeopardyNotifyByEmail = un.TaskInJeopardyNotifyByEmail,
                TaskInJeopardyNotifyByText = un.TaskInJeopardyNotifyByText,
                TaskOverdueNotifyByEmail = un.TaskOverdueNotifyByEmail,
                TaskOverdueNotifyByText = un.TaskOverdueNotifyByText,
                IncidentReportCreatedNotifyByEmail = un.IncidentReportCreatedNotifyByEmail,
                IncidentReportCreatedNotifyByText = un.IncidentReportCreatedNotifyByText
            };
        }
    }
}
