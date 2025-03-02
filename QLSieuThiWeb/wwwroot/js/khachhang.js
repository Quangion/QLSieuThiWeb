// Khởi tạo các biến toàn cục
let customers = [];
const customerModal = new bootstrap.Modal(document.getElementById('customerModal'));
const toast = new bootstrap.Toast(document.getElementById('toast'));
const historyModal = new bootstrap.Modal(document.getElementById('historyModal'));

// Sự kiện khi trang được tải
document.addEventListener('DOMContentLoaded', function () {
    loadCustomers();

    // Tìm kiếm khách hàng
    document.getElementById('searchInput').addEventListener('input', function (e) {
        const searchText = e.target.value.toLowerCase();
        const filteredCustomers = customers.filter(customer =>
            (customer.maKH || '').toLowerCase().includes(searchText) ||
            (customer.tenKH || '').toLowerCase().includes(searchText) ||
            (customer.sdt || '').toLowerCase().includes(searchText) ||
            (customer.diaChi || '').toLowerCase().includes(searchText)
        );
        renderCustomers(filteredCustomers);
    });

    // Kiểm tra số điện thoại
    document.getElementById('sdt').addEventListener('input', function (e) {
        this.value = this.value.replace(/[^0-9]/g, '');
        if (this.value.length > 10) this.value = this.value.slice(0, 10);
        this.setCustomValidity(this.value.length === 10 ? '' : 'Số điện thoại phải có đúng 10 chữ số');
    });

    // Kiểm tra form
    document.getElementById('customerForm').addEventListener('submit', event => {
        if (!event.target.checkValidity()) {
            event.preventDefault();
            event.stopPropagation();
        }
        event.target.classList.add('was-validated');
    });
});

// Tải danh sách khách hàng
async function loadCustomers() {
    try {
        const response = await fetch('/KhachHang/GetAllCustomers');
        if (!response.ok) throw new Error('Không thể tải dữ liệu khách hàng');
        const data = await response.json();
        if (Array.isArray(data)) {
            customers = data;
            renderCustomers(customers);
        } else {
            throw new Error('Dữ liệu không hợp lệ');
        }
    } catch (error) {
        showToast(`Lỗi: ${error.message}`, 'bg-danger');
    }
}

// Hiển thị danh sách khách hàng trong bảng
function renderCustomers(data) {
    const tbody = document.getElementById('customerTableBody');
    tbody.innerHTML = data.map(customer => `
        <tr>
            <td class="ps-4">${customer.maKH || ''}</td>
            <td>${customer.tenKH || ''}</td>
            <td>${customer.sdt || ''}</td>
            <td>${customer.diaChi || ''}</td>
            <td class="text-center">
                <button class="btn btn-warning btn-sm me-2" onclick="openEditModal('${customer.maKH}')">
                    <i class="fas fa-edit"></i>
                </button>
                <button class="btn btn-danger btn-sm me-2" onclick="deleteCustomer('${customer.maKH}')">
                    <i class="fas fa-trash"></i>
                </button>
                <button class="btn btn-info btn-sm" onclick="openHistoryModal('${customer.maKH}')">
                    <i class="fas fa-eye"></i>
                </button>
            </td>
        </tr>
    `).join('');
}

// Mở modal thêm khách hàng
function openAddModal() {
    document.getElementById('modalTitle').textContent = 'Thêm Khách Hàng Mới';
    document.getElementById('customerForm').reset();
    document.getElementById('customerForm').classList.remove('was-validated');
    document.getElementById('maKH').removeAttribute('readonly');
    customerModal.show();
}

// Mở modal sửa khách hàng
function openEditModal(maKH) {
    const customer = customers.find(c => c.maKH === maKH);
    if (customer) {
        document.getElementById('modalTitle').textContent = 'Cập Nhật Khách Hàng';
        document.getElementById('customerForm').classList.remove('was-validated');
        document.getElementById('maKH').value = customer.maKH;
        document.getElementById('maKH').setAttribute('readonly', true);
        document.getElementById('tenKH').value = customer.tenKH || '';
        document.getElementById('sdt').value = customer.sdt || '';
        document.getElementById('diaChi').value = customer.diaChi || '';
        customerModal.show();
    } else {
        showToast('Không tìm thấy khách hàng.', 'bg-warning');
    }
}

// Lưu khách hàng (thêm hoặc sửa)
async function saveCustomer() {
    const form = document.getElementById('customerForm');
    if (!form.checkValidity()) {
        form.classList.add('was-validated');
        return;
    }

    const formData = {
        maKH: document.getElementById('maKH').value.trim(),
        tenKH: document.getElementById('tenKH').value.trim(),
        sdt: document.getElementById('sdt').value.trim(),
        diaChi: document.getElementById('diaChi').value.trim() || null
    };
    const isEditing = document.getElementById('maKH').hasAttribute('readonly');

    try {
        const url = isEditing ? '/KhachHang/UpdateCustomer' : '/KhachHang/AddCustomer';
        const response = await fetch(url, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(formData)
        });
        const result = await response.json();
        if (result.success) {
            showToast(result.message, 'bg-success');
            customerModal.hide();
            await loadCustomers();
        } else {
            showToast(result.message || 'Lỗi khi lưu khách hàng', 'bg-danger');
        }
    } catch (error) {
        showToast(`Lỗi: ${error.message}`, 'bg-danger');
    }
}

// Xóa khách hàng
async function deleteCustomer(maKH) {
    if (!confirm('Bạn có chắc chắn muốn xóa khách hàng này?')) return;
    try {
        const response = await fetch('/KhachHang/DeleteCustomer', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(maKH)
        });
        console.log(response.body);
        console.log(response);
        const result = await response.json();
        if (result.success) {
            showToast(result.message, 'bg-success');
            await loadCustomers();
        } else {
            showToast(result.message || 'Lỗi khi xóa khách hàng', 'bg-danger');
        }
    } catch (error) {
        showToast(`Lỗi: ${error.message}`, 'bg-danger');
    }
}

// Hiển thị thông báo
function showToast(message, bgClass = 'bg-success') {
    const toastElement = document.getElementById('toast');
    toastElement.className = `toast align-items-center text-white border-0 ${bgClass}`;
    document.getElementById('toastMessage').textContent = message;
    toast.show();
}

// Mở modal lịch sử mua hàng
async function openHistoryModal(maKH) {
    const customer = customers.find(c => c.maKH === maKH);
    if (!customer || !customer.sdt) {
        showToast('Không tìm thấy thông tin khách hàng.', 'bg-warning');
        return;
    }

    document.getElementById('historyModalTitle').textContent = `Lịch Sử Mua Hàng - ${customer.tenKH}`;

    try {
        const response = await fetch('/KhachHang/GetHoaDonBySDT', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(customer.sdt)
        });
        if (!response.ok) throw new Error('Không thể tải lịch sử mua hàng');
        const historyData = await response.json();
        if (!Array.isArray(historyData)) throw new Error('Dữ liệu không hợp lệ');
        renderHistory(historyData);
        historyModal.show();
    } catch (error) {
        showToast(`Lỗi: ${error.message}`, 'bg-danger');
    }
}
// Hiển thị lịch sử mua hàng
function renderHistory(historyData) {
    const accordion = document.getElementById('historyAccordion');
    accordion.innerHTML = historyData.length === 0
        ? `<div class="text-center my-2">
               <i class="fas fa-box-open fa-3x text-muted mb-3"></i><br>
               <span class="text-muted fst-italic">Không có lịch sử mua hàng.</span>
           </div>`
        : historyData.map((history, index) => {
            // Thử hiển thị thời gian mà không cần định dạng
            const rawTime = history.thoiGian || 'N/A'; // Hiển thị trực tiếp giá trị thoiGian hoặc 'N/A' nếu không có
            const formattedTotal = history.tongTien
                ? new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(history.tongTien)
                : 'N/A';
            return `
                <div class="accordion-item">
                    <h2 class="accordion-header" id="heading-${index}">
                        <button class="accordion-button collapsed" type="button" data-bs-toggle="collapse" 
                                data-bs-target="#history-item-${index}" aria-expanded="false" aria-controls="history-item-${index}">
                            Mã HĐ: ${history.maHD || 'N/A'} - Thời gian: ${rawTime}
                        </button>
                    </h2>
                    <div id="history-item-${index}" class="accordion-collapse collapse" 
                         aria-labelledby="heading-${index}" data-bs-parent="#historyAccordion">
                        <div class="accordion-body">
                            <p><strong>Tổng tiền:</strong> ${formattedTotal}</p>
                            <p><strong>Trạng thái:</strong> ${history.trangThai || 'N/A'}</p>
                        </div>
                    </div>
                </div>`;
        }).join('');
}