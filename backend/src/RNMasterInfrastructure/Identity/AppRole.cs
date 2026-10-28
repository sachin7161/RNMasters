using Microsoft.AspNetCore.Identity;

namespace RNMasterInfrastructure.Identity;

public class AppRole : IdentityRole
{
    public int TenantId { get; set; }
}