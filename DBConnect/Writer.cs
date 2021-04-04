using System;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Npgsql;

namespace DBConnect
{
    public class Writer : Connecter
    {
        private List<Course> courses;
        private List<NpgsqlCommand> queries;
        public Writer()
        {
            courses = new List<Course>();
            queries = new List<NpgsqlCommand>();
        }

        private void ReadCourses()
        {
            string json = File.ReadAllText(jsonFile);
            courses = JsonConvert.DeserializeObject<List<Course>>(json);
        }

        private void GetQueries()
        {
            queries = new List<NpgsqlCommand>();
            queries.Add(new NpgsqlCommand("DELETE FROM courses_sections", connection));
            queries.Add(new NpgsqlCommand("DELETE FROM quiz_levels_questions", connection));
            queries.Add(new NpgsqlCommand("DELETE FROM questions_answers", connection));

            queries.Add(new NpgsqlCommand("DELETE FROM sections", connection));
            queries.Add(new NpgsqlCommand("DELETE FROM description_categories", connection));
            queries.Add(new NpgsqlCommand("DELETE FROM description_levels", connection));
            queries.Add(new NpgsqlCommand("DELETE FROM descriptions", connection));
            queries.Add(new NpgsqlCommand("DELETE FROM answers", connection));
            queries.Add(new NpgsqlCommand("DELETE FROM questions", connection));
            queries.Add(new NpgsqlCommand("DELETE FROM quiz_levels", connection));
            queries.Add(new NpgsqlCommand("DELETE FROM quizzes", connection));
            queries.Add(new NpgsqlCommand("DELETE FROM courses", connection));
            queries.Add(new NpgsqlCommand("DELETE FROM categories", connection));
            foreach (Course course in courses)
            {
                course.GetQuery(queries, connection);
            }
            queries = queries.Distinct().ToList();
        }

        private void Write()
        {
            foreach (NpgsqlCommand query in queries)
            {
                query.ExecuteNonQuery();
            }
        }

        public void run()
        {
            connect();
            ReadCourses();
            GetQueries();
            Write();
            CloseConnection();
        }
    }
}
