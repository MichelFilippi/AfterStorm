namespace AfterStorm.Application.Interfaces;

public interface IReplayStorage
{
    Task<string> SaveAsync(Stream replay, string fileName, CancellationToken cancellationToken = default);
}
