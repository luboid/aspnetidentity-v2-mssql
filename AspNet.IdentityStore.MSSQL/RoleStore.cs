using Microsoft.AspNet.Identity;
using System;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;

using Dapper;

namespace AspNet.IdentityStore
{
    public class RoleStore : IRoleStore<IdentityRole, string>, IDisposable /*IQueryableRoleStore<IdentityRole, Guid>,*/
	{
		bool _disposed;
        IDbContext _dbContext;

        public bool DisposeContext
		{
			get;
			set;
		}
		
        /*public IQueryable<IdentityRole> Roles
		{
			get
			{
				return this._roleStore.EntitySet;
			}
		}*/

        public RoleStore(IDbContext dbContext)
		{
			if (dbContext == null)
			{
				throw new ArgumentNullException("context");
			}
            _dbContext = dbContext;
		}

        public Task<IdentityRole> FindByIdAsync(string roleId)
        {
            this.ThrowIfDisposed();
            if (!string.IsNullOrWhiteSpace(roleId))
            {
                return Task.Run(() =>
                {
                    return _dbContext.FindRoleById(roleId);
                });
            }
            else
            {
                return Task.FromResult<IdentityRole>(null);
            }
        }

        public Task<IdentityRole> FindByNameAsync(string roleName)
        {
            this.ThrowIfDisposed();
            if (string.IsNullOrWhiteSpace(roleName))
            {
                return Task.FromResult<IdentityRole>(null);
            }
            else
            {
                return Task.Run(() =>
                {
                    return _dbContext.FindRoleByName(roleName);
                });
            }
        }

        public virtual async Task CreateAsync(IdentityRole role)
        {
            this.ThrowIfDisposed();

            if (role == null)
            {
                throw new ArgumentNullException("role");
            }

            await Task.Run(() =>
            {
                _dbContext.InsertOrUpdate(role);
            });
        }

        public virtual async Task DeleteAsync(IdentityRole role)
        {
            this.ThrowIfDisposed();
            if (role == null)
            {
                throw new ArgumentNullException("role");
            }

            await Task.Run(() =>
            {
                _dbContext.Delete(role);
            });
        }

        public virtual async Task UpdateAsync(IdentityRole role)
        {
            this.ThrowIfDisposed();
            if (role == null)
            {
                throw new ArgumentNullException("role");
            }

            await Task.Run(() =>
            {
                _dbContext.InsertOrUpdate(role);
            });
        }
		
        public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}
		
        private void ThrowIfDisposed()
		{
			if (this._disposed)
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}
		}
		
        protected virtual void Dispose(bool disposing)
		{
			if (this.DisposeContext && disposing && this._dbContext != null)
			{
                _dbContext.Dispose();
			}
			_disposed = true;
            _dbContext = null;
		}
	}
}
