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

// Mở modal sửa sản phẩm
function editProduct(maSP) {
    const product = products.find(p => p.maSP === maSP);
    if (product) {
        document.getElementById('modalTitle').textContent = 'Sửa Sản Phẩm';
        document.getElementById('maSP').value = product.maSP;
        document.getElementById('maSP').setAttribute('readonly', true);
        document.getElementById('tenSP').value = product.tenSP;
        document.getElementById('soLuong').value = product.soLuong;
        document.getElementById('gia').value = product.gia;
        
        // Đánh dấu form đang trong chế độ sửa
        document.getElementById('productForm').setAttribute('data-mode', 'edit');
        
        new bootstrap.Modal(document.getElementById('productModal')).show();
    }
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

// Cập nhật hàm saveProduct
async function saveProduct() {
    const form = document.getElementById('productForm');
    const isEditMode = form.getAttribute('data-mode') === 'edit';
    
    const maSP = document.getElementById('maSP').value.trim();
    const tenSP = document.getElementById('tenSP').value.trim();
    const soLuong = document.getElementById('soLuong').value.trim();
    const gia = document.getElementById('gia').value.trim();

    // Kiểm tra dữ liệu trước khi gửi
    if (!maSP || !tenSP || !soLuong || !gia) {
        showToast('Vui lòng điền đầy đủ thông tin', 'error');
        return;
    }

    // Kiểm tra số lượng và giá phải là số dương
    if (parseInt(soLuong) < 0 || parseInt(gia) < 0) {
        showToast('Số lượng và giá phải là số dương', 'error');
        return;
    }

    // Kiểm tra mã sản phẩm trùng lặp khi thêm mới
    if (!isEditMode) {
        const exists = await checkMaSPExists(maSP);
        if (exists) {
            showToast('Mã sản phẩm đã tồn tại!', 'error');
            return;
        }
    }

    const productData = {
        maSP: maSP,
        tenSP: tenSP,
        soLuong: soLuong,
        gia: gia
    };

    try {
        const response = await fetch(isEditMode ? '/SanPham/UpdateProduct' : '/SanPham/AddProduct', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(productData)
        });

        const result = await response.json();
        
        if (result.success) {
            showToast(result.message);
            await loadProducts();
            bootstrap.Modal.getInstance(document.getElementById('productModal')).hide();
        } else {
            showToast(result.message, 'error');
        }
    } catch (error) {
        console.error('Error:', error);
        showToast('Có lỗi xảy ra khi lưu sản phẩm', 'error');
    }
}

// Hàm xóa sản phẩm
async function deleteProduct(maSP) {
    if (!confirm('Bạn có chắc muốn xóa sản phẩm này?')) {
        return;
    }

    try {
        const response = await fetch('/SanPham/DeleteProduct', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(maSP)
        });

        const result = await response.json();
        
        if (result.success) {
            showToast(result.message);
            await loadProducts(); // Tải lại danh sách sản phẩm
        } else {
            showToast(result.message, 'error');
        }
    } catch (error) {
        console.error('Error:', error);
        showToast('Có lỗi xảy ra khi xóa sản phẩm', 'error');
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

// Hàm sửa sản phẩm
function suaSanPham(maSP) {
    console.log('Sửa sản phẩm:', maSP); // Debug log
    $.ajax({
        url: '/SanPham/GetSanPham',
        type: 'GET',
        data: { maSP: maSP },
        success: function(data) {
            if (data) {
                $('#editMaSP').val(data.maSP);
                $('#editTenSP').val(data.tenSP);
                $('#editSoLuong').val(data.soLuong);
                $('#editGia').val(data.gia);
                $('#editModal').modal('show');
            } else {
                alert('Không tìm thấy thông tin sản phẩm!');
            }
        },
        error: function(xhr, status, error) {
            console.error('Error:', error);
            alert('Có lỗi xảy ra khi lấy thông tin sản phẩm!');
        }
    });
}

// Hàm xóa sản phẩm
function xoaSanPham(maSP) {
    if (confirm('Bạn có chắc muốn xóa sản phẩm này?')) {
        $.ajax({
            url: '/SanPham/XoaSanPham',
            type: 'POST',
            data: { maSP: maSP },
            success: function(response) {
                if (response.success) {
                    alert('Xóa sản phẩm thành công!');
                    location.reload();
                } else {
                    alert('Xóa sản phẩm thất bại: ' + (response.message || 'Lỗi không xác định'));
                }
            },
            error: function(xhr, status, error) {
                console.error('Error:', error);
                alert('Có lỗi xảy ra khi xóa sản phẩm!');
            }
        });
    }
}

// Đảm bảo document đã sẵn sàng
$(document).ready(function() {
    console.log('Document ready'); // Debug log

    // Xử lý sự kiện click nút Lưu trong modal thêm mới
    $(document).on('click', '#btnLuuMoi', function() {
        console.log('Nút Lưu được click'); // Debug log
        luuSanPhamMoi();
    });

    // Xử lý sự kiện click nút Sửa
    $(document).on('click', '.btn-sua', function() {
        var maSP = $(this).data('masp');
        suaSanPham(maSP);
    });

    // Xử lý sự kiện click nút Xóa
    $(document).on('click', '.btn-xoa', function() {
        var maSP = $(this).data('masp');
        xoaSanPham(maSP);
    });
});

// Hàm lưu sản phẩm mới
function luuSanPhamMoi() {
    // Lấy dữ liệu từ form
    var maSP = $('#addMaSP').val().trim();
    var tenSP = $('#addTenSP').val().trim();
    var soLuong = $('#addSoLuong').val().trim();
    var gia = $('#addGia').val().trim();

    console.log('Dữ liệu form:', { maSP, tenSP, soLuong, gia }); // Debug log

    // Validate dữ liệu
    if (!maSP || !tenSP || !soLuong || !gia) {
        alert('Vui lòng nhập đầy đủ thông tin!');
        return;
    }

    // Gửi request AJAX
    $.ajax({
        url: '/SanPham/ThemSanPham',
        type: 'POST',
        data: {
            maSP: maSP,
            tenSP: tenSP,
            soLuong: soLuong,
            gia: gia
        },
        success: function(response) {
            console.log('Response:', response); // Debug log
            if (response.success) {
                alert('Thêm sản phẩm thành công!');
                $('#addModal').modal('hide');
                location.reload();
            } else {
                alert('Thêm sản phẩm thất bại: ' + (response.message || 'Lỗi không xác định'));
            }
        },
        error: function(xhr, status, error) {
            console.error('Error:', error);
            alert('Có lỗi xảy ra khi thêm sản phẩm!');
        }
    });
}

// Hàm lưu cập nhật
function luuCapNhat() {
    var maSP = $('#editMaSP').val();
    var tenSP = $('#editTenSP').val().trim();
    var soLuong = $('#editSoLuong').val().trim();
    var gia = $('#editGia').val().trim();

    if (!tenSP || !soLuong || !gia) {
        alert('Vui lòng nhập đầy đủ thông tin!');
        return;
    }

    $.ajax({
        url: '/SanPham/CapNhatSanPham',
        type: 'POST',
        data: {
            maSP: maSP,
            tenSP: tenSP,
            soLuong: soLuong,
            gia: gia
        },
        success: function(response) {
            if (response.success) {
                alert('Cập nhật sản phẩm thành công!');
                $('#editModal').modal('hide');
                location.reload();
            } else {
                alert('Cập nhật sản phẩm thất bại: ' + (response.message || 'Lỗi không xác định'));
            }
        },
        error: function(xhr, status, error) {
            console.error('Error:', error);
            alert('Có lỗi xảy ra khi cập nhật sản phẩm!');
        }
    });
} 