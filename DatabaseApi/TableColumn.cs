using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseApi
{
    public class TableColumn
    {
        public string Name { get; private set; }
        public Type Type { get; private set; }

        public TableColumn(string name, Type type)
        {
            Name = name;
            Type = type;
        }
    }
}
