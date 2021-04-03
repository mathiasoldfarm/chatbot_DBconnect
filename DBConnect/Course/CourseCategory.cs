using System;
using System.Data;
using Newtonsoft.Json;
using System.Collections.Generic;
using Npgsql;
using System.Linq;

namespace DBConnect {
    public class CourseCategory {
        public int id {
            get; set;
        }
        public string category {
            get; set;
        }

        public string colorClass
        {
            get; set;
        }

        public CourseCategory(DataRow row) {
            try {
                id = (int)row[0];
                category = (string)row[1];
                colorClass = (string)row[2];
            }
            catch {
                throw new Exception("Constructor argument DataRow was expected to have two arguments of type int, string");
            }
        }

        [JsonConstructor]
        public CourseCategory(int _id, string _category, string _colorClass)
        {
            id = _id;
            category = _category;
            colorClass = _colorClass;
        }

        public void GetQuery(List<NpgsqlCommand> queries, NpgsqlConnection connection)
        {
            if (queries.All(q => !q.CommandText.Contains("INSERT INTO categories(id, title, color_class)") || (int)q.Parameters["id"].Value != id))
            {
                NpgsqlCommand query = new NpgsqlCommand("INSERT INTO categories(id, title, color_class) VALUES(@id, @title, @color_class)", connection);
                query.Parameters.AddWithValue("id", id);
                query.Parameters.AddWithValue("title", category);
                query.Parameters.AddWithValue("color_class", colorClass);
                queries.Add(query);
            }
        }
    }
}
