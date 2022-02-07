using System;
using System.Collections.Generic;
using System.Text;
using Mapster;
using MapsterMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Sieve.Models;
using Sieve.Services;
using WepA.Data;
using WepA.Data.Repositories;
using WepA.GraphQL;
using WepA.GraphQL.Types;
using WepA.Helpers.Settings;
using WepA.Interfaces.Repositories;
using WepA.Interfaces.Services;
using WepA.Models.Dtos.Common;
using WepA.Models.Entities;
using WepA.Services;

namespace WepA.Helpers
{
	public static class ServiceExtensions
	{
		public static void AddAuthenticationExt(this IServiceCollection services)
		{
			var key = Encoding.ASCII.GetBytes(Environment.GetEnvironmentVariable("JWT_SECRET"));
			var validLocations = Environment.GetEnvironmentVariable("AUTH_VALID_LOCATIONS")
											.Split(",");
			services.AddAuthentication(_ =>
			{
				_.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
				_.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
			}).AddJwtBearer(_ =>
			{
				_.RequireHttpsMetadata = true;
				_.SaveToken = true;
				_.TokenValidationParameters = new TokenValidationParameters
				{
					ValidateIssuerSigningKey = true,
					ValidateIssuer = true,
					ValidateAudience = true,
					ValidateLifetime = true,

					ValidIssuers = validLocations,
					ValidAudiences = validLocations,
					IssuerSigningKey = new SymmetricSecurityKey(key),
					ClockSkew = TimeSpan.Zero
				};
			});
		}

		public static void AddCorsExt(this IServiceCollection services)
		{
			services.AddCors(o => o.AddPolicy("DevelopmentPolicy", builder =>
				builder.AllowAnyMethod()
					.AllowAnyHeader()
					.WithOrigins(
						"http://localhost:3000",
						"https://localhost:5000",
						"https://localhost:5001")));
		}

		public static void AddDIContainerExt(this IServiceCollection services)
		{
			services.AddScoped<SieveProcessor>();
			services.AddScoped<IUserService, UserService>();
			services.AddScoped<IAccountService, AccountService>();
			services.AddScoped<IEmailService, EmailService>();
			services.AddScoped<IJwtService, JwtService>();
			services.AddScoped<IUserRepository, UserRepository>();
		}

		public static void AddGraphQLExt(this IServiceCollection services)
		{
			services.AddErrorFilter<GraphQLErrorFilter>();
			services.AddGraphQLServer()
					.AddQueryType<Query>()
					.AddMutationType<Mutation>()
					.AddAuthorization()
					.AddProjections();
		}

		public static void AddIdentityExt(this IServiceCollection services, bool isDevelopment)
		{
			if (isDevelopment)
			{
				services.AddIdentity<ApplicationUser, IdentityRole>(_ =>
				{
					_.SignIn.RequireConfirmedEmail = false;
					_.SignIn.RequireConfirmedAccount = false;
					_.SignIn.RequireConfirmedPhoneNumber = false;
				})
				.AddEntityFrameworkStores<WepADbContext>()
				.AddTokenProvider<DataProtectorTokenProvider<ApplicationUser>>(TokenOptions.DefaultProvider);

				services.Configure<IdentityOptions>(_ =>
				{
					_.Password.RequireNonAlphanumeric = false;
					_.Password.RequireUppercase = false;
					_.Password.RequireLowercase = false;
					_.Password.RequireDigit = false;
					_.Password.RequiredLength = 0;
					_.Password.RequiredUniqueChars = 0;
				});
			}
		}

		public static void AddMapsterExt(this IServiceCollection services)
		{
			var config = new TypeAdapterConfig();

			config.NewConfig<ApplicationUser, UserDetailsResponse>()
				  .Map(dest => dest.Id, src => EncryptHelpers.EncodeBase64Url(src.Id));
			config.NewConfig<UserDetailsResponse, ApplicationUser>()
				  .Map(dest => dest.Id, src => EncryptHelpers.DecodeBase64Url(src.Id));
			config.NewConfig<UserDetailsResponse, UserDetails>()
				  .Map(dest => dest.DateOfBirth, src => src.DateOfBirthString);

			services.AddSingleton(config);
			services.AddScoped<IMapper, ServiceMapper>();
		}

		public static void AddOptionPatterns(this IServiceCollection services)
		{
			services.Configure<SendGridSettings>(_ =>
			{
				_.ApiKey = Environment.GetEnvironmentVariable("SENDGRID_API_KEY");
				_.SenderEmail = Environment.GetEnvironmentVariable("SENDGRID_SENDER_EMAIL");
				_.SenderName = Environment.GetEnvironmentVariable("SENDGRID_SENDER_NAME");
				_.TemplateId = Environment.GetEnvironmentVariable("SENDGRID_TEMPLATE_ID");
			});

			services.Configure<JwtSettings>(_ =>
			{
				_.Secret = Environment.GetEnvironmentVariable("JWT_SECRET");
				_.Issuer = Environment.GetEnvironmentVariable("JWT_ISSUSER");
				_.Audience = Environment.GetEnvironmentVariable("JWT_AUDIENCE");
				_.RefreshTokenExpiredDate = int.Parse(Environment.GetEnvironmentVariable("JWT_REFRESH_TOKEN_EXPIRED_DATE"));
				_.AccessTokenExpiredDate = int.Parse(Environment.GetEnvironmentVariable("JWT_ACCESS_TOKEN_EXPIRED_DATE"));
			});

			services.Configure<SieveOptions>(_ =>
			{
				_.CaseSensitive = bool.Parse(Environment.GetEnvironmentVariable("SIEVE_CASE_SENSITIVE"));
				_.ThrowExceptions = bool.Parse(Environment.GetEnvironmentVariable("SIEVE_THROW_EXCEPTIONS"));
				_.IgnoreNullsOnNotEqual = bool.Parse(Environment.GetEnvironmentVariable("SIEVE_IGNORE_NULLS_ON_NOT_EQUAL"));
				_.DefaultPageSize = int.Parse(Environment.GetEnvironmentVariable("SIEVE_DEFAULT_PAGE_SIZE"));
				_.MaxPageSize = int.Parse(Environment.GetEnvironmentVariable("SIEVE_MAX_PAGE_SIZE"));
			});
		}

		public static void AddNpgsqlDbContext(this IServiceCollection services)
		{
			services.AddDbContext<WepADbContext>(option =>
			{
				option.UseNpgsql($@"
					Server={Environment.GetEnvironmentVariable("POSTGRES_HOST")};
					Port={Environment.GetEnvironmentVariable("POSTGRES_PORT")};
					User Id={Environment.GetEnvironmentVariable("POSTGRES_USER_ID")};
					Password={Environment.GetEnvironmentVariable("POSTGRES_PASSWORD")};
					Database={Environment.GetEnvironmentVariable("POSTGRES_DB")};
					sslmode={Environment.GetEnvironmentVariable("POSTGRES_SSL_MODE")};
					Trust Server Certificate={Environment.GetEnvironmentVariable("POSTGRES_TRUST_SERVER_CERTIFICATE")};
					Integrated Security={Environment.GetEnvironmentVariable("POSTGRES_INTEGRATED_SECURITY")};
					Pooling={Environment.GetEnvironmentVariable("POSTGRES_POOLING")};");
			});
		}

		public static void AddSwaggerExt(this IServiceCollection services)
		{
			services.AddSwaggerGen(c =>
			{
				c.SwaggerDoc("v1", new OpenApiInfo { Title = "WepA", Version = "v1" });
				c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
				{
					Description = "JWT Authorization header using the Bearer scheme." +
								  "\nEnter 'Bearer' [space] and then your token in the text input below." +
								  "\nExample: 'Bearer 12345abcdef'",
					Name = "Authorization",
					In = ParameterLocation.Header,
					Type = SecuritySchemeType.ApiKey,
					Scheme = "Bearer"
				});
				c.AddSecurityRequirement(new OpenApiSecurityRequirement()
				{
					{
						new OpenApiSecurityScheme
						{
							Reference = new OpenApiReference
							{
								Type = ReferenceType.SecurityScheme,
								Id = "Bearer"
							},
							Scheme = "oauth2",
							Name = "Bearer",
							In = ParameterLocation.Header,
						},
						new List<string>()
					}
				});
			});
		}
	}
}