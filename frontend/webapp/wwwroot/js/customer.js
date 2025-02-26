const apiBaseUrl = window.location.hostname === "localhost"
    ? "http://localhost:5064/api/customers"  // URL de desenvolvimento (localhost)
    : "http://api:5064/api/customers"; // URL para produção (se necessário)

let customers = [];

// Função para exibir os clientes na tela
async function displayCustomers() {
    const response = await fetch(apiBaseUrl);
    if (response.ok) {
        customers = await response.json();
        renderCustomerList(customers);
    } else {
        console.error('Erro ao buscar clientes');
    }
}

// Função para renderizar a lista de clientes
function renderCustomerList(customers) {
    const customerCardContainer = document.getElementById('customerCardContainer');
    customerCardContainer.innerHTML = '';

    customers.forEach(customer => {
        const card = document.createElement('div');
        card.className = 'customer-card';
        card.innerHTML = `
            <h3>${customer.customerName}</h3>
            <p>Email: ${customer.customerEmail}</p>
            <p>ID: ${customer.id}</p>
            <div class="actions">
                <button onclick="showEditForm('${customer.id}')">...</button>
            </div>
        `;
        customerCardContainer.appendChild(card);
    });
}

// Função para mostrar o formulário de adicionar cliente
function showAddForm() {
    document.getElementById('addCustomerModal').style.display = 'block';
}

// Função para fechar os formulários
function closeForm() {
    document.getElementById('addCustomerModal').style.display = 'none';
    document.getElementById('editCustomerModal').style.display = 'none';
}

// Função para adicionar cliente (POST)
async function saveCustomer(event) {
    event.preventDefault();

    // Obter os valores do formulário
    const customerName = document.getElementById('customerName').value;
    const customerEmail = document.getElementById('customerEmail').value;

    // Enviar dados via POST para o backend
    const response = await fetch(apiBaseUrl, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
            CustomerName: customerName,
            CustomerEmail: customerEmail
        })
    });

    if (response.ok) {
        const newCustomer = await response.json();
        customers.push(newCustomer); // Adiciona o novo cliente à lista
        closeForm();
        displayCustomers(); // Recarrega a lista de clientes
    } else {
        console.error('Erro ao adicionar cliente');
    }
}

// Função para mostrar o formulário de edição de cliente (PUT)
function showEditForm(id) {
    const customer = customers.find(c => c.id === id);
    
    // Preenche os campos do formulário de edição com os dados do cliente
    document.getElementById('editCustomerId').value = customer.id;
    document.getElementById('editCustomerName').value = customer.customerName;
    document.getElementById('editCustomerEmail').value = customer.customerEmail;

    document.getElementById('editCustomerModal').style.display = 'block';
}

// Função para editar cliente (PUT)
async function updateCustomer(event) {
    event.preventDefault();

    // Obter os valores do formulário de edição
    const customerId = document.getElementById('editCustomerId').value;
    const customerName = document.getElementById('editCustomerName').value;
    const customerEmail = document.getElementById('editCustomerEmail').value;

    // Enviar dados via PUT para atualizar o cliente no backend
    const response = await fetch(`${apiBaseUrl}/${customerId}`, {
        method: 'PUT',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
            Id: customerId,
            CustomerName: customerName,
            CustomerEmail: customerEmail
        })
    });

    if (response.ok) {
        const updatedCustomer = await response.json();
        customers = customers.map(customer =>
            customer.id === customerId ? updatedCustomer : customer
        );
        closeForm();
        displayCustomers(); // Recarrega a lista de clientes
    } else {
        console.error('Erro ao editar cliente');
    }
}

// Função para buscar clientes conforme o filtro de nome ou email
function filterCustomers() {
    const filterValue = document.getElementById('filterInput').value.toLowerCase();
    const filteredCustomers = customers.filter(customer =>
        customer.customerName.toLowerCase().includes(filterValue) ||
        customer.customerEmail.toLowerCase().includes(filterValue)
    );
    renderCustomerList(filteredCustomers);
}

// Inicializa a exibição dos clientes ao carregar a página
displayCustomers();
