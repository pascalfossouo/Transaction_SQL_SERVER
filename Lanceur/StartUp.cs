using DataAccess.Ado;
using DataAccess.Contexts;
using DataAccess.Contexts.Interfaces;
using DataAccess.Factory;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using ObjectsAffaire.Configuration;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Text;

namespace Lanceur
{
    internal static class StartUp
    {
        public static IConfigurationRoot Configuration { get; set; }
        public static ApplicationConfiguration ApplicationConfiguration { get; set; }
        

        public static void ConfigureServices(IServiceCollection services)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            Configuration = builder.Build();


            services.Configure<DataConfiguration>(Configuration.GetSection("Data"));

            services.AddScoped<ISqlFactory, SqlFactory>();
            services.AddScoped<IOperationMassive, OperationMassive>();
          // services.AddScoped<INorthContext, NorthContext>();

            ConfigureContexte(services);
        }

        private static void ConfigureContexte(IServiceCollection services)
        {

            services.AddDbContext<NorthContext>(
                x => x.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));
        }
    }
}
