using API.Data;
using API.Helpers;
using API.Interfaces;
using API.Services;
using API.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.Extensions
{
    public static class ApplicationServiceExtensions
    {
        public static IServiceCollection AddAplicationServices(this IServiceCollection services, IConfiguration config) 
        {
            services.AddSingleton<PresenceTracker>();
            services.Configure<ClaudinarySettings>(config.GetSection("CloudinarySettings"));
            services.AddScoped<ITokenService, TokenService>();            
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<LogUserActivity>();
            services.AddScoped<IPhotoService,PhotoService>();
            services.AddAutoMapper(typeof(AutoMapperProfiles).Assembly);
            services.AddDbContext<DataContext>(options =>
            {
                options.UseSqlServer(config.GetConnectionString("DefaultConnection"));
            });
            return services;
        }
    }
}
