using System.Data;
using System.Diagnostics;

namespace Slovo_Filter;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
        TestDatabase();
        MainPage = new AppShell();
    }

    private async void TestDatabase()
    {
        try
        {
            var dbContext = new Slovo_Filter_DAL.dbContext();

            string query = "SELECT * FROM users";

            var result = await dbContext.ExecuteQueryAsync(query);

            foreach (DataRow row in result.Rows)
            {
                Console.WriteLine($"ID: {row["id"]}, Name: {row["name"]}, Email: {row["email"]}");
            }

            Debug.WriteLine("Database successfully executed");
        }
        catch
        {
            Debug.WriteLine("Database failed to execute");
        }
    }
    
    
}