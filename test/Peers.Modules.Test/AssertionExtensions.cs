using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Peers.Core.Data.Identity;

namespace Peers.Modules.Test;

public static class AssertX
{
    public static T IsType<T>(IResult actionResult)
    {
        try
        {
            return Assert.IsType<T>(actionResult);
        }
        catch (Xunit.Sdk.IsTypeException ex)
        {
            if (actionResult is BadRequest<ProblemDetails> badRequest && badRequest.Value is ProblemDetails problem1)
            {
                var message = $"Detail: {problem1.Detail}";
                if (problem1.Extensions.TryGetValue("errors", out var errors) && errors is string[])
                {
                    message += Environment.NewLine + string.Join(Environment.NewLine, errors as string[]);
                }

                throw new AssertException(message, ex);
            }
            else if (actionResult is ObjectResult objResult && objResult.Value is HttpValidationProblemDetails problem2)
            {
                throw new AssertException(string.Join(Environment.NewLine, problem2.Errors.SelectMany(p => p.Value)), ex);
            }
            else if (actionResult is ProblemHttpResult problem3)
            {
                var problemDetails = problem3.ProblemDetails;
                var message = $"Detail: {problemDetails.Detail}";
                if (problemDetails is HttpValidationProblemDetails validationProblemDetails)
                {
                    message += Environment.NewLine + string.Join(Environment.NewLine, validationProblemDetails.Errors.Select(p => $"{p.Key}: {string.Join(" | ", p.Value)}"));
                }

                throw new AssertException(message, ex);
            }

            throw;
        }
    }

    public static void DictMatch((string key, string value)[] expected, IReadOnlyDictionary<string, string> actual)
    {
        Assert.Equal(expected.Length, actual.Count);

        foreach (var (key, value) in expected)
        {
            Assert.Equal(value, actual[key]);
        }
    }

    public static async Task AssertAssignedRolesAndClaimsAsync<TUser, TContext>(
        this IdentityUserManager<TUser, TContext> um,
        TUser user,
        string[] expectedRoles,
        Claim[] expectedClaims)
        where TUser : IdentityUser<int>
        where TContext : IdentityDbContext<TUser, IdentityRole<int>, int>
    {
        var (roles, claims) = await um.GetRolesAndClaimsAsync(user);

        Assert.Equal(expectedRoles.Order(), roles.Order());

        var expectedClaimsSorted = expectedClaims.OrderBy(p => p.Type).ToArray();
        var claimsSorted = claims.OrderBy(p => p.Type).ToArray();

        Assert.Equal(expectedClaimsSorted.Length, claimsSorted.Length);
        for (var i = 0; i < expectedClaimsSorted.Length; i++)
        {
            Assert.Equal(expectedClaimsSorted[i].Type, claimsSorted[i].Type);
            Assert.Equal(expectedClaimsSorted[i].Value, claimsSorted[i].Value);
        }
    }
}

public class AssertException : Exception
{
    public AssertException(string message, Exception innerException)
        : base(message, innerException) { }
}
