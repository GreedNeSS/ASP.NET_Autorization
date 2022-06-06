using Authorization_By_Roles.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;

List<Person> people = new List<Person>()
{
    new Person("greed@gmail.com", "12345678", new Role("admin")),
    new Person("henry@gmail.com", "87654321", new Role("user")),
};

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.AccessDeniedPath = "/accessdenied";
        options.LoginPath = "/login";
    });
builder.Services.AddAuthorization();
var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/login", async (HttpContext context) =>
{
    string path = Path.Combine(Environment.CurrentDirectory, "wwwroot\\login.html");
    context.Response.ContentType = "text/html; charset=utf-8";
    await context.Response.SendFileAsync(path);
});

app.MapPost("/login", async (string? returnUrl, HttpContext context) =>
{
    var form = context.Request.Form;
    var email = form["email"];
    var password = form["password"];

    if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
    {
        return Results.BadRequest("Логин и/или пароль не установлены!");
    }

    Person? person = people.FirstOrDefault(p => p.Email == email && p.Password == password);

    if (person is null)
    {
        return Results.Unauthorized();
    }

    List<Claim> claims = new List<Claim>()
    {
        new Claim(ClaimTypes.Name, email),
        new Claim(ClaimTypes.Role, person.Role.Name)
    };
    ClaimsIdentity claimsIdentity = new ClaimsIdentity(claims, "Cookies");
    ClaimsPrincipal claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
    await context.SignInAsync(claimsPrincipal);
    return Results.Redirect(returnUrl ?? "/");
});

app.MapGet("/accessdenied", async (HttpContext context) =>
{
    context.Response.StatusCode = 403;
    await context.Response.WriteAsync("Access Denied");
});

app.MapGet("/admin", [Authorize(Roles = "admin")](HttpContext context) =>
{
    return Results.Text($"Hello Admin {context.User.Identity?.Name}");
});

app.MapGet("/", [Authorize(Roles = "admin, user")](HttpContext context) =>
{
    string login = context.User.FindFirstValue(ClaimTypes.Name);
    string role = context.User.FindFirstValue(ClaimTypes.Role);
    return Results.Text($"Login: {login}\nRole: {role}");
});

app.MapGet("/logout", (HttpContext context) =>
{
    context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    return Results.Text("Данные удалены!");
});

app.Run();
