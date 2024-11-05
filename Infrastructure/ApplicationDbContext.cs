using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Configurations.Validators;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;


namespace Infrastructure
{
    public class ApplicationDbContext : DbContext, IApplicationDbContext
    {
        //public DbContext DbContext => this;
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }

        public DbSet<Customer> Customers { get; set; }
        public DbSet<Driver> Drivers { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Order> Orders { get; set; }


        public async Task<T> AddAsync<T>(T entity) where T : BaseEntity
        {
            await Set<T>().AddAsync(entity);
            await SaveChangesAsync();
            return entity;
        }

        public async Task<bool> DeleteAsync<T>(int id) where T : BaseEntity
        {
            var entity = await Set<T>().FindAsync(id) ?? throw new Exception("Record Not Found");
            Set<T>().Remove(entity);

            await SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<T>> GetAsync<T>(Expression<Func<T, bool>> predicate = null, Func<IQueryable<T>, IQueryable<T>> includeExpression = null
          ) where T : BaseEntity
        {
            IQueryable<T> query = Set<T>().AsNoTracking();

            if (includeExpression != null)
            {
                query = includeExpression(query);
            }

            if (predicate != null)
            {
                query = query.Where(predicate);
            }
            // query=query.OrderByDescending(x=>x.Id);
            return await query.ToListAsync();
        }

        public async Task UpdateAsync<T>(T entity) where T : BaseEntity
        {
            Set<T>().Update(entity);
            await SaveChangesAsync();
        }



        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            new CustomerValidator().Configure(modelBuilder.Entity<Customer>());
            new DriverValidator().Configure(modelBuilder.Entity<Driver>());
            new OrderValidator().Configure(modelBuilder.Entity<Order>());
            new UserValidator().Configure(modelBuilder.Entity<User>());
            base.OnModelCreating(modelBuilder);
        }

    }
}
