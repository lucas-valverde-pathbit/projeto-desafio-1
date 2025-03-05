import { checkToken } from './permissions.js';

window.onload = () => {
    const apiBaseUrl = window.location.hostname === "localhost"
                       ? "http://localhost:5064/api/users"
                       : "http://api:5064/api/users";

    // Verificar se o usuário já está autenticado
    if (checkToken()) {
        window.location.href = 'home.html'; // Redirecionar para home.html se o token estiver presente
    }

    // Função para fazer login
    const loginForm = document.getElementById('loginForm');
    if (loginForm) {
        const loginEmail = document.getElementById('loginEmail');
        const loginPassword = document.getElementById('loginPassword');

        loginForm.addEventListener('submit', async (e) => {
            e.preventDefault();

            const email = loginEmail.value.trim();
            const password = loginPassword.value;

            // Chamada para login na API
            try {
                const response = await fetch(`${apiBaseUrl}/login`, { 
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify({ LoginEmail: email, LoginPassword: password })
                });

                const responseData = await response.json();
                console.log(responseData); // Adicionando log

                if (!response.ok) { 
                    showMessage('loginMessage', 'Erro ao fazer login. Verifique suas credenciais.', 'error');
                    return;
                }

                // Verificar se a resposta contém o token
                if (responseData.token) {
                    localStorage.setItem('token', responseData.token);
                    // Decodificar o token e armazenar informações do usuário
                    const userInfo = decodeToken(responseData.token);
                    localStorage.setItem('userInfo', JSON.stringify(userInfo));
                    console.log('Token e informações do usuário armazenados:', responseData.token, userInfo);
                    window.location.href = 'home.html'; // Redireciona para home.html

                } else {
                    showMessage('loginMessage', 'Erro ao receber o token.', 'error');
                }

            } catch (error) {
                showMessage('loginMessage', 'Erro ao conectar com o servidor. Tente novamente.', 'error');
            }
        });
    }

    // Função para exibir mensagens
    function showMessage(elementId, message, type) {
        const messageElement = document.getElementById(elementId);
        if (messageElement) {
            messageElement.textContent = message;
            messageElement.className = `message ${type}`;
            messageElement.style.display = 'block';
            
            setTimeout(() => {
                messageElement.style.display = 'none';
            }, 5000);
        }
    }

    // Função para decodificar o token JWT
    function decodeToken(token) {
        try {
            const base64Url = token.split('.')[1];
            const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
            const jsonPayload = decodeURIComponent(atob(base64).split('').map(function(c) {
                return '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2);
            }).join(''));
            
            return JSON.parse(jsonPayload);
        } catch (error) {
            console.error('Erro ao decodificar token:', error);
            return null;
        }
    }
};
