using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLitePCL;
using System.IO;
using Windows.Storage;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Globalization;
using SQLite;

namespace BWDB.Core
{
    public class OSInformation
    {
        string dbFileName;
        public OSInformation (string database)
        {
            dbFileName = database;
        }
        
        public Build GetBuild(int ProductID, int BuildID)
        {
            var dbConnection = new SQLiteConnection(dbFileName);
            
            var data = new Build();
            object[] args = { ProductID, BuildID };

            var query = dbConnection.Query<Build> (
                "SELECT * FROM BuildList " + 
                "JOIN ProductList ON BuildList.ProductID = ProductList.ProductID " + 
                "WHERE BuildList.ProductID = ? AND BuildID = ?", args);

            if (query.Count > 0) data = query[0];
            dbConnection.Dispose();
            return data;
        }

        public List<Build> GetBuilds(int ProductID)
        {
            var dbConnection = new SQLiteConnection(dbFileName);
            
            var query = dbConnection.Query<Build>(
                "SELECT * FROM BuildList " +
                "JOIN ProductList ON BuildList.ProductID = ProductList.ProductID " +
                 "WHERE BuildList.ProductID = ?", ProductID);

            dbConnection.Dispose();

            return query;
            
        }

        public List<Build> GetBuilds (string Keyword)
        {
            var dbConnection = new SQLiteConnection(dbFileName);
            if (dbConnection == null) throw new Exception();
            
            List<Build> query = new List<Build>();

            var command = dbConnection.CreateCommand(
                 "SELECT * FROM BuildList " +
                "JOIN ProductList ON BuildList.ProductID = ProductList.ProductID WHERE " +
                "Buildtag LIKE @keyword OR Version LIKE @keyword"
                );

            command.Bind("@keyword", $"%{Keyword}%");

            query = command.ExecuteQuery<Build>();

            dbConnection.Dispose();

            return query;
        }

        public List<Product> GetProducts()
        {
            var dbConnection = new SQLiteConnection(dbFileName);
            //var query = new List<Product>();

            var query = dbConnection.Query<Product>(
                "SELECT * FROM ProductList " +
                "JOIN Tags ON Tags.TagID = ProductList.TagID " +
                "JOIN Family on Family.FamilyID = ProductList.FamilyID");

            dbConnection.Dispose();
            return query;
        }
    }

}
