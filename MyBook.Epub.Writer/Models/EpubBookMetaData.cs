using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyBook.Epub.Writer.Models
{
    public class EpubBookMetaData
    {
        public string Identifier { get; set; }

        public string Title { get; set; }

        /// <summary>
        /// 	Language code (ISO 639-1)
        /// </summary>
        public string Language { get; set; }

        public string Author { get; set; }

        public string Publisher { get; set; }
    }
}
