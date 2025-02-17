const apiUserUrl = window.location.hostname === "localhost"
                   ? "http://localhost:5064/api/users"
                   : "http://api:5064/api/users";

// Função para cadastrar um usuário
document.getElementById("userForm").addEventListener("submit", async (e) => {
    e.preventDefault();

    const userEmail = document.getElementById("userEmail").value;
    const userPassword = document.getElementById("userPassword").value;
    const userRole = document.getElementById("userRole").value;

    const userData = { userEmail, userPassword, role: userRole };

    try {
        const response = await fetch(apiUserUrl, {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(userData),
        });

        const result = await response.json();
        if (response.ok) {
            alert("Usuário cadastrado com sucesso!");
            listUsers();  // Atualiza a lista de usuários
            document.getElementById("userForm").reset();  // Limpa o formulário
        } else {
            alert(`Erro: ${result.message}`);
        }
    } catch (error) {
        alert("Erro ao cadastrar o usuário.");
    }
});

// Função para listar os usuários cadastrados
async function listUsers() {
    try {
        const response = await fetch(apiUserUrl);
        const users = await response.json();

        if (response.ok) {
            const usersListDiv = document.getElementById("usersList");
            usersListDiv.innerHTML = "";  // Limpa a lista

            if (users.length === 0) {
                usersListDiv.innerHTML = "<p>Nenhum usuário cadastrado.</p>";
            } else {
                const listHtml = users.map(user => {
                    return `
                        <div class="user">
                            <h3>${user.userEmail}</h3>
                            <p>Papel: ${user.role}</p>
                            <button onclick="editUser('${user.id}')">Editar</button>
                            <button onclick="deleteUser('${user.id}')">Excluir</button>
                        </div>
                    `;
                }).join("");

                usersListDiv.innerHTML = listHtml;  // Exibe os usuários
            }
        } else {
            alert("Erro ao carregar os usuários.");
        }
    } catch (error) {
        alert("Erro ao carregar os usuários.");
    }
}

// Função para editar um usuário
async function editUser(userId) {
    try {
        const response = await fetch(`${apiUserUrl}/${userId}`);
        const user = await response.json();

        if (response.ok) {
            document.getElementById("userEmail").value = user.userEmail;
            document.getElementById("userPassword").value = user.userPassword;
            document.getElementById("userRole").value = user.role;
        } else {
            alert("Erro ao carregar os dados do usuário.");
        }
    } catch (error) {
        alert("Erro ao editar o usuário.");
    }
}

// Função para excluir um usuário
async function deleteUser(userId) {
    try {
        const response = await fetch(`${apiUserUrl}/${userId}`, {
            method: "DELETE"
        });

        if (response.ok) {
            alert("Usuário excluído com sucesso!");
            listUsers();  // Atualiza a lista de usuários
        } else {
            alert("Erro ao excluir o usuário.");
        }
    } catch (error) {
        alert("Erro ao excluir o usuário.");
    }
}

// Chama a função de listagem quando a página carrega
listUsers();
