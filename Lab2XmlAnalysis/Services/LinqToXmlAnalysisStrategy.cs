using Lab2XmlAnalysis.Models;
using System.Xml.Linq;

namespace Lab2XmlAnalysis.Services
{
    public class LinqToXmlAnalysisStrategy : IAnalysisStrategy
    {
        public List<Material> Analyze(SearchCriteria criteria, string xmlContent)
        {
            var materials = new List<Material>();
            if (string.IsNullOrEmpty(xmlContent)) return materials;

            XDocument doc = XDocument.Parse(xmlContent);
            var query = doc.Descendants("Material");

            // === ФІЛЬТРИ ===

            // По Назві
            if (!string.IsNullOrEmpty(criteria.Title))
            {
                query = query.Where(m => m.Attribute("Title")?.Value
                                          .Contains(criteria.Title, StringComparison.OrdinalIgnoreCase) == true);
            }

            // По Автору
            if (!string.IsNullOrEmpty(criteria.AuthorFullName))
            {
                query = query.Where(m => m.Element("Author")?.Element("FullName")?.Value
                                          .Contains(criteria.AuthorFullName, StringComparison.OrdinalIgnoreCase) == true);
            }

            // По Факультету
            if (!string.IsNullOrEmpty(criteria.Faculty) && criteria.Faculty != "Всі факультети")
            {
                query = query.Where(m => m.Element("Author")?.Element("Faculty")?.Value == criteria.Faculty);
            }

            // По Кафедрі
            if (!string.IsNullOrEmpty(criteria.Department) && criteria.Department != "Всі кафедри")
            {
                query = query.Where(m => m.Element("Author")?.Element("Department")?.Value == criteria.Department);
            }

            // Створення об'єктів
            foreach (var matElement in query)
            {
                var authorElement = matElement.Element("Author");
                var material = new Material
                {
                    Title = matElement.Attribute("Title")?.Value ?? "",
                    Type = matElement.Attribute("Type")?.Value ?? "",
                    CreationDate = matElement.Attribute("CreationDate")?.Value ?? "",
                    Volume = matElement.Attribute("Volume")?.Value ?? "",
                    Author = new Author
                    {
                        FullName = authorElement?.Element("FullName")?.Value ?? "",
                        Faculty = authorElement?.Element("Faculty")?.Value ?? "",
                        Department = authorElement?.Element("Department")?.Value ?? ""
                    }
                };
                materials.Add(material);
            }

            return materials;
        }
    }
}