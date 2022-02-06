using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Sieve.Models;
using WepA.Data;
using WepA.Helpers;
using WepA.Helpers.Settings;
using WepA.Middlewares;

namespace WepA
{
	public class Startup
	{
		public Startup(IWebHostEnvironment currentEnvironment, IConfiguration configuration)
		{
			CurrentEnvironment = currentEnvironment;
			Configuration = configuration;
		}

		public IConfiguration Configuration { get; }
		private IWebHostEnvironment CurrentEnvironment { get; set; }

		// Middlewares — This method gets called by the runtime. Use this method to configure the
		// HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
				app.UseSwagger();
				app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "WepA v1"));
			}
			app.UseCors("DevelopmentPolicy");

			app.UseHttpsRedirection();
			app.UseRouting();

			app.UseAuthentication();
			app.UseAuthorization();

			app.UseHttpStatusExceptionHandlingExt();

			app.UseEndpoints(endpoints =>
			{
				endpoints.MapGraphQL();
				endpoints.MapControllers();
			});
			app.UseGraphQLVoyager("/graphql-voyager");
		}

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{
			services.AddDbContext<WepADbContext>(options =>
				options.UseSqlServer(Configuration.GetConnectionString("WepA")));

			services.AddIdentityExt(CurrentEnvironment.IsDevelopment());
			services.AddCorsExt();
			services.AddMapsterExt();
			services.AddDIContainerExt();
			services.AddGraphQLExt();

			services.Configure<SendGridSettings>(Configuration.GetSection("ExternalProviders:SendGrid"));
			services.Configure<JwtSettings>(Configuration.GetSection("ServiceSettings:Jwt"));
			services.Configure<SieveOptions>(Configuration.GetSection("ServiceSettings:Sieve"));

			services.AddControllers()
					.AddJsonOptions(options => options.JsonSerializerOptions.WriteIndented = true);
			services.AddSwaggerExt();
		}
	}
}
