// Verifica se o usuário está autenticado
const token = localStorage.getItem('jwtToken');

if (!token) {
    // Redireciona para a página de login se não houver token
    window.location.href = 'index.html';
} else {
    // Verifica o tipo de usuário
    const userRole = JSON.parse(atob(token.split('.')[1])).role;
    
    // Configura as permissões baseadas no tipo de usuário
    if (userRole === 'ADMIN') {
        // Carrega funcionalidades de administrador
        loadAdminFeatures();
    } else if (userRole === 'CLIENTE') {
        // Carrega funcionalidades de cliente
        loadClientFeatures();
    } else {
        // Redireciona para login se o papel for inválido
        localStorage.removeItem('jwtToken');
        window.location.href = 'index.html';
    }
}

function loadAdminFeatures() {
    // Implementar funcionalidades específicas do administrador
    console.log('Carregando funcionalidades de administrador...');
}

function loadClientFeatures() {
    // Implementar funcionalidades específicas do cliente
    console.log('Carregando funcionalidades de cliente...');
}
