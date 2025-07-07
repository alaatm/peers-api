using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Mashkoor.Core.Common;
using Mashkoor.Core.Http;
using static Mashkoor.Core.Test.TestHttpClient;

namespace Mashkoor.Core.Test.Http;

public class HttpRequestExtensionsTests
{
    [Fact]
    public async Task BindFormDataAsync_throws_for_non_form_data_content()
    {
        // Arrange
        var client = await GetTestClientAsync(
            epCgf: ep => ep.MapPost("/data", async (ctx) =>
            {
                // Assert
                var ex = await Assert.ThrowsAsync<InvalidOperationException>(async () => await ctx.Request.BindFormDataAsync<Model>());
                Assert.Equal("BindFormDataAsync<T>() can only be used with form data.", ex.Message);
            }));

        // Act
        var result = await client.PostAsJsonAsync("/data", new { Name = "Test" });
    }

    [Fact]
    public async Task BindFormDataAsync_throws_for_invalid_form_data_name()
    {
        // Arrange
        var client = await GetTestClientAsync(
            epCgf: ep => ep.MapPost("/data", async (ctx) =>
            {
                // Assert
                var ex = await Assert.ThrowsAsync<InvalidOperationException>(async () => await ctx.Request.BindFormDataAsync<Model>());
                Assert.Equal(
                    "BindFormDataAsync<T>() form data must have a 'data' key with json content.",
                    ex.Message);
            }));

        var content = new MultipartFormDataContent
        {
            { new StringContent(JsonSerializer.Serialize(new { Name = "Test" }, GlobalJsonOptions.Default), Encoding.UTF8, "application/json"), "invalid" },
        };

        // Act
        var result = await client.PostAsync("/data", content);
    }

    [Fact]
    public async Task BindFormDataAsync_throws_for_invalid_json_data()
    {
        // Arrange
        var client = await GetTestClientAsync(
            epCgf: ep => ep.MapPost("/data", async (ctx) =>
            {
                // Assert
                var ex = await Assert.ThrowsAsync<InvalidOperationException>(async () => await ctx.Request.BindFormDataAsync<Model>());
                Assert.Equal(
                    "BindFormDataAsync<T>() form data 'data' key value must be a valid json string.",
                    ex.Message);
            }));

        var content = new MultipartFormDataContent
        {
            { new StringContent("invalid json", Encoding.UTF8, "application/json"), "data" },
        };

        // Act
        var result = await client.PostAsync("/data", content);
    }

    [Fact]
    public async Task BindFormDataAsync_throws_when_no_file_content_is_set()
    {
        // Arrange
        var client = await GetTestClientAsync(
            epCgf: ep => ep.MapPost("/data", async (ctx) =>
            {
                // Assert
                var ex = await Assert.ThrowsAsync<InvalidOperationException>(async () => await ctx.Request.BindFormDataAsync<Model>());
                Assert.Equal(
                    "BindFiles() no content type was set for file 'subFile1'.",
                    ex.Message);
            }));

        var content = new MultipartFormDataContent
        {
            { new StringContent(JsonSerializer.Serialize(new { Name = "Test" }, GlobalJsonOptions.Default), Encoding.UTF8, "application/json"), "data" },
            { new ByteArrayContent([1, 1]), "subFile1", "subFile1" },
        };

        // Act
        var result = await client.PostAsync("/data", content);
    }

    [Theory]
    [InlineData("subFile1")]
    [InlineData("subFile1.")]
    public async Task BindFormDataAsync_throws_when_no_file_extension_is_set(string filename)
    {
        // Arrange
        var client = await GetTestClientAsync(
            epCgf: ep => ep.MapPost("/data", async (ctx) =>
            {
                // Assert
                var ex = await Assert.ThrowsAsync<InvalidOperationException>(async () => await ctx.Request.BindFormDataAsync<Model>());
                Assert.Equal(
                    $"BindFiles() the file '{filename}' has no extension.",
                    ex.Message);
            }));

        var content = new MultipartFormDataContent
        {
            { new StringContent(JsonSerializer.Serialize(new { Name = "Test" }, GlobalJsonOptions.Default), Encoding.UTF8, "application/json"), "data" },
            { GetImageContent([1, 1]), "subFile1", filename },
        };

        // Act
        var result = await client.PostAsync("/data", content);
    }

    [Fact]
    public async Task BindFormDataAsync_binds_form_data_with_files()
    {
        // Arrange
        Model model = null;
        List<(string name, string descr)> nameAndDesc = null;
        var f1 = new Memory<byte>(new byte[2]);
        var f2 = new Memory<byte>(new byte[2]);

        var client = await GetTestClientAsync(
            epCgf: ep =>
                ep.MapPost("/data", async (ctx) =>
                {
                    var data = await ctx.Request.BindFormDataAsync<Model>();
                    model = data.Data;
                    nameAndDesc = [.. data.Files.Select(f => (f.Name, f.Description))];
                    await data.Files[0].Stream.ReadExactlyAsync(f1);
                    await data.Files[1].Stream.ReadExactlyAsync(f2);
                })
            );

        var content = new MultipartFormDataContent
        {
            { new StringContent(JsonSerializer.Serialize(new { Name = "Test" }, GlobalJsonOptions.Default), Encoding.UTF8, "application/json"), "data" },
            { GetImageContent([1, 1]), "Description for file #1", "subFile1.abc" },
            { GetImageContent([2, 2]), "Description for file #2", "subFile2.xyz" }
        };

        // Act
        var result = await client.PostAsync("/data", content);

        // Assert
        result.EnsureSuccessStatusCode();
        Assert.NotNull(model);
        Assert.Equal("Test", model.Name);
        Assert.Equal("subFile1.abc", nameAndDesc[0].name);
        Assert.Equal("Description for file #1", nameAndDesc[0].descr);
        Assert.Equal("subFile2.xyz", nameAndDesc[1].name);
        Assert.Equal("Description for file #2", nameAndDesc[1].descr);
        Assert.Equal(f1.ToArray(), [1, 1,]);
        Assert.Equal(f2.ToArray(), [2, 2,]);
    }

    [Fact]
    public async Task BindFormDataAsync_binds_form_data_without_files()
    {
        // Arrange
        FormData<Model> data = null;

        var client = await GetTestClientAsync(
            epCgf: ep =>
                ep.MapPost("/data", async (ctx) => data = await ctx.Request.BindFormDataAsync<Model>())
            );

        var content = new MultipartFormDataContent
        {
            { new StringContent(JsonSerializer.Serialize(new { Name = "Test" }, GlobalJsonOptions.Default), Encoding.UTF8, "application/json"), "data" },
        };

        // Act
        var result = await client.PostAsync("/data", content);

        // Assert
        result.EnsureSuccessStatusCode();
        Assert.NotNull(data);
        Assert.Equal("Test", data.Data.Name);
        Assert.Empty(data.Files);
    }

    [Fact]
    public void FormFile_with_clones_instance_with_override_values()
    {
        // Arrange
        var ff = new FormFile
        {
            Name = "name",
            Description = "description",
            ContentType = "image/heif",
            Stream = new MemoryStream(),
        };

        // Act and assert
        var ff1 = ff.With(name: "name2");
        Assert.NotSame(ff, ff1);
        Assert.Equal("name2", ff1.Name);
        Assert.Equal(ff.Description, ff1.Description);
        Assert.Equal(ff.ContentType, ff1.ContentType);
        Assert.Same(ff.Stream, ff1.Stream);

        var ff2 = ff.With(contentType: "audio/mp3");
        Assert.NotSame(ff, ff2);
        Assert.Equal(ff.Name, ff2.Name);
        Assert.Equal(ff.Description, ff2.Description);
        Assert.Equal("audio/mp3", ff2.ContentType);
        Assert.Same(ff.Stream, ff2.Stream);

        var stream = new MemoryStream();
        var ff3 = ff.With(stream: stream);
        Assert.NotSame(ff, ff3);
        Assert.Equal(ff.Name, ff3.Name);
        Assert.Equal(ff.Description, ff3.Description);
        Assert.Equal(ff.ContentType, ff3.ContentType);
        Assert.Same(stream, ff3.Stream);
    }

    private record Model(string Name);

    private static ByteArrayContent GetImageContent(byte[] bytes)
    {
        var bac = new ByteArrayContent(bytes);
        bac.Headers.ContentType = MediaTypeHeaderValue.Parse("image/jpeg");
        return bac;
    }
}
