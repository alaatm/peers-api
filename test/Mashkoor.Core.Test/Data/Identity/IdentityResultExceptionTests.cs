using Mashkoor.Core.Data.Identity;

namespace Mashkoor.Core.Test.Data.Identity;

public class IdentityResultExceptionTests
{
    [Fact]
    public void Ctor_formats_errors_into_message()
    {
        // Arrange
        var nl = Environment.NewLine;

        // Act
        var ex = new IdentityResultException(["err1", "err2"]);

        // Assert
        Assert.Equal($"{nl}err1{nl}err2", ex.Message);
    }

    [Fact]
    public void Unused_ctors_throw()
    {
        Assert.Throws<NotImplementedException>(() => new IdentityResultException());
        Assert.Throws<NotImplementedException>(() => new IdentityResultException("0"));
        Assert.Throws<NotImplementedException>(() => new IdentityResultException("0", new InvalidOperationException()));
    }
}
