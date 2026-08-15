#nullable enable
using Home.Application.Infrastructure.Recipes;
using Home.Application.Services.RecipeImports;
using System.Net;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Xml;

namespace Home.WebApi.Infrastructure.RecipeImports;

/// <summary>
/// Reads the schema.org Recipe most cooking sites embed as JSON-LD. Nothing else is scraped —
/// if a page has no structured recipe, the import honestly fails rather than guessing at HTML.
/// All fetch and parse failures surface as null, never exceptions, per the adapter rules.
/// </summary>
internal partial class JsonLdRecipeImportService(
    HttpClient httpClient,
    ILogger<JsonLdRecipeImportService> logger)
    : IRecipeImportService
{

    #region Fields

    private const int MaxContentBytes = 5 * 1024 * 1024;
    private const int MaxImageUrlLength = 2048;
    private const int MaxIngredientLength = 200;
    private const int MaxNameLength = 250;
    private const int MaxStepTitleLength = 250;

    #endregion Fields

    #region Methods

    public async Task<ImportedRecipe?> FetchRecipeAsync(string url, CancellationToken cancellationToken)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out var _Uri)
            || (_Uri.Scheme != Uri.UriSchemeHttp && _Uri.Scheme != Uri.UriSchemeHttps))
            return null;

        string _Html;

        try
        {
            using var _Response = await httpClient.GetAsync(_Uri, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

            if (!_Response.IsSuccessStatusCode || _Response.Content.Headers.ContentLength > MaxContentBytes)
                return null;

            _Html = await _Response.Content.ReadAsStringAsync(cancellationToken);
        }
        catch (Exception _Exception) when (_Exception is HttpRequestException or TaskCanceledException)
        {
            logger.LogInformation(_Exception, "Recipe import could not fetch {Url}.", url);
            return null;
        }

        foreach (Match _Match in JsonLdBlocks().Matches(_Html))
        {
            var _Recipe = TryParseRecipe(_Match.Groups[1].Value);

            if (_Recipe != null)
                return _Recipe;
        }

        return null;
    }

    /// <summary>
    /// A recipe node can sit at the root, inside a root array, or inside a @graph — and sites
    /// get the casing of "Recipe" wrong often enough that the type check is case-insensitive.
    /// </summary>
    private static JsonElement? FindRecipeNode(JsonElement element)
    {
        if (element.ValueKind == JsonValueKind.Array)
        {
            foreach (var _Item in element.EnumerateArray())
            {
                var _Found = FindRecipeNode(_Item);

                if (_Found != null)
                    return _Found;
            }

            return null;
        }

        if (element.ValueKind != JsonValueKind.Object)
            return null;

        if (element.TryGetProperty("@type", out var _Type) && IsRecipeType(_Type))
            return element;

        return element.TryGetProperty("@graph", out var _Graph)
            ? FindRecipeNode(_Graph)
            : null;
    }

    [GeneratedRegex("\\d+")]
    private static partial Regex FirstNumber();

    private static string? FirstScalar(JsonElement array)
    {
        foreach (var _Item in array.EnumerateArray())
        {
            if (_Item.ValueKind == JsonValueKind.String)
                return _Item.GetString();

            if (_Item.ValueKind == JsonValueKind.Number && _Item.TryGetInt32(out var _Number))
                return _Number.ToString();
        }

        return null;
    }

    private static bool IsRecipeType(JsonElement type)
    {
        if (type.ValueKind == JsonValueKind.String)
            return string.Equals(type.GetString(), "Recipe", StringComparison.OrdinalIgnoreCase);

        if (type.ValueKind != JsonValueKind.Array)
            return false;

        foreach (var _Item in type.EnumerateArray())
            if (_Item.ValueKind == JsonValueKind.String
                && string.Equals(_Item.GetString(), "Recipe", StringComparison.OrdinalIgnoreCase))
                return true;

        return false;
    }

    [GeneratedRegex("<script[^>]*type\\s*=\\s*[\"']application/ld\\+json[\"'][^>]*>(.*?)</script\\s*>", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    private static partial Regex JsonLdBlocks();

    /// <summary>
    /// An image arrives as an address, a list of addresses, or an ImageObject wrapping one —
    /// and only an absolute http or https address is ever kept.
    /// </summary>
    private static string? ReadImageUrl(JsonElement node)
    {
        if (node.ValueKind == JsonValueKind.String)
        {
            var _Url = node.GetString()?.Trim();

            return RecipeImageLogic.IsAWebAddress(_Url) && _Url!.Length <= MaxImageUrlLength
                ? _Url
                : null;
        }

        if (node.ValueKind == JsonValueKind.Array)
        {
            foreach (var _Item in node.EnumerateArray())
            {
                var _Found = ReadImageUrl(_Item);

                if (_Found != null)
                    return _Found;
            }

            return null;
        }

        return node.ValueKind == JsonValueKind.Object && node.TryGetProperty("url", out var _UrlNode)
            ? ReadImageUrl(_UrlNode)
            : null;
    }

    private static List<string> ReadIngredients(JsonElement recipe)
    {
        var _Ingredients = new List<string>();

        // "recipeIngredient" is the schema.org name; "ingredients" is a legacy spelling
        // old sites still use.
        if (!recipe.TryGetProperty("recipeIngredient", out var _Node)
            && !recipe.TryGetProperty("ingredients", out _Node))
            return _Ingredients;

        if (_Node.ValueKind == JsonValueKind.String)
        {
            var _Single = Sanitise(_Node.GetString(), MaxIngredientLength);

            if (_Single.Length > 0)
                _Ingredients.Add(_Single);

            return _Ingredients;
        }

        if (_Node.ValueKind != JsonValueKind.Array)
            return _Ingredients;

        foreach (var _Item in _Node.EnumerateArray())
        {
            if (_Item.ValueKind != JsonValueKind.String)
                continue;

            var _Ingredient = Sanitise(_Item.GetString(), MaxIngredientLength);

            if (_Ingredient.Length > 0)
                _Ingredients.Add(_Ingredient);
        }

        return _Ingredients;
    }

    /// <summary>
    /// Instructions arrive as a single string, an array of strings, HowToStep objects, or
    /// HowToSection objects wrapping more steps. A section's name becomes the title of the
    /// steps inside it that have none of their own.
    /// </summary>
    private static void ReadInstructions(JsonElement node, string sectionTitle, List<ImportedRecipeStep> steps)
    {
        if (node.ValueKind == JsonValueKind.String)
        {
            var _Content = Sanitise(node.GetString(), int.MaxValue);

            if (_Content.Length > 0)
                steps.Add(new ImportedRecipeStep(sectionTitle, _Content));

            return;
        }

        if (node.ValueKind == JsonValueKind.Array)
        {
            foreach (var _Item in node.EnumerateArray())
                ReadInstructions(_Item, sectionTitle, steps);

            return;
        }

        if (node.ValueKind != JsonValueKind.Object)
            return;

        var _Name = node.TryGetProperty("name", out var _NameNode) && _NameNode.ValueKind == JsonValueKind.String
            ? Sanitise(_NameNode.GetString(), MaxStepTitleLength)
            : string.Empty;

        if (node.TryGetProperty("itemListElement", out var _Children))
        {
            ReadInstructions(_Children, _Name.Length > 0 ? _Name : sectionTitle, steps);
            return;
        }

        if (node.TryGetProperty("text", out var _TextNode) && _TextNode.ValueKind == JsonValueKind.String)
        {
            var _Text = Sanitise(_TextNode.GetString(), int.MaxValue);

            if (_Text.Length == 0)
                return;

            // Many sites copy the text into name — a title that repeats the step is noise.
            var _Title = _Name.Length > 0 && !_Text.StartsWith(_Name, StringComparison.OrdinalIgnoreCase)
                ? _Name
                : sectionTitle;

            steps.Add(new ImportedRecipeStep(_Title, _Text));
        }
    }

    /// <summary>
    /// Durations are meant to be ISO-8601 ("PT30M") but real pages emit "PT", "P1Y" and
    /// "30 mins", all of which throw — an unreadable time is simply unknown.
    /// </summary>
    private static int? ReadMinutes(JsonElement recipe, string propertyName)
    {
        if (!recipe.TryGetProperty(propertyName, out var _Node))
            return null;

        var _Value = _Node.ValueKind switch
        {
            JsonValueKind.Array => FirstScalar(_Node),
            JsonValueKind.String => _Node.GetString(),
            _ => null
        };

        if (string.IsNullOrWhiteSpace(_Value))
            return null;

        try
        {
            var _Minutes = XmlConvert.ToTimeSpan(_Value.Trim()).TotalMinutes;

            return _Minutes > 0 && _Minutes <= RecipeValues.MaximumMinutes
                ? (int)Math.Round(_Minutes)
                : null;
        }
        catch (Exception _Exception) when (_Exception is FormatException or OverflowException)
        {
            return null;
        }
    }

    /// <summary>
    /// A yield is a number, "4", or "Serves 4 to 6" — the first number in it is the serving
    /// count worth keeping.
    /// </summary>
    private static int? ReadServings(JsonElement recipe)
    {
        if (!recipe.TryGetProperty("recipeYield", out var _Node))
            return null;

        var _Value = _Node.ValueKind switch
        {
            JsonValueKind.Array => FirstScalar(_Node),
            JsonValueKind.Number => _Node.TryGetInt32(out var _Yield) ? _Yield.ToString() : null,
            JsonValueKind.String => _Node.GetString(),
            _ => null
        };

        if (string.IsNullOrWhiteSpace(_Value))
            return null;

        var _Match = FirstNumber().Match(_Value);

        return _Match.Success
            && int.TryParse(_Match.Value, out var _Servings)
            && _Servings > 0
            && _Servings <= RecipeValues.MaximumServings
                ? _Servings
                : null;
    }

    /// <summary>
    /// Strips tags, decodes entities and collapses whitespace — recipe sites shove HTML into
    /// their JSON-LD more often than they should.
    /// </summary>
    private static string Sanitise(string? value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        var _Clean = Whitespace().Replace(WebUtility.HtmlDecode(Tags().Replace(value, " ")), " ").Trim();

        return _Clean.Length > maxLength ? _Clean[..maxLength].Trim() : _Clean;
    }

    [GeneratedRegex("<[^>]+>")]
    private static partial Regex Tags();

    private static ImportedRecipe? TryParseRecipe(string json)
    {
        try
        {
            using var _Document = JsonDocument.Parse(json);

            var _Node = FindRecipeNode(_Document.RootElement);

            if (_Node == null)
                return null;

            var _Recipe = _Node.Value;

            var _Name = _Recipe.TryGetProperty("name", out var _NameNode) && _NameNode.ValueKind == JsonValueKind.String
                ? Sanitise(_NameNode.GetString(), MaxNameLength)
                : string.Empty;

            if (_Name.Length == 0)
                return null;

            var _Ingredients = ReadIngredients(_Recipe);

            var _Steps = new List<ImportedRecipeStep>();

            if (_Recipe.TryGetProperty("recipeInstructions", out var _Instructions))
                ReadInstructions(_Instructions, string.Empty, _Steps);

            // A name alone is not a recipe — without a single ingredient or step the page
            // gave nothing worth importing.
            if (_Ingredients.Count == 0 && _Steps.Count == 0)
                return null;

            return new ImportedRecipe(
                ReadMinutes(_Recipe, "cookTime"),
                _Recipe.TryGetProperty("image", out var _ImageNode) ? ReadImageUrl(_ImageNode) : null,
                _Ingredients,
                _Name,
                ReadMinutes(_Recipe, "prepTime"),
                ReadServings(_Recipe),
                _Steps);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    [GeneratedRegex("\\s+")]
    private static partial Regex Whitespace();

    #endregion Methods

}
