using Lab2XmlAnalysis.Models;
using System.Xml;
using System.IO;

namespace Lab2XmlAnalysis.Services
{
    public class SaxAnalysisStrategy : IAnalysisStrategy
    {
        public List<Material> Analyze(SearchCriteria criteria, string xmlContent)
        {
            var materials = new List<Material>();
            if (string.IsNullOrEmpty(xmlContent)) return materials;

            using var stringReader = new StringReader(xmlContent);
            using var reader = XmlReader.Create(stringReader);

            Material? currentMaterial = null;
            Author? currentAuthor = null;
            string currentElement = "";
            bool keepThisMaterial = true;

            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.Element)
                {
                    currentElement = reader.Name;

                    if (currentElement == "Material")
                    {
                        currentMaterial = new Material();
                        currentAuthor = new Author();
                        currentMaterial.Author = currentAuthor;
                        keepThisMaterial = true;

                        if (reader.HasAttributes)
                        {
                            currentMaterial.Title = reader.GetAttribute("Title") ?? "";
                            currentMaterial.Type = reader.GetAttribute("Type") ?? "";
                            currentMaterial.CreationDate = reader.GetAttribute("CreationDate") ?? "";
                            currentMaterial.Volume = reader.GetAttribute("Volume") ?? "";

                            // === ФІЛЬТР ПО НАЗВІ ===
                            if (!string.IsNullOrEmpty(criteria.Title) &&
                                !currentMaterial.Title.Contains(criteria.Title, StringComparison.OrdinalIgnoreCase))
                            {
                                keepThisMaterial = false;
                            }
                        }
                    }
                }
                else if (reader.NodeType == XmlNodeType.Text)
                {
                    if (currentMaterial == null) continue;

                    switch (currentElement)
                    {
                        case "FullName":
                            currentAuthor!.FullName = reader.Value;
                            if (!string.IsNullOrEmpty(criteria.AuthorFullName) &&
                                !currentAuthor.FullName.Contains(criteria.AuthorFullName, StringComparison.OrdinalIgnoreCase))
                            {
                                keepThisMaterial = false;
                            }
                            break;
                        case "Faculty":
                            currentAuthor!.Faculty = reader.Value;
                            if (!string.IsNullOrEmpty(criteria.Faculty) && criteria.Faculty != "Всі факультети" &&
                                currentAuthor.Faculty != criteria.Faculty)
                            {
                                keepThisMaterial = false;
                            }
                            break;
                        case "Department":
                            currentAuthor!.Department = reader.Value;
                            if (!string.IsNullOrEmpty(criteria.Department) && criteria.Department != "Всі кафедри" &&
                                currentAuthor.Department != criteria.Department)
                            {
                                keepThisMaterial = false;
                            }
                            break;
                    }
                }
                else if (reader.NodeType == XmlNodeType.EndElement)
                {
                    if (reader.Name == "Material")
                    {
                        if (currentMaterial != null && keepThisMaterial)
                        {
                            materials.Add(currentMaterial);
                        }
                        currentMaterial = null;
                        currentAuthor = null;
                        keepThisMaterial = true;
                    }
                }
            }

            return materials;
        }
    }
}