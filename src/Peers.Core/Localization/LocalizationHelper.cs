using System.Globalization;
using System.Text;

namespace Peers.Core.Localization;

public static class LocalizationHelper
{
    public static string FormatList(string[] items)
    {
        if (items is null || items.Length == 0)
        {
            return string.Empty;
        }

        var lang = Lang.GetCurrent();
        var and = lang is Lang.ArLangCode ? " و " : " and ";

        if (items.Length == 1)
        {
            return $"'{items[0]}'";
        }
        if (items.Length == 2)
        {
            return $"'{items[0]}'{and}'{items[1]}'";
        }

        var sb = new StringBuilder();
        var allExceptLast = items.AsSpan()[..^1];
        var sep = lang is Lang.ArLangCode ? "، " : ", ";

        for (var i = 0; i < allExceptLast.Length; i++)
        {
            sb.Append(CultureInfo.InvariantCulture, $"'{allExceptLast[i]}'");
            if (i < allExceptLast.Length - 1)
            {
                sb.Append(sep);
            }
        }

        sb.Append(CultureInfo.InvariantCulture, $"{and}'{items[^1]}'");

        return sb.ToString();
    }
}
