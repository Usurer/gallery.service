using Api.Services;
using Microsoft.EntityFrameworkCore;

namespace Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddDbContext<GalleryContext>(options =>
            {
                options.UseSqlite(builder.Configuration.GetConnectionString("sqlite"));
            });

            builder.Services.Configure<FileSystemOptions>(builder.Configuration.GetSection(FileSystemOptions.FileSystem));
            builder.Services.AddScoped<IFileSystemService, FileSystemService>();

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

            var app = builder.Build();

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

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapControllers();


            app.Run();
        }
    }
}