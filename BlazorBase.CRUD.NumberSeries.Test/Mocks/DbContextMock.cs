using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorBase.CRUD.NumberSeries.Test.Mocks
{
    public class DbContextMock : DbContext
    {
        public DbContextMock(DbContextOptions<DbContextMock> options) : base(options) { }

        public DbSet<NoSeries> NoSeries { get; set; }
    }
}
