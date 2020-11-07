using System.Collections.Generic;
using System.Threading.Tasks;
using FluentResults;
using PlexRipper.Domain;

namespace PlexRipper.Application.Common
{
    public interface INotificationsService
    {
        Task<Result<int>> CreateNotification(Notification notification);

        Task<Result<List<Notification>>> GetNotifications();

        Task<Result<bool>> HideNotification(int id);

        Task<Result> SendResult(Result result);
    }
}