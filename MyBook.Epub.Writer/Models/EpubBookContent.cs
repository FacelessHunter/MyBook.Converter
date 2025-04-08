using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyBook.Epub.Writer.Models
{
    public class EpubBookContent
    {
        public SortedList<int, EpubChapter> Chapters { get; } = new SortedList<int, EpubChapter>();
    }
}
