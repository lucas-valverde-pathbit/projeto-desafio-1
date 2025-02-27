// Variáveis globais para armazenar dados selecionados
const apiBaseUrl = window.location.hostname === "localhost"
    ? "http://localhost:5064"  // URL de desenvolvimento (localhost)
    : "http://api:5064"; // URL para produção (se necessário)

let selectedCustomer = null;
let selectedProducts = []; // Alterado para armazenar múltiplos produtos

function showAddForm() {
    document.getElementById('addOrderModal').style.display = 'block';
    loadCustomers();
    loadProducts();
}

// Função para mostrar o modal de seleção de cliente
function showCustomerSelection() {
    const modal = document.getElementById('customerSelectionModal');
    if (modal) {
        modal.style.display = 'block';
        loadCustomers(); // Carrega os clientes
    } else {
        console.error('Modal de seleção de clientes não encontrado.');
    }
}

// Função para adicionar um novo campo de produto
function addProductField() {
    const productFieldsContainer = document.getElementById('productFields');
    
    const newProductField = document.createElement('div');
    newProductField.innerHTML = `
        <label for="productId">Produto:</label>
        <input type="text" class="productId" readonly required>
        <button type="button" onclick="showProductSelection(this)">+</button>
        <label for="quantity">Quantidade:</label>
        <input type="number" class="quantity" required min="1">
    `;
    
    productFieldsContainer.appendChild(newProductField);
}

// Função para carregar os clientes na lista
function loadCustomers() {
    fetch(`${apiBaseUrl}/api/customers`) // URL correta da API para buscar clientes
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
    const modal = document.getElementById('productSelectionModal');
    if (modal) {
        modal.style.display = 'block';
        loadProducts(); // Carrega os produtos
    } else {
        console.error('Modal de seleção de produtos não encontrado.');
    }
}

// Função para carregar os produtos na lista
function loadProducts() {
    fetch(`${apiBaseUrl}/api/products`) // URL correta da API para buscar produtos
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
    const productIdField = event.target.closest('div').querySelector('.productId');
    const quantityField = event.target.closest('div').querySelector('.quantity');
    
    selectedProducts.push(product); // Adiciona o produto à lista de produtos selecionados
    productIdField.value = product.productName; // Exibe o nome do produto no campo de entrada
    closeProductSelection();
}

// Função para fechar o modal de seleção de produto
function closeProductSelection() {
    document.getElementById('productSelectionModal').style.display = 'none';
}

// Função para buscar o endereço com base no CEP
function fetchAddress() {
    const cepInput = document.getElementById('cep');
    const cep = cepInput.value.replace(/\D/g, ''); // Remove caracteres não numéricos

    if (cep) {
        fetch(`${apiBaseUrl}/api/cep/${cep}`)
            .then(response => {
                if (!response.ok) {
                    return response.json().then(errorData => {
                        throw new Error(errorData.error || 'Erro ao buscar endereço');
                    });
                }
                return response.json();
            })
            .then(data => {
                console.log(data); // Log the response for debugging
                if (data && data.address) {
                    document.getElementById('deliveryAddress').value = data.address;
                } else {
                    alert('CEP não encontrado.');
                }
            })
            .catch(error => {
                console.error('Erro ao buscar endereço:', error);
                alert(error.message);
            });

    }
}

// Função para salvar o pedido
function saveOrder(event) {
    event.preventDefault();

    if (!selectedCustomer) {
        alert('Por favor, selecione todos os campos obrigatórios!');
        return;
    }

    const orderData = {
        customerId: selectedCustomer.id, // ID do cliente
        deliveryAddress: document.getElementById('deliveryAddress').value,
        status: document.getElementById('status').value,
        orderItems: [] // Alterado para armazenar os itens do pedido
    };

    const productFields = document.querySelectorAll('#productFields > div');
    productFields.forEach(field => {
        const productId = field.querySelector('input[id="productId"]').value;
        const quantity = field.querySelector('input[type="number"]').value;

        if (productId && quantity) {
            orderData.orderItems.push({
                productId: productId, // ID do produto
                orderItemQuantity: quantity, // Quantidade do produto
                orderItemPrice: selectedProduct.productPrice // Valor do produto
            });
        }
    });

    fetch(`${apiBaseUrl}/api/orders`, { // URL correta da API para criar pedidos
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
    fetch(`${apiBaseUrl}/api/orders`) // URL correta da API de pedidos
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
        fetch(`${apiBaseUrl}/api/orders/${orderId}`, { // URL correta de sua API
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
