CREATE TABLE NguoiDung (
    MaNguoiDung INT PRIMARY KEY IDENTITY,
    HoTen NVARCHAR(100) NOT NULL,
    SoDienThoai NVARCHAR(20),
    Email NVARCHAR(100) UNIQUE,
    TenDangNhap NVARCHAR(50) UNIQUE,
    MatKhau NVARCHAR(255),
    VaiTro NVARCHAR(20) -- 'Admin', 'NhanVien', 'KhachHang'
);
CREATE TABLE Phim (
    MaPhim INT PRIMARY KEY IDENTITY,
    TenPhim NVARCHAR(100) NOT NULL,
    ThoiLuong INT NOT NULL,
    TheLoai NVARCHAR(50),
    QuocGia NVARCHAR(50),
    NamSX INT,
    NgayCongChieu DATE
);
CREATE TABLE PhongChieu (
    MaPhong INT PRIMARY KEY IDENTITY,
    TenPhong NVARCHAR(50) NOT NULL
);
CREATE TABLE Ghe (
    MaGhe INT PRIMARY KEY IDENTITY,
    MaPhong INT NOT NULL,
    Hang CHAR(1) NOT NULL,
    Cot INT NOT NULL,
    FOREIGN KEY (MaPhong) REFERENCES PhongChieu(MaPhong)
);
CREATE TABLE LichChieu (
    MaLichChieu INT PRIMARY KEY IDENTITY,
    MaPhim INT NOT NULL,
    MaPhong INT NOT NULL,
    NgayChieu DATE NOT NULL,
    GioChieu TIME NOT NULL,
    GiaVe DECIMAL(10,2) NOT NULL,
    FOREIGN KEY (MaPhim) REFERENCES Phim(MaPhim),
    FOREIGN KEY (MaPhong) REFERENCES PhongChieu(MaPhong)
);
CREATE TABLE DatVe (
    MaDatVe INT PRIMARY KEY IDENTITY,
    MaNguoiDung INT NOT NULL,
    NgayDat DATETIME DEFAULT GETDATE(),
    TrangThai NVARCHAR(20) DEFAULT N'ChuaThanhToan', -- 'DaThanhToan', 'Huy'
    FOREIGN KEY (MaNguoiDung) REFERENCES NguoiDung(MaNguoiDung)
);
CREATE TABLE ChiTietDatVe (
    MaChiTiet INT PRIMARY KEY IDENTITY,
    MaDatVe INT NOT NULL,
    MaLichChieu INT NOT NULL,
    MaGhe INT NOT NULL,
    FOREIGN KEY (MaDatVe) REFERENCES DatVe(MaDatVe),
    FOREIGN KEY (MaLichChieu) REFERENCES LichChieu(MaLichChieu),
    FOREIGN KEY (MaGhe) REFERENCES Ghe(MaGhe),
    UNIQUE(MaLichChieu, MaGhe) -- đảm bảo 1 ghế trong suất chiếu chỉ đặt 1 lần
);
