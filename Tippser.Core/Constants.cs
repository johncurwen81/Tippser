using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tippser.Core
{
    public class Constants
    {
        public const string TippserConnectionString = nameof(TippserConnectionString);
        public const string ApiBaseUrl = nameof(ApiBaseUrl);
        public const string Anonymous = nameof(Anonymous);
        public const string Environment = nameof(Environment);
        public const string Development = nameof(Development);
        public const string ErrorRoute = @"/Error/Index";
        public const string SystemUserId = "0b3e703e-bd46-44b2-b868-1ffac7b4674f";
        public const string SuperAdminUserId = "0b3e703e-bd46-55b2-b868-1ffac7b4674f";
        public const string SuperAdminRoleId = "0b3e703e-bd46-66b2-b868-1ffac7b4674f";
        public const string UserRoleId = "0b3e703e-bd46-77b2-b868-1ffac7b4674f";
        public const string StatusCookieName = "Identity.StatusMessage";

        public class Roles
        {
            public const string SuperAdmin = nameof(SuperAdmin);
            public const string User = nameof(User);
        }
    }
}
