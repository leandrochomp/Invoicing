using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using NodaTime;

namespace Shared.Features.Common.Entities;

public abstract class BaseEntity
{
    [Column("created_at")]
    [ReadOnly(true)]
    public Instant CreatedAt { get; set; } = SystemClock.Instance.GetCurrentInstant();
    
    [Column("updated_at")]
    public Instant UpdatedAt { get; set; }
    
    [Column("is_deleted")]
    public bool IsDeleted { get; set; }
}