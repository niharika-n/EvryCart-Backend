using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebAPIs.Data
{
    public enum Enums
    {
        SuperAdmin = 1,
        Admin = 2,
        User = 3
    }

    public enum Operation
    {
        Create = 1,
        Read = 2,
        Update = 3,
        Delete = 4
    }

    public enum Status
    {
        Success = 1,
        Fail = 2,
        Error = 3
    }
}
