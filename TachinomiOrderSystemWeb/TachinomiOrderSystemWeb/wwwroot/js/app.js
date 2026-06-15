// App State
let menuItems = [];
let cart = [];
let grandTotal = 0;

let selectedMenuItem = null;
let orderQuantity = 1;

let isSqlAvailable = false;

// DOM Elements
const dbStatusBadge = document.getElementById('db-status-badge');
const dbStatusText = document.getElementById('db-status-text');
const menuGrid = document.getElementById('menu-grid');
const cartList = document.getElementById('cart-list');
const grandTotalEl = document.getElementById('grand-total');
const btnCheckout = document.getElementById('btn-checkout');

// Modals
const modalOrderConfirm = document.getElementById('modal-order-confirm');
const confirmItemName = document.getElementById('confirm-item-name');
const confirmItemPrice = document.getElementById('confirm-item-price');
const confirmQty = document.getElementById('confirm-qty');
const btnQtyMinus = document.getElementById('btn-qty-minus');
const btnQtyPlus = document.getElementById('btn-qty-plus');
const btnCancelOrder = document.getElementById('btn-cancel-order');
const btnSubmitOrder = document.getElementById('btn-submit-order');

const modalCheckout = document.getElementById('modal-checkout');
const checkoutTotal = document.getElementById('checkout-total');
const btnCloseCheckout = document.getElementById('btn-close-checkout');
const btnConfirmCheckout = document.getElementById('btn-confirm-checkout');

const modalCheckoutComplete = document.getElementById('modal-checkout-complete');
const btnResetSession = document.getElementById('btn-reset-session');

const modalDbWarning = document.getElementById('modal-db-warning');
const btnCloseWarning = document.getElementById('btn-close-warning');

const modalHistory = document.getElementById('modal-history');
const historyList = document.getElementById('history-list');
const btnViewHistory = document.getElementById('btn-view-history');
const btnClearHistory = document.getElementById('btn-clear-history');
const btnCloseHistory = document.getElementById('btn-close-history');

// SVG Vectors matching WPF vector paths
const beerSvg = `
<svg width="120" height="100" viewBox="0 0 120 100" xmlns="http://www.w3.org/2000/svg">
    <!-- Beer Glass Base Shadow -->
    <ellipse cx="60" cy="80" rx="35" ry="10" fill="#050505" opacity="0.5" />
    <!-- Glass Handle (Left side) -->
    <path d="M 40,35 L 25,35 A 15,15 0 0,0 25,65 L 40,65" stroke="#E2E8F0" stroke-width="5" fill="transparent" />
    <!-- Beer Glass & Liquid -->
    <path d="M 40,25 L 80,25 L 75,75 A 5,5 0 0,1 70,80 L 50,80 A 5,5 0 0,1 45,75 Z" fill="#D97706" stroke="#F1F5F9" stroke-width="3.5" />
    <!-- Beer Fluid Highlights -->
    <path d="M 48,32 L 72,32 L 68,72 L 52,72 Z" fill="#F59E0B" opacity="0.85" />
    <!-- Bubbles inside Beer -->
    <circle cx="52" cy="45" r="2" fill="#FFE0B2" />
    <circle cx="66" cy="55" r="1.5" fill="#FFE0B2" />
    <circle cx="58" cy="62" r="2" fill="#FFE0B2" />
    <!-- Foam Header -->
    <rect x="35" y="15" width="50" height="22" rx="11" ry="11" fill="#FFFFFF" />
    <circle cx="45" cy="22" r="9" fill="#FFFFFF" />
    <circle cx="75" cy="22" r="9" fill="#FFFFFF" />
</svg>
`;

const edamameSvg = `
<svg width="120" height="100" viewBox="0 0 120 100" xmlns="http://www.w3.org/2000/svg">
    <!-- Bowl Shadow -->
    <ellipse cx="60" cy="80" rx="40" ry="7.5" fill="#050505" opacity="0.5" />
    <!-- Red Lacquer Bowl -->
    <path d="M 20,45 C 20,80 100,80 100,45 Z" fill="#991B1B" stroke="#1E293B" stroke-width="2" />
    <path d="M 20,45 Q 60,53 100,45" fill="#7F1D1D" stroke="#1E293B" stroke-width="1" />
    <!-- Edamame Pod 1 -->
    <path d="M 35,32 Q 55,25 75,40 Q 55,50 35,32" fill="#22C55E" stroke="#166534" stroke-width="1.5" />
    <!-- Beans inside pod 1 -->
    <circle cx="46" cy="36" r="4" fill="#4ADE80" />
    <circle cx="58" cy="38" r="4" fill="#4ADE80" />
    <circle cx="68" cy="40" r="4" fill="#4ADE80" />
    <!-- Edamame Pod 2 (Overlayed) -->
    <path d="M 50,22 Q 70,25 85,15 Q 75,40 50,22" fill="#15803D" stroke="#14532D" stroke-width="1.5" />
    <!-- Edamame Pod 3 (Left side) -->
    <path d="M 25,48 Q 45,55 65,48 Q 45,40 25,48" fill="#16A34A" stroke="#15803D" stroke-width="1.5" />
</svg>
`;

const fallbackSvg = `
<svg width="120" height="100" viewBox="0 0 120 100" xmlns="http://www.w3.org/2000/svg">
    <circle cx="60" cy="50" r="30" fill="#374151" stroke="#4B5563" stroke-width="2" />
</svg>
`;

// Initialize Application
async function init() {
    await checkDbStatus();
    await loadMenu();
    setupEventListeners();
}

// 1. Check database connectivity
async function checkDbStatus() {
    try {
        const res = await fetch('/api/status');
        const data = await res.json();
        isSqlAvailable = data.isSqlAvailable;
        
        if (isSqlAvailable) {
            dbStatusBadge.className = 'db-status online';
            dbStatusText.textContent = 'LocalDB 接続中';
        } else {
            dbStatusBadge.className = 'db-status offline';
            dbStatusText.textContent = 'DB接続エラー (オフライン)';
            showModal(modalDbWarning);
        }
    } catch (e) {
        console.error('Failed to check database status:', e);
        dbStatusBadge.className = 'db-status offline';
        dbStatusText.textContent = 'DB接続エラー (オフライン)';
        showModal(modalDbWarning);
    }
}

// 2. Load menu items
async function loadMenu() {
    try {
        const res = await fetch('/api/menu');
        menuItems = await res.json();
        renderMenu();
    } catch (e) {
        console.error('Failed to load menu items:', e);
        // Fallbacks if server fails entirely
        menuItems = [
            { id: 1, name: '生ビール', price: 500 },
            { id: 2, name: '枝豆', price: 300 }
        ];
        renderMenu();
    }
}

// Render menu items
function renderMenu() {
    menuGrid.innerHTML = '';
    menuItems.forEach(item => {
        const card = document.createElement('div');
        card.className = 'menu-card';
        card.id = `menu-item-${item.id}`;
        
        let artSvg = fallbackSvg;
        if (item.name === '生ビール') artSvg = beerSvg;
        else if (item.name === '枝豆') artSvg = edamameSvg;

        card.innerHTML = `
            <div class="menu-card-art">${artSvg}</div>
            <div class="menu-card-name">${item.name}</div>
            <div class="menu-card-price">${item.price}円 (税込)</div>
            <div class="menu-card-badge">注文する</div>
        `;

        card.addEventListener('click', () => openOrderConfirm(item));
        menuGrid.appendChild(card);
    });
}

// Event Listeners setup
function setupEventListeners() {
    // Quantity Pickers
    btnQtyMinus.addEventListener('click', () => {
        if (orderQuantity > 1) {
            orderQuantity--;
            confirmQty.textContent = orderQuantity;
        }
    });
    
    btnQtyPlus.addEventListener('click', () => {
        if (orderQuantity < 10) {
            orderQuantity++;
            confirmQty.textContent = orderQuantity;
        }
    });

    // Modals buttons
    btnCancelOrder.addEventListener('click', () => hideModal(modalOrderConfirm));
    btnSubmitOrder.addEventListener('click', confirmOrder);

    btnCheckout.addEventListener('click', openCheckout);
    btnCloseCheckout.addEventListener('click', () => hideModal(modalCheckout));
    btnConfirmCheckout.addEventListener('click', confirmCheckout);

    btnResetSession.addEventListener('click', resetSession);
    btnCloseWarning.addEventListener('click', () => hideModal(modalDbWarning));

    btnViewHistory.addEventListener('click', openHistory);
    btnCloseHistory.addEventListener('click', () => hideModal(modalHistory));
    btnClearHistory.addEventListener('click', clearHistory);
}

// Modal helper controls
function showModal(modal) {
    modal.style.display = 'flex';
}

function hideModal(modal) {
    modal.style.display = 'none';
}

// 3. Order confirm flow
function openOrderConfirm(item) {
    selectedMenuItem = item;
    orderQuantity = 1;
    confirmQty.textContent = '1';
    confirmItemName.textContent = item.name;
    confirmItemPrice.textContent = `${item.price}円 (税込)`;
    showModal(modalOrderConfirm);
}

function confirmOrder() {
    if (!selectedMenuItem) return;
    
    const existing = cart.find(o => o.item.id === selectedMenuItem.id);
    const nowStr = new Date().toLocaleTimeString('ja-JP', { hour12: false });
    
    if (existing) {
        existing.quantity += orderQuantity;
    } else {
        cart.unshift({
            item: selectedMenuItem,
            quantity: orderQuantity,
            formattedOrderTime: nowStr
        });
    }

    grandTotal += selectedMenuItem.price * orderQuantity;
    hideModal(modalOrderConfirm);
    renderCart();
}

// Render Cart
function renderCart() {
    cartList.innerHTML = '';
    
    if (cart.length === 0) {
        cartList.innerHTML = '<li class="cart-empty-message">ご注文はまだありません。メニューから選択してください。</li>';
        grandTotalEl.textContent = '0 円';
        btnCheckout.disabled = true;
        return;
    }

    cart.forEach(cartItem => {
        const li = document.createElement('li');
        li.className = 'cart-item';
        
        li.innerHTML = `
            <div class="cart-item-details">
                <span class="cart-item-name">${cartItem.item.name}</span>
                <span class="cart-item-time">${cartItem.formattedOrderTime}</span>
            </div>
            <div class="cart-item-right">
                <span class="cart-item-qty">${cartItem.quantity}つ</span>
                <span class="cart-item-total">${cartItem.item.price * cartItem.quantity}円</span>
            </div>
        `;
        cartList.appendChild(li);
    });

    grandTotalEl.textContent = `${grandTotal} 円`;
    btnCheckout.disabled = false;
}

// 4. Checkout flow
function openCheckout() {
    if (cart.length === 0) return;
    checkoutTotal.textContent = `${grandTotal} 円`;
    showModal(modalCheckout);
}

async function confirmCheckout() {
    hideModal(modalCheckout);
    
    // Prepare API body
    const orderItems = cart.map(c => ({
        itemId: c.item.id,
        itemName: c.item.name,
        price: c.item.price,
        quantity: c.quantity
    }));

    const payload = {
        totalAmount: grandTotal,
        items: orderItems
    };

    try {
        const res = await fetch('/api/order', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(payload)
        });
        if (res.ok) {
            console.log('Order saved successfully.');
        } else {
            console.error('Failed to save order to database.');
        }
    } catch (e) {
        console.error('Network error saving order:', e);
    }

    showModal(modalCheckoutComplete);
}

function resetSession() {
    cart = [];
    grandTotal = 0;
    renderCart();
    hideModal(modalCheckoutComplete);
}

// 5. History Modal controls
async function openHistory() {
    showModal(modalHistory);
    await loadHistory();
}

async function loadHistory() {
    historyList.innerHTML = '<li class="history-empty-message">読み込み中...</li>';
    try {
        const res = await fetch('/api/history');
        const history = await res.json();
        
        historyList.innerHTML = '';
        if (history.length === 0) {
            historyList.innerHTML = '<li class="history-empty-message">履歴はありません。</li>';
            return;
        }

        history.forEach(h => {
            const li = document.createElement('li');
            li.className = 'history-item';
            
            // Format time
            const timeStr = new Date(h.orderTime).toLocaleString('ja-JP', {
                year: 'numeric',
                month: '2-digit',
                day: '2-digit',
                hour: '2-digit',
                minute: '2-digit',
                second: '2-digit',
                hour12: false
            });

            li.innerHTML = `
                <div class="history-item-details">
                    <span class="history-item-summary">${h.summaryText}</span>
                    <span class="history-item-time">${timeStr}</span>
                </div>
                <span class="history-item-total">${h.formattedTotalPrice}</span>
            `;
            historyList.appendChild(li);
        });
    } catch (e) {
        console.error('Failed to load history:', e);
        historyList.innerHTML = '<li class="history-empty-message">履歴の読み込みに失敗しました。</li>';
    }
}

async function clearHistory() {
    if (!confirm('会計履歴をすべて削除しますか？')) return;

    try {
        const res = await fetch('/api/history/clear', { method: 'POST' });
        if (res.ok) {
            await loadHistory();
        } else {
            alert('履歴の削除に失敗しました。');
        }
    } catch (e) {
        console.error('Failed to clear history:', e);
        alert('履歴の削除に失敗しました。');
    }
}

// Start app
window.addEventListener('DOMContentLoaded', init);
