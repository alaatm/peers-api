using System.Globalization;
using System.Text.RegularExpressions;
using FluentValidation;
using FluentValidation.Validators;

namespace Mashkoor.Core.Commands;

internal sealed class IbanValidator<T> : PropertyValidator<T, string>
{
    private static readonly Regex _uaeIban = new(@"^AE\d{21}$", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture, TimeSpan.FromSeconds(2));
    private readonly IStrLoc _l;

    public override string Name => "IbanValidator";

    public IbanValidator(IStrLoc l) => _l = l;

    public override bool IsValid(ValidationContext<T> context, string value)
    {
        //
        // Validation based on Wikipedia article:
        // https://en.wikipedia.org/wiki/International_Bank_Account_Number
        //

        if (value is null)
        {
            context.AddFailure(_l["IBAN is empty."]);
            return false;
        }

        value = value
            .Replace(" ", string.Empty, StringComparison.OrdinalIgnoreCase)
            .Replace("-", string.Empty, StringComparison.OrdinalIgnoreCase)
            .Trim();

        if (!_uaeIban.IsMatch(value))
        {
            context.AddFailure(_l["Invalid IBAN structure."]);
            return false;
        }

        // Move country and check digits to the end
        value = value[4..] + value[..4];

        // Replace letters with digits
        value = value.Replace("AE", "1014", StringComparison.OrdinalIgnoreCase);

        // Validate checksum
        if (decimal.Parse(value, CultureInfo.InvariantCulture) % 97 != 1)
        {
            context.AddFailure(_l["Incorrect IBAN check digits."]);
            return false;
        }

        return true;
    }

    protected override string GetDefaultMessageTemplate(string errorCode)
        => "'{PropertyName}' " + _l["must be a valid IBAN."];
}
