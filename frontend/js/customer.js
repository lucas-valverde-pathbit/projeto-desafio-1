const apiCustomerUrl = window.location.hostname === "localhost"
                       ? "http://localhost:5064/api/customers"
                       : "http://api:5064/api/customers";

// Função para cadastrar/atualizar um cliente
document.getElementById("customerForm").addEventListener("submit", async (e) => {
    e.preventDefault();

    const customerId = document.getElementById("customerId").value;
    const customerName = document.getElementById("customerName").value;
    const customerEmail = document.getElementById("customerEmail").value;

    const customerData = { customerName, customerEmail };
    const url = customerId ? `${apiCustomerUrl}/${customerId}` : apiCustomerUrl;
    const method = customerId ? "PUT" : "POST";

    try {
        const response = await fetch(url, {
            method: method,
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(customerData),
        });

        const result = await response.json();
        if (response.ok) {
            alert(`Cliente ${customerId ? 'atualizado' : 'cadastrado'} com sucesso!`);
            resetForm();
            listCustomers();
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
        if (!response.ok) {
            throw new Error(`Erro ao buscar clientes: ${response.statusText}`);
        }
        const customers = await response.json();
        
        // Verifica se o retorno é um array válido
        if (!Array.isArray(customers)) {
            throw new Error('Formato de dados inválido recebido da API');
        }

        
        // Verifica se o retorno é um array válido
        if (!Array.isArray(customers)) {
            throw new Error('Formato de dados inválido recebido da API');
        }

        
        // Verifica se o retorno é um array válido
        if (!Array.isArray(customers)) {
            throw new Error('Formato de dados inválido recebido da API');
        }


        if (response.ok) {
            const customersListDiv = document.getElementById("customersList");
            customersListDiv.innerHTML = "";

            if (customers.length === 0) {
                customersListDiv.innerHTML = "<p>Nenhum cliente cadastrado.</p>";
            } else {
        const listHtml = customers.map(customer => {
            // Verifica e fornece valores padrão para campos indefinidos
            const customerId = customer?.id || 'N/A';
            const customerName = customer?.customerName || 'Nome não informado';
            const customerEmail = customer?.customerEmail || 'Email não informado';
            
            return `
                <div class="customer" data-customer-id="${customerId}">
                    <h3>${customerName}</h3>
                    <p>Email: ${customerEmail}</p>
                    <p>ID: ${customerId}</p>
                    <button class="edit-btn" data-id="${customerId}">Editar</button>
                    <button class="delete-btn" data-id="${customerId}">Excluir</button>
                </div>
            `;
        }).join("");


        // Adiciona event listeners para os botões
        customersListDiv.innerHTML = listHtml;
        document.querySelectorAll('.edit-btn').forEach(btn => {
            btn.addEventListener('click', (e) => {
                const customerId = e.target.getAttribute('data-id');
                if (customerId) {
                    editCustomer(customerId);
                } else {
                    console.error('ID do cliente não encontrado');
                }
            });
        });
        document.querySelectorAll('.delete-btn').forEach(btn => {
            btn.addEventListener('click', (e) => {
                const customerId = e.target.getAttribute('data-id');
                if (customerId) {
                    if (confirm('Tem certeza que deseja excluir este cliente?')) {
                        deleteCustomer(customerId);
                    }
                } else {
                    console.error('ID do cliente não encontrado');
                }
            });
        });



                customersListDiv.innerHTML = listHtml;
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
        if (!customerId) {
            console.error('ID do cliente inválido:', customerId);
            alert('ID do cliente inválido');
            return;
        }
        console.log('Editando cliente com ID:', customerId);

        
        const response = await fetch(`${apiCustomerUrl}/${customerId}`);
        if (!response.ok) {
            throw new Error(`Erro ao buscar cliente: ${response.statusText}`);
        }
        const customer = await response.json();
        if (!customer || !customer.id) {
            throw new Error('Dados do cliente inválidos');
        }


        if (response.ok) {
            document.getElementById("customerId").value = customer.id;
            document.getElementById("customerName").value = customer.customerName;
            document.getElementById("customerEmail").value = customer.customerEmail;
            
            document.getElementById("submitButton").textContent = "Atualizar Cliente";
            document.getElementById("cancelEditButton").style.display = "inline-block";
        } else {
            alert("Erro ao carregar os dados do cliente.");
        }
    } catch (error) {
        alert("Erro ao editar o cliente.");
    }
}

// Função para cancelar edição
document.getElementById("cancelEditButton").addEventListener("click", () => {
    resetForm();
});

// Função para resetar o formulário
function resetForm() {
    document.getElementById("customerForm").reset();
    document.getElementById("customerId").value = "";
    document.getElementById("submitButton").textContent = "Cadastrar Cliente";
    document.getElementById("cancelEditButton").style.display = "none";
}

// Função para excluir um cliente
async function deleteCustomer(customerId) {
    try {
        if (!customerId) {
            console.error('ID do cliente inválido:', customerId);
            alert('ID do cliente inválido');
            return;
        }
        console.log('Excluindo cliente com ID:', customerId);

        
        const response = await fetch(`${apiCustomerUrl}/${customerId}`, {
            method: "DELETE"
        });
        if (!response.ok) {
            throw new Error(`Erro ao excluir cliente: ${response.statusText}`);
        }


        if (response.ok) {
            alert("Cliente excluído com sucesso!");
            listCustomers();
        } else {
            alert("Erro ao excluir o cliente.");
        }
    } catch (error) {
        alert("Erro ao excluir o cliente.");
    }
}

// Chama a função de listagem quando a página carrega
listCustomers();
