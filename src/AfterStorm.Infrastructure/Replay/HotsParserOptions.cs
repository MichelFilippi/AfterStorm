namespace AfterStorm.Infrastructure.Replay;

public sealed class HotsParserOptions
{
    public string PythonExecutable { get; set; } = string.Empty;
    public string ScriptPath { get; set; } = string.Empty;
    public string WorkingDirectory { get; set; } = string.Empty;
}
