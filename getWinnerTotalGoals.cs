using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.Json;

class Result
{
    public static int getWinnerTotalGoals(string competition, int year)
    {
        string winner = GetCompetitionWinner(competition, year);
        int totalGoals = GetTotalGoals(winner, year, competition);
        return totalGoals;
    }

    private static string GetCompetitionWinner(string competition, int year)
    {
        const string competitionsApi = "https://jsonmock.hackerrank.com/api/football_competitions";
        string winner = "";

        using (HttpClient client = new HttpClient())
        {
            var parameters = new Dictionary<string, string>
            {
                { "name", competition },
                { "year", year.ToString() }
            };

            HttpResponseMessage response = client.GetAsync(competitionsApi + ToQueryString(parameters)).Result;

            if (response.IsSuccessStatusCode)
            {
                var content = response.Content.ReadAsStringAsync().Result;
                var result = JsonSerializer.Deserialize<CompetitionResult>(content);

                if (result.data.Length > 0)
                {
                    winner = result.data[0].winner;
                }
            }
        }

        return winner;
    }

    private static int GetTotalGoals(string team, int year, string competition)
    {
        int totalGoals = 0;

        totalGoals += HandleData(team, 1, year, competition);
        totalGoals += HandleData(team, 2, year, competition);

        return totalGoals;
    }

    private static int HandleData(string team, int whichTeam, int year, string competition)
    {
        string teamNum = $"team{whichTeam}goals";
        int goals = 0;
        int totalPages = 1;

        using (HttpClient client = new HttpClient())
        {
            while (totalPages <= 100) // Adjust the limit as needed
            {
                var parameters = new Dictionary<string, string>
                {
                    { "year", year.ToString() },
                    { $"team{whichTeam}", team },
                    { "page", totalPages.ToString() },
                    { "competition", competition }
                };

                HttpResponseMessage response = client.GetAsync("https://jsonmock.hackerrank.com/api/football_matches" + ToQueryString(parameters)).Result;

                if (!response.IsSuccessStatusCode)
                {
                    break;
                }

                var content = response.Content.ReadAsStringAsync().Result;
                var result = JsonSerializer.Deserialize<MatchResult>(content);

                foreach (var match in result.data)
                {
                    goals += ParseGoals(match, teamNum);
                }

                totalPages++;

                if (totalPages > result.total_pages)
                {
                    break;
                }
            }
        }

        return goals;
    }

    private static int ParseGoals(Match match, string teamNum)
    {
        int goals = 0;

        if (int.TryParse(match.team1goals, out int team1Goals) && int.TryParse(match.team2goals, out int team2Goals))
        {
            if (teamNum == "team1goals")
            {
                goals = team1Goals;
            }
            else if (teamNum == "team2goals")
            {
                goals = team2Goals;
            }
        }

        return goals;
    }

    private static string ToQueryString(Dictionary<string, string> parameters)
    {
        var keyValuePairs = new List<string>();
        foreach (var kvp in parameters)
        {
            keyValuePairs.Add($"{Uri.EscapeDataString(kvp.Key)}={Uri.EscapeDataString(kvp.Value)}");
        }
        return "?" + string.Join("&", keyValuePairs);
    }
}

public class CompetitionResult
{
    public CompetitionData[] data { get; set; }
}

public class CompetitionData
{
    public string winner { get; set; }
}

public class MatchResult
{
    public int total_pages { get; set; }
    public Match[] data { get; set; }
}

public class Match
{
    public string team1goals { get; set; }
    public string team2goals { get; set; }
}

class Solution
{
    public static void Main(string[] args)
    {
        string competition = Console.ReadLine();
        int year = Convert.ToInt32(Console.ReadLine().Trim());

        int result = Result.getWinnerTotalGoals(competition, year);
        Console.WriteLine(result);
    }
}
