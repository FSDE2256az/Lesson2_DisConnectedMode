using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Diagnostics;

class DataAcces
{
    DbConnection? conn = null;
    SqlDataReader? reader = null;
    DataTable? table = null;
    DataSet? dataset = null;
    SqlDataAdapter? adapter = null;

    public DataAcces()
    {
        conn = new SqlConnection("Data Source=STHQ012E-01;Initial Catalog=Library;User ID=admin;Password=admin;Connect Timeout=30;");
    }

    #region Connected Mode With Data Table

    // DataTable Custom version
    private void WorkingWithDataTableCustom()
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

        ShowTableWithConsole(table);
    }

    // Connected Mode with DataTable
    private void WorkingWithDataTable()
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

    #region DisConnected Mode

    // Disconnected Mode
    // 1. DataSet
    // 2. DbDataAdapter

    public void FillWithTableOnDisconnected()
    {
        var query = "Select * From Authors;";

        var command = new SqlCommand()
        {
            CommandText = query,
            Connection = (SqlConnection)conn
        };


        adapter = new SqlDataAdapter(command);
        table = new DataTable();

        //adapter.Fill(3, 10); // Hansi araliqda goturmek isteyirikse
        adapter.Fill(table);

        // SqlDataAdapter
        // Fill			-> Select
        // Update		-> Insert, Update, Delete

        // Fill-den gelen datani hara yazmali

        // DataSet		-> Multi Select, Single Select
        // DataTable	-> Single Select 


        ShowTableWithConsole(table);
    }

    public void FillWithDataSetOnDisconnected()
    {
        var query = "Select * From Books; Select * From Authors;";

        var command = new SqlCommand()
        {
            CommandText = query,
            Connection = (SqlConnection)conn
        };


        adapter = new SqlDataAdapter(command);

        //// Data Adapter arxada nece isleyir

        //// Open()
        //// SqlCommand
        //// SqlDataReader
        //// Close()

        dataset = new DataSet();

        adapter.Fill(dataset, "mytable");

        ShowDataSetWithConsole(dataset);
        // ShowTableWithConsole(dataset.Tables[0]);
        // ShowTableWithConsole(dataset.Tables["mytable"]);
        // ShowTableWithConsole(dataset.Tables["mytable1"]);
        Console.WriteLine(dataset.Tables["mytable1"].Rows[0][1]); // FirsyName-i goturmek
    }


    //// DbCommand in SqlDataAdapter
    ///  1. SelectCommand	-> Fill 
    ///  2. InsertCommand	-> Update
    ///  3. UpdateCommand	-> Update
    ///  4. DeleteCommand	-> Update

    public void UpdateWithTableOnDisconnected()
    {
        var query = "Select * From Authors;";

        var command = new SqlCommand()
        {
            CommandText = query,
            Connection = (SqlConnection)conn
        };

        dataset = new DataSet();
        adapter = new SqlDataAdapter(command);

        // Deyisiklikler DataAdapter terefinden islenilsin deye
        var builder = new SqlCommandBuilder(adapter);


        adapter.Fill(dataset, "mytable");

        ShowDataSetWithConsole(dataset);

        if (dataset is not null)
        {
            // Update
            // dataset.Tables["mytable"].Rows[0][1] = "Burhan";

            // Delete
            // dataset.Tables["mytable"].Rows[14].Delete();

            DataRow newRow = dataset.Tables["mytable"].NewRow();
            newRow[0] = 51;                  // Id
            newRow[1] = "Kamran";          // FirstName
            newRow[2] = "Karimzada";        // LastName
            dataset.Tables["mytable"].Rows.Add(newRow);

            adapter.Update(dataset, "mytable");
            Console.WriteLine("Successfully operation");
        }
        ShowDataSetWithConsole(dataset);

        Debug.WriteLine(builder.GetInsertCommand().CommandText);
        Debug.WriteLine(builder.GetUpdateCommand().CommandText);
        Debug.WriteLine(builder.GetDeleteCommand().CommandText);
    }

    public void CustomUpdateCommand()
    {
        string selectSQL = "SELECT * FROM Books;";
        adapter = new SqlDataAdapter(selectSQL, (SqlConnection)conn);


        dataset = new DataSet();
        adapter.Fill(dataset, "myTable");



        //// Way 1
        // SqlCommand updateCommand = new SqlCommand("UPDATE Books SET Pages=@pPages WHERE Id=@pId", (SqlConnection)conn);



        // Way 2
        SqlCommand updateCommand = new SqlCommand()
        {
            CommandText = "usp_UpdateBooks",
            Connection = (SqlConnection)conn,
            CommandType = CommandType.StoredProcedure,
        };

        updateCommand.Parameters.Add(new SqlParameter("@pId", SqlDbType.Int));
        updateCommand.Parameters["@pId"].SourceVersion = DataRowVersion.Original;
        updateCommand.Parameters["@pId"].SourceColumn = "Id";


        updateCommand.Parameters.Add(new SqlParameter("@pPages", SqlDbType.Int));
        updateCommand.Parameters["@pPages"].SourceVersion = DataRowVersion.Current;
        updateCommand.Parameters["@pPages"].SourceColumn = "Pages";

        updateCommand.Parameters["@pId"].Value = 1;
        updateCommand.Parameters["@pPages"].Value = 5;


        adapter.UpdateCommand = updateCommand;

        dataset.Tables["myTable"].Rows[1]["Pages"] = 1999;  // məsələn

        adapter.Update(dataset, "myTable");

        Console.WriteLine("Successfully operation");
    }

    #endregion


    private static void ShowTableWithConsole(DataTable table)
    {
        // Columns ekrana cixartmaq
        foreach (DataColumn Dcolumn in table.Columns)
            Console.Write($"{Dcolumn.ColumnName,-15}");

        Console.WriteLine();

        // Rows ekrana cixartmaq
        foreach (DataRow Drow in table.Rows)
        {
            foreach (var item in Drow.ItemArray)
                Console.Write($"{item,-15}");
            Console.WriteLine();
        }
    }


    private static void ShowDataSetWithConsole(DataSet set)
    {
        foreach (DataTable table in set.Tables)
        {
            // Columns ekrana cixartmaq
            foreach (DataColumn Dcolumn in table.Columns)
                Console.Write($"{Dcolumn.ColumnName,-15}");

            Console.WriteLine();

            // Rows ekrana cixartmaq
            foreach (DataRow Drow in table.Rows)
            {
                foreach (var item in Drow.ItemArray)
                    Console.Write($"{item,-15}");
                Console.WriteLine();
            }
        }

    }
}


class Program
{
    static void Main(string[] args)
    {
        var dataAccess = new DataAcces();

        // dataAccess.WorkingWithDataTableCustom();
        // dataAccess.WorkingWithDataTable();
        // dataAccess.FillWithTableOnDisconnected();

        // dataAccess.FillWithDataSetOnDisconnected();
        // dataAccess.UpdateWithTableOnDisconnected();
        dataAccess.CustomUpdateCommand();


    }

}