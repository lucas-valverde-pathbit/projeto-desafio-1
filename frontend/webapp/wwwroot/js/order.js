const apiOrderUrl = window.location.hostname === "localhost"
                   ? "http://localhost:5064/api/orders"
                   : "http://api:5064/api/orders";

// Função para cadastrar um pedido
document.getElementById("orderForm").addEventListener("submit", async (e) => {
    e.preventDefault();

    const orderCustomerId = document.getElementById("orderCustomerId").value;
    const orderUserId = document.getElementById("orderUserId").value;
    const orderDate = document.getElementById("orderDate").value;
    const orderDeliveryAddress = document.getElementById("orderDeliveryAddress").value;

    const orderData = { 
        customerId: orderCustomerId,
        userId: orderUserId,
        orderDate: orderDate,
        deliveryAddress: orderDeliveryAddress
    };

    try {
        const response = await fetch(apiOrderUrl, {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(orderData),
        });

        const result = await response.json();
        if (response.ok) {
            alert("Pedido cadastrado com sucesso!");
            listOrders();  // Atualiza a lista de pedidos
            document.getElementById("orderForm").reset();  // Limpa o formulário
        } else {
            alert(`Erro: ${result.message}`);
        }
    } catch (error) {
        alert("Erro ao cadastrar o pedido.");
    }
});

// Função para listar os pedidos cadastrados
async function listOrders() {
    try {
        const response = await fetch(apiOrderUrl);
        const orders = await response.json();

        if (response.ok) {
            const ordersListDiv = document.getElementById("ordersList");
            ordersListDiv.innerHTML = "";  // Limpa a lista

            if (orders.length === 0) {
                ordersListDiv.innerHTML = "<p>Nenhum pedido cadastrado.</p>";
            } else {
                const listHtml = orders.map(order => {
                    return `
                        <div class="order">
                            <h3>Pedido ID: ${order.id}</h3>
                            <p>Cliente ID: ${order.customerId}</p>
                            <p>Usuário ID: ${order.userId}</p>
                            <p>Data: ${order.orderDate}</p>
                            <p>Endereço de Entrega: ${order.deliveryAddress}</p>
                            <button onclick="editOrder('${order.id}')">Editar</button>
                            <button onclick="deleteOrder('${order.id}')">Excluir</button>
                        </div>
                    `;
                }).join("");

                ordersListDiv.innerHTML = listHtml;  // Exibe os pedidos
            }
        } else {
            alert("Erro ao carregar os pedidos.");
        }
    } catch (error) {
        alert("Erro ao carregar os pedidos.");
    }
}

// Função para editar um pedido
async function editOrder(orderId) {
    try {
        const response = await fetch(`${apiOrderUrl}/${orderId}`);
        const order = await response.json();

        if (response.ok) {
            document.getElementById("orderCustomerId").value = order.customerId;
            document.getElementById("orderUserId").value = order.userId;
            document.getElementById("orderDate").value = order.orderDate;
            document.getElementById("orderDeliveryAddress").value = order.deliveryAddress;
        } else {
            alert("Erro ao carregar os dados do pedido.");
        }
    } catch (error) {
        alert("Erro ao editar o pedido.");
    }
}

// Função para excluir um pedido
async function deleteOrder(orderId) {
    try {
        const response = await fetch(`${apiOrderUrl}/${orderId}`, {
            method: "DELETE"
        });

        if (response.ok) {
            alert("Pedido excluído com sucesso!");
            listOrders();  // Atualiza a lista de pedidos
        } else {
            alert("Erro ao excluir o pedido.");
        }
    } catch (error) {
        alert("Erro ao excluir o pedido.");
    }
}

// Chama a função de listagem quando a página carrega
listOrders();
