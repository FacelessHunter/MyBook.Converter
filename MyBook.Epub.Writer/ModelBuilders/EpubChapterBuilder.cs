using MyBook.Epub.Writer.Models;
using MyBook.Epub.Writer.Models.ChapterModels;
using System;

namespace MyBook.Epub.Writer
{
    public class EpubChapterBuilder
    {
        private EpubChapter _epubChapter;
        private int _sequenceNumber;

        public EpubChapterBuilder()
        {
            _sequenceNumber = 0;
            _epubChapter = new EpubChapter();
        }

        public EpubChapterBuilder SetTitle(string title) 
        {
            _epubChapter.Title = title;

            return this;
        }

        public EpubChapterBuilder AddTextParagraph(string text)
        {
            var paragraph = new Paragraph(_sequenceNumber, text);

            _epubChapter.Content.Add(_sequenceNumber, paragraph);

            _sequenceNumber++;

            return this;
        }

        public EpubChapter Build() => _epubChapter;

    }
}
