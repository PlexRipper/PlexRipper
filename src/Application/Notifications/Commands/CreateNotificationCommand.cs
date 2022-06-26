namespace PlexRipper.Application.Notifications
{
    public class CreateNotificationCommand : IRequest<Result<int>>
    {
        public Notification Notification { get; }

        public CreateNotificationCommand(Notification notification)
        {
            Notification = notification;
        }
    }
}