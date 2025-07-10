using Mashkoor.Core.Localization;
using Mashkoor.Modules.Users.Commands;

namespace Mashkoor.Modules.Test.Users.Commands.Validation;

public class SetPreferredLanguageTests : CommandValidatorTestBase<SetPreferredLanguage.Command, SetPreferredLanguage.Validator>
{
    [Fact]
    public void ValidateTest()
    {
        CheckNotEmptyWithLengthRestriction(2, p => p.PreferredLanguage);
        CheckLen(p => p.PreferredLanguage, 2, 2);
        CheckOk();
    }

    protected override SetPreferredLanguage.Command GetValidCommand() => new(Lang.EnLangCode);
}
