let customers = [];
const customerModal = new bootstrap.Modal(document.getElementById('customerModal'));
const toast = new bootstrap.Toast(document.getElementById('toast'));

document.addEventListener('DOMContentLoaded', function() {
    loadCustomers();
    
    // Thêm sự kiện search
    document.getElementById('searchInput').addEventListener('input', function(e) {
        const searchText = e.target.value.toLowerCase();
        const filteredCustomers = customers.filter(customer => 
            (customer.maKH || '').toLowerCase().includes(searchText) ||
            (customer.tenKH || '').toLowerCase().includes(searchText) ||
            (customer.sdt || '').toLowerCase().includes(searchText) ||
            (customer.diaChi || '').toLowerCase().includes(searchText)
        );
        renderCustomers(filteredCustomers);
    });

    // Thêm validation cho input mã KH
    const maKHInput = document.getElementById('maKH');
    maKHInput.addEventListener('input', function(e) {
        const maKH = this.value.trim();
        if (!isEditing && isCustomerIdExists(maKH)) {
            this.setCustomValidity('Mã khách hàng đã tồn tại');
            showToast('Mã khách hàng đã tồn tại trong hệ thống', 'bg-danger');
        } else {
            this.setCustomValidity('');
        }
    });

    // Thêm validation cho input số điện thoại
    const sdtInput = document.getElementById('sdt');
    sdtInput.addEventListener('input', function(e) {
        // Chỉ cho phép nhập số
        this.value = this.value.replace(/[^0-9]/g, '');
        
        // Giới hạn độ dài 10 số
        if (this.value.length > 10) {
            this.value = this.value.slice(0, 10);
        }

        // Cập nhật validation message
        if (this.value.length > 0 && this.value.length !== 10) {
            this.setCustomValidity('Số điện thoại phải có đúng 10 chữ số');
        } else {
            this.setCustomValidity('');
        }
    });

    // Form validation
    const forms = document.querySelectorAll('.needs-validation');
    Array.from(forms).forEach(form => {
        form.addEventListener('submit', event => {
            if (!form.checkValidity()) {
                event.preventDefault();
                event.stopPropagation();
            }
            form.classList.add('was-validated');
        });
    });
});

async function loadCustomers() {
    try {
        const response = await fetch('/KhachHang/GetAllCustomers');
        if (!response.ok) throw new Error('Network response was not ok');
        
        const data = await response.json();
        if (Array.isArray(data)) {
            customers = data;
            renderCustomers(customers);
        }
    } catch (error) {
        console.error('Error:', error);
        showToast('Lỗi khi tải dữ liệu', 'bg-danger');
    }
}

function renderCustomers(data) {
    const tbody = document.getElementById('customerTableBody');
    tbody.innerHTML = data.map(customer => `
        <tr>
            <td class="ps-4">${customer.maKH || ''}</td>
            <td>${customer.tenKH || ''}</td>
            <td>${customer.sdt || ''}</td>
            <td>${customer.diaChi || ''}</td>
            <td class="text-center">
                <button class="btn btn-warning btn-sm btn-action me-2" onclick="openEditModal('${customer.maKH}')">
                    <i class="fas fa-edit"></i>
                </button>
                <button class="btn btn-danger btn-sm btn-action" onclick="deleteCustomer('${customer.maKH}')">
                    <i class="fas fa-trash"></i>
                </button>
            </td>
        </tr>
    `).join('');
}

function openAddModal() {
    document.getElementById('modalTitle').textContent = 'Thêm Khách Hàng Mới';
    document.getElementById('customerForm').reset();
    document.getElementById('customerForm').classList.remove('was-validated');
    document.getElementById('maKH').removeAttribute('readonly');
    customerModal.show();
}

function openEditModal(maKH) {
    const customer = customers.find(c => c.maKH === maKH);
    if (customer) {
        document.getElementById('modalTitle').textContent = 'Cập Nhật Khách Hàng';
        document.getElementById('customerForm').classList.remove('was-validated');
        document.getElementById('maKH').value = customer.maKH;
        document.getElementById('maKH').setAttribute('readonly', true);
        document.getElementById('tenKH').value = customer.tenKH;
        document.getElementById('sdt').value = customer.sdt;
        document.getElementById('diaChi').value = customer.diaChi || '';
        customerModal.show();
    }
}

// Thêm hàm kiểm tra số điện thoại
function isValidPhoneNumber(phone) {
    return /^[0-9]{10}$/.test(phone);
}

// Thêm hàm kiểm tra khách hàng tồn tại
function isCustomerExists(sdt) {
    return customers.some(customer => customer.sdt === sdt);
}

// Thêm hàm kiểm tra mã KH tồn tại
function isCustomerIdExists(maKH) {
    return customers.some(customer => customer.maKH === maKH);
}

// Cập nhật lại hàm saveCustomer
async function saveCustomer() {
    const form = document.getElementById('customerForm');
    const maKHInput = document.getElementById('maKH');
    const sdtInput = document.getElementById('sdt');
    const maKH = maKHInput.value.trim();
    const sdt = sdtInput.value.trim();

    // Reset validation
    form.classList.remove('was-validated');
    maKHInput.setCustomValidity('');
    sdtInput.setCustomValidity('');

    // Kiểm tra mã KH khi thêm mới
    const isEditing = maKHInput.hasAttribute('readonly');
    if (!isEditing && isCustomerIdExists(maKH)) {
        showToast('Mã khách hàng đã tồn tại trong hệ thống', 'bg-danger');
        maKHInput.setCustomValidity('Mã khách hàng đã tồn tại');
        form.classList.add('was-validated');
        return;
    }

    // Validate số điện thoại
    if (sdt === '') {
        sdtInput.setCustomValidity('Vui lòng nhập số điện thoại');
        form.classList.add('was-validated');
        return;
    }

    if (!isValidPhoneNumber(sdt)) {
        sdtInput.setCustomValidity('Số điện thoại phải có đúng 10 chữ số');
        form.classList.add('was-validated');
        return;
    }

    // Kiểm tra trùng số điện thoại khi thêm mới
    if (!isEditing && isCustomerExists(sdt)) {
        showToast('Số điện thoại đã tồn tại trong hệ thống', 'bg-danger');
        sdtInput.setCustomValidity('Số điện thoại đã tồn tại');
        form.classList.add('was-validated');
        return;
    }

    if (!form.checkValidity()) {
        form.classList.add('was-validated');
        return;
    }

    const formData = {
        maKH: maKH,
        tenKH: document.getElementById('tenKH').value,
        sdt: sdt,
        diaChi: document.getElementById('diaChi').value || null
    };

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
            showToast(result.message, 'bg-danger');
        }
    } catch (error) {
        console.error('Error:', error);
        showToast('Lỗi khi lưu dữ liệu', 'bg-danger');
    }
}

async function deleteCustomer(maKH) {
    if (!confirm('Bạn có chắc chắn muốn xóa khách hàng này?')) return;

    try {
        const response = await fetch('/KhachHang/DeleteCustomer', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(maKH)
        });

        const result = await response.json();
        
        if (result.success) {
            showToast(result.message, 'bg-success');
            await loadCustomers();
        } else {
            showToast(result.message, 'bg-danger');
        }
    } catch (error) {
        console.error('Error:', error);
        showToast('Lỗi khi xóa dữ liệu', 'bg-danger');
    }
}

function showToast(message, bgClass = 'bg-success') {
    const toastElement = document.getElementById('toast');
    toastElement.className = `toast align-items-center text-white border-0 ${bgClass}`;
    document.getElementById('toastMessage').textContent = message;
    toast.show();
} 