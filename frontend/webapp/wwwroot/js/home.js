import { checkToken } from './permissions.js';

window.onload = () => {
    // Verificar se o token está presente
    if (!checkToken()) {
        window.location.href = 'login.html'; // Redirecionar para login.html se o token não estiver presente
    }
};
