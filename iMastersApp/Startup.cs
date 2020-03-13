using System.IdentityModel.Tokens.Jwt;
using IdentityServer4;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace iMastersApp
{
    public class Startup
    {
        private readonly IConfiguration _configuration;

        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        private static void ConfigureIdentityServerOnClient(IServiceCollection services)
        {
            //Exibe as claims de maneira mais "amigável"
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

            //Adciona o serviço de autenticação
            services
                .AddAuthentication(options =>
                {
                    //Nosso esquema default será baseado em cookie
                    options.DefaultScheme = "Cookies";
                    //Como precisamos recuperar os dados depois do login, utilizamos o OpenID Connect que por padrão utiliza o escopo do Profile
                    options.DefaultChallengeScheme = "oidc";
                })
                .AddCookie("Cookies")
                .AddOpenIdConnect("oidc", options =>
                {
                    options.SignInScheme = "Cookies";

                    //Aponta para o nosso servidor de autenticação
                    options.Authority = "http://localhost:5000";
                    options.RequireHttpsMetadata = false;

                    //Nome da nossa aplicação que tentará se autenticar no nosso servidor de identidade
                    //Observe que ela possui o mesmo nome da app que liberamos no nosso servidor de identidade
                    options.ClientId = "iMastersApp";
                    options.SaveTokens = true;

                    //Adicionamos o scopo do e-mail para utilizarmos a claim de e-mail.
                    options.Scope.Add(IdentityServerConstants.StandardScopes.Email);

                    options.Scope.Add("custom.profile");
                });
        }
        
        #region Called for ASPNETCORE_ENVIRONMENT=Development
        public void ConfigureDevelopmentServices(IServiceCollection services)
        {
            services
                .AddControllersWithViews()
                .AddRazorRuntimeCompilation()
                .AddNewtonsoftJson(options =>
                {
                    options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
                    options.SerializerSettings.Formatting = Formatting.Indented;
                });

            ConfigureIdentityServerOnClient(services);
        }

        public void ConfigureDevelopment(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseDeveloperExceptionPage();
            app.UseAuthentication();
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    "default", "{controller=Home}/{action=Index}/{id?}"
                );
            });
        }
        #endregion

        #region Called for ASPNETCORE_ENVIRONMENT=Production
        public void ConfigureProductionServices(IServiceCollection services)
        {
            services
                .AddControllersWithViews()
                .AddRazorRuntimeCompilation()
                .AddNewtonsoftJson(options =>
                {
                    options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
                });

            ConfigureIdentityServerOnClient(services);
        }

        public void ConfigureProduction(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseExceptionHandler("/Home/Error");
            app.UseAuthentication();
            app.UseHsts();
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    "default", "{controller=Home}/{action=Index}/{id?}"
                );
            });
        }
        #endregion
    }
}
