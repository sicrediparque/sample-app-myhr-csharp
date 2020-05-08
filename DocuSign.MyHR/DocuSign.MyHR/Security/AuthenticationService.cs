using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Security.Claims;
using System.Text;
using DocuSign.eSign.Client;
using DocuSign.eSign.Client.Auth;
using DocuSign.MyHR.Domain;
using DocuSign.MyHR.Repositories;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Configuration;
using IAuthenticationService = DocuSign.MyHR.Security.IAuthenticationService;

namespace DocuSign.MyHR.Services
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly ITokenRepository _tokenRepository;
        private readonly IConfiguration _configurationService;
        private ApiClient _apiClient;

        public AuthenticationService(ITokenRepository tokenRepository, IConfiguration configurationService)
        {
            _tokenRepository = tokenRepository;
            _configurationService = configurationService;
            _apiClient = new ApiClient();
            _apiClient.SetOAuthBasePath(_configurationService["DocuSign:AuthServer"]);
        } 

        public (ClaimsPrincipal, AuthenticationProperties) AuthenticateFromJwt()
        {
            OAuth.OAuthToken authToken = _apiClient.RequestJWTUserToken(
                _configurationService["DocuSign:IntegrationKey"],
                _configurationService["DocuSign:UserId"],
                _configurationService["DocuSign:AuthServer"],
                Encoding.UTF8.GetBytes(ConfigurationManager.AppSettings["DocuSignRSAKey"]),
                1);

            OAuth.UserInfo userInfo = _apiClient.GetUserInfo(authToken.access_token);
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userInfo.Accounts.First().AccountId),
                new Claim(ClaimTypes.Name, userInfo.Name)
            };

            var claimsIdentity = new ClaimsIdentity(
                claims,
                CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                AllowRefresh = true,
                ExpiresUtc = DateTimeOffset.Now.AddDays(1),
                IsPersistent = true,
            };

            return (new ClaimsPrincipal(claimsIdentity), authProperties);
        } 

        public void Logout(string userId)
        {
            _tokenRepository.RemoveTokens(userId);
        }
         
        private void SaveDocuSignToken(OAuth.UserInfo userInfo, OAuth.OAuthToken token)
        {
            var tokenInfo = new DocuSignToken
            {
                UserId = userInfo.Accounts.First().AccountId,
                AccessToken = token.access_token,
                ExpireIn = DateTime.Now.AddSeconds(token.expires_in.Value),
                RefreshToken = token.refresh_token
            };
            _tokenRepository.SaveToken(tokenInfo);
        }
    }
}