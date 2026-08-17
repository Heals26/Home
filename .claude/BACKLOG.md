# Backlog

*Things Mitch has asked for that are deliberately **not** being built yet. Newest first. Each entry
records enough to pick it up cold: what it is, why it was parked, and what has to be decided first.
When one gets built, move the reasoning into `DECISIONS.md` and delete it from here.*

## In-app feedback button, and a support tool to read it

**Asked for 17 Aug 2026. Parked: not now.**

A **Feedback** button in the app that posts through the API into the database, plus a separate
support tool for reading what comes in.

Shape it will probably take: a `Feedback` entity (content, who, which screen, when, app version),
household-scoped like everything else, with a `CreateFeedback` slice and a button that can be
dropped on any page. The support side is the open question — reading across *all* households
deliberately breaks the household-isolation rule that every other query obeys
(`DECISIONS.md`, 14 Aug), so it needs either a separate operator-facing app or an explicit
support role with its own authorisation policy. That decision comes first.

Until then: the dashboard's family notes are the capture surface, read straight out of
`home.Announcement`.

## Spotify — play music from the app

**Asked for 17 Aug 2026. Parked: not now.**

Control or play music from the kitchen tablet.

Follows the external-service adapter pattern (`ISpotifyService` in `Home.Application/Services/`,
adapter in `Home.WebApi/Infrastructure/`, unreachable provider is a return value not an exception —
the Lights slice is the worked example). The hard part is not the API, it is that Spotify uses
OAuth **per user**, not a single household token like LIFX, so it needs a real authorisation-code
flow with refresh, a callback URL, and a decision about whose account the tablet plays from.
Playback control also requires Spotify Premium. Settle the account question before scoping.
