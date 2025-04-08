using MyBook.Epub.Writer.Models.ChapterModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyBook.Epub.Writer.Models
{
    public class EpubChapter
    {
        public string Title { get; set; }

        public SortedList<int, BaseRowModel> Content { get; } = new SortedList<int, BaseRowModel>();
    }
}
