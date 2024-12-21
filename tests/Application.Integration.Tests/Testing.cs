using AcademyCleanTesting.Domain.Constants;
using AcademyCleanTesting.Infrastructure.Data;
using AcademyCleanTesting.Infrastructure.Identity;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Application.IntegrationTests;

[SetUpFixture]
public partial class Testing
{
    private static string? _userId;
    private static ITestDatabase _database = default!;
    private static CustomWebApplicationFactory _customWebApplicationFactory = default!;
    private static IServiceScopeFactory _serviceScopeFactory = default!;

    [OneTimeSetUp]
    public async Task RunBeforeAnyTest()
    {
        _database = await TestDatabaseFactory.CreateAsync();
        _customWebApplicationFactory = new CustomWebApplicationFactory(_database.GetConnection());
        _serviceScopeFactory = _customWebApplicationFactory.Services.GetRequiredService<IServiceScopeFactory>();
    }

    [OneTimeTearDown]
    public async Task RunAfterAnyTest()
    {
        await _database.DisposeAsync();
        await _customWebApplicationFactory.DisposeAsync();
    }

    public static async Task AddAsync<TEntity>(TEntity entity)
        where TEntity : class
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        dbContext.Add(entity);
        await dbContext.SaveChangesAsync();
    }

    public static async Task<int> CountAsync<TEntity>()
        where TEntity : class
    {
        using var scope = _serviceScopeFactory.CreateScope();
        using var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        return await dbContext.Set<TEntity>().CountAsync();
    }

    public static async Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        return await mediator.Send(request);
    }

    public static async Task SendAsync(IRequest request) // Or could be IBaseRequest
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        await mediator.Send(request);
    }

    public static string? GetUserId()
    {
        return _userId;
    }

    public static async Task<string> RunAsDefaultUserAsync()
    {
        return await RunAsUserAsync("test@local", "Testing1234!", Array.Empty<string>());
    }

    public static async Task<string> RunAsAdministratorAsync()
    {
        return await RunAsUserAsync("administrator@local", "Administrator1234!", new[] { Roles.Administrator });
    }

    public static async Task<string> RunAsUserAsync(string userName, string password, string[] roles)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        using var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        // create a new user
        var user = new ApplicationUser { UserName = userName, Email = userName };
        var result = await userManager.CreateAsync(user, password);

        if (roles.Length > 0)
        {
            using var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            foreach (var role in roles) 
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }

            await userManager.AddToRolesAsync(user, roles);
        }

        if (result.Succeeded)
        {
            _userId = user.Id;
            return _userId;
        }

        var errors = string.Join(Environment.NewLine, result.ToApplicationResult().Errors);
        throw new Exception($"Unable to create {userName}.{Environment.NewLine}{errors}");
    }

    public static async Task ResetState()
    {
        try
        {
            await _database.ResetAsync();
        }
        catch (Exception)
        {
            // Do nothing
        }

        _userId = null;
    }
}
