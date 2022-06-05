using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie();
var app = builder.Build();

app.UseAuthentication();

app.MapGet("/login", async (HttpContext context) =>
{
    var claims = new List<Claim>()
    {
        new Claim(ClaimTypes.Name, "GreedNeSS"),
        new Claim("Company", "Microsoft"),
        new Claim(ClaimTypes.MobilePhone, "+7438709322432"),
    };

    ClaimsIdentity claimsIdentity = new ClaimsIdentity(claims, "Cookies");
    await context.SignInAsync(new ClaimsPrincipal(claimsIdentity));
    return Results.Redirect("/");
});

app.MapGet("/", (HttpContext context) =>
{
    var claimsIdentity = context.User;
    var name = claimsIdentity.Identity?.Name;
    var age = claimsIdentity.FindFirst("Age")?.Value;
    var company = claimsIdentity.FindFirst("Company")?.Value;
    var phone = claimsIdentity.FindFirst(ClaimTypes.MobilePhone)?.Value;
    var languages = claimsIdentity.FindAll("Language");

    string response = $"Name: {name}, Age: {age}, Company: {company}, Phone: {phone}";

    if (languages is not null)
    {
        response += ", Languages: ";

        foreach (var l in languages)
        {
            response += $" {l.Value},";
        }
    }

    return response;
});

app.MapGet("/addage", async (HttpContext context) =>
{
    if (context.User.Identity is ClaimsIdentity claimsIdentity && context.User.Identity.IsAuthenticated)
    {
        claimsIdentity.AddClaim(new Claim("age", "37"));
        var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
        await context.SignInAsync(claimsPrincipal);
    }
    return Results.Redirect("/");
});

app.MapGet("/addlangs", async (HttpContext context) =>
{
    if (context.User.Identity is ClaimsIdentity claimsIdentity && context.User.Identity.IsAuthenticated)
    {
        claimsIdentity.AddClaim(new Claim("Language", "Russian"));
        claimsIdentity.AddClaim(new Claim("Language", "English"));
        claimsIdentity.AddClaim(new Claim("Language", "German"));
        var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
        await context.SignInAsync(claimsPrincipal);
    }
    return Results.Redirect("/");
});

app.MapGet("/removephone", async (HttpContext context) =>
{
    if (context.User.Identity is ClaimsIdentity identity && context.User.Identity.IsAuthenticated)
    {
        var phone = identity.FindFirst(ClaimTypes.MobilePhone);

        if (identity.TryRemoveClaim(phone))
        {
            var claimsPrincipal = new ClaimsPrincipal(identity);
            await context.SignInAsync(claimsPrincipal);
        }
    }

    return Results.Redirect("/");
});

app.MapGet("/logout", async (HttpContext context) =>
{
    await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    return "Данные удалены";
});

app.Run();
