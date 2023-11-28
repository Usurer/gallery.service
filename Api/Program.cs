using Api.Database;
using Api.Services;
using EasyCaching.Disk;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

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

            builder.Services.AddCors(options =>
            {
                options.AddDefaultPolicy(p =>
                {
                    p
                        .AllowAnyOrigin()
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                });
            });

            builder.Services.AddEasyCaching(options =>
            {
                //options.WithProtobuf("disk");
                options.WithMessagePack("disk");

                options.UseInMemory("in-memory");

                //use disk cache
                options.UseDisk(config =>
                {
                    config.DBConfig = new DiskDbOptions { BasePath = "C:\\Coding\\Meaningful Projects\\Gallery\\_cache" };
                }, "disk");
            });

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
    }
}