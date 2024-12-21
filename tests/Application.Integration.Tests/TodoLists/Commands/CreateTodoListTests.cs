using AcademyCleanTesting.Application.Common.Exceptions;
using AcademyCleanTesting.Application.TodoLists.Commands.CreateTodoList;
using AcademyCleanTesting.Domain.Entities;
using FluentAssertions;

namespace Application.IntegrationTests.TodoLists.Commands;

using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using static Testing;

public class CreateTodoListTests : BaseTestFixture
{
    [Test]
    public async Task Handle_ShoulCreateListSuccessfully_WhenRequestorIsAnonymous()
    {
        // Arrange
        var command = new CreateTodoListCommand { Title = "Part of KL famed golden triangle" };

        // Act
        var entityId = await SendAsync(command);
        var listCount = await CountAsync<TodoList>();

        // Assert
        listCount.Should().Be(1);
        entityId.Should().BeGreaterThan(0);
    }

    [Test]
    public async Task Handle_ShoulThrowValidationException_WhenTitleIsEmpty()
    {
        // Arrange
        var command = new CreateTodoListCommand { Title = string.Empty };

        // Act + Assert
        await FluentActions.Invoking(() => SendAsync(command))
            .Should().ThrowAsync<ValidationException>();
    }

    [Test]
    public async Task Handle_ShoulThrowValidationException_WhenTitleLengthIsOver200()
    {
        // Arrange
        var command = new CreateTodoListCommand { Title = "rjuogfchrbehorrnmkuicuyczhdwmozqrjuogfchrbehorrnmkuicuyczhdwmozqrjuogfchrbehorrnmkuicuyczhdwmozqrjuogfchrbehorrnmkuicuyczhdwmozqrjuogfchrbehorrnmkuicuyczhdwmozqrjuogfchrbehorrnmkuicuyczhdwmozqrjuogfchrbehorrnmkuicuyczhdwmozqrjuogfchrbehorrnmkuicuyczhdwmozq" };

        // Act + Assert
        await FluentActions.Invoking(() => SendAsync(command))
            .Should().ThrowAsync<ValidationException>();
    }

    [Test]
    public async Task Handle_ShoulThrowValidationException_WhenTitleIsDuplicated()
    {
        // Arrange
        const string title = "When you stick a broom handle into the soil it will flower overnight";
        await AddAsync(new TodoList
        {
            Title = title
        });
        var command = new CreateTodoListCommand { Title = title };

        // Act + Assert
        (await FluentActions.Invoking(() => SendAsync(command))
            .Should().ThrowAsync<ValidationException>())
            .Where(ex => ex.Errors.ContainsKey("Title"))
            .And.Errors["Title"].First().Should().Be("'Title' must be unique.");
    }
}
