using System;
using System.IO;
using System.Net.Http;
using Newtonsoft.Json;
using System.Threading.Tasks;

public class Result
{
    public static async Task<int> getTotalGoals(string team, int year)
    {
        const string apiBase = "https://jsonmock.hackerrank.com/api/football_matches";
        int totalGoals = 0;

        using (HttpClient client = new HttpClient())
        {
            // Fetch data where the specified team is team1
            totalGoals += await FetchTeamGoalsAsync(client, apiBase, team, year, "team1");

            // Fetch data where the specified team is team2
            totalGoals += await FetchTeamGoalsAsync(client, apiBase, team, year, "team2");
        }

        return totalGoals;
    }

    private static async Task<int> FetchTeamGoalsAsync(HttpClient client, string apiBase, string team, int year, string teamParam)
    {
        int totalGoals = 0;
        int page = 1;

        while (true)
        {
            string url = $"{apiBase}?year={year}&{teamParam}={team}&page={page}";
            HttpResponseMessage response = await client.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                break;
            }

            string content = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<MatchResult>(content);

            foreach (var match in result.data)
            {
                totalGoals += ParseGoals(match, team, teamParam);
            }

            page++;

            if (page > result.total_pages)
            {
                break;
            }
        }

        return totalGoals;
    }

    private static int ParseGoals(Match match, string team, string teamParam)
    {
        if (teamParam == "team1" && match.team1.ToLower() == team.ToLower())
        {
            return int.Parse(match.team1goals);
        }
        else if (teamParam == "team2" && match.team2.ToLower() == team.ToLower())
        {
            return int.Parse(match.team2goals);
        }
        return 0;
    }
}

public class Match
{
    public string team1 { get; set; }
    public string team2 { get; set; }
    public string team1goals { get; set; }
    public string team2goals { get; set; }
}

public class MatchResult
{
    public int total { get; set; }
    public int total_pages { get; set; }
    public Match[] data { get; set; }
}

class Solution
{
    public static void Main(string[] args)
    {
        TextWriter textWriter = new StreamWriter(@System.Environment.GetEnvironmentVariable("OUTPUT_PATH"), true);

        string team = Console.ReadLine();
        int year = Convert.ToInt32(Console.ReadLine().Trim());

        int result = Result.getTotalGoals(team, year).Result;
        textWriter.WriteLine(result);

        textWriter.Flush();
        textWriter.Close();
    }
}
