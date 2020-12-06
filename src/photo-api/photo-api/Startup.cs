using System;
using System.Text;
using Audit.Core;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using photo_api.Helpers;

namespace photo_api
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            //ImageHelper.ResizeImage(@"D:\out_1600_90.jpg", @"D:\out_1600_90.jpg", 400, 400, 50);
            //ImageHelper.ResizeImage(@"D:\FOTOS\2020\11-Noviembre\IMG_20201030_164902.jpg", @"D:\out_1600_10.jpg", 1600, 1600, 10);
            //ImageHelper.ResizeImage(@"D:\FOTOS\2020\11-Noviembre\IMG_20201030_164902.jpg", @"D:\out_800_80.jpg", 800, 800, 80);
            //ImageHelper.ResizeImage(@"D:\FOTOS\2020\11-Noviembre\IMG_20201030_164902.jpg", @"D:\out_800_70.jpg", 800, 800, 70);

            Configuration = configuration;
        }

        public static IConfiguration Configuration { get; private set; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors();
            services.AddControllers().AddNewtonsoftJson(options => {
                options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            ConfigureAuditNet();
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseCors(options => options.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

        }

        public static void EphemeralLog(string text, bool important = false)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return;
            }
            Console.WriteLine(text);
            Audit.Core.AuditScope.CreateAndSave("Ephemeral", new { Status = text });
        }

        private void ConfigureAuditNet()
        {
            Audit.Core.Configuration.Setup()
                .UseUdp(_ => _
                    .RemoteAddress("127.0.0.1")
                    .RemotePort(2224)
                    .CustomSerializer(ev =>
                    {
                        if (ev.EventType == "Ephemeral")
                        {
                            return Encoding.UTF8.GetBytes(ev.CustomFields["Status"] as string);
                        }
                        else
                        {
                            var action = (ev as Audit.WebApi.AuditEventWebApi)?.Action;
                            var msg = $"Action: {action.ControllerName}/{action.ActionName}{new Uri(action.RequestUrl).Query} - Duration: {ev.Duration} ms. - Response: {action.ResponseStatusCode} {action.ResponseStatus}: {System.Text.Json.JsonSerializer.Serialize(action.ResponseBody?.Value)}. Event: {action.ToJson()}";
                            return Encoding.UTF8.GetBytes(msg);
                        }
                    }));

            EphemeralLog($"photo-api started at {DateTime.Now}", true);
        }

    }
}
