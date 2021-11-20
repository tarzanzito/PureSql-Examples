using System;
using System.Collections.Generic;

namespace Candal.Models
{
    public class MyIdentityUser2
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public List<MyIdentityRole2> Roles { get; set; }
    }

    public class MyIdentityRole2
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }

}
