using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrmLibrary
{
    internal static class CommandPattertns
    {
        public const string CreateDatabase = "CRT DB {0}.\n";
        public const string DropDatabase = "DRP DB {0}.\n";
        public const string DropTable = "DRP TB {0}.\n";
        public const string CreateTable = "CRT TB {0} {1}.\n";
        public const string InsertValues = "INS TB {0} {1}.\n";
        public const string UseDatabase = "USE DB {0}.\n";
    }
}
