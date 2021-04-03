using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Npgsql;

namespace DBConnect {
    public class Course {
        public int id {
            get; set;
        }
        public string title {
            get; set;
        }
        public string description {
            get; set;
        }
        public CourseCategory category {
            get; set;
        }
        private List<(int, Section)> sectionsData {
            get; set;
        }

        public List<Section> sections {
            get {
                return sectionsData.Select(x => x.Item2).ToList();
            }
            set
            {
                if (value != null) {
                    sectionsData = new List<(int, Section)>();
                    for (int i = 1; i <= value.Count; i++) {
                        sectionsData.Add((i, value[i-1]));
                    }
                }
            }
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

        [JsonConstructor]
        public Course(int _id, string _title, string _description, List<Section> _sections)
        {
            id = _id;
            title = _title;
            description = _description;
            sections = _sections;
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

        public void GetQuery(List<NpgsqlCommand> queries, NpgsqlConnection connection)
        {
            List<Section> _sections = sections;
            List<Section> parent_sorted_sections = new List<Section>();

            // Sorting sections such that no section is added before it's parent
            bool inserted = true;
            while (inserted)
            {
                inserted = false;
                foreach (Section section in _sections)
                {
                    if (parent_sorted_sections.All(s => s.id != section.id) && (section.parent == 0 || parent_sorted_sections.Any(s => s.id == section.parent)))
                    {
                        inserted = true;
                        parent_sorted_sections.Add(section);
                    }
                }
            }
            _sections = parent_sorted_sections;


            foreach (Section section in _sections)
            {
                section.GetQuery(queries, connection);
            }
            category.GetQuery(queries, connection);

            NpgsqlCommand query = new NpgsqlCommand("INSERT INTO courses(id, title, description, category) VALUES(@id, @title, @description, @category)", connection);
            query.Parameters.AddWithValue("id", id);
            query.Parameters.AddWithValue("title", title);
            query.Parameters.AddWithValue("description", description);
            query.Parameters.AddWithValue("category", category?.id);
            queries.Add(query);
            for (int i = 0; i < _sections.Count; i++)
            {
                query = new NpgsqlCommand("INSERT INTO courses_sections(course_id, section_id, \"order\") VALUES (@course_id, @section_id, @order)", connection);
                query.Parameters.AddWithValue("course_id", id);
                query.Parameters.AddWithValue("section_id", _sections[i].id);
                query.Parameters.AddWithValue("order", i);
                queries.Add(query);
            }
        }
    }
}
