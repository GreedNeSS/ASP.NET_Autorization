using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie();
var app = builder.Build();

app.UseAuthentication();

app.MapGet("/login", async (HttpContext context) =>
{
    var claimsIdentity = new ClaimsIdentity("Undefined");
    var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
    await context.SignInAsync(claimsPrincipal);
    return Results.Redirect("/");
});

app.MapGet("/", (HttpContext context) => 
{
    var user = context.User.Identity;

    if (user is not null && user.IsAuthenticated)
    {
        return Results.Text($"Пользователь аутентифицирован. Тип аутентификации: {user.AuthenticationType}");
    }

    return Results.Text("Пользователь не аутентифицирован!");
});

app.MapGet("/logout", async (HttpContext context) =>
{
    await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    return "Данные удалены!";
});

app.Run();
