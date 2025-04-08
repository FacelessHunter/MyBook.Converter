using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedModels
{
    public class FileModel
    {
        public string FileName { get; set; }       

        public string ContentType { get; set; }      

        public MemoryStream Stream { get; set; }
    }
}
