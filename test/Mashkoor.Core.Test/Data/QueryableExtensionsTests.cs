using System.Text.Json;
using Mashkoor.Core.Common;
using Mashkoor.Core.Data;

namespace Mashkoor.Core.Test.Data;

public class QueryableExtensionsTests
{
    [Theory]
    [InlineData("[]")]
    [InlineData("[\"1\",\"2\"]")]
    public void ApplyFilters_throw_when_ops_count_is_not_an_odd_number(string ops)
    {
        // Arrange
        var models = Enumerable.Empty<Model>().AsQueryable();

        // Act & assert
        var ex = Assert.Throws<InvalidOperationException>(() => models.ApplyFilters("{\"myKey\":" + ops + "}"));
        Assert.Equal("Query filter parsing error at key 'myKey'. Invalid operations count.", ex.Message);
    }

    [Fact]
    public void ApplyFilters_throw_when_invalid_logic_op()
    {
        // Arrange
        var models = Enumerable.Empty<Model>().AsQueryable();

        // Act & assert
        var ex = Assert.Throws<InvalidOperationException>(() => models.ApplyFilters(/*lang=json,strict*/ "{\"myKey\":[\"eq(1)\",\"INVLD\",\"eq(2)\"]}"));
        Assert.Equal("Query filter parsing error at key 'myKey'. Invalid logic operator 'INVLD'.", ex.Message);
    }

    [Fact]
    public void ApplyFilters_throw_when_invalid_operation_regex_mismatch()
    {
        // Arrange
        var models = Enumerable.Empty<Model>().AsQueryable();

        // Act & assert
        var ex = Assert.Throws<InvalidOperationException>(() => models.ApplyFilters(/*lang=json,strict*/ "{\"myKey\":[\"???\"]}"));
        Assert.Equal("Query filter parsing error at key 'myKey'. Invalid operation '???'.", ex.Message);
    }

    [Fact]
    public void ApplyFilters_throw_when_invalid_operation_regex_mismatch_no_args()
    {
        // Arrange
        var models = Enumerable.Empty<Model>().AsQueryable();

        // Act & assert
        var ex = Assert.Throws<InvalidOperationException>(() => models.ApplyFilters(/*lang=json,strict*/ "{\"myKey\":[\"eq()\"]}"));
        Assert.Equal("Query filter parsing error at key 'myKey'. Invalid operation 'eq()'.", ex.Message);
    }

    [Fact]
    public void ApplyFilters_throw_when_invalid_operation()
    {
        // Arrange
        var models = Enumerable.Empty<Model>().AsQueryable();

        // Act & assert
        var ex = Assert.Throws<InvalidOperationException>(() => models.ApplyFilters(/*lang=json,strict*/ "{\"myKey\":[\"INVLD(1)\"]}"));
        Assert.Equal("Query filter parsing error at key 'myKey'. Invalid operation 'INVLD(1)'.", ex.Message);
    }

    [Fact]
    public void ApplyFilters_throw_when_invalid_filter_key()
    {
        // Arrange
        var models = Enumerable.Empty<Model>().AsQueryable();

        // Act & assert
        var ex = Assert.Throws<InvalidOperationException>(() => models.ApplyFilters(/*lang=json,strict*/ "{\"   \":[\"INVLD(1)\"]}"));
        Assert.Equal("Query filter parsing error. One or more keys are empty.", ex.Message);
    }

    [Theory]
    [InlineData("{}")]
    [InlineData("")]
    [InlineData("null")]
    [InlineData(null)]
    public void ApplyFilters_doesnt_filter_for_empty_or_null_filters(string filters)
    {
        // Arrange
        var models = new List<Model>
            {
                new(null, 10, "Hello", new DateTime(1900, 1, 1)),
                new(null, 20, "World", new DateTime(2000, 1, 1)),
            }.AsQueryable();

        // Act
        var q = models.ApplyFilters(filters);

        // Assert
        Assert.Equal(2, q.Count());
    }

    [Fact]
    public void ApplyFilters_startswith_test()
    {
        // Arrange
        var filters = new Dictionary<string, string[]>
        {
            ["Str"] = ["startswith(H)"],
        };

        var models = new List<Model>
            {
                new(null, 10, "Hello", new DateTime(1900, 1, 1)),
                new(null, 20, "World", new DateTime(2000, 1, 1)),
            }.AsQueryable();

        // Act
        var q = models.ApplyFilters(JsonSerializer.Serialize(filters, GlobalJsonOptions.Default));

        // Assert
        Assert.Equal(1, q.Count());
        Assert.Equal(models.First(), q.Single());
    }

    [Fact]
    public void ApplyFilters_contains_test()
    {
        // Arrange
        var filters = new Dictionary<string, string[]>
        {
            ["Str"] = ["contains(el)"],
        };

        var models = new List<Model>
            {
                new(null, 10, "Hello", new DateTime(1900, 1, 1)),
                new(null, 20, "World", new DateTime(2000, 1, 1)),
            }.AsQueryable();

        // Act
        var q = models.ApplyFilters(JsonSerializer.Serialize(filters, GlobalJsonOptions.Default));

        // Assert
        Assert.Equal(1, q.Count());
        Assert.Equal(models.First(), q.Single());
    }

    [Fact]
    public void ApplyFilters_endswith_test()
    {
        // Arrange
        var filters = new Dictionary<string, string[]>
        {
            ["Str"] = ["endswith(ld)"],
        };

        var models = new List<Model>
            {
                new(null, 10, "Hello", new DateTime(1900, 1, 1)),
                new(null, 20, "World", new DateTime(2000, 1, 1)),
            }.AsQueryable();

        // Act
        var q = models.ApplyFilters(JsonSerializer.Serialize(filters, GlobalJsonOptions.Default));

        // Assert
        Assert.Equal(1, q.Count());
        Assert.Equal(models.Skip(1).First(), q.Single());
    }

    [Fact]
    public void ApplyFilters_lt_test()
    {
        // Arrange
        var filters = new Dictionary<string, string[]>
        {
            ["Num"] = ["lt(10)"],
        };

        var models = new List<Model>
            {
                new(null, 10, "Hello", new DateTime(1900, 1, 1)),
                new(null, 20, "World", new DateTime(2000, 1, 1)),
            }.AsQueryable();

        // Act
        var q = models.ApplyFilters(JsonSerializer.Serialize(filters, GlobalJsonOptions.Default));

        // Assert
        Assert.Empty(q);
    }

    [Fact]
    public void ApplyFilters_le_test()
    {
        // Arrange
        var filters = new Dictionary<string, string[]>
        {
            ["Num"] = ["le(10)"],
        };

        var models = new List<Model>
            {
                new(null, 10, "Hello", new DateTime(1900, 1, 1)),
                new(null, 20, "World", new DateTime(2000, 1, 1)),
            }.AsQueryable();

        // Act
        var q = models.ApplyFilters(JsonSerializer.Serialize(filters, GlobalJsonOptions.Default));

        // Assert
        Assert.Equal(1, q.Count());
        Assert.Equal(models.First(), q.Single());
    }

    [Fact]
    public void ApplyFilters_eq_test()
    {
        // Arrange
        var filters = new Dictionary<string, string[]>
        {
            ["Dtt"] = ["eq(2000-01-01)"],
        };

        var models = new List<Model>
            {
                new(null, 10, "Hello", new DateTime(1900, 1, 1)),
                new(null, 20, "World", new DateTime(2000, 1, 1)),
            }.AsQueryable();

        // Act
        var q = models.ApplyFilters(JsonSerializer.Serialize(filters, GlobalJsonOptions.Default));

        // Assert
        Assert.Equal(1, q.Count());
        Assert.Equal(models.Skip(1).First(), q.Single());
    }

    [Fact]
    public void ApplyFilters_ge_test()
    {
        // Arrange
        var filters = new Dictionary<string, string[]>
        {
            ["Num"] = ["ge(10)"],
        };

        var models = new List<Model>
            {
                new(null, 10, "Hello", new DateTime(1900, 1, 1)),
                new(null, 20, "World", new DateTime(2000, 1, 1)),
            }.AsQueryable();

        // Act
        var q = models.ApplyFilters(JsonSerializer.Serialize(filters, GlobalJsonOptions.Default));

        // Assert
        Assert.Equal(2, q.Count());
    }

    [Fact]
    public void ApplyFilters_gt_test()
    {
        // Arrange
        var filters = new Dictionary<string, string[]>
        {
            ["Num"] = ["gt(10)"],
        };

        var models = new List<Model>
            {
                new(null, 10, "Hello", new DateTime(1900, 1, 1)),
                new(null, 20, "World", new DateTime(2000, 1, 1)),
            }.AsQueryable();

        // Act
        var q = models.ApplyFilters(JsonSerializer.Serialize(filters, GlobalJsonOptions.Default));

        // Assert
        Assert.Equal(1, q.Count());
        Assert.Equal(models.Skip(1).First(), q.Single());
    }

    [Fact]
    public void ApplyFilters_ne_test()
    {
        // Arrange
        var filters = new Dictionary<string, string[]>
        {
            ["Num"] = ["ne(10)"],
        };

        var models = new List<Model>
            {
                new(null, 10, "Hello", new DateTime(1900, 1, 1)),
                new(null, 20, "World", new DateTime(2000, 1, 1)),
            }.AsQueryable();

        // Act
        var q = models.ApplyFilters(JsonSerializer.Serialize(filters, GlobalJsonOptions.Default));

        // Assert
        Assert.Equal(1, q.Count());
        Assert.Equal(models.Skip(1).First(), q.Single());
    }

    [Theory]
    [InlineData("eq(true)", 1)]
    [InlineData("eq(false)", 0)]
    [InlineData("ne(true)", 0)]
    [InlineData("ne(false)", 1)]
    public void ApplyFilters_bool_test(string op, int expectedCount)
    {
        // Arrange
        var filters = new Dictionary<string, string[]>
        {
            ["Bool"] = [op],
        };

        var models = new List<Model>
            {
                new(null, 10, "Hello", new DateTime(1900, 1, 1), true),
            }.AsQueryable();

        // Act
        var q = models.ApplyFilters(JsonSerializer.Serialize(filters, GlobalJsonOptions.Default));

        // Assert
        Assert.Equal(expectedCount, q.Count());
    }

    [Fact]
    public void ApplyFilters_multi_filter_per_key_test_num()
    {
        // Arrange
        var filters = new Dictionary<string, string[]>
        {
            ["Num"] = ["gt(10)", "and", "le(30)", "or", "eq(40)"],
        };

        var models = new List<Model>
            {
                new(null, 10, "One", new DateTime(1900, 1, 1)),
                new(null, 20, "Two", new DateTime(2000, 1, 1)),
                new(null, 30, "Three", new DateTime(2100, 1, 1)),
                new(null, 40, "Four", new DateTime(2200, 1, 1)),
            }.AsQueryable();

        // Act
        var q = models.ApplyFilters(JsonSerializer.Serialize(filters, GlobalJsonOptions.Default));

        // Assert
        Assert.Equal(3, q.Count());
        Assert.True(models.Skip(1).SequenceEqual(q));
    }

    [Fact]
    public void ApplyFilters_multi_filter_per_key_test_str()
    {
        // Arrange
        var filters = new Dictionary<string, string[]>
        {
            ["Str"] = ["startswith(T)", "or", "eq(Four)"],
        };

        var models = new List<Model>
            {
                new(null, 10, "One", new DateTime(1900, 1, 1)),
                new(null, 20, "Two", new DateTime(2000, 1, 1)),
                new(null, 30, "Three", new DateTime(2100, 1, 1)),
                new(null, 40, "Four", new DateTime(2200, 1, 1)),
            }.AsQueryable();

        // Act
        var q = models.ApplyFilters(JsonSerializer.Serialize(filters, GlobalJsonOptions.Default));

        // Assert
        Assert.Equal(3, q.Count());
        Assert.True(models.Skip(1).SequenceEqual(q));
    }

    [Fact]
    public void ApplyFilters_multiple_filters_test()
    {
        // Arrange
        var filters = new Dictionary<string, string[]>
        {
            ["Num"] = ["ge(10)", "or", "eq(1)"],
            ["Str"] = ["contains(ell)"],
            ["Dtt"] = ["lt(1900-01-02)"],
        };

        var models = new List<Model>
            {
                new(null, 1, "Hello", new DateTime(1900, 1, 1)),
                new(null, 10, "Hello", new DateTime(1900, 1, 1)),
                new(null, 20, "Hello", new DateTime(1900, 1, 1)),
                new(null, 10, "Hello", new DateTime(1905, 1, 1)),
                new(null, 20, "Hello", new DateTime(1905, 1, 1)),
            }.AsQueryable();

        // Act
        var q = models.ApplyFilters(JsonSerializer.Serialize(filters, GlobalJsonOptions.Default));

        // Assert
        Assert.Equal(3, q.Count());
        Assert.True(models.Take(3).SequenceEqual(q));
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void ApplySorting_doesnt_sort_for_empty_or_null_sortField(string sortField)
    {
        // Arrange
        var models = new List<Model>
            {
                new(null, 10, "Hello", new DateTime(1900, 1, 1)),
                new(null, 20, "World", new DateTime(2000, 1, 1)),
            }.AsQueryable();

        // Act
        var q = models.ApplySorting(sortField, "descend");

        // Assert
        Assert.Equal(models.First(), q.First());
        Assert.Equal(models.Skip(1).First(), q.Skip(1).First());
    }

    [Fact]
    public void ApplySorting_defaults_to_asc_order()
    {
        // Arrange
        var models = new List<Model>
            {
                new(null, 20, "World", new DateTime(2000, 1, 1)),
                new(null, 10, "Hello", new DateTime(1900, 1, 1)),
            }.AsQueryable();

        // Act
        var q = models.ApplySorting("Num", null);

        // Assert
        Assert.Equal(models.First(), q.Skip(1).First());
        Assert.Equal(models.Skip(1).First(), q.First());
    }

    [Fact]
    public void ApplySorting_asc_test()
    {
        // Arrange
        var models = new List<Model>
            {
                new(null, 10, "Hello", new DateTime(1900, 1, 1)),
                new(null, 20, "World", new DateTime(2000, 1, 1)),
            }.AsQueryable();

        // Act
        var q = models.ApplySorting("Num", "ascend");

        // Assert
        Assert.Equal(models.First(), q.First());
        Assert.Equal(models.Skip(1).First(), q.Skip(1).First());
    }

    [Theory]
    [InlineData("desc")]
    [InlineData("descend")]
    public void ApplySorting_desc_test(string sortOrder)
    {
        // Arrange
        var models = new List<Model>
            {
                new(null, 10, "Hello", new DateTime(1900, 1, 1)),
                new(null, 20, "World", new DateTime(2000, 1, 1)),
            }.AsQueryable();

        // Act
        var q = models.ApplySorting("Num", sortOrder);

        // Assert
        Assert.Equal(models.First(), q.Skip(1).First());
        Assert.Equal(models.Skip(1).First(), q.First());
    }

    [Fact]
    public void ApplySorting_asc_nullable_test()
    {
        // Arrange
        var models = new List<Model>
            {
                new(10, 10, "Hello", new DateTime(1900, 1, 1)),
                new(20, 20, "World", new DateTime(2000, 1, 1)),
            }.AsQueryable();

        // Act
        var q = models.ApplySorting("NNum", "ascend");

        // Assert
        Assert.Equal(models.First(), q.First());
        Assert.Equal(models.Skip(1).First(), q.Skip(1).First());
    }

    [Fact]
    public void ApplySorting_desc_nullable_test()
    {
        // Arrange
        var models = new List<Model>
            {
                new(10, 10, "Hello", new DateTime(1900, 1, 1)),
                new(20, 20, "World", new DateTime(2000, 1, 1)),
            }.AsQueryable();

        // Act
        var q = models.ApplySorting("NNum", "descend");

        // Assert
        Assert.Equal(models.First(), q.Skip(1).First());
        Assert.Equal(models.Skip(1).First(), q.First());
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(null)]
    public void ApplyPaging_page_defaults_to_1_when_zero_or_negative_or_null(int? page)
    {
        // Arrange
        var models = Enumerable.Range(1, 50).Select(i => new Model(null, i, $"{i}", new DateTime(1900, 1, 1))).ToList().AsQueryable();

        // Act
        var q = models.ApplyPaging(page, 10);

        // Assert
        Assert.Equal(models.First(), q.First());
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(null)]
    public void ApplyPaging_pageSize_defaults_to_15_when_zero_or_negative_or_null(int? pageSize)
    {
        // Arrange
        var models = Enumerable.Range(1, 50).Select(i => new Model(null, i, $"{i}", new DateTime(1900, 1, 1))).ToList().AsQueryable();

        // Act
        var q = models.ApplyPaging(3, pageSize);

        // Assert
        Assert.Equal(models.Skip(30).First(), q.First());
        Assert.Equal(15, q.Count());
    }

    [Fact]
    public void ApplyPaging_applies_paging()
    {
        // Arrange
        var models = Enumerable.Range(1, 50).Select(i => new Model(null, i, $"{i}", new DateTime(1900, 1, 1))).ToList().AsQueryable();

        // Act
        var q = models.ApplyPaging(0, 5);

        // Assert
        Assert.Equal(5, q.Count());
    }

    internal class Model
    {
        public decimal? NNum { get; set; }
        public int Num { get; set; }
        public string Str { get; set; }
        public DateTime Dtt { get; set; }
        public bool Bool { get; set; }

        public Model(decimal? nn, int n, string s, DateTime d, bool b = false) => (NNum, Num, Str, Dtt, Bool) = (nn, n, s, d, b);
    }
}
