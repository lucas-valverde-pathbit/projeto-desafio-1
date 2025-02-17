// Seleciona todos os links da página
const links = document.querySelectorAll('.button-link');

// Adiciona um efeito visual ao clicar nos botões (pode ser personalizado)
links.forEach(link => {
    link.addEventListener('click', () => {
        // Adiciona uma classe 'active' para o link que foi clicado
        links.forEach(l => l.classList.remove('active'));
        link.classList.add('active');
    });
});
