using Peers.Core.Cqrs.Pipeline;
using Peers.Modules.Catalog.Domain.Attributes;
using Peers.Modules.Catalog.Domain.Translations;

namespace Peers.Modules.Catalog.Queries;

public static class GetProductTypeDetails
{
    /// <summary>
    /// Retrieving a catalog item by its unique identifier.
    /// </summary>
    /// <param name="Id">The unique identifier of the catalog item to retrieve.</param>
    [Authorize(Roles = $"{Roles.CatalogManager}, {Roles.Seller}")]
    public sealed record Query(int Id) : IQuery, IValidatable;

    public sealed class Validator : AbstractValidator<Query>
    {
        public Validator([NotNull] IStrLoc l)
            => RuleFor(p => p.Id).GreaterThan(0);
    }

    /// <summary>
    /// Represents the response containing product type information and associated attribute definitions.
    /// </summary>
    /// <param name="Info">The product type information returned by the ListProductTypes operation.</param>
    /// <param name="AttributeDefinitions">An array of attribute definitions associated with the product type. Each definition describes a specific
    /// attribute, including its key, display names, kind, and options.</param>
    public sealed record Response(
        ListProductTypes.Response Info,
        Response.AttributeDefinitionDto[] AttributeDefinitions)
    {
        public Response() : this(default!, default!) { }

        /// <summary>
        /// Represents the definition of an attribute, including its key, localized names, type, and configuration
        /// options.
        /// </summary>
        /// <param name="Id">The unique identifier of the attribute definition.</param>
        /// <param name="GroupDefinitionId">The unique identifier of the group definition to which this attribute belongs, if applicable; otherwise, null.</param>
        /// <param name="Key">The unique key that identifies the attribute definition. Cannot be null.</param>
        /// <param name="Names">An array of localized names for the attribute. Each entry provides a display name in a specific language.
        /// Cannot be null.</param>
        /// <param name="Kind">The kind of attribute, indicating its data type or category.</param>
        /// <param name="IsRequired">Indicates whether the attribute is required. Set to <see langword="true"/> if the attribute must have a
        /// value; otherwise, <see langword="false"/>.</param>
        /// <param name="IsVariant">Indicates whether the attribute can vary between different variants of an entity. Set to <see
        /// langword="true"/> if the attribute is variant-specific; otherwise, <see langword="false"/>.</param>
        /// <param name="Options">An array of available options for the attribute, if applicable. May be null if the attribute does not support options.</param>
        /// <param name="Members">An array of member attribute definitions if this attribute is a group; otherwise, null.</param>
        public sealed record AttributeDefinitionDto(
            int Id,
            int? GroupDefinitionId,
            string Key,
            AttributeDefinitionTr.Dto[] Names,
            AttributeKind Kind,
            bool IsRequired,
            bool IsVariant,
            AttributeOptionDto[]? Options,
            AttributeDefinitionDto[]? Members)
        {
            public AttributeDefinitionDto() : this(default, default, default!, default!, default!, default!, default, default, default) { }
        }

        /// <summary>
        /// Represents a data transfer object for an attribute option, including its code, optional parent code, and
        /// localized display names.
        /// </summary>
        /// <param name="Id">The unique identifier of the attribute option.</param>
        /// <param name="Code">The unique code that identifies the attribute option. Cannot be null.</param>
        /// <param name="ParentCode">The code of the parent attribute option, if any; otherwise, null.</param>
        /// <param name="Names">An array of localized display names for the attribute option. Cannot be null.</param>
        public sealed record AttributeOptionDto(
            int Id,
            string Code,
            string? ParentCode,
            EnumAttributeOptionTr.Dto[] Names)
        {
            public AttributeOptionDto() : this(default, default!, default, default!) { }
        }
    }

    public sealed class Handler : ICommandHandler<Query>
    {
        private readonly PeersContext _context;

        public Handler(PeersContext context) => _context = context;

        public async Task<IResult> Handle([NotNull] Query cmd, CancellationToken ctk)
        {
            var result = await _context
                .ProductTypes
                .Where(p => p.Id == cmd.Id)
                .Select(p => new Response
                {
                    Info = new ListProductTypes.Response
                    {
                        Id = p.Id,
                        ParentId = p.ParentId,
                        Names = p.Translations.Select(p => new ProductTypeTr.Dto { LangCode = p.LangCode, Name = p.Name }).ToArray(),
                        SlugPath = p.SlugPath,
                        Version = p.Version,
                        State = p.State,
                        Kind = p.Kind,
                    },
                    AttributeDefinitions = p.Attributes
                        .OrderBy(p => p.Position)
                        .Select(p => new Response.AttributeDefinitionDto
                        {
                            Id = p.Id,
                            GroupDefinitionId = (p as NumericAttributeDefinition)!.GroupDefinitionId,
                            Key = p.Key,
                            Names = p.Translations.Select(t => new AttributeDefinitionTr.Dto { LangCode = t.LangCode, Name = t.Name, Unit = t.Unit }).ToArray(),
                            Kind = p.Kind,
                            IsRequired = p.IsRequired,
                            IsVariant = p.IsVariant,
                            Options = p.Kind == AttributeKind.Enum
                                ? ((EnumAttributeDefinition)p).Options
                                    .OrderBy(p => p.Position)
                                    .Select(o => new Response.AttributeOptionDto
                                    {
                                        Id = o.Id,
                                        Code = o.Code,
                                        ParentCode = o.ParentOption!.Code,
                                        Names = o.Translations.Select(t => new EnumAttributeOptionTr.Dto { LangCode = t.LangCode, Name = t.Name }).ToArray()
                                    }).ToArray()
                                : null,
                        }).ToArray()
                })
                .FirstOrDefaultAsync(ctk);

            // Post-processing to populate member attributes for group definitions
            if (result is not null)
            {
                var removeList = new List<Response.AttributeDefinitionDto>();

                for (var i = 0; i < result.AttributeDefinitions.Length; i++)
                {
                    var def = result.AttributeDefinitions[i];
                    if (def.Kind is AttributeKind.Group)
                    {
                        var members = result.AttributeDefinitions.Where(a => a.GroupDefinitionId == def.Id).ToArray();
                        result.AttributeDefinitions[i] = def with { Members = members };
                        removeList.AddRange(members);
                    }
                }

                // Remove member attributes from the top-level list
                result = result with
                {
                    AttributeDefinitions = [.. result.AttributeDefinitions.Except(removeList)]
                };
            }

            return result is null
                ? Result.NotFound()
                : Result.Ok(result);
        }
    }
}
