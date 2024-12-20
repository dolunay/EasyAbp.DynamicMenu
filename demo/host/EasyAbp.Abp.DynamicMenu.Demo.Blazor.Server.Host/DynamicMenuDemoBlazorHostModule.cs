﻿using System;
using System.IO;
using System.Net.Http;
using Blazorise.Bootstrap5;
using Blazorise.Icons.FontAwesome;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Volo.Abp;
using Volo.Abp.AspNetCore.Authentication.JwtBearer;
using Volo.Abp.AspNetCore.Components.Server.BasicTheme;
using Volo.Abp.AspNetCore.Components.Server.BasicTheme.Bundling;
using Volo.Abp.AspNetCore.Components.Web.Theming.Routing;
using Volo.Abp.AspNetCore.Mvc.Localization;
using Volo.Abp.AspNetCore.Mvc.UI.Bundling;
using Volo.Abp.AspNetCore.Serilog;
using Volo.Abp.Autofac;
using Volo.Abp.Identity.Blazor.Server;
using Volo.Abp.Modularity;
using Volo.Abp.MultiTenancy;
using Volo.Abp.SettingManagement.Blazor.Server;
using Volo.Abp.Swashbuckle;
using Volo.Abp.TenantManagement.Blazor.Server;
using Volo.Abp.UI.Navigation;
using Volo.Abp.VirtualFileSystem;
using EasyAbp.Abp.DynamicMenu.Demo.Blazor.Server.Host.Menus;
using EasyAbp.Abp.DynamicMenu.Demo.Localization;
using EasyAbp.Abp.DynamicMenu.Demo.MultiTenancy;
using EasyAbp.Abp.DynamicMenu.EntityFrameworkCore;
using Volo.Abp.AspNetCore.Mvc.UI.Theme.LeptonXLite;
using Volo.Abp.AspNetCore.Mvc.UI.Theme.LeptonXLite.Bundling;
using Volo.Abp.Account.Web;

namespace EasyAbp.Abp.DynamicMenu.Demo.Blazor.Server.Host
{
    [DependsOn(typeof(AbpAspNetCoreMvcUiLeptonXLiteThemeModule))]
    [DependsOn(typeof(AbpAspNetCoreComponentsServerBasicThemeModule))]

    [DependsOn(typeof(AbpAutofacModule))]
    [DependsOn(typeof(AbpSwashbuckleModule))]
    [DependsOn(typeof(AbpAspNetCoreAuthenticationJwtBearerModule))]
    [DependsOn(typeof(AbpAspNetCoreSerilogModule))]
    [DependsOn(typeof(AbpIdentityBlazorServerModule))]
    [DependsOn(typeof(AbpTenantManagementBlazorServerModule))]
    [DependsOn(typeof(AbpSettingManagementBlazorServerModule))]

    [DependsOn(typeof(DemoHttpApiModule))]
    [DependsOn(typeof(DemoEntityFrameworkCoreModule))]
    [DependsOn(typeof(DemoApplicationModule))]

    //[DependsOn(typeof(AbpIdentityWebModule))]
    [DependsOn(typeof(AbpAccountWebModule))]
    [DependsOn(typeof(AbpAccountWebIdentityServerModule))]

    [DependsOn(typeof(DemoBlazorServerModule))]
    //[DependsOn(typeof(DemoWebModule))]
    public class DynamicMenuDemoBlazorHostModule : AbpModule
    {
        public override void PreConfigureServices(ServiceConfigurationContext context)
        {
            context.Services.PreConfigure<AbpMvcDataAnnotationsLocalizationOptions>(options =>
            {
                options.AddAssemblyResource(
                    typeof(DemoResource),
                    typeof(DemoDomainModule).Assembly,
                    typeof(DemoDomainSharedModule).Assembly,
                    typeof(DemoApplicationModule).Assembly,
                    typeof(DemoApplicationContractsModule).Assembly,
                    typeof(DynamicMenuDemoBlazorHostModule).Assembly
                );
            });
        }

        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            var hostingEnvironment = context.Services.GetHostingEnvironment();
            var configuration = context.Services.GetConfiguration();

            Configure<AbpBundlingOptions>(options =>
            {
                // MVC UI
                options.StyleBundles.Configure(
                    LeptonXLiteThemeBundles.Styles.Global,
                    bundle =>
                    {
                        bundle.AddFiles("/global-styles.css");
                    }
                );

                //BLAZOR UI
                options.StyleBundles.Configure(
                    BlazorBasicThemeBundles.Styles.Global,
                    bundle =>
                    {
                        bundle.AddFiles("/blazor-global-styles.css");
                        //You can remove the following line if you don't use Blazor CSS isolation for components
                        bundle.AddFiles("/EasyAbp.Abp.DynamicMenu.Demo.Blazor.Server.Host.styles.css");
                    }
                );
            });

            context.Services.AddAuthentication()
                .AddJwtBearer(options =>
                {
                    options.Authority = configuration["AuthServer:Authority"];
                    options.RequireHttpsMetadata = Convert.ToBoolean(configuration["AuthServer:RequireHttpsMetadata"]);
                    options.Audience = "DynamicMenu";
                });

            if (hostingEnvironment.IsDevelopment())
            {
                Configure<AbpVirtualFileSystemOptions>(options =>
                {
                    options.FileSets.ReplaceEmbeddedByPhysical<DemoDomainSharedModule>(Path.Combine(hostingEnvironment.ContentRootPath, string.Format("..{0}..{0}src{0}EasyAbp.Abp.DynamicMenu.Demo.Domain.Shared", Path.DirectorySeparatorChar)));
                    options.FileSets.ReplaceEmbeddedByPhysical<DemoDomainModule>(Path.Combine(hostingEnvironment.ContentRootPath, string.Format("..{0}..{0}src{0}EasyAbp.Abp.DynamicMenu.Demo.Domain", Path.DirectorySeparatorChar)));
                    options.FileSets.ReplaceEmbeddedByPhysical<DemoApplicationContractsModule>(Path.Combine(hostingEnvironment.ContentRootPath, string.Format("..{0}..{0}src{0}EasyAbp.Abp.DynamicMenu.Demo.Application.Contracts", Path.DirectorySeparatorChar)));
                    options.FileSets.ReplaceEmbeddedByPhysical<DemoApplicationModule>(Path.Combine(hostingEnvironment.ContentRootPath, string.Format("..{0}..{0}src{0}EasyAbp.Abp.DynamicMenu.Demo.Application", Path.DirectorySeparatorChar)));
                    options.FileSets.ReplaceEmbeddedByPhysical<DynamicMenuDemoBlazorHostModule>(hostingEnvironment.ContentRootPath);
                });
            }

            context.Services.AddAbpSwaggerGen(
                options =>
                {
                    options.SwaggerDoc("v1", new OpenApiInfo { Title = "DynamicMenu API", Version = "v1" });
                    options.DocInclusionPredicate((docName, description) => true);
                    options.CustomSchemaIds(type => type.FullName);
                });

            Configure<AbpMultiTenancyOptions>(options =>
            {
                options.IsEnabled = MultiTenancyConsts.IsEnabled;
            });

            context.Services.AddTransient(sp => new HttpClient
            {
                BaseAddress = new Uri("/")
            });

            context.Services
                .AddBootstrap5Providers()
                .AddFontAwesomeIcons();

            Configure<AbpNavigationOptions>(options =>
            {
                options.MenuContributors.Add(new DynamicMenuDemoMenuContributor());
            });

            Configure<AbpRouterOptions>(options =>
            {
                options.AppAssembly = typeof(DynamicMenuDemoBlazorHostModule).Assembly;
            });
        }

        public override void OnApplicationInitialization(ApplicationInitializationContext context)
        {
            var env = context.GetEnvironment();
            var app = context.GetApplicationBuilder();

            app.UseAbpRequestLocalization();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseCorrelationId();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthentication();
            app.UseJwtTokenMiddleware();

            if (MultiTenancyConsts.IsEnabled)
            {
                app.UseMultiTenancy();
            }

            app.UseUnitOfWork();
            app.UseIdentityServer();
            app.UseAuthorization();
            app.UseSwagger();
            app.UseAbpSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "DynamicMenu API");
            });
            app.UseConfiguredEndpoints();
        }
    }
}
