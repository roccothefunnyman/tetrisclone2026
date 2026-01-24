// Tetris Clone 2026
// Game constants
const COLS = 10;
const ROWS = 20;
const BLOCK_SIZE = 30;
const COLORS = [
    null,
    '#00f0f0', // I - Cyan
    '#0000f0', // J - Blue
    '#f0a000', // L - Orange
    '#f0f000', // O - Yellow
    '#00f000', // S - Green
    '#a000f0', // T - Purple
    '#f00000'  // Z - Red
];

// Tetromino shapes
const SHAPES = [
    null,
    [[0,0,0,0], [1,1,1,1], [0,0,0,0], [0,0,0,0]], // I
    [[2,0,0], [2,2,2], [0,0,0]],                   // J
    [[0,0,3], [3,3,3], [0,0,0]],                   // L
    [[4,4], [4,4]],                                 // O
    [[0,5,5], [5,5,0], [0,0,0]],                   // S
    [[0,6,0], [6,6,6], [0,0,0]],                   // T
    [[7,7,0], [0,7,7], [0,0,0]]                    // Z
];

// Game state
let canvas, ctx, nextCanvas, nextCtx;
let board, currentPiece, nextPiece;
let score, level, lines;
let gameLoop, dropCounter, dropInterval;
let lastTime, isPaused, isGameOver;

// Initialize the game
function init() {
    canvas = document.getElementById('game-canvas');
    ctx = canvas.getContext('2d');
    nextCanvas = document.getElementById('next-canvas');
    nextCtx = nextCanvas.getContext('2d');

    document.addEventListener('keydown', handleKeyPress);
    document.getElementById('restart-btn').addEventListener('click', startGame);

    startGame();
}

function startGame() {
    board = createBoard();
    score = 0;
    level = 1;
    lines = 0;
    dropCounter = 0;
    dropInterval = 1000;
    isPaused = false;
    isGameOver = false;
    lastTime = 0;

    currentPiece = createPiece();
    nextPiece = createPiece();

    updateDisplay();
    hideOverlays();

    if (gameLoop) cancelAnimationFrame(gameLoop);
    gameLoop = requestAnimationFrame(update);
}

function createBoard() {
    return Array.from({ length: ROWS }, () => Array(COLS).fill(0));
}

function createPiece() {
    const type = Math.floor(Math.random() * 7) + 1;
    return {
        shape: SHAPES[type].map(row => [...row]),
        color: type,
        x: Math.floor(COLS / 2) - Math.floor(SHAPES[type][0].length / 2),
        y: 0
    };
}

function update(time = 0) {
    if (isGameOver) return;

    const deltaTime = time - lastTime;
    lastTime = time;

    if (!isPaused) {
        dropCounter += deltaTime;
        if (dropCounter > dropInterval) {
            moveDown();
            dropCounter = 0;
        }
    }

    draw();
    gameLoop = requestAnimationFrame(update);
}

function draw() {
    // Clear canvas
    ctx.fillStyle = 'rgba(0, 0, 0, 0.5)';
    ctx.fillRect(0, 0, canvas.width, canvas.height);

    // Draw board
    drawBoard();

    // Draw current piece
    if (currentPiece) {
        drawPiece(ctx, currentPiece, currentPiece.x, currentPiece.y);
        drawGhostPiece();
    }

    // Draw next piece preview
    drawNextPiece();
}

function drawBoard() {
    for (let y = 0; y < ROWS; y++) {
        for (let x = 0; x < COLS; x++) {
            if (board[y][x]) {
                drawBlock(ctx, x, y, COLORS[board[y][x]]);
            }
        }
    }

    // Draw grid
    ctx.strokeStyle = 'rgba(255, 255, 255, 0.1)';
    ctx.lineWidth = 1;
    for (let x = 0; x <= COLS; x++) {
        ctx.beginPath();
        ctx.moveTo(x * BLOCK_SIZE, 0);
        ctx.lineTo(x * BLOCK_SIZE, canvas.height);
        ctx.stroke();
    }
    for (let y = 0; y <= ROWS; y++) {
        ctx.beginPath();
        ctx.moveTo(0, y * BLOCK_SIZE);
        ctx.lineTo(canvas.width, y * BLOCK_SIZE);
        ctx.stroke();
    }
}

function drawBlock(context, x, y, color, alpha = 1) {
    const padding = 2;
    context.fillStyle = color;
    context.globalAlpha = alpha;
    context.fillRect(
        x * BLOCK_SIZE + padding,
        y * BLOCK_SIZE + padding,
        BLOCK_SIZE - padding * 2,
        BLOCK_SIZE - padding * 2
    );

    // Add highlight
    context.fillStyle = 'rgba(255, 255, 255, 0.3)';
    context.fillRect(
        x * BLOCK_SIZE + padding,
        y * BLOCK_SIZE + padding,
        BLOCK_SIZE - padding * 2,
        4
    );

    context.globalAlpha = 1;
}

function drawPiece(context, piece, offsetX, offsetY, alpha = 1) {
    piece.shape.forEach((row, y) => {
        row.forEach((value, x) => {
            if (value) {
                drawBlock(context, offsetX + x, offsetY + y, COLORS[value], alpha);
            }
        });
    });
}

function drawGhostPiece() {
    const ghost = { ...currentPiece, y: currentPiece.y };
    while (!collides(ghost.shape, ghost.x, ghost.y + 1)) {
        ghost.y++;
    }
    if (ghost.y !== currentPiece.y) {
        drawPiece(ctx, currentPiece, ghost.x, ghost.y, 0.3);
    }
}

function drawNextPiece() {
    nextCtx.fillStyle = 'rgba(0, 0, 0, 0.3)';
    nextCtx.fillRect(0, 0, nextCanvas.width, nextCanvas.height);

    if (nextPiece) {
        const offsetX = (nextCanvas.width / BLOCK_SIZE - nextPiece.shape[0].length) / 2;
        const offsetY = (nextCanvas.height / BLOCK_SIZE - nextPiece.shape.length) / 2;

        nextPiece.shape.forEach((row, y) => {
            row.forEach((value, x) => {
                if (value) {
                    const blockSize = 25;
                    const padding = 2;
                    nextCtx.fillStyle = COLORS[value];
                    nextCtx.fillRect(
                        (offsetX + x) * blockSize + padding + 10,
                        (offsetY + y) * blockSize + padding + 10,
                        blockSize - padding * 2,
                        blockSize - padding * 2
                    );
                }
            });
        });
    }
}

function collides(shape, offsetX, offsetY) {
    for (let y = 0; y < shape.length; y++) {
        for (let x = 0; x < shape[y].length; x++) {
            if (shape[y][x]) {
                const newX = offsetX + x;
                const newY = offsetY + y;

                if (newX < 0 || newX >= COLS || newY >= ROWS) {
                    return true;
                }
                if (newY >= 0 && board[newY][newX]) {
                    return true;
                }
            }
        }
    }
    return false;
}

function merge() {
    currentPiece.shape.forEach((row, y) => {
        row.forEach((value, x) => {
            if (value) {
                const boardY = currentPiece.y + y;
                const boardX = currentPiece.x + x;
                if (boardY >= 0) {
                    board[boardY][boardX] = value;
                }
            }
        });
    });
}

function clearLines() {
    let linesCleared = 0;

    for (let y = ROWS - 1; y >= 0; y--) {
        if (board[y].every(cell => cell !== 0)) {
            board.splice(y, 1);
            board.unshift(Array(COLS).fill(0));
            linesCleared++;
            y++; // Check same row again
        }
    }

    if (linesCleared > 0) {
        // Scoring: 100, 300, 500, 800 for 1, 2, 3, 4 lines
        const points = [0, 100, 300, 500, 800];
        score += points[linesCleared] * level;
        lines += linesCleared;

        // Level up every 10 lines
        const newLevel = Math.floor(lines / 10) + 1;
        if (newLevel > level) {
            level = newLevel;
            dropInterval = Math.max(100, 1000 - (level - 1) * 100);
        }

        updateDisplay();
    }
}

function moveDown() {
    if (!collides(currentPiece.shape, currentPiece.x, currentPiece.y + 1)) {
        currentPiece.y++;
    } else {
        merge();
        clearLines();
        currentPiece = nextPiece;
        nextPiece = createPiece();

        if (collides(currentPiece.shape, currentPiece.x, currentPiece.y)) {
            gameOver();
        }
    }
}

function moveLeft() {
    if (!collides(currentPiece.shape, currentPiece.x - 1, currentPiece.y)) {
        currentPiece.x--;
    }
}

function moveRight() {
    if (!collides(currentPiece.shape, currentPiece.x + 1, currentPiece.y)) {
        currentPiece.x++;
    }
}

function rotate() {
    const rotated = currentPiece.shape[0].map((_, i) =>
        currentPiece.shape.map(row => row[i]).reverse()
    );

    // Wall kick - try to fit the rotated piece
    const kicks = [0, -1, 1, -2, 2];
    for (const kick of kicks) {
        if (!collides(rotated, currentPiece.x + kick, currentPiece.y)) {
            currentPiece.shape = rotated;
            currentPiece.x += kick;
            return;
        }
    }
}

function hardDrop() {
    while (!collides(currentPiece.shape, currentPiece.x, currentPiece.y + 1)) {
        currentPiece.y++;
        score += 2;
    }
    moveDown();
    updateDisplay();
}

function handleKeyPress(e) {
    if (isGameOver) return;

    switch (e.key) {
        case 'ArrowLeft':
            if (!isPaused) moveLeft();
            break;
        case 'ArrowRight':
            if (!isPaused) moveRight();
            break;
        case 'ArrowDown':
            if (!isPaused) {
                moveDown();
                score += 1;
                updateDisplay();
            }
            break;
        case 'ArrowUp':
            if (!isPaused) rotate();
            break;
        case ' ':
            if (!isPaused) hardDrop();
            e.preventDefault();
            break;
        case 'p':
        case 'P':
            togglePause();
            break;
    }
}

function togglePause() {
    isPaused = !isPaused;
    document.getElementById('pause-screen').classList.toggle('hidden', !isPaused);
}

function gameOver() {
    isGameOver = true;
    document.getElementById('final-score').textContent = score;
    document.getElementById('game-over').classList.remove('hidden');
}

function hideOverlays() {
    document.getElementById('game-over').classList.add('hidden');
    document.getElementById('pause-screen').classList.add('hidden');
}

function updateDisplay() {
    document.getElementById('score').textContent = score;
    document.getElementById('level').textContent = level;
    document.getElementById('lines').textContent = lines;
}

// Start the game when the page loads
window.addEventListener('load', init);
