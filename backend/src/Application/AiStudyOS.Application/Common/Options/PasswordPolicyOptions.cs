namespace AiStudyOS.Application.Common.Options;

public class PasswordPolicyOptions
{
    public const string SectionName = "PasswordPolicy";

    public int MinimumLength { get; init; } = 8;
    public int MaximumLength { get; init; } = 128;
}
