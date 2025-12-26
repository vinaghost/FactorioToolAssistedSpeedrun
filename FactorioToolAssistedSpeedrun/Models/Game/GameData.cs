using FactorioToolAssistedSpeedrun.Conventers;
using FactorioToolAssistedSpeedrun.Models.Prototypes;
using System.Collections.Frozen;
using System.Text.Json.Serialization;

namespace FactorioToolAssistedSpeedrun.Models.Game
{
    public class GameData
    {
        public GameData()
        {
        }

        public GameData(GameData data)
        {
            Technologies = data.Technologies;
            Items = data.Items;
            Recipes = data.Recipes;
            TechnologiesLocale = data.TechnologiesLocale;
            ItemsLocale = data.ItemsLocale;
            RecipesLocale = data.RecipesLocale;
            ReverseTechnologiesLocale = data.ReverseTechnologiesLocale;
            ReverseItemsLocale = data.ReverseItemsLocale;
            ReverseRecipesLocale = data.ReverseRecipesLocale;
        }

        public static GameData Create(PrototypeData prototypeData, LocalePrototype technologyLocale, LocalePrototype itemLocale, LocalePrototype recipeLocale)
        {
            var techLocaleDict = technologyLocale.Names.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);
            var itemLocaleDict = itemLocale.Names.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);
            var recipeLocaleDict = recipeLocale.Names.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);

            var technoLogiesDict = prototypeData.Technologies
                .Select(static t => new TechnologyGame(t.Value))
                .ToFrozenDictionary(x => x.Name!, x => x, StringComparer.OrdinalIgnoreCase);

            var itemsDict = prototypeData.Items
                .Where(i => !i.Value.Hidden && !i.Value.Parameter)
                .Select(i => new ItemGame(i.Value))
                .Concat(prototypeData.AmmoItems
                    .Where(i => !i.Value.Hidden && !i.Value.Parameter)
                    .Select(i => new ItemGame(i.Value)))
                .Concat(prototypeData.Capsules
                    .Where(i => !i.Value.Hidden && !i.Value.Parameter)
                    .Select(i => new ItemGame(i.Value)))
                .Concat(prototypeData.Guns
                    .Where(i => !i.Value.Hidden && !i.Value.Parameter)
                    .Select(i => new ItemGame(i.Value)))
                .Concat(prototypeData.ItemsWithEntityData
                    .Where(i => !i.Value.Hidden && !i.Value.Parameter)
                    .Select(i => new ItemGame(i.Value)))
                .Concat(prototypeData.Modules
                    .Where(i => !i.Value.Hidden && !i.Value.Parameter)
                    .Select(i => new ItemGame(i.Value)))
                .Concat(prototypeData.Tools
                    .Where(i => !i.Value.Hidden && !i.Value.Parameter)
                    .Select(i => new ItemGame(i.Value)))
                .Concat(prototypeData.Armors
                    .Where(i => !i.Value.Hidden && !i.Value.Parameter)
                    .Select(i => new ItemGame(i.Value)))
                .Concat(prototypeData.RepairTools
                    .Where(i => !i.Value.Hidden && !i.Value.Parameter)
                    .Select(i => new ItemGame(i.Value)))
                .ToFrozenDictionary(x => x.Name!, x => x, StringComparer.OrdinalIgnoreCase);

            var recipesDict = prototypeData.Recipes
                .Where(r => !r.Value.Hidden)
                .Select(r => new RecipeGame(r.Value))
                .ToFrozenDictionary(x => x.Name!, x => x, StringComparer.OrdinalIgnoreCase);

            var gameData = new GameData()
            {
                Technologies = technoLogiesDict,
                Items = itemsDict,
                Recipes = recipesDict,

                TechnologiesLocale = techLocaleDict,
                ItemsLocale = itemLocaleDict,
                RecipesLocale = recipeLocaleDict,
                ReverseTechnologiesLocale = techLocaleDict
                    .GroupBy(x => x.Value)
                    .ToFrozenDictionary(x => x.Key, x => x.Select(x => x.Key).First(), StringComparer.OrdinalIgnoreCase),
                ReverseItemsLocale = itemLocaleDict
                    .GroupBy(x => x.Value)
                    .ToFrozenDictionary(x => x.Key, x => x.Select(x => x.Key).First(), StringComparer.OrdinalIgnoreCase),
                ReverseRecipesLocale = recipeLocaleDict
                    .GroupBy(x => x.Value)
                    .ToFrozenDictionary(x => x.Key, x => x.Select(x => x.Key).First(), StringComparer.OrdinalIgnoreCase),
            };
            return gameData;
        }

        [JsonConverter(typeof(FrozenDictionaryConverter<TechnologyGame>))]
        public required FrozenDictionary<string, TechnologyGame> Technologies { get; set; }

        [JsonConverter(typeof(FrozenDictionaryConverter<ItemGame>))]
        public required FrozenDictionary<string, ItemGame> Items { get; set; }

        [JsonConverter(typeof(FrozenDictionaryConverter<RecipeGame>))]
        public required FrozenDictionary<string, RecipeGame> Recipes { get; set; }

        [JsonConverter(typeof(FrozenDictionaryConverter<string>))]
        public required FrozenDictionary<string, string> TechnologiesLocale { get; set; }

        [JsonConverter(typeof(FrozenDictionaryConverter<string>))]
        public required FrozenDictionary<string, string> ItemsLocale { get; set; }

        [JsonConverter(typeof(FrozenDictionaryConverter<string>))]
        public required FrozenDictionary<string, string> RecipesLocale { get; set; }

        [JsonConverter(typeof(FrozenDictionaryConverter<string>))]
        public required FrozenDictionary<string, string> ReverseTechnologiesLocale { get; set; }

        [JsonConverter(typeof(FrozenDictionaryConverter<string>))]
        public required FrozenDictionary<string, string> ReverseItemsLocale { get; set; }

        [JsonConverter(typeof(FrozenDictionaryConverter<string>))]
        public required FrozenDictionary<string, string> ReverseRecipesLocale { get; set; }
    }
}