using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
namespace AspNet.IdentityStore
{
    public class IdentityRole : IRole<string>
	{
        public string Id
		{
			get;
			set;
		}

		public string Name
		{
			get;
			set;
		}
		
        public IdentityRole()
		{ }
        
        public IdentityRole(string roleName)
        {
            Name = roleName;
        }
    }
}
