import { checkPermissions } from './permissions.js';

window.onload = () => {
    // Função para verificar se o usuário está autenticado
    function checkAuth() {
        const { isAuthenticated } = checkPermissions();
        if (!isAuthenticated) {
        window.location.href = 'index.html'; // Redireciona para o index se não houver token

            return;
        }
    }

    // Verifica o token assim que a página for carregada
    checkAuth();
};
