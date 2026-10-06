namespace Home.WebUI.Infrastructure.Features;

/// <summary>
/// Everything the app can do, written down where the app can read it. A phase that ships something
/// adds its line here as part of being finished.
/// </summary>
public static class FeatureCatalogue
{

    #region Properties

    /// <summary>
    /// One section per part of the app, in the order the navigation lists them.
    /// </summary>
    public static IReadOnlyList<FeatureArea> Areas { get; } =
    [
        new FeatureArea(
            Eyebrow: "Home",
            Headline: "The home screen",
            Href: "/",
            HueClass: "text-ink-50",
            Features:
            [
                new("Today", "Today's events, chores and meals in one list, with tomorrow underneath."),
                new("Tick things off here", "Anything due today can be ticked without leaving the screen."),
                new("Weather", "Today's temperature and the next few days."),
                new("Family notes", "Pin a note for everyone, like bin night."),
                new("Recently", "The last few things anyone changed."),
                new("Clock and greeting", "The time, and a hello for whoever is up.")
            ]),
        new FeatureArea(
            Eyebrow: "Calendar",
            Headline: "Everything the family has on",
            Href: "/calendar",
            HueClass: "text-calendar",
            Features:
            [
                new("Events", "All day, or with a start and an end.")
                {
                    Sample = FeatureSample.CalendarChip
                },
                new("Repeats", "Daily, weekly or monthly. Skip one without touching the rest."),
                new("Who it is for", "Put names on an event, and filter the calendar to one person."),
                new("Subscribed calendars", "Add a school or sports calendar by link. It shows up here and nothing can edit it."),
                new("Month, week and list", "Three ways to see the same days."),
                new("Chores and meals too", "Not only events."),
                new("Your time zone", "Times show in the zone of the device you are on.")
            ]),
        new FeatureArea(
            Eyebrow: "Recipes",
            Headline: "Your recipes in one place",
            Href: "/recipes",
            HueClass: "text-recipes",
            Features:
            [
                new("Import from a link", "Paste a recipe link and it fills in the name, photo, times, ingredients and steps."),
                new("Amounts", "Ingredients keep their amount and unit, so you can shop from them."),
                new("Steps", "Numbered, with headings where the recipe has them."),
                new("Notes", "On the recipe, or on one ingredient."),
                new("Photos", "From the site, or taken on your phone."),
                new("Search and sort", "By name, or by how long it takes."),
                new("Send to a list", "Pick the ingredients you need and add them to a shopping list.")
            ]),
        new FeatureArea(
            Eyebrow: "Meals",
            Headline: "What you are eating this week",
            Href: "/meal-plan",
            HueClass: "text-recipes",
            Features:
            [
                new("Your meals", "Breakfast, lunch, dinner, snack, or whatever you call them.")
                {
                    Sample = FeatureSample.MealCell
                },
                new("Recipes or anything else", "Plan a recipe, or type in takeaway. You can rename it later."),
                new("Move a meal", "A day either way, or drag it somewhere else."),
                new("Last cooked", "The picker tells you when you last had it."),
                new("Leftovers", "Plan a big cook and it offers the next night as leftovers."),
                new("Shop for the week", "Add everything planned to a shopping list in one go.")
            ]),
        new FeatureArea(
            Eyebrow: "Shopping",
            Headline: "Lists for the shops",
            Href: "/shopping-lists",
            HueClass: "text-shopping",
            Features:
            [
                new("Usual price", "Each line shows what it cost last time, and flags it when this one is dearer.")
                {
                    Sample = FeatureSample.ShoppingLine
                },
                new("Shopping mode", "Turn it on at the shops. The whole row ticks and the trolley total sits at the bottom.")
                {
                    Sample = FeatureSample.AisleRow
                },
                new("Type it how you say it", "\"2 kg potatoes\" comes out as two kilos of potatoes."),
                new("Suggestions", "What you buy often, offered as you type."),
                new("Aisles", "Set your aisles up once. Items land in the right one from then on."),
                new("My order or by aisle", "Set per list."),
                new("Two phones, one shop", "Split up at the door and you are still on the same shop."),
                new("Untick all, clear ticked", "Keep the list for next week, or empty out what you bought."),
                new("As many lists as you want", "One per shop or job. Archive the ones you are done with.")
            ]),
        new FeatureArea(
            Eyebrow: "Week",
            Headline: "Chores and jobs",
            Href: "/activities",
            HueClass: "text-week",
            Features:
            [
                new("Your columns", "Name them, add them, move them.")
                {
                    Sample = FeatureSample.ChoreCard
                },
                new("Week or day", "Same cards, two views."),
                new("Who is doing it", "Assign a card, or show just yours on this device."),
                new("Labels", "Colour them however you like."),
                new("Due dates", "A day, or a time on that day."),
                new("Your own fields", "You decide what a card holds."),
                new("Who ticked it", "Recorded with the time.")
            ]),
        new FeatureArea(
            Eyebrow: "Lights",
            Headline: "Your lights, by room",
            Href: "/lights",
            HueClass: "text-lights",
            Features:
            [
                new("Rooms", "Group bulbs and switch the lot."),
                new("Scenes", "Save how the lights are now and bring it back with one tap."),
                new("Schedules", "Run a scene at a time, or at sunrise or sunset."),
                new("Only if", "Skip it unless the lights are already on, or already off."),
                new("Effects", "Breathe or pulse a room."),
                new("Previous look", "Put the room back how it was.")
            ]),
        new FeatureArea(
            Eyebrow: "The household",
            Headline: "Members, settings and the rest",
            Href: "/settings",
            HueClass: "text-household",
            Features:
            [
                new("Undo", "Any tick or delete, for about ten seconds after."),
                new("Members", "With or without a login. Kids can be put on things without signing in."),
                new("History", "What changed and who did it, newest first."),
                new("Devices", "See what is signed in, and sign it out."),
                new("Dark or light", "Set per device."),
                new("Feedback", "Send a problem or an idea from inside the app."),
                new("This page", "What every part of the app does, with the newest things first."),
                new("One household", "Nothing here can see another household's things.")
            ])
    ];

    /// <summary>
    /// The last few things built, newest first.
    /// </summary>
    public static IReadOnlyList<ShippedFeature> NewLately { get; } =
    [
        new(new DateOnly(2026, 10, 6), "This page", "A list of what the app does, so nothing built gets forgotten about."),
        new(new DateOnly(2026, 10, 5), "Dropped connections", "If the connection drops the app waits it out, and plays back whatever you tapped while it was gone."),
        new(new DateOnly(2026, 10, 2), "Feedback button", "Send a problem or an idea from inside the app."),
        new(new DateOnly(2026, 9, 20), "Weeks start on Sunday", "The week, the calendar and the meal plan all start on the same day."),
        new(new DateOnly(2026, 9, 20), "Imported amounts", "Ingredients come in with the amount and unit split out of the text."),
        new(new DateOnly(2026, 9, 18), "Undo", "Any tick or delete, for about ten seconds after."),
        new(new DateOnly(2026, 9, 16), "Shopping mode", "A shop with a start and an end, big rows to tick, and the trolley total at the bottom."),
        new(new DateOnly(2026, 9, 11), "Usual prices", "Lines remember what they cost and flag a dearer one.")
    ];

    #endregion Properties

}
