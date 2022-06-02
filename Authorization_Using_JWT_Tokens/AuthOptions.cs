using Microsoft.IdentityModel.Tokens;
using System;
using System.Text;

namespace Authorization_Using_JWT_Tokens
{
    public class AuthOptions
    {
        public const string ISSUER = "MyAuthServer";
        public const string AUDIENCE = "MyAuthClient";
        const string KEY = "SecurityCode123..///.4536";
        public static SymmetricSecurityKey GetSymmetricSecurityKey()
        {
            return new SymmetricSecurityKey(Encoding.UTF8.GetBytes(KEY));
        }
    }
}
