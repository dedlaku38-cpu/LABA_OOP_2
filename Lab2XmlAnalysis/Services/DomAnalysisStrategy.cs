using Lab2XmlAnalysis.Models;
using System.Xml;

namespace Lab2XmlAnalysis.Services
{
    public class DomAnalysisStrategy : IAnalysisStrategy
    {
        public List<Material> Analyze(SearchCriteria criteria, string xmlContent)
        {
            var materials = new List<Material>();
            if (string.IsNullOrEmpty(xmlContent)) return materials;

            var doc = new XmlDocument();
            doc.LoadXml(xmlContent);

            XmlNodeList materialNodes = doc.SelectNodes("/Archive/Material");
            if (materialNodes == null) return materials;

            foreach (XmlNode matNode in materialNodes)
            {
                var authorNode = matNode.SelectSingleNode("Author");
                if (authorNode == null) continue;

                string authorFullName = authorNode.SelectSingleNode("FullName")?.InnerText ?? "";
                string faculty = authorNode.SelectSingleNode("Faculty")?.InnerText ?? "";
                string department = authorNode.SelectSingleNode("Department")?.InnerText ?? "";

                // Отримуємо назву для фільтрації
                string title = matNode.Attributes?["Title"]?.Value ?? "";

                // === ФІЛЬТРИ ===

                // По Назві
                if (!string.IsNullOrEmpty(criteria.Title))
                {
                    if (!title.Contains(criteria.Title, StringComparison.OrdinalIgnoreCase)) continue;
                }

                // По Автору
                if (!string.IsNullOrEmpty(criteria.AuthorFullName))
                {
                    if (!authorFullName.Contains(criteria.AuthorFullName, StringComparison.OrdinalIgnoreCase)) continue;
                }

                // По Факультету
                if (!string.IsNullOrEmpty(criteria.Faculty) && criteria.Faculty != "Всі факультети")
                {
                    if (faculty != criteria.Faculty) continue;
                }

                // По Кафедрі
                if (!string.IsNullOrEmpty(criteria.Department) && criteria.Department != "Всі кафедри")
                {
                    if (department != criteria.Department) continue;
                }

                // Якщо пройшли всі фільтри - створюємо об'єкт
                var material = new Material
                {
                    Title = title,
                    Type = matNode.Attributes?["Type"]?.Value ?? "",
                    CreationDate = matNode.Attributes?["CreationDate"]?.Value ?? "",
                    Volume = matNode.Attributes?["Volume"]?.Value ?? "",
                    Author = new Author
                    {
                        FullName = authorFullName,
                        Faculty = faculty,
                        Department = department
                    }
                };
                materials.Add(material);
            }

            return materials;
        }
    }
}