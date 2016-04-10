using System;
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

            var employeeList = new[]
            {
                new TestEmployee {Id = 1, Age = 20, Name = "Alex", Salary = 1500.0},
                new TestEmployee {Id = 2, Age = 22, Name = "Helen", Salary = 950.0}
            };

            orm.InsertContent(dbName, employeeList);
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
