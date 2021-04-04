using System;
using System.Data;
using System.Collections.Generic;
using Newtonsoft.Json;
using Npgsql;

namespace DBConnect {
    public class Quiz {
        public int id {
            get; set;
        }
        public List<QuizLevel> levels {
            get; set;
        }

        public Quiz(DataRow row) {
            try {
                id = (int)row[0];
                levels = new List<QuizLevel>();
            }
            catch {
                throw new Exception("Constructor argument DataRow was expected to have three arguments of type int string");
            }
        }

        [JsonConstructor]
        public Quiz(int _id, List<QuizLevel> _levels)
        {
            id = _id;
            levels = _levels;
        }

        public void AddQuizLevel(QuizLevel quizLevel) {
            levels.Add(quizLevel);
        }

        public void GetQuery(List<NpgsqlCommand> queries, NpgsqlConnection connection)
        {
            NpgsqlCommand query = new NpgsqlCommand("INSERT INTO quizzes(id) VALUES(@id)", connection);
            query.Parameters.AddWithValue("id", id);
            queries.Add(query);
            foreach (QuizLevel quizLevel in levels)
            {
                quizLevel.GetQuery(queries, connection, id);
            }
        }
    }
}
