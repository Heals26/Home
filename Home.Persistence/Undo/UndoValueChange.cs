using System.Text.Json;

namespace Home.Persistence.Undo;

public record UndoValueChange(string Property, JsonElement Before, JsonElement After);
