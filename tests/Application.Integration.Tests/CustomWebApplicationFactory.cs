using System.Data.Common;
using AcademyCleanTesting.Application.Common.Interfaces;
using AcademyCleanTesting.Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;

namespace Application.IntegrationTests;
using static Testing;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly DbConnection _connection;

    public CustomWebApplicationFactory(DbConnection connection)
    {
        _connection = connection;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<IUser>()
            .TryAddTransient(provider => Mock.Of<IUser>(s => s.Id == GetUserId()));

            services.RemoveAll<DbContextOptions<ApplicationDbContext>>()
            .AddDbContext<ApplicationDbContext>((sp, options) =>
            {
                options.AddInterceptors(sp.GetRequiredService<ISaveChangesInterceptor>());
                options.UseSqlServer(_connection);
            });
        });
    }
}
