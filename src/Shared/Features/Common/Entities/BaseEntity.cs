using NodaTime;

namespace Shared.Features.Common.Entities;

public abstract class BaseEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Instant CreatedAt { get; set; }
    public Instant? UpdatedAt { get; set; }
    public bool IsDeleted { get; set; }
}