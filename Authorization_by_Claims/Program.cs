using Authorization_by_Claims.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;

List<Person> people = new List<Person>
{
    new Person("greed@gmail.com", "1234", "Moscow", "Microsoft"),
    new Person("henry@gmail.com", "1234", "Москва", "Yandex"),
    new Person("marc@gmail.com", "1234", "London", "Google"),
};

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.AccessDeniedPath = "/login";
        options.LoginPath = "/login";
    });
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("OnlyForMoscow", policy =>
    {
        policy.RequireClaim(ClaimTypes.Locality, "Moscow", "Москва");
    });
    options.AddPolicy("OnlyForMicrosoft", policy =>
    {
        policy.RequireClaim("company", "Microsoft");
    });
});
var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("login", async (HttpContext context) =>
{
    context.Response.ContentType = "text/html; charset=utf-8";
    string path = Path.Combine(Environment.CurrentDirectory, "wwwroot\\Login.html");
    await context.Response.SendFileAsync(path);
});

app.MapGet("/logout", async (HttpContext context) =>
{
    await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    return Results.Text("Данные удалены!");
});

app.MapPost("/login", async (string? returnUrl, HttpContext context) =>
{
    var form = context.Request.Form;

    string login = form["email"];
    string password = form["password"];

    if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
    {
        return Results.BadRequest("Логин и/или пароль не установлены!");
    }

    Person? person = people.FirstOrDefault(p => p.Email == login && p.Password == password);

    if (person is null)
    {
        return Results.Unauthorized();
    }

    List<Claim> claims = new List<Claim>
    {
        new Claim(ClaimTypes.Name, person.Email),
        new Claim(ClaimTypes.Locality, person.City),
        new Claim("company", person.Company),
    };

    ClaimsIdentity claimsIdentity = new ClaimsIdentity(claims, "Cookies");
    ClaimsPrincipal claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
    await context.SignInAsync(claimsPrincipal);
    return Results.Redirect(returnUrl ?? "/");
});

app.MapGet("/", [Authorize] (HttpContext context) =>
{
    string? login = context.User.FindFirst(ClaimTypes.Name)?.Value;
    string? city = context.User.FindFirst(ClaimTypes.Locality)?.Value;
    string? company = context.User.FindFirst("company")?.Value;

    return $"Login: {login}\nCity: {city}\nCompany: {company}";
});

app.MapGet("/city", [Authorize(Policy = "OnlyForMoscow")] () =>
{
    return "Вы живёте в Москве!";
});

app.MapGet("/comp", [Authorize(Policy = "OnlyForMicrosoft")] () =>
{
    return "Вы работаете в Microsoft!";
});

app.MapGet("/cc", [Authorize(Policy = "OnlyForMoscow")][Authorize(Policy = "OnlyForMicrosoft")] () =>
{
    return "Вы живёте в Москве и работаете в Microsoft!";
});

app.Run();
