// Variáveis globais para armazenar dados selecionados
let selectedCustomer = null;
let selectedProduct = null;

// Função para abrir o modal de seleção de cliente
function showCustomerSelection() {
    document.getElementById('customerSelectionModal').style.display = 'block';
    loadCustomers(); // Carrega os clientes
}

// Função para carregar os clientes na lista
function loadCustomers() {
    fetch('http://localhost:5064/api/customers') // URL correta da API para buscar clientes
        .then(response => response.json())
        .then(customers => {
            const container = document.getElementById('customerListContainer');
            container.innerHTML = ''; // Limpa a lista antes de adicionar novos itens

            customers.forEach(customer => {
                const button = document.createElement('button');
                button.textContent = customer.customerName;
                button.onclick = function() {
                    selectCustomer(customer);
                };
                container.appendChild(button);
            });
        })
        .catch(error => console.error('Erro ao carregar clientes:', error));
}

// Função para selecionar um cliente
function selectCustomer(customer) {
    selectedCustomer = customer;
    document.getElementById('customerId').value = customer.customerName; // Exibe o nome do cliente no campo de entrada
    closeCustomerSelection();
}

// Função para fechar o modal de seleção de cliente
function closeCustomerSelection() {
    document.getElementById('customerSelectionModal').style.display = 'none';
}

// Função para abrir o modal de seleção de produto
function showProductSelection() {
    document.getElementById('productSelectionModal').style.display = 'block';
    loadProducts(); // Carrega os produtos
}

// Função para carregar os produtos na lista
function loadProducts() {
    fetch('http://localhost:5064/api/products') // URL correta da API para buscar produtos
        .then(response => response.json())
        .then(products => {
            const container = document.getElementById('productListContainer');
            container.innerHTML = ''; // Limpa a lista antes de adicionar novos itens

            products.forEach(product => {
                const button = document.createElement('button');
                button.textContent = product.productName;
                button.onclick = function() {
                    selectProduct(product);
                };
                container.appendChild(button);
            });
        })
        .catch(error => console.error('Erro ao carregar produtos:', error));
}

// Função para selecionar um produto
function selectProduct(product) {
    selectedProduct = product;
    document.getElementById('productId').value = product.productName; // Exibe o nome do produto no campo de entrada
    closeProductSelection();
}

// Função para fechar o modal de seleção de produto
function closeProductSelection() {
    document.getElementById('productSelectionModal').style.display = 'none';
}

// Função para buscar o endereço com base no CEP
function fetchAddress() {
    const cep = document.getElementById('cep').value;
    if (cep) {
        fetch(`https://viacep.com.br/ws/${cep}/json/`)
            .then(response => response.json())
            .then(data => {
                if (!data.erro) {
                    document.getElementById('deliveryAddress').value = `${data.logradouro}, ${data.bairro}, ${data.localidade} - ${data.uf}`;
                } else {
                    alert('CEP não encontrado.');
                }
            })
            .catch(error => console.error('Erro ao buscar endereço:', error));
    }
}

// Função para salvar o pedido
function saveOrder(event) {
    event.preventDefault();

    if (!selectedCustomer || !selectedProduct) {
        alert('Por favor, selecione todos os campos obrigatórios!');
        return;
    }

    const orderData = {
        customerId: selectedCustomer.id, // ID do cliente
        deliveryAddress: document.getElementById('deliveryAddress').value,
        status: document.getElementById('status').value,
        products: [
            {
                productId: selectedProduct.id, // ID do produto
                orderItemQuantity: document.getElementById('quantity').value,
                orderItemPrice: selectedProduct.productPrice // Valor do produto
            }
        ]
    };

    fetch('http://localhost:5064/api/orders', { // URL correta da API para criar pedidos
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify(orderData)
    })
    .then(response => response.json())
    .then(newOrder => {
        alert('Pedido criado com sucesso!');
        closeForm(); // Fecha o formulário
        loadOrders(); // Recarrega a lista de pedidos
    })
    .catch(error => console.error('Erro ao criar pedido:', error));
}

// Função para carregar os pedidos
function loadOrders() {
    fetch('http://localhost:5064/api/orders') // URL correta da API de pedidos
        .then(response => response.json())
        .then(orders => {
            const container = document.getElementById('orderCardContainer');
            container.innerHTML = ''; // Limpa a lista antes de adicionar novos pedidos

            orders.forEach(order => {
                const orderCard = createOrderCard(order);
                container.appendChild(orderCard);
            });
        })
        .catch(error => console.error('Erro ao carregar pedidos:', error));
}

// Função para criar um card de pedido
function createOrderCard(order) {
    const card = document.createElement('div');
    card.classList.add('order-card');
    card.innerHTML = `
        <h3>Pedido #${order.id}</h3>
        <p><strong>Cliente:</strong> ${order.customerName}</p>
        <p><strong>Endereço:</strong> ${order.deliveryAddress}</p>
        <p><strong>Status:</strong> ${order.status}</p>
        <div class="actions">
            <button onclick="editOrder('${order.id}')">Editar</button>
            <button onclick="deleteOrder('${order.id}')">Excluir</button>
        </div>
    `;
    return card;
}

// Função para editar um pedido
function editOrder(orderId) {
    alert('Editar pedido: ' + orderId);
}

// Função para excluir um pedido
function deleteOrder(orderId) {
    if (confirm('Tem certeza que deseja excluir este pedido?')) {
        fetch(`http://localhost:5064/api/orders/${orderId}`, { // URL correta de sua API
            method: 'DELETE'
        })
        .then(() => {
            alert('Pedido excluído!');
            loadOrders(); // Atualiza a lista de pedidos
        })
        .catch(error => console.error('Erro ao excluir pedido:', error));
    }
}

// Função para fechar o formulário de criação de pedido
function closeForm() {
    document.getElementById('addOrderModal').style.display = 'none';
}

// Função para abrir o formulário de criação de pedido
function showAddForm() {
    document.getElementById('addOrderModal').style.display = 'block';
    loadCustomers();
    loadProducts();
}
