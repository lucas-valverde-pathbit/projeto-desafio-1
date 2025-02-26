// URL da API - Certifique-se de que está correta para seu ambiente
const apiProductUrl = window.location.hostname === "localhost"
    ? "http://localhost:5064/api/products"
    : "http://api:5064/api/products";

// Seleciona os elementos necessários
const productForm = document.getElementById("productForm");
const productList = document.getElementById("productsList");
const productFormContainer = document.querySelector(".product-form-container");
const addProductBtn = document.getElementById("addProductBtn");
let editingProduct = null;

// Função para listar os produtos cadastrados
async function listProducts() {
    try {
        const response = await fetch(apiProductUrl, {
            headers: {
                'Authorization': `Bearer ${localStorage.getItem('token')}` // Adicionando o token no cabeçalho
            }
        });

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
    console.log(`Iniciando edição do produto ID: ${productId}`);
    try {
        const response = await fetch(`${apiProductUrl}/${productId}`, {
            headers: {
                'Authorization': `Bearer ${localStorage.getItem('token')}` // Adicionando o token no cabeçalho
            }
        });

        if (!response.ok) {
            throw new Error(`Erro HTTP! Status: ${response.status}`);
        }

        const product = await response.json();
        console.log("Dados do produto recebidos:", product);

        document.getElementById("productName").value = product.productName;
        document.getElementById("productDescription").value = product.productDescription;
        document.getElementById("productPrice").value = product.productPrice;
        document.getElementById("productStockQuantity").value = product.productStockQuantity;

        editingProduct = product;
        document.querySelector('.product-form-container').style.display = "block";
        document.getElementById("formTitle").innerText = "Editar Produto";
        console.log("Editando produto existente:", product);
    } catch (error) {
        console.error("Erro ao editar o produto:", error);
        alert(`Erro ao carregar os dados do produto: ${error.message}`);
    }
}

// Função para excluir um produto
async function deleteProduct(productId) {
    try {
        const response = await fetch(`${apiProductUrl}/${productId}`, {
            method: "DELETE",
            headers: {
                'Authorization': `Bearer ${localStorage.getItem('token')}` // Adicionando o token no cabeçalho
            }
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

// Função para cadastrar ou atualizar o produto
productForm.addEventListener("submit", async (e) => {
    e.preventDefault();
    console.log("Formulário de produto submetido");

    const productName = document.getElementById("productName").value;
    const productDescription = document.getElementById("productDescription").value;
    const productPrice = parseFloat(document.getElementById("productPrice").value);
    const productStockQuantity = parseInt(document.getElementById("productStockQuantity").value);

    if (!productName || !productDescription || isNaN(productPrice) || isNaN(productStockQuantity)) {
        alert("Por favor, preencha todos os campos corretamente.");
        return;
    }

    const productData = { 
        productName, 
        productDescription, 
        productPrice, 
        productStockQuantity 
    };

    try {
        let response;
        if (editingProduct) {
            console.log("Atualizando produto existente ID:", editingProduct.id);
            const url = `${apiProductUrl}/${editingProduct.id}`;
            response = await fetch(url, {
                method: "PUT",
                headers: { 
                    "Content-Type": "application/json",
                    "Authorization": `Bearer ${localStorage.getItem('token')}`
                },
                body: JSON.stringify(productData),
            });
        } else {
            console.log("Criando novo produto");
            response = await fetch(apiProductUrl, {
                method: "POST",
                headers: { 
                    "Content-Type": "application/json",
                    "Authorization": `Bearer ${localStorage.getItem('token')}`
                },
                body: JSON.stringify(productData),
            });
        }

        if (!response.ok) {
            throw new Error(`Erro HTTP! Status: ${response.status}`);
        }

        const result = await response.json();
        console.log("Resposta da API:", result);

        alert("Produto salvo com sucesso!");
        listProducts();
        document.getElementById("productForm").reset();
        productFormContainer.style.display = "none";
        editingProduct = null;
    } catch (error) {
        console.error("Erro ao salvar o produto:", error);
        alert(`Erro ao salvar o produto: ${error.message}`);
    }
});

// Função para exibir o formulário de cadastro
addProductBtn.addEventListener("click", function() {
    console.log("Botão Adicionar Produto clicado");
    document.querySelector('.product-form-container').style.display = "block";
    document.getElementById("formTitle").innerText = "Cadastrar Produto";
    productForm.reset();
    editingProduct = null;
    console.log("Formulário de cadastro exibido");
});

// Chama a função de listagem quando a página carrega
listProducts();
