using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Centrifugo.Client.Options;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Protocol;

namespace Centrifugo.Sample.TokenProvider
{
    public class CentrifugoTokenProvider : ICentrifugoTokenProvider
    {
        private readonly AuthOptions _authOptions;

        public CentrifugoTokenProvider(IOptions<AuthOptions> authOptions)
        {
            _authOptions = authOptions.Value;
        }

        public Task<string> ConnectionTokenAsync(
            string clientId,
            string? clientProvidedInfo = null
        )
        {
            var now = DateTime.UtcNow;

            var claims = new List<Claim>
            {
                new Claim("sub", clientId)
            };

            if (_authOptions.TokenLifetime.HasValue)
            {
                var unixTime = DateTimeOffset.UtcNow.Add(_authOptions.TokenLifetime.Value).ToUnixTimeSeconds();
                claims.Add(new Claim("exp", unixTime.ToString()));
            }


            var jwt = new JwtSecurityToken(
//                issuer: _authOptions.Issuer,
//                audience: _authOptions.Audience,
                notBefore: now,
                claims: claims,
                expires: _authOptions.TokenLifetime.HasValue ? now.Add(_authOptions.TokenLifetime.Value) : now,
                signingCredentials: new SigningCredentials(GetSymmetricSecurityKey(_authOptions.SecretKey), SecurityAlgorithms.HmacSha256)
            );

            if (clientProvidedInfo != null)
            {
                jwt.Payload.Add("info", new { name = clientProvidedInfo });
            }
            Console.WriteLine("Connection token: {0}", jwt);
//            Console.WriteLine("Token key: {0}", _authOptions.SecretKey);

            return Task.FromResult(new JwtSecurityTokenHandler().WriteToken(jwt));

            static SymmetricSecurityKey GetSymmetricSecurityKey(string secretKey)
            {
                return new SymmetricSecurityKey(Encoding.ASCII.GetBytes(secretKey));
            }
        }

        public Task<string> SubscriptionTokenAsync(
            string clientId,
            string channel
        )
        {
            var now = DateTime.UtcNow;

            var claims = new List<Claim>
            {
                new Claim("sub", clientId),
                new Claim("channel", channel),
            };

            if (_authOptions.TokenLifetime.HasValue)
            {
                var unixTime = DateTimeOffset.UtcNow.Add(_authOptions.TokenLifetime.Value).ToUnixTimeSeconds();
                claims.Add(new Claim("exp", unixTime.ToString()));
            }


            var jwt = new JwtSecurityToken(
                //                issuer: _authOptions.Issuer,
                //                audience: _authOptions.Audience,
                notBefore: now,
                claims: claims,
                expires: _authOptions.TokenLifetime.HasValue ? now.Add(_authOptions.TokenLifetime.Value) : now,
                signingCredentials: new SigningCredentials(GetSymmetricSecurityKey(_authOptions.SecretKey), SecurityAlgorithms.HmacSha256)
            );

            Console.WriteLine("Subscription token: {0}", jwt);
//            Console.WriteLine("Token key: {0}", _authOptions.SecretKey);

            return Task.FromResult(new JwtSecurityTokenHandler().WriteToken(jwt));

            static SymmetricSecurityKey GetSymmetricSecurityKey(string secretKey)
            {
                return new SymmetricSecurityKey(Encoding.ASCII.GetBytes(secretKey));
            }
        }
    }
}