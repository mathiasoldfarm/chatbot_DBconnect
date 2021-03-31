using System;
using System.Data;
using System.Collections.Generic;

namespace DBConnect {
    public class Section {
        public int id {
            get; private set;
        }
        public string name {
            get; private set;
        }
        public Quiz quiz {
            get; private set;
        }
        public Description description {
            get; private set;
        }
        public Section parent {
            get; private set;
        }

        public Section(DataRow row) {
            try {
                id = (int)row[0];
                name = (string)row[1];
            }
            catch {
                throw new Exception("Constructor argument DataRow was expected to have two arguments of type int string");
            }
        }

        public void AddDescription(Description _description) {
            if ( quiz != null ) {
                throw new Exception("Description cannot be set if quiz is not null");
            } else {
                description = _description;
            }
        }

        public void AddQuiz(Quiz _quiz) {
            if (description != null) {
                throw new Exception("Quiz cannot be set if description is not null");
            }
            else {
                quiz = _quiz;
            }
        }

        public void AddParent(Section section) {
            parent = section;
        }
    }
}
