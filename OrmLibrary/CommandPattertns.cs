using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrmLibrary
{
    internal static class CommandPattertns
    {
        public const string CreateDatabase = "CRT DB {0}.";
        public const string DropDatabase = "DRP DB {0}.";
        public const string DropTable = "DRP TB {0}.";
        public const string CreateTable = "CRT TB {0} {1}.";
        public const string InsertValues = "INS TB {0} {1}.";
        public const string UseDatabase = "USE DB {0}.";
    }
}
