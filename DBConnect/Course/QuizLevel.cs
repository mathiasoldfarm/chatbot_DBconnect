using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Npgsql;

namespace DBConnect
{
    public class QuizLevel
    {
        public int id { get; set; }
        public int level {
            get; set;
        }
        private  List<(int, Question)> questionsData {
            get; set;
        }

        public List<Question> questions {
            get {
                return questionsData.Select(x => x.Item2).ToList();
            }
            set
            {
                if (value != null)
                {
                    questionsData = new List<(int, Question)>();
                    for (int i = 1; i <= value.Count; i++)
                    {
                        questionsData.Add((i, value[i-1]));
                    }
                }
            }
        }

        public QuizLevel(DataRow row) {
            try {
                id = (int)row[0];
                level = (int)row[1];
                questionsData = new List<(int, Question)>();
            }
            catch {
                throw new Exception("Constructor argument DataRow was expected to have three arguments of type int, int string");
            }
        }

        [JsonConstructor]
        public QuizLevel(int _id, int _level, List<Question> _questions)
        {
            id = _id;
            level = _level;
            questions = _questions;
        }

        public void AddQuestion((int, Question) items) {
            questionsData.Add(items);
        }

        public void SortQuestions() {
            questionsData.Sort((x, y) => y.Item1.CompareTo(x.Item1));
        }

        public void GetQuery(List<NpgsqlCommand> queries, NpgsqlConnection connection, int quizId)
        {
            List<Question> _questions = questions;
            foreach (Question question in _questions)
            {
                question.GetQuery(queries, connection);
            }

            NpgsqlCommand query = new NpgsqlCommand("INSERT INTO course_quiz_levels(id, level, quiz_id) VALUES(@id, @level, @quiz_id)", connection);
            query.Parameters.AddWithValue("id", id);
            query.Parameters.AddWithValue("level", level);
            query.Parameters.AddWithValue("quiz_id", quizId);
            queries.Add(query);

            for (int i = 0; i < _questions.Count; i++)
            {
                query = new NpgsqlCommand("INSERT INTO course_level_questions(quiz_level_id, question_id, \"order\") VALUES (@quiz_level_id, @question_id, @order)", connection);
                query.Parameters.AddWithValue("quiz_level_id", id);
                query.Parameters.AddWithValue("question_id", _questions[i].id);
                query.Parameters.AddWithValue("order", i);
                queries.Add(query);
            }
        }
    }
}
