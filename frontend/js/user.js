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


    // Função para abrir as tabs
    function openTab(tabName) {
        const tabs = document.querySelectorAll('.tab-content');
        tabs.forEach(tab => tab.classList.remove('active'));
        
        const tabLinks = document.querySelectorAll('.tab-link');
        tabLinks.forEach(link => link.classList.remove('active'));
        
        const tabElement = document.getElementById(tabName);
        const tabLink = document.querySelector(`[onclick="openTab('${tabName}')"]`);
        
        if (tabElement && tabLink) {
            tabElement.classList.add('active');
            tabLink.classList.add('active');
        }
    }

    // Função para fazer login
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
            
            const email = loginEmail.value;
            const password = loginPassword.value;
            
            // Verifica conexão com a API antes de tentar login
            const apiAvailable = await checkApiConnection();
            if (!apiAvailable) {
                showMessage('loginMessage', 'Serviço indisponível. Tente novamente mais tarde.', 'error');
                return;
            }

            try {
                const response = await fetch(`${apiBaseUrl}/login`, {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify({
                        LoginEmail: email,
                        LoginPassword: password
                    })
                });

                if (!response.ok) {
                    throw new Error(`HTTP error! status: ${response.status}`);
                }


                let responseData;
                try {
                    responseData = await response.json();
                } catch (error) {
                    // Handle non-JSON responses
                    const text = await response.text();
                    showMessage('loginMessage', text || 'Erro ao fazer login. Verifique suas credenciais.', 'error');
                    console.error('Login error:', text);
                    return;
                }
                
                if (response.ok && responseData.token) {
                    localStorage.setItem('jwtToken', responseData.token);
                    // Verify token was stored before redirect
                    if (localStorage.getItem('jwtToken')) {
                        window.location.href = 'index.html';
                    } else {
                        showMessage('loginMessage', 'Erro ao armazenar token. Tente novamente.', 'error');
                    }
                } else {


                    const errorMsg = responseData.message || 'Erro ao fazer login. Verifique suas credenciais.';
                    showMessage('loginMessage', errorMsg, 'error');
                    console.error('Login error:', responseData);
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
            
            // Verificando se as senhas coincidem
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
                    // Open login tab after successful signup
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

    // Verifica autenticação ao carregar a página
    const token = localStorage.getItem('jwtToken');
    
    // Only redirect if trying to access login page while already authenticated
    if (token && window.location.pathname.includes('login')) {
        window.location.href = 'index.html';
    }

    // Redirect to login if not authenticated and trying to access protected pages
    if (!token && !window.location.pathname.includes('user.html')) {
        // Store intended destination before redirect
        sessionStorage.setItem('redirectAfterLogin', window.location.pathname);
        window.location.href = 'user.html';
        return;
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
