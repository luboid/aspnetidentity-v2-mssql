using System;
namespace AspNet.IdentityStore
{
	public class IdentityUserClaim
	{
        public string Id
		{
			get;
			set;
		}
        public string UserId
		{
			get;
			set;
		}
		public string ClaimType
		{
			get;
			set;
		}
		public string ClaimValue
		{
			get;
			set;
		}
	}
}
