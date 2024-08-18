using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Huojian.LibraryManagement.Models
{
    public class Reader
    {
        public string Id { get; set; }

        public string UserName { get; set; }

        public string RealName { get; set; }

        public string Gender { get; set; }

        public string BirthDay { get; set; }

        public string CreateTime { get; set; }
        public string Phone { get; set; }

        public string Email { get; set; }
    }
}
