using Shared.Features.Common.Entities;

namespace Shared.Features.Clients.Entities;

public class Client : BaseEntity
{
    public string Name { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string? CompanyName { get; set; }
    public string? Address { get; set; }
    public string? PhoneNumber { get; set; }
}