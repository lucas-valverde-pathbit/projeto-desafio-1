const apiBaseUrl = window.location.hostname === "localhost" 
    ? "http://localhost:5064"  // URL de desenvolvimento
    : "http://api:5064"; // URL para produção

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
// Função para submeter o pedido
function submitOrder(event) {
    event.preventDefault();  // Impede o envio padrão do formulário

    // Obtendo as informações do usuário
    const userInfo = JSON.parse(localStorage.getItem('userInfo'));
    const deliveryAddress = document.getElementById('deliveryAddress').value;
    const deliveryZipCode = document.getElementById('cep').value;
    const status = document.getElementById('status').value;

    // Verificando se os campos obrigatórios foram preenchidos
    if (!deliveryAddress || !status) {
        alert('Por favor, preencha todos os campos obrigatórios!');
        return;
    }

    // Acessando os campos de produto
    const productFields = document.querySelectorAll('#orderDetails .product-field');  // Mudando o seletor para os produtos na comanda
    let productSelected = false;
    let orderItems = [];
    let totalAmount = 0;

    // Verificando os campos de produto e coletando os dados
    productFields.forEach(field => {
        const productName = field.querySelector('span:nth-child(1)').textContent;  // Nome do produto (não é input)
        const productPrice = parseFloat(field.querySelector('span:nth-child(2)').textContent.replace('R$ ', '').replace(',', '.'));  // Garantir que o preço está correto
        const quantity = parseInt(field.querySelector('input').value);

        // Verificando se todos os campos foram preenchidos corretamente
        if (productName && productPrice && quantity > 0) {
            productSelected = true;

            // Calculando o total do item e somando no total geral
            const itemTotal = productPrice * quantity;
            totalAmount += itemTotal;

            // Adicionando os itens ao array
            const productId = field.querySelector('input[type="hidden"]').value;  // ID do produto
            orderItems.push({
                productId: productId,  // ID do produto
                productName: productName,
                productPrice: productPrice,
                quantity: quantity
            });
        }
    });

    // Se nenhum produto foi selecionado, exibe um alerta
    if (!productSelected) {
        alert('Por favor, adicione pelo menos um produto!');
        return;
    }

    // Montando os dados do pedido
    const orderData = {
        CustomerId: userInfo.nameId,
        DeliveryAddress: deliveryAddress,
        DeliveryZipCode: deliveryZipCode,
        Status: status,
        TotalAmount: totalAmount,  // Enviando o valor total
        OrderItems: orderItems  // Enviando os itens do pedido
    };

    // Verificando os dados do pedido antes de enviar
    console.log('Dados do pedido a serem enviados:', JSON.stringify(orderData, null, 2));

    // Recuperando o token de autenticação
    const token = localStorage.getItem('token');
    fetch(`${apiBaseUrl}/api/orders`, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'Authorization': `Bearer ${token}`  // Passando o token de autenticação
        },
        body: JSON.stringify(orderData)  // Enviando os dados do pedido
    })
    .then(response => {
        if (!response.ok) {
            return response.json().then(err => { throw new Error(err.title); });
        }
        return response.json();
    })
    .then(newOrder => {
        alert('Pedido criado com sucesso!');
        closeOrderModal();  // Fechar o modal após a criação do pedido
        loadOrders();  // Atualizar a lista de pedidos
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

// Função para carregar os pedidos
function loadOrders() {
    fetch(`${apiBaseUrl}/api/orders`) // URL correta da API de pedidos
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

function createOrderCard(order) {
    const card = document.createElement('div');
    card.classList.add('order-card');
    
    // Exibindo os dados do pedido
    card.innerHTML = ` 
        <h3>Pedido ID: ${order.id}</h3>
        <p><strong>Data do Pedido:</strong> ${new Date(order.orderDate).toLocaleDateString()}</p>
        <p><strong>Status:</strong> ${order.status === 1 ? 'ENVIADO' : 'PENDENTE'}</p>
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

function showOrdersTab() {
    // Remover a classe 'active' das abas e ocultá-las
    document.getElementById('productsTab').classList.remove('active');
    document.getElementById('ordersTab').classList.add('active');

    // Atualizar o estilo dos botões (opcional)
    document.getElementById('ordersTabBtn').classList.add('active');
    document.getElementById('productsTabBtn').classList.remove('active');
}

function showProductsTab() {
    // Remover a classe 'active' das abas e ocultá-las
    document.getElementById('ordersTab').classList.remove('active');
    document.getElementById('productsTab').classList.add('active');

    // Atualizar o estilo dos botões (opcional)
    document.getElementById('productsTabBtn').classList.add('active');
    document.getElementById('ordersTabBtn').classList.remove('active');
}

// Inicializar com a aba de produtos aberta
showProductsTab(); // Exibe a aba de produtos por padrão

document.addEventListener('DOMContentLoaded', loadProducts); 
