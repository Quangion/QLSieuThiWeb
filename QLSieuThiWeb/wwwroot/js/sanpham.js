// Khởi tạo biến toàn cục
let isEdit = false;
let productModal;

// Khởi tạo khi trang đã load xong
$(document).ready(function() {
    console.log('Document ready');
    
    // Khởi tạo modal
    productModal = new bootstrap.Modal(document.getElementById('productModal'));
    
    // Reset form khi đóng modal
    $('#productModal').on('hidden.bs.modal', function() {
        $('#productForm').trigger('reset');
        isEdit = false;
    });
});

// Mở modal thêm mới
function openModal() {
    console.log("Mở modal thêm mới");
    isEdit = false;
    $('#modalTitle').text('Thêm sản phẩm mới');
    $('#maSP').prop('readonly', false);
    $('#productForm').trigger('reset');
    $('#productModal').modal('show');
}

// Mở modal sửa
function editProduct(id) {
    isEdit = true;
    $('#modalTitle').text('Sửa sản phẩm');
    $('#maSP').prop('readonly', true);

    $.ajax({
        url: '/SanPham/GetSanPham',
        type: 'GET',
        data: { maSP: id },
        success: function (data) {
            $('#maSP').val(data.maSP);
            $('#tenSP').val(data.tenSP);
            $('#soLuong').val(data.soLuong);
            $('#gia').val(data.gia);
            $('#productModal').modal('show');
        }
    });
}

// Xóa sản phẩm
function deleteProduct(id) {
    if (confirm('Bạn có chắc chắn muốn xóa sản phẩm này? \nLưu ý: Tất cả chi tiết hóa đơn liên quan cũng sẽ bị xóa.')) {
        $.ajax({
            url: '/SanPham/XoaSanPham',
            type: 'POST',
            data: { maSP: id },
            success: function (result) {
                if (result.success) {
                    alert('Xóa sản phẩm thành công!');
                    location.reload();
                }
                else {
                    alert(result.message);
                }
            }
        });
    }
}

// Lưu sản phẩm mới
function saveProduct() {
    console.log("Bắt đầu lưu sản phẩm");
    
    let data = {
        maSP: $('#maSP').val(),
        tenSP: $('#tenSP').val(),
        soLuong: $('#soLuong').val(),
        gia: $('#gia').val()
    };

    console.log("Dữ liệu form:", data);

    // Kiểm tra dữ liệu
    if (!data.maSP || !data.tenSP || !data.soLuong || !data.gia) {
        alert('Vui lòng nhập đầy đủ thông tin!');
        return;
    }

    let url = isEdit ? '/SanPham/CapNhatSanPham' : '/SanPham/ThemSanPham';

    $.ajax({
        url: url,
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(data),
        success: function (result) {
            console.log("Response:", result);
            if (result.success) {
                alert(isEdit ? 'Cập nhật thành công!' : 'Thêm mới thành công!');
                $('#productModal').modal('hide');
                location.reload();
            }
            else {
                alert(result.message);
            }
        },
        error: function(xhr, status, error) {
            console.error("Lỗi:", error);
            alert('Có lỗi xảy ra khi thêm sản phẩm!');
        }
    });
}

// Reset form khi đóng modal
$('#productModal').on('hidden.bs.modal', function () {
    $('#productForm').trigger('reset');
    isEdit = false;
});

// Thêm event listeners khi trang load
document.addEventListener('DOMContentLoaded', function() {
    // Thêm event listeners cho input số lượng và giá
    document.getElementById('soLuong').addEventListener('keypress', validateNumberInput);
    document.getElementById('gia').addEventListener('keypress', validateNumberInput);
    
    // Ngăn chặn việc nhập e hoặc dấu chấm trong input number
    document.getElementById('soLuong').addEventListener('keydown', function(e) {
        if (e.key === 'e' || e.key === '.') {
            e.preventDefault();
        }
    });
    
    document.getElementById('gia').addEventListener('keydown', function(e) {
        if (e.key === 'e' || e.key === '.') {
            e.preventDefault();
        }
    });
    
    // Thêm event listener cho input tìm kiếm
    document.getElementById('search-text').addEventListener('input', searchProducts);
    
    loadProducts();
});

let allProducts = [];
let products = [];
let isEditing = false;

// Lấy dữ liệu từ server
function loadProducts() {
    $.ajax({
        url: '/SanPham/GetAllProducts',
        type: 'GET',
        success: function (data) {
            if (data.error) {
                console.error(data.error);
                return;
            }
            var html = '';
            $.each(data, function (i, item) {
                html += '<tr>';
                html += '<td>' + item.maSP + '</td>';
                html += '<td>' + item.tenSP + '</td>';
                html += '<td>' + item.soLuong + '</td>';
                html += '<td>' + item.gia + '</td>';
                html += '<td><button onclick="editProduct(\'' + item.maSP + '\')">Sửa</button>';
                html += '<button onclick="deleteProduct(\'' + item.maSP + '\')">Xóa</button></td>';
                html += '</tr>';
            });
            $('#productTable tbody').html(html);
        },
        error: function (xhr, status, error) {
            console.error('Error:', error);
        }
    });
}

// Hiển thị bảng sản phẩm
function renderProductTable() {
    const tbody = document.getElementById('productTableBody');
    tbody.innerHTML = '';
    
    products.forEach(product => {
        tbody.innerHTML += `
            <tr>
                <td>${product.maSP}</td>
                <td>${product.tenSP}</td>
                <td>${formatCurrency(product.gia)}</td>
                <td>${product.soLuong}</td>
                <td>
                    <button class="btn btn-sm btn-warning me-2" onclick="editProduct('${product.maSP}')">
                        <i class="fas fa-pen"></i>
                    </button>
                    <button class="btn btn-sm btn-danger" onclick="deleteProduct('${product.maSP}')">
                        <i class="fas fa-trash"></i>
                    </button>
                </td>
            </tr>
        `;
    });
}

// Hàm mở modal thêm sản phẩm mới
function openAddModal() {
    document.getElementById('modalTitle').textContent = 'Thêm Sản Phẩm';
    document.getElementById('productForm').reset();
    document.getElementById('maSP').removeAttribute('readonly');
    
    // Đánh dấu form đang trong chế độ thêm mới
    document.getElementById('productForm').setAttribute('data-mode', 'add');
    
    new bootstrap.Modal(document.getElementById('productModal')).show();
}

// Thêm hàm kiểm tra input
function validateNumberInput(event) {
    // Chỉ cho phép nhập số
    if (event.key < '0' || event.key > '9') {
        event.preventDefault();
    }
}

// Thêm hàm kiểm tra mã sản phẩm
async function checkMaSPExists(maSP) {
    try {
        const response = await fetch('/SanPham/CheckMaSP', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(maSP)
        });

        const result = await response.json();
        return result.exists;
    } catch (error) {
        console.error('Error:', error);
        return false;
    }
}

// Thêm hàm tìm kiếm
function searchProducts() {
    const searchText = document.getElementById('search-text').value.toLowerCase().trim();
    
    if (searchText === '') {
        products = [...allProducts]; // Nếu không có text tìm kiếm, hiển thị tất cả
    } else {
        products = allProducts.filter(product => 
            product.maSP.toLowerCase().includes(searchText) ||
            product.tenSP.toLowerCase().includes(searchText) ||
            product.soLuong.toLowerCase().includes(searchText) ||
            product.gia.toLowerCase().includes(searchText)
        );
    }
    
    renderProductTable();
}

// Hiển thị toast thông báo
function showToast(message, type = 'success') {
    const toast = document.getElementById('toastNotification');
    const toastMessage = document.getElementById('toastMessage');
    toastMessage.textContent = message;
    toast.classList.remove('bg-danger', 'bg-success');
    toast.classList.add(type === 'error' ? 'bg-danger' : 'bg-success');
    toast.classList.add('text-white');
    const bsToast = new bootstrap.Toast(toast);
    bsToast.show();
}

// Format tiền tệ
function formatCurrency(amount) {
    return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(amount);
}

// Thêm event listener cho nút lưu
document.getElementById('saveButton').addEventListener('click', saveProduct);

function editProduct(maSP) {
    isEdit = true;
    document.getElementById('modalTitle').textContent = 'Sửa sản phẩm';
    document.getElementById('maSP').readOnly = true;

    // Lấy thông tin sản phẩm
    fetch(`/SanPham/GetSanPham?maSP=${maSP}`)
        .then(response => response.json())
        .then(data => {
            document.getElementById('maSP').value = data.maSP;
            document.getElementById('tenSP').value = data.tenSP;
            document.getElementById('soLuong').value = data.soLuong;
            document.getElementById('gia').value = data.gia;
            $('#productModal').modal('show');
        })
        .catch(error => {
            console.error('Error:', error);
            alert('Không thể lấy thông tin sản phẩm!');
        });
}

function deleteProduct(maSP) {
    if (confirm('Bạn có chắc muốn xóa sản phẩm này?')) {
        fetch('/SanPham/XoaSanPham', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify({ maSP: maSP })
        })
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                alert('Xóa sản phẩm thành công!');
                location.reload();
            } else {
                alert(data.message || 'Xóa sản phẩm thất bại!');
            }
        })
        .catch(error => {
            console.error('Error:', error);
            alert('Có lỗi xảy ra khi xóa sản phẩm!');
        });
    }
}

function saveProduct() {
    const form = document.getElementById('productForm');
    if (!form.checkValidity()) {
        form.reportValidity();
        return;
    }

    const data = {
        maSP: document.getElementById('maSP').value,
        tenSP: document.getElementById('tenSP').value,
        soLuong: parseInt(document.getElementById('soLuong').value),
        gia: parseFloat(document.getElementById('gia').value)
    };

    const url = isEdit ? '/SanPham/CapNhatSanPham' : '/SanPham/ThemSanPham';

    fetch(url, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
        },
        body: JSON.stringify(data)
    })
    .then(response => response.json())
    .then(data => {
        if (data.success) {
            alert(isEdit ? 'Cập nhật thành công!' : 'Thêm mới thành công!');
            $('#productModal').modal('hide');
            location.reload();
        } else {
            alert(data.message || 'Thao tác thất bại!');
        }
    })
    .catch(error => {
        console.error('Error:', error);
        alert('Có lỗi xảy ra!');
    });
} 