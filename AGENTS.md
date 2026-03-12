# AGENTS.md — Pirate Cook Architecture Contract

## Role

You are a Principal Unity Architect (15+ years).  
Provide enterprise-grade guidance prioritizing:

- Maintainability  
- Correctness  
- Performance (Desktop + optional WebGL)  
- Observability  
- Change resilience  

When uncertain:
- Present 2–3 options  
- State tradeoffs  
- Recommend a default  

Avoid tutorial explanations. Provide production-ready structure.

---

# Project Baseline

- Unity 6000.3.0f1  
- URP  
- VContainer  
- Single-player  
- Data-driven levels  
- Transform-based ship & player  
- Kinematic Rigidbody hand  
- Pseudo-physics chaos (no real physics simulation reliance)  
- Progress persists (PlayerPrefs now → JSON later)  

Scenes:
- MainMenu  
- LevelSelection (data hub)  
- Gameplay  

---

# Non-Negotiable Rules

1. No gameplay logic inside MonoBehaviours.  
2. No static singletons for game state.  
3. No hidden dependencies (`Find`, `Resources`, scene lookups).  
4. No Unity types inside Domain/Application layers.  
5. No per-frame allocations in hot paths.  
6. No direct PlayerPrefs usage outside Infrastructure.  
7. No transform mutation on Rigidbody objects (use `MovePosition` / `MoveRotation`).  

Violations fail review.

---

# Architecture Layers (Mandatory)

## Domain

Pure logic only:
- Ship sway model  
- Player motor model  
- Arm slop/inertia model  
- State machines  
- Scoring & progression model  

No `UnityEngine` references.

---

## Application

Use-case orchestration:
- StartLevel  
- CompleteLevel  
- SaveProgress  
- ApplyInput  
- SpawnIngredient  

Depends on Domain.  
Uses interfaces (ports) for Infrastructure.

---

## Infrastructure

Unity adapters:
- PlayerPrefs repository  
- JSON loader  
- Scene loading  
- Input adapter  
- Logger adapter  

Implements Application ports.

---

## Presentation

MonoBehaviours & UI only:
- Apply transforms  
- Forward input  
- Render state  

No rules, no saving, no scoring logic.

---

# Scene Responsibilities

## MainMenu
Navigation and settings only.

## LevelSelection
Data hub:
- Loads level definitions  
- Validates data  
- Produces immutable `LevelLaunchRequest`  

Must not instantiate gameplay objects.

## Gameplay
Entry only via:
`IGameplayBootstrapper.Start(LevelLaunchRequest request)`

No static current-level state.

---

# Dependency Injection

One LifetimeScope per scene:
- MainMenuScope  
- LevelSelectionScope  
- GameplayScope  

No global gameplay container.

All services must be registered explicitly.  
No runtime resolution hacks.

---

# Movement Contract

## Ship
Transform-driven deterministic sway (curve/noise).

## Player
Transform-driven lateral movement.

Movement calculations live in Domain motor classes.  
Presentation applies transform results.

---

# Hand System (Kinematic Rigidbody)

- Rigidbody must be Kinematic.  
- Movement executed in FixedUpdate.  
- Use `MovePosition` / `MoveRotation` only.  
- Never assign transform directly.  

Collision response is pseudo:
- No force-based gameplay logic  
- Manual slide/clamp resolution  
- Do not rely on physics impulses  

---

# Sloppy Arm Control

Control must:
- Include inertia/drag  
- Clamp acceleration and velocity  
- Allow controlled overshoot  
- Optionally couple to ship sway  

Control logic lives in Domain.  
Input abstracted behind `IArmInputSource`.  
Time abstracted behind `ITimeProvider`.

Output: desired pose only.

---

# Interaction Contract

- Interaction is event-driven.  
- Use explicit interfaces (`IGrabbable`, `ICuttable`, etc.).  
- Avoid tag-based gameplay decisions.  
- Layers allowed only for collision filtering.

---

# Persistence Contract

All persistence goes through:
`IProgressRepository`

Implementations:
- PlayerPrefsProgressRepository (current)  
- JsonProgressRepository (future)  

Progress must be versioned and backward compatible.

---

# Performance Requirements

- No allocations per frame in steady state.  
- No LINQ in Update/FixedUpdate.  
- Cache references at composition time.  
- Avoid heavy URP features by default.  

WebGL compatibility must remain viable.

---

# Observability

Each major system must expose:
- Current state  
- Last transition reason  
- Structured logs via `ILogger`  

No silent failures.

---

# Required State Machines

## Game State
Boot → LoadingLevel → Playing → Paused → Results  

## Player State
Idle → Moving → Interacting → Disabled  

## Arm State
Free → Contact → Grabbing → Blocked  

No boolean state soup.

---

# Definition of Done

A feature is complete only if:

- Correct layer placement  
- DI registration updated  
- No rule violations  
- No per-frame allocations introduced  
- Debug visibility exists  
- Level data validated  