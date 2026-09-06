namespace BusinessTwin.Server.Shared;

public abstract class EntityAudit
{
    protected EntityAudit(Guid id, DateTimeOffset createdAt, string createdBy)
    {
        Id = id;
        CreatedAt = createdAt;
        CreatedBy = createdBy;
    }

    public Guid Id { get; }

    public DateTimeOffset CreatedAt { get; }

    public string CreatedBy { get; }
}