function xemChiTiet(maHD) {
    $.ajax({
        url: '/QuanLyHoaDon/ChiTietHoaDon',
        type: 'GET',
        data: { maHD: maHD },
        success: function (response) {
            if (response.success) {
                let html = '';
                response.data.forEach(function (item) {
                    html += `<tr>
                        <td>${item.maSP}</td>
                        <td>${item.slMua}</td>
                        <td>${formatMoney(item.tongTienSP)}</td>
                    </tr>`;
                });
                $('#chiTietHoaDon').html(html);
                $('#modalChiTiet').modal('show');
            } else {
                alert('Lỗi khi tải chi tiết hóa đơn: ' + response.message);
            }
        },
        error: function () {
            alert('Đã xảy ra lỗi!');
        }
    });
}

function capNhatTrangThai(maHD) {
    if (confirm('Xác nhận đã giao hàng?')) {
        $.ajax({
            url: '/QuanLyHoaDon/CapNhatTrangThai',
            type: 'POST',
            data: { maHD: maHD },
            success: function (response) {
                if (response.success) {
                    alert('Cập nhật trạng thái thành công!');
                    location.reload();
                } else {
                    alert('Lỗi: ' + response.message);
                }
            },
            error: function () {
                alert('Đã xảy ra lỗi!');
            }
        });
    }
}

function formatMoney(amount) {
    return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(amount);
} 