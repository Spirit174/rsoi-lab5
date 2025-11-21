using Booking.System.PaymentService.Core.Interfaces;
using Booking.System.PaymentService.DataBase.Repositories;
using Booking.System.PaymentService.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

namespace Booking.System.PaymentService;

public class Startup
{
    public IConfiguration Configuration { get; }

    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddControllers().AddNewtonsoftJson();
        
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Booking System",
                Version = "v1",
            });
            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                In = ParameterLocation.Header,
                Description = "Enter JWT token",
                Name = "Authorization",
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer"
            });
            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
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
        
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.Authority = "http://keycloak:8080/realms/booking";
                options.Audience = "booking-client";
                options.RequireHttpsMetadata = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = "http://keycloak:8080/realms/booking"
                };
            });

        services.AddAuthorization();

        services.AddSwaggerGenNewtonsoftSupport();
        
        services.AddDbContext(Configuration);
        
        services.AddScoped<IPaymentRepository, PaymentRepository>();
        services.AddScoped<IPaymentService, Core.Services.PaymentService>();
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseSwagger(c =>
        {
            c.RouteTemplate = "/api/v1/swagger/{documentName}/swagger.json";
        });
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/api/v1/swagger/v1/swagger.json", "Person.Server.Http v1");
            c.RoutePrefix = "api/v1/swagger";
        });
        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
            endpoints.MapGet("/manage/health", () => Results.Ok(new { status = "Healthy", service = "payment" }));
        });
    }
}