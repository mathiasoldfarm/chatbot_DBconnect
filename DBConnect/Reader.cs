﻿    using System;
using System.Data;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Linq;
using Npgsql;

namespace DBConnect
{
    public class Reader : Connecter
    {
        private Dictionary<int, Answer> answers;
        private Dictionary<int, Question> questions;
        private Dictionary<int, QuizLevel> quizLevels;
        private Dictionary<int, Quiz> quizzes;
        private Dictionary<int, DescriptionLevelCategory> descriptionLevelCategories;
        private Dictionary<int, DescriptionLevel> descriptionLevels;
        private Dictionary<int, Description> descriptions;
        private Dictionary<int, Section> sections;
        private Dictionary<int, CourseCategory> courseCategories;
        protected Dictionary<int, Course> courses;

        public Reader() {
            answers = new Dictionary<int, Answer>();
            questions = new Dictionary<int, Question>();
            quizLevels = new Dictionary<int, QuizLevel>();
            quizzes = new Dictionary<int, Quiz>();
            descriptionLevelCategories = new Dictionary<int, DescriptionLevelCategory>();
            descriptionLevels = new Dictionary<int, DescriptionLevel>();
            descriptions = new Dictionary<int, Description>();
            sections = new Dictionary<int, Section>();
            courseCategories = new Dictionary<int, CourseCategory>();
            courses = new Dictionary<int, Course>();
        }

        private DataSet GetData(Dictionary<string, string> QueryData) {
            DataSet ds = new DataSet();

            foreach(KeyValuePair<string, string> QueryTable in QueryData) {
                NpgsqlDataAdapter da = new NpgsqlDataAdapter(QueryTable.Key, connection);
                ds.Tables.Add(QueryTable.Value);
                da.Fill(ds.Tables[QueryTable.Value]);
            }

            return ds;
        }

        private void GetAnswers(DataTable table) {
            foreach (DataRow row in table.Rows) {
                int id = (int)row[0];
                answers.Add(id, new Answer(row));
            }
        }

        private void GetQuestions(DataTable table) {
            foreach (DataRow row in table.Rows) {
                int id = (int)row[0];
                int correctId = (int)row[2];

                Question question = new Question(row);
                question.AddCorrect(answers[correctId]);

                questions.Add(id, question);
            }
        }

        private void GetQuizLevels(DataTable table) {
            foreach (DataRow row in table.Rows) {
                int id = (int)row[0];
                quizLevels.Add(id, new QuizLevel(row));
            }
        }

        private void GetQuizzes(DataTable table) {
            foreach (DataRow row in table.Rows) {
                int id = (int)row[0];
                quizzes.Add(id, new Quiz(row));
            }
        }

        private void GetDescriptionLevelCategories(DataTable table) {
            foreach(DataRow row in table.Rows) {
                int id = (int)row[0];
                descriptionLevelCategories.Add(id, new DescriptionLevelCategory(row));
            }
        }

        private void GetDescriptionLevels(DataTable table) {
            foreach (DataRow row in table.Rows) {
                int id = (int)row[0];
                object categoryId = row[3];

                DescriptionLevel descriptionLevel = new DescriptionLevel(row);

                if (categoryId != DBNull.Value) {
                    descriptionLevel.AddCategory(descriptionLevelCategories[(int)categoryId]);
                }

                descriptionLevels.Add(id, descriptionLevel);
            }
        }

        private void GetDescriptions(DataTable table) {
            foreach (DataRow row in table.Rows) {
                int id = (int)row[0];
                descriptions.Add(id, new Description(row));
            }
        }

        private void GetSections(DataTable table) {
            Dictionary<int, object> temporaryParentMapper = new Dictionary<int, object>();

            foreach (DataRow row in table.Rows) {
                int id = (int)row[0];
                object parentId = row[4];
                Section section = new Section(row);

                object quizId = row[2];
                object descriptionId = row[3];
                if (quizId != DBNull.Value) {
                    section.AddQuiz(quizzes[(int)quizId]);
                }
                if (descriptionId != DBNull.Value) {
                    section.AddDescription(descriptions[(int)descriptionId]);
                }

                sections.Add(id, section);
                temporaryParentMapper.Add(id, parentId);
            }

            foreach (KeyValuePair<int, Section> section in sections) {
                object parentId = temporaryParentMapper[section.Key];
                if (parentId == DBNull.Value) {
                    section.Value.AddParent(-1);
                } else {
                    section.Value.AddParent((int)parentId);
                }
            }
        }

        private void GetCourseCategories(DataTable table) {
            foreach (DataRow row in table.Rows) {
                int id = (int)row[0];
                courseCategories.Add(id, new CourseCategory(row));
            }
        }

        private void GetCourses(DataTable table) {
            foreach (DataRow row in table.Rows) {
                int id = (int)row[0];
                string title = (string)row[1];
                string description = (string)row[2];
                int categoryId = (int)row[3];

                Course course = new Course(row);
                course.AddCategory(courseCategories[categoryId]);

                courses.Add(id, course);
            }
        }

        private void AddQuestionAnswerRelations(DataTable table) {
            foreach(DataRow row in table.Rows) {
                int questionId = (int)row[0];
                int answerId = (int)row[1];
                int order = (int)row[2];
                questions[questionId].AddAnswer((order, answers[answerId]));
            }
            foreach(Question question in questions.Values) {
                question.SortAnswers();
            }
        }

        private void AddQuizLevelQuestionRelations(DataTable table) {
            foreach (DataRow row in table.Rows) {
                int quizLevelId = (int)row[0];
                int questionId = (int)row[1];
                int order = (int)row[2];
                quizLevels[quizLevelId].AddQuestion((order, questions[questionId]));
            }
            foreach (QuizLevel quizLevel in quizLevels.Values) {
                quizLevel.SortQuestions();
            }
        }

        private void AddQuizQuizLevelRelations(DataTable table) {
            foreach (DataRow row in table.Rows) {
                int quizId = (int)row[2];
                int quizLevelId = (int)row[0];
                quizzes[quizId].AddQuizLevel(quizLevels[quizLevelId]);
            }
        }

        private void AddDescriptionDescriptionLevelRelations(DataTable table) {
            foreach (DataRow row in table.Rows) {
                int descriptionId = (int)row[4];
                int descriptionLevelId = (int)row[0];
                descriptions[descriptionId].AddDescriptionLevel(descriptionLevels[descriptionLevelId]);
            }
        }

        private void AddCourseSectionRelations(DataTable table) {
            foreach (DataRow row in table.Rows) {
                int courseId = (int)row[0];
                int sectionId = (int)row[1];
                int order = (int)row[2];
                courses[courseId].AddSection((order, sections[sectionId]));
            }
            foreach(Course course in courses.Values) {
                course.SortSections();
            }
        }

        protected virtual void ReadCourses() {
            string possibleAnswersTable = "possibleAnswers";
            string questionsTable = "questions";
            string quizLevelsTable = "quizLevels";
            string quizTable = "quiz";
            string descriptionLevelCategoriesTable = "descriptionLevelCategories";
            string descriptionLevelsTable = "descriptionLevels";
            string descriptionTable = "description";
            string sectionsTable = "sections";
            string courseCategoriesTable = "courseCategories";
            string coursesTable = "courses";

            string questionAnswerRelationsTable = "questionAnswerRelations";
            string quizLevelQuestionRelationsTable = "quizLevelQuestionRelations";
            string courseSectionRelations = "courseSectionRelations";


            Dictionary<string, string> QueryData = new Dictionary<string, string>();
            QueryData.Add("SELECT * FROM answers;", possibleAnswersTable);
            QueryData.Add("SELECT * FROM questions", questionsTable);
            QueryData.Add("SELECT * FROM quiz_levels", quizLevelsTable);
            QueryData.Add("SELECT * FROM quizzes", quizTable);
            QueryData.Add("SELECT * FROM description_categories", descriptionLevelCategoriesTable);
            QueryData.Add("SELECT * FROM description_levels", descriptionLevelsTable);
            QueryData.Add("SELECT * FROM descriptions", descriptionTable);
            QueryData.Add("SELECT * FROM sections", sectionsTable);
            QueryData.Add("SELECT * FROM categories", courseCategoriesTable);
            QueryData.Add("SELECT * FROM courses", coursesTable);

            QueryData.Add("SELECT * FROM questions_answers;", questionAnswerRelationsTable);
            QueryData.Add("SELECT * FROM quiz_levels_questions;", quizLevelQuestionRelationsTable);
            QueryData.Add("SELECT * FROM courses_sections", courseSectionRelations);

            DataSet DBData = GetData(QueryData);

            GetAnswers(DBData.Tables[possibleAnswersTable]);
            GetQuestions(DBData.Tables[questionsTable]);
            GetQuizLevels(DBData.Tables[quizLevelsTable]);
            GetQuizzes(DBData.Tables[quizTable]);
            GetDescriptionLevelCategories(DBData.Tables[descriptionLevelCategoriesTable]);
            GetDescriptionLevels(DBData.Tables[descriptionLevelsTable]);
            GetDescriptions(DBData.Tables[descriptionTable]);
            GetSections(DBData.Tables[sectionsTable]);
            GetCourseCategories(DBData.Tables[courseCategoriesTable]);
            GetCourses(DBData.Tables[coursesTable]);

            AddQuestionAnswerRelations(DBData.Tables[questionAnswerRelationsTable]);
            AddQuizLevelQuestionRelations(DBData.Tables[quizLevelQuestionRelationsTable]);
            AddQuizQuizLevelRelations(DBData.Tables[quizLevelsTable]);
            AddDescriptionDescriptionLevelRelations(DBData.Tables[descriptionLevelsTable]);
            AddCourseSectionRelations(DBData.Tables[courseSectionRelations]);
        }

        private List<Course> CreateStructure() {
            foreach (Course course in courses.Values) {
                foreach(Section section in course.sections) {
                    if ( section.parent != 0 ) {
                        sections[section.parent].children.Add(section);
                    }
                }

                course.sections = course.sections.Where(s => s.parent == 0).ToList();
            }

            return courses.Values.ToList();

        }

        public void Serialize() {
            string json = JsonConvert.SerializeObject(
                CreateStructure(),
                new JsonSerializerSettings {
                    NullValueHandling = NullValueHandling.Ignore
                }
            );
            json = JValue.Parse(json).ToString(Formatting.Indented);
            File.WriteAllText(jsonFile, json);
        }

        public void run() {
            connect();
            ReadCourses();
            CloseConnection();
            Serialize();
        }
    }
}