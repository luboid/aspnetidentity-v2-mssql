using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
namespace AspNet.IdentityStore
{
	public class IdentityUser : IUser<string> 
	{
		public string Email
		{
			get;
			set;
		}
		public bool EmailConfirmed
		{
			get;
			set;
		}
		public string PasswordHash
		{
			get;
			set;
		}
		public string SecurityStamp
		{
			get;
			set;
		}
		public string PhoneNumber
		{
			get;
			set;
		}
		public bool PhoneNumberConfirmed
		{
			get;
			set;
		}
		public bool TwoFactorEnabled
		{
			get;
			set;
		}
        public ICollection<IdentityUserRole> Roles
		{
			get;
			private set;
		}
        public ICollection<IdentityUserClaim> Claims
		{
			get;
			private set;
		}
        public ICollection<IdentityUserLogin> Logins
		{
			get;
			private set;
		}
        public string Id
		{
			get;
			set;
		}
		public string UserName
		{
			get;
			set;
		}
		public IdentityUser()
		{
            this.Claims = new List<IdentityUserClaim>();
            this.Roles = new List<IdentityUserRole>();
            this.Logins = new List<IdentityUserLogin>();
		}
	}}
