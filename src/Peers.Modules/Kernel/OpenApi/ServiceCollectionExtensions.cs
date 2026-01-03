namespace Peers.Modules.Kernel.OpenApi;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds OpenAPI services related to the default document with several app-specific transformers.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddOpenApiWithTransformers(this IServiceCollection services) => services
        .AddOpenApi(o =>
        {
            o.CreateSchemaReferenceId = typeInfo =>
            {
                var t = typeInfo.Type;
                return SchemaIdBuilder.Build(t);
            };
            o.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
            o.AddOperationTransformer<AuthorizeOperationTransformer>();
            o.AddOperationTransformer<MultipartJsonEncodingTransformer>();
            o.AddSchemaTransformer<EnumNamesTransformer>();
            o.AddSchemaTransformer<DictionaryKeysTransformer>();
            o.AddSchemaTransformer<PointSchemaTransformer>();
            o.AddSchemaTransformer<AttributeInputDtoSchemaTransformer>();
        });
}
