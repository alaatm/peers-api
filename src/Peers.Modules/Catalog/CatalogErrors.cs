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
    /// Attribute kind must be 'Enum' to set as variant.
    /// </summary>
    public static DomainError VariantReqEnum => new(Titles.ValidationFailed, "catalog.variant-req-enum");
    /// <summary>
    /// Cannot remove attribute '{0}'; the following attributes depend on it: {1}.
    /// </summary>
    public static DomainError RemoveForbiddenHasDependants(string attrKey, string[] dependants)
        => new(Titles.CannotApplyOperation, "catalog.remove-forbidden-has-dependants", attrKey, LocalizationHelper.FormatList(dependants));
    /// <summary>
    /// Attribute '{0}' depends on '{1}'; every option must be scoped to a parent option of '{1}'.
    /// </summary>
    public static DomainError DepScopeReq(string attrKey, string parentAttrKey) => new(Titles.ValidationFailed, "catalog.dep-scope-req", attrKey, parentAttrKey);
    /// <summary>
    /// Attribute '{0}' has no dependency; options must not be scoped.
    /// </summary>
    public static DomainError ScopeForbiddenWithoutDep(string attrKey) => new(Titles.ValidationFailed, "catalog.scope-forbidden-without-dep", attrKey);
    /// <summary>
    /// Attribute '{0}' depends on '{1}', but option '{2}' is scoped to an option of '{3}'.
    /// </summary>
    public static DomainError InvalidScopeParent(string attrKey, string expectedParentAttrKey, string optionKey, string actualParentAttrKey)
        => new(Titles.ValidationFailed, "catalog.invalid-scope-parent", attrKey, expectedParentAttrKey, optionKey, actualParentAttrKey);
    /// <summary>
    /// Attribute '{0}' depends on '{1}', which belongs to a different product type.
    /// </summary>
    public static DomainError CrossPTAttrDep(string attrKey, string parentAttrKey)
        => new(Titles.ValidationFailed, "catalog.cross-pt-attr-dep", attrKey, parentAttrKey);
    /// <summary>
    /// Attribute '{0}' ({1}) cannot depend on '{2}' ({3}); only {1}→{1} is allowed.
    /// </summary>
    public static DomainError DependencyCombinationNotSupported(string childKey, AttributeKind childKind, string parentKey, AttributeKind parentKind)
        => new(Titles.ValidationFailed, "catalog.dependency-combination-not-supported", childKey, childKind.ToString(), parentKey, parentKind.ToString());

    /// <summary>
    /// Option '{0}' already exists.
    /// </summary>
    public static DomainError OptAlreadyExists(string key) => new(Titles.ResourceConflict, "catalog.opt-already-exists", key);
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
    public static DomainError OptNotFound(string key) => new(Titles.NotFound, "catalog.opt-not-found", key);
    /// <summary>
    /// Scope parent option belongs to a different attribute than the declared dependency.
    /// </summary>
    public static DomainError ParentOptMustBelongToDep => new(Titles.ValidationFailed, "catalog.parent-opt-must-belong-to-dep");
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
    /// The lookup value '{0}' was not found.
    /// </summary>
    public static DomainError LookupValueNotFound(string key) => new(Titles.NotFound, "catalog.lookup-value-not-found", key);
    /// <summary>
    /// The lookup value(s) {0} of type '{1}' is not in the allowed set declared by nearest ancestor '{2}'.
    /// </summary>
    public static DomainError LookupValuesNotAllowedByAncestor(string[] valueKeys, string typeKey, string ancestorSlugPath)
        => new(Titles.ValidationFailed, "catalog.lookup-values-not-allowed-by-ancestor", LocalizationHelper.FormatList(valueKeys), typeKey, ancestorSlugPath);
    /// <summary>
    /// Duplicate allow-list entry(s) detected for lookup value(s): {0}. Each value may appear at most once.
    /// </summary>
    public static DomainError DuplicateAllowedLookupValues(string[] valueKeys)
        => new(Titles.ResourceConflict, "catalog.duplicate-allowed-lookup-values", LocalizationHelper.FormatList(valueKeys));
    /// <summary>
    /// The lookup type '{1}' used by attribute '{0}' requires an allow-list of permitted values on the product type, but none was provided.
    /// </summary>
    public static DomainError MissingLookupAllowList(string attrKey, string lookupTypeKey)
        => new(Titles.ValidationFailed, "catalog.missing-lookup-allow-list", attrKey, lookupTypeKey);

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
