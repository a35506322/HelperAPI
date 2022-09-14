using HelperAPI.Helper;
using HelperAPI.Modules.Implements;
using HelperAPI.Modules.Interfaces;
using HelperAPI.Services.Implements;
using HelperAPI.Services.Interfaces;
using InsuranceAgents.Domain.Helpers.UITC;
using IronOcr;
using Microsoft.AspNetCore.Hosting;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace HelperAPI.Extensions
{
    public static class ModuleExtensions
    { 
        private static IEnumerable<IBaseModule> GetModules()
        {
            /*
                利用繼承介面方式去尋找出這些Module
                Activator.CreateInstance => 動態創造實例
                Cast => 轉換型態
             */
            var modules = typeof(IBaseModule).Assembly.GetTypes()
                           .Where(b => b.IsClass && b.IsAssignableTo(typeof(IBaseModule)))
                           .Select(Activator.CreateInstance)
                           .Cast<IBaseModule>();

            return modules;
        }

        public static void Routers(this IEndpointRouteBuilder builder)
        {
            foreach (var module in GetModules())
            {
                module.AddModuleRoutes(builder);
            }
        }

        public static void AddServices(this IServiceCollection services)
        {
            // 將Service結尾且生命週期相同的物件,統一註冊
            services.Scan(scan => scan
                    .FromAssemblyOf<Program>()     // 1.遍歷IService類別所在程序集中的所有類別
                    .AddClasses(classes =>          // 2.要自動註冊的類別,條件為logic結尾的類別
                        classes.Where(t => t.Name.EndsWith("Service", StringComparison.OrdinalIgnoreCase)))
                    .AsImplementedInterfaces()      // 3.註冊的類別有實作界面
                    .WithScopedLifetime()           // 4.生命週期設定為Scoped
            );
        }

        public static void AddRepositories(this IServiceCollection services)
        {

        }

        public static void AddGlobalConfig(this IServiceCollection services)
        {
            services.AddScoped<SecurityHelper>();
            services.AddScoped<ScanHelper>();
            services.AddScoped<CaptchaCrackedHelper>();
            services.AddScoped<IronTesseract>();
        }
    }
}
