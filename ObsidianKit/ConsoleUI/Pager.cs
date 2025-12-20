namespace ObsidianKit.ConsoleUI;

/// <summary>
/// Provides pagination functionality for console output
/// </summary>
public static class Pager
{
    /// <summary>
    /// Display lines with pagination support. User can press Enter/Space to see more, or Q to quit.
    /// </summary>
    /// <param name="lines">Lines to display</param>
    /// <param name="pageSize">Number of lines per page (default: auto-detect based on console height)</param>
    public static void DisplayWithPagination(IEnumerable<string> lines, int? pageSize = null)
    {
        var linesList = lines.ToList();
        if (!linesList.Any())
            return;

        // Auto-detect page size based on console height, leave space for prompt
        var actualPageSize = pageSize ?? (Console.WindowHeight - 3);
        if (actualPageSize <= 0)
            actualPageSize = 20; // Fallback

        var currentIndex = 0;
        var totalLines = linesList.Count;

        while (currentIndex < totalLines)
        {
            var remainingLines = totalLines - currentIndex;
            var linesToShow = Math.Min(actualPageSize, remainingLines);

            // Display current page
            for (int i = 0; i < linesToShow; i++)
            {
                Console.WriteLine(linesList[currentIndex + i]);
            }

            currentIndex += linesToShow;

            // If there are more lines, show prompt
            if (currentIndex < totalLines)
            {
                var previousColor = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Write($"-- More ({currentIndex}/{totalLines}) -- Press Enter/Space for more, Q to quit: ");
                Console.ForegroundColor = previousColor;

                var key = Console.ReadKey(intercept: true);
                Console.WriteLine(); // Move to next line

                if (key.Key == ConsoleKey.Q || key.Key == ConsoleKey.Escape)
                {
                    break;
                }
                // Enter, Space, or Down arrow continues
            }
        }
    }

    /// <summary>
    /// Ask user if they want to see all results when count is large
    /// </summary>
    /// <param name="count">Number of items to display</param>
    /// <param name="threshold">Threshold to trigger confirmation (default: 50)</param>
    /// <returns>True if user wants to see all, false otherwise</returns>
    public static bool ConfirmLargeOutput(int count, int threshold = 50)
    {
        if (count <= threshold)
            return true;

        var previousColor = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.Write($"Found {count} items. Display all? (y/n): ");
        Console.ForegroundColor = previousColor;

        var key = Console.ReadKey(intercept: true);
        Console.WriteLine();

        return key.Key == ConsoleKey.Y;
    }
}
