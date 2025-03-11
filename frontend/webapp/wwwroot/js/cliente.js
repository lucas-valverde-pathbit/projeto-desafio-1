const apiBaseUrl = window.location.hostname === "localhost" 
    ? "http://localhost:5064"  // URL de desenvolvimento
    : "http://api:5064"; // URL para produção

// Função para mostrar a tab de pedidos
function showOrdersTab() {
    document.getElementById('ordersTab').style.display = 'block';
    document.getElementById('productsTab').style.display = 'none';
    loadOrders();  // Carregar pedidos
}

// Função para mostrar a tab de produtos
function showProductsTab() {
    document.getElementById('productsTab').style.display = 'block';
    document.getElementById('ordersTab').style.display = 'none';
    loadProducts();  // Carregar produtos
}

// Função para carregar produtos na lista
function loadProducts() {
    fetch(`${apiBaseUrl}/api/products`)
        .then(response => response.json())
        .then(products => {
            const productListContainer = document.getElementById('productListContainer');
            productListContainer.innerHTML = '';  // Limpar a lista

            products.forEach(product => {
                const productDiv = document.createElement('div');
                productDiv.classList.add('product-item');
                productDiv.innerHTML = `
                    <strong>${product.productName}</strong> - R$ ${product.productPrice.toFixed(2)}
                    <button onclick="addProductToOrder('${product.id}', '${product.productName}', ${product.productPrice})">Adicionar</button>
                `;
                productListContainer.appendChild(productDiv);
            });
        })
        .catch(error => console.error('Erro ao carregar produtos:', error));
}

// Função para adicionar o produto ao pedido
function addProductToOrder(productId, productName, productPrice) {
    const productFieldsContainer = document.getElementById('productFields');
    
    const productField = document.createElement('div');
    productField.classList.add('product-field');
    
    productField.innerHTML = `
        <input type="text" value="${productName}" readonly>
        <input type="number" value="1" class="product-quantity">
        <input type="text" value="R$ ${productPrice.toFixed(2)}" readonly>
        <input type="hidden" value="${productId}" class="product-id">
        <button type="button" onclick="removeProductFromOrder(this)">Remover</button>
    `;
    
    productFieldsContainer.appendChild(productField);
}

// Função para remover um produto do pedido
function removeProductFromOrder(button) {
    const productField = button.closest('.product-field');
    productField.remove();
}

// Função para salvar o pedido
function saveOrder(event) {
    event.preventDefault();

    const userInfo = JSON.parse(localStorage.getItem('userInfo'));
    const deliveryAddress = document.getElementById('deliveryAddress').value;
    const deliveryZipCode = document.getElementById('cep').value;
    const status = document.getElementById('status').value;

    // Verificar se os campos obrigatórios estão preenchidos
    if (!deliveryAddress || !status) {
        alert('Por favor, preencha todos os campos obrigatórios!');
        return;
    }

    // Coletar os produtos do pedido
    const productFields = document.querySelectorAll('#productFields .product-field');
    let orderItems = [];
    let totalAmount = 0;

    productFields.forEach(field => {
        const productId = field.querySelector('.product-id').value;
        const productName = field.querySelector('input[type="text"]').value;
        const productPrice = parseFloat(field.querySelector('input[type="text"]:nth-child(3)').value.replace('R$ ', '').replace(',', '.'));
        const quantity = parseInt(field.querySelector('.product-quantity').value);

        if (productName && productPrice && quantity > 0) {
            const itemTotal = productPrice * quantity;
            totalAmount += itemTotal;

            orderItems.push({
                productId: productId,
                productName: productName,
                productPrice: productPrice,
                quantity: quantity
            });
        }
    });

    if (orderItems.length === 0) {
        alert('Por favor, adicione ao menos um produto ao pedido!');
        return;
    }

    // Montar os dados do pedido
    const orderData = {
        CustomerId: userInfo.customerId,
        DeliveryAddress: deliveryAddress,
        DeliveryZipCode: deliveryZipCode,
        Status: status,
        TotalAmount: totalAmount,
        OrderItems: orderItems
    };

    // Enviar dados para o backend
    const token = localStorage.getItem('token');
    fetch(`${apiBaseUrl}/api/orders`, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'Authorization': `Bearer ${token}`
        },
        body: JSON.stringify(orderData)
    })
    .then(response => response.json())
    .then(newOrder => {
        alert('Pedido criado com sucesso!');
        document.getElementById('addOrderModal').style.display = 'none';  // Fechar modal
        loadOrders();  // Recarregar pedidos
    })
    .catch(error => console.error('Erro ao criar pedido:', error));
}

// Função para carregar pedidos
function loadOrders() {
    fetch(`${apiBaseUrl}/api/orders`)
        .then(response => response.json())
        .then(orders => {
            const container = document.getElementById('orderCardContainer');
            container.innerHTML = ''; // Limpar a lista

            orders.forEach(order => {
                const orderCard = document.createElement('div');
                orderCard.classList.add('order-card');
                orderCard.innerHTML = `
                    <h3>Pedido ID: ${order.id}</h3>
                    <p><strong>Data:</strong> ${new Date(order.orderDate).toLocaleDateString()}</p>
                    <p><strong>Status:</strong> ${order.status}</p>
                    <p><strong>Total:</strong> R$ ${order.totalAmount.toFixed(2)}</p>
                    <button onclick="viewOrderDetails(${order.id})">Ver Detalhes</button>
                `;
                container.appendChild(orderCard);
            });
        })
        .catch(error => console.error('Erro ao carregar pedidos:', error));
}


document.addEventListener("DOMContentLoaded", () => {
    const profileSection = document.getElementById('profileSection');
    const ordersSection = document.getElementById('ordersSection');

    // Mostrar perfil do cliente
    function showProfile() {
        profileSection.style.display = 'block';
        ordersSection.style.display = 'none';
        loadProfile();
    }

    // Mostrar pedidos do cliente
    function showOrders() {
        ordersSection.style.display = 'block';
        profileSection.style.display = 'none';
        loadOrders();
    }

    // Carregar perfil do cliente
    function loadProfile() {
        const userId = 123; // Substitua com o ID real do cliente logado
        fetch(`/api/customers/${userId}`)
            .then(response => response.json())
            .then(data => {
                document.getElementById('name').value = data.name;
                document.getElementById('email').value = data.email;
            })
            .catch(error => console.error("Erro ao carregar perfil:", error));
    }

    // Atualizar perfil do cliente
    function updateProfile(event) {
        event.preventDefault();
        const name = document.getElementById('name').value;
        const userId = 123; // Substitua com o ID real do cliente logado

        fetch(`/api/customers/${userId}`, {
            method: 'PUT',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({ name })
        })
        .then(response => response.json())
        .then(data => {
            alert("Perfil atualizado com sucesso!");
        })
        .catch(error => console.error("Erro ao atualizar perfil:", error));
    }

    // Carregar pedidos do cliente
    function loadOrders() {
        const userId = 123; // Substitua com o ID real do cliente logado
        fetch(`/api/orders?customerId=${userId}`)
            .then(response => response.json())
            .then(data => {
                const ordersList = document.getElementById('ordersList');
                ordersList.innerHTML = ''; // Limpar lista antes de carregar

                data.forEach(order => {
                    const orderItem = document.createElement('div');
                    orderItem.innerHTML = `
                        <p>ID: ${order.id} - Status: ${order.status}</p>
                        <button onclick="deleteOrder(${order.id})" ${order.status !== 'Pendente' ? 'disabled' : ''}>Cancelar Pedido</button>
                    `;
                    ordersList.appendChild(orderItem);
                });
            })
            .catch(error => console.error("Erro ao carregar pedidos:", error));
    }

    // Excluir pedido (somente se estiver com status 'Pendente')
    function deleteOrder(orderId) {
        const userId = 123; // Substitua com o ID real do cliente logado
        fetch(`/api/orders/${orderId}`, {
            method: 'DELETE'
        })
        .then(response => {
            if (response.ok) {
                alert('Pedido cancelado com sucesso!');
                loadOrders(); // Recarregar lista de pedidos
            }
        })
        .catch(error => console.error("Erro ao excluir pedido:", error));
    }

    // Carregar perfil por padrão
    showProfile();
});
