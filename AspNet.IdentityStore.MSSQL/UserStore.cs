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
        IDbContext _dbContext;

        public bool DisposeContext
        {
            get;
            set;
        }

        //public IQueryable<IdentityUser> Users
        //{
        //    get
        //    {
        //        return _userStore.EntitySet;
        //    }
        //}

        public UserStore(IDbContext dbContext)
        {
            if (dbContext == null)
            {
                throw new ArgumentNullException("context");
            }
            _dbContext = dbContext;
        }

        public Task<DateTimeOffset> GetLockoutEndDateAsync(IdentityUser user)
        {
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }
            return Task.FromResult<DateTimeOffset>(user.LockoutEndDateUtc.HasValue ? new DateTimeOffset(DateTime.SpecifyKind(user.LockoutEndDateUtc.Value, DateTimeKind.Utc)) : default(DateTimeOffset));
        }

        public Task SetLockoutEndDateAsync(IdentityUser user, DateTimeOffset lockoutEnd)
        {
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            user.LockoutEndDateUtc =
                ((lockoutEnd == DateTimeOffset.MinValue) ? null : new DateTime?(lockoutEnd.UtcDateTime));

            //_dbContext.UpdateProperty(user, () => user.LockoutEndDateUtc, 
            //    ((lockoutEnd == DateTimeOffset.MinValue) ? null : new DateTime?(lockoutEnd.UtcDateTime)));

            return Task.FromResult<int>(0);
        }

        public Task<int> IncrementAccessFailedCountAsync(IdentityUser user)
        {
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }
            user.AccessFailedCount++;
            return Task.FromResult<int>(user.AccessFailedCount);
        }

        public Task ResetAccessFailedCountAsync(IdentityUser user)
        {
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }
            user.AccessFailedCount = 0;
            return Task.FromResult<int>(0);
        }

        public Task<int> GetAccessFailedCountAsync(IdentityUser user)
        {
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }
            return Task.FromResult<int>(user.AccessFailedCount);
        }

        public Task<bool> GetLockoutEnabledAsync(IdentityUser user)
        {
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }
            return Task.FromResult<bool>(user.LockoutEnabled);
        }

        public Task SetLockoutEnabledAsync(IdentityUser user, bool enabled)
        {
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            user.LockoutEnabled = enabled;

            //_dbContext.UpdateProperty(user, () => user.LockoutEnabled, enabled);

            return Task.FromResult<int>(0);
        }

        public virtual Task<IList<Claim>> GetClaimsAsync(IdentityUser user)
        {
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            return Task.Run(() =>
            {
                return _dbContext.GetClaims(user);
            });
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

            return Task.Run(() =>
            {
                _dbContext.AddClaim(user, claim);
            });
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

            return Task.Run(() =>
            {
                _dbContext.RemoveClaim(user, claim);
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
            
            //_dbContext.UpdateProperty(user, () => user.EmailConfirmed, confirmed);

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

            //_dbContext.UpdateProperty(user, () => user.Email, email);

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
            return Task.Run(() =>
            {
                return _dbContext.GetUserByEmail(email);
            });
        }

        public virtual Task<IdentityUser> FindByIdAsync(string userId)
        {
            ThrowIfDisposed();
            return Task.Run(() =>
            {
                return _dbContext.GetUserById(userId);
            });
        }

        public virtual Task<IdentityUser> FindByNameAsync(string userName)
        {
            ThrowIfDisposed();
            return Task.Run(() =>
            {
                return _dbContext.GetUserByUserName(userName);
            });
        }

        public virtual Task CreateAsync(IdentityUser user)
        {
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            return Task.Run(() =>
            {
                _dbContext.InsertOrUpdate(user);
            });
        }

        public virtual Task DeleteAsync(IdentityUser user)
        {
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            return Task.Run(() =>
            {
                _dbContext.Delete(user);
            });
        }

        public virtual Task UpdateAsync(IdentityUser user)
        {
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            return Task.Run(() =>
            {
                _dbContext.InsertOrUpdate(user);
            });
        }

        public virtual Task<IdentityUser> FindAsync(UserLoginInfo login)
        {
            ThrowIfDisposed();
            if (login == null)
            {
                throw new ArgumentNullException("login");
            }

            return Task.Run(() =>
            {
                return _dbContext.GetUserByLoginInfo(login);
            });
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

            return Task.Run(() =>
            {
                _dbContext.AddLogin(user, login);
            });
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

            return Task.Run(() =>
            {
                _dbContext.RemoveLogin(user, login);
            });
        }

        public virtual Task<IList<UserLoginInfo>> GetLoginsAsync(IdentityUser user)
        {
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            return Task.Run(() =>
            {
                return _dbContext.GetLogins(user);
            });
        }

        public Task SetPasswordHashAsync(IdentityUser user, string passwordHash)
        {
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            user.PasswordHash = passwordHash;
            
            //_dbContext.UpdateProperty(user, () => user.PasswordHash, passwordHash);

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
            
            //_dbContext.UpdateProperty(user, () => user.PhoneNumber, phoneNumber);

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
            
            //_dbContext.UpdateProperty(user, () => user.PhoneNumberConfirmed, confirmed);

            return Task.FromResult<int>(0);
        }

        public virtual async Task AddToRoleAsync(IdentityUser user, string roleName)
        {
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            IdentityRole role = await _dbContext.GetRoleByName(roleName);
            if (role == null)
            {
                throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Resources.RoleNotFound, roleName));
            }

            await Task.Run(() =>
            {
                _dbContext.AddRole(user, role);
            });
        }

        public virtual async Task RemoveFromRoleAsync(IdentityUser user, string roleName)
        {
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            IdentityRole role = await _dbContext.GetRoleByName(roleName);
            if (null != role)
            {
                await Task.Run(() =>
                {
                    _dbContext.RemoveRole(user, role);
                });
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
                return _dbContext.GetRoles(user);
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

            return await Task.Run(() =>
            {
                return _dbContext.IsInRole(user, roleName);
            });
        }

        public Task SetSecurityStampAsync(IdentityUser user, string stamp)
        {
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            user.SecurityStamp = stamp;

            //_dbContext.UpdateProperty(user, () => user.SecurityStamp, stamp);

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

            //_dbContext.UpdateProperty(user, () => user.TwoFactorEnabled, enabled);

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

        void ThrowIfDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (DisposeContext && disposing && _dbContext != null)
            {
                _dbContext.Dispose();
            }
            _disposed = true;
            _dbContext = null;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
