using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Client;

namespace WebApplication1.Controllers
{
    public class AuthController(IConfiguration configuration) : ControllerBase
    {
        [HttpGet("token")]
        public async Task<IActionResult> GetToken()
        {
            var app = ConfidentialClientApplicationBuilder
                .Create(configuration["AzureAd:ClientId"])
                .WithClientSecret(configuration["AzureAd:ClientSecret"])
                .WithAuthority($"https://login.microsoftonline.com/{configuration["AzureAd:TenantId"]}")
                .Build();

            var result = await app.AcquireTokenForClient(
                new[] { "api://YOUR-API-CLIENT-ID/.default" })
                .ExecuteAsync();

            return Ok(new { accessToken = result.AccessToken });
        }
    }
}
