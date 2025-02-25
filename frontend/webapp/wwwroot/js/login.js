window.onload = () => {
    const apiBaseUrl = window.location.hostname === "localhost"
                       ? "http://localhost:5064/api/users"
                       : "http://api:5064/api/users";
    
    // Função para verificar conexão com a API
    async function checkApiConnection() {
        try {
            const response = await fetch(`${apiBaseUrl}/status`, {
                method: 'GET',
                headers: { 'Content-Type': 'application/json' }
            });
            return response.ok;
        } catch (error) {
            console.error('Erro ao conectar com a API:', error);
            return false;
        }
    }

    // Função para fazer login com validação
    const loginForm = document.getElementById('loginForm');
    if (loginForm) {
        const loginEmail = document.getElementById('loginEmail');
        const loginPassword = document.getElementById('loginPassword');
        
        if (!loginEmail || !loginPassword) {
            console.error('Elementos do formulário de login não encontrados');
            return;
        }

        loginForm.addEventListener('submit', async (e) => {
            e.preventDefault();
            
            const email = loginEmail.value.trim();
            const password = loginPassword.value;
            
            // Verifica conexão com a API antes de tentar login
            const apiAvailable = await checkApiConnection();
            if (!apiAvailable) {
                showMessage('loginMessage', 'Serviço indisponível. Tente novamente mais tarde.', 'error');
                return;
            }

            try {
                showMessage('loginMessage', 'Fazendo login, por favor aguarde...', 'info');
                const response = await fetch(`${apiBaseUrl}/login`, { 
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify({
                        LoginEmail: email,
                        LoginPassword: password
                    })
                });

                if (!response.ok) { 
                    showMessage('loginMessage', 'Erro ao fazer login. Verifique suas credenciais.', 'error');
                    throw new Error(`HTTP error! status: ${response.status}`);
                }

                let responseData;
                try {
                    responseData = await response.json();
                } catch (error) {
                    const text = await response.text();
                    showMessage('loginMessage', text || 'Erro ao fazer login. Verifique suas credenciais.', 'error'); 
                    loginEmail.value = '';
                    loginPassword.value = '';
                    console.error('Login error:', text);
                    return;
                }

                if (response.ok) {
                    console.log('Resposta do login:', responseData);
                    const userData = {
                        id: responseData.id,
                        name: responseData.userName,
                        email: responseData.userEmail,
                        role: responseData.role
                    };
                    console.log('Armazenando dados do usuário:', userData);
                    localStorage.setItem('userData', JSON.stringify(userData));
                    console.log('Token recebido:', responseData.Token);
                    localStorage.setItem('token', responseData.Token);
                    console.log('Token armazenado:', localStorage.getItem('token'));
                    window.location.href = 'index.html';
                }
            } catch (error) {
                console.error('Erro no login:', error);
                const errorMsg = error.message.includes('HTTP error')
                    ? 'Credenciais inválidas. Verifique seu email e senha.'
                    : 'Erro ao conectar com o servidor. Tente novamente.';
                showMessage('loginMessage', errorMsg, 'error');
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

    // Verifica se há um token ao carregar a página
    if (localStorage.getItem('token')) {
        console.log('Token encontrado ao carregar a página, redirecionando para index.html');
        window.location.href = 'index.html';
    }
}
