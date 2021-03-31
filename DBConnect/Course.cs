using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;

namespace DBConnect {
    public class Course {
        public int id {
            get; private set;
        }
        public string title {
            get; private set;
        }
        public string description {
            get; private set;
        }
        public CourseCategory category {
            get; private set;
        }
        private List<(int, Section)> sectionsData {
            get; set;
        }

        public List<Section> sections {
            get {
                return sectionsData.Select(x => x.Item2).ToList();
            } set {}
        }   

        public Course(DataRow row) {
            try {
                id = (int)row[0];
                title = (string)row[1];
                description = (string)row[2];
                sectionsData = new List<(int, Section)>();
            }
            catch {
                throw new Exception("Constructor argument DataRow was expected to have three arguments of type int, string, string");
            }
        }

        public void AddCategory(CourseCategory _category) {
            category = _category;
        }

        public void AddSection((int, Section) items) {
            sectionsData.Add(items);
        }

        public void SortSections() {
            sectionsData.Sort((x, y) => y.Item1.CompareTo(x.Item1));
        }
    }
}
