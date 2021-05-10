using System;
using System.Data;
using Newtonsoft.Json;
using System.Collections.Generic;
using Npgsql;
using System.Linq;

namespace DBConnect {
    public class DescriptionLevelCategory {
        [JsonIgnore]
        public int id {
            get; set;
        }
        public string category {
            get; set;
        }

        public DescriptionLevelCategory(DataRow row) {
            try {
                id = (int)row[0];
                category = (string)row[1];
            }
            catch {
                throw new Exception("Constructor argument DataRow was expected to have three arguments of type int, string string");
            }
        }

        [JsonConstructor]
        public DescriptionLevelCategory(int _id, string _category)
        {
            id = _id;
            category = _category;
        }

        public void GetQuery(List<NpgsqlCommand> queries, NpgsqlConnection connection)
        {
            if (queries.All(q => !q.CommandText.Contains("INSERT INTO description_categories(id, category)") || (int)q.Parameters["id"].Value != id))
            {
                NpgsqlCommand query = new NpgsqlCommand("INSERT INTO description_categories(id, category) VALUES(@id, @category)", connection);
                query.Parameters.AddWithValue("id", id);
                query.Parameters.AddWithValue("category", category);
                queries.Add(query);
            }
        }
    }
}
