using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseApi
{
    public interface IDbApi
    {
        void CreateDb(string command);
        void CreateTable(string command, string dbName);

        void DropDb(string command);
        void DropTable(string command, string dbName);

        string UseDb(string command);
    }
}
