using SharedModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedModels.Models
{
    public class Book
    {
        public string Title { get; set; }

        public string Description { get; set; }

        public EncodedImage CoverPage { get; set; }

        public SortedList<int, Chapter> Chapters { get; set; } = new SortedList<int, Chapter>();

        public Book(string title) 
        { 
            Title = title;
        }

        public void AddChapter(int sequenceNumber, Chapter chapter)
        {
            Chapters.Add(sequenceNumber, chapter);
        }

        public void SetCoverPage(EncodedImage image)
        {
            CoverPage = image;
        }
    }
}
