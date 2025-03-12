const apiBaseUrl = window.location.hostname === "localhost"
    ? "http://localhost:5064"  // URL de desenvolvimento
    : "http://api:5064"; // URL para produção

    getCustomerByEmail();

    function filterProducts() {
        const filter = document.getElementById("filterInput").value.toLowerCase();
        const productCards = document.getElementsByClassName("product-item");

        Array.from(productCards).forEach(card => {
            const productName = card.querySelector(".product-name").innerText.toLowerCase();
            const productDescription = card.querySelector(".product-description").innerText.toLowerCase();

            if (productName.includes(filter) || productDescription.includes(filter)) {
                card.style.display = "";
            } else {
                card.style.display = "none";
            }
        });
    }

    // Função para carregar a lista de produtos (requisição GET)
    function loadProducts() {
        fetch(`${apiBaseUrl}/api/products`) // URL correta da API de produtos
            .then(response => response.json())
            .then(response => {
                const container = document.getElementById('productsList');
                if (container) {
                    container.innerHTML = ''; // Limpa a lista antes de adicionar novos produtos

                    // Verifica se a resposta tem a estrutura esperada
                    const products = response.$values || response; // Verifique a estrutura da resposta
                    if (Array.isArray(products)) {
                        products.forEach(product => {
                            const productCard = createProductCard(product);
                            container.appendChild(productCard);
                        });
                    } else {
                        console.error('A resposta da API não contém uma lista de produtos:', response);
                    }
                } else {
                    console.error('Contêiner de produtos não encontrado.');
                }
            })
            .catch(error => console.error('Erro ao carregar produtos:', error));
    }

    function createProductCard(product) {
        const card = document.createElement('div');
        card.classList.add('product-item');

        // Exibindo os dados do produto
        card.innerHTML = `
            <h3 class="product-name">${product.productName}</h3>
            <p class="product-description"><strong>Descrição:</strong> ${product.productDescription || 'Descrição não disponível'}</p>
            <p><strong>Preço:</strong> R$ ${product.productPrice.toFixed(2)}</p>
            <div class="actions">
                <button onclick="addProductToComanda('${product.id}', '${product.productName}', ${product.productPrice})">Adicionar à Comanda</button>
            </div>
        `;

        return card;
    }

    function addProductToComanda(productId, productName, productPrice) {
        const orderDetailsContainer = document.getElementById('orderDetails');

        // Criando o campo do produto na comanda
        const productField = document.createElement('div');
        productField.classList.add('product-field');

        const productNameField = document.createElement('span');
        productNameField.textContent = productName;

        const productPriceField = document.createElement('span');
        productPriceField.textContent = `R$ ${productPrice.toFixed(2)}`;

        const productQuantityField = document.createElement('input');
        productQuantityField.setAttribute('type', 'number');
        productQuantityField.setAttribute('value', 1);
        productQuantityField.addEventListener('change', updateTotalAmount);

        const productIdField = document.createElement('input');
        productIdField.setAttribute('type', 'hidden');
        productIdField.setAttribute('value', productId);

        const removeButton = document.createElement('button');
        removeButton.textContent = 'Remover';
        removeButton.addEventListener('click', function () {
            productField.remove();
            updateTotalAmount();
        });

        productField.appendChild(productNameField);
        productField.appendChild(productPriceField);
        productField.appendChild(productQuantityField);
        productField.appendChild(productIdField);
        productField.appendChild(removeButton);

        orderDetailsContainer.appendChild(productField);

        updateTotalAmount();
    }
    // Função para atualizar o total da comanda
function updateTotalAmount() {
    const orderDetailsContainer = document.getElementById('orderDetails');
    let total = 0;

    const productFields = orderDetailsContainer.getElementsByClassName('product-field');
    Array.from(productFields).forEach(productField => {
        const price = parseFloat(productField.querySelector('span:nth-child(2)').textContent.replace('R$', '').trim());
        const quantity = productField.querySelector('input').value;
        total += price * quantity;
    });

    const totalAmount = document.getElementById('totalAmount');
    if (!totalAmount) {
        const totalElement = document.createElement('div');
        totalElement.id = 'totalAmount';
        totalElement.textContent = `Total: R$ ${total.toFixed(2)}`;
        document.getElementById('orderSummary').appendChild(totalElement);
    } else {
        totalAmount.textContent = `Total: R$ ${total.toFixed(2)}`;
    }
}

// Função para abrir o modal de finalização de pedido
function openOrderModal() {
    const modal = document.getElementById('orderModal');
    modal.style.display = 'flex';  // Exibe o modal
}

// Função para fechar o modal
function closeOrderModal() {
    const modal = document.getElementById('orderModal');
    modal.style.display = 'none';  // Esconde o modal
}

// Função para submeter o pedido

function submitOrder(event) {
    event.preventDefault();  // Impede o envio padrão do formulário

    // Obtendo as informações do usuário
    const userInfo = JSON.parse(localStorage.getItem('userInfo'));
    const deliveryAddress = document.getElementById('deliveryAddress').value;
    const deliveryZipCode = document.getElementById('cep').value;
    const status = document.getElementById('status').value;

    // Mapeando os status para o formato esperado pelo backend
    const statusMapping = {
        "Pendente": 0,
        "Enviado": 1,
        "Entregue": 2,
        "Cancelado": 3
    };

    // Verifica se o status é válido
    if (!(status in statusMapping)) {
        alert('Status inválido! Por favor, selecione um status válido.');
        return;
    }

    // Mapeando o status para o valor numérico do enum
    const orderStatus = statusMapping[status];

    // Verificando se os campos obrigatórios foram preenchidos
    if (!deliveryAddress || !status) {
        alert('Por favor, preencha todos os campos obrigatórios!');
        return;
    }

    // Acessando os campos de produto
    const productFields = document.querySelectorAll('#orderDetails .product-field');
    let productSelected = false;
    let orderItems = [];
    let totalAmount = 0;

    // Verificando os campos de produto e coletando os dados
    productFields.forEach(field => {
        const productName = field.querySelector('span:nth-child(1)').textContent;
        const productPrice = parseFloat(field.querySelector('span:nth-child(2)').textContent.replace('R$ ', '').replace(',', '.'));
        const quantity = parseInt(field.querySelector('input').value);

        if (productName && productPrice && quantity > 0) {
            productSelected = true;

            const itemTotal = productPrice * quantity;
            totalAmount += itemTotal;

            const productId = field.querySelector('input[type="hidden"]').value;
            orderItems.push({
                productId: productId,
                productName: productName,
                productPrice: productPrice,
                quantity: quantity
            });
        }
    });

    if (!productSelected) {
        alert('Por favor, adicione pelo menos um produto!');
        return;
    }

    const orderData = {
        CustomerId: userInfo.nameId,
        DeliveryAddress: deliveryAddress,
        DeliveryZipCode: deliveryZipCode,
        Status: orderStatus,  // Passando o status mapeado
        TotalAmount: totalAmount,
        OrderItems: orderItems
    };

    // Envio do pedido
    const token = localStorage.getItem('token');
    fetch(`${apiBaseUrl}/api/orders`, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'Authorization': `Bearer ${token}`
        },
        body: JSON.stringify(orderData)
    })
    .then(response => {
        if (!response.ok) {
            return response.json().then(err => { throw new Error(err.title); });
        }
        return response.json();
    })
    .then(newOrder => {
        alert('Pedido criado com sucesso!');
        closeOrderModal();
        loadOrders();
    })
    .catch(error => {
        console.error('Erro ao criar pedido:', error);
        alert('Erro ao criar pedido: ' + error.message);
    });
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
const statusMapping = {
    0: "Pendente",
    1: "Enviado",
    2: "Entregue",
    3: "Cancelado"
};

// Função para buscar o cliente pelo e-mail armazenado no localStorage e salvar o CustomerId
function getCustomerByEmail() {
    // Recupera o e-mail do localStorage a partir do objeto userInfo
    const userInfo = JSON.parse(localStorage.getItem('userInfo'));

    if (!userInfo || !userInfo.email) {
        console.error('E-mail do usuário não encontrado no localStorage.');
        return;
    }

    const email = userInfo.email;

    // Fazendo a requisição para buscar o cliente pelo email
    fetch(`${apiBaseUrl}/api/customers/email/${email}`)
        .then(response => response.json())
        .then(response => {
            // Verifica se o cliente foi encontrado
            if (response && response.id) {  // Verifica se a resposta contém o id do cliente
                const customerId = response.id;  // Agora estamos acessando o id diretamente

                // Salvando o CustomerId no localStorage
                localStorage.setItem('customerId', customerId);
                console.log('CustomerId encontrado:', customerId);

            } else {
                console.error('Cliente não encontrado!');
            }
        })
        .catch(error => {
            console.error('Erro ao buscar o cliente:', error);
        });
}

function loadOrders() {
    const customerId = localStorage.getItem('customerId');

    if (!customerId) {
        console.error('CustomerId não encontrado. O usuário precisa estar logado.');
        return;
    }

  
    fetch(`${apiBaseUrl}/api/orders/customer/${customerId}`) // Adicionando o filtro de CustomerId
        .then(response => response.json())
        .then(response => {
            const container = document.getElementById('ordersList');
            if (container) {
                container.innerHTML = ''; // Limpa a lista antes de adicionar novos pedidos

                // Verifica se a resposta tem a estrutura esperada
                const orders = response.$values || response; // Verifique a estrutura da resposta
                if (Array.isArray(orders)) {
                    orders.forEach(order => {
                        const orderCard = createOrderCard(order);
                        container.appendChild(orderCard);
                    });
                } else {
                    console.error('A resposta da API não contém uma lista de pedidos:', response);
                }
            } else {
                console.error('Contêiner de pedidos não encontrado.');
            }
        })
        .catch(error => console.error('Erro ao carregar pedidos:', error));
}


// Função para criar o cartão de cada pedido
function createOrderCard(order) {
    const card = document.createElement('div');
    card.classList.add('order-card');

    // Convertendo o status numérico para o valor legível
    const orderStatus = statusMapping[order.status] || "Status desconhecido";  // Caso o status não seja encontrado

    // Exibindo os dados do pedido
    card.innerHTML = `
        <h3>Pedido ID: ${order.id}</h3>
        <p><strong>Data do Pedido:</strong> ${new Date(order.orderDate).toLocaleDateString()}</p>
        <p><strong>Status:</strong> ${orderStatus}</p>  <!-- Status convertido -->
        <p><strong>CEP de Entrega:</strong> ${order.deliveryZipCode}</p>
        <p><strong>Endereço de Entrega:</strong> ${order.deliveryAddress}</p>

        <div class="order-items">
            <h4>Itens do Pedido:</h4>
            <ul>
                ${order.orderItems.$values.map(item => {
                    // Agora os dados são diretamente acessados do item
                    const productName = item.productName || 'Produto não disponível';
                    const productDescription = item.productDescription || 'Descrição não disponível';
                    const productPrice = item.productPrice || 0;
                    const itemTotal = item.quantity * productPrice;

                    return `
                        <li>
                            <strong>ID do Produto:</strong> ${item.productId} <br>
                            <strong>Nome:</strong> ${productName} <br>
                            <strong>Descrição:</strong> ${productDescription} <br>
                            <strong>Quantidade:</strong> ${item.quantity} <br>
                            <strong>Preço Unitário:</strong> R$ ${productPrice.toFixed(2)} <br>
                            <strong>Preço Total:</strong> R$ ${itemTotal.toFixed(2)}
                        </li>
                    `;
                }).join('')}
            </ul>
        </div>

        <div class="order-summary">
            <p><strong>Total do Pedido:</strong> R$ ${order.totalAmount.toFixed(2)}</p>
        </div>

        <div class="actions">
            <button onclick="editOrder('${order.id}')">Editar</button>
            <button onclick="deleteOrder('${order.id}')">Excluir</button>
        </div>
    `;

    return card;
}

function createOrderCard(order) {
    const card = document.createElement('div');
    card.classList.add('order-card');

    // Exibindo os dados do pedido
    card.innerHTML = `
        <h3>Pedido ID: ${order.id}</h3>
        <p><strong>Data do Pedido:</strong> ${new Date(order.orderDate).toLocaleDateString()}</p>
        <p><strong>Status:</strong> ${order.status}</p>
        <p><strong>CEP de Entrega:</strong> ${order.deliveryZipCode}</p>
        <p><strong>Endereço de Entrega:</strong> ${order.deliveryAddress}</p>

        <div class="order-items">
            <h4>Itens do Pedido:</h4>
            <ul>
                ${order.orderItems.$values.map(item => {
                    // Agora os dados são diretamente acessados do item
                    const productName = item.productName || 'Produto não disponível';
                    const productDescription = item.productDescription || 'Descrição não disponível';
                    const productPrice = item.productPrice || 0;
                    const itemTotal = item.quantity * productPrice;

                    return `
                        <li>
                            <strong>ID do Produto:</strong> ${item.productId} <br>
                            <strong>Nome:</strong> ${productName} <br>
                            <strong>Descrição:</strong> ${productDescription} <br>
                            <strong>Quantidade:</strong> ${item.quantity} <br>
                            <strong>Preço Unitário:</strong> R$ ${productPrice.toFixed(2)} <br>
                            <strong>Preço Total:</strong> R$ ${itemTotal.toFixed(2)}
                        </li>
                    `;
                }).join('')}
            </ul>
        </div>

        <div class="order-summary">
            <p><strong>Total do Pedido:</strong> R$ ${order.totalAmount.toFixed(2)}</p>
        </div>

        <div class="actions">
            <button onclick="editOrder('${order.id}')">Editar</button>
            <button onclick="deleteOrder('${order.id}')">Excluir</button>
        </div>
    `;

    return card;
}


function submitProfileEdit(event) {
    event.preventDefault();
    
    const name = document.getElementById('name').value;
    const email = document.getElementById('email').value;
    const currentPassword = document.getElementById('currentPassword').value;
    const newPassword = document.getElementById('newPassword').value;

    const requestData = {
        name: name,
        email: email,
        currentPassword: currentPassword,
        newPassword: newPassword
    };
    const token = localStorage.getItem('token');
    fetch(`${apiBaseUrl}/api/users/edit-profile`, {
        method: 'PUT',
        headers: {
            'Content-Type': 'application/json',
            'Authorization': `Bearer ${token}`
        },
        body: JSON.stringify(requestData)
    })
    .then(response => response.json())
    .then(data => {
        if (data.message === "Perfil atualizado com sucesso.") {
            alert('Perfil atualizado!');
        } else {
            alert('Erro ao atualizar o perfil.');
        }
    })
    .catch(error => console.error('Erro:', error));
}

function showOrdersTab() {
    // Remover a classe 'active' das abas e ocultá-las
    document.getElementById('productsTab').classList.remove('active');
    document.getElementById('ordersTab').classList.add('active');
    document.getElementById('profileTab').classList.remove('active');

    document.getElementById('productsTabBtn').classList.remove('active');
    document.getElementById('ordersTabBtn').classList.add('active');
    document.getElementById('profileTabBtn').classList.remove('active');
}

function showProductsTab() {
    // Remover a classe 'active' das abas e ocultá-las
    document.getElementById('ordersTab').classList.remove('active');
    document.getElementById('productsTab').classList.add('active');
    document.getElementById('profileTab').classList.remove('active');
    // Atualizar o estilo dos botões (opcional)
    document.getElementById('productsTabBtn').classList.add('active');
    document.getElementById('ordersTabBtn').classList.remove('active');
    document.getElementById('profileTabBtn').classList.remove('active');
}
function showProfileTab() {
    // Remover a classe 'active' das abas e ocultá-las
    document.getElementById('ordersTab').classList.remove('active');
    document.getElementById('productsTab').classList.remove('active');
    document.getElementById('profileTab').classList.add('active');
    // Atualizar o estilo dos botões (opcional)
    document.getElementById('productsTabBtn').classList.remove('active');
    document.getElementById('ordersTabBtn').classList.remove('active');
    document.getElementById('profileTabBtn').classList.add('active');
}



showProductsTab();

document.addEventListener('DOMContentLoaded', loadProducts);
document.addEventListener('DOMContentLoaded', loadOrders);