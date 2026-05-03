using NSubstitute;
using Tickets.Data.Abstractions;

namespace Tickets.Tests.Mocks;

/// <summary>
/// Provides mock implementations of data access layer for API+DAL integration testing
/// Uses NSubstitute to create mock repositories that return empty data
/// </summary>
public static class MockDataAccessProvider
{
    public static IUnitOfWork CreateMockUnitOfWork()
    {
        var unitOfWork = Substitute.For<IUnitOfWork>();

        // Configure repositories to return empty collections by default
        // This allows API and service layers to work without actual database
        unitOfWork.Events.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Enumerable.Empty<Domain.Entities.Event>()));

        unitOfWork.Venues.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Enumerable.Empty<Domain.Entities.Venue>()));

        return unitOfWork;
    }
}
