using System.Text.Json;
using Peers.Core.Payments.Providers.Moyasar.Models;

namespace Peers.Core.Test.Payments.Providers.Moyasar.Models;

public class MoyasarErrorResponseTests
{
    [Theory]
    [MemberData(nameof(Errors_returns_null_when_errors_is_not_a_JsonElement_Data))]
    public void Errors_returns_null_when_errors_is_not_a_JsonElement(string encoded, string expected)
    {
        // Arrange
        var json = "{\"type\":\"error\",\"message\":\"error\",\"errors\":" + encoded + "}";
        var errorResponse = JsonSerializer.Deserialize<MoyasarErrorResponse>(json);

        // Act
        var errString = errorResponse.ToString();

        // Assert
        Assert.EndsWith(expected, errString);
    }

    public static TheoryData<string, string> Errors_returns_null_when_errors_is_not_a_JsonElement_Data()
    {
        var nl = Environment.NewLine;

        var one = $"Errors:{nl}";
        var two = $"Errors:{nl}value: error{nl}";
        var three = $"Errors:{nl}extra:{nl}value: e{nl}";
        var four = $"Errors:{nl}amount:{nl}array:{nl}value: e1{nl}value: e2{nl}";
        var five = $"Errors:{nl}source:{nl}k1:{nl}array:{nl}value: e1{nl}";

        return new()
        {
            { "null", one },
            { "\"error\"", two },
            { /*lang=json,strict*/ "{ \"extra\" : \"e\" }", three },
            { /*lang=json,strict*/ "{ \"amount\" : [\"e1\", \"e2\"] }", four },
            { /*lang=json,strict*/ "{ \"source\" : { \"k1\": [\"e1\"] } }", five },
        };
    }
}
