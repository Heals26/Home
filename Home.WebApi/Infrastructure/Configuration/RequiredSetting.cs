namespace Home.WebApi.Infrastructure.Configuration;

/// <summary>
/// One setting that is wrong, and what to do about it.
/// </summary>
/// <param name="Key">The configuration key, exactly as it is written in settings.</param>
/// <param name="Problem">What is wrong with it, in one sentence.</param>
/// <param name="Fix">The command that sets it, ready to paste.</param>
public record RequiredSetting(string Key, string Problem, string Fix);
