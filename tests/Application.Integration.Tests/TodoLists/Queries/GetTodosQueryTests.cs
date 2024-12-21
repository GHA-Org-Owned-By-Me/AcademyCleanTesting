using AcademyCleanTesting.Application.TodoLists.Queries.GetTodos;
using AcademyCleanTesting.Domain.Entities;
using FluentAssertions;
using static Application.IntegrationTests.Testing;

namespace Application.IntegrationTests.TodoLists.Queries;

public class GetTodosQueryTests : BaseTestFixture
{
    [Test]
    public async Task Handle_ShouldReturnListsAndAssociatedItems_WhenRequestIsValid()
    {
        // Arrange
        await RunAsDefaultUserAsync();

        await AddAsync(new TodoList
        {
            Title = "Shopping",
            Items = 
            {
                new TodoItem { Title = "Fresh Fruit", Done = true },
                new TodoItem { Title = "Bread", Done = true },
                new TodoItem { Title = "Milk", Done = true },
                new TodoItem { Title = "Toilet Paper" },
                new TodoItem { Title = "Tuna" },
                new TodoItem { Title = "Pasta" }
            }
        });
        var query = new GetTodosQuery();

        // Act
        TodosVm result = await SendAsync(query);

        // Assert
        result.Should().NotBeNull();
        result.Lists.Should().HaveCount(1);
        result.Lists.First().Items.Should().HaveCount(6);
    }

    [Test]
    public async Task Handle_ShouldThrowUnauthorizedAccessException_WhenRequestorIsAnonymous()
    {
        // Arrange
        await AddAsync(new TodoList
        {
            Title = "Shopping",
            Items =
            {
                new TodoItem { Title = "Fresh Fruit", Done = true },
                new TodoItem { Title = "Bread", Done = true },
                new TodoItem { Title = "Milk", Done = true },
                new TodoItem { Title = "Toilet Paper" },
                new TodoItem { Title = "Tuna" },
                new TodoItem { Title = "Pasta" }
            }
        });
        var query = new GetTodosQuery();

        // Act
        var action = () => SendAsync(query);

        // Assert
        await action.Should().ThrowAsync<UnauthorizedAccessException>();
    }
}
