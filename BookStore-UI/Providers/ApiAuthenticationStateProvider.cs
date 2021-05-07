using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Security.Claims;
using System.Threading.Tasks;

namespace BookStore_UI.Providers
{
    public class ApiAuthenticationStateProvider : AuthenticationStateProvider
    {
        private readonly ILocalStorageService _localStorage;
        private readonly JwtSecurityTokenHandler _tokenHandler;

        public ApiAuthenticationStateProvider(ILocalStorageService localStorage, JwtSecurityTokenHandler tokenHandler)
        {
            _localStorage = localStorage;
            _tokenHandler = tokenHandler;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            try
            {
                var savedToken = await _localStorage.GetItemAsync<string>("authToken");
                if(string.IsNullOrWhiteSpace(savedToken))
                {
                    return new AuthenticationState(new System.Security.Claims.ClaimsPrincipal(new ClaimsIdentity()));
                }

                var tokenContent = _tokenHandler.ReadJwtToken(savedToken);
                var expiry = tokenContent.ValidTo;
                if(expiry < DateTime.Now)
                {
                    await _localStorage.RemoveItemAsync("authToken");

                    return new AuthenticationState(new System.Security.Claims.ClaimsPrincipal(new ClaimsIdentity()));
                }
                var claims = ParseClaim(tokenContent);

                var user = new System.Security.Claims.ClaimsPrincipal(new ClaimsIdentity(claims, "jwt"));

                return new AuthenticationState(user);

            }
            catch (Exception)
            {
                return new AuthenticationState(new System.Security.Claims.ClaimsPrincipal(new ClaimsIdentity()));
            }
        }

        private IList<Claim> ParseClaim(JwtSecurityToken tokenContent)
        {
            var claims = tokenContent.Claims.ToList();

            claims.Add(new Claim(ClaimTypes.Name, tokenContent.Subject));

            return claims;
        }
    }
}
