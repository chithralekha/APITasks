namespace Magpie.DTO
{
    public class UserNotification
    {
        public RaciRole RaciRole { get; set; }
        public bool TaskCreatedNotifyByEmail { get; set; }
        public bool TaskCreatedNotifyByText { get; set; }
        public bool TaskStartedNotifyByEmail { get; set; }
        public bool TaskStartedNotifyByText { get; set; }
        public bool TaskCompletedNotifyByEmail { get; set; }
        public bool TaskCompletedNotifyByText { get; set; }
        public bool TaskInJeopardyNotifyByEmail { get; set; }
        public bool TaskInJeopardyNotifyByText { get; set; }
        public bool TaskOverdueNotifyByEmail { get; set; }
        public bool TaskOverdueNotifyByText { get; set; }
        public bool IncidentReportCreatedNotifyByEmail { get; set; }
        public bool IncidentReportCreatedNotifyByText { get; set; }
    }
}
