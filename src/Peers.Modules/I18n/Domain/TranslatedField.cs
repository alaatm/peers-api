namespace Peers.Modules.I18n.Domain;

/// <summary>
/// Represents a localized field to be used in domain factory methods.
/// </summary>
/// <param name="Language">The language.</param>
/// <param name="Value">The value.</param>
public sealed record TranslatedField(Language Language, string Value)
{
    public static TranslatedField[] CreateList([NotNull] params (Language lang, string value)[] args)
    {
        var retVal = new TranslatedField[args.Length];
        for (var i = 0; i < args.Length; i++)
        {
            retVal[i] = new TranslatedField(args[i].lang, args[i].value);
        }

        return retVal;
    }
}
