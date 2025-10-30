using Peers.Modules.Catalog.Domain.Attributes;

namespace Peers.Modules.Listings.Domain.Snapshots;

public sealed record HeaderAttrSnapshot(
    string DefinitionKey,
    AttributeKind Kind,
    string Value
);
