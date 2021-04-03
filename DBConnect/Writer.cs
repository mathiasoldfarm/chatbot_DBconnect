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
            queries.Add(new NpgsqlCommand("DELETE FROM course_level_questions", connection));
            queries.Add(new NpgsqlCommand("DELETE FROM course_question_possible_answers", connection));

            queries.Add(new NpgsqlCommand("DELETE FROM course_sections", connection));
            queries.Add(new NpgsqlCommand("DELETE FROM course_description_categories", connection));
            queries.Add(new NpgsqlCommand("DELETE FROM course_description_levels", connection));
            queries.Add(new NpgsqlCommand("DELETE FROM course_description", connection));
            queries.Add(new NpgsqlCommand("DELETE FROM course_possible_answers", connection));
            queries.Add(new NpgsqlCommand("DELETE FROM course_questions", connection));
            queries.Add(new NpgsqlCommand("DELETE FROM course_quiz_levels", connection));
            queries.Add(new NpgsqlCommand("DELETE FROM course_quiz", connection));
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
