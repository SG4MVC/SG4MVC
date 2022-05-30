using System;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Sg4Mvc;

[assembly: GenerateSg4Mvc]

namespace AspNetFeatureFolders;

public class Program
{
    public static void Main(String[] args)
    {
        CreateWebHostBuilder(args).Build().Run();
    }

    public static IWebHostBuilder CreateWebHostBuilder(String[] args) =>
        WebHost.CreateDefaultBuilder(args)
            .UseStartup<Startup>();
}
