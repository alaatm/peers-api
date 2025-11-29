namespace Peers.Core.Nafath.Models;

internal static class NafathCallbackConstants
{
    public static class Status
    {
        public const string Namespace = "status";
        public const string Completed = "COMPLETED";
        public const string Rejected = "REJECTED";
    }

    public static class Attrs
    {
        public const string NationalId = "nin";
        public const string IQamaNumber = "iqamaNumber";
        public const string FirstNameAr = "firstName";
        public const string LastNameAr = "familyName";
        public const string LastNameArNonSaudi = "lastName";
        public const string FirstNameEn = "englishFirstName";
        public const string LastNameEn = "englishLastName";
        public const string Gender = "gender";
    }
}
