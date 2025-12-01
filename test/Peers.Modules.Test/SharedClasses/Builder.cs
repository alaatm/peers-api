using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Bogus;
using Bogus.DataSets;
using Microsoft.Extensions.Localization;
using Peers.Core.Http;
using Peers.Core.Localization;
using Peers.Core.Security.Hashing;
using Peers.Modules.Customers.Domain;
using Peers.Modules.Users.Commands;
using Peers.Modules.Users.Domain;

namespace Peers.Modules.Test.SharedClasses;

public static partial class EntityBuilder
{
    public static string TestUsername() => UsernameFixerRegex().Replace(new Internet().UserName(), "_");
    public static string TestPhoneNumber() => new PhoneNumbers().PhoneNumber("+9665########");
    public static string TestNationalId() => new Randomizer().Replace("1#########");

    public static Faker<AppUser> Test2FUser(
        DateTime? date = null,
        string username = null,
        string phoneNumber = null,
        bool? isBanned = null,
        bool registerDevice = true,
        bool addToken = true) => new Faker<AppUser>()
        .CustomInstantiator(p =>
        {
            date ??= DateTime.UtcNow;

            var user = AppUser.CreateTwoFactorAccount(
                date.Value,
                username ?? TestUsername(),
                phoneNumber ?? TestPhoneNumber(),
                Lang.EnLangCode);

            user.Status = (isBanned ?? false) ? UserStatus.Banned : user.Status;
            if (registerDevice)
            {
                user.DeviceList = [TestDevice(user, date).Generate()];
            }
            if (addToken)
            {
                user.GetOrCreateRefreshToken(date.Value);
            }

            return user;
        });

    public static Faker<AppUser> TestPwUser(
        DateTime? date = null,
        string email = null,
        string firstName = null,
        string lastName = null,
        bool? isBanned = null,
        bool addToken = true) => new Faker<AppUser>()
        .CustomInstantiator(p =>
        {
            date ??= DateTime.UtcNow;

            var user = AppUser.CreatePasswordAccount(
                date.Value,
                email ?? p.Internet.Email(),
                firstName ?? p.Name.FirstName(),
                lastName ?? p.Name.LastName(),
                Lang.EnLangCode);

            user.Status = (isBanned ?? false) ? UserStatus.Banned : user.Status;
            if (addToken)
            {
                user.GetOrCreateRefreshToken(date.Value);
            }
            return user;
        });

    public static Faker<Customer> TestCustomer(
        DateTime? date = null,
        string username = null,
        string phoneNumber = null,
        string secret = null,
        bool? isBanned = null) => new Faker<Customer>()
        .CustomInstantiator(p =>
        {
            var user = Test2FUser(date ?? DateTime.UtcNow, username, phoneNumber, isBanned).Generate();
            var customer = Customer.Create(user, secret ?? new HmacHash().GenerateKey());
            return customer;
        });

    public static Faker<Device> TestDevice(AppUser user, DateTime? date = null) => new Faker<Device>()
        .CustomInstantiator(p =>
        {
            var deviceCmd = TestRegisterDevice.Generate();
            return new Device(date ?? DateTime.UtcNow, user, deviceCmd.Id, deviceCmd.Manufacturer, deviceCmd.Model, deviceCmd.Platform, deviceCmd.OSVersion, deviceCmd.Idiom, deviceCmd.Type, deviceCmd.PnsHandle,
                deviceCmd.App, deviceCmd.AppVersion);
        });

    public static string TestIban(
        string bankCode = null)
    {
        // See https://en.wikipedia.org/wiki/International_Bank_Account_Numbe
        // and /src/.../Commands/IbanValidator.cs

        bankCode ??= "45";
        var accountNumber = new Randomizer().Replace("##################");

        // The 28100 represent SA converted to its numeric equivalent, 28 for S, 10 for A.
        // The 00 at the end is the initial check digits
        var iban = $"{bankCode}{accountNumber}281000";

        var checkDigits = (98 - (decimal.Parse(iban, CultureInfo.InvariantCulture) % 97)).ToString("00", CultureInfo.InvariantCulture);
        return $"SA{checkDigits}{bankCode}{accountNumber}";
    }

    [GeneratedRegex("[^A-Za-z0-9_]")]
    private static partial Regex UsernameFixerRegex();
}

public static class CommandBuilder
{
    public static FormFile TestFormFile(string name = null, string contentType = null, int size = 2) => new()
    {
        ContentType = contentType ?? "image/jpeg",
        Name = name ?? "test name",
        Stream = new MemoryStream(new byte[size]),
    };

    public static readonly Faker<ChangePassword.Command> TestChangePassword = new Faker<ChangePassword.Command>()
        .CustomInstantiator(p =>
        {
            var currentPassword = p.Internet.Password(6);
            var newPassword = p.Internet.Password(6);
            return new ChangePassword.Command(currentPassword, newPassword, newPassword);
        });

    public static Faker<Enroll.Command> TestEnroll() => new Faker<Enroll.Command>()
        .CustomInstantiator(p =>
        {
            var username = TestUsername();
            var phoneNumber = TestPhoneNumber();
            return new Enroll.Command(username, phoneNumber, null);
        });

    public static Faker<EnrollConfirm.Command> TestEnrollConfirm() => new Faker<EnrollConfirm.Command>()
        .CustomInstantiator(p =>
        {
            string username = null;
            string phoneNumber = null;
            string password = null;

            username = TestUsername();
            phoneNumber = TestPhoneNumber();

            return new EnrollConfirm.Command(
                p.Random.String2(4, "0123456789"),
                username,
                phoneNumber,
                password,
                Lang.EnLangCode);
        });

    public static Faker<SignIn.Command> TestSignIn(bool byUsername = false) => new RecordFaker<SignIn.Command>()
        .StrictMode(true)
        .RuleFor(p => p.Username, p => byUsername ? TestUsername() : null)
        .RuleFor(p => p.PhoneNumber, p => byUsername ? null : TestPhoneNumber())
        .RuleFor(p => p.Platform, p => null);

    public static Faker<CreateToken.Command> TestCreateToken(CreateToken.GrantType grantType) => new Faker<CreateToken.Command>()
        .CustomInstantiator(p =>
        {
            string username = null;
            string password = null;

            switch (grantType)
            {
                case CreateToken.GrantType.Mfa:
                    username = TestUsername();
                    password = p.Random.String2(6, "0123456789");
                    break;
                case CreateToken.GrantType.Password:
                    username = p.Internet.Email();
                    password = p.Internet.Password(6);
                    break;
                case CreateToken.GrantType.RefreshToken:
                    username = TestUsername(); // or email
                    password = Guid.NewGuid().ToString();
                    break;
                default:
                    break;
            }

            return new CreateToken.Command(username, password, grantType);
        });

    public static readonly Faker<RegisterDevice.Command> TestRegisterDevice = new RecordFaker<RegisterDevice.Command>()
        .StrictMode(true)
        .RuleFor(p => p.Id, p => Guid.NewGuid())
        .RuleFor(p => p.Manufacturer, p => p.PickRandom("Apple", "Samsung", "LG"))
        .RuleFor(p => p.Model, p => p.Random.String2(6))
        .RuleFor(p => p.Platform, p => p.PickRandom("iOS", "Android"))
        .RuleFor(p => p.OSVersion, p => p.PickRandom("12.0", "17.0"))
        .RuleFor(p => p.Idiom, p => p.PickRandom("Phone", "Tablet"))
        .RuleFor(p => p.Type, p => p.PickRandom("Physical", "Virtual"))
        .RuleFor(p => p.PnsHandle, p => Guid.NewGuid().ToString())
        .RuleFor(p => p.App, p => p.PickRandom("App1", "App2", "App3"))
        .RuleFor(p => p.AppVersion, p => p.PickRandom("1.0", "1.1", "1.2"))
        .RuleFor(p => p.TrackingId, p => null);
}

public static class MockBuilder
{
    public static Mock<IStringLocalizerFactory> GetLocalizerFactoryMoq()
    {
        var stringLocalizerFactory = new Mock<IStringLocalizerFactory>();
        stringLocalizerFactory
            .Setup(s => s.Create(It.IsAny<Type>()))
            .Returns((Type t) => new SLMoq());
        return stringLocalizerFactory;
    }

    public class SLMoq : IStringLocalizer
    {
        public virtual LocalizedString this[string name]
            => new(name, name);
        public virtual LocalizedString this[string name, params object[] arguments]
            => new(string.Format(CultureInfo.InvariantCulture, name, arguments), string.Format(CultureInfo.InvariantCulture, name, arguments));
        public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures)
            => throw new NotImplementedException();
    }
    public class SLMoq<T> : SLMoq, IStringLocalizer<T>
    {
    }

    public class SLCultureMoq<T> : SLMoq, IStringLocalizer<T>
    {
        public override LocalizedString this[string name]
        {
            get
            {
                var l = base[name];
                return new(l.Name, $"{Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName}:{l.Value}");
            }
        }

        public override LocalizedString this[string name, params object[] arguments]
        {
            get
            {
                var l = base[name, arguments];
                return new(l.Name, $"{Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName}:{l.Value}");
            }
        }
    }
}

public class RecordFaker<T> : Faker<T> where T : class
{
    public RecordFaker() => CustomInstantiator(_ => RuntimeHelpers.GetUninitializedObject(typeof(T)) as T);
}
