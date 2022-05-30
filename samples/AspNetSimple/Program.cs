using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Sg4Mvc;

[assembly: GenerateSg4Mvc]

namespace AspNetSimple;

public class Program
{
    public static void Main(String[] args)
    {
        CreateHostBuilder(args).Build().Run();
    }

    public static IHostBuilder CreateHostBuilder(String[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(b => b
                .UseStartup<Startup>());
}
