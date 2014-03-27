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
        IDbContext _context;

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

        public RoleStore(IDbContext context)
		{
			if (context == null)
			{
				throw new ArgumentNullException("context");
			}
            _context = context;
		}

        public Task<IdentityRole> FindByIdAsync(string roleId)
        {
            this.ThrowIfDisposed();
            if (!string.IsNullOrWhiteSpace(roleId))
            {
                return Task.Run(() =>
                {
                    using (var ctx = _context.Open())
                        return ctx.Connection.Query<IdentityRole>("SELECT * FROM [dbo].[AspNetRoles] WHERE [Id] = @roleId",
                            param: new { roleId },
                            transaction: ctx.Transaction).FirstOrDefault();
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
                    roleName = roleName.ToUpper();
                    using (var ctx = _context.Open())
                        return ctx.Connection.Query<IdentityRole>("SELECT * FROM [dbo].[AspNetRoles] WHERE UPPER([Name]) = @roleName",
                            param: new { roleName },
                            transaction: ctx.Transaction).FirstOrDefault();
                });
            }
            //return this._roleStore.EntitySet.FirstOrDefaultAsync((IdentityRole u) => u.Name.ToUpper() == roleName.ToUpper());
        }

        public virtual async Task CreateAsync(IdentityRole role)
        {
            this.ThrowIfDisposed();

            if (role == null)
            {
                throw new ArgumentNullException("role");
            }

            if (string.IsNullOrWhiteSpace(role.Id))
            {
                role.Id = Guid.NewGuid().ToString("D");
            }

            await Task.Run(() =>
            {
                using (var ctx = _context.BeginTransaction())
                {
                    ctx.Connection.Execute(@"INSERT INTO [dbo].[AspNetRoles]([Id],[Name]) VALUES(@Id,@Name)",
                        param: role,
                        transaction: ctx.Transaction);

                    ctx.Commit();
                }
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
                using (var ctx = _context.BeginTransaction())
                {
                    ctx.Connection.Execute(@"DELETE FROM [dbo].[AspNetRoles] WHERE [Id] = @Id",
                        param: new { role.Id },
                        transaction: ctx.Transaction);

                    ctx.Commit();
                }
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
                using (var ctx = _context.BeginTransaction())
                {
                    ctx.Connection.Execute(@"UPDATE [dbo].[AspNetRoles] SET [Name] = @Name WHERE [Id] = @Id", 
                        param: role,
                        transaction: ctx.Transaction);

                    ctx.Commit();
                }
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
			if (this.DisposeContext && disposing && this._context != null)
			{
                _context.Dispose();
			}
			_disposed = true;
            _context = null;
		}
	}
}
