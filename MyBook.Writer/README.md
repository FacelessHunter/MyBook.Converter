# MyBook.Writer

A clean, modern library for creating e-books in EPUB and FB2 formats using .NET.

[![NuGet](https://img.shields.io/nuget/v/MyBook.Writer.svg)](https://www.nuget.org/packages/MyBook.Writer/)
[![Build and Publish](https://github.com/YourUsername/DataScraper/actions/workflows/nuget-publish.yml/badge.svg)](https://github.com/YourUsername/DataScraper/actions/workflows/nuget-publish.yml)

## Features

- Create EPUB and FB2 format e-books
- Fluent builder API for easy book creation
- Support for chapters, cover images, and metadata
- Works completely in memory - no temporary files
- Optional file-based output
- Factory pattern for format selection
- Clean and simple API with a single interface
- Integrated logging using Microsoft.Extensions.Logging

## Installation

```bash
dotnet add package MyBook.Writer
```

## Quick Start

### Creating an EPUB Book

```csharp
// Create a new EPUB builder
var builder = EpubBuilder.Create("My Book Title", "en")
    .WithAuthor("Author Name");

// Add a cover image
using (var coverStream = File.OpenRead("cover.jpg"))
{
    builder.WithCover(coverStream, "cover.jpg", "image/jpeg");
}

// Add chapters
builder.AddChapter(1, "Chapter 1", new List<string>
{
    "First paragraph of chapter 1.", 
    "Second paragraph with more text."
});

builder.AddChapter(2, "Chapter 2", new List<string>
{
    "Chapter 2 begins here.",
    "More content for chapter 2."
});

// Build and save
var writer = await builder.BuildAsync();
await writer.SaveToFileAsync("mybook.epub");
```

### Creating an FB2 Book

```csharp
// Create a new FB2 builder
var builder = FB2Builder.Create("My FB2 Book", "en")
    .WithAuthor("Author Name");

// Add cover
using (var coverStream = File.OpenRead("cover.jpg"))
{
    builder.WithCover(coverStream, "cover.jpg", "image/jpeg");
}

// Add chapters
builder.AddChapter(1, "Chapter 1", new[] { "Content for chapter 1" });
builder.AddChapter(2, "Chapter 2", new[] { "Content for chapter 2" });

// Build and save
var writer = await builder.BuildAsync();
await writer.SaveToFileAsync("mybook.fb2");
```

### Using the Book Factory (Format-Agnostic)

```csharp
// Create a book in any format with a simple interface
var format = BookFormat.Epub; // or BookFormat.FB2
IBookBuilder builder = BookFactory.CreateBuilder(format, "Book Title", "en");

// Use the fluent interface to configure the book
builder.WithAuthor("Author Name")
       .AddChapter(1, "Chapter", new[] { "Content" });

// Build and save
var writer = await builder.BuildAsync();
await writer.SaveAsStreamAsync(); // Get as stream
// or
await writer.SaveToFileAsync($"book.{format.ToString().ToLower()}");
```

## Extension Points

- Implement `IBookWriter` to add support for additional formats
- Create new implementations of `IBookBuilder` for custom building processes
- All format-specific builders implement the common interface for consistent API

## Architecture

MyBook.Writer follows a clean architecture with distinct layers:

- **Core**: Domain models and interfaces
- **Builders**: Fluent builders for different formats
- **Writers**: Format-specific implementations 

### Interface Design

The library uses a simplified interface design:

- `IBookBuilder`: Common interface for all book builders
- `IBookWriter`: Common interface for all book writers
- `BookFactory`: Factory class to create the appropriate builder

This design makes it easy to use the library with a consistent API regardless of the book format you're targeting.

## Logging

MyBook.Writer includes comprehensive logging capabilities for both EPUB and FB2 formats. The library uses Microsoft.Extensions.Logging to provide detailed information about its operations.

### Setting Up Logging

```csharp
using Microsoft.Extensions.Logging;
using MyBook.Writer.Core.Services;

// Create a logger factory
using var loggerFactory = LoggerFactory.Create(builder =>
{
    builder
        .AddFilter("MyBook.Writer", LogLevel.Debug) // Set minimum level
        .AddConsole() // Log to console
        .AddDebug();  // Log to debug output
});

// Configure the book writer library to use our logger factory
loggerFactory.ConfigureBookWriter();
```

### Format-Specific Logging

Both EPUB and FB2 formats provide detailed logging throughout the creation process:

#### EPUB Format Logging
- Book building process, including each file's creation
- Content serialization and packaging details
- XML document creation for EPUB structure
- Container creation and metadata handling
- Chapter creation with detailed stats

#### FB2 Format Logging
- Book metadata and content creation
- XML document structure and content
- Image handling and embedding
- Chapter and paragraph processing details

### Detailed Logging Control

You can adjust the logging verbosity to get more detailed information:

```csharp
// For normal operation
builder.AddFilter("MyBook.Writer", LogLevel.Information);

// For debugging
builder.AddFilter("MyBook.Writer", LogLevel.Debug);

// For detailed tracing (very verbose)
builder.AddFilter("MyBook.Writer", LogLevel.Trace);
```

## NuGet Package

MyBook.Writer is available as a NuGet package:

- [MyBook.Writer on NuGet.org](https://www.nuget.org/packages/MyBook.Writer/)

To install the package, use one of the following methods:

**Package Manager Console:**
```
Install-Package MyBook.Writer
```

**.NET CLI:**
```
dotnet add package MyBook.Writer
```

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request. 