# What Home is

*Written 13 August 2026, from Mitch's own words. This is the north star — read it before deciding
what to build or how anything should look. Update it only when the product direction genuinely
changes, and date the change in `DECISIONS.md`.*

Home is **one application a family relies on** to visually organise their household. It is designed
to sit on a **tablet next to the kitchen** — always on, glanceable from across the room, driven by
fingers (often wet, often mid-task), used by every member of the family, not just the person who
set it up.

## The pillars

1. **Recipes** — look up, import, and view recipes. Cooking is the app's home turf; the tablet is
   *in the kitchen*.
2. **Shopping lists** — the family's shared lists, added to during the week, used at the shop.
3. **Weekly tasks** — see the family's tasks for the week at a glance and manage them.
4. **Home control** — if the household has smart-home gear set up (lights today, via LIFX; other
   devices as they come), control it from the same screen. This is a bonus pillar, not the core:
   the app must be fully useful with zero smart-home hardware.

## What that implies

- **Tablet-first, touch-first.** Landscape tablet is the primary viewport. Large touch targets,
  no hover-dependent affordances, no dense desktop chrome.
- **Glanceable.** The most common interaction is a look, not a tap. The dashboard answers
  "what's happening this week / what's for dinner / what's on the list" without navigation.
- **Family-proof.** A user who never read a manual — a child, a partner, a guest — must not get
  stuck, lost, or frustrated. Errors recover gracefully; destructive actions are hard to hit by
  accident.
- **Not generic.** The UI should feel deliberately designed for this product, not assembled from
  template defaults.

## Explicitly open

- **Hosting** — cloud-hosted or locally hosted are both acceptable; no decision has been forced
  yet. Don't build anything that closes either door without a `DECISIONS.md` entry.
- **Other smart-home integrations** — nothing beyond lights is planned or researched yet.
