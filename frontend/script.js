const apiUrl = "http://api:5064/api/products";  // URL corrigida


// Enviar o formulário para cadastrar um produto
document.getElementById("productForm").addEventListener("submit", async (e) => {
    e.preventDefault();

    const productName = document.getElementById("productName").value;
    const productDescription = document.getElementById("productDescription").value;
    const productPrice = parseFloat(document.getElementById("productPrice").value);
    const productStockQuantity = parseInt(document.getElementById("productStockQuantity").value);

    const ProductData = {
        productName,
        productDescription,
        productPrice,
        productStockQuantity
    };

    try {
        const response = await fetch(apiUrl, {
            method: "POST",
            headers: {
                "Content-Type": "application/json",
            },
            body: JSON.stringify(productData),
        });

        const result = await response.json();

        if (response.ok) {
            alert("Produto cadastrado com sucesso!");
            document.getElementById("productForm").reset();
        } else {
            alert(`Erro: ${result.message}`);
        }
    } catch (error) {
        alert("Erro ao cadastrar o produto. Tente novamente.");
    }
});
