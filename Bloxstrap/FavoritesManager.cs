using System.IO;
using System.Text.Json;

namespace Bloxstrap
{
    public class FavoriteGame
    {
        public string Name { get; set; } = "";
        public string PlaceId { get; set; } = "";
    }

    /// <summary>
    /// Persisted list of pinned games shown on the launch menu.
    /// </summary>
    public class FavoritesManager : JsonManager<List<FavoriteGame>>
    {
        public override string ClassName => "Favorites";

        public static FavoritesManager Instance { get; } = new();

        private static string DeeplinkBase => "roblox://experiences/start?placeId=";

        public void Add(string name, string placeId)
        {
            if (String.IsNullOrWhiteSpace(name) || String.IsNullOrWhiteSpace(placeId))
                throw new ArgumentException("Name and place ID are required");

            if (Prop.Any(x => x.PlaceId == placeId))
                throw new InvalidOperationException("That game is already pinned");

            Prop.Add(new FavoriteGame { Name = name.Trim(), PlaceId = placeId.Trim() });
            Save();
        }

        public void Remove(FavoriteGame game)
        {
            Prop.Remove(game);
            Save();
        }

        public static void Launch(FavoriteGame game)
        {
            Utilities.ShellExecute($"{DeeplinkBase}{Uri.EscapeDataString(game.PlaceId)}");
        }
    }
}
