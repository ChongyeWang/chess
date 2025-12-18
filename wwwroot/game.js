const PIECES = {
    'K': '♔', 'Q': '♕', 'R': '♖', 'B': '♗', 'N': '♘', 'P': '♙',
    'k': '♚', 'q': '♛', 'r': '♜', 'b': '♝', 'n': '♞', 'p': '♟'
};

let connection = null;
let playerColor = null;
let currentTurn = 'white';
let selectedSquare = null;
let gameBoard = null;
let pieces = [];

let findGameBtn, statusDiv, playerColorDiv, turnIndicatorDiv, gameContainer, chessBoardDiv;
let usernameDisplay, opponentNameDiv, logoutBtn, endGameBtn, moveCountDiv;

function initializeElements() {
    findGameBtn = document.getElementById('find-game-btn');
    statusDiv = document.getElementById('status');
    playerColorDiv = document.getElementById('player-color');
    turnIndicatorDiv = document.getElementById('turn-indicator');
    gameContainer = document.getElementById('game-container');
    chessBoardDiv = document.getElementById('chess-board');
    usernameDisplay = document.getElementById('username-display');
    opponentNameDiv = document.getElementById('opponent-name');
    logoutBtn = document.getElementById('logout-btn');
    endGameBtn = document.getElementById('end-game-btn');
    moveCountDiv = document.getElementById('move-count');

    const navUsername = document.getElementById('nav-username');
    const logoutBtnNav = document.getElementById('logout-btn-nav');

    if (navUsername && currentUsername) {
        navUsername.textContent = currentUsername;
    }

    if (logoutBtnNav) {
        logoutBtnNav.addEventListener('click', async () => {
            await fetch('/api/logout');
            localStorage.removeItem('username');
            window.location.href = '/login.html';
        });
    }

    if (!findGameBtn || !statusDiv || !usernameDisplay) {
        console.error('Some page elements are missing!');
        return false;
    }
    return true;
}

let currentUsername = localStorage.getItem('username');
let currentUserId = null;
let moveCount = 0;

function initializeConnection() {

    connection = new signalR.HubConnectionBuilder()
        .withUrl("/Chesshub")
        .withAutomaticReconnect()
        .configureLogging(signalR.LogLevel.Information)
        .build();

    connection.on("Error", (message) => {
        console.error("SERVER ERROR EVENT:", message);
        statusDiv.textContent = message;
        toast(message, "error", 3500);
    });


    connection.on("WaitingForOpponent", (data) => {
        statusDiv.textContent = "Waiting for opponent...";
        findGameBtn.disabled = true;
    });

    connection.on("GameStart", (data) => {
        statusDiv.textContent = "Game started!";
        gameContainer.style.display = 'block';
        gameBoard = data.board;
        currentTurn = data.currentTurn;

        moveCount = 0;
        const opponentName = playerColor === 'white' ? data.blackPlayer : data.whitePlayer;
        opponentNameDiv.textContent = `Playing against: ${opponentName}`;
        moveCountDiv.textContent = `Moves: 0`;
        endGameBtn.style.display = 'inline-block';
        findGameBtn.style.display = 'none';
        clearMovesList();
        renderBoard(data.board);
        updateTurnIndicator();
    });


    connection.on("AssignColor", (color) => {
        playerColor = color;
        playerColorDiv.textContent = `You are playing as: ${color.toUpperCase()}`;
        playerColorDiv.className = `player-color ${color}`;
    });


    connection.on("UpdateBoard", (updatedBoard, currentTurnFromServer) => {
        gameBoard = updatedBoard;
        pieces = gameBoard.Pieces || gameBoard.pieces || [];

        if(!pieces) {
            console.error("No pieces found in the updated board:", gameBoard);
        }
        currentTurn = currentTurnFromServer;
        moveCount = moveCount + 1;
        selectedSquare = null;
        moveCountDiv.textContent = `Moves: ${moveCount}`;
        renderBoard(gameBoard);
        updateTurnIndicator();
    });

    connection.on("MoveMade", (moveData) => {
        displayMove(moveData);
    });

    connection.on("Check", (colorInCheck) => {
        toast(`${colorInCheck.toUpperCase()} is in CHECK!`, "warn", 4000);
    });

    connection.on("GameEnded", (data) => {
        toast(`Game ended: ${data.reason}`, "info", 6000);
        statusDiv.textContent = `Game ended: ${data.reason}`;
        turnIndicatorDiv.textContent = "";
    });

    connection.on("Game Over", (data) => {
        toast(`Game Over — ${data.winner} (${data.reason})`, "success", 7000);
        statusDiv.textContent = `Game Over — ${data.winner} (${data.reason})`;
        turnIndicatorDiv.textContent = "";
    });




    connection.on("OpponentDisconnected", () => {
        statusDiv.textContent = "Opponent disconnected. Please refresh to find a new game.";
        chessBoardDiv.innerHTML = '';
        gameContainer.style.display = 'none';
        findGameBtn.disabled = false;
    });


    connection.start()
        .then(() => {
            statusDiv.textContent = "Connected! Click 'Find Game' to start.";
        })
        .catch(err => {
            statusDiv.textContent = "Connection failed. Please refresh the page.";
        });




}

function setupEventListeners() {

    findGameBtn.addEventListener('click', async () => {

    if (!connection) {
        console.error("No SignalR connection object");
        return;
    }

    if (connection.state !== signalR.HubConnectionState.Connected) {
        console.warn("SignalR not connected yet:", connection.state);
        return;
    }

    try {
        await connection.invoke("FindGame", currentUsername, currentUserId);
        findGameBtn.disabled = true;
    } catch (err) {
        console.error("FindGame invoke failed:", err);
        findGameBtn.disabled = false;
    }
});

    

    logoutBtn.addEventListener('click', async () => {
        await fetch('/api/logout');
        localStorage.removeItem('username');
        window.location.href = '/login.html';
    });

    endGameBtn.addEventListener('click', () => {
        if (confirm('Are you sure you want to end this game?')) {
            connection.invoke("EndGame", "Player ended game")
                .catch(err => console.error(err));

            window.location.href = '/index.html';
        }
    });
}

function renderBoard(board) {
    chessBoardDiv.innerHTML = '';

    pieces = board.Pieces || board.pieces || [];

    for (let row = 0; row < 8; row++) {
        for (let col = 0; col < 8; col++) {
            const square = document.createElement('div');
            square.className = 'square';
            square.dataset.row = row;
            square.dataset.col = col;

            if ((row + col) % 2 === 0) {
                square.classList.add('light');
            } else {
                square.classList.add('dark');
            }

            const piece = pieces.find(p => p.XPosition === col && p.YPosition === row);


            if (piece) {
                const key = piece.Color === "white" ? piece.Symbol.toUpperCase() : piece.Symbol.toLowerCase();
                square.textContent = PIECES[key];

                if (piece.Color === "white") {
                    square.style.color = '#ffffff';
                } else {
                    square.style.color = '#000000';
                }
            }


            square.addEventListener('click', () => handleSquareClick(row, col));

            chessBoardDiv.appendChild(square);
        }
    }
}

async function handleSquareClick(row, col) {
    if (currentTurn !== playerColor) {
        console.warn("Not your turn!");
        return;
    }

    const piece = pieces.find(p => p.XPosition === col && p.YPosition === row);;

    if (selectedSquare === null) {
        if (piece && isPieceOwnedByPlayer(piece)) {
            selectedSquare = { row, col };
            highlightSquare(row, col);
        }
        else if (piece) {
            console.error(`Selection failed. Piece Color: '${piece.Color}' vs Player Color: '${playerColor}'`);
        }
    } else {
        if (selectedSquare.row === row && selectedSquare.col === col) {
            selectedSquare = null;
            renderBoard(gameBoard);
        } else {
            await movePiece(selectedSquare.row, selectedSquare.col, row, col);
        }
    }
}

function isPieceOwnedByPlayer(piece) {
    if (!piece || typeof piece.Color !== 'string') {
        console.error("Piece object is missing 'Color' property:", piece);
        return false;
    }
    return piece.Color.toLowerCase() === playerColor.toLowerCase();
}

function highlightSquare(row, col) {
    renderBoard(gameBoard);
    const squares = chessBoardDiv.children;
    const index = row * 8 + col;
    squares[index].classList.add('selected');
}

async function movePiece(fromRow, fromCol, toRow, toCol) {    
    try {
        const result = await connection.invoke("MovePiece",
            fromRow.toString(),
            fromCol.toString(),
            toRow.toString(),
            toCol.toString()
        );
    }catch (err) {
        console.error("MovePiece invoke failed:", err);
    }
}




function updateTurnIndicator() {
    if (currentTurn.toLowerCase() === playerColor.toLowerCase()) {
        turnIndicatorDiv.textContent = "Your turn";
        turnIndicatorDiv.className = "turn-indicator your-turn";
    } else {
        turnIndicatorDiv.textContent = "Opponent's turn";
        turnIndicatorDiv.className = "turn-indicator opponent-turn";
    }
}

function toast(msg, ms = 2500) {
  const el = document.getElementById("toast");
  if (!el) return;
  el.textContent = msg;
  el.classList.remove("hidden");
  setTimeout(() => el.classList.add("hidden"), ms);
}

function displayMove(moveData) {
    const movesList = document.getElementById('moves-list');
    if (!movesList) return;
    const cols = ['a', 'b', 'c', 'd', 'e', 'f', 'g', 'h'];
    const fromCol = moveData.fromCol !== undefined ? moveData.fromCol : moveData.FromCol;
    const fromRow = moveData.fromRow !== undefined ? moveData.fromRow : moveData.FromRow;
    const toCol = moveData.toCol !== undefined ? moveData.toCol : moveData.ToCol;
    const toRow = moveData.toRow !== undefined ? moveData.toRow : moveData.ToRow;
    const piece = moveData.piece || moveData.Piece || '';
    const player = moveData.player || moveData.Player || '';
    const moveNumber = moveData.moveNumber || moveData.MoveNumber || 0;

    const fromPos = `${cols[fromCol]}${8 - fromRow}`;
    const toPos = `${cols[toCol]}${8 - toRow}`;
    const pieceSymbol = PIECES[piece] || piece;

    const moveItem = document.createElement('div');
    moveItem.className = `move-item ${player}`;
    
    moveItem.innerHTML = `
        <span class="move-number">${moveNumber}.</span>
        <span class="move-notation">${pieceSymbol} ${fromPos} → ${toPos}</span>
        <span style="color: #999; float: right; font-size: 0.85em;">${player === 'white' ? '⚪' : '⚫'}</span>
    `;

    movesList.appendChild(moveItem);
    movesList.scrollTop = movesList.scrollHeight;
}

function clearMovesList() {
    const movesList = document.getElementById('moves-list');
    if (movesList) {
        movesList.innerHTML = '';
    }
}

window.addEventListener('load', async () => {
    if (!initializeElements()) {
        console.error('Failed to initialize page elements');
        return;
    }

    if (!currentUsername) {
        window.location.href = '/login.html';
        return;
    }

    try {
        const response = await fetch('/api/me', { credentials: 'include' });
        if (response.ok) {
            const data = await response.json();
            currentUserId = data.userId;
        } else {
            window.location.href = '/login.html';
            return;
        }
    } catch (error) {
        console.error('Error fetching user info:', error);
        window.location.href = '/login.html';
        return;
    }

    usernameDisplay.textContent = `Welcome, ${currentUsername}!`;
    setupEventListeners();
    initializeConnection();
});