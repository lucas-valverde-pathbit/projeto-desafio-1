const apiBaseUrl = window.location.hostname === "localhost" 
                   ? "http://localhost:5064/api/users" 
                   : "http://api:5064/api/users";

// Função para abrir as tabs
function openTab(tabName) {
    const tabs = document.querySelectorAll('.tab-content');
    tabs.forEach(tab => tab.classList.remove('active'));
    
    const tabLinks = document.querySelectorAll('.tab-link');
    tabLinks.forEach(link => link.classList.remove('active'));
    
    document.getElementById(tabName).classList.add('active');
    document.querySelector(`[onclick="openTab('${tabName}')"]`).classList.add('active');
}

// Função para fazer login
const loginForm = document.getElementById('loginForm');
if (loginForm) {
    loginForm.addEventListener('submit', async (e) => {
        e.preventDefault();
        
        const email = document.getElementById('signupEmail').value;
        const password = document.getElementById('signupPassword').value;
        
        try {
            const response = await fetch(`${apiBaseUrl}/login`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ email, password })
            });
            
            const data = await response.json();
            
            if (response.ok) {
                localStorage.setItem('jwtToken', data.token);
                window.location.href = 'index.html';
            } else {
                showMessage('loginMessage', data.message || 'Erro ao fazer login', 'error');
            }
        } catch (error) {
            showMessage('loginMessage', 'Erro de conexão com o servidor', 'error');
        }
    });
}

// Função para cadastrar novo usuário
const signupForm = document.getElementById('signupForm');
if (signupForm) {
    signupForm.addEventListener('submit', async (e) => {
        e.preventDefault();
        
        const name = document.getElementById('signupName').value;
        const email = document.getElementById('signupEmail').value;
        const password = document.getElementById('signupPassword').value;
        const confirmPassword = document.getElementById('signupConfirmPassword').value;
        
        // Verificando se as senhas coincidem
        if (password !== confirmPassword) {
            showMessage('signupMessage', 'As senhas não coincidem', 'error');
            return;
        }

        const role = document.getElementById('signupRole').value.toUpperCase();
        if (!['CLIENTE', 'ADMINISTRADOR'].includes(role)) {
            showMessage('signupMessage', 'Tipo de usuário inválido. Escolha entre CLIENTE ou ADMINISTRADOR', 'error');
            return;
        }

        // Exibindo os valores dos campos antes de enviar
        console.log({
            SignupName: name,
            SignupEmail: email,
            SignupPassword: password,
            SignupRole: role
        });

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
            
            // Verificando a resposta da API
            if (response.ok) {
                showMessage('signupMessage', 'Cadastro realizado com sucesso!', 'success');
                document.getElementById('signupForm').reset();
            } else {
                showMessage('signupMessage', data.message || 'Erro ao cadastrar', 'error');
            }
        } catch (error) {
            console.error('Erro ao realizar o cadastro:', error);
            showMessage('signupMessage', 'Erro de conexão com o servidor', 'error');
        }
    });
}

// Função para exibir mensagens
function showMessage(elementId, message, type) {
    const messageElement = document.getElementById(elementId);
    messageElement.textContent = message;
    messageElement.className = `message ${type}`;
    messageElement.style.display = 'block';
    
    setTimeout(() => {
        messageElement.style.display = 'none';
    }, 5000);
}

// Verifica autenticação ao carregar a página
window.addEventListener('load', () => {
    const token = localStorage.getItem('jwtToken');
    
    if (!token && window.location.pathname.endsWith('index.html')) {
        window.location.href = 'index.html';
        return;
    }

    if (token && window.location.pathname.includes('user.html')) {
        window.location.href = 'index.html';
    }

    if (token) {
        try {
            const payload = JSON.parse(atob(token.split('.')[1]));
            const currentTime = Math.floor(Date.now() / 1000);
            
            if (payload.exp < currentTime) {
                localStorage.removeItem('jwtToken');
                window.location.href = 'index.html';
            }
        } catch (error) {
            console.error('Erro ao verificar token:', error);
            localStorage.removeItem('jwtToken');
            window.location.href = 'index.html';
        }
    }
});
