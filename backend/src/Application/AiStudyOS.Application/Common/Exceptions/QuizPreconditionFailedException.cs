namespace AiStudyOS.Application.Common.Exceptions;

public class QuizPreconditionFailedException(string reason) : Exception(reason);
