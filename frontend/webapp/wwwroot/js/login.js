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
                    console.log('Token armazenado no localStorage:', responseData.token);
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
};
