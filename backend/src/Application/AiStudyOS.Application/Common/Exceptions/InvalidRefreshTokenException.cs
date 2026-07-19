namespace AiStudyOS.Application.Common.Exceptions;

public class InvalidRefreshTokenException(string reason) : Exception($"Invalid refresh token: {reason}");
