using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;

using Dapper;
using AspNet.IdentityStore.Properties;

namespace AspNet.IdentityStore
{
    public class UserStore :
        IUserLoginStore<IdentityUser, string>,
        IUserClaimStore<IdentityUser, string>,
        IUserRoleStore<IdentityUser, string>,
        IUserPasswordStore<IdentityUser, string>,
        IUserSecurityStampStore<IdentityUser, string>,
        //IQueryableUserStore<IdentityUser, Guid>, 
        IUserEmailStore<IdentityUser, string>,
        IUserPhoneNumberStore<IdentityUser, string>,
        IUserTwoFactorStore<IdentityUser, string>,
        IUserLockoutStore<IdentityUser, string>, 
        IUserStore<IdentityUser, string>,
        IUserStore<IdentityUser>,
        IDisposable
    {
        bool _disposed;
        IDbContext _context;

        public bool DisposeContext
        {
            get;
            set;
        }

        //public bool AutoSaveChanges
        //{
        //    get;
        //    set;
        //}

        //public IQueryable<IdentityUser> Users
        //{
        //    get
        //    {
        //        return _userStore.EntitySet;
        //    }
        //}

        public UserStore(IDbContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }
            _context = context;
            //AutoSaveChanges = true;
        }

        public Task<DateTimeOffset> GetLockoutEndDateAsync(IdentityUser user)
        {
            this.ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }
            return Task.FromResult<DateTimeOffset>(user.LockoutEndDateUtc.HasValue ? new DateTimeOffset(DateTime.SpecifyKind(user.LockoutEndDateUtc.Value, DateTimeKind.Utc)) : default(DateTimeOffset));
        }
        public Task SetLockoutEndDateAsync(IdentityUser user, DateTimeOffset lockoutEnd)
        {
            this.ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }
            user.LockoutEndDateUtc = ((lockoutEnd == DateTimeOffset.MinValue) ? null : new DateTime?(lockoutEnd.UtcDateTime));
            return Task.FromResult<int>(0);
        }
        public Task<int> IncrementAccessFailedCountAsync(IdentityUser user)
        {
            this.ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }
            user.AccessFailedCount++;
            return Task.FromResult<int>(user.AccessFailedCount);
        }
        public Task ResetAccessFailedCountAsync(IdentityUser user)
        {
            this.ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }
            user.AccessFailedCount = 0;
            return Task.FromResult<int>(0);
        }

        public Task<int> GetAccessFailedCountAsync(IdentityUser user)
        {
            this.ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }
            return Task.FromResult<int>(user.AccessFailedCount);
        }

        public Task<bool> GetLockoutEnabledAsync(IdentityUser user)
        {
            this.ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }
            return Task.FromResult<bool>(user.LockoutEnabled);
        }

        public Task SetLockoutEnabledAsync(IdentityUser user, bool enabled)
        {
            this.ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }
            user.LockoutEnabled = enabled;
            return Task.FromResult<int>(0);
        }

        public virtual Task<IList<Claim>> GetClaimsAsync(IdentityUser user)
        {
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            IList<Claim> result = (
                from c in user.Claims
                select new Claim(c.ClaimType, c.ClaimValue)).ToList<Claim>();

            return Task.FromResult<IList<Claim>>(result);
        }

        public virtual Task AddClaimAsync(IdentityUser user, Claim claim)
        {
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }
            if (claim == null)
            {
                throw new ArgumentNullException("claim");
            }

            user.Claims.Add(new IdentityUserClaim { UserId = user.Id, ClaimType = claim.Type, ClaimValue = claim.Value });

            return Task.FromResult<int>(0);
        }

        public virtual Task RemoveClaimAsync(IdentityUser user, Claim claim)
        {
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }
            if (claim == null)
            {
                throw new ArgumentNullException("claim");
            }

            List<IdentityUserClaim> list = (
                from uc in user.Claims
                where uc.ClaimValue == claim.Value && uc.ClaimType == claim.Type
                select uc).ToList<IdentityUserClaim>();

            foreach (IdentityUserClaim current in list)
            {
                user.Claims.Remove(current);
            }

            return Task.Run(() =>
            {
                using (var ctx = _context.BeginTransaction())
                {
                    ctx.Connection.Execute(
                        sql: @"DELETE FROM [dbo].[AspNetUserClaims] WHERE [Id] IN @Ids",
                        param: new { Ids = list.Select(i => i.Id).ToArray() },
                        transaction: ctx.Transaction);

                    ctx.Commit();
                }
            });
        }

        public Task<bool> GetEmailConfirmedAsync(IdentityUser user)
        {
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }
            return Task.FromResult<bool>(user.EmailConfirmed);
        }

        public Task SetEmailConfirmedAsync(IdentityUser user, bool confirmed)
        {
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }
            user.EmailConfirmed = confirmed;
            return Task.FromResult<int>(0);
        }

        public Task SetEmailAsync(IdentityUser user, string email)
        {
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }
            user.Email = email;
            return Task.FromResult<int>(0);
        }

        public Task<string> GetEmailAsync(IdentityUser user)
        {
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }
            return Task.FromResult<string>(user.Email);
        }

        public Task<IdentityUser> FindByEmailAsync(string email)
        {
            ThrowIfDisposed();
            return GetIdentityUserByEmailAsync(email);
        }

        public virtual Task<IdentityUser> FindByIdAsync(string userId)
        {
            ThrowIfDisposed();
            return GetIdentityUserAsync(userId);
        }

        public virtual Task<IdentityUser> FindByNameAsync(string userName)
        {
            ThrowIfDisposed();
            return GetIdentityUserByUserNameAsync(userName);
        }

        public virtual async Task CreateAsync(IdentityUser user)
        {
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            await CreateOrUpdateIdentityUserAsync(user);
        }

        public virtual async Task DeleteAsync(IdentityUser user)
        {
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }
            
            await DeleteIdentityUser(user);
        }

        public virtual async Task UpdateAsync(IdentityUser user)
        {
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            await CreateOrUpdateIdentityUserAsync(user);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public virtual async Task<IdentityUser> FindAsync(UserLoginInfo login)
		{
			ThrowIfDisposed();
			if (login == null)
			{
				throw new ArgumentNullException("login");
			}

			return await GetIdentityUserByLoginInfoAsync(login);
		}

        public virtual Task AddLoginAsync(IdentityUser user, UserLoginInfo login)
        {
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }
            if (login == null)
            {
                throw new ArgumentNullException("login");
            }

            string provider = login.LoginProvider;
            string key = login.ProviderKey;
            if (!user.Logins.Any(l => l.ProviderKey == key && l.LoginProvider == provider))
            {
                user.Logins.Add(new IdentityUserLogin
                {
                    UserId = user.Id,
                    ProviderKey = key,
                    LoginProvider = provider
                });
            }

            return Task.FromResult<int>(0);
        }

        public virtual Task RemoveLoginAsync(IdentityUser user, UserLoginInfo login)
        {
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }
            if (login == null)
            {
                throw new ArgumentNullException("login");
            }
            string provider = login.LoginProvider;
            string key = login.ProviderKey;
            var identityUserLogin = user.Logins.SingleOrDefault(l => l.LoginProvider == provider && l.ProviderKey == key);
            if (identityUserLogin != null)
            {
                user.Logins.Remove(identityUserLogin);
            }
            return Task.FromResult<int>(0);
        }

        public virtual Task<IList<UserLoginInfo>> GetLoginsAsync(IdentityUser user)
        {
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }
            IList<UserLoginInfo> result = (
                from l in user.Logins
                select new UserLoginInfo(l.LoginProvider, l.ProviderKey)).ToList<UserLoginInfo>();
            return Task.FromResult<IList<UserLoginInfo>>(result);
        }

        public Task SetPasswordHashAsync(IdentityUser user, string passwordHash)
        {
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }
            user.PasswordHash = passwordHash;
            return Task.FromResult<int>(0);
        }

        public Task<string> GetPasswordHashAsync(IdentityUser user)
        {
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }
            return Task.FromResult<string>(user.PasswordHash);
        }

        public Task<bool> HasPasswordAsync(IdentityUser user)
        {
            return Task.FromResult<bool>(user.PasswordHash != null);
        }

        public Task SetPhoneNumberAsync(IdentityUser user, string phoneNumber)
        {
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }
            user.PhoneNumber = phoneNumber;
            return Task.FromResult<int>(0);
        }

        public Task<string> GetPhoneNumberAsync(IdentityUser user)
        {
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }
            return Task.FromResult<string>(user.PhoneNumber);
        }

        public Task<bool> GetPhoneNumberConfirmedAsync(IdentityUser user)
        {
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }
            return Task.FromResult<bool>(user.PhoneNumberConfirmed);
        }

        public Task SetPhoneNumberConfirmedAsync(IdentityUser user, bool confirmed)
        {
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }
            user.PhoneNumberConfirmed = confirmed;
            return Task.FromResult<int>(0);
        }

        public virtual async Task AddToRoleAsync(IdentityUser user, string roleName)
        {
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            IdentityRole identityRole = await GetRoleByName(roleName);
            if (identityRole == null)
            {
                throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Resources.RoleNotFound, roleName));
            }

            if (!user.Roles.Any(r => r.RoleId == identityRole.Id))
            {
                user.Roles.Add(new IdentityUserRole
                {
                    UserId = user.Id,
                    RoleId = identityRole.Id
                });
            }
        }

        Task<IdentityRole> GetRoleByName(string roleName)
        {
            if (string.IsNullOrWhiteSpace(roleName))
            {
                //throw new ArgumentException(Resources.ValueCannotBeNullOrEmpty, "roleName");
                return Task.FromResult<IdentityRole>(null);
            }
            else
            {
                return Task.Run(() =>
                {
                    roleName = roleName.ToUpper();
                    using (var ctx = _context.Open())
                        return ctx.Connection.Query<IdentityRole>(
                            sql: @"SELECT [Id], [Name] FROM [dbo].[AspNetRoles] WHERE UPPER([Name]) = @roleName",
                            param: new { roleName },
                            transaction: ctx.Transaction).FirstOrDefault();
                });
            }
        }

        public virtual async Task RemoveFromRoleAsync(IdentityUser user, string roleName)
        {
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            IdentityRole identityRole = await GetRoleByName(roleName);
            if (null != identityRole)
            {
                var identityUserRole = user.Roles.FirstOrDefault(r => r.RoleId == identityRole.Id);
                if (null != identityUserRole)
                {
                    user.Roles.Remove(identityUserRole);
                }
            }
        }

        public virtual Task<IList<string>> GetRolesAsync(IdentityUser user)
        {
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            return Task.Run(() =>
            {
                using (var ctx = _context.Open())
                    return ctx.Connection.Query<string>(
                            sql: @"SELECT [Name] FROM [dbo].[AspNetRoles] WHERE [Id] IN @Ids",
                            param: new { Ids = user.Roles.Select(i => i.RoleId).ToArray() },
                            transaction: ctx.Transaction).ToList<string>() as IList<string>;
            });
        }

        public virtual async Task<bool> IsInRoleAsync(IdentityUser user, string roleName)
        {
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            if (string.IsNullOrWhiteSpace(roleName))
            {
                throw new ArgumentException(Resources.ValueCannotBeNullOrEmpty, "roleName");
            }

            roleName = await Task.Run(() =>
            {
                using (var ctx = _context.Open())
                    return ctx.Connection.Query<string>(
                            sql: @"SELECT [Name] FROM [dbo].[AspNetRoles] WHERE [Id] IN @Ids AND UPPER([Name]) = @Name",
                            param: new { Ids = user.Roles.Select(i => i.RoleId).ToArray(), roleName = roleName.ToUpper() },
                            transaction: ctx.Transaction).FirstOrDefault();
            });

            return !string.IsNullOrWhiteSpace(roleName);
        }

        public Task SetSecurityStampAsync(IdentityUser user, string stamp)
        {
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }
            user.SecurityStamp = stamp;
            return Task.FromResult<int>(0);
        }

        public Task<string> GetSecurityStampAsync(IdentityUser user)
        {
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }
            return Task.FromResult<string>(user.SecurityStamp);
        }

        public Task SetTwoFactorEnabledAsync(IdentityUser user, bool enabled)
        {
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }
            user.TwoFactorEnabled = enabled;
            return Task.FromResult<int>(0);
        }

        public Task<bool> GetTwoFactorEnabledAsync(IdentityUser user)
        {
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }
            return Task.FromResult<bool>(user.TwoFactorEnabled);
        }

        Task<IdentityUser> GetIdentityUserByLoginInfoAsync(UserLoginInfo login)
        {
            if (null == login || string.IsNullOrWhiteSpace(login.LoginProvider) || string.IsNullOrWhiteSpace(login.ProviderKey))
            {
                return Task.FromResult<IdentityUser>(null);
            }
            else
            {
                return Task.Run(() =>
                {
                    using (var ctx = _context.Open())
                    {
                        string id = ctx.Connection.Query<string>(sql: @"SELECT [UserId] FROM [dbo].[AspNetUserLogins] WHERE [LoginProvider] = @LoginProvider AND [ProviderKey] = @ProviderKey",
                            param: login,
                            transaction: ctx.Transaction).SingleOrDefault();

                        return GetIdentityUserAsync(ctx, id);
                    }
                });
            }
        }

        Task<IdentityUser> GetIdentityUserByEmailAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return Task.FromResult<IdentityUser>(null);
            }
            else
            {
                return Task.Run(() =>
                {
                    using (var ctx = _context.Open())
                    {
                        var id = ctx.Connection.Query<string>(sql: @"SELECT [Id] FROM [dbo].[AspNetUsers] WHERE [Email] = LOWER(@email)",
                            param: new { email },
                            transaction: ctx.Transaction).SingleOrDefault();

                        return GetIdentityUserAsync(ctx, id);
                    }
                });
            }
        }

        Task<IdentityUser> GetIdentityUserByUserNameAsync(string userName)
        {
            if (string.IsNullOrWhiteSpace(userName))
            {
                return Task.FromResult<IdentityUser>(null);
            }
            else
            {
                return Task.Run(() =>
                {
                    using (var ctx = _context.Open())
                    {
                        var id = ctx.Connection.Query<string>(sql: @"SELECT [Id] FROM [dbo].[AspNetUsers] WHERE [UserName] = LOWER(@userName)",
                            param: new { userName },
                            transaction: ctx.Transaction).SingleOrDefault();

                        return GetIdentityUserAsync(ctx, id);
                    }
                });
            }
        }

        Task<IdentityUser> GetIdentityUserAsync(string id)
        {
            return Task.Run(() =>
            {
                using (var ctx = _context.Open())
                    return GetIdentityUserAsync(ctx, id);
            });
        }

        IdentityUser GetIdentityUserAsync(IDbConnectionContext ctx, string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return null;
            }
            else
            {
                string query = @"
SELECT [Id]
      ,[Email]
      ,[EmailConfirmed]
      ,[PasswordHash]
      ,[SecurityStamp]
      ,[PhoneNumber]
      ,[PhoneNumberConfirmed]
      ,[TwoFactorEnabled]
      ,[LockoutEndDateUtc]
      ,[LockoutEnabled]
      ,[AccessFailedCount]
      ,[UserName]
  FROM [dbo].[AspNetUsers]
 WHERE [ID] = @ID
SELECT [UserId]
      ,[RoleId]
  FROM [dbo].[AspNetUserRoles]
 WHERE [UserId] = @ID
SELECT [LoginProvider]
      ,[ProviderKey]
      ,[UserId]
  FROM [dbo].[AspNetUserLogins]
 WHERE [UserId] = @ID
SELECT [Id]
      ,[UserId]
      ,[ClaimType]
      ,[ClaimValue]
  FROM [dbo].[AspNetUserClaims]
 WHERE [UserId] = @ID
";

                using (var multi = ctx.Connection.QueryMultiple(sql: query, param: new { id }, transaction: ctx.Transaction))
                {
                    IdentityUser ua = multi.Read<IdentityUser>().SingleOrDefault();
                    if (null != ua)
                    {
                        (ua.Roles as List<IdentityUserRole>).AddRange(multi.Read<IdentityUserRole>());
                        (ua.Logins as List<IdentityUserLogin>).AddRange(multi.Read<IdentityUserLogin>());
                        (ua.Claims as List<IdentityUserClaim>).AddRange(multi.Read<IdentityUserClaim>());
                    }
                    return ua;
                }
            }
        }

        Task<IdentityUser> CreateOrUpdateIdentityUserAsync(IdentityUser user)
        {
            using (var ctx = _context.BeginTransaction())
            {
                var insert = CreateOrUpdateIdentityUser(ctx, user);
                CreateOrUpdateIdentityUserRole(ctx, user, insert);
                CreateOrUpdateIdentityUserLogin(ctx, user, insert);
                CreateOrUpdateIdentityUserClaim(ctx, user, insert);

                ctx.Commit();
            }
            return Task.FromResult(user);
        }

        Task DeleteIdentityUser(IdentityUser user)
        {
            using (var ctx = _context.BeginTransaction())
            {
                ctx.Connection.Execute(
                    sql: @"DELETE FROM [dbo].[AspNetUsers] WHERE [Id] = @Id",
                    param: new { user.Id },
                    transaction: ctx.Transaction);

                ctx.Commit();
            }
            return Task.FromResult<int>(0);
        }

        bool CreateOrUpdateIdentityUser(IDbConnectionContext ctx, IdentityUser user)
        {
            if (string.IsNullOrWhiteSpace(user.Id))
            {
                user.Id = Guid.NewGuid().ToString("D");
            }

            bool insert = false;
            int update = ctx.Connection.Execute(sql: @"UPDATE [dbo].[AspNetUsers]
   SET [Email] = LOWER(@Email)
      ,[EmailConfirmed] = @EmailConfirmed
      ,[PasswordHash] = @PasswordHash
      ,[SecurityStamp] = @SecurityStamp
      ,[PhoneNumber] = @PhoneNumber
      ,[PhoneNumberConfirmed] = @PhoneNumberConfirmed
      ,[TwoFactorEnabled] = @TwoFactorEnabled
      ,[LockoutEndDateUtc] = @LockoutEndDateUtc
      ,[LockoutEnabled] = @LockoutEnabled
      ,[AccessFailedCount] = @AccessFailedCount
      ,[UserName] = LOWER(@UserName)
 WHERE [Id] = @Id", param: user, transaction: ctx.Transaction);
            if (0 == update)
            {
                insert = true;
                ctx.Connection.Execute(sql: @"INSERT INTO [dbo].[AspNetUsers]
           ([Id]
           ,[Email]
           ,[EmailConfirmed]
           ,[PasswordHash]
           ,[SecurityStamp]
           ,[PhoneNumber]
           ,[PhoneNumberConfirmed]
           ,[TwoFactorEnabled]
           ,[LockoutEndDateUtc]
           ,[LockoutEnabled]
           ,[AccessFailedCount]
           ,[UserName])
     VALUES
           (@Id
           ,LOWER(@Email)
           ,@EmailConfirmed
           ,@PasswordHash
           ,@SecurityStamp
           ,@PhoneNumber
           ,@PhoneNumberConfirmed
           ,@TwoFactorEnabled
           ,@LockoutEndDateUtc
           ,@LockoutEnabled
           ,@AccessFailedCount
           ,LOWER(@UserName))", param: user, transaction: ctx.Transaction);
            }
            return insert;
        }

        void CreateOrUpdateIdentityUserClaim(IDbConnectionContext ctx, IdentityUser user, bool insert)
        {
            foreach (var claim in user.Claims)
            {
                if (string.IsNullOrWhiteSpace(claim.Id))
                {
                    claim.Id = Guid.NewGuid().ToString("D");
                }

                if (string.IsNullOrWhiteSpace(claim.UserId))
                {
                    claim.UserId = user.Id;
                }
                else
                {
                    if (claim.UserId != user.Id)
                    {
                        throw new ArgumentException(Resources.InvalidRoleUserID);
                    }
                }
            }

            ICollection<IdentityUserClaim> forDelete = null, forInsert = user.Claims;
            if (!insert)
            {
                var oldClaims = ctx.Connection.Query<IdentityUserClaim>(
                    sql: @"SELECT [Id],[UserId],[ClaimType],[ClaimValue] FROM [dbo].[AspNetUserClaims] WHERE [UserId] = @Id",
                    param: new { user.Id },
                    transaction: ctx.Transaction).ToList();

                forDelete = (from n in oldClaims
                             where !user.Claims.Any(o => o.ClaimType == n.ClaimType && o.ClaimValue == n.ClaimValue)
                             select n).ToList();

                forInsert = (from n in user.Claims
                             where !oldClaims.Any(o => o.ClaimType == n.ClaimType && o.ClaimValue == n.ClaimValue)
                             select n).ToList();
            }

            if (null != forDelete)
            {
                ctx.Connection.Execute(
                    sql: @"DELETE FROM [dbo].[AspNetUserClaims] WHERE [Id] IN @Ids",
                    param: new { Ids = forDelete.Select(c => c.Id).ToArray() },
                    transaction: ctx.Transaction);
            }

            ctx.Connection.Execute(sql: @"INSERT INTO [dbo].[AspNetUserClaims]([Id],[UserId],[ClaimType],[ClaimValue])
     VALUES(@Id,@UserId,@ClaimType,@ClaimValue)", param: forInsert, transaction: ctx.Transaction);  
        }
        
        void CreateOrUpdateIdentityUserLogin(IDbConnectionContext ctx, IdentityUser user, bool insert)
        {
            //check user ids
            foreach (var login in user.Logins)
            {
                if (string.IsNullOrWhiteSpace(login.UserId))
                {
                    login.UserId = user.Id;
                }
                else
                {
                    if (login.UserId != user.Id)
                    {
                        throw new ArgumentException(Resources.InvalidRoleUserID);
                    }
                }
            }

            ICollection<IdentityUserLogin> forDelete = null, forInsert = user.Logins;
            if (!insert)
            {
                var oldLogins = ctx.Connection.Query<IdentityUserLogin>(
                    sql: @"SELECT [LoginProvider],[ProviderKey],[UserId] FROM [dbo].[AspNetUserLogins] WHERE [UserId] = @Id",
                    param: new { user.Id },
                    transaction: ctx.Transaction).ToList();

                forDelete = (from n in oldLogins
                             where !user.Logins.Any(o => o.LoginProvider == n.LoginProvider && o.ProviderKey == n.ProviderKey)
                             select n).ToList();

                forInsert = (from n in user.Logins
                             where !oldLogins.Any(o => o.LoginProvider == n.LoginProvider && o.ProviderKey == n.ProviderKey)
                             select n).ToList();
            }

            if (null != forDelete)
            {
                ctx.Connection.Execute(
                    sql: @"DELETE FROM [dbo].[AspNetUserLogins] WHERE [LoginProvider] = @LoginProvider AND [ProviderKey] = @ProviderKey AND [UserId] = @UserId",
                    param: forDelete,
                    transaction: ctx.Transaction);
            }

            ctx.Connection.Execute(sql: @"INSERT INTO [dbo].[AspNetUserLogins]([LoginProvider],[ProviderKey],[UserId])
     VALUES(@LoginProvider,@ProviderKey,@UserId)", param: forInsert, transaction: ctx.Transaction);  
        }

        void CreateOrUpdateIdentityUserRole(IDbConnectionContext ctx, IdentityUser user, bool insert)
        {
            //check user ids
            foreach (var role in user.Roles)
            {
                if (string.IsNullOrWhiteSpace(role.UserId))
                {
                    role.UserId = user.Id;
                }
                else
                {
                    if (role.UserId != user.Id)
                    {
                        throw new ArgumentException(Resources.InvalidRoleUserID);
                    }
                }
            }

            ICollection<IdentityUserRole> forDelete = null, forInsert = user.Roles;
            if (!insert)
            {
                var oldRoles = ctx.Connection.Query<IdentityUserRole>(
                    sql: @"SELECT [UserId],[RoleId] FROM [dbo].[AspNetUserRoles] WHERE [UserId] = @Id",
                    param: new { user.Id },
                    transaction: ctx.Transaction).ToList();

                forDelete = (from n in oldRoles
                             where !user.Roles.Any(o => o.UserId == n.UserId && o.RoleId == n.RoleId)
                             select n).ToList();

                forInsert = (from n in user.Roles
                             where !oldRoles.Any(o => o.UserId == n.UserId && o.RoleId == n.RoleId)
                             select n).ToList();
            }

            if (null != forDelete)
            {
                ctx.Connection.Execute(
                    sql: @"DELETE FROM [dbo].[AspNetUserRoles] WHERE [UserId] = @UserId AND [RoleId] = @RoleId",
                    param: forDelete,
                    transaction: ctx.Transaction);
            }

            ctx.Connection.Execute(sql: @"INSERT INTO [dbo].[AspNetUserRoles]([UserId],[RoleId])
     VALUES(@UserId,@RoleId)", param: forInsert, transaction: ctx.Transaction);        
        }

        void ThrowIfDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (DisposeContext && disposing && _context != null)
            {
                _context.Dispose();
            }
            _disposed = true;
            _context = null;
        }
    }
}
