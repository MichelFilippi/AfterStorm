namespace AfterStorm.Application.Models;

public sealed record ReplayUploadResult(
    Guid MatchId,
    string ReplayHash,
    string ExternalReplayId,
    string Map,
    int DurationSeconds,
    int Players,
    int Talents,
    int TimelineEvents);
