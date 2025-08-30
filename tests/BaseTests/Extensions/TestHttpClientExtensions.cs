using Reaparr.Application;
using Reaparr.Application.Contracts;

namespace Reaparr.BaseTests;

public static class TestHttpClientExtensions
{
    public static async Task SignIn(this HttpClient client)
    {
        var formData = new MultipartFormDataContent
        {
            { new StringContent(DefaultUserAppCredentials.DefaultUsername), "Username" },
            { new StringContent(DefaultUserAppCredentials.DefaultPassword), "Password" },
        };

        var response = await client.PostAsync(ApiRoutes.LoginEndpoint, formData);

        response.IsSuccessStatusCode.ShouldBeTrue();
    }
}
