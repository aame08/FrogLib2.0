using FrogLib.Server.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

internal class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddDbContext<Froglib2Context>(options =>
            options.UseMySql(
                builder.Configuration.GetConnectionString("DefaultConnection"),
                ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("DefaultConnection"))));

        //builder.Services.AddScoped<IBooksService, BooksService>();
        //builder.Services.AddScoped<ICollectionsService, CollectionsService>();
        //builder.Services.AddScoped<IReviewsService, ReviewsService>();
        //builder.Services.AddScoped(provider =>
        //    new Lazy<IBooksService>(provider.GetRequiredService<IBooksService>));

        //builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("JwtOptions"));
        //builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        //    .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
        //    {
        //        var jwtOptions = builder.Services.BuildServiceProvider().GetRequiredService<IOptions<JwtOptions>>().Value;

        //        options.TokenValidationParameters = new()
        //        {
        //            ValidateIssuer = false,
        //            ValidateAudience = false,
        //            ValidateLifetime = true,
        //            ValidateIssuerSigningKey = true,
        //            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SecretKey))
        //        };
        //    });
        //builder.Services.AddAuthorization();
        //builder.Services.AddSingleton<JwtProvider>();
        //builder.Services.AddSingleton<ContentModerationService>();

        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowVueApp",
                policy => policy.WithOrigins("http://localhost:5173")
                        .AllowAnyMethod()
                .AllowAnyHeader()
                        .AllowCredentials());
        });

        builder.Services.AddControllers();
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(c =>
        {
            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
            {
                Name = "Authorization",
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Введите токен следующим образом: Bearer {your token}",
            });
            c.AddSecurityRequirement(new OpenApiSecurityRequirement {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    new string[] {}
                }
            });
        });

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseStaticFiles();

        app.UseCors("AllowVueApp");

        app.UseHttpsRedirection();

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();

        app.Run();
    }
}