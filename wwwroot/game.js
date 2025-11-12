const PIECES = {
    'K': '♔', 'Q': '♕', 'R': '♖', 'B': '♗', 'N': '♘', 'P': '♙',
    'k': '♚', 'q': '♛', 'r': '♜', 'b': '♝', 'n': '♞', 'p': '♟'
};

let connection = null;
let playerColor = null;
let currentTurn = 'white';
let selectedSquare = null;
let gameBoard = null;

const findGameBtn = document.getElementById('find-game-btn');
const statusDiv = document.getElementById('status');
const playerColorDiv = document.getElementById('player-color');
const turnIndicatorDiv = document.getElementById('turn-indicator');
const gameContainer = document.getElementById('game-container');
const chessBoardDiv = document.getElementById('chess-board');
const usernameDisplay = document.getElementById('username-display');
const opponentNameDiv = document.getElementById('opponent-name');
const logoutBtn = document.getElementById('logout-btn');
const endGameBtn = document.getElementById('end-game-btn');
const moveCountDiv = document.getElementById('move-count');

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
        
        renderBoard(data.board);
        updateTurnIndicator();
    });

    connection.on("AssignColor", (color) => {
        playerColor = color;
        playerColorDiv.textContent = `You are playing as: ${color.toUpperCase()}`;
        playerColorDiv.className = `player-color ${color}`;
    });

    connection.on("PieceMoved", (data) => {
        gameBoard[data.toRow][data.toCol] = data.piece;
        gameBoard[data.fromRow][data.fromCol] = '';
        currentTurn = data.currentTurn;
        moveCount = data.moveNumber || moveCount + 1;
        moveCountDiv.textContent = `Moves: ${moveCount}`;
        renderBoard(gameBoard);
        selectedSquare = null;
        updateTurnIndicator();
    });
    
    connection.on("GameEnded", (data) => {
        statusDiv.textContent = `Game ended: ${data.reason}`;
        statusDiv.style.color = '#e74c3c';
        endGameBtn.style.display = 'none';
        findGameBtn.style.display = 'inline-block';
        findGameBtn.disabled = false;
        gameContainer.style.display = 'none';
        alert(`Game Over!\nReason: ${data.reason}\nTotal Moves: ${data.totalMoves}`);
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
            console.log("Connected to server");
            statusDiv.textContent = "Connected! Click 'Find Game' to start.";
        })
        .catch(err => {
            console.error(err);
            statusDiv.textContent = "Connection failed. Please refresh the page.";
        });
}

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

function renderBoard(board) {
    chessBoardDiv.innerHTML = '';
    
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
            
            const piece = board[row][col];
            if (piece) {
                square.textContent = PIECES[piece];
                if (piece === piece.toUpperCase()) {
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

    const piece = gameBoard[row][col];
    
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
    if (playerColor === 'white') {
        return piece === piece.toUpperCase();
    } else {
        return piece === piece.toLowerCase();
    }
}

function highlightSquare(row, col) {
    renderBoard(gameBoard);
    const squares = chessBoardDiv.children;
    const index = row * 8 + col;
    squares[index].classList.add('selected');
}

function movePiece(fromRow, fromCol, toRow, toCol) {
    if (connection && connection.state === signalR.HubConnectionState.Connected) {
        connection.invoke("MovePiece", 
            fromRow.toString(), 
            fromCol.toString(), 
            toRow.toString(), 
            toCol.toString()
        ).catch(err => console.error(err));
    }
}

function updateTurnIndicator() {
    if (currentTurn === playerColor) {
        turnIndicatorDiv.textContent = "Your turn";
        turnIndicatorDiv.className = "turn-indicator your-turn";
    } else {
        turnIndicatorDiv.textContent = "Opponent's turn";
        turnIndicatorDiv.className = "turn-indicator opponent-turn";
    }
}

window.addEventListener('load', async () => {
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
    initializeConnection();
});

