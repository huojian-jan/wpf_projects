using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Huojian.LibraryManagement.Components.Protocol.Swager.Model
{
    public class UserInfo
    {
        public string UserName { get; set; }

        public string RealName { get; set; }

        public string UserId { get; set; }

        public string ReaderId { get; set; }

        public string Gender { get; set; }

        public string Password { get; set; }

        public string AvatarUrl { get; set; }

        public string UserType { get; set; }

        public DateTime BirthDay { get; set; }
        public string Phone { get; set; }

        public string Email { get; set; }
    }
}
