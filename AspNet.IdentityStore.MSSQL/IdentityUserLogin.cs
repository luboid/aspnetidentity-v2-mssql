using System;
namespace AspNet.IdentityStore
{
	public class IdentityUserLogin
	{
		public string LoginProvider
		{
			get;
			set;
		}
		public string ProviderKey
		{
			get;
			set;
		}
        public string UserId
		{
			get;
			set;
		}
	}
}
