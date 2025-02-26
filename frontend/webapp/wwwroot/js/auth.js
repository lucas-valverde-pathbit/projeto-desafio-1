import { checkToken } from './permissions.js';

function verifyToken() {
    if (!checkToken()) {
        window.location.href = 'login.html'; // Redirecionar para login.html se o token não estiver presente
    }
}

// Verificar o token ao carregar a página
verifyToken();

// Verificar o token periodicamente (a cada 30 segundos)
setInterval(verifyToken, 30000);
