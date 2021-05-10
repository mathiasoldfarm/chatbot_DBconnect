using System;
using System.Data;
using Newtonsoft.Json;
using System.Collections.Generic;
using Npgsql;

namespace DBConnect {
    public class DescriptionLevel {
        [JsonIgnore]
        public int id {
            get; set;
        }
        public string description {
            get; set;
        }
        public int level {
            get; set;
        }
        public DescriptionLevelCategory category {
            get; set;
        }

        public DescriptionLevel(DataRow row) {
            try {
                id = (int)row[0];
                description = (string)row[1];
                level = (int)row[2];
            }
            catch {
                throw new Exception("Constructor argument DataRow was expected to have two arguments of type int, string, int");
            }
        }


        [JsonConstructor]
        public DescriptionLevel(int _id, string _description, int _level, DescriptionLevelCategory _category)
        {
            id = _id;
            description = _description;
            level = _level;
            category = _category;
        }

        public DescriptionLevel(string _description, int _level) {
            description = _description;
            level = _level;
        }


        public void AddCategory(DescriptionLevelCategory _category) {
            category = _category;
        }

        public void GetQuery(List<NpgsqlCommand> queries, NpgsqlConnection connection, int descriptionId)
        {
            category?.GetQuery(queries, connection);

            NpgsqlCommand query = new NpgsqlCommand("INSERT INTO description_levels(id, description, level, category, description_id) VALUES(@id, @description, @level, @category, @description_id)", connection);
            query.Parameters.AddWithValue("id", id);
            query.Parameters.AddWithValue("description", description);
            query.Parameters.AddWithValue("level", level);
            if (category != null)
            {
                query.Parameters.AddWithValue("category", category.category);
            } else
            {
                query.Parameters.AddWithValue("category", DBNull.Value);
            }
            query.Parameters.AddWithValue("description_id", descriptionId);
            queries.Add(query);
        }
    }
}
