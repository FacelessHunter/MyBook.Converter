# DataConvertor

A utility for converting book data from JSON to EPUB and FB2 formats using the MyBook.Writer library.

## Features

- Converts scraped book data to EPUB or FB2 format
- Comprehensive logging and performance diagnostics
- Configurable output formats and settings

## Logging System

DataConvertor integrates Microsoft.Extensions.Logging for detailed logging:

### Log Levels

The application supports the following log levels:

- **Trace**: Detailed diagnostics (very verbose)
- **Debug**: Useful for development debugging
- **Information**: General operational information (default)
- **Warning**: Non-critical issues
- **Error**: Errors that need attention
- **Critical**: Critical failures

### Configuration

Logging is configured through the `ConfigurationService` and `LoggingService` classes:

```csharp
// Set custom log level
var loggerFactory = LoggingService.ConfigureLogging(LogLevel.Debug);
```

### Performance Tracking

The application uses the DiagnosticService to track operation performance:

```csharp
// Output performance metrics
foreach (var operation in diagnostics.GetOperationDurations())
{
    logger.LogInformation("Operation: {Operation}, Duration: {Duration}ms", 
        operation.Key, operation.Value);
}
```

### Integration with MyBook.Writer

The logging system is fully integrated with the MyBook.Writer library to provide detailed insights into the book creation process:

- Book building steps
- Content processing
- File operations

## Usage

The DataConvertor reads JSON book data and converts it to the specified format:

1. Configure output format in `ConfigurationService.Defaults`
2. Set logging level as needed
3. Run the application
4. Performance metrics will be output upon completion

## Extending

You can extend the logging functionality:

- Create custom log providers
- Implement additional diagnostic metrics
- Add custom logging for specific operations

## Dependencies

- Microsoft.Extensions.Logging
- MyBook.Writer
- SharedModels 