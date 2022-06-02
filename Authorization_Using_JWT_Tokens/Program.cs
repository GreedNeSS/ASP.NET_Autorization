using Authorization_Using_JWT_Tokens;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

List<Person> people = new List<Person>()
{
    new Person("greed@gmail.com", "343434"),
    new Person("mark@gmail.com", "54321"),
};

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = AuthOptions.ISSUER,
            ValidateAudience = true,
            ValidAudience = AuthOptions.AUDIENCE,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = AuthOptions.GetSymmetricSecurityKey()
        };
    });
builder.Services.AddAuthorization();
var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();

app.MapPost("/login", (Person loginData) =>
{
    Person? person = people.FirstOrDefault(p => p.Email == loginData.Email && p.Password == loginData.Password);

    if (person is null)
    {
        return Results.Unauthorized();
    }

    var claims = new List<Claim>() { new Claim(ClaimTypes.Name, loginData.Email) };
    var jwt = new JwtSecurityToken(
        issuer: AuthOptions.ISSUER,
        audience: AuthOptions.AUDIENCE,
        claims: claims,
        expires: DateTime.Now.Add(TimeSpan.FromMinutes(2)),
        signingCredentials: new SigningCredentials(AuthOptions.GetSymmetricSecurityKey(), SecurityAlgorithms.HmacSha256));
    var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);
    var response = new
    {
        access_token = encodedJwt,
        username = loginData.Email
    };

    return Results.Json(response);
});

app.MapGet("/data", [Authorize] () => Results.Json(new { message = "You are logged in" }));

app.Run();
