# Fortress Defend — Project Notes

## Game Concept

3D top-down fortress/base defence inspired by The Riftbreaker. Coded with GODOT.

I am coming from Gamemaker, so I am slowly wrapping my head around the differences like Gamemakers create/step events vs Godot.

Keep the scope initially small and expand later.

## Development Philosophy

- Code must be very readable.
- Prefer data-driven systems.
- Avoid duplicated code.
- Build reusable foundations rather than temporary solutions.
- Visuals should be replaceable without rewriting gameplay systems.
- Basic placeholder 3D models are fine during development.
- Keep systems modular.
- Explain unfamiliar GDScript because this is my first Godot project.

## Current Controls

- Arrow Keys — Move player

## Current Progress

- Godot 3D project created.
- Basic room/ground created.
- Player uses CharacterBody3D.
- Player can move with arrow keys.
- Gravity and collision working.
- Angled top-down camera follows player.

## Architecture Decisions

### Player
Uses CharacterBody3D.

### Turrets
Planned:
- Shared turret foundation.
- Data-driven turret definitions.
- Different weapon/behaviour modules where appropriate.
- Avoid giant `if turret_type == ...` chains.

## Next Steps

- Continue basic player/camera setup.
- Keep learning Godot fundamentals before building larger systems.