using System;
using System.Data;
using Newtonsoft.Json;
using System.Collections.Generic;
using Npgsql;

namespace DBConnect {
    public class Section {
        [JsonIgnore]
        public int id {
            get; set;
        }

        [JsonIgnore]
        public int depth {
            get; set;
        }

        public string name {
            get; set;
        }
        public Quiz quiz {
            get; set;
        }
        public Description description {
            get; set;
        }

        public bool usingQuiz {
            get; set;
        }
        public bool usingDescription {
            get; set;
        }

        [JsonIgnore]
        public int parent {
            get; set;
        }

        public List<Section> children {
            get; set;
        }

        public Section(DataRow row) {
            try {
                id = (int)row[0];
                name = (string)row[1];
                if (row[5] != DBNull.Value) {
                    usingQuiz = (bool)row[5];
                }
                if (row[6] != DBNull.Value) {
                    usingDescription = (bool)row[6];
                }
                children = new List<Section>();
                parent = -1;
            }
            catch {
                throw new Exception("Constructor argument DataRow was expected to have two arguments of type int string");
            }
        }

        [JsonConstructor]
        public Section(int _id, string _name, Quiz _quiz, Description _description, int _parent)
        {
            parent = _parent;
            id = _id;
            name = _name;
            quiz = _quiz;
            description = _description;
            parent = -1;
        }

        public Section(string _name, int _depth) {
            name = _name;
            depth = _depth;
        }


        public void AddDescription(Description _description) {
            description = _description;
        }

        public void AddQuiz(Quiz _quiz) {
            quiz = _quiz;
        }

        public void AddParent(int id) {
            parent = id;
        }

        public void GetQuery(List<NpgsqlCommand> queries, NpgsqlConnection connection)
        {
            quiz?.GetQuery(queries, connection);
            description?.GetQuery(queries, connection);

            NpgsqlCommand query = new NpgsqlCommand("INSERT INTO sections(id, section_name, quiz_id, description_id, parent_id) VALUES(@id, @section_name, @quiz_id, @description_id, @parent_id)", connection);
            query.Parameters.AddWithValue("id", id);
            query.Parameters.AddWithValue("section_name", name);
            if ( quiz != null )
            {
                query.Parameters.AddWithValue("quiz_id", quiz.id);
                query.Parameters.AddWithValue("description_id", DBNull.Value);
            } else
            {
                query.Parameters.AddWithValue("quiz_id", DBNull.Value);
                query.Parameters.AddWithValue("description_id", description.id);
            }
            if ( parent == -1 )
            {
                query.Parameters.AddWithValue("parent_id", DBNull.Value);
            } else {
                query.Parameters.AddWithValue("parent_id", parent);
            }
            queries.Add(query);
        }
    }
}
