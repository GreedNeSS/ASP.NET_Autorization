using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace CustomPolicy.Authorize
{
    public class AgeHandler : AuthorizationHandler<AgeRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, AgeRequirement requirement)
        {
            var birthdayClaim = context.User.FindFirst(c => c.Type == ClaimTypes.DateOfBirth);

            if (birthdayClaim != null)
            {
                if (DateTime.TryParse(birthdayClaim.Value, out DateTime birthday))
                {
                    if (birthday.AddYears(requirement.Age) < DateTime.Now)
                    {
                        context.Succeed(requirement);
                    }
                }
            }
            return Task.CompletedTask;
        }
    }
}
