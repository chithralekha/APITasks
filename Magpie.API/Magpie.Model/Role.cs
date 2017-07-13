using System;
using System.Collections.Generic;

namespace Magpie.Model
{
    public class Role
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public RoleType RoleType { get; set; }
    }
}
