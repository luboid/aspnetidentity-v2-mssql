using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AspNet.IdentityStore
{
    public interface IDbContext : IDisposable
    {
        IDbConnectionContext Open();
        IDbConnectionContext BeginTransaction();
    }
}
