using PlexRipper.Domain.Common.API;
using System.Threading.Tasks;

namespace PlexRipper.Application.Common.Interfaces.API
{
    public interface IApi
    {
        Task Request(Request request);
        Task<T> Request<T>(Request request);
        Task<string> RequestContent(Request request);
        T DeserializeXml<T>(string receivedString);
        Task<bool> Download(Request request, string fileName);
    }
}
