using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Account_Apis.Dtos
{
    public class UserProfileDto
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
    }
}