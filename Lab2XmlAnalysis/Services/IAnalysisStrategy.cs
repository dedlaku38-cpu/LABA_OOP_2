// File: Services/IAnalysisStrategy.cs
using Lab2XmlAnalysis.Models;

namespace Lab2XmlAnalysis.Services
{
    // Defines the contract for all analysis strategies
    public interface IAnalysisStrategy
    {
        // Each strategy must implement this method
        List<Material> Analyze(SearchCriteria criteria, string xmlContent);
    }
}