using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyBook.Epub.Writer.Models.ChapterModels
{
    public abstract class BaseRowModel
    {
        public int Id { get; set; }

        protected BaseRowModel(int id)
        {
            Id = id;
        }
    }
}
