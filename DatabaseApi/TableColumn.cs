using System;

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
