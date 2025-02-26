import { checkToken } from './permissions.js';

window.onload = () => {
    // Verificar se o token está presente
    if (!checkToken()) {
        window.location.href = 'login.html'; // Redirecionar para login.html se o token não estiver presente
    }

    // Verificar periodicamente se o token expirou ou foi removido
    setInterval(() => {
        if (!checkToken()) {
            window.location.href = 'login.html'; // Redirecionar para login.html se o token expirar ou for removido
        }
    }, 30000); // Verificar a cada 30 segundos
};
