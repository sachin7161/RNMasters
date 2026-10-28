using RNMasterInfrastructure.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace RNMasterInfrastructure.Persistence;

public class RNMasterDbContext
    : IdentityDbContext<AppUser, AppRole, string>
{
    public RNMasterDbContext(
        DbContextOptions<RNMasterDbContext> options)
        : base(options)
    {
    }

}