using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace website_ci
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.ConfigureKestrel(serverOptions =>
                    {
                    })
                    .UseStartup<Startup>()
                    .UseUrls("http://127.0.0.1:8889");
                })
                .Build();
            
            host.Run();
        }
    }
}
