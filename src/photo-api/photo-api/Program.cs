using System;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace photo_api
{
    public class Program
    {
        public static void Main(string[] args)
        {
           
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder
                        .UseKestrel(options =>
                        {
                            // listen for HTTP
                            options.Listen(IPAddress.Any, 8080);

                            // retrieve certificate from store
                            using (var store = new X509Store(StoreName.My, StoreLocation.LocalMachine))
                            {
                                store.Open(OpenFlags.ReadOnly);
                                var certs = store.Certificates.Find(X509FindType.FindBySubjectName, "photo-3d.eastus.cloudapp.azure.com", false);
                                if (certs.Count > 0)
                                {
                                    var certificate = certs[0];

                                    // listen for HTTPS
                                    options.Listen(IPAddress.Any, 443, listenOptions =>
                                    {
                                        listenOptions.UseHttps(certificate);
                                    });
                                }
                            }
                        })
                        .UseStartup<Startup>();

                });
    }
}
