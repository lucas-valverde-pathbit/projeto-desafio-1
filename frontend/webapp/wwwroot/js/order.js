// Variáveis globais para armazenar dados selecionados
const apiBaseUrl = window.location.hostname === "localhost"
    ? "http://localhost:5064"  // URL de desenvolvimento (localhost)
    : "http://api:5064"; // URL para produção (se necessário)

let selectedCustomer = null;
let selectedProducts = []; // Alterado para armazenar múltiplos produtos
let productCount = 0; // Contador para produtos

// Função para exibir o formulário de adição de pedido
function showAddOrderForm() {
    document.getElementById('addOrderModal').style.display = 'block';
    loadCustomers();
    loadProducts();
    showProductSelection(); // Abre o modal de seleção de produtos automaticamente
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
    newProductField.classList.add('product-field-container'); // Classe para o contêiner de produto
    newProductField.setAttribute('data-product-index', productCount); // Atribui um índice único
    productCount++; // Incrementa o contador

    newProductField.innerHTML = `
        <div class="product-field">
            <label for="productId">Produto:</label>
            <input type="text" class="productId" readonly required>
            <button type="button" class="select-product-btn">+</button>
            <label for="quantity">Quantidade:</label>
            <input type="number" class="quantity" required min="1">
        </div>
    `;

    productFieldsContainer.appendChild(newProductField);

    // Adiciona o evento de clique ao botão de seleção de produto
    newProductField.querySelector('.select-product-btn').addEventListener('click', function() {
        showProductSelection(newProductField); // Passando o contêiner correto
    });
}

// Função para carregar os clientes na lista
function loadCustomers() {
    fetch(`${apiBaseUrl}/api/customers`) // URL correta da API para buscar clientes
        .then(response => response.json())
        .then(data => {
            const customers = data.$values; // Acessando o array de clientes
            console.log('Clientes carregados:', customers); // Log para verificar os dados recebidos
            if (!Array.isArray(customers)) {
                console.error('A resposta não é um array de clientes:', customers);
                return; // Retorna se a resposta não for um array
            }
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

// Função para abrir o modal de seleção de produtos
function showProductSelection(productFieldContainer) {
    const modal = document.getElementById('productSelectionModal');
    if (modal) {
        modal.style.display = 'block';
        loadProducts();

        // Armazena o índice do produto no modal, se o contêiner for fornecido
        if (productFieldContainer) {
            modal.setAttribute('data-target', productFieldContainer.dataset.productIndex);
        }
    } else {
        console.error('Modal de seleção de produtos não encontrado.');
    }
}

// Função para carregar os produtos na lista
function loadProducts() {
    fetch(`${apiBaseUrl}/api/products`) // URL correta da API para buscar produtos
        .then(response => response.json())
        .then(data => {
            const products = data.$values; // Acessando o array de produtos
            console.log('Produtos carregados:', products); // Log para verificar os dados recebidos
            if (!Array.isArray(products)) {
                console.error('A resposta não é um array de produtos:', products);
                return; // Retorna se a resposta não for um array
            }
            const container = document.getElementById('productListContainer');
            container.innerHTML = ''; // Limpa a lista antes de adicionar novos itens

            products.forEach(product => {
                const button = document.createElement('button');
                button.textContent = product.productName;

                // Usar addEventListener para capturar o evento corretamente
                button.addEventListener('click', function(event) {
                    selectProduct(product); // Passa o produto selecionado
                });

                container.appendChild(button);
            });
        })
        .catch(error => console.error('Erro ao carregar produtos:', error));
}

// Função para selecionar um produto
function selectProduct(product) {
    const modal = document.getElementById('productSelectionModal');
    const targetIndex = modal.getAttribute('data-target'); // Obtém o índice do produto a ser atualizado
    const productFieldContainer = document.querySelector(`.product-field-container[data-product-index="${targetIndex}"]`); // Seleciona o contêiner correto

    if (!productFieldContainer) {
        console.error('Contêiner de produto não encontrado.');
        return;
    }

    // Buscando os campos dentro do contêiner de produto
    const productIdField = productFieldContainer.querySelector('.productId');
    const quantityField = productFieldContainer.querySelector('.quantity');

    if (productIdField && quantityField) {
        // Atualizando os campos com o produto selecionado
        productIdField.value = product.productName;  // Nome do produto
        quantityField.value = 1;  // Quantidade padrão
        selectedProduct = product;  // Armazena o produto selecionado para o campo atual

        closeProductSelection();  // Fecha o modal de seleção
    } else {
        console.error('Campos de produto ou quantidade não encontrados.');
    }
}

// Função para fechar o modal de seleção de produtos
function closeProductSelection() {
    document.getElementById('productSelectionModal').style.display = 'none';
}

function fetchAddress() {
    const cep = document.getElementById('cep').value;
    fetch(`${apiBaseUrl}/api/cep/${cep}`)
        .then(response => {
            if (!response.ok) {
                throw new Error('Erro ao buscar endereço');
            }
            return response.json();
        })
        .then(data => {
            // Preenche os campos de endereço com os dados retornados
            document.getElementById('deliveryAddress').value = data.address;
        })
        .catch(error => console.error('Erro:', error));
}

// Função para salvar o pedido

function saveOrder(event) {
    event.preventDefault();

    if (!selectedCustomer) {
        alert('Por favor, selecione todos os campos obrigatórios!');
        return;
    }

    const orderData = {
        order: {
            customerId: selectedCustomer.id, // ID do cliente
            deliveryAddress: document.getElementById('deliveryAddress').value,
            status: 'Pendente', // Definindo valor fixo como 'Pendente'
            orderItems: [] // Itens do pedido
        },
        token: localStorage.getItem('token') // Incluindo o token no payload
    };


    console.log('Dados do pedido:', orderData); // Log para verificar os dados do pedido

    const productFields = document.querySelectorAll('.product-field-container');
    productFields.forEach(field => {
        const productName = field.querySelector('.productId').value;
        const quantity = field.querySelector('.quantity').value;

        if (productName && quantity) {
            console.log(`Adicionando produto: ${productName}, Quantidade: ${quantity}`); // Log para verificar os produtos e quantidades
            // Aqui usamos o `selectedProduct` para obter o ID e o preço do produto
            orderData.order.orderItems.push({
                productId: selectedProduct.id, // ID real do produto selecionado no campo atual
                quantity: parseInt(quantity), // Alterado para número inteiro
                price: selectedProduct.productPrice // Preço do produto
            });

        }
    });

    console.log('Dados do pedido após adição de itens:', orderData); // Log para verificar os dados do pedido após a adição de itens

    fetch(`${apiBaseUrl}/api/orders`, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        },

        body: JSON.stringify(orderData)
    })
    .then(response => {
        if (!response.ok) {
            return response.json().then(err => {
                console.error('Erro ao criar pedido:', err);
                alert('Erro ao criar pedido: ' + JSON.stringify(err.errors)); // Exibir os erros de validação
                throw new Error('Erro ao criar pedido: ' + err.message);
            });
        }
        return response.json();
    })
    .then(newOrder => {
        alert('Pedido criado com sucesso!');
        closeForm();
        loadOrders();
    })
    .catch(error => console.error('Erro ao criar pedido:', error));
}    

// Função para carregar os pedidos
function loadOrders() {
    fetch(`${apiBaseUrl}/api/orders`) // URL correta da API de pedidos
        .then(response => response.json())
        .then(orders => {
            const container = document.getElementById('orderCardContainer');
            if (container) {
                container.innerHTML = ''; // Limpa a lista antes de adicionar novos pedidos

                orders.forEach(order => {
                    const orderCard = createOrderCard(order);
                    container.appendChild(orderCard);
                });
            } else {
                console.error('Contêiner de pedidos não encontrado.');
            }
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

// Função para filtrar pedidos por status
function filterOrders() {
    const statusFilter = document.getElementById('statusFilter').value;

    fetch(`${apiBaseUrl}/api/orders?status=${statusFilter}`)
        .then(response => response.json())
        .then(orders => {
            const container = document.getElementById('orderCardContainer');
            if (container) {
                container.innerHTML = '';

                orders.forEach(order => {
                    const orderCard = createOrderCard(order);
                    container.appendChild(orderCard);
                });
            } else {
                console.error('Contêiner de pedidos não encontrado.');
            }
        })
        .catch(error => console.error('Erro ao carregar pedidos filtrados:', error));
}
