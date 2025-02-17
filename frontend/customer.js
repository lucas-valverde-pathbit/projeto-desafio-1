const apiCustomerUrl = window.location.hostname === "localhost" 
               ? "http://localhost:5064/api/customers" 
               : "http://api:5064/api/customers"; 

document.getElementById("customerForm").addEventListener("submit", async (e) => {
    e.preventDefault();  // Previne o comportamento padrão do formulário de recarregar a página
    console.log("Cadastro de Cliente: Formulário enviado.");

    const customerName = document.getElementById("customerName").value;
    const customerEmail = document.getElementById("customerEmail").value;

    const customerData = { customerName, customerEmail };

    try {
        const response = await fetch(apiCustomerUrl, {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(customerData),
        });

        const result = await response.json();
        if (response.ok) {
            alert("Cliente cadastrado com sucesso!");
            document.getElementById("customerForm").reset();  // Limpa o formulário
        } else {
            alert(`Erro: ${result.message}`);
        }
    } catch (error) {
        alert("Erro ao cadastrar o cliente.");
    }
});
