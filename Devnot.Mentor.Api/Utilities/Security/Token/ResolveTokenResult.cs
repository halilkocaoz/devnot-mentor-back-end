using System;
using System.Collections.Generic;
using System.Security.Claims;

namespace DevnotMentor.Api.Utilities.Security.Token
{
    /// <summary>
    /// It provides token result properties.
    /// </summary>
    public class ResolveTokenResult
    {
        /// <summary>
        /// Check token is valid.
        /// </summary>
        public bool IsValid { get; set; }
        /// <summary>
        /// It provides error message when token is not valid.
        /// </summary>
        public string ErrorMessage { get; set; }
        /// <summary>
        /// Token expiry date.
        /// </summary>
        public DateTime ExpiryDate { get; set; }
        /// <summary>
        /// Key value pair of claim.
        /// </summary>
        public IEnumerable<Claim> Claims { get; set; }
    }
}