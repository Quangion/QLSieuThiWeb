let allProducts = [];
let products = [];

async function loadProducts() {
    $.ajax({
        url: '/Kho/GetAllProducts',
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
                html += '<td><button onclick="updateQuantity(\'' + item.maSP + '\')">Cập nhật</button></td>';
                html += '</tr>';
            });
            $('#productTable tbody').html(html);
        },
        error: function (xhr, status, error) {
            console.error('Error:', error);
        }
    });
}

function renderProductTable() {
    const tbody = document.getElementById('productTableBody');
    tbody.innerHTML = '';
    
    products.forEach(product => {
        tbody.innerHTML += `
            <tr>
                <td>${product.maSP}</td>
                <td>${product.tenSP}</td>
                <td>${product.soLuong}</td>
                <td>${product.gia}</td>
                <td>
                    <button class="btn btn-sm btn-primary me-2" onclick="openStockModal('${product.maSP}')">
                        <i class="fas fa-plus"></i> Nhập kho
                    </button>
                    <button class="btn btn-sm btn-danger" onclick="clearStock('${product.maSP}')">
                        <i class="fas fa-trash"></i> Xả kho
                    </button>
                </td>
            </tr>
        `;
    });
}

function searchProducts() {
    const searchText = document.getElementById('search-text').value.toLowerCase().trim();
    
    if (searchText === '') {
        products = [...allProducts];
    } else {
        products = allProducts.filter(product => 
            product.tenSP.toLowerCase().includes(searchText) ||
            product.maSP.toLowerCase().includes(searchText)
        );
    }
    
    renderProductTable();
}

function openStockModal(maSP) {
    const product = allProducts.find(p => p.maSP === maSP);
    if (product) {
        document.getElementById('MaSP').value = product.maSP;
        document.getElementById('TenSP').value = product.tenSP;
        document.getElementById('SoLuongHienTai').value = product.soLuong;
        document.getElementById('SoLuongNhap').value = '';
        
        const modal = new bootstrap.Modal(document.getElementById('stockModal'));
        modal.show();
    }
}

async function updateStock() {
    const maSP = document.getElementById('MaSP').value;
    const soLuongHienTai = parseInt(document.getElementById('SoLuongHienTai').value) || 0;
    const soLuongNhap = parseInt(document.getElementById('SoLuongNhap').value) || 0;
    
    if (soLuongNhap <= 0) {
        showToast('Số lượng nhập phải lớn hơn 0', 'error');
        return;
    }

    const tongSoLuong = soLuongHienTai + soLuongNhap;

    try {
        const response = await fetch('/Kho/UpdateStock', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({
                maSP: maSP,
                soLuong: tongSoLuong.toString()
            })
        });

        const result = await response.json();
        
        if (result.success) {
            showToast(result.message);
            await loadProducts();
            bootstrap.Modal.getInstance(document.getElementById('stockModal')).hide();
        } else {
            showToast(result.message, 'error');
        }
    } catch (error) {
        console.error('Error:', error);
        showToast('Có lỗi xảy ra khi cập nhật kho', 'error');
    }
}

async function clearStock(maSP) {
    if (!confirm('Bạn có chắc muốn xả kho sản phẩm này?')) {
        return;
    }

    try {
        const response = await fetch('/Kho/ClearStock', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(maSP)
        });

        const result = await response.json();
        
        if (result.success) {
            showToast(result.message);
            await loadProducts();
        } else {
            showToast(result.message, 'error');
        }
    } catch (error) {
        console.error('Error:', error);
        showToast('Có lỗi xảy ra khi xả kho', 'error');
    }
}

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

function capNhatSoLuong(maSP) {
    let soLuongHienTai = parseInt($(`tr[data-masp="${maSP}"]`).find('td:eq(2)').text());
    
    let soLuongMoi = prompt(`Số lượng hiện tại: ${soLuongHienTai}\nNhập số lượng mới:`);
    
    if (soLuongMoi != null) {
        if (!$.isNumeric(soLuongMoi) || parseInt(soLuongMoi) < 0) {
            alert('Vui lòng nhập số lượng hợp lệ!');
            return;
        }

        $.ajax({
            url: '/Kho/CapNhatSoLuong',
            type: 'POST',
            data: { 
                maSP: maSP, 
                soLuong: soLuongMoi 
            },
            success: function(response) {
                if (response.success) {
                    alert('Cập nhật thành công!');
                    location.reload();
                } else {
                    alert('Cập nhật thất bại: ' + response.message);
                }
            },
            error: function() {
                alert('Có lỗi xảy ra!');
            }
        });
    }
}

function timKiem() {
    let searchText = $('#searchInput').val().toLowerCase();
    $('table tbody tr').each(function() {
        let text = $(this).text().toLowerCase();
        $(this).toggle(text.indexOf(searchText) > -1);
    });
}

$(document).ready(function() {
    // Khởi tạo DataTable
    $('#khoTable').DataTable({
        "language": {
            "lengthMenu": "Hiển thị _MENU_ dòng",
            "zeroRecords": "Không tìm thấy dữ liệu",
            "info": "Hiển thị trang _PAGE_ / _PAGES_",
            "infoEmpty": "Không có dữ liệu",
            "infoFiltered": "(lọc từ _MAX_ dòng)",
            "search": "Tìm kiếm:",
            "paginate": {
                "first": "Đầu",
                "last": "Cuối",
                "next": "Sau",
                "previous": "Trước"
            }
        }
    });
});

document.addEventListener('DOMContentLoaded', function() {
    document.getElementById('search-text').addEventListener('input', searchProducts);
    loadProducts();
});

function showNhapKho(maSP) {
    // Lấy dòng hiện tại
    const currentRow = $(`#buttons_${maSP}`).closest('tr');
    const tenSP = currentRow.find('td:eq(1)').text(); // Lấy tên sản phẩm từ cột thứ 2
    const tonKho = currentRow.find('td:eq(2)').text(); // Lấy tồn kho từ cột thứ 3

    currentRow.find(`#buttons_${maSP}`).hide();
    currentRow.find(`#nhapKho_${maSP}`).show();
    currentRow.find(`#soLuongNhap_${maSP}`).focus();
}

function showXuatKho(maSP) {
    const currentRow = $(`#buttons_${maSP}`).closest('tr');
    const tenSP = currentRow.find('td:eq(1)').text();
    const tonKho = currentRow.find('td:eq(2)').text();

    currentRow.find(`#buttons_${maSP}`).hide();
    currentRow.find(`#xuatKho_${maSP}`).show();
    currentRow.find(`#soLuongXuat_${maSP}`).focus();
}

function huyNhapKho(maSP) {
    const currentRow = $(`#nhapKho_${maSP}`).closest('tr');
    currentRow.find(`#nhapKho_${maSP}`).hide();
    currentRow.find(`#soLuongNhap_${maSP}`).val('');
    currentRow.find(`#buttons_${maSP}`).show();
}

function huyXuatKho(maSP) {
    const currentRow = $(`#xuatKho_${maSP}`).closest('tr');
    currentRow.find(`#xuatKho_${maSP}`).hide();
    currentRow.find(`#soLuongXuat_${maSP}`).val('');
    currentRow.find(`#buttons_${maSP}`).show();
}

function nhapKho(maSP) {
    const currentRow = $(`#nhapKho_${maSP}`).closest('tr');
    const soLuongNhap = parseInt(currentRow.find(`#soLuongNhap_${maSP}`).val());

    if (!soLuongNhap || soLuongNhap <= 0) {
        alert('Vui lòng nhập số lượng hợp lệ!');
        return;
    }

    $.ajax({
        url: '/Kho/NhapKho',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({
            maSP: maSP,
            soLuongNhap: soLuongNhap
        }),
        success: function(response) {
            if (response.success) {
                alert('Nhập kho thành công!');
                huyNhapKho(maSP);
                loadDanhSachSanPham();
            } else {
                alert(response.message);
            }
        },
        error: function() {
            alert('Có lỗi xảy ra!');
        }
    });
}

function xuatKho(maSP) {
    const currentRow = $(`#xuatKho_${maSP}`).closest('tr');
    const soLuongXuat = parseInt(currentRow.find(`#soLuongXuat_${maSP}`).val());
    const tonKho = parseInt(currentRow.find('td:eq(2)').text());

    if (!soLuongXuat || soLuongXuat <= 0) {
        alert('Vui lòng nhập số lượng hợp lệ!');
        return;
    }

    if (soLuongXuat > tonKho) {
        alert('Số lượng xuất không được vượt quá số lượng tồn kho!');
        return;
    }

    $.ajax({
        url: '/Kho/XuatKho',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({
            maSP: maSP,
            soLuongXuat: soLuongXuat
        }),
        success: function(response) {
            if (response.success) {
                alert('Xuất kho thành công!');
                huyXuatKho(maSP);
                loadDanhSachSanPham();
            } else {
                alert(response.message);
            }
        },
        error: function() {
            alert('Có lỗi xảy ra!');
        }
    });
}

function loadDanhSachSanPham() {
    $.get('/Kho/GetDanhSachSanPham', function(response) {
        if (response.success) {
            let html = '';
            response.data.forEach(function(sp) {
                html += `
                    <tr>
                        <td>${sp.maSP}</td>
                        <td>${sp.tenSP}</td>
                        <td>${sp.soLuong}</td>
                        <td>${formatMoney(sp.gia)}</td>
                        <td>
                            <div class="input-group" style="display: none;" id="nhapKho_${sp.maSP}">
                                <input type="number" class="form-control form-control-sm" 
                                       id="soLuongNhap_${sp.maSP}" min="1" placeholder="Số lượng">
                                <button class="btn btn-success btn-sm" onclick="nhapKho('${sp.maSP}')">
                                    <i class="fas fa-check"></i>
                                </button>
                                <button class="btn btn-secondary btn-sm" onclick="huyNhapKho('${sp.maSP}')">
                                    <i class="fas fa-times"></i>
                                </button>
                            </div>
                            <div class="input-group" style="display: none;" id="xuatKho_${sp.maSP}">
                                <input type="number" class="form-control form-control-sm" 
                                       id="soLuongXuat_${sp.maSP}" min="1" placeholder="Số lượng">
                                <button class="btn btn-primary btn-sm" onclick="xuatKho('${sp.maSP}')">
                                    <i class="fas fa-check"></i>
                                </button>
                                <button class="btn btn-secondary btn-sm" onclick="huyXuatKho('${sp.maSP}')">
                                    <i class="fas fa-times"></i>
                                </button>
                            </div>
                            <div class="btn-group" id="buttons_${sp.maSP}">
                                <button type="button" class="btn btn-outline-success btn-sm" 
                                        onclick="showNhapKho('${sp.maSP}')">
                                    <i class="fas fa-plus"></i> Nhập
                                </button>
                                <button type="button" class="btn btn-outline-primary btn-sm" 
                                        onclick="showXuatKho('${sp.maSP}')">
                                    <i class="fas fa-minus"></i> Xuất
                                </button>
                            </div>
                        </td>
                    </tr>`;
            });
            $('tbody').html(html);
        }
    });
}

function formatMoney(amount) {
    return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(amount);
}

$(document).ready(function() {
    loadDanhSachSanPham();
}); 