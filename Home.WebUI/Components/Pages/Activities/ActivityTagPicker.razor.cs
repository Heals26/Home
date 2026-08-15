using Home.WebUI.DataAccess.Tags.Models;
using Microsoft.AspNetCore.Components;

namespace Home.WebUI.Components.Pages.Activities;

public partial class ActivityTagPicker
{

    #region Properties

    [Parameter] public EventCallback<long> OnToggle { get; set; }
    [Parameter] public HashSet<long> SelectedTagIDs { get; set; } = [];
    [Parameter] public List<TagDto> Tags { get; set; } = [];

    #endregion Properties

    #region Methods

    private static string GetChipClasses(bool isSelected)
    {
        var _Base = "inline-flex items-center gap-1.5 rounded-full px-3 py-2 min-h-[40px] text-xs font-semibold text-ink-950 transition active:scale-95";
        var _State = isSelected ? "ring-2 ring-offset-2 ring-offset-ink-900 ring-ink-50" : "opacity-40 hover:opacity-75";

        return $"{_Base} {_State}";
    }

    #endregion Methods

}
