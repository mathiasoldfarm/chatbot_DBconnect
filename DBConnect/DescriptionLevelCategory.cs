using System;
using System.Data;

namespace DBConnect {
    public class DescriptionLevelCategory {
        public int id {
            get; private set;
        }
        public string category {
            get; private set;
        }

        public DescriptionLevelCategory(DataRow row) {
            try {
                id = (int)row[0];
                category = (string)row[1];
            }
            catch {
                throw new Exception("Constructor argument DataRow was expected to have three arguments of type int, string string");
            }
        }
    }
}
