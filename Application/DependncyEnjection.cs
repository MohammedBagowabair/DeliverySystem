using Application.Interfaces;
using Application.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Application
{
    public static class DependncyEnjection
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddMediatR(configuration =>
            {
                configuration.RegisterServicesFromAssemblies(Assembly.GetExecutingAssembly());
            });



            //services.AddScoped<IApplicationDbContext, ApplicationDbContext>();
            //services.AddScoped<IOrderService, OrderService>();
            //services.AddScoped<IDriverService, DriverService>();
            //services.AddScoped<ICustomerService, CustomerService>();
            //services.AddScoped<IUserService, UserService>();
            //services.AddScoped<IDashboardService, DashboardService>();
            //services.AddScoped<IPdfGeneratorService, PdfGeneratorService>();



            return services;

        }

    }
}
