using invoice_backend.Models.Enums;

namespace invoice_backend.Models;

public abstract class BaseEntity
{
    public int Id { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime ModifiedAt { get; set; }

    public DateTime? SoftDeletedAt { get; set; }

    public bool IsActive { get; set; } = true;

    public bool IsArchived { get; set; } = false;

    public Status Status { get; set; } = Status.Active;
}
