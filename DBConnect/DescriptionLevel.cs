using System;
using System.Data;

namespace DBConnect {
    public class DescriptionLevel {
        public int id {
            get; private set;
        }
        public string description {
            get; private set;
        }
        public int level {
            get; private set;
        }
        public DescriptionLevelCategory category {
            get; private set;
        }

        public DescriptionLevel(DataRow row) {
            try {
                id = (int)row[0];
                description = (string)row[1];
                level = (int)row[2];
            }
            catch {
                throw new Exception("Constructor argument DataRow was expected to have two arguments of type int, string, int");
            }
        }

        public void AddCategory(DescriptionLevelCategory _category) {
            category = _category;
        }
    }
}
