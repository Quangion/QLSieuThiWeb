function xemHangBanChay() {
    $.get('/BaoCao/GetSanPhamBanChay', function(response) {
        if (response.success) {
            var html = '';
            response.data.forEach(function(item) {
                html += `<tr>
                    <td>${item.stt}</td>
                    <td>${item.maSP}</td>
                    <td>${item.tenSP}</td>
                    <td>${item.soLuongBan}</td>
                </tr>`;
            });
            $('#hangBanChayData').html(html);
        } else {
            alert('Lỗi: ' + response.message);
        }
    });
}

function xemThongKeNgay() {
    var ngay = $('#ngayThongKe').val();
    if (!ngay) {
        alert('Vui lòng chọn ngày!');
        return;
    }

    $.get('/BaoCao/GetThongKeTheoNgay', { ngay: ngay }, function(response) {
        if (response.success) {
            var data = response.data;
            var html = `
                <div class="row">
                    <div class="col-md-4">
                        <div class="card bg-primary text-white">
                            <div class="card-body">
                                <h5>Số Hóa Đơn</h5>
                                <h3>${data.soHoaDon}</h3>
                            </div>
                        </div>
                    </div>
                    <div class="col-md-4">
                        <div class="card bg-success text-white">
                            <div class="card-body">
                                <h5>Tổng Số Lượng</h5>
                                <h3>${data.tongSoLuong}</h3>
                            </div>
                        </div>
                    </div>
                    <div class="col-md-4">
                        <div class="card bg-info text-white">
                            <div class="card-body">
                                <h5>Tổng Doanh Thu</h5>
                                <h3>${formatMoney(data.tongDoanhThu)}</h3>
                            </div>
                        </div>
                    </div>
                </div>`;
            $('#thongKeNgayData').html(html);
        } else {
            alert('Lỗi: ' + response.message);
        }
    });
}

function xemThongKeThang() {
    var thang = $('#thang').val();
    var nam = $('#nam').val();

    $.get('/BaoCao/GetThongKeTheoThang', { thang: thang, nam: nam }, function(response) {
        if (response.success) {
            var data = response.data;
            var html = `
                <div class="row">
                    <div class="col-md-4">
                        <div class="card bg-primary text-white">
                            <div class="card-body">
                                <h5>Số Hóa Đơn</h5>
                                <h3>${data.soHoaDon}</h3>
                            </div>
                        </div>
                    </div>
                    <div class="col-md-4">
                        <div class="card bg-success text-white">
                            <div class="card-body">
                                <h5>Tổng Số Lượng</h5>
                                <h3>${data.tongSoLuong}</h3>
                            </div>
                        </div>
                    </div>
                    <div class="col-md-4">
                        <div class="card bg-info text-white">
                            <div class="card-body">
                                <h5>Tổng Doanh Thu</h5>
                                <h3>${formatMoney(data.tongDoanhThu)}</h3>
                            </div>
                        </div>
                    </div>
                </div>`;
            $('#thongKeThangData').html(html);
        } else {
            alert('Lỗi: ' + response.message);
        }
    });
}

function xemDoanhThuThang() {
    var date = $('#thangDoanhThu').val();
    if (!date) {
        alert('Vui lòng chọn tháng!');
        return;
    }

    var thang = new Date(date).getMonth() + 1;
    var nam = new Date(date).getFullYear();

    $.get('/BaoCao/GetDoanhThuThang', { thang: thang, nam: nam }, function(response) {
        if (response.success) {
            var html = `
                <div class="alert alert-info">
                    Tổng doanh thu tháng ${response.data.thang}/${response.data.nam}: 
                    ${formatMoney(response.data.doanhThu)}
                </div>`;
            $('#doanhThuThangData').html(html);
        } else {
            alert('Lỗi: ' + response.message);
        }
    });
}

function xemDoanhThuNam() {
    var nam = $('#namDoanhThu').val();
    if (!nam) {
        alert('Vui lòng chọn năm!');
        return;
    }

    $.get('/BaoCao/GetDoanhThuNam', { nam: nam }, function(response) {
        if (response.success) {
            var html = `
                <div class="alert alert-info">
                    Tổng doanh thu năm ${response.data.nam}: 
                    ${formatMoney(response.data.doanhThu)}
                </div>`;
            $('#doanhThuNamData').html(html);
        } else {
            alert('Lỗi: ' + response.message);
        }
    });
}

function formatMoney(amount) {
    return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(amount);
} 