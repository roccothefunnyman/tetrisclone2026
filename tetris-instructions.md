# Tetris Clone - Claude Code Build Instructions

## Project Overview
Build a fully playable Tetris clone as a single, self-contained HTML file. The game should be polished, responsive, and faithful to classic Tetris mechanics.

---

## Technical Requirements

### File Structure
- **Single HTML file** containing all HTML, CSS, and JavaScript
- No external dependencies or CDN links
- Must work offline when opened in any modern browser

### Canvas & Rendering
- Use HTML5 `<canvas>` for the game board rendering
- Game board: **10 columns × 20 rows** (standard Tetris dimensions)
- Block size: **30px × 30px**
- Smooth rendering at 60fps using `requestAnimationFrame`

---

## Game Mechanics

### The 7 Tetromino Pieces
Implement all 7 standard Tetris pieces with their classic colors:

| Piece | Shape | Color |
|-------|-------|-------|
| I | ████ (4 in a row) | Cyan (#00FFFF) |
| O | 2×2 square | Yellow (#FFFF00) |
| T | T-shape | Purple (#800080) |
| S | S-shape (zigzag) | Green (#00FF00) |
| Z | Z-shape (reverse zigzag) | Red (#FF0000) |
| J | J-shape | Blue (#0000FF) |
| L | L-shape | Orange (#FFA500) |

### Piece Rotation
- Implement **Super Rotation System (SRS)** or simple 90° clockwise rotation
- Include wall kick logic: if rotation would cause collision, try shifting piece left/right by 1-2 cells
- O-piece does not rotate (it's a square)

### Movement & Controls
| Key | Action |
|-----|--------|
| ← (Left Arrow) | Move piece left |
| → (Right Arrow) | Move piece right |
| ↓ (Down Arrow) | Soft drop (faster descent) |
| ↑ (Up Arrow) | Rotate clockwise |
| Spacebar | Hard drop (instant drop to bottom) |
| P | Pause/Resume game |
| R | Restart game (when game over) |

### Gravity & Speed
- Pieces fall automatically at a rate based on current level
- **Starting speed**: 1 cell per 800ms (Level 1)
- **Speed increases** with each level: `dropInterval = Math.max(100, 800 - (level - 1) * 75)`
- Soft drop: 10× faster than normal drop speed
- Hard drop: Instantly places piece at lowest valid position

### Line Clearing
- When a row is completely filled, it should be cleared
- All rows above cleared line(s) shift down
- Support clearing multiple lines simultaneously (1-4 lines)
- Add a brief visual flash/animation when lines clear

### Collision Detection
- Pieces cannot move through walls or other placed pieces
- Pieces cannot rotate if it would cause overlap
- When a piece cannot move down further, it "locks" after a brief delay (~500ms)

### Game Over Condition
- Game ends when a new piece spawns but immediately collides with existing blocks
- Display "GAME OVER" overlay with final score
- Allow restart with 'R' key

---

## Scoring System

### Points
| Action | Points |
|--------|--------|
| Single line clear | 100 × level |
| Double line clear | 300 × level |
| Triple line clear | 500 × level |
| Tetris (4 lines) | 800 × level |
| Soft drop | 1 point per cell |
| Hard drop | 2 points per cell |

### Leveling
- Start at **Level 1**
- Level up every **10 lines cleared**
- Display current level prominently

---

## User Interface Layout

### Screen Layout (left to right)
```
┌─────────────────────────────────────────────────────┐
│                    TETRIS                           │
├─────────────┬─────────────────────┬─────────────────┤
│   NEXT      │                     │    SCORE        │
│   ┌───┐     │                     │    12500        │
│   │ T │     │     GAME BOARD      │                 │
│   └───┘     │     (10 × 20)       │    LEVEL        │
│             │                     │    5            │
│   HOLD      │                     │                 │
│   ┌───┐     │                     │    LINES        │
│   │ I │     │                     │    47           │
│   └───┘     │                     │                 │
│             │                     │                 │
│  CONTROLS   │                     │                 │
│  ← → Move   │                     │                 │
│  ↑ Rotate   │                     │                 │
│  ↓ Soft     │                     │                 │
│  Space Hard │                     │                 │
│  P Pause    │                     │                 │
└─────────────┴─────────────────────┴─────────────────┘
```

### Required UI Elements
1. **Game Board**: 10×20 grid with visible grid lines (subtle)
2. **Next Piece Preview**: Shows the upcoming piece
3. **Hold Piece Display**: Shows held piece (optional feature, see below)
4. **Score Display**: Current score, updates in real-time
5. **Level Display**: Current level
6. **Lines Display**: Total lines cleared
7. **Controls Reference**: Brief control instructions
8. **Pause Overlay**: Semi-transparent overlay when paused
9. **Game Over Overlay**: Shows final score, restart instructions

---

## Visual Design

### Color Scheme
- **Background**: Dark (#1a1a2e or similar dark blue/black)
- **Game board background**: Slightly lighter (#16213e)
- **Grid lines**: Subtle (#ffffff15)
- **Text**: White or light gray
- **UI panels**: Semi-transparent dark panels

### Visual Polish
- Pieces should have a subtle **3D effect** (lighter top/left edges, darker bottom/right)
- **Ghost piece**: Show a semi-transparent preview of where the piece will land
- **Smooth animations** for piece movement
- **Flash effect** when lines are cleared
- Clean, modern font (system fonts are fine: `-apple-system, BlinkMacSystemFont, 'Segoe UI', sans-serif`)

---

## Optional Features (Implement if time permits)

### Hold Piece
- Press 'C' to hold current piece and swap with previously held piece
- Can only hold once per piece drop
- Display held piece in UI

### High Score
- Store high score in `localStorage`
- Display high score alongside current score

### Sound Effects (optional)
- Use Web Audio API for simple sounds
- Move, rotate, drop, line clear, game over sounds

---

## Code Quality Requirements

### Structure
```javascript
// Suggested code organization:
// 1. Constants (BOARD_WIDTH, BOARD_HEIGHT, BLOCK_SIZE, COLORS, SHAPES)
// 2. Game State variables
// 3. Tetromino class or object factory
// 4. Board management functions
// 5. Collision detection functions
// 6. Input handling
// 7. Rendering functions
// 8. Game loop
// 9. Initialization
```

### Best Practices
- Use `const` and `let` appropriately
- Clear, descriptive variable and function names
- Comment complex logic
- Efficient rendering (only redraw when needed)
- Clean separation between game logic and rendering

---

## Testing Checklist

Before considering complete, verify:

- [ ] All 7 pieces spawn and display correctly
- [ ] Pieces rotate properly with wall kicks
- [ ] Left/right movement works and stops at walls
- [ ] Soft drop accelerates piece
- [ ] Hard drop instantly places piece
- [ ] Lines clear when complete
- [ ] Multiple lines can clear simultaneously
- [ ] Score updates correctly for all actions
- [ ] Level increases every 10 lines
- [ ] Speed increases with level
- [ ] Game over triggers correctly
- [ ] Pause/resume works
- [ ] Restart works after game over
- [ ] Ghost piece shows correct landing position
- [ ] Next piece preview updates correctly
- [ ] No visual glitches or flickering
- [ ] Responsive to rapid key presses

---

## Deliverable

A single file named `tetris.html` that:
1. Opens directly in a browser
2. Is immediately playable
3. Contains all code inline (no external files)
4. Is well-formatted and readable
5. Works in Chrome, Firefox, Safari, and Edge

---

## Example Starting Template

```html
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Tetris</title>
    <style>
        /* All CSS here */
    </style>
</head>
<body>
    <!-- Minimal HTML structure -->
    <script>
        // All JavaScript here
        // Game implementation
    </script>
</body>
</html>
```

---

## Summary

Build a complete, polished Tetris clone in a single HTML file with:
- Classic 10×20 board with all 7 tetrominoes
- Full keyboard controls (arrows, space, P, R)
- Scoring system with levels
- Clean UI with next piece preview, score, level, lines
- Ghost piece and visual polish
- Proper collision detection and line clearing
- Game over and restart functionality

The result should feel like a professional, playable game that's faithful to the original Tetris experience.
