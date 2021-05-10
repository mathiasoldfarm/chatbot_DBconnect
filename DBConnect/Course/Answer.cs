using System;
using System.Data;
using Newtonsoft.Json;
using System.Collections.Generic;
using Npgsql;

namespace DBConnect
{
    public class Answer
    {
        [JsonIgnore]
        public int id {
            get; set;
        }
        public string answer {
            get; set;
        }
        public string explanation {
            get; set;
        }

        public Answer(DataRow row) {
            try {
                id = (int)row[0];
                answer = (string)row[1];
                explanation = (string)row[2];
            } catch {
                throw new Exception("Constructor argument DataRow was expected to have three arguments of type int, string string");
            }
        }

        [JsonConstructor]
        public Answer(int _id, string _answer, string _explantion)
        {
            id = _id;
            answer = _answer;
            explanation = _explantion;
        }

        public Answer(string _answer, string _explantion) {
            answer = _answer;
            explanation = _explantion;
        }

        public void GetQuery(List<NpgsqlCommand> queries, NpgsqlConnection connection)
        {
            NpgsqlCommand query = new NpgsqlCommand("INSERT INTO answers(id, answer, explanation) VALUES(@id, @answer, @explanation)", connection);
            query.Parameters.AddWithValue("id", id);
            query.Parameters.AddWithValue("answer", answer);
            query.Parameters.AddWithValue("explanation", explanation);
            queries.Add(query);
        }
    }
}
