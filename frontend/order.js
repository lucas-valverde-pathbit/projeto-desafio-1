const apiOrderUrl = window.location.hostname === "localhost" 
               ? "http://localhost:5064/api/orders" 
               : "http://api:5064/api/orders"; 

document.getElementById("orderForm").addEventListener("submit", async (e) => {
    e.preventDefault(); // Impede o envio padrão do formulário

    const orderCustomer = document.getElementById("orderCustomer").value;
    const orderUser = document.getElementById("orderUser").value;
    const orderDeliveryAddress = document.getElementById("orderDeliveryAddress").value;

    const orderData = {
        customerId: orderCustomer,
        userId: orderUser,
        deliveryAddress: orderDeliveryAddress,
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
            document.getElementById("orderForm").reset();
        } else {
            alert(`Erro: ${result.message}`);
        }
    } catch (error) {
        alert("Erro ao cadastrar o pedido.");
    }
});
