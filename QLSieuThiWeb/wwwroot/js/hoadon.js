let products = [];
let gioHang = [];
const toast = new bootstrap.Toast(document.getElementById('toast'));

document.addEventListener('DOMContentLoaded', async function() {
    await loadProducts();
    renderProducts();
    setupEventListeners();
    
    // Thêm sự kiện cho input số lượng
    document.getElementById('soLuong').addEventListener('input', function(e) {
        if (this.value < 1) this.value = 1;
    });

    // Thêm sự kiện cho input SĐT
    document.getElementById('sdtKhachHang').addEventListener('input', function(e) {
        this.value = this.value.replace(/[^\d]/g, '');
        if (this.value.length > 10) {
            this.value = this.value.slice(0, 10);
        }
    });

    // Thêm sự kiện search sản phẩm
    document.getElementById('searchInput').addEventListener('input', function(e) {
        const searchText = e.target.value.toLowerCase();
        const filteredProducts = products.filter(product => 
            product.maSP.toLowerCase().includes(searchText) ||
            product.tenSP.toLowerCase().includes(searchText)
        );
        renderProducts(filteredProducts);
    });
});

async function loadProducts() {
    try {
        const response = await fetch('/HoaDon/GetProducts');
        if (!response.ok) throw new Error('Network response was not ok');
        products = await response.json();
        console.log('Loaded products:', products);
    } catch (error) {
        console.error('Error loading products:', error);
        showToast('Lỗi khi tải danh sách sản phẩm', 'bg-danger');
    }
}

function renderProducts(data) {
    const container = document.getElementById('danhSachSanPham');
    const template = document.getElementById('sanPhamTemplate');
    
    container.innerHTML = '';
    data.forEach(product => {
        const productElement = document.importNode(template.content, true);
        
        // Thay thế các placeholder
        productElement.innerHTML = productElement.innerHTML
            .replace('{{tenSP}}', product.tenSP)
            .replace('{{maSP}}', product.maSP)
            .replace('{{donGia}}', formatCurrency(product.donGia))
            .replace('{{soLuong}}', product.soLuong);
            
        container.appendChild(productElement);
    });
}

function tangSoLuong(btn) {
    const input = btn.parentElement.querySelector('input');
    const max = parseInt(input.max);
    const currentValue = parseInt(input.value);
    if (currentValue < max) {
        input.value = currentValue + 1;
    }
}

function giamSoLuong(btn) {
    const input = btn.parentElement.querySelector('input');
    const currentValue = parseInt(input.value);
    if (currentValue > 1) {
        input.value = currentValue - 1;
    }
}

function themVaoGio(btn) {
    const productElement = btn.closest('.list-group-item');
    const maSP = productElement.querySelector('small').textContent.replace('Mã: ', '');
    const soLuong = parseInt(productElement.querySelector('input').value);
    
    const product = products.find(p => p.maSP === maSP);
    if (!product) return;

    // Kiểm tra sản phẩm đã có trong giỏ chưa
    const existingItem = gioHang.find(item => item.maSP === maSP);
    if (existingItem) {
        existingItem.soLuong += soLuong;
        existingItem.tongTien = existingItem.soLuong * existingItem.donGia;
    } else {
        gioHang.push({
            maSP: product.maSP,
            tenSP: product.tenSP,
            donGia: product.donGia,
            soLuong: soLuong,
            tongTien: product.donGia * soLuong
        });
    }

    renderGioHang();
    showToast('Đã thêm vào giỏ hàng');
}

function renderGioHang() {
    const tbody = document.querySelector('#gioHangTable tbody');
    tbody.innerHTML = '';
    
    gioHang.forEach((item, index) => {
        const row = tbody.insertRow();
        row.innerHTML = `
            <td>${item.maSP}</td>
            <td>${item.tenSP}</td>
            <td>${formatCurrency(item.donGia)}</td>
            <td>${item.soLuong}</td>
            <td>${formatCurrency(item.tongTien)}</td>
            <td>
                <button class="btn btn-danger btn-sm" onclick="xoaKhoiGio(${index})">Xóa</button>
            </td>
        `;
    });
}

function xoaKhoiGio(index) {
    gioHang.splice(index, 1);
    renderGioHang();
    showToast('Đã xóa sản phẩm khỏi giỏ');
}

function formatCurrency(amount) {
    return new Intl.NumberFormat('vi-VN', {
        style: 'currency',
        currency: 'VND'
    }).format(amount);
}

function showToast(message, bgClass = 'bg-success') {
    const toastElement = document.getElementById('toast');
    toastElement.className = `toast align-items-center text-white border-0 ${bgClass}`;
    document.getElementById('toastMessage').textContent = message;
    const toast = new bootstrap.Toast(toastElement);
    toast.show();
}

// Thiết lập các sự kiện
function setupEventListeners() {
    const maSPInput = document.getElementById("maSP");
    console.log('maSPInput:', maSPInput); // Kiểm tra đã lấy được element

    // Sự kiện khi gõ
    maSPInput.addEventListener("input", async function() {
        const maSP = this.value.trim();
        if (maSP) {
            // Log để kiểm tra giá trị
            console.log('Searching for:', maSP);
            
            // Gọi API
            fetch(`/HoaDon/GetProductByMa?maSP=${encodeURIComponent(maSP)}`)
                .then(response => {
                    // Log response
                    console.log('API Response:', response);
                    return response.json();
                })
                .then(data => {
                    // Log data
                    console.log('Data received:', data);
                    
                    if (data.success) {
                        // Điền thông tin sản phẩm
                        document.getElementById("tenSP").value = data.product.tenSP;
                        document.getElementById("donGia").value = formatCurrency(data.product.donGia);
                        
                        // Log kết quả điền
                        console.log('Filled product info:', {
                            tenSP: data.product.tenSP,
                            donGia: data.product.donGia
                        });
                    }
                })
                .catch(error => {
                    console.error('Error:', error);
                });
        }
    });

    // Xử lý khi nhập tên sản phẩm
    document.getElementById("tenSP").addEventListener("change", function() {
        let tenSP = this.value.trim();
        if (!tenSP) return;

        fetch(`/HoaDon/GetProductByTen?tenSP=${encodeURIComponent(tenSP)}`)
            .then(response => response.json())
            .then(data => {
                console.log('API response:', data); // Kiểm tra response
                if (data.success) {
                    document.getElementById("maSP").value = data.product.maSP;
                    document.getElementById("donGia").value = formatCurrency(data.product.donGia);
                    document.getElementById("soLuong").max = data.product.soLuong;
                } else {
                    showToast("Sản phẩm không tồn tại!", "bg-warning");
                    this.value = '';
                    document.getElementById("maSP").value = '';
                    document.getElementById("donGia").value = '';
                }
            })
            .catch(error => {
                console.error("Lỗi:", error);
                showToast("Lỗi khi tìm sản phẩm", "bg-danger");
            });
    });

    // Click vào dòng trong bảng giỏ hàng
    document.getElementById('cartTableBody').addEventListener('click', function(e) {
        const row = e.target.closest('tr');
        if (!row) return;
        
        const cells = row.cells;
        document.getElementById('maSP').value = cells[0].textContent;
        document.getElementById('tenSP').value = cells[1].textContent;
        document.getElementById('donGia').value = cells[2].textContent;
    });
}

// Hàm điền thông tin sản phẩm vào form
function fillProductInfo(product) {
    document.getElementById("tenSP").value = product.tenSP;
    document.getElementById("donGia").value = formatCurrency(product.donGia);
    if (document.getElementById("soLuong")) {
        document.getElementById("soLuong").max = product.soLuong;
    }
    console.log('Filled product info:', product); // Kiểm tra việc điền thông tin
}

// Hàm xóa thông tin sản phẩm
function clearProductInfo() {
    document.getElementById("tenSP").value = '';
    document.getElementById("donGia").value = '';
    if (document.getElementById("soLuong")) {
        document.getElementById("soLuong").value = '1';
    }
}

// Thêm vào giỏ hàng
async function themVaoGio() {
    const maSP = document.getElementById('maSP').value.trim();
    const soLuong = parseInt(document.getElementById('soLuong').value);
    
    if (!maSP) {
        showToast('Hãy nhập đầy đủ thông tin sản phẩm!', 'bg-warning');
        return;
    }

    try {
        // Kiểm tra số lượng tồn
        const response = await fetch(`/HoaDon/CheckSoLuong?maSP=${maSP}&soLuong=${soLuong}`);
        const data = await response.json();
        
        if (!data.success) {
            showToast(data.message, 'bg-warning');
            return;
        }

        // Kiểm tra sản phẩm đã có trong giỏ
        const existingIndex = KiemTraSPTrongHoaDon(maSP);
        
        if (existingIndex === -1) {
            // Thêm mới
            gioHang.push({
                maSP: maSP,
                tenSP: document.getElementById('tenSP').value,
                donGia: parseCurrency(document.getElementById('donGia').value),
                soLuong: soLuong,
                tongTien: parseCurrency(document.getElementById('donGia').value) * soLuong
            });
        } else {
            // Cộng thêm số lượng
            gioHang[existingIndex].soLuong += soLuong;
            gioHang[existingIndex].tongTien = gioHang[existingIndex].donGia * gioHang[existingIndex].soLuong;
        }

        renderGioHang();
        clearHD();
        tinhTong();
    } catch (error) {
        console.error('Error:', error);
        showToast('Lỗi khi thêm vào giỏ', 'bg-danger');
    }
}

// Các hàm hỗ trợ
function KiemTraSPTrongHoaDon(maSP) {
    return gioHang.findIndex(item => item.maSP === maSP);
}

function clearHD() {
    document.getElementById('maSP').value = '';
    document.getElementById('tenSP').value = '';
    document.getElementById('donGia').value = '';
    document.getElementById('soLuong').value = '1';
}

function tinhTong() {
    const tongTien = gioHang.reduce((sum, item) => sum + item.tongTien, 0);
    document.getElementById('tongTien').value = formatCurrency(tongTien);
}

// Sửa sản phẩm trong giỏ
function suaSanPhamGio(maSP) {
    const item = gioHang.find(item => item.maSP === maSP);
    if (!item) return;

    // Đưa thông tin lên form
    document.getElementById('maSP').value = item.maSP;
    document.getElementById('tenSP').value = item.maSP;
    document.getElementById('soLuong').value = item.soLuong;
    document.getElementById('donGia').value = formatCurrency(item.donGia);

    // Xóa sản phẩm khỏi giỏ
    gioHang = gioHang.filter(i => i.maSP !== maSP);
    renderGioHang();
}

// Xử lý nhập tiền khách trả
document.getElementById('khachTra').addEventListener('input', function(e) {
    formatInputCurrency(this);
    tinhTienThua();
});

// Format tiền tệ khi nhập
function formatInputCurrency(input) {
    // Lấy giá trị chỉ có số
    let value = input.value.replace(/[^\d]/g, '');
    
    // Nếu có giá trị thì format
    if (value) {
        const number = parseInt(value);
        input.value = formatCurrency(number);
    }
}

// Tính tiền thừa
function tinhTienThua() {
    const tongTien = parseCurrency(document.getElementById('tongTien').value);
    const khachTra = parseCurrency(document.getElementById('khachTra').value);
    
    const traLaiInput = document.getElementById('traLai');
    
    if (khachTra && tongTien) {
        if (khachTra >= tongTien) {
            const tienThua = khachTra - tongTien;
            traLaiInput.value = formatCurrency(tienThua);
            traLaiInput.classList.remove('text-danger');
        } else {
            const thieu = tongTien - khachTra;
            traLaiInput.value = '-' + formatCurrency(thieu);
            traLaiInput.classList.add('text-danger');
        }
    } else {
        traLaiInput.value = '';
        traLaiInput.classList.remove('text-danger');
    }
}

// Xử lý thanh toán
async function thanhToan() {
    if (gioHang.length === 0) {
        showToast('Giỏ hàng trống!', 'bg-warning');
        return;
    }

    const sdtKH = document.getElementById('sdtKhachHang').value;
    if (!sdtKH) {
        showToast('Vui lòng nhập số điện thoại khách hàng', 'bg-warning');
        return;
    }

    const khachTraStr = document.getElementById('khachTra').value;
    const tongTienStr = document.getElementById('tongTien').value;
    const khachTra = parseCurrency(khachTraStr);
    const tongTien = parseCurrency(tongTienStr);

    if (!khachTra) {
        showToast('Vui lòng nhập số tiền khách trả', 'bg-warning');
        return;
    }

    if (khachTra < tongTien) {
        showToast('Số tiền khách trả không đủ', 'bg-warning');
        return;
    }

    const trangThai = document.getElementById('hangChuaGiao').checked ? 'chưa giao' : 'đã giao';

    try {
        const response = await fetch('/HoaDon/ThanhToan', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({
                SDT: sdtKH,
                TrangThai: trangThai,
                TongTien: tongTien.toString(),
                GioHang: gioHang.map(item => ({
                    maSP: item.maSP,
                    soLuong: item.soLuong,
                    donGia: item.donGia
                }))
            })
        });

        const result = await response.json();

        if (result.success) {
            showToast('Thanh toán thành công!', 'bg-success');
            
            // Reset form và giỏ hàng
            gioHang = [];
            renderGioHang();
            resetProductForm();
            document.getElementById('sdtKhachHang').value = '';
            document.getElementById('khachTra').value = '';
            document.getElementById('traLai').value = '';
            document.getElementById('hangChuaGiao').checked = false;

            // In hóa đơn nếu cần
            if (confirm('Bạn có muốn in hóa đơn không?')) {
                window.open(`/HoaDon/InHoaDon/${result.maHD}`, '_blank');
            }
        } else {
            showToast(result.message || 'Lỗi khi thanh toán', 'bg-danger');
        }
    } catch (error) {
        console.error('Error:', error);
        showToast('Lỗi khi thanh toán', 'bg-danger');
    }
}

// Reset form sản phẩm
function resetProductForm() {
    const maSPSelect = document.getElementById('maSP');
    const tenSPSelect = document.getElementById('tenSP');
    const donGiaInput = document.getElementById('donGia');
    const soLuongInput = document.getElementById('soLuong');
    
    maSPSelect.value = '';
    tenSPSelect.value = '';
    donGiaInput.value = '';
    soLuongInput.value = '1';
}

// Parse tiền tệ về số
function parseCurrency(currencyString) {
    return parseFloat(currencyString.replace(/[^\d]/g, ''));
}

// Load danh sách sản phẩm
function loadSanPham() {
    $.get('/HoaDon/GetSanPham', function(data) {
        var html = '';
        $.each(data, function(i, item) {
            html += `
                <div class="list-group-item">
                    <div class="d-flex justify-content-between align-items-center">
                        <div>
                            <h6 class="mb-1">${item.tenSP}</h6>
                            <small>Mã: ${item.maSP}</small>
                            <p class="mb-1">Giá: ${formatMoney(item.gia)}đ</p>
                        </div>
                        <div class="input-group" style="width: 150px;">
                            <input type="number" class="form-control form-control-sm" value="1" min="1">
                            <button class="btn btn-primary btn-sm" onclick="themVaoGio('${item.maSP}', this)">
                                <i class="fas fa-plus"></i>
                            </button>
                        </div>
                    </div>
                </div>
            `;
        });
        $('#danhSachSanPham').html(html);
    });
}

// Thêm vào giỏ
function themVaoGio(maSP, button) {
    let soLuong = parseInt($(button).prev('input').val());
    let maxSoLuong = parseInt($(button).prev('input').attr('max'));
    
    if (soLuong > maxSoLuong) {
        alert('Số lượng vượt quá tồn kho!');
        return;
    }

    $.get('/HoaDon/GetSanPhamByMa/' + maSP, function(sp) {
        // Kiểm tra sản phẩm đã có trong giỏ chưa
        let existingItem = gioHang.find(item => item.maSP === maSP);
        if (existingItem) {
            if (existingItem.soLuong + soLuong > maxSoLuong) {
                alert('Tổng số lượng vượt quá tồn kho!');
                return;
            }
            existingItem.soLuong += soLuong;
            existingItem.tongTien = existingItem.soLuong * sp.gia;
        } else {
            gioHang.push({
                maSP: sp.maSP,
                tenSP: sp.tenSP,
                gia: sp.gia,
                soLuong: soLuong,
                tongTien: soLuong * sp.gia
            });
        }
        capNhatGioHang();
    });
}

// Cập nhật hiển thị giỏ hàng
function capNhatGioHang() {
    let html = '';
    let tongTien = 0;
    
    gioHang.forEach((item, index) => {
        html += `
            <tr>
                <td>${index + 1}</td>
                <td>${item.maSP}</td>
                <td>${item.tenSP}</td>
                <td>${formatMoney(item.gia)}đ</td>
                <td>${item.soLuong}</td>
                <td>${formatMoney(item.tongTien)}đ</td>
                <td>
                    <button class="btn btn-danger btn-sm" onclick="xoaSanPham(${index})">
                        <i class="fas fa-trash"></i>
                    </button>
                </td>
            </tr>
        `;
        tongTien += item.tongTien;
    });

    $('#gioHangTable tbody').html(html);
    $('#tongTien').text(formatMoney(tongTien));
}

// Xóa sản phẩm khỏi giỏ
function xoaSanPham(index) {
    gioHang.splice(index, 1);
    capNhatGioHang();
}

// Format tiền
function formatMoney(amount) {
    return amount.toString().replace(/\B(?=(\d{3})+(?!\d))/g, ",");
}

// Tính tiền thừa
function tinhTienThua() {
    let tongTien = parseFloat($('#tongTien').text().replace(/[,.đ]/g, ''));
    let tienKhachDua = parseFloat($('#tienKhachDua').val() || 0);
    let tienThua = tienKhachDua - tongTien;
    $('#tienThua').val(formatMoney(tienThua > 0 ? tienThua : 0) + 'đ');
}

// Thanh toán
function thanhToanNgay() {
    if (gioHang.length === 0) {
        alert('Giỏ hàng trống!');
        return;
    }
    
    let sdtKH = $('#sdtKH').val();
    let tongTien = parseFloat($('#tongTien').text().replace(/[,.đ]/g, ''));
    let tienKhachDua = parseFloat($('#tienKhachDua').val() || 0);

    if (tienKhachDua < tongTien) {
        alert('Số tiền khách đưa không đủ!');
        return;
    }

    // Gửi dữ liệu lên server
    $.ajax({
        url: '/HoaDon/ThanhToan',
        type: 'POST',
        data: {
            sdtKH: sdtKH,
            tongTien: tongTien,
            chiTietHoaDon: gioHang,
            hinhThucThanhToan: 'ThanhToanNgay'
        },
        success: function(response) {
            if (response.success) {
                alert('Thanh toán thành công!');
                gioHang = [];
                capNhatGioHang();
                $('#sdtKH').val('');
                $('#tienKhachDua').val('');
                $('#tienThua').val('');
            } else {
                alert('Thanh toán thất bại!');
            }
        }
    });
} 