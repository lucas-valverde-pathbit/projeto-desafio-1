const apiProductUrl = window.location.hostname === "localhost"
                       ? "http://localhost:5064/api/products"
                       : "http://api:5064/api/products";

// Função para cadastrar um produto
document.getElementById("productForm").addEventListener("submit", async (e) => {
    e.preventDefault();

    const productName = document.getElementById("productName").value;
    const productDescription = document.getElementById("productDescription").value;
    const productPrice = parseFloat(document.getElementById("productPrice").value);
    const productStockQuantity = parseInt(document.getElementById("productStockQuantity").value);

    const productData = { productName, productDescription, productPrice, productStockQuantity };

    try {
        const response = await fetch(apiProductUrl, {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(productData),
        });

        const result = await response.json();
        if (response.ok) {
            alert("Produto cadastrado com sucesso!");
            listProducts();  // Atualiza a lista de produtos
            document.getElementById("productForm").reset();  // Limpa o formulário
        } else {
            alert(`Erro: ${result.message}`);
        }
    } catch (error) {
        alert("Erro ao cadastrar o produto.");
    }
});

// Função para listar os produtos cadastrados
async function listProducts() {
    try {
        const response = await fetch(apiProductUrl);
        const products = await response.json();

        if (response.ok) {
            const productsListDiv = document.getElementById("productsList");
            productsListDiv.innerHTML = "";  // Limpa a lista

            if (products.length === 0) {
                productsListDiv.innerHTML = "<p>Nenhum produto cadastrado.</p>";
            } else {
                const listHtml = products.map(product => {
                    return `
                        <div class="product">
                            <h3>${product.productName}</h3>
                            <p>Descrição: ${product.productDescription}</p>
                            <p>Preço: R$ ${product.productPrice}</p>
                            <p>Quantidade em Estoque: ${product.productStockQuantity}</p>
                            <button onclick="editProduct('${product.id}')">Editar</button>
                            <button onclick="deleteProduct('${product.id}')">Excluir</button>
                        </div>
                    `;
                }).join("");

                productsListDiv.innerHTML = listHtml;  // Exibe os produtos
            }
        } else {
            alert("Erro ao carregar os produtos.");
        }
    } catch (error) {
        alert("Erro ao carregar os produtos.");
    }
}

// Função para editar um produto
async function editProduct(productId) {
    try {
        const response = await fetch(`${apiProductUrl}/${productId}`);
        const product = await response.json();

        if (response.ok) {
            document.getElementById("productName").value = product.productName;
            document.getElementById("productDescription").value = product.productDescription;
            document.getElementById("productPrice").value = product.productPrice;
            document.getElementById("productStockQuantity").value = product.productStockQuantity;
        } else {
            alert("Erro ao carregar os dados do produto.");
        }
    } catch (error) {
        alert("Erro ao editar o produto.");
    }
}

// Função para excluir um produto
async function deleteProduct(productId) {
    try {
        const response = await fetch(`${apiProductUrl}/${productId}`, {
            method: "DELETE"
        });

        if (response.ok) {
            alert("Produto excluído com sucesso!");
            listProducts();  // Atualiza a lista de produtos
        } else {
            alert("Erro ao excluir o produto.");
        }
    } catch (error) {
        alert("Erro ao excluir o produto.");
    }
}

// Chama a função de listagem quando a página carrega
listProducts();
