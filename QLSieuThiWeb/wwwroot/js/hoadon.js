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
function timKiemHoaDon() {
    var maHD = $('#searchMaHD').val().trim();

    // Nếu bạn muốn hiển thị tất cả hóa đơn khi ô tìm kiếm trống, bạn có thể bỏ đoạn kiểm tra này
    // Nếu không muốn gửi yêu cầu khi ô tìm kiếm trống, bạn có thể thêm đoạn kiểm tra dưới đây
    if (maHD === '') {
        // Xóa kết quả hiện tại hoặc hiển thị tất cả hóa đơn
        $('table tbody').html(''); // Hoặc hiển thị danh sách hóa đơn mặc định
        return;
    }

    $.ajax({
        url: '/QuanLyHoaDon/TimKiemHoaDon',
        type: 'GET',
        data: { maHD: maHD },
        success: function (response) {
            if (response.success && response.data.length > 0) {
                let html = '';
                response.data.forEach(function (item) {
                    html += `<tr>
                        <td>${item.maHD}</td>
                        <td>${new Date(item.thoiGian).toLocaleString('vi-VN')}</td>
                        <td>${item.sdt}</td>
                        <td>${item.tongTien.toLocaleString('vi-VN', { style: 'currency', currency: 'VND' })}</td>
                        <td><span class="badge ${item.trangThai == 'Đã giao' ? 'bg-success' : 'bg-warning'}">${item.trangThai}</span></td>
                        <td>
                            <button class="btn btn-outline-primary btn-sm" onclick="xemChiTiet('${item.maHD}')">
                                <i class="fas fa-eye"></i>
                            </button>
                            ${item.trangThai != 'Đã giao' ? `<button class="btn btn-outline-success btn-sm" onclick="capNhatTrangThai('${item.maHD}')">
                                <i class="fas fa-truck"></i>
                            </button>` : ''}
                        </td>
                    </tr>`;
                });
                $('table tbody').html(html);
            } else {
                // Nếu không tìm thấy hóa đơn, hiển thị thông báo
                $('table tbody').html('<tr><td colspan="6" class="text-center">Không tìm thấy hóa đơn</td></tr>');
            }
        },
        error: function () {
            alert('Đã xảy ra lỗi!');
        }
    });
}

