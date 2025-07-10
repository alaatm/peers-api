namespace Mashkoor.Modules.I18n.Domain;

/// <summary>
/// Represents a localized field to be used in commands.
/// </summary>
/// <param name="Language">The language.</param>
/// <param name="Value">The value.</param>
public sealed record LocalizedField(string Language, string Value)
{
    public TranslatedField ToTranslatedField([NotNull] Language[] languages)
    {
        foreach (var l in languages)
        {
            if (l.Id.Equals(Language, StringComparison.OrdinalIgnoreCase))
            {
                return new TranslatedField(l, Value);
            }
        }

        throw new InvalidOperationException($"No language found for '{Language}'.");
    }

    public static LocalizedField[] CreateList([NotNull] params (string lang, string value)[] args)
    {
        var retVal = new LocalizedField[args.Length];
        for (var i = 0; i < args.Length; i++)
        {
            retVal[i] = new LocalizedField(args[i].lang, args[i].value);
        }

        return retVal;
    }
}

public static class LocalizedFieldExtensions
{
    public static TranslatedField[] ToTranslatedFields(
        [NotNull] this LocalizedField[] fields,
        [NotNull] Language[] languages)
    {
        var retVal = new TranslatedField[fields.Length];
        for (var i = 0; i < fields.Length; i++)
        {
            retVal[i] = fields[i].ToTranslatedField(languages);
        }
        return retVal;
    }
}
