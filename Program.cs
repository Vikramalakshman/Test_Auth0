using System.Security.Claims; //👈 new code
using Microsoft.OpenApi.Models; //👈 new code
using Microsoft.IdentityModel.Tokens; //👈 new code
using Microsoft.AspNetCore.Authentication.JwtBearer; //👈 new code

namespace Test_Auth0
{
    public class Program
    {
        public static void Main(string[] args)
        {
            const string corsPolicy = "MyCorsPolicy";

            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllers();

            builder.Services.AddCors(setupActions =>
            {
                setupActions.AddPolicy(corsPolicy, policy =>
                {
                    // policy.AllowAnyOrigin()
                    policy.WithOrigins(new string[]
                           {
                                "http://localhost:4200/"
                           })
                           .AllowAnyHeader()
                           .AllowAnyMethod()
                           .AllowCredentials(); 
                    //The CORS protocol does not allow specifying a wildcard (any) origin and credentials at the same time.
                    //Configure the CORS policy by listing individual origins if credentials needs to be supported.
                });
            });

            //👇 new code
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                options.Authority =  $"https://{builder.Configuration["Auth0:Domain"]}/";
                options.Audience = builder.Configuration["Auth0:Audience"];
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    NameClaimType = ClaimTypes.NameIdentifier
                };
            });
            //👆 new code

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                //👇 new code
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Test Auth0", Version = "v1.0.0" });
                
                var securitySchema = new OpenApiSecurityScheme
                {
                    Description = "Using the Authorization header with the Bearer scheme.",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                };

                c.AddSecurityDefinition("Bearer", securitySchema);

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    { securitySchema, new[] { "Bearer" } }
                });
                //👆 new code
            });

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseCors(corsPolicy);

            app.UseAuthentication();  //👈 new code

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}