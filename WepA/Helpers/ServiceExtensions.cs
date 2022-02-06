using System;
using System.Collections.Generic;
using System.Text;
using Mapster;
using MapsterMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Sieve.Services;
using WepA.Data;
using WepA.Data.Repositories;
using WepA.GraphQL;
using WepA.GraphQL.Types;
using WepA.Interfaces.Repositories;
using WepA.Interfaces.Services;
using WepA.Models.Dtos.Common;
using WepA.Models.Entities;
using WepA.Services;

namespace WepA.Helpers
{
	public static class ServiceExtensions
	{
		public static void AddAuthenticationExt(this IServiceCollection services, string secret)
		{
			var key = Encoding.ASCII.GetBytes(secret);
			var validLocations = new List<string>{
				"http://localhost:3000",
				"https://localhost:5001",
				"https://localhost:5000",
			};
			services.AddAuthentication(options =>
			{
				options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
				options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
			}).AddJwtBearer(options =>
			{
				options.RequireHttpsMetadata = true;
				options.SaveToken = true;
				options.TokenValidationParameters = new TokenValidationParameters
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
				services.AddIdentity<ApplicationUser, IdentityRole>(options =>
				{
					options.SignIn.RequireConfirmedEmail = false;
					options.SignIn.RequireConfirmedAccount = false;
					options.SignIn.RequireConfirmedPhoneNumber = false;
				})
				.AddEntityFrameworkStores<WepADbContext>()
				.AddTokenProvider<DataProtectorTokenProvider<ApplicationUser>>(TokenOptions.DefaultProvider);

				services.Configure<IdentityOptions>(options =>
				{
					options.Password.RequireNonAlphanumeric = false;
					options.Password.RequireUppercase = false;
					options.Password.RequireLowercase = false;
					options.Password.RequireDigit = false;
					options.Password.RequiredLength = 0;
					options.Password.RequiredUniqueChars = 0;
				});
			}
		}

		public static void AddMapsterExt(this IServiceCollection services)
		{
			var config = new TypeAdapterConfig();

			config.NewConfig<ApplicationUser, UserDetailsResponse>()
				  .Map(dest => dest.Id, src => EncryptHelpers.EncodeBase64Url(src.Id));
			config.NewConfig<UserDetailsResponse, UserDetails>()
				  .Map(dest => dest.DateOfBirth, src => src.DateOfBirthString);

			services.AddSingleton(config);
			services.AddScoped<IMapper, ServiceMapper>();
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