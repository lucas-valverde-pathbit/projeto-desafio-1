window.addEventListener('DOMContentLoaded', () => {
    const userInfo = JSON.parse(localStorage.getItem('userInfo'));


    if (!userInfo || userInfo.role !== 'ADMINISTRADOR') {
        window.location.href = 'sem-permissao.html';
    }
});