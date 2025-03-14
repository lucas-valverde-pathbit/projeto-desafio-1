window.addEventListener('DOMContentLoaded', () => {
    // Recupera as informações do usuário do localStorage
    const userInfo = JSON.parse(localStorage.getItem('userInfo'));

    // Se o usuário não estiver logado ou não for um administrador, redireciona para o login
    if (!userInfo || userInfo.role !== 'ADMINISTRADOR') {
        // Redireciona para a página de login
        window.location.href = 'sem-permissao.html';
    }
});

const apiBaseUrl = window.location.hostname === "localhost"
    ? "http://localhost:5064"
    : "http://api:5064";       


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


function loadProducts() {
    fetch(`${apiBaseUrl}/api/products`) 
        .then(response => response.json())
        .then(response => {
            const container = document.getElementById('productsList');
            if (container) {
                container.innerHTML = ''; 

  
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
     <div>
        <h3 class="product-name">${product.productName}</h3>
        <p class="product-description"><strong>Descrição:</strong> ${product.productDescription || 'Descrição não disponível'}</p>
        <p><strong>Preço:</strong> R$ ${product.productPrice.toFixed(2)}</p>
        <p><strong>Estoque:</strong> ${product.productStockQuantity}</p>
        <div class="actions">
            <button class="edit-button" onclick="editProduct('${product.id}')">Editar</button>
        </div>
     <div>
    `;

    return card;
}

async function editProduct(productId) {
    console.log("productId recebido:", productId); // Log do productId recebido

    if (!productId) {
        console.error("productId não foi fornecido!");
        return;
    }

    try {
        const response = await fetch(`${apiBaseUrl}/api/products/${productId}`);
        const product = await response.json();

        console.log("Produto para edição:", product); // Log da resposta da API

        if (product) {
            // Agora estamos lidando diretamente com o produto, não é necessário o `find`
            document.getElementById("editProductModal").style.display = "block";
            document.getElementById("addProductModal").style.display = "none";

            document.getElementById("editProductId").value = product.id;
            document.getElementById("editProductName").value = product.productName;
            document.getElementById("editProductDescription").value = product.productDescription;
            document.getElementById("editProductPrice").value = product.productPrice;
            document.getElementById("editProductStockQuantity").value = product.productStockQuantity;
        } else {
            console.error("Produto não encontrado.");
        }

    } catch (error) {
        console.error("Erro ao editar produto:", error);
    }
}


async function updateProduct(event) {
    // Coleta os dados do produto a partir dos campos do formulário
    const productId = document.getElementById("editProductId").value;
    const updatedProduct = {
        productName: document.getElementById("editProductName").value,
        productDescription: document.getElementById("editProductDescription").value,
        productPrice: parseFloat(document.getElementById("editProductPrice").value),
        productStockQuantity: parseInt(document.getElementById("editProductStockQuantity").value)
    };

    // Verificação simples para garantir que os campos obrigatórios não estejam vazios
    if (!updatedProduct.productName || !updatedProduct.productDescription || isNaN(updatedProduct.productPrice) || isNaN(updatedProduct.productStockQuantity)) {
        alert("Por favor, preencha todos os campos corretamente.");
        return;
    }

    // Exibe os dados para fins de depuração (remova em produção)
    console.log("Dados do produto para atualização:", updatedProduct);

    try {
        // Chama a API para atualizar o produto
        const response = await fetch(`${apiBaseUrl}/api/products/update/${productId}`, {
            method: 'PUT',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify(updatedProduct)  // Envia os dados do produto como JSON
        });

        // Verifica se a resposta da API foi bem-sucedida
        if (!response.ok) {
            throw new Error('Erro ao atualizar produto.');
        }

        const result = await response.json();
        console.log("Produto atualizado com sucesso:", result);

        // Exibir uma mensagem de sucesso
        alert('Produto atualizado com sucesso!');
        
        location.reload();
        closeForm();
    } catch (error) {
        console.error("Erro ao atualizar produto:", error);
        alert("Erro ao atualizar produto. Por favor, tente novamente.");
    }
}

async function deleteProduct() {
    const productId = document.getElementById("editProductId").value;

    try {
        const response = await fetch(`${apiBaseUrl}/api/products/${productId}`, {
            method: 'DELETE',
            headers: {
                'Content-Type': 'application/json',
            }
        });

        if (!response.ok) {
            throw new Error('Erro ao excluir produto.');
        }
        location.reload();
        closeForm();

    } catch (error) {
        console.error("Erro ao excluir produto:", error);
    }
}


async function saveProduct(event) {
    event.preventDefault();
    const product = {
        ProductName: document.getElementById("productName").value,
        ProductDescription: document.getElementById("productDescription").value,
        ProductPrice: parseFloat(document.getElementById("productPrice").value),
        ProductStockQuantity: parseInt(document.getElementById("productStockQuantity").value),
    };

    try {
        const response = await fetch(`${apiBaseUrl}/api/products`, {  
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(product) 
        });

        if (!response.ok) {
            throw new Error("Erro ao salvar produto.");
        }
        location.reload();
        closeForm();
    } catch (error) {
        console.error('Erro ao salvar produto:', error);
    }
}


function showAddProductForm() {
    document.getElementById("addProductModal").style.display = "block";
    document.getElementById("editProductModal").style.display = "none";
    document.getElementById("productForm").reset();
}


function closeForm() {
    document.getElementById("addProductModal").style.display = "none";
    document.getElementById("editProductModal").style.display = "none";
}

document.addEventListener('DOMContentLoaded', loadProducts); 
