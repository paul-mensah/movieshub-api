namespace MoviesHub.Api.Helpers;

public static class ShuffleResponseList
{
    public static List<T> GetRandomMovies<T>(this List<T> moviesList, int limit) where T : class
    {
        var random = new Random();
        var totalCount = moviesList.Count;

        var shuffledList = moviesList.OrderBy(m => random.Next(totalCount));
        
        // Get top ten movies after shuffling movies list
        var topTenInShuffledList = shuffledList.Take(limit).ToList();

        return topTenInShuffledList;
    }
}