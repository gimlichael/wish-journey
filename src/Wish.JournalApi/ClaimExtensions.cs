using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace Wish.JournalApi
{
    public static class ClaimExtensions
    {
        public static string OwnerIdOrDefault(this IEnumerable<Claim> claims)
        {
            return claims.SingleOrDefault(claim => claim.Type == "OwnerId")?.Value;
        }
    }
}
