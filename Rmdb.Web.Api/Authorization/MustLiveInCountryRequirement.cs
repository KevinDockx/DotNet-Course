using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rmdb.Web.Api.Authorization
{
    /// <summary>
    /// Authorization layer requirement for user country
    /// </summary>
    public class MustLiveInCountryRequirement : IAuthorizationRequirement
    {
        /// <summary>
        /// The country that must be matched
        /// </summary>
        public string Country { get; private set; }

        /// <summary>
        /// MustLiveInCountryRequirement constructor
        /// </summary>
        /// <param name="country"></param>
        public MustLiveInCountryRequirement(string country)
        {
            Country = country;
        }
    }
}
