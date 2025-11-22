// File: Models/SearchCriteria.cs
namespace Lab2XmlAnalysis.Models
{
    // Holds all parameters for our search
    public class SearchCriteria
    {
        // We will search by these fields
        public string? Title { get; set; }
        public string? Faculty { get; set; }
        public string? Department { get; set; }
        public string? AuthorFullName { get; set; }
    }
}