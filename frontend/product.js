const apiProductUrl = window.location.hostname === "localhost" 
               ? "http://localhost:5064/api/products" 
               : "http://api:5064/api/products"; 

document.getElementById("productForm").addEventListener("submit", async (e) => {
    e.preventDefault(); // Impede o envio padrão do formulário

    const productName = document.getElementById("productName").value;
    const productDescription = document.getElementById("productDescription").value;
    const productPrice = parseFloat(document.getElementById("productPrice").value);
    const productStockQuantity = parseInt(document.getElementById("productStockQuantity").value);

    const productData = {
        productName,
        productDescription,
        productPrice,
        productStockQuantity
    };

    try {
        const response = await fetch(apiProductUrl, {
            method: "POST",
            headers: { "Content-Type": "application/json" },
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

// Função para mostrar os produtos cadastrados
document.getElementById("showProductsBtn").addEventListener("click", async () => {
    try {
        const response = await fetch(apiUrl, {
            method: "GET",
            headers: {
                "Content-Type": "application/json",
            },
        });

        const products = await response.json();

        if (response.ok) {
            const productsListDiv = document.getElementById("productsList");
            productsListDiv.innerHTML = ""; 

            if (products.length === 0) {
                productsListDiv.innerHTML = "<p>Nenhum produto cadastrado.</p>";
            } else {
                const listHtml = products.map(product => {
                    return `
                        <div class="product">
                            <h3>${product.productName}</h3>
                            <p>${product.productDescription}</p>
                            <p>Preço: R$ ${product.productPrice}</p>
                            <p>Quantidade em Estoque: ${product.productStockQuantity}</p>
                        </div>
                    `;
                }).join("");  

                productsListDiv.innerHTML = listHtml;  
            }
        } else {
            alert("Erro ao carregar os produtos.");
        }
    } catch (error) {
        alert("Erro ao carregar os produtos. Tente novamente.");
    }
});
