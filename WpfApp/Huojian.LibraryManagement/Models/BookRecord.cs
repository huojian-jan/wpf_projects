using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Animation;

namespace Huojian.LibraryManagement.Models
{
  public class BookRecord
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public string Category { get; set; }

        public string AuthorName { get; set; }

        public double Price { get; set; }

        public string Language { get; set; }
    }
}
