using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Npgsql;

namespace DBConnect
{
    public class Question
    {
        public int id {
            get; set;
        }
        public string question {
            get; set;
        }
        public Answer correct {
            get; set;
        }
        private List<(int, Answer)> possibleAnswersData {
            get; set;
        }

        public List<Answer> possibleAnswers {
            get {
                return possibleAnswersData.Select(x => x.Item2).ToList();
            }
            set
            {
                if (value != null)
                {
                    possibleAnswersData = new List<(int, Answer)>();
                    for (int i = 0; i < value.Count; i++)
                    {
                        possibleAnswersData.Add((i, value[i]));
                    }
                }
            }
        }

        public Question(DataRow row)
        {
            try {
                id = (int)row[0];
                question = (string)row[1];
                possibleAnswersData = new List<(int, Answer)>();
            }
            catch {
                throw new Exception("Constructor argument DataRow was expected to have two arguments of type int, string");
            }
        }

        [JsonConstructor]
        public Question(int _id, string _question, List<Answer> _possibleAnswers)
        {
            id = _id;
            question = _question;
            possibleAnswers = _possibleAnswers;
        }

        public void AddCorrect(Answer _correct) {
            correct = _correct;
        }

        public void AddAnswer((int, Answer) items) {
            possibleAnswersData.Add(items);
        }

        public void SortAnswers() {
            possibleAnswersData.Sort((x, y) => x.Item1.CompareTo(y.Item1));
        }

        public void GetQuery(List<NpgsqlCommand> queries, NpgsqlConnection connection)
        {
            List<Answer> _possibleAnswers = possibleAnswers;
            foreach (Answer answer in _possibleAnswers)
            {
                answer.GetQuery(queries, connection);
            }

            NpgsqlCommand query = new NpgsqlCommand("INSERT INTO questions(id, question, correct) VALUES(@id, @question, @correct)", connection);
            query.Parameters.AddWithValue("id", id);
            query.Parameters.AddWithValue("question", question);
            query.Parameters.AddWithValue("correct", correct.id);
            queries.Add(query);

            for (int i = 0; i < _possibleAnswers.Count; i++)
            {
                query = new NpgsqlCommand("INSERT INTO questions_answers(question_id, possible_answer_id, \"order\") VALUES (@question_id, @possible_answer_id, @order)", connection);
                query.Parameters.AddWithValue("question_id", id);
                query.Parameters.AddWithValue("possible_answer_id", _possibleAnswers[i].id);
                query.Parameters.AddWithValue("order", i);
                queries.Add(query);
            }
        }
    }
}
