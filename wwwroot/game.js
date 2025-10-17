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
        renderBoard(gameBoard);
        selectedSquare = null;
        updateTurnIndicator();
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
        connection.invoke("FindGame")
            .catch(err => console.error(err));
        findGameBtn.disabled = true;
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

window.addEventListener('load', () => {
    initializeConnection();
});

