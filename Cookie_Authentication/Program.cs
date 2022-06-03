using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Cookie_Authentication.Models;
using System.Security.Claims;

List<Person> people = new List<Person>()
{
    new Person("greed@gmail.com", "343434"),
    new Person("mark@gmail.com", "54321"),
};

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddAuthorization();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options => options.LoginPath = "/login");
var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/login", async (HttpContext context) =>
{
    string path = Path.Combine(Environment.CurrentDirectory, "wwwroot\\Login.html");
    context.Response.ContentType = "text/html; charset=utf-8";
    await context.Response.SendFileAsync(path);
});

app.MapPost("/login", async (string? returnUrl, HttpContext context) =>
{
    var form = context.Request.Form;
    
    if (!form.ContainsKey("email") || !form.ContainsKey("password"))
    {
        return Results.BadRequest("Email и/или пароль не установлены!");
    }

    Person? person = people.FirstOrDefault(p => p.Email == form["email"] && p.Password == form["password"]);

    if (person is null)
    {
        return Results.Unauthorized();
    }

    var claims = new List<Claim>(){ new Claim(ClaimTypes.Name, person.Email) };
    ClaimsIdentity claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
    await context.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));
    return Results.Redirect(returnUrl ?? "/");
});

app.MapGet("/logout", async (HttpContext context) =>
{
    await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    return Results.Redirect("/login");
});

app.MapGet("/", [Authorize] async (HttpContext context) =>
{
    string path = Path.Combine(Environment.CurrentDirectory, "wwwroot\\Index.html");
    context.Response.ContentType = "text/html; charset=utf-8";
    await context.Response.SendFileAsync(path);
});

app.Run();
