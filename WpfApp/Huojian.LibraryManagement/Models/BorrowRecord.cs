using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Huojian.LibraryManagement.Models
{
    public class BorrowRecord
    {
        public string BookName { get; set; }

        public string ReaderId { get; set; }

        public string ReaderName { get; set; }

        public string BorrowTime { get; set; }

        public string ReturnTime { get; set; }

        public string BookStatus { get; set; }
    }
}
