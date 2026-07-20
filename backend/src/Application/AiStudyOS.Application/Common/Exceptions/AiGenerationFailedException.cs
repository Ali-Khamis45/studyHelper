namespace AiStudyOS.Application.Common.Exceptions;

public class AiGenerationFailedException(string reason) : Exception($"AI generation failed: {reason}");
