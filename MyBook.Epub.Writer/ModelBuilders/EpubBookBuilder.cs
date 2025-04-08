using MyBook.Epub.Writer.Models;
using System;

namespace MyBook.Epub.Writer.ModelBuilders
{
    public class EpubBookBuilder
    {
        private EpubBook _epubBook;

        public EpubBookBuilder CreateBook()
        {
            _epubBook = new EpubBook();

            return this;
        }

        public EpubBookBuilder SetIdentifier(string identifier)
        {
            _epubBook.Metadata.Identifier = identifier;

            return this;
        }

        public EpubBookBuilder SetTitle(string title) 
        {
            _epubBook.Metadata.Title = title;

            return this;
        }

        public EpubBookBuilder SetLanguage(string language)
        {
            _epubBook.Metadata.Language = language;

            return this;
        }

        public EpubBookBuilder SetCoverPage(MemoryStream stream, string id, string contentType, string filename)
        {
            _epubBook.CoverPage = new SharedModels.FileModel()
            {
                Stream = stream,
                FileName = "cover." + filename.Split('.').Last(),
                ContentType = contentType,
            };

            return this;
        }

        public EpubBookBuilder AddChapter(int sequenceNumber, EpubChapter chapter)
        {
            _epubBook.Content.Chapters.Add(sequenceNumber, chapter);

            return this;
        }

        public EpubBook Build()
        {
            return _epubBook;
        }
    }
}
