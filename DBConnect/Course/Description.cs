using System;
using System.Data;
using System.Collections.Generic;
using Newtonsoft.Json;
using Npgsql;

namespace DBConnect {
    public class Description {
        public int id {
            get; set;
        }
        public List<DescriptionLevel> levels {
            get; set;
        }

        public Description(DataRow row) {
            try {
                id = (int)row[0];
                levels = new List<DescriptionLevel>();
            }
            catch {
                throw new Exception("Constructor argument DataRow was expected to have ine argument of type int");
            }
        }

        [JsonConstructor]
        public Description(int _id, List<DescriptionLevel> _levels)
        {
            _id = id;
            _levels = levels;
        }

        public void AddDescriptionLevel(DescriptionLevel descriptionLevel) {
            levels.Add(descriptionLevel);
        }

        public void GetQuery(List<NpgsqlCommand> queries, NpgsqlConnection connection)
        {
            NpgsqlCommand query = new NpgsqlCommand("INSERT INTO course_description(id) VALUES(@id)", connection);
            query.Parameters.AddWithValue("id", id);
            queries.Add(query);
            foreach (DescriptionLevel descriptionLevel in levels)
            {
                descriptionLevel.GetQuery(queries, connection, id);
            }
        }
    }
}
