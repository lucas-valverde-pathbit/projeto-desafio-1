// Verifica se o usuário está autenticado
const token = localStorage.getItem('jwtToken');

// Seleciona todos os botões com a classe 'admin-only'
const adminButtons = document.querySelectorAll('.admin-only');

// Se não houver token, redireciona para login
if (!token) {
    window.location.href = 'login.html';
} else {
    // Decodifica o token e verifica o papel do usuário
    const userRole = JSON.parse(atob(token.split('.')[1])).role;

    // Se o papel não for 'ADMINISTRADOR', remove os botões com a classe 'admin-only'
    if (userRole !== 'ADMINISTRADOR') {
        adminButtons.forEach(button => {
            button.remove(); // Remove o botão completamente
        });
    } else {
        // Carrega funcionalidades específicas de administrador
        loadAdminFeatures();
    }

    // Caso o papel seja 'CLIENTE', não faz nada com os botões 'admin-only', pois são removidos
    if (userRole === 'CLIENTE') {
        loadClientFeatures();
    }
}

// Função para carregar as funcionalidades de administrador
function loadAdminFeatures() {
    console.log('Carregando funcionalidades de administrador...');
}

// Função para carregar as funcionalidades de cliente
function loadClientFeatures() {
    console.log('Carregando funcionalidades de cliente...');
}

// Função para obter o nome do usuário a partir do token
function getUserNameFromToken() {
    const token = localStorage.getItem('jwtToken');
    if (token) {
        try {
            const payload = JSON.parse(atob(token.split('.')[1]));
            return payload.name || "Usuário";
        } catch (error) {
            console.error('Erro ao decodificar o token:', error);
            return "Usuário";
        }
    }
    return null;
}

// Função para exibir o nome do usuário e o botão de logout
function displayUserInfo() {
    const userName = getUserNameFromToken();
    const userNameElement = document.getElementById('userName');
    const logoutBtn = document.getElementById('logoutBtn');

    if (userName) {
        userNameElement.textContent = userName;

        // Mostra o botão de logout ao clicar no nome
        userNameElement.addEventListener('click', () => {
            logoutBtn.style.display = 'inline-block'; // Mostra o botão de logout
        });
    }
}

// Função para fazer logout
function logout() {
    localStorage.removeItem('jwtToken');
    document.getElementById('logoutBtn').style.display = 'none';
    window.location.href = 'login.html';
}

// Chama a função para exibir as informações do usuário ao carregar a página
displayUserInfo();
