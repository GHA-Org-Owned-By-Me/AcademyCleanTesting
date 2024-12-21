namespace Application.IntegrationTests.TodoLists.Commands;

using AcademyCleanTesting.Application.Common.Exceptions;
using AcademyCleanTesting.Application.TodoLists.Commands.PurgeTodoLists;
using AcademyCleanTesting.Domain.Entities;
using FluentAssertions;
using static Testing;

public class PurgeTodoListsTests : BaseTestFixture
{
    [Test]
    public async Task Handle_ShouldThrow_WhenRequestorIsNotAdmin()
    {
        // Arrange
        await RunAsDefaultUserAsync();
        var command = new PurgeTodoListsCommand();

        // Act
        var action = () => SendAsync(command);

        // Assert
        await action.Should().ThrowAsync<ForbiddenAccessException>();
    }

    [Test]
    public async Task Handle_ShouldThrowUnauthorizedAccessException_WhenRequestorIsAnonymous()
    {
        // Arrange
        var command = new PurgeTodoListsCommand();

        // Act
        var action = () => SendAsync(command);

        // Assert
        await action.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Test]
    public async Task Handle_ShouldRemoveAllTodoLists_WhenRequestorIsAdmin()
    {
        // Arrange
        await RunAsAdministratorAsync();

        await AddAsync(new TodoList { Title = "1st List" });
        await AddAsync(new TodoList { Title = "2nd List" });
        await AddAsync(new TodoList { Title = "3rd List" });

        var command = new PurgeTodoListsCommand();

        // Act
        await SendAsync(command);
        var recordCount = await CountAsync<TodoList>();

        // Assert
        recordCount.Should().Be(0);
    }
}
