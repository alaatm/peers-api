namespace Peers.Modules;

public static class IdentityInfoExtensions
{
    extension(IIdentityInfo ii)
    {
        public bool IsCustomer => ii.IsInRole(Roles.Customer);
        public bool IsStaff => ii.IsInRole(Roles.Staff);
    }
}
