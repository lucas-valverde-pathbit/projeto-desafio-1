const apiBaseUrl = window.location.hostname === "localhost"
    ? "http://localhost:5064/api/orders"  // URL de desenvolvimento (localhost)
    : "http://api:5064/api/orders"; // URL para produção (se necessário)

let orders = [];

async function fetchCustomers() {
    const response = await fetch("http://localhost:5064/api/customers"); // URL para buscar clientes
    if (response.ok) {
        const customers = await response.json();
        const customerListContainer = document.getElementById('customerListContainer');
        customerListContainer.innerHTML = ''; // Limpa a lista anterior

        customers.forEach(customer => {
            const customerItem = document.createElement('div');
            customerItem.innerHTML = `
                <p>${customer.customerName} (ID: ${customer.id})</p>
                <button onclick="selectCustomer('${customer.id}')">Selecionar</button>
            `;
            customerListContainer.appendChild(customerItem);
        });
    } else {
        console.error('Erro ao buscar clientes');
    }
}

function showCustomerSelection() {
    document.getElementById('customerSelectionModal').style.display = 'block';
    fetchCustomers(); // Busca a lista de clientes ao abrir o modal
}

function closeCustomerSelection() {
    document.getElementById('customerSelectionModal').style.display = 'none';
}

function selectCustomer(customerId) {
    const selectedCustomerElement = document.getElementById('selectedCustomer');
    selectedCustomerElement.innerText = `Cliente Selecionado: ${customerId}`; // Atualiza o texto com o ID do cliente selecionado
    closeCustomerSelection(); // Fecha o modal
}

async function displayOrders() {
    const response = await fetch(apiBaseUrl);
    if (response.ok) {
        orders = await response.json();
        renderOrderList(orders);
    } else {
        console.error('Erro ao buscar pedidos');
    }
}

// Função para renderizar a lista de pedidos
function renderOrderList(orders) {
    const orderCardContainer = document.getElementById('orderCardContainer');
    orderCardContainer.innerHTML = '';

    orders.forEach(order => {
        const card = document.createElement('div');
        card.className = 'order-card';
        card.innerHTML = `
            <h3>${order.Id}</h3>
            <p>Detalhes: ${order.DeliveryAddress}</p>
            <p>ID: ${order.Id}</p>
            <p>Data do Pedido: ${order.OrderDate}</p>
            <p>Status: ${order.Status}</p>
            <p>Cliente ID: ${order.CustomerId}</p>
            <p>Usuário ID: ${order.UserId}</p>

            <div class="actions">
                <button onclick="showEditForm('${order.id}')">Editar</button>
            </div>
        `;
        orderCardContainer.appendChild(card);
    });
}

// Função para mostrar o formulário de adicionar pedido
function showAddForm() {
    document.getElementById('addOrderModal').style.display = 'block';
}

// Função para fechar os formulários
function closeForm() {
    document.getElementById('addOrderModal').style.display = 'none';
    document.getElementById('editOrderModal').style.display = 'none';
}

// Função para adicionar pedido (POST)
async function saveOrder(event) {
    event.preventDefault();

    // Obter os valores do formulário
    const orderName = document.getElementById('orderName').value;
    const orderDetails = document.getElementById('orderDetails').value;

    // Enviar dados via POST para o backend
    const response = await fetch(apiBaseUrl, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
            OrderName: orderName,
            OrderDetails: orderDetails
        })
    });

    if (response.ok) {
        const newOrder = await response.json();
        orders.push(newOrder); // Adiciona o novo pedido à lista
        closeForm();
        displayOrders(); // Recarrega a lista de pedidos
    } else {
        console.error('Erro ao adicionar pedido');
    }
}

// Função para mostrar o formulário de edição de pedido (PUT)
function showEditForm(id) {
    const order = orders.find(o => o.id === id);
    
    // Preenche os campos do formulário de edição com os dados do pedido
    document.getElementById('editOrderId').value = order.id;
    document.getElementById('editOrderName').value = order.orderName;
    document.getElementById('editOrderDetails').value = order.orderDetails;

    document.getElementById('editOrderModal').style.display = 'block';
}

// Função para editar pedido (PUT)
async function updateOrder(event) {
    event.preventDefault();

    // Obter os valores do formulário de edição
    const orderId = document.getElementById('editOrderId').value;
    const orderName = document.getElementById('editOrderName').value;
    const orderDetails = document.getElementById('editOrderDetails').value;

    // Enviar dados via PUT para atualizar o pedido no backend
    const response = await fetch(`${apiBaseUrl}/${orderId}`, {
        method: 'PUT',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
            Id: orderId,
            OrderName: orderName,
            OrderDetails: orderDetails
        })
    });

    if (response.ok) {
        const updatedOrder = await response.json();
        orders = orders.map(o => o.id === orderId ? updatedOrder : o);
        closeForm();
        displayOrders();
    } else {
        console.error('Erro ao editar pedido');
    }
}

// Função para deletar pedido (DELETE)
async function deleteOrder() {
    const orderId = document.getElementById('editOrderId').value;

    const response = await fetch(`${apiBaseUrl}/${orderId}`, {
        method: 'DELETE'
    });

    if (response.ok) {
        orders = orders.filter(o => o.id !== orderId); // Remove o pedido da lista
        closeForm();
        displayOrders(); // Recarrega a lista de pedidos
    } else {
        console.error('Erro ao deletar pedido');
    }
}

// Função para filtrar pedidos (com base no nome ou detalhes)
function filterOrders() {
    const filterValue = document.getElementById('filterInput').value.toLowerCase();

    const filteredOrders = orders.filter(order => 
        order.orderName.toLowerCase().includes(filterValue) || 
        order.orderDetails.toLowerCase().includes(filterValue)
    );

    renderOrderList(filteredOrders);
}

// Carregar os pedidos ao inicializar
window.onload = function() {
    displayOrders();
};
