using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Huojian.LibraryManagement.Components.Protocol.Swager.Model
{
    public class BorrowRecord
    {
        public string BookId { get; set; }

        public string ReaderId { get; set; }

        public DateTime BorrowedTime { get; set; }

        public DateTime ReturnTime { get; set; }
    }
}
