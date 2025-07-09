using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using FluentValidation;
using FluentValidation.TestHelper;
using Microsoft.Extensions.Localization;
using Mashkoor.Core.Commands;
using Mashkoor.Core.Http;
using static Mashkoor.Modules.Test.SharedClasses.MockBuilder;

namespace Mashkoor.Modules.Test;

public abstract class CommandValidatorTestBase<TCommand, TCommandValidator>
    where TCommand : ICommand
    where TCommandValidator : IValidator<TCommand>
{
    private Expression _preSet;
    private object _preValue;

    protected TCommandValidator Validator
    {
        get
        {
            var ctor = typeof(TCommandValidator).GetConstructors().FirstOrDefault();
            var ctorParams = ctor.GetParameters();
            var args = new object[ctorParams.Length];
            for (var i = 0; i < ctorParams.Length; i++)
            {
                var paramType = ctorParams[i].ParameterType;
                if (paramType == typeof(IStringLocalizer<res>))
                {
                    args[i] = new SLMoq<res>();
                }
                else if (paramType == typeof(TimeProvider))
                {
                    args[i] = TimeProvider.System;
                }
            }

            return (TCommandValidator)ctor.Invoke(args);
        }
    }

    protected abstract TCommand GetValidCommand();

    public void CheckNotNull<TProperty>(Expression<Func<TCommand, TProperty>> memberAccessor)
        => Check(memberAccessor, null, true);

    public void CheckNotEmpty(Expression<Func<TCommand, Guid>> memberAccessor, TCommand cmd = default)
    {
        Check(memberAccessor, null, true, cmd);
        Check(memberAccessor, Guid.Empty, true, cmd);
        Check(memberAccessor, Guid.NewGuid(), false, cmd);
    }

    public void CheckNotEmpty<TProperty>(Expression<Func<TCommand, TProperty>> memberAccessor, TCommand cmd = default)
    {
        Trace.Assert(GetPropertyInfo(memberAccessor).pi.PropertyType == typeof(string));
        Check(memberAccessor, null, true, cmd);
        Check(memberAccessor, "", true, cmd);
        Check(memberAccessor, " ", true, cmd);
        Check(memberAccessor, "test", false, cmd);
    }

    public void CheckNotEmptyWithLengthRestriction<TProperty>(int maxLen, Expression<Func<TCommand, TProperty>> memberAccessor, TCommand cmd = default)
    {
        Trace.Assert(GetPropertyInfo(memberAccessor).pi.PropertyType == typeof(string));
        Check(memberAccessor, null, true, cmd);
        Check(memberAccessor, "", true, cmd);
        Check(memberAccessor, " ", true, cmd);
        Check(memberAccessor, new string('0', maxLen), false, cmd);
    }

    public void CheckAllNotEmpty<TProperty>(Expression<Func<TCommand, TProperty>> memberAccessor)
    {
        Trace.Assert(GetPropertyInfo(memberAccessor).pi.PropertyType == typeof(string[]));
        Check(memberAccessor, null, true);
        Check(memberAccessor, Array.Empty<string>(), true);
        Check(memberAccessor, new[] { "" }, true);
        Check(memberAccessor, new[] { "test" }, false);
    }

    public void CheckIdArray<TProperty>(Expression<Func<TCommand, TProperty>> memberAccessor)
    {
        Trace.Assert(GetPropertyInfo(memberAccessor).pi.PropertyType == typeof(int[]));
        Check(memberAccessor, null, true);
        Check(memberAccessor, Array.Empty<int>(), true);
        Check(memberAccessor, new[] { 1 }, false);
        Check(memberAccessor, new[] { 0, 1 }, true);
        Check(memberAccessor, new[] { 1, 2 }, false);
    }

    public void CheckPercentage<TProperty>(Expression<Func<TCommand, TProperty>> memberAccessor)
    {
        var propType = GetPropertyInfo(memberAccessor).pi.PropertyType;
        Trace.Assert(propType == typeof(decimal));

        Check(memberAccessor, 0.01m, false);
        Check(memberAccessor, 0.99m, false);
        Check(memberAccessor, 0m, true);
        Check(memberAccessor, 1m, true);
    }

    public void CheckRange<TProperty>(Expression<Func<TCommand, TProperty>> memberAccessor, int min, int max)
    {
        var propType = GetPropertyInfo(memberAccessor).pi.PropertyType;
        Trace.Assert(propType == typeof(int) || propType == typeof(float) || propType == typeof(double));

        Check(memberAccessor, min, false);
        Check(memberAccessor, max, false);
        Check(memberAccessor, min - 1, true);
        Check(memberAccessor, max + 1, true);
    }

    public void CheckRange<TProperty>(Expression<Func<TCommand, TProperty>> memberAccessor, double min, double max)
    {
        var propType = GetPropertyInfo(memberAccessor).pi.PropertyType;
        Trace.Assert(propType == typeof(int) || propType == typeof(float) || propType == typeof(double) || propType == typeof(decimal));

        Check(memberAccessor, min, false);
        Check(memberAccessor, max, false);
        Check(memberAccessor, min - 1, true);
        Check(memberAccessor, max + 1, true);
    }

    public void CheckRange<TProperty>(Expression<Func<TCommand, TProperty>> memberAccessor, decimal min, decimal max)
    {
        var propType = GetPropertyInfo(memberAccessor).pi.PropertyType;
        Trace.Assert(propType == typeof(decimal));

        Check(memberAccessor, min, false);
        Check(memberAccessor, max, false);
        Check(memberAccessor, min - 1, true);
        //Check(memberAccessor, max + 1, true);
    }

    public void CheckMin<TProperty>(Expression<Func<TCommand, TProperty>> memberAccessor, decimal min)
    {
        var propType = GetPropertyInfo(memberAccessor).pi.PropertyType;
        Trace.Assert(propType == typeof(decimal));

        Check(memberAccessor, min - 1, true);
        Check(memberAccessor, min, false);
        Check(memberAccessor, min + 1, false);
    }

    public void CheckMax<TProperty>(Expression<Func<TCommand, TProperty>> memberAccessor, decimal max)
    {
        var propType = GetPropertyInfo(memberAccessor).pi.PropertyType;
        Trace.Assert(propType == typeof(decimal));

        Check(memberAccessor, max + 1, true);
        Check(memberAccessor, max, true);
        Check(memberAccessor, max - 1, false);
    }

    public void CheckDateFuture<TProperty>(Expression<Func<TCommand, TProperty>> memberAccessor)
    {
        var propType = GetPropertyInfo(memberAccessor).pi.PropertyType;
        var isNullable = propType.IsGenericType;
        Trace.Assert(!isNullable ? propType == typeof(DateTime) : propType == typeof(DateTime?));

        if (isNullable)
        {
            Check(memberAccessor, null, true);
        }

        Check(memberAccessor, DateTime.UtcNow.AddYears(-1), true);
        Check(memberAccessor, DateTime.UtcNow.AddYears(1), false);
    }

    public void CheckDatePast<TProperty>(Expression<Func<TCommand, TProperty>> memberAccessor)
    {
        var propType = GetPropertyInfo(memberAccessor).pi.PropertyType;
        var isNullable = propType.IsGenericType;
        Trace.Assert(!isNullable ? propType == typeof(DateTime) : propType == typeof(DateTime?));

        if (isNullable)
        {
            Check(memberAccessor, null, true);
        }

        Check(memberAccessor, DateTime.UtcNow.AddYears(-1), false);
        Check(memberAccessor, DateTime.UtcNow.AddYears(1), true);
    }

    public void CheckDateOnlyFuture<TProperty>(Expression<Func<TCommand, TProperty>> memberAccessor)
    {
        var propType = GetPropertyInfo(memberAccessor).pi.PropertyType;
        var isNullable = propType.IsGenericType;
        Trace.Assert(!isNullable ? propType == typeof(DateOnly) : propType == typeof(DateOnly?));

        if (isNullable)
        {
            Check(memberAccessor, null, true);
        }

        Check(memberAccessor, DateTime.UtcNow.AddYears(-1).ToDateOnly(), true);
        Check(memberAccessor, DateTime.UtcNow.AddYears(1).ToDateOnly(), false);
    }

    public void CheckDateOnlyPast<TProperty>(Expression<Func<TCommand, TProperty>> memberAccessor)
    {
        var propType = GetPropertyInfo(memberAccessor).pi.PropertyType;
        var isNullable = propType.IsGenericType;
        Trace.Assert(!isNullable ? propType == typeof(DateOnly) : propType == typeof(DateOnly?));

        if (isNullable)
        {
            Check(memberAccessor, null, true);
        }

        Check(memberAccessor, DateTime.UtcNow.AddYears(-1).ToDateOnly(), false);
        Check(memberAccessor, DateTime.UtcNow.AddYears(1).ToDateOnly(), true);
    }

    public void CheckLen<TProperty>(Expression<Func<TCommand, TProperty>> memberAccessor, int len, TCommand cmd = default)
        => CheckLen(memberAccessor, len, int.MaxValue, cmd);

    public void CheckLen<TProperty>(Expression<Func<TCommand, TProperty>> memberAccessor, int min, int max, TCommand cmd = default)
    {
        Trace.Assert(GetPropertyInfo(memberAccessor).pi.PropertyType == typeof(string));
        Check(memberAccessor, null, true, cmd);
        Check(memberAccessor, "", true, cmd);
        Check(memberAccessor, " ", true, cmd);
        if (max == int.MaxValue)
        {
            Check(memberAccessor, new string('s', min - 1), true, cmd);
            Check(memberAccessor, new string('s', min), false, cmd);
        }
        else
        {
            Check(memberAccessor, new string('s', min - 1), true);
            Check(memberAccessor, new string('s', max + 1), true);
            for (var i = min; i <= max; i++)
            {
                Check(memberAccessor, new string('s', i), false);
            }
        }
    }

    public void CheckEmail<TProperty>(Expression<Func<TCommand, TProperty>> memberAccessor, TCommand cmd = default)
    {
        Trace.Assert(GetPropertyInfo(memberAccessor).pi.PropertyType == typeof(string));
        Check(memberAccessor, null, true, cmd);
        Check(memberAccessor, "", true, cmd);
        Check(memberAccessor, " ", true, cmd);
        Check(memberAccessor, "invalid", true, cmd);
        Check(memberAccessor, "example@domain.com", false, cmd);
    }

    public void CheckPhone<TProperty>(Expression<Func<TCommand, TProperty>> memberAccessor)
    {
        Trace.Assert(GetPropertyInfo(memberAccessor).pi.PropertyType == typeof(string));
        Check(memberAccessor, null, true);
        Check(memberAccessor, "", true);
        Check(memberAccessor, " ", true);
        Check(memberAccessor, "5511111111", true);
        Check(memberAccessor, "+966511111111", false);
    }

    public void CheckCreditCard<TProperty>(Expression<Func<TCommand, TProperty>> memberAccessor)
    {
        Trace.Assert(GetPropertyInfo(memberAccessor).pi.PropertyType == typeof(string));
        Check(memberAccessor, null, true);
        Check(memberAccessor, "", true);
        Check(memberAccessor, " ", true);
        Check(memberAccessor, "1", true);
        Check(memberAccessor, "378282246310005", false);
    }

    public void CheckId<TProperty>(Expression<Func<TCommand, TProperty>> memberAccessor)
    {
        Trace.Assert(GetPropertyInfo(memberAccessor).pi.PropertyType == typeof(int));
        Check(memberAccessor, 0, true);
        Check(memberAccessor, 5, false);
    }

    public void CheckNullableId<TProperty>(Expression<Func<TCommand, TProperty>> memberAccessor)
    {
        Trace.Assert(GetPropertyInfo(memberAccessor).pi.PropertyType == typeof(int?));
        Check(memberAccessor, 0, true);
        Check(memberAccessor, 5, false);
        Check(memberAccessor, null, false);
    }

    public void CheckEnum<TProperty>(Expression<Func<TCommand, TProperty>> memberAccessor, params TProperty[] exclusions)
    {
        var propType = GetPropertyInfo(memberAccessor).pi.PropertyType;
        var isNullable = propType.IsGenericType;
        Trace.Assert(!isNullable ? propType.IsEnum : Nullable.GetUnderlyingType(propType).IsEnum);

        foreach (var e in Enum.GetValues(GetEnumType<TProperty>()))
        {
            if (Array.IndexOf(exclusions, e) < 0)
            {
                Check(memberAccessor, e, false);
            }
        }

        if (isNullable)
        {
            var nullableValue = (TProperty)Enum.ToObject(Nullable.GetUnderlyingType(propType), int.MaxValue);
            Check(memberAccessor, null, true);
            Check(memberAccessor, nullableValue, true);
        }
        else
        {
            Check(memberAccessor, int.MaxValue, true);
        }
    }

    public void CheckImageNullable<TProperty>(Expression<Func<TCommand, TProperty>> memberAccessor)
    {
        var propType = GetPropertyInfo(memberAccessor).pi.PropertyType;
        Trace.Assert(propType == typeof(FormFile));

        Check(memberAccessor, new FormFile { ContentType = "image/jpeg" }, false);
        Check(memberAccessor, new FormFile { ContentType = "audio/mp3" }, true);
        Check(memberAccessor, null, false);
    }

    public void CheckImage<TProperty>(Expression<Func<TCommand, TProperty>> memberAccessor)
    {
        var propType = GetPropertyInfo(memberAccessor).pi.PropertyType;
        Trace.Assert(propType == typeof(FormFile));

        Check(memberAccessor, new FormFile { ContentType = "image/jpeg" }, false);
        Check(memberAccessor, new FormFile { ContentType = "audio/mp3" }, true);
        Check(memberAccessor, null, true);
    }

    public void CheckSvgImage<TProperty>(Expression<Func<TCommand, TProperty>> memberAccessor)
    {
        var propType = GetPropertyInfo(memberAccessor).pi.PropertyType;
        Trace.Assert(propType == typeof(FormFile));

        Check(memberAccessor, new FormFile { ContentType = "image/svg+xml" }, false);
        Check(memberAccessor, new FormFile { ContentType = "image/jpeg" }, true);
        Check(memberAccessor, null, true);
    }

    public void CheckImages<TProperty>(Expression<Func<TCommand, TProperty>> memberAccessor)
    {
        var propType = GetPropertyInfo(memberAccessor).pi.PropertyType;
        Trace.Assert(propType == typeof(FormFile[]));

        Check(memberAccessor, Array.Empty<FormFile>(), false);
        Check(memberAccessor, new[] { new FormFile { ContentType = "image/jpeg" } }, false);
        Check(memberAccessor, new[] { new FormFile { ContentType = "audio/mp3" } }, true);
        Check(memberAccessor, new[] { new FormFile { ContentType = "image/jpeg" }, new FormFile { ContentType = "audio/mp3" } }, true);
    }

    public void Check<TProperty>(Expression<Func<TCommand, TProperty>> memberAccessor, object value, bool shouldFail, TCommand cmd = default)
    {
        cmd ??= GetValidCommand();
        SetValue(cmd, memberAccessor, value);

        if (_preSet is not null)
        {
            var (_, pi) = GetPropertyInfo(_preSet);
            pi.SetValue(cmd, _preValue);
        }

        var result = Validator.TestValidate(cmd);
        result.Assert(memberAccessor, shouldFail);
    }

    public void Check<TProperty>(TCommand cmd, Expression<Func<TCommand, TProperty>> memberAccessor, object value, bool shouldFail)
    {
        SetValue(cmd, memberAccessor, value);

        if (_preSet is not null)
        {
            var (_, pi) = GetPropertyInfo(_preSet);
            pi.SetValue(cmd, _preValue);
        }

        var result = Validator.TestValidate(cmd);
        result.Assert(memberAccessor, shouldFail);
    }

    public void CheckOk()
    {
        var cmd = GetValidCommand();
        var result = Validator.TestValidate(cmd);
        result.ShouldNotHaveAnyValidationErrors();
    }

    public void When<TProperty>(Expression<Func<TCommand, TProperty>> memberAccessor, TProperty value, Action<CommandValidatorTestBase<TCommand, TCommandValidator>> action)
    {
        _preSet = memberAccessor.Body;
        _preValue = value;
        action(this);
        _preValue = null;
        _preSet = null;
    }

    private static void SetValue<TProperty>(TCommand cmd, Expression<Func<TCommand, TProperty>> memberAccessor, object value)
    {
        var (parentPi, pi) = GetPropertyInfo(memberAccessor);
        if (parentPi is null)
        {
            pi.SetValue(cmd, value);
        }
        else
        {
            var subCmd = parentPi.GetValue(cmd);
            pi.SetValue(subCmd, value);
        }
    }

    private static (PropertyInfo parentPi, PropertyInfo pi) GetPropertyInfo<TProperty>(Expression<Func<TCommand, TProperty>> memberAccessor)
        => GetPropertyInfo(memberAccessor.Body);

    private static (PropertyInfo parentPi, PropertyInfo pi) GetPropertyInfo(Expression expression)
    {
        var propName = GetMemberName(expression);
        var pi = typeof(TCommand).GetProperty(propName);

        var parentPi = GetParentMember(((MemberExpression)expression).Expression);
        if (parentPi is not null)
        {
            pi = parentPi.PropertyType.GetProperty(propName);
        }

        return (parentPi, pi);
    }

    private static string GetMemberName(Expression expression) => expression.NodeType switch
    {
        ExpressionType.MemberAccess => ((MemberExpression)expression).Member.Name,
        ExpressionType.Convert => GetMemberName(((UnaryExpression)expression).Operand),
        _ => throw new NotSupportedException(expression.NodeType.ToString()),
    };

    private static PropertyInfo GetParentMember(Expression expression) => expression.NodeType switch
    {
        ExpressionType.MemberAccess => ((MemberExpression)expression).Member as PropertyInfo,
        _ => null,
    };

    private static Type GetEnumType<T>() => typeof(T).IsGenericType
        ? typeof(T).GenericTypeArguments[0]
        : typeof(T);
}

public static class TestValidationResultExtensions
{
    public static void Assert<T, TProperty>(this TestValidationResult<T> result, Expression<Func<T, TProperty>> memberAccessor, bool shouldFail)
    {
        if (shouldFail)
        {
            result.ShouldHaveValidationErrorFor(memberAccessor);
        }
        else
        {
            result.ShouldNotHaveValidationErrorFor(memberAccessor);
        }
    }
}
