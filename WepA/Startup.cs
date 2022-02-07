using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using WepA.Helpers;
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
				app.UseSwaggerUI(_ =>
					_.SwaggerEndpoint("/swagger/v1/swagger.json", "WepA v1"));
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
			services.AddNpgsqlDbContext();

			services.AddIdentityExt(CurrentEnvironment.IsDevelopment());
			services.AddAuthenticationExt();
			services.AddDIContainerExt();
			services.AddOptionPatterns();
			services.AddMapsterExt();
			services.AddGraphQLExt();
			services.AddCorsExt();

			services.AddControllers().AddJsonOptions(options =>
				options.JsonSerializerOptions.WriteIndented = true);
			services.AddSwaggerExt();
		}
	}
}
