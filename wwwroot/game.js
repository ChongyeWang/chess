const PIECES = {
    'K': '♔', 'Q': '♕', 'R': '♖', 'B': '♗', 'N': '♘', 'P': '♙',
    'k': '♚', 'q': '♛', 'r': '♜', 'b': '♝', 'n': '♞', 'p': '♟'
};

let connection = null;
let playerColor = null;
let currentTurn = 'white';
let selectedSquare = null;
let gameBoard = null;

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
        .withUrl("/chesshub")
        .withAutomaticReconnect()
        .build();


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
        console.log("Received UpdateBoard:", updatedBoard, "Current turn:", currentTurnFromServer);
        gameBoard = updatedBoard;
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



    connection.on("OpponentDisconnected", () => {
        statusDiv.textContent = "Opponent disconnected. Please refresh to find a new game.";
        chessBoardDiv.innerHTML = '';
        gameContainer.style.display = 'none';
        findGameBtn.disabled = false;
    });

    connection.on("Error", (message) => {
        alert(message);
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
    findGameBtn.addEventListener('click', () => {
        if (connection && connection.state === signalR.HubConnectionState.Connected) {
            connection.invoke("FindGame", currentUsername, currentUserId)
                .catch(err => console.error(err));
            findGameBtn.disabled = true;
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
        }
    });
}

function renderBoard(board) {
    chessBoardDiv.innerHTML = '';

    if (!board) {
        console.error("No board data");
        return;
    }

    gameBoard = board;

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

            const piece = gameBoard.find(p => p.xPosition === col && p.yPosition === row);


            if (piece) {
                const key = piece.color === "white" ? piece.type.toUpperCase() : piece.type.toLowerCase();
                square.textContent = PIECES[key];
                if (piece.color === "white") {
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

function handleSquareClick(row, col) {
    if (currentTurn !== playerColor) {
        return;
    }

    const piece = gameBoard.find(p => p.xPosition === col && p.yPosition === row);;

    if (selectedSquare === null) {
        if (piece && isPieceOwnedByPlayer(piece)) {
            selectedSquare = { row, col };
            highlightSquare(row, col);
        }
    } else {
        if (selectedSquare.row === row && selectedSquare.col === col) {
            selectedSquare = null;
            renderBoard(gameBoard);
        } else {
            movePiece(selectedSquare.row, selectedSquare.col, row, col);
        }
    }
}

function isPieceOwnedByPlayer(piece) {
    return piece.color.toLowerCase() === playerColor.toLowerCase();
}

function highlightSquare(row, col) {
    renderBoard(gameBoard);
    const squares = chessBoardDiv.children;
    const index = row * 8 + col;
    squares[index].classList.add('selected');
}

function movePiece(fromRow, fromCol, toRow, toCol) {
    console.log("movePiece called:", fromRow, fromCol, "->", toRow, toCol);

    if (connection && connection.state === signalR.HubConnectionState.Connected) {
        connection.invoke("MovePiece",
            fromRow.toString(),
            fromCol.toString(),
            toRow.toString(),
            toCol.toString()
        ).catch(err => console.error("❌ MovePiece invoke failed:", err));
    } else {
        console.warn("⚠️ Not connected to SignalR yet!");
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

function displayMove(moveData) {
    const movesList = document.getElementById('moves-list');
    if (!movesList) return;

    const PIECES = {
        'K': '♔', 'Q': '♕', 'R': '♖', 'B': '♗', 'N': '♘', 'P': '♙',
        'k': '♚', 'q': '♛', 'r': '♜', 'b': '♝', 'n': '♞', 'p': '♟'
    };

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
            console.log('User ID:', currentUserId);
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