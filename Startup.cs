﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.HttpOverrides;

namespace OidcSampleApp
{
    public class Startup
    {
        // These variables are only here for visibility in the sample
        // In production you should move this to a configuration file
        const string ONELOGIN_OPENID_CONNECT_CLIENT_ID = "your-onelogin-openid-connect-client-id";
        const string ONELOGIN_OPENID_CONNECT_CLIENT_SECRET = "your-onelogin-openid-connect-client-secret";

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();

            // Allow sign in via an OpenId Connect provider like OneLogin
            services.AddAuthentication(options => {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
            })
            .AddCookie(options => {
                options.LoginPath = "/Account/Login/";
            })
            .AddOpenIdConnect(options =>
                {
                    options.ClientId = ONELOGIN_OPENID_CONNECT_CLIENT_ID;
                    options.ClientSecret = ONELOGIN_OPENID_CONNECT_CLIENT_SECRET;

                    // For EU Authority use: "https://openid-connect-eu.onelogin.com/oidc";
                    options.Authority = "https://openid-connect.onelogin.com/oidc";

                    options.ResponseType = "code";
                    options.GetClaimsFromUserInfoEndpoint = true;
                }
            );
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();

            app.UseAuthentication();

            // This is needed if running behind a reverse proxy
            // like ngrok which is great for testing while developing
            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                RequireHeaderSymmetry = false,
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            });

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
