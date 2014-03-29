using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using AspNet.IdentityStore.Properties;
using Microsoft.AspNet.Identity;
using System.Security.Claims;
using System.Linq.Expressions;
using System.Reflection;

namespace AspNet.IdentityStore
{
    internal static class UserDb
    {
        public static void InsertOrUpdate(this IDbContext context, IdentityUser user)
        {
            if (string.IsNullOrWhiteSpace(user.Id))
            {
                user.Id = Guid.NewGuid().ToString("D");
            }

            using (var ctx = context.BeginTransaction())
            {
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

                ctx.Commit();
            }
        }

        public static void Delete(this IDbContext context, IdentityUser user)
        {
            using (var ctx = context.BeginTransaction())
            {
                ctx.Connection.Execute(
                    sql: @"DELETE FROM [dbo].[AspNetUsers] WHERE [Id] = @Id",
                    param: new { user.Id },
                    transaction: ctx.Transaction);

                ctx.Commit();
            }
        }

        public static IList<Claim> GetClaims(this IDbContext context, IdentityUser user)
        {
            using (var ctx = context.Open())
                return ctx.Connection.Query(
                        sql: @"SELECT [ClaimType], [ClaimValue] FROM [dbo].[AspNetUserClaims] WHERE [UserId] = @Id",
                        param: new { user.Id },
                        transaction: ctx.Transaction)
                        .Select(c => new Claim(c.ClaimType, c.ClaimValue))
                        .ToList<Claim>();
        }

        public static void AddClaim(this IDbContext context, IdentityUser user, Claim claim)
        {
            using (var ctx = context.BeginTransaction())
            {
                var id = ctx.Connection.Query<string>(
                        sql: @"SELECT [Id] FROM [dbo].[AspNetUserClaims] WHERE [UserId] = @UserId AND [ClaimType] = @ClaimType AND [ClaimValue] = @ClaimValue",
                        param: new {
                            UserId = user.Id,
                            ClaimType = claim.Type,
                            ClaimValue = claim.Value
                        },
                        transaction: ctx.Transaction).FirstOrDefault();

                if (string.IsNullOrWhiteSpace(id))
                {
                    ctx.Connection.Execute(
                        sql: @"INSERT INTO [dbo].[AspNetUserClaims]([Id],[UserId],[ClaimType],[ClaimValue])
     VALUES(@Id,@UserId,@ClaimType,@ClaimValue)",
                        param: new
                        {
                            Id = Guid.NewGuid().ToString("D"),
                            UserId = user.Id,
                            ClaimType = claim.Type,
                            ClaimValue = claim.Value
                        },
                        transaction: ctx.Transaction);
                }

                ctx.Commit();
            }
        }

        public static void RemoveClaim(this IDbContext context, IdentityUser user, Claim claim)
        {
            using (var ctx = context.BeginTransaction())
            {
                ctx.Connection.Execute(
                    sql: @"DELETE FROM [dbo].[AspNetUserClaims] WHERE [UserId] = @UserId AND [ClaimType] = @ClaimType AND [ClaimValue] = @ClaimValue",
                    param: new
                    {
                        UserId = user.Id,
                        ClaimType = claim.Type,
                        ClaimValue = claim.Value
                    },
                    transaction: ctx.Transaction);

                ctx.Commit();
            }
        }

        public static IList<UserLoginInfo> GetLogins(this IDbContext context, IdentityUser user)
        {
            using (var ctx = context.Open())
                return ctx.Connection.Query<UserLoginInfo>(
                        sql: @"SELECT [LoginProvider], [ProviderKey] FROM [dbo].[AspNetUserLogins] WHERE [UserId] = @Id",
                        param: new { user.Id },
                        transaction: ctx.Transaction).ToList();
        }

        public static void AddLogin(this IDbContext context, IdentityUser user, UserLoginInfo login)
        {
            var param = new
            {
                UserId = user.Id,
                LoginProvider = login.LoginProvider,
                ProviderKey = login.ProviderKey
            };

            using (var ctx = context.BeginTransaction())
            {
                var id = ctx.Connection.Query<string>(
                        sql: @"SELECT [UserId] FROM [dbo].[AspNetUserLogins] WHERE [LoginProvider] = @LoginProvider AND [ProviderKey] = @ProviderKey AND [UserId] = @UserId",
                        param: param,
                        transaction: ctx.Transaction).FirstOrDefault();

                if (string.IsNullOrWhiteSpace(id))
                {
                    ctx.Connection.Execute(
                        sql: @"INSERT INTO [dbo].[AspNetUserLogins]([LoginProvider],[ProviderKey],[UserId])
     VALUES(@LoginProvider,@ProviderKey,@UserId)",
                        param: param,
                        transaction: ctx.Transaction);
                }

                ctx.Commit();
            }
        }

        public static void RemoveLogin(this IDbContext context, IdentityUser user, UserLoginInfo login)
        {
            using (var ctx = context.BeginTransaction())
            {
                ctx.Connection.Execute(
                        sql: @"DELETE FROM [dbo].[AspNetUserLogins] WHERE [LoginProvider] = @LoginProvider AND [ProviderKey] = @ProviderKey AND [UserId] = @UserId",
                        param: new
                        {
                            UserId = user.Id,
                            LoginProvider = login.LoginProvider,
                            ProviderKey = login.ProviderKey
                        },
                        transaction: ctx.Transaction);

                ctx.Commit();
            }
        }

        public static bool IsInRole(this IDbContext context, IdentityUser user, string roleName)
        {
            string resultRoleName;
            using (var ctx = context.Open())
                resultRoleName = ctx.Connection.Query<string>(
                        sql: @"SELECT [Name]
  FROM [dbo].[AspNetRoles]
 WHERE [Id] IN (SELECT [RoleId]
				  FROM [dbo].[AspNetUserRoles]
				 WHERE [UserId] = @Id)
   AND UPPER([Name]) = @roleName",
                        param: new { user.Id, roleName = roleName.ToUpper() },
                        transaction: ctx.Transaction).FirstOrDefault();

            return !string.IsNullOrWhiteSpace(resultRoleName);
        }

        public static IList<string> GetRoles(this IDbContext context, IdentityUser user)
        {
            using (var ctx = context.Open())
                return ctx.Connection.Query<string>(
                        sql: @"SELECT [Name]
  FROM [dbo].[AspNetRoles]
 WHERE [Id] IN (SELECT [RoleId]
				  FROM [dbo].[AspNetUserRoles]
				 WHERE [UserId] = @Id)",
                        param: new { user.Id },
                        transaction: ctx.Transaction).ToList<string>() as IList<string>;
        }

        public static void AddRole(this IDbContext context, IdentityUser user, IdentityRole role)
        {
            var param = new
            {
                UserId = user.Id,
                RoleId = role.Id
            };

            using (var ctx = context.BeginTransaction())
            {
                var id = ctx.Connection.Query<string>(
                        sql: @"SELECT [UserId] FROM [dbo].[AspNetUserRoles] WHERE [UserId] = @UserId AND [RoleId] = @RoleId",
                        param: param,
                        transaction: ctx.Transaction).FirstOrDefault();

                if (string.IsNullOrWhiteSpace(id))
                {
                    ctx.Connection.Execute(
                        sql: @"INSERT INTO [dbo].[AspNetUserRoles]([UserId],[RoleId]) VALUES(@UserId,@RoleId)",
                        param: param,
                        transaction: ctx.Transaction);
                }

                ctx.Commit();
            }
        }

        public static void RemoveRole(this IDbContext context, IdentityUser user, IdentityRole role)
        {
            using (var ctx = context.BeginTransaction())
            {
                ctx.Connection.Execute(
                        sql: @"DELETE FROM [dbo].[AspNetUserRoles] WHERE [UserId] = @UserId AND [RoleId] = @RoleId",
                        param: new
                        {
                            UserId = user.Id,
                            RoleId = role.Id
                        },
                        transaction: ctx.Transaction);

                ctx.Commit();
            }
        }

        public static IdentityUser GetUserByLoginInfo(this IDbContext context, UserLoginInfo login)
        {
            if (string.IsNullOrWhiteSpace(login.ProviderKey) || string.IsNullOrWhiteSpace(login.LoginProvider))
            {
                return null;
            }
            else
            {
                using (var ctx = context.Open())
                    return ctx.Connection.Query<IdentityUser>(sql: @"SELECT * 
  FROM [dbo].[AspNetUsers] 
 WHERE [Id] = (SELECT [UserId] 
                 FROM [dbo].[AspNetUserLogins] 
                WHERE [LoginProvider] = @LoginProvider 
                  AND [ProviderKey] = @ProviderKey)",
                        param: login,
                        transaction: ctx.Transaction).SingleOrDefault();
            }
        }

        public static IdentityUser GetUserByEmail(this IDbContext context, string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return null;
            }
            else
            {
                using (var ctx = context.Open())
                    return ctx.Connection.Query<IdentityUser>(
                        sql: "SELECT * FROM [dbo].[AspNetUsers] WHERE [Email] = @email",
                        param: new { email = email.ToLower() },
                        transaction: ctx.Transaction).SingleOrDefault();
            }
        }

        public static IdentityUser GetUserByUserName(this IDbContext context, string userName)
        {
            if (string.IsNullOrWhiteSpace(userName))
            {
                return null;
            }
            else
            {
                using (var ctx = context.Open())
                    return ctx.Connection.Query<IdentityUser>(
                        sql: "SELECT * FROM [dbo].[AspNetUsers] WHERE [UserName] = @userName",
                        param: new { userName = userName.ToLower() },
                        transaction: ctx.Transaction).SingleOrDefault();
            }
        }

        public static IdentityUser GetUserById(this IDbContext context, string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                return null;
            }
            else
            {
                using (var ctx = context.Open())
                    return ctx.Connection.Query<IdentityUser>(
                        sql: "SELECT * FROM [dbo].[AspNetUsers] WHERE [Id] = @userId",
                        param: new { userId },
                        transaction: ctx.Transaction).SingleOrDefault();
            }
        }

        public static void UpdateProperty(this IDbContext context, IdentityUser user, Expression<Func<object>> modelProperty, object value)
        {
            var p = modelProperty.GetPropertyInfo();

            if (null == p || p.DeclaringType != user.GetType())
                throw new ArgumentException("Invalid property specified.");

            var sql = string.Format("UPDATE [dbo].[AspNetUsers] SET [{0}] = @{0} WHERE [Id] = @Id", p.Name);
            var parameters = new DynamicParameters();
            parameters.Add("@Id", user.Id);
            parameters.Add("@" + p.Name, value: value);

            using (var ctx = context.BeginTransaction())
            {
                ctx.Connection.Execute(
                    sql: sql, 
                    param: parameters, 
                    transaction: ctx.Transaction);

                ctx.Commit();
            }

            p.SetValue(user, value);
        }

        static PropertyInfo GetPropertyInfo(this Expression<Func<object>> expression)
        {
            MemberExpression memberExp = null;
            if (expression.Body.NodeType == ExpressionType.Convert)
            {
                memberExp = ((UnaryExpression)expression.Body).Operand as MemberExpression;
            }
            else if (expression.Body.NodeType == ExpressionType.MemberAccess)
            {
                memberExp = expression.Body as MemberExpression;
            }
            return ((memberExp != null) ? memberExp.Member : null) as PropertyInfo;
        }
    }
}
