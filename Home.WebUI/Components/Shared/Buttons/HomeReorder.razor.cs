using Microsoft.AspNetCore.Components;

namespace Home.WebUI.Components.Shared.Buttons;

public partial class HomeReorder
{

    #region Properties

    [Parameter] public bool CanMoveDown { get; set; } = true;
    [Parameter] public bool CanMoveUp { get; set; } = true;

    /// <summary>
    /// What is being moved, so a screen reader says "Move Onion up" rather than "Move up" twenty
    /// times down a list.
    /// </summary>
    [Parameter] public string Label { get; set; } = string.Empty;

    [Parameter] public EventCallback OnMoveDown { get; set; }
    [Parameter] public EventCallback OnMoveUp { get; set; }

    #endregion Properties

}
