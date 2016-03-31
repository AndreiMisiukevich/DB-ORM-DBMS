namespace DatabaseApi
{
    public interface IDbApi
    {
        void CreateDataBase(string command);
        void CreateTable(string command, string dbName);

        void DropDatabase(string command);
        void DropTable(string command, string dbName);

        string GetContent(string command, string dbName);
        void InsertContent(string command, string dbName);

        string UseDb(string command);
    }
}
