namespace AiStudyOS.Application.Common.Exceptions;

public class EmailAlreadyExistsException(string email) : Exception($"An account with email '{email}' already exists.");
