using Api.Database;
using Api.Services;
using EasyCaching.Disk;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Json;
using System.Globalization;

namespace Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // Not needed, can as well just pass args to CreateBuilder, i'll leave it here just a reminder
            var options = new WebApplicationOptions
            {
                Args = args,
            };
            WebApplicationBuilder builder = WebApplication.CreateBuilder(options);

            AddLogging(builder);

            // Add services to the container.
            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(options =>
            {
                options.EnableAnnotations();
            });

            builder.Services.AddDbContext<GalleryContext>(options =>
            {
                options.UseSqlite(builder.Configuration.GetConnectionString("sqlite"));
                // TODO: Check if save works
                options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
            });

            IConfigurationSection fileSystemConfigSection = builder.Configuration.GetSection(FileSystemOptions.FileSystem);

            builder.Services.Configure<FileSystemOptions>(fileSystemConfigSection);
            builder.Services.AddScoped<IFileSystemService, FileSystemService>();
            builder.Services.AddScoped<IStorageService, DatabaseStorageService>();
            builder.Services.AddScoped<IScansStateService, ScansStateService>();

            builder.Services.AddCors(options =>
            {
                options.AddDefaultPolicy(policy =>
                {
                    policy.AllowAnyOrigin()
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                });
            });

            builder.Services.AddEasyCaching(options =>
            {
                options.WithMessagePack("disk");
                options.UseInMemory("in-memory");

                options.UseDisk(config =>
                {
                    config.DBConfig = new DiskDbOptions { BasePath = "C:\\Coding\\Meaningful Projects\\Gallery\\_cache" };
                }, "disk");
            });

            builder.Services.AddScoped<ImageResizeService>();

            //builder.Services.AddHostedService<ScheduledScanService>();

            builder.Services.AddSingleton<IScansProcessingService, ScansProcessingService>();

            WebApplication app = builder.Build();

            using (var scope = app.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<GalleryContext>();
                dbContext.Database.EnsureCreated();
            }

            app.UseCors();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }
            else
            {
                app.UseExceptionHandler();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }

        private static void AddLogging(WebApplicationBuilder builder)
        {
            builder.Logging.ClearProviders();
            builder.Host.UseSerilog((builderContext, serviceProvider, configuration) =>
            {
                configuration
                    .WriteTo.Console(
                        restrictedToMinimumLevel: LogEventLevel.Information,
                        formatProvider: CultureInfo.InvariantCulture)
                    .WriteTo.File(
                        restrictedToMinimumLevel: LogEventLevel.Warning,
                        formatter: new JsonFormatter(),
                        path: "./logs/log.txt",
                        rollingInterval: RollingInterval.Day
                    );
            });
        }
    }
}