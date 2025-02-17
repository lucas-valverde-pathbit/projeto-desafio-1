const apiUserUrl = window.location.hostname === "localhost" 
               ? "http://localhost:5064/api/users" 
               : "http://api:5064/api/users"; 

document.getElementById("userForm").addEventListener("submit", async (e) => {
    e.preventDefault(); // Impede o envio padrão do formulário

    const userEmail = document.getElementById("userEmail").value;
    const userPassword = document.getElementById("userPassword").value;

    const userData = { userEmail, userPassword };

    try {
        const response = await fetch(apiUserUrl, {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(userData),
        });

        const result = await response.json();

        if (response.ok) {
            alert("Usuário cadastrado com sucesso!");
            document.getElementById("userForm").reset();
        } else {
            alert(`Erro: ${result.message}`);
        }
    } catch (error) {
        alert("Erro ao cadastrar o usuário.");
    }
});
