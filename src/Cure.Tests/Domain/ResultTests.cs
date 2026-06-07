using Cure.Domain.Common;
using FluentAssertions;

namespace Cure.Tests.Domain;

public class ResultTests
{
    [Fact]
    public void Success_ShouldHaveIsSuccessTrue()
    {
        // Arrange & Act
        var result = Result.Success();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.Error.Should().Be(Error.None);
    }

    [Fact]
    public void Failure_ShouldHaveIsFailureTrueAndContainError()
    {
        // Arrange
        var error = new Error("Test.Error", "Something went wrong.");

        // Act
        var result = Result.Failure(error);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(error);
    }

    [Fact]
    public void GenericSuccess_ShouldContainValue()
    {
        // Arrange
        var expectedValue = 42;

        // Act
        var result = Result<int>.Success(expectedValue);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(expectedValue);
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void GenericFailure_ShouldThrowWhenAccessingValue()
    {
        // Arrange
        var error = new Error("Test.Error", "Value access denied.");
        var result = Result<int>.Failure(error);

        // Act
        var act = () => result.Value;

        // Assert
        result.IsFailure.Should().BeTrue();
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void ValidationFailure_ShouldContainMultipleErrors()
    {
        // Arrange
        var errors = new[]
        {
            new Error("Field.Required", "Name is required."),
            new Error("Field.Invalid", "Email format is invalid.")
        };

        // Act
        var result = Result<string>.ValidationFailure(errors);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().HaveCount(2);
        result.Errors[0].Code.Should().Be("Field.Required");
        result.Errors[1].Code.Should().Be("Field.Invalid");
        result.Error.Should().Be(errors[0]);
    }
}
