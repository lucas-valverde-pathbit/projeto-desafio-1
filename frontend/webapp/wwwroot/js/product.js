const apiBaseUrl = window.location.hostname === "localhost"
    ? "http://localhost:5064/api/products"  // URL para desenvolvimento (localhost)
    : "http://api:5064/api/products";       // URL para produção

// Função para carregar a lista de produtos (requisição GET)
async function loadProducts() {
    try {
        const response = await fetch(apiBaseUrl); // URL do seu backend
        if (!response.ok) {
            throw new Error("Erro ao carregar produtos.");
        }
        const products = await response.json(); // Converte a resposta para JSON

        console.log("Produtos recebidos da API: ", products); // Verifique o que é retornado

        const productListContainer = document.getElementById("productsList");
        productListContainer.innerHTML = ""; // Limpa a lista de produtos antes de preenchê-la

        if (products.length > 0) {
            // Cria os cartões de produto e os insere no DOM
            products.forEach(product => {
                const productCard = document.createElement('div');
                productCard.classList.add('product-card');

                // Verifique se os dados são válidos antes de usar toFixed()
                const productPrice = (product.ProductPrice && !isNaN(product.ProductPrice)) ? product.ProductPrice.toFixed(2) : "N/A";
                const productStockQuantity = (product.ProductStockQuantity && !isNaN(product.ProductStockQuantity)) ? product.ProductStockQuantity : "N/A";

                productCard.innerHTML = `
                    <h3 class="product-name">${product.productName}</h3>
                    <p class="product-description">${product.productDescription}</p>
                    <p><strong>Preço:</strong> R$ ${productPrice}</p>
                    <p><strong>Estoque:</strong> ${productStockQuantity}</p>
                    <button onclick="editProduct('${product.Id}')">Editar</button>
                    <button onclick="deleteProduct('${product.Id}')">Excluir</button>
                `;
                productListContainer.appendChild(productCard);
            });
        } else {
            const noProductsMessage = document.createElement('p');
            noProductsMessage.textContent = "Nenhum produto encontrado.";
            productListContainer.appendChild(noProductsMessage);
        }
    } catch (error) {
        console.error('Erro ao carregar produtos:', error);
    }
}

// Função para editar um produto (requisição GET)
async function editProduct(productId) {
    try {
        const response = await fetch(`${apiBaseUrl}/${productId}`);
        const product = await response.json();

        console.log("Produto para edição: ", product); // Verifique a estrutura da resposta para edição

        document.getElementById("editProductModal").style.display = "block";
        document.getElementById("addProductModal").style.display = "none";

        document.getElementById("editProductId").value = product.Id;
        document.getElementById("editProductName").value = product.productName;
        document.getElementById("editProductDescription").value = product.productDescription;
        document.getElementById("editProductPrice").value = product.ProductPrice; // Corrigido de Productprice para ProductPrice
        document.getElementById("editProductStockQuantity").value = product.ProductStockQuantity; // Corrigido de roductStockQuantity para ProductStockQuantity
    } catch (error) {
        console.error("Erro ao editar produto:", error);
    }
}

// Função para atualizar um produto (requisição PUT)
async function updateProduct(event) {
    event.preventDefault();

    const productId = document.getElementById("editProductId").value;
    const updatedProduct = {
        productName: document.getElementById("editProductName").value,
        productDescription: document.getElementById("editProductDescription").value,
        productPrice: parseFloat(document.getElementById("editProductPrice").value),
        productStockQuantity: parseInt(document.getElementById("editProductStockQuantity").value),
    };

    console.log("Dados do produto para atualização: ", updatedProduct);  // Debugging

    try {
        const response = await fetch(`${apiBaseUrl}/${productId}`, {
            method: 'PUT',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(updatedProduct)  // Envia os dados como JSON
        });

        if (!response.ok) {
            throw new Error("Erro ao atualizar produto.");
        }

        loadProducts();  // Atualiza a lista de produtos
        closeForm();
    } catch (error) {
        console.error('Erro ao atualizar produto:', error);
    }
}

// Função para excluir um produto (requisição DELETE)
async function deleteProduct(productId) {
    try {
        // Certificando-se de que o productId é passado corretamente
        const response = await fetch(`${apiBaseUrl}/${productId}`, {
            method: 'DELETE'
        });

        if (!response.ok) {
            throw new Error("Erro ao excluir produto.");
        }

        loadProducts();  // Atualiza a lista de produtos
    } catch (error) {
        console.error('Erro ao excluir produto:', error);
    }
}

// Função para salvar um novo produto (requisição POST)
async function saveProduct(event) {
    event.preventDefault();
    const product = {
        ProductName: document.getElementById("productName").value,
        ProductDescription: document.getElementById("productDescription").value,
        ProductPrice: parseFloat(document.getElementById("productPrice").value),
        ProductStockQuantity: parseInt(document.getElementById("productStockQuantity").value),
    };

    try {
        const response = await fetch(apiBaseUrl, {  // URL do backend
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(product)  // Envia os dados como JSON
        });

        if (!response.ok) {
            throw new Error("Erro ao salvar produto.");
        }

        loadProducts();  // Atualiza a lista de produtos após salvar
        closeForm();
    } catch (error) {
        console.error('Erro ao salvar produto:', error);
    }
}

// Função para exibir o formulário de adição de produto
function showAddProductForm() {
    document.getElementById("addProductModal").style.display = "block";
    document.getElementById("editProductModal").style.display = "none";
    document.getElementById("productForm").reset();  // Limpar formulário
}

// Função para fechar os formulários
function closeForm() {
    document.getElementById("addProductModal").style.display = "none";
    document.getElementById("editProductModal").style.display = "none";
}

// Função para filtrar produtos
function filterProducts() {
    const filter = document.getElementById("filterInput").value.toLowerCase();
    const productCards = document.getElementsByClassName("product-card");

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

// Inicializar a lista de produtos ao carregar a página
document.addEventListener('DOMContentLoaded', loadProducts);  // Chama loadProducts quando a página carrega
