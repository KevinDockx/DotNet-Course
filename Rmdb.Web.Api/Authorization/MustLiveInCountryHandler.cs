using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Linq;
using System.Threading.Tasks;

namespace Rmdb.Web.Api.Authorization
{
    /// <summary>
    /// Authorization layer handler for user country
    /// </summary>
    public class MustLiveInCountryHandler : AuthorizationHandler<MustLiveInCountryRequirement>
    {

        /// <summary>
        /// Constructor for MustLiveInCountryHandler
        /// </summary>
        public MustLiveInCountryHandler()
        {
        }

        /// <summary>
        /// Handle the requirement
        /// </summary>
        /// <param name="context"></param>
        /// <param name="requirement"></param>
        /// <returns></returns>
        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context, MustLiveInCountryRequirement requirement)
        {
            var countryFromClaims = context.User.Claims.FirstOrDefault(c => c.Type == "country").Value;

            if (requirement.Country != countryFromClaims)
            {
                context.Fail();
                return Task.CompletedTask;
            }

            // all checks out
            context.Succeed(requirement);
            return Task.CompletedTask;
        }
    }
}
