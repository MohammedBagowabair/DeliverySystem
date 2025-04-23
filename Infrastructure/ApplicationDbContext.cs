using Application.Interfaces;
using Domain.Common.Models;
using Domain.Entities;
using Infrastructure.Configurations.Validators;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Linq.Expressions;


namespace Infrastructure
{
    public class ApplicationDbContext : DbContext, IApplicationDbContext
    {
        public DbContext _dbContext => this;
        private readonly IConfiguration _configuration;

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IConfiguration configuration) : base(options)
        {
            _configuration = configuration;
        }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Driver> Drivers { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Order> Orders { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var connectionString = _configuration.GetConnectionString("MySqlConnectionStrings");
            optionsBuilder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));

            optionsBuilder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString), mySqlOptionsAction =>
            {
                mySqlOptionsAction.EnableRetryOnFailure();
            });
        }
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
            try
            {
                Set<T>().Update(entity);
                await SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

        }
        public async Task<PagedList<T>> GetPagedAsync<T>(int page, int PageSize, System.Linq.Expressions.Expression<Func<T, bool>> predicate = null, string[] includes = null, bool IsOrde = false) where T : BaseEntity
        {
            IQueryable<T> query = Set<T>().AsNoTracking();
            var count = predicate is not null ? await query.Where(predicate).CountAsync() : await query.CountAsync();
            query = predicate != null ? query.Where(predicate) : query;
            query = IsOrde == false ? query.OrderBy(f => f.Id) : query.OrderByDescending(x => x.Id);
            query = query.Skip((page - 1) * PageSize);
            query = query.Take(PageSize);
            if (includes != null && includes.Length > 0)
            {
                foreach (string include in includes)
                {
                    query = query.Include(include);
                }
            }
            var entities = await query.ToListAsync();
            return new PagedList<T>(count, entities, page, PageSize);
        }

        //------------------------------------------Dashboard--------------------------------------------------
        // Count quntity of any tables(total users,total drivers,total customers, and total orders)

        public async Task<int> CountAsync<T>(Expression<Func<T, bool>> predicate = null) where T : BaseEntity
        {
            IQueryable<T> query = Set<T>();

            if (predicate != null)
            {
                query = query.Where(predicate);
            }

            return await query.CountAsync();
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
