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
    checkApiConnection();
    const signupForm = document.getElementById('signupForm');

    if (signupForm) {
        signupForm.addEventListener('submit', async (e) => {
            e.preventDefault();

            const name = document.getElementById('signupName').value;
            const email = document.getElementById('signupEmail').value;
            const password = document.getElementById('signupPassword').value;
            const role = document.getElementById('signupRole').value;

            const response = await fetch(`${apiBaseUrl}/signup`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({
                    SignupName: name,
                    SignupEmail: email,
                    SignupPassword: password,
                    SignupRole: role === 'ADMINISTRADOR' ? 0 : 1 
                })


            });

            let data;
            try {
                data = await response.json();
            } catch (error) {
                console.error('Erro ao processar resposta:', error);
                alert('Erro ao processar resposta do servidor.');
                return;
            }

            if (response.ok) {
                console.log('Cadastro realizado com sucesso:', data);
                alert(data.message);
                window.location.href = 'login.html';
            } else {
                console.error('Erro no cadastro:', data);
                alert(data.message || 'Erro ao cadastrar usuário. Verifique os dados informados.');
            }

        });
    }
});
