using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.EntityFrameworkCore.TablePerHierarchy
{
    class Helper
    {
        public static readonly Func<string> Random30Characters = () => Guid.NewGuid().ToString().Replace("-", string.Empty).Substring(0, 30);
    }
}
