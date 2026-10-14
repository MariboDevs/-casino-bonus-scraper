namespace BonusScraper.Cli;

/// <summary>
/// Process exit codes, chosen so a scheduler can alert on anything non-zero while still
/// telling "some casinos failed, data was published" apart from "nothing was published".
/// </summary>
public static class ExitCodes
{
    public const int Success = 0;
    public const int Failed = 1;
    public const int InvalidConfiguration = 2;
    public const int PartiallySucceeded = 3;
}
