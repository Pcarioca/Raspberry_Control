using System;
using System.IO;

namespace ConsoleApp1;

public static class HtmlLoader
{
    /// <summary>
    /// Loads the HTML content from a file.
    /// </summary>
    /// <param name="fileName">The name of the HTML file to load (e.g., "index.html")</param>
    /// <returns>The HTML content as a string</returns>
    public static string LoadHtml(string fileName)
    {
        try
        {
            // Get the directory where the application is running
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string filePath = Path.Combine(baseDirectory, fileName);
            
            // If file not found in base directory, try the project root
            if (!File.Exists(filePath))
            {
                // Go up to find the project root
                string? projectRoot = Directory.GetParent(baseDirectory)?.Parent?.Parent?.Parent?.FullName;
                if (projectRoot != null)
                {
                    filePath = Path.Combine(projectRoot, fileName);
                }
            }
            
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"HTML file '{fileName}' not found at {filePath}");
            }
            
            return File.ReadAllText(filePath);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading HTML file: {ex.Message}");
            return $"<html><body><h1>Error loading page</h1><p>{ex.Message}</p></body></html>";
        }
    }
}
