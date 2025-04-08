# MyBook.Writer NuGet Package

[![NuGet](https://img.shields.io/nuget/v/MyBook.Writer.svg)](https://www.nuget.org/packages/MyBook.Writer/)
[![Downloads](https://img.shields.io/nuget/dt/MyBook.Writer.svg)](https://www.nuget.org/packages/MyBook.Writer/)

A .NET library for creating e-books in EPUB and FB2 formats with support for rich formatting, table of contents, and metadata.

## Installation

### Package Manager Console

```
Install-Package MyBook.Writer
```

### .NET CLI

```
dotnet add package MyBook.Writer
```

## Quick Start

### Creating an EPUB Book

```csharp
using MyBook.Writer;
using MyBook.Writer.Builders;

// Create an EPUB builder
var builder = EpubBuilder.Create("My First E-Book", "en")
    .WithAuthor("Your Name");

// Add chapters
builder.AddChapter(1, "Introduction", new List<string>
{
    "This is the first paragraph of the introduction.",
    "This is the second paragraph with more content."
});

builder.AddChapter(2, "Chapter One", new List<string>
{
    "Chapter one begins with this first paragraph.",
    "And continues with this second paragraph."
});

// Build and save the book
var writer = await builder.BuildAsync();
await writer.SaveToFileAsync("my-first-ebook.epub");
```

### Creating an FB2 Book

```csharp
using MyBook.Writer;
using MyBook.Writer.Builders;

// Create an FB2 builder
var builder = FB2Builder.Create("My First FB2 Book", "en")
    .WithAuthor("Your Name");

// Add chapters
builder.AddChapter(1, "Introduction", new List<string>
{
    "This is the first paragraph of the introduction.",
    "This is the second paragraph with more content."
});

// Build and save the book
var writer = await builder.BuildAsync();
await writer.SaveToFileAsync("my-first-book.fb2");
```

### Using the Factory

```csharp
using MyBook.Writer;

// Create a book in your preferred format
var builder = BookFactory.CreateBuilder(BookFormat.Epub, "My Book Title", "en");

// Configure the book
builder.WithAuthor("Author Name")
       .AddChapter(1, "Chapter Title", new[] { "Paragraph content" });

// Build and save
var writer = await builder.BuildAsync();
await writer.SaveToFileAsync("output-path.epub");
```

## Setting Up Logging

```csharp
using Microsoft.Extensions.Logging;
using MyBook.Writer;
using MyBook.Writer.Core.Services;

// Configure logging
using var loggerFactory = LoggerFactory.Create(builder =>
{
    builder
        .AddFilter("MyBook.Writer", LogLevel.Debug)
        .AddConsole()
        .AddDebug();
});

// Configure the library to use your logger factory
loggerFactory.ConfigureBookWriter();

// Create a book using the factory (with logging enabled)
var bookBuilder = BookFactory.CreateBuilder(BookFormat.Epub, "Book With Logging", "en");
```

## Advanced Usage Examples

See the [GitHub repository](https://github.com/YourUsername/DataScraper) for more advanced examples and complete documentation. 