window.addEventListener('DOMContentLoaded', () => {
    const userInfo = JSON.parse(localStorage.getItem('userInfo'));

    if (!userInfo || userInfo.role !== 'ADMINISTRADOR') {
        window.location.href = 'sem-permissao.html';
    }
});

const apiBaseUrl = window.location.hostname === "localhost"
    ? "http://localhost:5064" 
    : "http://api:5064"; 

// Função para carregar os pedidos
function loadOrders() {
    fetch(`${apiBaseUrl}/api/orders`)
        .then(response => response.json())
        .then(response => {
            const container = document.getElementById('ordersList');
            if (container) {
                container.innerHTML = '';
                const orders = response.$values || response;
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

// Função para criar o cartão do pedido
function createOrderCard(order) {
    const card = document.createElement('div');
    card.classList.add('order-card');
    
    const statusMapping = {
        0: "Pendente",
        1: "Enviado",
        2: "Entregue",
        3: "Cancelado"
    };
    const Status = statusMapping[order.status];
    card.innerHTML = `
        <h3>Pedido ID: ${order.id}</h3>
        <p><strong>Data do Pedido:</strong> ${new Date(order.orderDate).toLocaleDateString()}</p>
        <p><strong>Status:</strong> ${Status}</p>
        <p><strong>CEP de Entrega:</strong> ${order.deliveryZipCode}</p>
        <p><strong>Endereço de Entrega:</strong> ${order.deliveryAddress}</p>
        <p><strong>Id Do Cliente:</strong> ${order.customerId}</p>
        <div class="order-items">
            <h4>Itens do Pedido:</h4>
            <ul>
                ${order.orderItems.$values.map(item => {
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

        <div class="order-actions">
            <button onclick="updateOrderStatus('${order.id}', 1)">Marcar como Enviado</button>
            <button onclick="updateOrderStatus('${order.id}', 2)">Marcar como Entregue</button>
            <button onclick="updateOrderStatus('${order.id}', 3)">Cancelar Pedido</button>
        </div>
    `;
    
    return card;
}


async function updateOrderStatus(orderId, status) {
    const statusMapping = {
        1: 'Enviado',
        2: 'Entregue',
        3: 'Cancelado'
    };

    const confirmAction = confirm(`Tem certeza que deseja marcar este pedido como "${statusMapping[status]}"?`);

    if (confirmAction) {
        try {
            const response = await fetch(`${apiBaseUrl}/api/orders/update-status/${orderId}`, {
                method: 'PUT',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify(status) // Passando o status diretamente
            });

            if (!response.ok) {
                throw new Error('Erro ao atualizar o status do pedido');
            }

            // Atualizar a lista de pedidos ou qualquer outro processo necessário
            loadOrders();
        } catch (error) {
            console.error("Erro ao atualizar status:", error);
        }
    }
}


document.addEventListener('DOMContentLoaded', loadOrders);
