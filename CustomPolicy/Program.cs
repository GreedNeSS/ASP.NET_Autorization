using CustomPolicy.Models;
using CustomPolicy.Authorize;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;

List<Person> people = new List<Person>
{
    new Person("greed@gmail.com", "1234", new DateTime(1991, 6, 26)),
    new Person("henry@gmail.com", "1234", new DateTime(2004, 6, 26)),
};

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.AccessDeniedPath = "/";
        options.LoginPath = "/login";
    });

builder.Services.AddTransient<IAuthorizationHandler, AgeHandler>();
builder.Services.AddAuthorization(opts =>
{
    opts.AddPolicy("AgeLimit", policy => policy.Requirements.Add(new AgeRequirement(18)));
});

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

    var claims = new List<Claim>() 
    { 
        new Claim(ClaimTypes.Name, person.Email),
        new Claim(ClaimTypes.DateOfBirth, person.Birthday.ToString()) 
    };
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

app.MapGet("/age", [Authorize(Policy = "AgeLimit")](HttpContext context) =>
{
    return "¬ы старше 18 лет!";
});

app.Run();
