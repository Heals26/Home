using System.Text.Json;

namespace Home.Persistence.Undo;

public record UndoValue(string Property, JsonElement Value);
