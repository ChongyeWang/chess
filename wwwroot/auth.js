const loginForm = document.getElementById('loginForm');
const registerForm = document.getElementById('registerForm');
const showRegister = document.getElementById('show-register');
const showLogin = document.getElementById('show-login');
const errorMessage = document.getElementById('error-message');

showRegister.addEventListener('click', (e) => {
    e.preventDefault();
    document.getElementById('login-form').style.display = 'none';
    document.getElementById('register-form').style.display = 'block';
    errorMessage.textContent = '';
});

showLogin.addEventListener('click', (e) => {
    e.preventDefault();
    document.getElementById('register-form').style.display = 'none';
    document.getElementById('login-form').style.display = 'block';
    errorMessage.textContent = '';
});

loginForm.addEventListener('submit', async (e) => {
    e.preventDefault();

    const username = document.getElementById('login-username').value;
    const password = document.getElementById('login-password').value;

    try {
        const response = await fetch('/api/login', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify({ username, password })
        });

        if (response.ok) {
            const data = await response.json();
            localStorage.setItem('username', data.username);
            window.location.href = '/index.html';
        } else {
            errorMessage.textContent = 'Invalid username or password';
        }
    } catch (error) {
        errorMessage.textContent = 'Login failed. Please try again.';
    }
});

registerForm.addEventListener('submit', async (e) => {
    e.preventDefault();

    const username = document.getElementById('register-username').value;
    const email = document.getElementById('register-email').value;
    const password = document.getElementById('register-password').value;

    try {
        const response = await fetch('/api/register', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify({ username, email, password })
        });

        if (response.ok) {
            const data = await response.json();
            localStorage.setItem('username', data.username);
            window.location.href = '/index.html';
        } else {
            const error = await response.json();
            errorMessage.textContent = error.message || 'Registration failed';
        }
    } catch (error) {
        errorMessage.textContent = 'Registration failed. Please try again.';
    }
});
