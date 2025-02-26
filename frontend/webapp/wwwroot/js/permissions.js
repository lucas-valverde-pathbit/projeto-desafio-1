function checkToken() {
    const token = localStorage.getItem('token');
    if (!token) {
        return false; // Token não encontrado
    }

    // Verificar a validade do token (simplificado)
    const payload = JSON.parse(atob(token.split('.')[1]));
    const expiration = payload.exp * 1000; // Converter para milissegundos
    if (Date.now() > expiration) {
        localStorage.removeItem('token'); // Token expirado
        return false;
    }

    return true; // Token válido
}

export { checkToken };
