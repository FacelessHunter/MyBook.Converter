using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedModels.Models
{
    public class Chapter
    {
        public string Title { get; set; }

        public List<string> Paragraphs { get; set; }

        public Chapter(string title, List<string> paragraphs)
        {
            Title = title;
            Paragraphs = paragraphs;
        }
    }
}
