﻿// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System.Threading;
using System.Threading.Tasks;
using DataLayer.EfClassesSql;
using DataLayer.EfCode.Configurations;
using DataLayer.NoSqlCode;
using Microsoft.EntityFrameworkCore;

namespace DataLayer.EfCode
{
    public class SqlDbContext : DbContext
    {
        private readonly IBookUpdater _bookUpdater;

        //NOTE: if the bookUpdater isn't provided, then it reverts to a normal SaveChanges.
        public SqlDbContext(DbContextOptions<SqlDbContext> options, IBookUpdater bookUpdater = null)      
            : base(options)
        {
            _bookUpdater = bookUpdater;
        }

        public DbSet<Book> Books { get; set; }
        public DbSet<Author> Authors { get; set; }

        //I only have to override these two version of SaveChanges, as the other two versions call these
        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            if (_bookUpdater == null || !_bookUpdater.FoundBookChangesToProjectToNoSql(this))
                return base.SaveChanges(acceptAllChangesOnSuccess);
            return _bookUpdater.CallBaseSaveChangesWithNoSqlWrite(this, () => base.SaveChanges(acceptAllChangesOnSuccess));
        }

        public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = new CancellationToken())
        {
            if (_bookUpdater == null || !_bookUpdater.FoundBookChangesToProjectToNoSql(this))
                return await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
            return await _bookUpdater.CallBaseSaveChangesWithNoSqlWriteAsync(this,
                () => base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken));
        }

        protected override void
            OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new BookConfig());       
            modelBuilder.ApplyConfiguration(new BookAuthorConfig()); 
        }
    }
}

