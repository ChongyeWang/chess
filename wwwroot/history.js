const PIECES = {
    'K': '♔', 'Q': '♕', 'R': '♖', 'B': '♗', 'N': '♘', 'P': '♙',
    'k': '♚', 'q': '♛', 'r': '♜', 'b': '♝', 'n': '♞', 'p': '♟'
};

const gameMoveIndices = {};

window.loadedGames = [];

async function loadHistory() {
    try {
        const response = await fetch('/api/history', {
            credentials: 'include'
        });

        if (!response.ok) {
            console.error('Failed to fetch history:', response.status);
            window.location.href = '/login.html';
            return;
        }

        const games = await response.json();
        console.log('Games loaded:', games);
        
        window.loadedGames = games;

        const historyList = document.getElementById('history-list');
        const noHistory = document.getElementById('no-history');

        if (!games || games.length === 0) {
            noHistory.style.display = 'block';
            return;
        }

        games.forEach(game => {
            const card = createGameCard(game);
            historyList.appendChild(card);
            gameMoveIndices[game.id] = -1;
        });
    } catch (error) {
        console.error('Error loading history:', error);
        alert('Error loading game history: ' + error.message);
    }
}

function createGameCard(game) {
    const card = document.createElement('div');
    card.className = 'game-card';

    const endDate = new Date(game.endTime).toLocaleString();
    const duration = calculateDuration(game.startTime, game.endTime);

    card.innerHTML = `
        <div class="game-header">
            <div class="game-players">
                ${game.whitePlayerName} (White) vs ${game.blackPlayerName} (Black)
            </div>
            <div class="game-date">${endDate}</div>
        </div>
        <div class="game-details">
            <span>📊 ${(game.moves || game.Moves || []).length} moves</span>
            <span>⏱️ ${duration}</span>
            <span>🏁 ${game.result || game.Result}</span>
        </div>
        <div class="board-viewer" id="board-viewer-${game.id}">
            <div class="board-navigation">
                <button class="nav-btn" id="prev-btn-${game.id}">← Previous</button>
                <div class="move-info" id="move-info-${game.id}">Initial Position</div>
                <button class="nav-btn" id="next-btn-${game.id}">Next →</button>
            </div>
            <div class="history-board-container">
                <div class="history-board" id="history-board-${game.id}"></div>
            </div>
        </div>
        <div class="move-list" id="moves-${game.id}"></div>
    `;

    const prevBtn = card.querySelector(`#prev-btn-${game.id}`);
    const nextBtn = card.querySelector(`#next-btn-${game.id}`);
    
    prevBtn.addEventListener('click', (e) => {
        e.stopPropagation();
        previousMove(game.id);
    });
    
    nextBtn.addEventListener('click', (e) => {
        e.stopPropagation();
        nextMove(game.id);
    });

    card.addEventListener('click', (e) => {
        if (!e.target.closest('.board-navigation') && !e.target.closest('.history-board') && !e.target.closest('.board-viewer')) {
            toggleBoardViewer(game);
        }
    });

    return card;
}

function toggleBoardViewer(game) {
    const boardViewer = document.getElementById(`board-viewer-${game.id}`);
    const boardDiv = document.getElementById(`history-board-${game.id}`);

    if (boardViewer.style.display === 'none' || !boardViewer.style.display) {
        if (boardDiv && boardDiv.children.length === 0) {
            gameMoveIndices[game.id] = -1;
            renderBoardAtMove(game, -1);
        }
        boardViewer.style.display = 'block';
        updateNavigationButtons(game);
    } else {
        boardViewer.style.display = 'none';
    }
}

function initializeBoard() {
    const pieces = [];
    const blackRanks = ['r', 'n', 'b', 'q', 'k', 'b', 'n', 'r'];
    const whiteRanks = ['R', 'N', 'B', 'Q', 'K', 'B', 'N', 'R'];

    for (let i = 0; i < 8; i++) {
        pieces.push({ type: blackRanks[i], color: 'black', xPosition: i, yPosition: 0 });
        pieces.push({ type: 'p', color: 'black', xPosition: i, yPosition: 1 });
        pieces.push({ type: 'P', color: 'white', xPosition: i, yPosition: 6 });
        pieces.push({ type: whiteRanks[i], color: 'white', xPosition: i, yPosition: 7 });
    }

    return pieces;
}

function applyMove(board, move) {
    const newBoard = board.map(p => ({ ...p }));

    const fromCol = move.fromCol !== undefined ? move.fromCol : move.FromCol;
    const fromRow = move.fromRow !== undefined ? move.fromRow : move.FromRow;
    const toCol = move.toCol !== undefined ? move.toCol : move.ToCol;
    const toRow = move.toRow !== undefined ? move.toRow : move.ToRow;

    const piece = newBoard.find(p => p.xPosition === fromCol && p.yPosition === fromRow);
    
    if (!piece) {
        console.error('Piece not found at move position:', move);
        return newBoard;
    }

    const capturedIndex = newBoard.findIndex(p => p.xPosition === toCol && p.yPosition === toRow);
    if (capturedIndex !== -1) {
        newBoard.splice(capturedIndex, 1);
    }

    piece.xPosition = toCol;
    piece.yPosition = toRow;

    return newBoard;
}

function renderBoardAtMove(game, moveIndex) {
    const boardDiv = document.getElementById(`history-board-${game.id}`);
    if (!boardDiv) return;
    
    boardDiv.innerHTML = '';

    const moves = game.moves || game.Moves || [];

    let currentBoard = initializeBoard();

    for (let i = 0; i <= moveIndex && i < moves.length; i++) {
        currentBoard = applyMove(currentBoard, moves[i]);
    }

    for (let row = 0; row < 8; row++) {
        for (let col = 0; col < 8; col++) {
            const square = document.createElement('div');
            square.className = 'history-square';
            
            if ((row + col) % 2 === 0) {
                square.classList.add('light');
            } else {
                square.classList.add('dark');
            }

            const piece = currentBoard.find(p => p.xPosition === col && p.yPosition === row);
            
            if (piece) {
                const key = piece.color === 'white' ? piece.type.toUpperCase() : piece.type.toLowerCase();
                square.textContent = PIECES[key] || piece.type;
                square.style.color = piece.color === 'white' ? '#ffffff' : '#000000';
            }

            boardDiv.appendChild(square);
        }
    }

    const moveInfo = document.getElementById(`move-info-${game.id}`);
    if (moveInfo) {
        if (moveIndex === -1) {
            moveInfo.textContent = 'Initial Position';
        } else if (moveIndex < moves.length) {
            const move = moves[moveIndex];
            const cols = ['a', 'b', 'c', 'd', 'e', 'f', 'g', 'h'];
            const fromCol = move.fromCol !== undefined ? move.fromCol : move.FromCol;
            const fromRow = move.fromRow !== undefined ? move.fromRow : move.FromRow;
            const toCol = move.toCol !== undefined ? move.toCol : move.ToCol;
            const toRow = move.toRow !== undefined ? move.toRow : move.ToRow;
            const piece = move.piece || move.Piece || '';
            const player = move.player || move.Player || '';
            const moveNumber = move.moveNumber || move.MoveNumber || moveIndex + 1;
            
            const fromPos = `${cols[fromCol]}${8 - fromRow}`;
            const toPos = `${cols[toCol]}${8 - toRow}`;
            const pieceSymbol = PIECES[piece] || piece;
            moveInfo.textContent = `Move ${moveNumber}: ${pieceSymbol} ${fromPos} → ${toPos} (${player})`;
        } else {
            moveInfo.textContent = 'Final Position';
        }
    }
}

function updateNavigationButtons(game) {
    const currentIndex = gameMoveIndices[game.id];
    const prevBtn = document.getElementById(`prev-btn-${game.id}`);
    const nextBtn = document.getElementById(`next-btn-${game.id}`);
    const moves = game.moves || game.Moves || [];

    if (prevBtn) prevBtn.disabled = currentIndex === -1;
    if (nextBtn) nextBtn.disabled = currentIndex >= moves.length - 1;
}

function previousMove(gameId) {
    if (gameMoveIndices[gameId] > -1) {
        gameMoveIndices[gameId]--;
        const game = getGameById(gameId);
        if (game) {
            renderBoardAtMove(game, gameMoveIndices[gameId]);
            updateNavigationButtons(game);
        }
    }
}

function nextMove(gameId) {
    const game = getGameById(gameId);
    if (!game) return;
    
    const moves = game.moves || game.Moves || [];
    if (gameMoveIndices[gameId] < moves.length - 1) {
        gameMoveIndices[gameId]++;
        renderBoardAtMove(game, gameMoveIndices[gameId]);
        updateNavigationButtons(game);
    }
}

function getGameById(gameId) {
    return window.loadedGames?.find(g => g.id === gameId);
}

function displayMoves(game, container) {
    const cols = ['a', 'b', 'c', 'd', 'e', 'f', 'g', 'h'];
    const moves = game.moves || game.Moves || [];

    moves.forEach(move => {
        const moveItem = document.createElement('div');
        moveItem.className = 'move-item';

        const fromCol = move.fromCol !== undefined ? move.fromCol : move.FromCol;
        const fromRow = move.fromRow !== undefined ? move.fromRow : move.FromRow;
        const toCol = move.toCol !== undefined ? move.toCol : move.ToCol;
        const toRow = move.toRow !== undefined ? move.toRow : move.ToRow;
        const piece = PIECES[move.piece || move.Piece] || move.piece || move.Piece;
        const player = move.player || move.Player;
        const moveNumber = move.moveNumber || move.MoveNumber;
        const timestamp = move.timestamp || move.Timestamp;

        const fromPos = `${cols[fromCol]}${8 - fromRow}`;
        const toPos = `${cols[toCol]}${8 - toRow}`;
        const time = timestamp ? new Date(timestamp).toLocaleTimeString() : '';

        moveItem.innerHTML = `
            <strong>Move ${moveNumber}:</strong>
            ${player === 'white' ? '⚪' : '⚫'} 
            ${piece} ${fromPos} → ${toPos}
            ${time ? `<span style="color: #999; float: right;">${time}</span>` : ''}
        `;

        container.appendChild(moveItem);
    });
}

function calculateDuration(start, end) {
    const ms = new Date(end) - new Date(start);
    const minutes = Math.floor(ms / 60000);
    const seconds = Math.floor((ms % 60000) / 1000);
    return `${minutes}m ${seconds}s`;
}

async function logout() {
    await fetch('/api/logout');
    localStorage.removeItem('username');
    window.location.href = '/login.html';
}

window.addEventListener('load', () => {
    const username = localStorage.getItem('username');
    if (username) {
        const navUsername = document.getElementById('nav-username');
        if (navUsername) {
            navUsername.textContent = username;
        }
    }
    loadHistory();
});
