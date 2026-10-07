using AfterStorm.Application.Interfaces;

namespace AfterStorm.Infrastructure.Storage;

public sealed class LocalReplayStorage : IReplayStorage
{
    private readonly string _rootPath;

    public LocalReplayStorage(string rootPath)
    {
        _rootPath = rootPath;
        Directory.CreateDirectory(_rootPath);
    }

    public async Task<string> SaveAsync(Stream replay, string fileName, CancellationToken cancellationToken = default)
    {
        var extension = Path.GetExtension(fileName);
        if (!extension.Equals(".StormReplay", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Only .StormReplay files are accepted.");

        var safeName = $"{Guid.NewGuid():N}.StormReplay";
        var path = Path.Combine(_rootPath, safeName);

        await using var output = File.Create(path);
        await replay.CopyToAsync(output, cancellationToken);
        return path;
    }
}
