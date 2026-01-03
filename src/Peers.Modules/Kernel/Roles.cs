namespace Peers.Modules.Kernel;

public static class Roles
{
    public const string Admin = "Admin";
    public const string Customer = "Customer";
    public const string Seller = "Seller";

    // Represents a staff member, regardless of their role.
    public const string Staff = "Staff";
    public const string CatalogManager = "CatalogManager";
    public const string UsersManager = "UsersManager";
    public const string SettingsManager = "SettingsManager";
    public const string MaintenanceAdmin = "MaintenanceAdmin";
    public const string FinanceAdmin = "FinanceAdmin";
    public const string PowerAdmin = "PowerAdmin";

    public static string[] Default =>
    [
        Admin,
        CatalogManager,
        UsersManager,
        SettingsManager,
        MaintenanceAdmin,
        FinanceAdmin,
        PowerAdmin,
        Staff,

        Customer,
        Seller,
    ];
}
