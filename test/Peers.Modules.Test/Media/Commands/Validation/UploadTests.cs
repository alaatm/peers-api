using FluentValidation.TestHelper;
using Peers.Core.Http;
using Peers.Modules.Media.Commands;

namespace Peers.Modules.Test.Media.Commands.Validation;

public class UploadTests : CommandValidatorTestBase<Upload.Command, Upload.Validator>
{
    [Fact]
    public void ValidateTest()
    {
        Check(GetValidCommand(), p => p.TargetId, null, false);
        Check(GetValidCommand(), p => p.TargetId, 0, true);

        CheckNotNull(p => p.Metadata);
        Check(GetValidCommand(), p => p.Metadata, new Dictionary<string, Upload.Command.FileMetadata>(), true);

        CheckNotNull(p => p.Files);
        Check(GetValidCommand(), p => p.Files, Array.Empty<FormFile>(), true);

        var cmd = GetValidCommand() with { Files = [null] };
        Validator.TestValidate(cmd).ShouldHaveValidationErrorFor(p => p.Files);

        cmd = GetValidCommand() with { Files = [new FormFile() { Name = "", ContentType = "image/jpeg" }] };
        Validator.TestValidate(cmd).ShouldHaveValidationErrorFor("Files[0]");

        cmd = GetValidCommand() with { Files = [new FormFile() { Name = "file1", ContentType = "zx" }] };
        Validator.TestValidate(cmd).ShouldHaveValidationErrorFor("Files[0]");

        CheckOk();
    }

    protected override Upload.Command GetValidCommand() => Handlers.UploadTests.TestUploadCommand();
}
