# Backlog

*Things Mitch has asked for that are deliberately **not** being built yet. Newest first. Each entry
records enough to pick it up cold: what it is, why it was parked, and what has to be decided first.
When one gets built, move the reasoning into `DECISIONS.md` and delete it from here.*

## Drop the client application row for the bundled web app

**Raised 4 Sep 2026. Parked: setup stays manual until this is decided.**

A fresh database cannot be signed into. The API rejects any client it does not recognise, nothing
seeds `home.ClientApplication`, and the five `OAuth:AccessToken:*` secrets on `Home.WebUI` have to
be matched to that row by hand. It cost an evening on a second machine on 3 Sep, and the failure is
a bare 401 with nothing useful on screen.

Three ways out, in increasing order of how much they change:

1. A one-shot command on the API that inserts the row and prints the exact `user-secrets set` lines
   to paste. Smallest, reversible, leaves the auth model alone.
2. Fold it into the existing `/setup` flow. Complicated by a chicken-and-egg problem: the web app
   needs the credentials in configuration before anyone can reach `/setup` at all.
3. **Ask whether the bundled web app needs a client application at all.** The concept exists so a
   third-party client can be identified and revoked. `Home.WebUI` is not third party, it ships in
   this repository and is deployed alongside the API, and treating it as one buys a shared secret
   that nobody outside the repository ever sees. Removing the requirement for the first-party
   client would delete a whole setup step and five secrets.

Option 3 is the one worth having and the one that needs the decision, because it touches
`BasicAuthenticationHandler`, `WebAppPlatformHandler` and the token endpoint, and it has to leave
the door open for a real third-party client later. Until it is decided, **every installation does
step 4 of `README.md` by hand, and the documentation says so.**

Worth knowing when that decision is taken: the token endpoint is anonymous, so the `Authorization:
Basic` header the web app sends it is **never read**. `BasicAuthenticationHandler` guards the rest
of the API, not this. The client is identified entirely by the `client_id` and `client_secret` form
fields, which the two grant interactors check for themselves. Whatever replaces this has one caller
to satisfy, not two.

Related: C5 in `ROADMAP.md` (nothing validates the OAuth secrets at startup, so a missing one is
also a silent 401).

## In-app feedback button, and a support tool to read it

**Asked for 17 Aug 2026. Parked: not now.**

A **Feedback** button in the app that posts through the API into the database, plus a separate
support tool for reading what comes in.

Shape it will probably take: a `Feedback` entity (content, who, which screen, when, app version),
household-scoped like everything else, with a `CreateFeedback` slice and a button that can be
dropped on any page. The support side is the open question, because reading across *all* households
deliberately breaks the household-isolation rule that every other query obeys
(`DECISIONS.md`, 14 Aug), so it needs either a separate operator-facing app or an explicit
support role with its own authorisation policy. That decision comes first.

Until then: the dashboard's family notes are the capture surface, read straight out of
`home.Announcement`.

## Spotify: play music from the app

**Asked for 17 Aug 2026. Parked: not now.**

Control or play music from the kitchen tablet.

Follows the external-service adapter pattern (`ISpotifyService` in `Home.Application/Services/`,
adapter in `Home.WebApi/Infrastructure/`, unreachable provider is a return value not an exception,
with the Lights slice as the worked example). The hard part is not the API, it is that Spotify uses
OAuth **per user**, not a single household token like LIFX, so it needs a real authorisation-code
flow with refresh, a callback URL, and a decision about whose account the tablet plays from.
Playback control also requires Spotify Premium. Settle the account question before scoping.

## Retire the superseded-session branch, then drop the two columns

**Found 5 Sep 2026 while doing phase 3.** Small, but it changes the refresh path, so it is not the
cleanup phase 3 assumed.

`UserAuthentication.SupersededOnUTC` and `SupersededByAuthenticationMetadataID` are read on every
refresh by `CreateRefreshGrantInteractor.ResolveSupersededSession`, which follows the pointer from
an old token to the session that replaced it. It is backwards compatibility for tokens issued
before the 19 Aug no-rotation decision, and the roadmap wrongly recorded it as dead code.

Measured 5 Sep 2026: 1 row in the table, 0 carrying a superseded pointer, so nothing exercises the
branch. Order matters, because doing it the other way signs out anyone still holding such a token:

1. Re-check that no row has `SupersededOnUTC` set.
2. Delete `ResolveSupersededSession` and its call site in `CreateRefreshGrantInteractor`.
3. Drop the two columns from the entity, the EF configuration, and the table.

## Let a shopping list item point at an ingredient

**Found 5 Sep 2026 while doing phase 3.** Blocks the thing ingredient notes were actually for.

`ShoppingListItem` carries a free-text `Name` and nothing else. It has no link to `Ingredient`, so
an ingredient note ("Woolies brand only", "the one in the green tin") cannot follow the ingredient
onto the list, which is the one place the person doing the shop would read it.

Two problems sit behind this and the second is the real one:

- The list item needs an optional `IngredientID`, set when the item came from a recipe and null
  when someone typed it straight in.
- Nothing in the application ever reuses an `Ingredient`. `AddRecipeIngredient` and `ImportRecipe`
  both create a fresh row per recipe, so "olive oil" is a different ingredient in every recipe that
  uses it and a note on one of them means nothing to the others. Pointing a list item at one of
  those rows is pointing at a duplicate.

Deduplicating ingredients is the prerequisite, and it is not free: `Amount` and `Unit` live on the
`Ingredient` row, not on the `RecipeIngredient` join, so sharing a row today would mean sharing
"1 tbsp" between two recipes that need different amounts. Moving `Amount` and `Unit` onto the join
comes first, then deduplication, then this.
