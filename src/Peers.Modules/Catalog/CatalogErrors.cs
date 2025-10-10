using System.Globalization;
using System.Numerics;
using Peers.Core.Domain.Errors;
using Peers.Core.Localization;
using Peers.Modules.Catalog.Domain.Attributes;

namespace Peers.Modules.Catalog;

public static class CatalogErrors
{
    /// <summary>
    /// Product type must be in 'Draft' state.
    /// </summary>
    public static DomainError NotDraft => new(Titles.CannotApplyOperation, "catalog.not-draft");
    /// <summary>
    /// Product type must be in 'Published' state.
    /// </summary>
    public static DomainError NotPublished => new(Titles.CannotApplyOperation, "catalog.not-published");
    /// <summary>
    /// Product type is already in 'Published' state.
    /// </summary>
    public static DomainError AlreadyPublished => new(Titles.CannotApplyOperation, "catalog.already-published");
    /// <summary>
    /// Child product type '{0}' already exists.
    /// </summary>
    public static DomainError ChildAlreadyExists(string name) => new(Titles.ResourceConflict, "catalog.child-already-exists", name);
    /// <summary>
    /// Attribute '{0}' does not exist.
    /// </summary>
    public static DomainError AttrNotFound(string attrKey) => new(Titles.NotFound, "catalog.attr-not-found", attrKey);
    /// <summary>
    /// Enum attribute '{0}' does not exist.
    /// </summary>
    public static DomainError EnumAttrNotFound(string attrKey) => new(Titles.NotFound, "catalog.enum-attr-not-found", attrKey);
    /// <summary>
    /// Attribute '{0}' kind must be 'Enum'.
    /// </summary>
    public static DomainError AttrNotEnum(string attrKey) => new(Titles.ValidationFailed, "catalog.attr-not-enum", attrKey);
    /// <summary>
    /// Attribute '{0}' already exists.
    /// </summary>
    public static DomainError AttrAlreadyExists(string attrKey) => new(Titles.ResourceConflict, "catalog.attr-already-exists", attrKey);
    /// <summary>
    /// Variant attributes are not allowed for boolean, string, or date kinds.
    /// </summary>
    public static DomainError VariantNotAllowedForBoolStrDate => new(Titles.ValidationFailed, "catalog.variant-not-allowed-for-bool-str-date");
    /// <summary>
    /// Cannot remove attribute '{0}'; the following attributes depend on it: {1}.
    /// </summary>
    public static DomainError RemoveForbiddenHasDependants(string attrKey, string[] dependants)
        => new(Titles.CannotApplyOperation, "catalog.remove-forbidden-has-dependants", attrKey, LocalizationHelper.FormatList(dependants));
    /// <summary>
    /// Attribute '{0}' dependency can only be set when no options are defined.
    /// </summary>
    public static DomainError EnumAttrDepSetOnlyIfNoOpts(string attrKey)
        => new(Titles.ValidationFailed, "catalog.enum-attr-dep-set-only-if-no-opts", attrKey);
    /// <summary>
    /// Attribute '{0}' has no options defined.
    /// </summary>
    public static DomainError EnumAttrNoOptions(string attrKey) => new(Titles.ValidationFailed, "catalog.enum-attr-no-options", attrKey);
    /// <summary>
    /// Option '{2}' of attribute '{0}' is not scoped, but attribute '{0}' depends on '{1}'; every option must be scoped to a parent option of '{1}'.
    /// </summary>
    public static DomainError EnumOptNotScopedButDep(string attrKey, string parentAttrKey, string optCode)
        => new(Titles.ValidationFailed, "catalog.opt-not-scoped-but-dep", attrKey, parentAttrKey, optCode);
    /// <summary>
    /// Option '{1}' of attribute '{0}' is scoped, but the attribute has no dependency.
    /// </summary>
    public static DomainError OptScopedButNoDep(string attrKey, string optCode) => new(Titles.ValidationFailed, "catalog.opt-scoped-but-no-dep", attrKey, optCode);
    /// <summary>
    /// Option '{1}' of attribute '{0}' has a duplicate code with another option.
    /// </summary>
    public static DomainError DuplicateEnumOptCode(string attrKey, string optCode)
        => new(Titles.ResourceConflict, "catalog.duplicate-enum-opt-code", attrKey, optCode);
    /// <summary>
    /// Option '{1}' of attribute '{0}' has a duplicate position with another option.
    /// </summary>
    public static DomainError DuplicateEnumOptPosition(string attrKey, string optCode)
        => new(Titles.ResourceConflict, "catalog.duplicate-enum-opt-position", attrKey, optCode);
    /// <summary>
    /// Attribute '{0}' depends on '{1}', but option '{2}' is scoped to an option of '{3}'.
    /// </summary>
    public static DomainError InvalidScopeParent(string attrKey, string expectedParentAttrKey, string optCode, string actualParentAttrKey)
        => new(Titles.ValidationFailed, "catalog.invalid-scope-parent", attrKey, expectedParentAttrKey, optCode, actualParentAttrKey);
    /// <summary>
    /// Attribute '{0}' depends on '{1}', which belongs to a different product type.
    /// </summary>
    public static DomainError CrossPTAttrDep(string attrKey, string parentAttrKey)
        => new(Titles.ValidationFailed, "catalog.cross-pt-attr-dep", attrKey, parentAttrKey);
    /// <summary>
    /// Attribute '{0}' position conflicts with another attribute.
    /// </summary>
    public static DomainError DuplicateAttrPosition(string attrKey) => new(Titles.ResourceConflict, "catalog.duplicate-attr-position", attrKey);
    /// <summary>
    /// Attribute '{0}' does not exist or is not of numeric kind.
    /// </summary>
    public static DomainError NumericAttrNotFound(string attrKey) => new(Titles.NotFound, "catalog.numeric-attr-not-found", attrKey);
    /// <summary>
    /// Attribute '{0}' does not exist or is not of group kind.
    /// </summary>
    public static DomainError GroupAttrNotFound(string attrKey) => new(Titles.NotFound, "catalog.group-attr-not-found", attrKey);
    /// <summary>
    /// Group attribute '{0}' must be variant.
    /// </summary>
    public static DomainError GroupAttrMustBeVariant(string attrKey)
        => new(Titles.ValidationFailed, "catalog.group-attr-must-be-variant", attrKey);
    /// <summary>
    /// Group attribute '{0}' must not be required.
    /// </summary>
    public static DomainError GroupAttrMustNotBeRequired(string attrKey)
        => new(Titles.ValidationFailed, "catalog.group-attr-must-not-be-required", attrKey);
    /// <summary>
    /// Group attribute '{0}' must be variant and cannot be required.
    /// </summary>
    public static DomainError GroupAttrMustBeVariantNotRequired(string attrKey)
        => new(Titles.ValidationFailed, "catalog.group-attr-must-be-variant-not-required", attrKey);
    /// <summary>
    /// Group attribute '{0}' must have at least {1} member attributes.
    /// </summary>
    public static DomainError GroupAttrMinMembers(string attrKey, int minMembers)
        => new(Titles.ValidationFailed, "catalog.group-attr-min-members", attrKey, minMembers.ToString(CultureInfo.InvariantCulture));
    /// <summary>
    /// Member attribute '{1}' cannot be added to group attribute '{0}' because it is already a member of group attribute '{2}'.
    /// </summary>
    public static DomainError GroupAttrMemberAlreadyInGroup(string groupAttrKey, string memberAttrKey, string existingGroupAttrKey)
        => new(Titles.ValidationFailed, "catalog.group-attr-member-already-in-group", groupAttrKey, memberAttrKey, existingGroupAttrKey);
    /// <summary>
    /// Member attribute '{1}' of group attribute '{0}' does not exist.
    /// </summary>
    public static DomainError GroupAttrMemberProductTypeMismatch(string attrKey, string memberProductTypeKey)
        => new(Titles.ValidationFailed, "catalog.group-attr-member-product-type-mismatch", attrKey, memberProductTypeKey);
    /// <summary>
    /// Member attribute '{1}' of group attribute '{0}' must be of kind 'Int' or 'Decimal'.
    /// </summary>
    public static DomainError GroupAttrMemberMustBeNumeric(string attrKey, string memberAttrKey)
        => new(Titles.ValidationFailed, "catalog.group-attr-member-must-be-numeric", attrKey, memberAttrKey);
    /// <summary>
    /// Group attribute '{0}' has member attributes of mixed numeric kinds; all members must be either 'Int' or 'Decimal'.
    /// </summary>
    public static DomainError GroupAttrMembersMixedNumericKind(string attrKey)
        => new(Titles.ValidationFailed, "catalog.group-attr-members-mixed-numeric-kind", attrKey);
    /// <summary>
    /// Group attribute '{0}' has member attributes with mixed or missing units; all members must have the same non-empty unit or all must have no unit.
    /// </summary>
    public static DomainError GroupAttrMembersMixedUnits(string attrKey)
        => new(Titles.ValidationFailed, "catalog.group-attr-members-mixed-units", attrKey);
    /// <summary>
    /// Member attribute '{1}' of group attribute '{0}' must not be a variant axis.
    /// </summary>
    public static DomainError GroupAttrMemberMustNotBeVariant(string attrKey, string memberAttrKey)
        => new(Titles.ValidationFailed, "catalog.group-attr-member-must-not-be-variant", attrKey, memberAttrKey);
    /// <summary>
    /// Group attribute '{0}' cannot include member attribute '{1}' more than once.
    /// </summary>
    public static DomainError GroupAttrDuplicateMember(string attrKey, string memberAttrKey)
        => new(Titles.ResourceConflict, "catalog.group-attr-duplicate-member", attrKey, memberAttrKey);
    /// <summary>
    /// A member attribute of the same key as '{1}' is already included in group attribute '{0}'.
    /// </summary>
    public static DomainError GroupAttrMemberAlreadyExists(string attrKey, string memberAttrKey)
        => new(Titles.ResourceConflict, "catalog.group-attr-member-already-exists", attrKey, memberAttrKey);
    /// <summary>
    /// Variant attribute '{0}' cannot be dependent.
    /// </summary>
    public static DomainError VariantAttributeCannotBeDependent(string attrKey)
        => new(Titles.ValidationFailed, "catalog.variant-attribute-cannot-be-dependent", attrKey);
    /// <summary>
    /// Attribute '{0}' cannot depend on attribute '{1}' because they belong to different product types.
    /// </summary>
    public static DomainError DepDiffProductType(string childKey, string parentKey)
        => new(Titles.ValidationFailed, "catalog.dep-diff-product-type", childKey, parentKey);
    /// <summary>
    /// Attribute '{0}' cannot depend on itself.
    /// </summary>
    public static DomainError SelfDep(string attrKey) => new(Titles.ValidationFailed, "catalog.self-dep", attrKey);
    /// <summary>
    /// Attribute '{0}' ({1}) cannot depend on '{2}' ({3}); only {1}→{1} is allowed.
    /// </summary>
    public static DomainError DepComboNotSupported(string childKey, AttributeKind childKind, string parentKey, AttributeKind parentKind)
        => new(Titles.ValidationFailed, "catalog.dep-combo-not-supported", childKey, childKind.ToString(), parentKey, parentKind.ToString());
    /// <summary>
    /// Option '{0}' already exists.
    /// </summary>
    public static DomainError EnumOptAlreadyExists(string code) => new(Titles.ResourceConflict, "catalog.opt-already-exists", code);
    /// <summary>
    /// Attribute has no dependency; scoped option is not allowed.
    /// </summary>
    public static DomainError ScopeOptReqDep => new(Titles.ValidationFailed, "catalog.scope-opt-req-dep");
    /// <summary>
    /// Attribute depends on another attribute; options must be scoped.
    /// </summary>
    public static DomainError DepReqScopeOtp => new(Titles.ValidationFailed, "catalog.dep-req-scope-opt");
    /// <summary>
    /// Option '{0}' does not exist.
    /// </summary>
    public static DomainError EnumOptNotFound(string code) => new(Titles.NotFound, "catalog.opt-not-found", code);
    /// <summary>
    /// Scope parent option belongs to a different attribute than the declared dependency.
    /// </summary>
    public static DomainError ParentOptMustBelongToDep => new(Titles.ValidationFailed, "catalog.parent-opt-must-belong-to-dep");
    /// <summary>
    /// The lookup type '{0}' does not allow variants.
    /// </summary>
    public static DomainError LookupTypeDoesNotAllowVariants(string key) => new(Titles.ValidationFailed, "catalog.lookup-type-does-not-allow-variants", key);
    /// <summary>
    /// Cyclic attribute dependency detected.
    /// </summary>
    public static DomainError CyclicDependency => new(Titles.ValidationFailed, "catalog.cyclic-dependency");
    /// <summary>
    /// The allowed lookup types contain types not defined in the schema: {0}.
    /// </summary>
    /// <returns></returns>
    public static DomainError AllowListContainsLookupTypesNotInSchema(string[] lookupTypeKeys)
        => new(Titles.ValidationFailed, "catalog.allowlist-contains-lookup-types-not-in-schema", LocalizationHelper.FormatList(lookupTypeKeys));
    /// <summary>
    /// Lookup type is required for lookup attributes.
    /// </summary>
    public static DomainError LookupTypeRequired => new(Titles.ValidationFailed, "catalog.lookup-type-required");
    /// <summary>
    /// The lookup types {0} are assigned to multiple attributes on the same product type, which is not allowed.
    /// </summary>
    public static DomainError DuplicateLookupTypeOnProductType(string[] lookupTypeKeys)
        => new(Titles.ResourceConflict, "catalog.duplicate-lookup-types-on-product-type", LocalizationHelper.FormatList(lookupTypeKeys));
    /// <summary>
    /// The lookup option '{0}' was not found.
    /// </summary>
    public static DomainError LookupOptNotFound(string key) => new(Titles.NotFound, "catalog.lookup-opt-not-found", key);
    /// <summary>
    /// The lookup option(s) {0} of type '{1}' is not in the allowed set declared by nearest ancestor '{2}'.
    /// </summary>
    public static DomainError LookupOptsNotAllowedByAncestor(string[] codes, string typeKey, string ancestorSlugPath)
        => new(Titles.ValidationFailed, "catalog.lookup-opts-not-allowed-by-ancestor", LocalizationHelper.FormatList(codes), typeKey, ancestorSlugPath);
    /// <summary>
    /// Duplicate allow-list entry(s) detected for lookup option(s): {0}. Each option may appear at most once.
    /// </summary>
    public static DomainError DuplicateAllowedLookupOpts(string[] codes)
        => new(Titles.ResourceConflict, "catalog.duplicate-allowed-lookup-opts", LocalizationHelper.FormatList(codes));
    /// <summary>
    /// The lookup type '{1}' used by attribute '{0}' requires an allow-list of permitted values on the product type, but none was provided.
    /// </summary>
    public static DomainError MissingLookupAllowList(string attrKey, string lookupTypeKey)
        => new(Titles.ValidationFailed, "catalog.missing-lookup-allow-list", attrKey, lookupTypeKey);
    /// <summary>
    /// Value '{1}' for attribute '{0}' must be an integral number.
    /// </summary>
    public static DomainError AttrValueMustBeIntegral(string attrKey, decimal value) => new(Titles.ValidationFailed, "catalog.attribute-value-must-be-integral", attrKey, value);
    /// <summary>
    /// Value '{1}' for attribute '{0}' must be at least '{2}'.
    /// </summary>
    public static DomainError AttrValueMustBeAtLeast<T>(string attrKey, T value, INumber<T> minValue) where T : struct, INumber<T>
        => new(Titles.ValidationFailed, "catalog.attribute-value-must-be-at-least", attrKey, value, minValue);
    /// <summary>
    /// Value '{1}' for attribute '{0}' must be at most '{2}'.
    /// </summary>
    public static DomainError AttrValueMustBeAtMost<T>(string attrKey, T value, INumber<T> maxValue) where T : struct, INumber<T>
        => new(Titles.ValidationFailed, "catalog.attribute-value-must-be-at-most", attrKey, value, maxValue);
    /// <summary>
    /// Value '{1}' for attribute '{0}' not aligned to step '{2}'.
    /// </summary>
    public static DomainError AttrValueNotAlignedToStep<T>(string attrKey, T value, T step) where T : struct, INumber<T>
        => new(Titles.ValidationFailed, "catalog.attribute-value-not-aligned-to-step", attrKey, value, step);
    /// <summary>
    /// Value '{1}' for attribute '{0}' must match the pattern '{2}'.
    /// Value '{1}' for attribute '{0}' must not be empty.
    /// </summary>
    public static DomainError AttrValueMustBeValidString(string attrKey, string value, string? regex)
        => regex is not null
            ? new(Titles.ValidationFailed, "catalog.attribute-value-must-be-valid-string", attrKey, value, regex)
            : new(Titles.ValidationFailed, "catalog.attribute-value-must-be-non-empty-string", attrKey, value);
    /// <summary>
    /// Key '{0}' must be in lower_snake format.
    /// </summary>
    /// <returns></returns>
    public static DomainError KeyFormatInvalid(string key) => new(Titles.ValidationFailed, "catalog.common.key-format-invalid", key);

    public static class Titles
    {
        public const string ValidationFailed = "error.validation_failed";
        public const string CannotApplyOperation = "error.cannot_apply_operation";
        public const string NotFound = "error.not_found";
        public const string ResourceConflict = "error.resource_conflict";
    }
}
