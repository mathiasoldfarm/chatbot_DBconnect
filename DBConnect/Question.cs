using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;

namespace DBConnect
{
    public class Question
    {
        public int id {
            get; private set;
        }
        public string question {
            get; private set;
        }
        public Answer correct { get; private set; }
        private List<(int, Answer)> possibleAnswersData {
            get; set;
        }

        public List<Answer> possibleAnswers {
            get {
                return possibleAnswersData.Select(x => x.Item2).ToList();
            }
            set {
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

        public void AddCorrect(Answer _correct) {
            correct = _correct;
        }

        public void AddAnswer((int, Answer) items) {
            possibleAnswersData.Add(items);
        }

        public void SortAnswers() {
            possibleAnswersData.Sort((x, y) => y.Item1.CompareTo(x.Item1));
        }
    }
}
