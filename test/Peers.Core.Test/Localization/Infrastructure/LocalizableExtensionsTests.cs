using System.Globalization;
using Peers.Core.Domain;
using Peers.Core.Localization.Infrastructure;

namespace Peers.Core.Test.Localization.Infrastructure;

public class LocalizableExtensionsTests
{
    [Fact]
    public void Tr_ReturnsNull_WhenNoTranslations()
    {
        // Arrange
        var lang = "en";
        var entity = new TestEntity();

        // Act
        var currentCulture = Thread.CurrentThread.CurrentCulture;
        Thread.CurrentThread.CurrentCulture = new CultureInfo(lang);
        var result1 = entity.Tr();
        var result2 = entity.Tr(lang);
        Thread.CurrentThread.CurrentCulture = currentCulture;

        // Assert
        Assert.Null(result1);
        Assert.Null(result2);
    }

    [Fact]
    public void Tr_ReturnsNull_WhenLangCodeNotFound()
    {
        // Arrange
        var lang = "en";
        var entity = new TestEntity
        {
            Translations =
            {
                new TestEntityTr { LangCode = "ar", Content = "مرحبا" },
            }
        };

        // Act
        var currentCulture = Thread.CurrentThread.CurrentCulture;
        Thread.CurrentThread.CurrentCulture = new CultureInfo(lang);
        var result1 = entity.Tr();
        var result2 = entity.Tr(lang);
        Thread.CurrentThread.CurrentCulture = currentCulture;

        // Assert
        Assert.Null(result1);
        Assert.Null(result2);
    }

    [Fact]
    public void Tr_ReturnsTranslation_WhenLangCodeFound()
    {
        // Arrange
        var lang = "ar";
        var entity = new TestEntity
        {
            Translations =
            {
                new TestEntityTr { LangCode = "en", Content = "Hello" },
                new TestEntityTr { LangCode = "ar", Content = "مرحبا" },
            }
        };

        // Act
        var currentCulture = Thread.CurrentThread.CurrentCulture;
        Thread.CurrentThread.CurrentCulture = new CultureInfo(lang);
        var result1 = entity.Tr();
        var result2 = entity.Tr(lang);
        Thread.CurrentThread.CurrentCulture = currentCulture;

        // Assert
        Assert.NotNull(result1);
        Assert.Equal("مرحبا", result1!.Content);
        Assert.NotNull(result2);
        Assert.Equal("مرحبا", result2!.Content);
        _ = result1.EntityId; // Verify EntityId is accessible
    }

    [Fact]
    public void UpsertTranslations_Throws_WhenDuplicateLangCodesInIncoming()
    {
        // Arrange
        var entity = new TestEntity();
        var incoming = new[]
        {
            TestEntityTr.Dto.Create("en", "Hello"),
            TestEntityTr.Dto.Create("en", "Hi"), // Duplicate lang code
        };

        // Act & Assert
        var exception = Assert.Throws<TranslationValidationException>(() =>
            LocalizableExtensions.UpsertTranslations(entity, incoming));
        Assert.Contains("Duplicate language 'en'.", exception.Message);
    }

    [Fact]
    public void UpsertTranslations_Throws_WhenInvalidLangCodeInIncoming()
    {
        // Arrange
        var entity = new TestEntity();
        var incoming = new[]
        {
            TestEntityTr.Dto.Create("xx", "Hello"), // Invalid lang code
        };

        // Act & Assert
        var exception = Assert.Throws<TranslationValidationException>(() =>
            LocalizableExtensions.UpsertTranslations(entity, incoming));
        Assert.Contains("Unsupported language 'xx'.", exception.Message);
    }

    [Fact]
    public void UpsertTranslations_AddsTranslationsCorrectly()
    {
        // Arrange
        var entity = new TestEntity();
        var incoming = new[]
        {
            TestEntityTr.Dto.Create("en", "Hello"),
            TestEntityTr.Dto.Create("ar", "مرحبا"),
        };

        // Act
        LocalizableExtensions.UpsertTranslations(entity, incoming);

        // Assert
        Assert.Equal(2, entity.Translations.Count);
        Assert.Equal("Hello", entity.Tr("en")!.Content);
        Assert.Equal("مرحبا", entity.Tr("ar")!.Content);
    }

    [Fact]
    public void UpsertTranslations_UpdatesTranslationsCorrectly()
    {
        // Arrange
        var entity = new TestEntity
        {
            Translations =
            {
                new TestEntityTr { LangCode = "en", Content = "Hello" },
                new TestEntityTr { LangCode = "ar", Content = "مرحبا" },
            }
        };

        var incoming = new[]
        {
            TestEntityTr.Dto.Create("en", "Hi"), // Update existing
            TestEntityTr.Dto.Create("ar", "أهلا"), // Update existing
        };

        // Act
        LocalizableExtensions.UpsertTranslations(entity, incoming);

        // Assert
        Assert.Equal(2, entity.Translations.Count);
        Assert.Equal("Hi", entity.Tr("en")!.Content);
        Assert.Equal("أهلا", entity.Tr("ar")!.Content);
    }

    [Fact]
    public void UpsertTranslations_AddsAndUpdatesTranslationsCorrectly()
    {
        // Arrange
        var entity = new TestEntity
        {
            Translations =
            {
                new TestEntityTr { LangCode = "en", Content = "Hello" },
            }
        };

        var incoming = new[]
        {
            TestEntityTr.Dto.Create("en", "Hi"), // Update existing
            TestEntityTr.Dto.Create("ar", "مرحبا"), // Add new
        };

        // Act
        LocalizableExtensions.UpsertTranslations(entity, incoming);

        // Assert
        Assert.Equal(2, entity.Translations.Count);
        Assert.Equal("Hi", entity.Tr("en")!.Content);
        Assert.Equal("مرحبا", entity.Tr("ar")!.Content);
    }

    [Fact]
    public void WriteTranslations_GeneratesAllSupportedLanguages()
    {
        // Arrange
        var entity = new TestEntity
        {
            Translations =
            {
                new TestEntityTr { LangCode = "en", Content = "Hello" },
                new TestEntityTr { LangCode = "ar", Content = "مرحبا" },
            }
        };

        // Act
        LocalizableExtensions.WriteTranslations(entity, out TestEntityTr.Dto[] result);

        // Assert
        Assert.Equal(2, result.Length);
        Assert.Equal("en", result[0].LangCode);
        Assert.Equal("Hello", result[0].Content);
        Assert.Equal("ar", result[1].LangCode);
        Assert.Equal("مرحبا", result[1].Content);
    }

    [Fact]
    public void WriteTranslations_IncludesNullForMissingLanguages()
    {
        // Arrange
        var entity = new TestEntity
        {
            Translations =
            {
                new TestEntityTr { LangCode = "en", Content = "Hello" },
            }
        };

        // Act
        LocalizableExtensions.WriteTranslations(entity, out TestEntityTr.Dto[] result);

        // Assert
        Assert.Equal(2, result.Length);
        Assert.Equal("en", result[0].LangCode);
        Assert.Equal("Hello", result[0].Content);
        Assert.Equal("ar", result[1].LangCode);
        Assert.Null(result[1].Content); // Missing language
    }

    [Fact]
    public void WriteTranslationMap_GeneratesAllSupportedLanguages()
    {
        // Arrange
        var entity = new TestEntity
        {
            Translations =
            {
                new TestEntityTr { LangCode = "en", Content = "Hello" },
                new TestEntityTr { LangCode = "ar", Content = "مرحبا" },
            }
        };

        // Act
        LocalizableExtensions.WriteTranslationMap(entity, out Dictionary<string, TestEntityTr.Dto> result);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal("Hello", result["en"].Content);
        Assert.Equal("مرحبا", result["ar"].Content);
    }

    [Fact]
    public void WriteTranslationMap_IncludesNullForMissingLanguages()
    {
        // Arrange
        var entity = new TestEntity
        {
            Translations =
            {
                new TestEntityTr { LangCode = "en", Content = "Hello" },
            }
        };

        // Act
        LocalizableExtensions.WriteTranslationMap(entity, out Dictionary<string, TestEntityTr.Dto> result);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal("Hello", result["en"].Content);
        Assert.True(result.ContainsKey("ar"));
        Assert.Null(result["ar"].Content); // Missing language
    }

    private class TestEntity : Entity, ILocalizable<TestEntity, TestEntityTr>
    {
        public ICollection<TestEntityTr> Translations { get; } = [];
    }

    private class TestEntityTr : TranslationBase<TestEntity, TestEntityTr>
    {
        public string Content { get; set; } = string.Empty;

        public sealed class Dto : DtoBase
        {
            public string Content { get; set; } = default!;

            public override void ApplyTo(TestEntityTr target) => target.Content = Content;
            public override void ApplyFrom(TestEntityTr source) => Content = source.Content;
            public static Dto Create(string langCode, string content) => new() { LangCode = langCode, Content = content };
        }
    }
}
