using System;

namespace Infrastructure.External.ShopeeWebApi
{
    public class AuthenticationToken
    {
        public AuthenticationToken(
            string accessToken,
            DateTime accessTokenExpiry,
            string refreshToken,
            DateTime refreshTokenExpiry)
        {
            AccessToken = accessToken;
            AccessTokenExpiry = accessTokenExpiry;
            RefreshToken = refreshToken;
            RefreshTokenExpiry = refreshTokenExpiry;

        }



        public string AccessToken { get; set; }
        public DateTime AccessTokenExpiry { get; set; }
        public string RefreshToken { get; set; }
        public DateTime RefreshTokenExpiry { get; set; }

        public bool IsValid => AccessTokenExpiry.AddMinutes(-10) > DateTime.Now;
        public bool CanRefresh => RefreshTokenExpiry.AddMinutes(-10) > DateTime.Now;
    }
}
