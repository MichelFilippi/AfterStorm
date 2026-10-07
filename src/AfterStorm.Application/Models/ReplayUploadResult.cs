namespace AfterStorm.Application.Models;

public sealed record ReplayUploadResult(
    Guid MatchId,
    string ReplayHash,
    string Map,
    int Players,
    int Talents);
