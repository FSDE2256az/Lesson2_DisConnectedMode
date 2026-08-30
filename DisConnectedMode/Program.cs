using System.Data;
using System.Data.Common;
using System.Data.SqlClient;

class DataAcces
{
    DbConnection? conn = null;
    SqlDataReader? reader = null;
    DataTable? table = null;
    DataSet? dataSet = null;
    SqlDataAdapter? dataAdapter = null;

    public DataAcces()
    {
        conn =new SqlConnection("Data Source=STHQ012E-01;Initial Catalog=Library;User ID=admin;Password=admin;Connect Timeout=30;");
    }


    #region Connected Mode With Data Table

    // DataTable Custom version
    public void WorkingWithDataTableCustom()
    {
        var table = new DataTable();

        // Columnlari elave etmek
        table.Columns.Add(new DataColumn("Id"));
        table.Columns.Add(new DataColumn("FirstName"));
        table.Columns.Add(new DataColumn("LastName"));

        // Column-u daha etrafli elave etmek 
        var column = new DataColumn()
        {
            AllowDBNull = true,
            DataType = typeof(int),
            DefaultValue = 0,
            ColumnName = "Score"
        };

        table.Columns.Add(column);

        table.Rows.Add(1, "Kamran", "Karimzada", 10);
        table.Rows.Add(2, "Burhan", "Orucov", 11);
        table.Rows.Add(3, "Xayyam", "Cabbarov", 9);

        ShowTableWithConsole(table);
    }

    // Connected Mode with DataTable
    public void WorkingWithDataTable()
    {
        try
        {
            // Column-u cox olan ilk yazilmalidir. 
            var query = "Select * From Books; Select * From Authors; ";

            using var command = new SqlCommand(query, (SqlConnection)conn);

            conn?.Open();

            table = new DataTable();
            reader = command.ExecuteReader();


            bool isColumnName = true;

            do
            {
                while (reader.Read())
                {

                    if (isColumnName)
                    {
                        for (int i = 0; i < reader.FieldCount; i++)
                            table.Columns.Add(reader.GetName(i));
                        isColumnName = false;
                    }


                    DataRow row = table.NewRow();

                    for (int i = 0; i < reader.FieldCount; i++)
                        row[i] = reader[i];

                    table.Rows.Add(row);
                }
            } while (reader.NextResult());


            // Console-a cixartdigimiz Funksiya
            ShowTableWithConsole(table);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Probably wrong request syntax");
        }
        finally
        {
            // Close the connection
            conn?.Close();
            reader?.Close();
        }
    }

    public void FillWithConnected()
    {
        // Table-i custom
        // WorkingWithDataTableCustom();

        // Sql-den datani cekib Table-da cixartmaq ( Connected Mode )
        WorkingWithDataTable();
    }

    #endregion

    private static void ShowTableWithConsole(DataTable table)
    {
        // Columns ekrana cixartmaq
        foreach (DataColumn Dcolumn in table.Columns)
            Console.Write($"{Dcolumn.ColumnName, -15}");

        Console.WriteLine();

        // Rows ekrana cixartmaq
        foreach (DataRow Drow in table.Rows)
        {
            foreach (var item in Drow.ItemArray)
                Console.Write($"{item, -15}");
            Console.WriteLine();
        }
    }


}


class Program
{
    static void Main(string[] args)
    {
        var dataAccess = new DataAcces();

        // dataAccess.WorkingWithDataTableCustom();
        dataAccess.WorkingWithDataTable();



    }

}