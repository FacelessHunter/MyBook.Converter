# MyBook.Writer Tests

This project contains tests for the MyBook.Writer library, which allows for creating e-books in EPUB and FB2 formats.

## Test Categories

The test project is organized into the following test categories:

1. **BookFactoryTests**: Tests for the `BookFactory` class which creates book builders based on the desired format.

2. **BookBuilderTests**: Tests for the book builder implementations (EPUB and FB2) to ensure the fluent API works correctly.

3. **IntegrationTests**: End-to-end tests that validate the full workflow of creating, building, and saving books.

4. **ExampleTests**: Tests for the example code in the `BookWriterExample` class to ensure the documented usage examples work.

## Test Helpers

The project includes test helper classes to provide substitutions for common dependencies:

1. **TestLogger<T> and TestLogger**: Simple implementations of `ILogger<T>` and `ILogger` that record log messages for verification.

2. **TestLoggerFactory**: An implementation of `ILoggerFactory` that creates and tracks `TestLogger` instances.

## Running Tests

Run tests using any of the following methods:

### Command Line

```bash
dotnet test
```

### Visual Studio 

1. Open the solution in Visual Studio
2. Use the Test Explorer to run all or selected tests

## Adding New Tests

When adding new tests, follow these guidelines:

1. **Choose the appropriate test class** or create a new one if the test doesn't fit into an existing category.

2. **Follow the Arrange-Act-Assert (AAA) pattern** to structure your tests.

3. **Use descriptive test names** that clearly describe what is being tested.

4. **Handle file resources properly**:
   - Create temporary files in unique locations using `Path.GetTempPath()` and `Guid.NewGuid()`
   - Always clean up files in a `finally` block
   - Handle potential IO exceptions during cleanup
   - Dispose streams properly using `using` statements

5. **Using Substitutions Instead of Mocks**:
   - Use the provided test helpers (`TestLogger`, `TestLoggerFactory`) for interfaces like `ILogger` and `ILoggerFactory`
   - For other interfaces, create simple substitution classes that implement the required interface
   - Record important calls/information in your substitutions for later verification
   - Avoid complex mocking libraries to keep tests simple and maintainable

## Test Coverage

The current test suite covers:

- Creation of book builders via the factory pattern
- Fluent API for building books
- Building and saving books to files and streams
- Error handling for invalid inputs
- Example code validation

## Adding More Test Coverage

Consider adding tests for:

1. **Invalid inputs**: Test more edge cases with null or invalid parameters.

2. **Content validation**: Verify that the generated book content matches the expected structure.

3. **Performance testing**: Test the library with larger books to ensure good performance.

4. **Concurrency testing**: Test multiple book builds happening simultaneously.

5. **Compatibility testing**: Test with different versions of .NET if applicable. 