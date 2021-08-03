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
        protected List<Course> courses;
        private List<NpgsqlCommand> queries;
        public Writer()
        {
            courses = new List<Course>();
            queries = new List<NpgsqlCommand>();
        }

        private void GiveIds() {
            int answerId = 0;
            int courseId = 0;
            int courseCategoryId = 0;
            int descriptionId = 0;
            int descriptionLevelId = 0;
            int descriptionLevelCategory = 0;
            int questionId = 0;
            int quizId = 0;
            int quizLevelId = 0;
            int sectionId = 0;

            List<string> courseCategories = new List<string>();
            List<string> descriptionLevelCategories = new List<string>();

            foreach (Course course in courses) {
                foreach (Section section in course.sections) {
                    section.id = sectionId;
                    sectionId++;
                }
            }

            foreach (Course course in courses) {
                course.id = courseId;
                courseId++;

                int numberOfCourseCategories = courseCategories.Count;
                bool found = false;
                for (int i = 0; i < numberOfCourseCategories; i++) {
                    if (courseCategories[i] == course.category.category) {
                        course.category.id = i;
                        found = true;
                        break;
                    }
                }

                if (!found) {
                    course.category.id = courseCategoryId;
                    courseCategories.Add(course.category.category);
                    courseCategoryId++;
                }

                foreach (Section section in course.sections) {
                    if ( section.children != null ) {
                        foreach (Section child in section.children) {
                            child.parent = section.id;
                        }
                    }

                    if (section.description != null) {
                        section.description.id = descriptionId;
                        descriptionId++;
                        foreach (DescriptionLevel level in section.description.levels) {
                            level.id = descriptionLevelId;
                            descriptionLevelId++;

                            if ( level.category != null ) {
                                int numberOfDescriptionLevelCategories = descriptionLevelCategories.Count;
                                found = false;
                                for (int i = 0; i < numberOfDescriptionLevelCategories; i++) {
                                    if (descriptionLevelCategories[i] == level.category.category) {
                                        level.category.id = i;
                                        found = true;
                                        break;
                                    }
                                }

                                if (!found) {
                                    level.category.id = descriptionLevelCategory;
                                    descriptionLevelCategories.Add(level.category.category);
                                    descriptionLevelCategory++;
                                }
                            }
                        }
                    } else {
                        section.quiz.id = quizId;
                        quizId++;
                        foreach (QuizLevel level in section.quiz.levels) {
                            level.id = quizLevelId;
                            quizLevelId++;
                            foreach (Question question in level.questions) {
                                question.id = questionId;
                                questionId++;
                                foreach (Answer answer in question.possibleAnswers) {
                                    answer.id = answerId;
                                    answerId++;
                                    if ( answer.answer == question.correct.answer ) {
                                        question.correct.id = answer.id;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private void FlattenSections(List<Section> newSectionList, List<Section> sectionsInWrittenOrder, Section current, int depth) {
            current.depth = depth;
            sectionsInWrittenOrder.Add(current);
            if ( current.children != null ) {
                foreach (Section section in current.children) {
                    FlattenSections(newSectionList, sectionsInWrittenOrder, section, depth + 1);
                }
            }
            newSectionList.Add(current);
        }

        private void ReStructureSections() {
            foreach (Course course in courses) {
                List<Section> newSectionList = new List<Section>();
                List<Section> sectionsInWrittenOrder = new List<Section>();
                foreach (Section section in course.sections) {
                    FlattenSections(newSectionList, sectionsInWrittenOrder, section, 0);
                }
                course.sections = sectionsInWrittenOrder;
                //course.sections = newSectionList;
            }
        }

        private void ReadCourses()
        {
            string json = File.ReadAllText(jsonFile);
            courses = JsonConvert.DeserializeObject<List<Course>>(json);

            ReStructureSections();

            // TODO: Remove. Temporary for 1 levels only for sake of ease
            foreach (Course course in courses) {
                foreach (Section section in course.sections) {
                    if (section.description != null) {
                        section.description.levels = new List<DescriptionLevel> { section.description.levels[0] };
                    } else {
                        section.quiz.levels = new List<QuizLevel> { section.quiz.levels[0] };
                    }
                }
            }

            GiveIds();
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

        protected virtual void Write()
        {
            GetQueries();

            foreach (NpgsqlCommand query in queries)
            {
                query.ExecuteNonQuery();
            }
        }

        public void run()
        {
            connect();
            ReadCourses();
            Write();
            CloseConnection();
        }
    }
}
