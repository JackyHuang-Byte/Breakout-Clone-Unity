# 🧱 Breakout Clone (Unity, C#)

A 2D game where the player controls a paddle to clear bricks under a time limit, with added power-ups and custom levels.
This project was created to practice level design, collision mechanics, and data loading in Unity.  

---

## 🛠️ What It Does

- Ball physics and paddle control using Unity’s built-in collision system
- Players can create and save their own level presets using `JsonUtility`
- Bricks can trigger power-ups when hit, adding variety to gameplay
- Time-based win/loss conditions to encourage fast and precise play
- Basic in-game UI includes score display and time countdown

---

## 🧩 How It’s Structured

- Each level is defined by a JSON preset and loaded at runtime
- Brick objects support custom health values and conditional logic
- Power-ups are activated by destroy specific bricks

---

## ▶️ How to Run

1. Open in Unity 2022.3 or newer  
2. Load the `MainScene` from the `Scenes` folder  
3. Enter Play Mode  
4. Create your own level layout and set hit counts for bricks, or use the default preset with Quick Start  
5. Use arrow keys (← / →) or A & D keys to control the paddle

---

## 💬 What I Learned

- Practiced level data management and basic serialization with JSON
- Applied game logic with timing and dynamic interactions
- Improved understanding of Unity’s physics-based collision responses
