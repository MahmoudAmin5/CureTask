using Cure.Domain.Common;

namespace Cure.Domain.Entities;

public sealed class Nurse : Entity
{
    public string UserId { get; set; } = null!;

    public string Department { get; set; } = null!;

    public string LicenseNumber { get; set; } = null!;

    public DateTime HireDate { get; set; }

    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
}
