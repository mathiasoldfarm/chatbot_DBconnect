using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DBConnect {
    public class TextReader : Reader {
        private List<Section> MakeStructure(List<Section> sections) {
            List<Section> newSections = new List<Section>();
            Dictionary<int, Section> depthMostRecent = new Dictionary<int, Section>();

            foreach(Section section in sections) {
                if (section.depth == 0 && depthMostRecent.ContainsKey(0)) {
                    newSections.Add(depthMostRecent[0]);
                }
                depthMostRecent[section.depth] = section;
                if ( section.depth != 0 ) {
                    Section parent = depthMostRecent[section.depth - 1];
                    if ( parent.children == null ) {
                        parent.children = new List<Section>();
                    }
                    parent.children.Add(section);
                }
            }
            if (depthMostRecent.ContainsKey(0)) {
                newSections.Add(depthMostRecent[0]);
            }


            return newSections;
        }

        protected override void ReadCourses() {
            string[] files = Directory.GetFiles("./../../../../../courses/", "" , SearchOption.AllDirectories);
            List<Course> coursesTemp = new List<Course>();
            foreach( string filename in files ) {
                if ( !filename.Contains(".DS_Store") ) {
                    Course course;
                    CourseCategory courseCategory;

                    string[] content = File.ReadAllLines(filename);

                    string courseTitle = content[0].Replace("Course: ", "");
                    string courseDescription = content[0].Replace("Course description:", "");
                    string courseCategoryCategory = content[2].Replace("Category: ", "");
                    string courseCategoryColor = content[3].Replace("Color: ", "");

                    courseCategory = new CourseCategory(courseCategoryCategory, courseCategoryColor);

                    course = new Course(courseTitle, courseDescription, new List<Section>());
                    course.category = courseCategory;

                    List<Section> sections = new List<Section>();

                    string descriptionTitle = "";
                    string descriptionDescription = "";
                    string quizTitle = "";
                    List<Question> quizQuestions = new List<Question>();
                    int currentDepth = 0;

                    for (int i = 6; i < content.Length; i++) {
                        string line = content[i];
                        if (line.Contains("Text: ") || line.Contains("Quiz: ")) {
                            currentDepth = line.Where(c => c == '\t').Count() / 2;
                        } 
                        
                        line = line.Replace("\n", "").Replace("\t", "");

                        if (line.Contains("Text: ")) {
                            if (descriptionTitle != "" || quizTitle != "") {
                                throw new Exception("Title already set");
                            }
                            descriptionTitle = line.Replace("Text: ", "");
                        } else if (line.Contains("Quiz: ")) {
                            if (descriptionTitle != "" || quizTitle != "") {
                                throw new Exception("Title already set");
                            }
                            quizTitle = line.Replace("Quiz: ", "");
                        } else if (line.Contains("__________________________")) {
                            Section section;
                            if (descriptionTitle != "") {
                                section = new Section(descriptionTitle, currentDepth);
                                DescriptionLevel level = new DescriptionLevel(descriptionDescription, 1);
                                Description description = new Description(new List<DescriptionLevel> { level });
                                section.description = description;
                            } else if (quizTitle != "") {
                                section = new Section(quizTitle, currentDepth);
                                QuizLevel level = new QuizLevel(quizQuestions, 1);
                                Quiz quiz = new Quiz(new List<QuizLevel> { level });
                                section.quiz = quiz;
                            } else {
                                throw new Exception("No titles set");
                            }
                            sections.Add(section);

                            // Init values
                            descriptionTitle = "";
                            descriptionDescription = "";
                            quizTitle = "";
                            quizQuestions = new List<Question>();
                            currentDepth = 0;
                        } else if (quizTitle != "") {
                            string questionLine = line;
                            string possibleAnswer1 = content[i + 1].Replace("\n", "").Replace("\t", "");
                            string possibleAnswer2 = content[i + 2].Replace("\n", "").Replace("\t", "");
                            string possibleAnswer3 = content[i + 3].Replace("\n", "").Replace("\t", "");
                            string possibleAnswer4 = content[i + 4].Replace("\n", "").Replace("\t", "");

                            string correct = "";
                            if (possibleAnswer1.Contains(" <- Correct")) {
                                possibleAnswer1 = possibleAnswer1.Replace(" <- Correct", "");
                                correct = possibleAnswer1;
                            } else if (possibleAnswer2.Contains(" <- Correct")) {
                                possibleAnswer2 = possibleAnswer2.Replace(" <- Correct", "");
                                correct = possibleAnswer2;
                            } else if (possibleAnswer3.Contains(" <- Correct")) {
                                possibleAnswer3 = possibleAnswer3.Replace(" <- Correct", "");
                                correct = possibleAnswer3;
                            } else if (possibleAnswer4.Contains(" <- Correct")) {
                                possibleAnswer4 = possibleAnswer4.Replace(" <- Correct", "");
                                correct = possibleAnswer4;
                            } else {
                                throw new Exception("Correct could not be found");
                            }

                            string[] answerExplanation = possibleAnswer1.Split(": ");
                            Answer answer1 = new Answer(answerExplanation[0], answerExplanation[1]);

                            answerExplanation = possibleAnswer2.Split(": ");
                            Answer answer2 = new Answer(answerExplanation[0], answerExplanation[1]);

                            answerExplanation = possibleAnswer3.Split(": ");
                            Answer answer3 = new Answer(answerExplanation[0], answerExplanation[1]);

                            answerExplanation = possibleAnswer4.Split(": ");
                            Answer answer4 = new Answer(answerExplanation[0], answerExplanation[1]);

                            answerExplanation = correct.Split(": ");
                            Answer correctAnswer = new Answer(answerExplanation[0], answerExplanation[1]);

                            Question question = new Question(questionLine, new List<Answer> { answer1, answer2, answer3, answer4 });
                            question.AddCorrect(correctAnswer);
                            quizQuestions.Add(question);

                            i += 5;
                        } else {
                            if (descriptionTitle != "" && quizTitle == "") {
                                descriptionDescription += line;
                            } else {
                                if ( !content[i-1].Contains("__________________________") ) {
                                    throw new Exception($"Invalid line: {line}");
                                }
                            }
                        }
                    }
                    course.sections = MakeStructure(sections);
                    coursesTemp.Add(course);
                }
            }
            int n = coursesTemp.Count;
            for( int i = 0; i < n; i++ ) {
                courses[i] = coursesTemp[i];
            }
        }
    }
}
