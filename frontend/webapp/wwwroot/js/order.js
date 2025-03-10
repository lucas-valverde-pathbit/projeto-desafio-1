const apiBaseUrl = window.location.hostname === "localhost"
    ? "http://localhost:5064"  // URL de desenvolvimento (localhost)
    : "http://api:5064"; // URL para produção (se necessário)

let selectedProducts = []; // Alterado para armazenar múltiplos produtos
let productCount = 0; // Contador para produtos

// Função para exibir o formulário de adição de pedido
function showAddOrderForm() {
    document.getElementById('addOrderModal').style.display = 'block';
    loadProducts();
    showProductSelection(); // Abre o modal de seleção de produtos automaticamente
}

// Função para adicionar um campo de produto
function addProductField() {
    const productFieldsContainer = document.getElementById('productFields');
    
    // Criando um novo campo de produto
    const productField = document.createElement('div');
    productField.classList.add('product-field');
    
    // Criando o campo de nome do produto
    const productNameField = document.createElement('input');
    productNameField.classList.add('product-name');
    productNameField.setAttribute('type', 'text');
    productNameField.setAttribute('placeholder', 'Produto');
    productNameField.setAttribute('readonly', true);
    
    // Criando o campo de preço do produto
    const productPriceField = document.createElement('input');
    productPriceField.classList.add('product-price');
    productPriceField.setAttribute('type', 'text');
    productPriceField.setAttribute('placeholder', 'Preço');
    productPriceField.setAttribute('readonly', true);
    
    // Criando o campo de quantidade do produto
    const productQuantityField = document.createElement('input');
    productQuantityField.classList.add('product-quantity');
    productQuantityField.setAttribute('type', 'number');
    productQuantityField.setAttribute('value', 1);
    
    // Criando o campo oculto para armazenar o ID do produto
    const productIdField = document.createElement('input');
    productIdField.classList.add('product-id');
    productIdField.setAttribute('type', 'hidden');  // Campo oculto para o ID
    productIdField.setAttribute('value', '');  // Inicialmente sem valor
    
    // Criando o botão para abrir a lista de produtos
    const productSelectButton = document.createElement('button');
    productSelectButton.textContent = 'Selecionar Produto';
    productSelectButton.setAttribute('type', 'button');
    
    // Adicionando o evento de clique para abrir a lista de produtos
    productSelectButton.addEventListener('click', function() {
        const index = productFieldsContainer.childElementCount - 1;  // Pega o índice correto
        openProductSelectionModal(index);  // Passa o índice para a função
    });
    
    // Adicionando os campos e o botão ao container
    productField.appendChild(productNameField);
    productField.appendChild(productPriceField);
    productField.appendChild(productQuantityField);
    productField.appendChild(productIdField);  // Adiciona o campo de ID
    productField.appendChild(productSelectButton);
    
    // Adicionando o novo campo ao container de campos de produtos
    productFieldsContainer.appendChild(productField);
}



function openProductSelectionModal(targetIndex) {
    const modal = document.getElementById('productSelectionModal');
    modal.setAttribute('data-target', targetIndex);  // Guarda o índice do campo

    // Exibe o modal
    modal.style.display = 'block';

    // Chama a função para carregar os produtos
    fetchProducts();  // Carrega a lista de produtos
}




function fetchProducts() {
    fetch(`${apiBaseUrl}/api/products`)
        .then(response => response.json())
        .then(products => {
            if (Array.isArray(products.$values)) {
                const productListContainer = document.getElementById('productListContainer');
                productListContainer.innerHTML = ''; // Limpa o container antes de adicionar os produtos

                products.$values.forEach(product => {
                    // Criando o elemento do produto
                    const productDiv = document.createElement('div');
                    productDiv.classList.add('product-item');
                    productDiv.textContent = `${product.productName} - R$ ${product.productPrice.toFixed(2)}`;

                    // Criando o botão de selecionar para o produto
                    const selectButton = document.createElement('button');
                    selectButton.textContent = 'Selecionar Produto';
                    
                    // Adicionando o evento de clique no botão de selecionar
                    selectButton.addEventListener('click', () => selectProduct(product));
                    
                    // Adicionando o botão ao item do produto
                    productDiv.appendChild(selectButton);

                    // Adicionando o item do produto ao container
                    productListContainer.appendChild(productDiv);
                });
            } else {
                console.error('A resposta da API não contém a propriedade $values esperada.');
            }
        })
        .catch(error => {
            console.error('Erro ao carregar produtos:', error);
        });
}


function loadProductsToModal(products) {
    const productListContainer = document.getElementById('productListContainer');
    productListContainer.innerHTML = '';  // Limpa a lista antes de adicionar novos produtos

    // Adiciona os produtos ao modal
    products.forEach(product => {
        const productItem = document.createElement('div');
        productItem.classList.add('product-item');
        productItem.innerHTML = `
            <div>
                <strong>${product.productName}</strong><br>
                Descrição: ${product.productDescription}<br>
                Preço: R$ ${product.productPrice.toFixed(2)}  
            </div>
            <button type="button" onclick="selectProduct('${product.id}', '${product.productName}', ${product.productPrice})">Selecionar</button>
        `;
        productListContainer.appendChild(productItem);  // Adiciona o produto à lista de produtos no modal
    });
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



function selectProduct(product) {
    const modal = document.getElementById('productSelectionModal');
    const targetIndex = modal.getAttribute('data-target');  // Obtém o índice do campo selecionado

    const productFields = document.querySelectorAll('#productFields .product-field');
    const targetProductField = productFields[targetIndex];  // Acessa o campo correto pelo índice

    if (targetProductField) {
        console.log(`Selected Product ID: ${product.id}`); // Log o ID do produto
        // Preenche os campos com as informações do produto selecionado
        const productNameField = targetProductField.querySelector('.product-name');
        const productPriceField = targetProductField.querySelector('.product-price');
        const productQuantityField = targetProductField.querySelector('.product-quantity');
        const productIdField = targetProductField.querySelector('.product-id');  // Acessando o campo oculto de ID

        // Preenche os campos com os dados do produto
        productNameField.value = product.productName;
        productPriceField.value = `R$ ${product.productPrice.toFixed(2)}`;
        productQuantityField.value = 1;  // Definindo a quantidade inicial como 1
        
        // Preenche o campo oculto com o ID do produto
        productIdField.value = product.id;  // Atribui o ID correto ao campo oculto

        // Fechar o modal de seleção de produto
        closeProductSelection();
    } else {
        console.error('Campo de produto não encontrado');
    }
}


function closeProductSelection() {
    const modal = document.getElementById('productSelectionModal');
    modal.style.display = 'none';  // Fecha o modal
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
    const productFields = document.querySelectorAll('#productFields .product-field');
    let productSelected = false;
    let orderItems = [];
    let totalAmount = 0;

    // Verificando os campos de produto e coletando os dados
    productFields.forEach(field => {
        const productName = field.querySelector('.product-name').value;
        const productPrice = parseFloat(field.querySelector('.product-price').value.replace('R$ ', '').replace(',', '.')); // Garantir que o preço está correto
        const quantity = parseInt(field.querySelector('.product-quantity').value);

        // Verificando se todos os campos foram preenchidos corretamente
        if (productName && productPrice && quantity > 0) {
            productSelected = true;

            // Calculando o total do item e somando no total geral
            const itemTotal = productPrice * quantity;
            totalAmount += itemTotal;

            // Adicionando os itens ao array
            const productId = field.querySelector('.product-id').value;
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
        CustomerId: userInfo.customerId,
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
        closeForm();  // Fechar o formulário após a criação
        loadOrders();  // Atualizar a lista de pedidos
    })
    .catch(error => {
        console.error('Erro ao criar pedido:', error);
        alert('Erro ao criar pedido: ' + error.message);
    });
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
// function filterOrders() {
//     const statusFilter = document.getElementById('statusFilter').value;

//     fetch(`${apiBaseUrl}/api/orders?status=${statusFilter}`)
//         .then(response => response.json())
//         .then(orders => {
//             const container = document.getElementById('orderCardContainer');
//             if (container) {
//                 container.innerHTML = '';

//                 orders.forEach(order => {
//                     const orderCard = createOrderCard(order);
//                     container.appendChild(orderCard);
//                 });
//             } else {
//             }
//         })
//         .catch(error => console.error('Erro ao carregar pedidos filtrados:', error));
// }
