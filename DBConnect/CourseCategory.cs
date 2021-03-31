using System;
using System.Data;

namespace DBConnect {
    public class CourseCategory {
        public int id {
            get; private set;
        }
        public string category {
            get; private set;
        }

        public CourseCategory(DataRow row) {
            try {
                id = (int)row[0];
                category = (string)row[1];
            }
            catch {
                throw new Exception("Constructor argument DataRow was expected to have two arguments of type int, string");
            }
        }
    }
}
