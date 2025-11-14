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

        const historyList = document.getElementById('history-list');
        const noHistory = document.getElementById('no-history');

        if (!games || games.length === 0) {
            noHistory.style.display = 'block';
            return;
        }

        games.forEach(game => {
            const card = createGameCard(game);
            historyList.appendChild(card);
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
            <span>📊 ${game.moves.length} moves</span>
            <span>⏱️ ${duration}</span>
            <span>🏁 ${game.result}</span>
        </div>
        <div class="move-list" id="moves-${game.id}"></div>
    `;

    card.addEventListener('click', () => toggleMoves(game));

    return card;
}

function toggleMoves(game) {
    const moveList = document.getElementById(`moves-${game.id}`);

    if (moveList.style.display === 'none' || !moveList.style.display) {
        if (moveList.children.length === 0) {
            displayMoves(game, moveList);
        }
        moveList.style.display = 'block';
    } else {
        moveList.style.display = 'none';
    }
}

function displayMoves(game, container) {
    const PIECES = {
        'K': '♔', 'Q': '♕', 'R': '♖', 'B': '♗', 'N': '♘', 'P': '♙',
        'k': '♚', 'q': '♛', 'r': '♜', 'b': '♝', 'n': '♞', 'p': '♟'
    };

    const cols = ['a', 'b', 'c', 'd', 'e', 'f', 'g', 'h'];

    game.moves.forEach(move => {
        const moveItem = document.createElement('div');
        moveItem.className = 'move-item';

        const fromPos = `${cols[move.fromCol]}${8 - move.fromRow}`;
        const toPos = `${cols[move.toCol]}${8 - move.toRow}`;
        const piece = PIECES[move.piece] || move.piece;
        const time = new Date(move.timestamp).toLocaleTimeString();

        moveItem.innerHTML = `
            <strong>Move ${move.moveNumber}:</strong>
            ${move.player === 'white' ? '⚪' : '⚫'} 
            ${piece} ${fromPos} → ${toPos}
            <span style="color: #999; float: right;">${time}</span>
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
