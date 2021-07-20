using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RapidBots;
using RapidBots.Extensions;
using BirthdayBot.Extensions;
using BirthdayBot.BLL;

namespace BirthdayBot
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IWebHostEnvironment environment)
        {
            this.Configuration = configuration;
            this.Environment = environment;
        }

        public IWebHostEnvironment Environment { get; }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            AssemblyBLL.LoadAssembly(); // .Net will load BLL project in assemblies only if there is a connection to it. BLL assembly is important for reflection to commands etc
            var rapidBotsOptions = new RapidBotsOptions();
            this.Configuration.GetSection(nameof(RapidBotsOptions)).Bind(rapidBotsOptions);

            string connectionString;
            if (this.Environment.IsDevelopment())
            {
                connectionString = this.Configuration.GetConnectionString("DefaultConnection");
            }
            else
            {
                connectionString = this.Configuration.GetConnectionString("DefaultConnectionProd");
            }
            services.AddDataAccess(connectionString);
            services.AddControllers().AddNewtonsoftJson();
            services.AddRapidBots(rapidBotsOptions);
            services.AddLocalizationSettings();
            services.AddAutoMapper(); // Should be the last
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
