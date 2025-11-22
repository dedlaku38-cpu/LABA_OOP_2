// File: Models/Material.cs
namespace Lab2XmlAnalysis.Models
{
    // Represents the Author entity
    public class Author
    {
        public string FullName { get; set; } = string.Empty;
        public string Faculty { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
    }

    // Represents the Material entity from XML
    public class Material
    {
        public string Title { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string CreationDate { get; set; } = string.Empty;
        public string Volume { get; set; } = string.Empty;

        // A material can have one author in this structure
        public Author Author { get; set; } = new Author();

        // Helper property for easy display in UI
        public string DisplayInfo =>
            $"{Title} ({Type})\n" +
            $"Автор: {Author.FullName}\n" +
            $"Факультет: {Author.Faculty}, Кафедра: {Author.Department}\n" +
            $"Дата: {CreationDate}, Обсяг: {Volume}\n";
    }
}