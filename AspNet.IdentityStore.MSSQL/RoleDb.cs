using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Dapper;

namespace AspNet.IdentityStore
{
    internal static class RoleDb
    {
        public static void InsertOrUpdate(this IDbContext context, IdentityRole role)
        {
            if (string.IsNullOrWhiteSpace(role.Id))
            {
                role.Id = Guid.NewGuid().ToString("D");
            }

            using (var ctx = context.BeginTransaction())
            {
                int update = ctx.Connection.Execute(@"UPDATE [dbo].[AspNetRoles] SET [Name] = @Name WHERE [Id] = @Id",
                    param: role,
                    transaction: ctx.Transaction);

                if (0 == update)
                {
                    ctx.Connection.Execute(@"INSERT INTO [dbo].[AspNetRoles]([Id],[Name]) VALUES(@Id,@Name)",
                        param: role,
                        transaction: ctx.Transaction);
                }

                ctx.Commit();
            }
        }

        public static void Delete(this IDbContext context, IdentityRole role)
        {
            using (var ctx = context.BeginTransaction())
            {
                ctx.Connection.Execute(@"DELETE FROM [dbo].[AspNetRoles] WHERE [Id] = @Id",
                    param: new { role.Id },
                    transaction: ctx.Transaction);

                ctx.Commit();
            }
        }


        public static IdentityRole FindRoleById(this IDbContext context, string roleId)
        {
            if (!string.IsNullOrWhiteSpace(roleId))
            {
                using (var ctx = context.Open())
                    return ctx.Connection.Query<IdentityRole>("SELECT * FROM [dbo].[AspNetRoles] WHERE [Id] = @roleId",
                        param: new { roleId },
                        transaction: ctx.Transaction).FirstOrDefault();
            }
            else
            {
                return null;
            }
        }

        public static IdentityRole FindRoleByName(this IDbContext context, string roleName)
        {
            if (string.IsNullOrWhiteSpace(roleName))
            {
                return null;
            }
            else
            {
                roleName = roleName.ToUpper();
                using (var ctx = context.Open())
                    return ctx.Connection.Query<IdentityRole>("SELECT * FROM [dbo].[AspNetRoles] WHERE UPPER([Name]) = @roleName",
                        param: new { roleName },
                        transaction: ctx.Transaction).FirstOrDefault();
            }
        }
        
        public static Task<IdentityRole> GetRoleByName(this IDbContext context, string roleName)
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
                    using (var ctx = context.Open())
                        return ctx.Connection.Query<IdentityRole>(
                            sql: @"SELECT [Id], [Name] FROM [dbo].[AspNetRoles] WHERE UPPER([Name]) = @roleName",
                            param: new { roleName },
                            transaction: ctx.Transaction).FirstOrDefault();
                });
            }
        }
    }
}
