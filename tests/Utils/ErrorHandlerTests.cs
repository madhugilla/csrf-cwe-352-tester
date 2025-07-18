using CSRFTester.Utils;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CSRFTester.Tests.Utils;

public class ErrorHandlerTests
{
    private readonly Mock<ILogger<ErrorHandler>> _mockLogger;
    private readonly ErrorHandler _errorHandler;

    public ErrorHandlerTests()
    {
        _mockLogger = new Mock<ILogger<ErrorHandler>>();
        _errorHandler = new ErrorHandler(_mockLogger.Object);
    }

    [Fact]
    public void HandleException_WithException_ShouldLogErrorAndDisplayToConsole()
    {
        // Arrange
        var exception = new InvalidOperationException("Test exception message");
        var context = "Test context";

        var originalOut = Console.Out;
        var stringWriter = new StringWriter();
        Console.SetOut(stringWriter);

        try
        {
            // Act
            _errorHandler.HandleException(exception, context);

            // Assert
            var output = stringWriter.ToString();
            Assert.Contains("Error: Test exception message", output);
            Assert.Contains("Context: Test context", output);

            // Verify logging
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Test context")),
                    exception,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
        finally
        {
            Console.SetOut(originalOut);
        }
    }

    [Fact]
    public void HandleException_WithoutContext_ShouldLogErrorWithoutContext()
    {
        // Arrange
        var exception = new ArgumentException("Argument exception");

        var originalOut = Console.Out;
        var stringWriter = new StringWriter();
        Console.SetOut(stringWriter);

        try
        {
            // Act
            _errorHandler.HandleException(exception);

            // Assert
            var output = stringWriter.ToString();
            Assert.Contains("Error: Argument exception", output);
            Assert.DoesNotContain("Context:", output);

            // Verify logging
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    exception,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
        finally
        {
            Console.SetOut(originalOut);
        }
    }

    [Fact]
    public void HandleException_WithEmptyContext_ShouldNotDisplayContext()
    {
        // Arrange
        var exception = new FileNotFoundException("File not found");
        var context = "";

        var originalOut = Console.Out;
        var stringWriter = new StringWriter();
        Console.SetOut(stringWriter);

        try
        {
            // Act
            _errorHandler.HandleException(exception, context);

            // Assert
            var output = stringWriter.ToString();
            Assert.Contains("Error: File not found", output);
            Assert.DoesNotContain("Context:", output);
        }
        finally
        {
            Console.SetOut(originalOut);
        }
    }

    [Fact]
    public void HandleWarning_WithMessage_ShouldLogWarningAndDisplayToConsole()
    {
        // Arrange
        var message = "This is a warning message";
        var context = "Warning context";

        var originalOut = Console.Out;
        var stringWriter = new StringWriter();
        Console.SetOut(stringWriter);

        try
        {
            // Act
            _errorHandler.HandleWarning(message, context);

            // Assert
            var output = stringWriter.ToString();
            Assert.Contains("Warning: This is a warning message", output);

            // Verify logging
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("This is a warning message") && v.ToString()!.Contains("Warning context")),
                    It.IsAny<Exception?>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
        finally
        {
            Console.SetOut(originalOut);
        }
    }

    [Fact]
    public void HandleWarning_WithoutContext_ShouldLogWarningWithoutContext()
    {
        // Arrange
        var message = "Simple warning";

        var originalOut = Console.Out;
        var stringWriter = new StringWriter();
        Console.SetOut(stringWriter);

        try
        {
            // Act
            _errorHandler.HandleWarning(message);

            // Assert
            var output = stringWriter.ToString();
            Assert.Contains("Warning: Simple warning", output);

            // Verify logging
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Simple warning")),
                    It.IsAny<Exception?>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
        finally
        {
            Console.SetOut(originalOut);
        }
    }

    [Fact]
    public void HandleInfo_WithMessage_ShouldLogInfoAndDisplayToConsole()
    {
        // Arrange
        var message = "Information message";

        var originalOut = Console.Out;
        var stringWriter = new StringWriter();
        Console.SetOut(stringWriter);

        try
        {
            // Act
            _errorHandler.HandleInfo(message);

            // Assert
            var output = stringWriter.ToString();
            Assert.Contains("Info: Information message", output);

            // Verify logging
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Information message")),
                    It.IsAny<Exception?>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
        finally
        {
            Console.SetOut(originalOut);
        }
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void HandleInfo_WithEmptyMessage_ShouldHandleGracefully(string? message)
    {
        // Arrange
        var originalOut = Console.Out;
        var stringWriter = new StringWriter();
        Console.SetOut(stringWriter);

        try
        {
            // Act & Assert - Should not throw
            _errorHandler.HandleInfo(message!);

            var output = stringWriter.ToString();
            Assert.Contains("Info:", output);
        }
        finally
        {
            Console.SetOut(originalOut);
        }
    }
}