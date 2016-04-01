using OrmLibrary;

namespace TestClient
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var orm = new OrmSimple();
            var dbName = "TestDatabase";
            orm.CreateDataBase(dbName);
            orm.CreateTable<TestEmployee>(dbName);
            orm.CreateTable<TestProfession>(dbName);
        }
    }

    public class TestEmployee
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Age { get; set; }
        public double Salary { get; set; }
    }

    public class TestProfession
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
