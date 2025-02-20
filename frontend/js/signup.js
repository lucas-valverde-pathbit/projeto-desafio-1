document.addEventListener('DOMContentLoaded', () => {
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

    // Função para cadastrar novo usuário
    const signupForm = document.getElementById('signupForm');
    if (signupForm) {
        const signupName = document.getElementById('signupName');
        const signupEmail = document.getElementById('signupEmail');
        const signupPassword = document.getElementById('signupPassword');
        const signupConfirmPassword = document.getElementById('signupConfirmPassword');
        const signupRole = document.getElementById('signupRole');
        
        if (!signupName || !signupEmail || !signupPassword || !signupConfirmPassword || !signupRole) {
            console.error('Elementos do formulário de cadastro não encontrados');
            return;
        }

        signupForm.addEventListener('submit', async (e) => {
            e.preventDefault();
            
            const name = signupName.value;
            const email = signupEmail.value;
            const password = signupPassword.value;
            const confirmPassword = signupConfirmPassword.value;
            const role = signupRole.value.toUpperCase();
            
            if (password !== confirmPassword) {
                showMessage('signupMessage', 'As senhas não coincidem', 'error');
                return;
            }

            if (!['CLIENTE', 'ADMINISTRADOR'].includes(role)) {
                showMessage('signupMessage', 'Tipo de usuário inválido. Escolha entre CLIENTE ou ADMINISTRADOR', 'error');
                return;
            }

            try {
                const response = await fetch(`${apiBaseUrl}/signup`, {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify({ 
                        SignupName: name,
                        SignupEmail: email,
                        SignupPassword: password,
                        SignupRole: role
                    })
                });
                
                const data = await response.json();
                
                if (response.ok) {
                    showMessage('signupMessage', 'Cadastro realizado com sucesso!', 'success');
                    signupForm.reset();
                    openTab('login');
                } else {
                    showMessage('signupMessage', data.message || 'Erro ao cadastrar', 'error');
                }
            } catch (error) {
                console.error('Erro ao realizar o cadastro:', error);
                showMessage('signupMessage', 'Erro de conexão com o servidor', 'error');
            }
        });
    }

    // Função para navegar entre abas/páginas
    function openTab(tabName) {
        window.location.href = `${tabName}.html`;
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
});
