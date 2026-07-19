using AiStudyOS.Application.Common.Interfaces;

namespace AiStudyOS.Infrastructure.Common;

public class SystemDateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow => DateTime.UtcNow;
}
