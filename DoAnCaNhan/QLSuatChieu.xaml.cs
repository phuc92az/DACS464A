using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace DoAnCaNhan
{
    /// <summary>
    /// Interaction logic for QLSuatChieu.xaml
    /// </summary>
    public partial class QLSuatChieu : Window
    {
        private readonly QLRapDBEntities1 _db = new QLRapDBEntities1();
        private int? _dangChonId = null;

        public QLSuatChieu()
        {
            InitializeComponent();
            LoadCombos();
            LoadGrid();
        }
        private void LoadCombos()
        {
            cboPhim.ItemsSource = _db.Phims.OrderBy(p => p.TenPhim).ToList();
            cboPhong.ItemsSource = _db.PhongChieux.OrderBy(p => p.TenPhong).ToList();
        }
        private void LoadGrid()
        {
            var data = (from lc in _db.LichChieux
                        join p in _db.Phims on lc.MaPhim equals p.MaPhim
                        join pc in _db.PhongChieux on lc.MaPhong equals pc.MaPhong
                        orderby lc.NgayChieu, lc.GioChieu
                        select new
                        {
                            lc.MaLichChieu,
                            lc.MaPhim,
                            TenPhim = p.TenPhim,
                            lc.MaPhong,
                            TenPhong = pc.TenPhong,
                            lc.NgayChieu,
                            GioChieu = lc.GioChieu.ToString().Substring(0, 5), // HH:mm
                            lc.GiaVe
                        }).ToList();

            dgSuatChieu.ItemsSource = data;
        }
        private bool TryParseInputs(out int maPhim, out int maPhong, out DateTime ngay, out TimeSpan gio, out decimal gia)
        {
            maPhim = maPhong = 0; ngay = DateTime.MinValue; gio = TimeSpan.Zero; gia = 0;

            if (cboPhim.SelectedValue == null || cboPhong.SelectedValue == null || !dpNgay.SelectedDate.HasValue)
            {
                MessageBox.Show("Vui lòng chọn Phim/Phòng/Ngày.");
                return false;
            }

            if (!TimeSpan.TryParseExact(txtGio.Text.Trim(), @"hh\:mm", CultureInfo.InvariantCulture, out gio))
            {
                MessageBox.Show("Giờ chiếu không hợp lệ (định dạng HH:mm).");
                return false;
            }

            var vi = CultureInfo.GetCultureInfo("vi-VN");
            if (!decimal.TryParse(txtGia.Text.Trim(), NumberStyles.Number, vi, out gia) || gia < 0)
            {
                MessageBox.Show("Giá vé không hợp lệ (ví dụ: 90.000).");
                return false;
            }

            maPhim = (int)cboPhim.SelectedValue;
            maPhong = (int)cboPhong.SelectedValue;
            ngay = dpNgay.SelectedDate.Value.Date;
            return true;
        }
        private bool BiChongGio(int maPhong, DateTime ngay, TimeSpan gioBatDau, int thoiLuongPhim, int bufferPhut, int? excludeId = null)
        {
            var startNew = ngay.Add(gioBatDau);
            var endNew = startNew.AddMinutes(thoiLuongPhim + bufferPhut);

            var cungNgay = (from lc in _db.LichChieux.Where(x => x.MaPhong == maPhong && x.NgayChieu == ngay)
                            join p in _db.Phims on lc.MaPhim equals p.MaPhim
                            select new
                            {
                                lc.MaLichChieu,
                                Start = ngay.Add(lc.GioChieu),
                                End = ngay.Add(lc.GioChieu).AddMinutes(p.ThoiLuong + bufferPhut)
                            }).ToList();

            foreach (var s in cungNgay.Where(x => !excludeId.HasValue || x.MaLichChieu != excludeId.Value))
            {
                bool overlap = !(s.End <= startNew || s.Start >= endNew);
                if (overlap) return true;
            }
            return false;
        }
        private void ResetForm()
        {
            _dangChonId = null;
            cboPhim.SelectedIndex = -1;
            cboPhong.SelectedIndex = -1;
            dpNgay.SelectedDate = null;
            txtGio.Clear();
            txtGia.Clear();
            dgSuatChieu.UnselectAll();
        }
        private void AddLichChieu(LichChieu lc)
        {
            dynamic ctx = _db;
            try { ctx.LichChieux.Add(lc); }        // DbContext
            catch { try { ctx.LichChieux.AddObject(lc); } catch { } } // ObjectContext
        }
        private void DeleteLichChieu(LichChieu lc)
        {
            dynamic ctx = _db;
            try { ctx.LichChieux.Remove(lc); }         // DbContext
            catch { try { ctx.LichChieux.DeleteObject(lc); } catch { } } // ObjectContext
        }
        private void Commit()
        {
            dynamic ctx = _db;
            try { ctx.SaveChanges(); }      // DbContext / ObjectContext
            catch { try { ctx.SubmitChanges(); } catch { throw; } } // LINQ to SQL (nếu có)
        }
        private static string GetDeepError(Exception ex)
        {
            while (ex.InnerException != null) ex = ex.InnerException;
            return ex.Message;
        }
        private void btnThem_Click(object sender, RoutedEventArgs e)
        {
            if (!TryParseInputs(out var maPhim, out var maPhong, out var ngay, out var gio, out var gia)) return;

            int thoiLuong = _db.Phims.Where(p => p.MaPhim == maPhim).Select(p => p.ThoiLuong).FirstOrDefault();
            int bufferPhut = 10;

            // slot trùng hẳn giờ
            if (_db.LichChieux.Any(l => l.MaPhong == maPhong && l.NgayChieu == ngay && l.GioChieu == gio))
            {
                MessageBox.Show("Đã có suất cùng phòng, ngày và giờ này.");
                return;
            }
            // chồng giờ theo thời lượng
            if (BiChongGio(maPhong, ngay, gio, thoiLuong, bufferPhut))
            {
                MessageBox.Show("Suất chiếu bị chồng giờ trong cùng phòng. Hãy chọn giờ khác.");
                return;
            }
            var lc = new LichChieu
            {
                MaPhim = maPhim,
                MaPhong = maPhong,
                NgayChieu = ngay,
                GioChieu = gio,
                GiaVe = gia
            };
            AddLichChieu(lc);

            try
            {
                Commit();
                LoadGrid();
                ResetForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show(GetDeepError(ex), "Lỗi lưu CSDL");
            }
            
        }
        private void btnSua_Click(object sender, RoutedEventArgs e)
        {
            if (_dangChonId == null)
            {
                MessageBox.Show("Chưa chọn suất chiếu để sửa.");
                return;
            }
            if (!TryParseInputs(out var maPhim, out var maPhong, out var ngay, out var gio, out var gia)) return;

            var lc = _db.LichChieux.FirstOrDefault(x => x.MaLichChieu == _dangChonId.Value);
            if (lc == null) return;

            int thoiLuong = _db.Phims.Where(p => p.MaPhim == maPhim).Select(p => p.ThoiLuong).FirstOrDefault();
            int bufferPhut = 10;

            if (_db.LichChieux.Any(l => l.MaPhong == maPhong && l.NgayChieu == ngay && l.GioChieu == gio && l.MaLichChieu != lc.MaLichChieu))
            {
                MessageBox.Show("Đã có suất cùng phòng, ngày và giờ này.");
                return;
            }
            if (BiChongGio(maPhong, ngay, gio, thoiLuong, bufferPhut, excludeId: lc.MaLichChieu))
            {
                MessageBox.Show("Suất chiếu bị chồng giờ trong cùng phòng.");
                return;
            }

            lc.MaPhim = maPhim;
            lc.MaPhong = maPhong;
            lc.NgayChieu = ngay;
            lc.GioChieu = gio;
            lc.GiaVe = gia;

            try
            {
                Commit();
                LoadGrid();
                ResetForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show(GetDeepError(ex), "Lỗi lưu CSDL");
            }
        }
        private void btnXoa_Click(object sender, RoutedEventArgs e)
        {
            if (_dangChonId == null)
            {
                MessageBox.Show("Chưa chọn suất chiếu để xoá.");
                return;
            }

            var lc = _db.LichChieux.FirstOrDefault(x => x.MaLichChieu == _dangChonId.Value);
            if (lc == null) return;

            // chặn xoá nếu đã có vé
            bool daCoVe = _db.ChiTietDatVes.Any(ct => ct.MaLichChieu == lc.MaLichChieu);
            if (daCoVe)
            {
                MessageBox.Show("Không thể xoá: Suất chiếu này đã có vé/chi tiết đặt vé.");
                return;
            }

            if (MessageBox.Show("Xoá suất chiếu này?", "Xác nhận", MessageBoxButton.YesNo) != MessageBoxResult.Yes)
                return;

            DeleteLichChieu(lc);

            try
            {
                Commit();
                LoadGrid();
                ResetForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show(GetDeepError(ex), "Lỗi lưu CSDL");
            }
        }
        private void btnMoi_Click(object sender, RoutedEventArgs e) => ResetForm();
        private void dgSuatChieu_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            dynamic row = dgSuatChieu.SelectedItem;
            if (row == null) return;

            _dangChonId = row.MaLichChieu;
            cboPhim.SelectedValue = row.MaPhim;
            cboPhong.SelectedValue = row.MaPhong;
            dpNgay.SelectedDate = row.NgayChieu;
            txtGio.Text = row.GioChieu; // chuỗi HH:mm
            txtGia.Text = row.GiaVe.ToString(CultureInfo.GetCultureInfo("vi-VN"));
        }
    } 
}
