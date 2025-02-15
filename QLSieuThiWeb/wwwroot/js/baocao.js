function xemDoanhThuNgay() {
    let ngay = document.getElementById("ngayThongKe").value;
    fetch(`/BaoCao/GetDoanhThuNgay?ngay=${ngay}`)
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                document.getElementById("doanhThuNgayData").innerHTML = `<p>Doanh thu ngày ${ngay}: ${data.doanhThu} VND</p>`;
            } else {
                alert(data.message);
            }
        });
}

function xemDoanhThuThang() {
    let thang = document.getElementById("thang").value;
    let nam = document.getElementById("nam").value;
    fetch(`/BaoCao/GetDoanhThuThang?thang=${thang}&nam=${nam}`)
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                document.getElementById("doanhThuThangData").innerHTML = `<p>Doanh thu tháng ${thang}/${nam}: ${data.doanhThu} VND</p>`;
            } else {
                alert(data.message);
            }
        });
}

function xemDoanhThuNam() {
    let nam = document.getElementById("namDoanhThu").value;
    fetch(`/BaoCao/GetDoanhThuNam?nam=${nam}`)
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                document.getElementById("doanhThuNamData").innerHTML = `<p>Doanh thu năm ${nam}: ${data.doanhThu} VND</p>`;
            } else {
                alert(data.message);
            }
        });
}
