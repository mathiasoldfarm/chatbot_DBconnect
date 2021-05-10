using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using System.Threading;
using System.Web;

namespace DBConnect {
    public class TextWriter : Writer {

        protected override void Write() {
            foreach(Course course in courses) {
                string filename = @$"{course.category.category}-{course.title.ToLower().Replace(" ", "-")}";
                string path = $"../../../../../courses/{filename}.txt";
                if ( File.Exists(path) ) {
                    File.WriteAllText(path, string.Empty);
                }
                using (StreamWriter writer = File.AppendText(path)) {
                    writer.WriteLine($"Course: {course.title}");
                    writer.WriteLine($"Course description: {course.description}");
                    writer.WriteLine($"Category: {course.category.category}");
                    writer.WriteLine($"Color: {course.category.colorClass}");

                    if (course.SectionsInWrittenOrder.Count > 0) {
                        writer.WriteLine("");
                        writer.WriteLine("");
                    }

                    foreach (Section section in course.SectionsInWrittenOrder) {
                        string indent = "";
                        for ( int i = 0; i < section.depth; i++ ) {
                            indent += "\t\t";
                        }

                        if ( section.description != null ) {
                            writer.WriteLine(indent + $"Text: {section.name}");
                            // TODO: Handle multiple levels
                            string text = section.description.levels[0].description;
                            writer.WriteLine(indent + text.Replace("\n", "\n" + indent));
                        } else {
                            writer.WriteLine(indent + $"Quiz: {section.name}");
                            // TODO: Handle multiple levels
                            int n = section.quiz.levels[0].questions.Count;
                            int counter = 0;
                            foreach (Question question in section.quiz.levels[0].questions) {
                                writer.WriteLine(indent + question.question);
                                foreach(Answer answer in question.possibleAnswers) {
                                    string answerLine = answer.answer + ": " + answer.explanation;
                                    if ( answer.id == question.correct.id ) {
                                        answerLine += " <- Correct";
                                    }
                                    writer.WriteLine(indent + answerLine);
                                }
                                if ( counter < n - 1 ) {
                                    writer.WriteLine(indent);
                                }

                            }
                        }
                        writer.WriteLine($"__________________________");
                        writer.WriteLine("");
                    }
                };
            }
        }
    }
}
