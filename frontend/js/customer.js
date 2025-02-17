const apiCustomerUrl = window.location.hostname === "localhost"
                       ? "http://localhost:5064/api/customers"
                       : "http://api:5064/api/customers";

// Função para cadastrar um cliente
document.getElementById("customerForm").addEventListener("submit", async (e) => {
    e.preventDefault();

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
            listCustomers();  // Atualiza a lista de clientes
            document.getElementById("customerForm").reset();  // Limpa o formulário
        } else {
            alert(`Erro: ${result.message}`);
        }
    } catch (error) {
        alert("Erro ao cadastrar o cliente.");
    }
});

// Função para listar os clientes cadastrados
async function listCustomers() {
    try {
        const response = await fetch(apiCustomerUrl);
        const customers = await response.json();

        if (response.ok) {
            const customersListDiv = document.getElementById("customersList");
            customersListDiv.innerHTML = "";  // Limpa a lista

            if (customers.length === 0) {
                customersListDiv.innerHTML = "<p>Nenhum cliente cadastrado.</p>";
            } else {
                const listHtml = customers.map(customer => {
                    return `
                        <div class="customer">
                            <h3>${customer.customerName}</h3>
                            <p>Email: ${customer.customerEmail}</p>
                            <button onclick="editCustomer('${customer.id}')">Editar</button>
                            <button onclick="deleteCustomer('${customer.id}')">Excluir</button>
                        </div>
                    `;
                }).join("");

                customersListDiv.innerHTML = listHtml;  // Exibe os clientes
            }
        } else {
            alert("Erro ao carregar os clientes.");
        }
    } catch (error) {
        alert("Erro ao carregar os clientes.");
    }
}

// Função para editar um cliente
async function editCustomer(customerId) {
    try {
        const response = await fetch(`${apiCustomerUrl}/${customerId}`);
        const customer = await response.json();

        if (response.ok) {
            document.getElementById("customerName").value = customer.customerName;
            document.getElementById("customerEmail").value = customer.customerEmail;
        } else {
            alert("Erro ao carregar os dados do cliente.");
        }
    } catch (error) {
        alert("Erro ao editar o cliente.");
    }
}

// Função para excluir um cliente
async function deleteCustomer(customerId) {
    try {
        const response = await fetch(`${apiCustomerUrl}/${customerId}`, {
            method: "DELETE"
        });

        if (response.ok) {
            alert("Cliente excluído com sucesso!");
            listCustomers();  // Atualiza a lista de clientes
        } else {
            alert("Erro ao excluir o cliente.");
        }
    } catch (error) {
        alert("Erro ao excluir o cliente.");
    }
}

// Chama a função de listagem quando a página carrega
listCustomers();
