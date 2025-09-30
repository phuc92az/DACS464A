using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace DoAnCaNhan
{
    /// <summary>
    /// Interaction logic for QLPhim.xaml
    /// </summary>
    public partial class QLPhim : Window
    {
        public QLPhim()
        {
            InitializeComponent();
            LoadPhim();
        }
        QLRapDBEntities1 db = new QLRapDBEntities1();


        private void btnLuu_Click(object sender, RoutedEventArgs e)
        {
            // Kiểm tra thông tin nhập vào
            if (string.IsNullOrWhiteSpace(txtTenPhim.Text) || string.IsNullOrWhiteSpace(txtThoiLuong.Text) || string.IsNullOrWhiteSpace(txtTheLoai.Text) || string.IsNullOrWhiteSpace(txtNamSX.Text) || string.IsNullOrWhiteSpace(txtNCC.Text))
            {
                MessageBox.Show("Vui lòng nhập đầy đủ thông tin!");
                return;
            }

            // Ép kiểu năm sản xuất
            if (!int.TryParse(txtNamSX.Text.Trim(), out int namSX))
            {
                MessageBox.Show("Năm sản xuất không hợp lệ.");
                return;
            }

            // Ép kiểu thời lượng
            if (!int.TryParse(txtThoiLuong.Text.Trim(), out int thoiLuong))
            {
                MessageBox.Show("Thời lượng không hợp lệ.");
                return;
            }

            // Tạo đối tượng Phim
            Phim ph = new Phim
            {
                TenPhim = txtTenPhim.Text.Trim(),
                ThoiLuong = thoiLuong,
                TheLoai = txtTheLoai.Text.Trim(),
                QuocGia = txtQuocGia.Text.Trim(),
                NamSX = namSX
            };

            // Kiểm tra và ép kiểu Ngày Công Chiếu
            DateTime ngayCongChieu;
            if (DateTime.TryParseExact(txtNCC.Text.Trim(), "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out ngayCongChieu))
            {
                ph.NgayCongChieu = ngayCongChieu;
            }
            else
            {
                MessageBox.Show("Ngày công chiếu không hợp lệ, vui lòng nhập theo định dạng dd/MM/yyyy.");
                return; // Dừng lại nếu ngày không hợp lệ
            }

            // Thêm phim vào cơ sở dữ liệu và lưu
            db.Phims.Add(ph);
            db.SaveChanges();

            MessageBox.Show("Thêm Phim thành công");
            LoadPhim(); // Cập nhật lại danh sách phim
        }
        public void LoadPhim()
        {
            DG_Phim.ItemsSource = db.Phims.ToList();
        }
        private void btnXoa_Click(object sender, RoutedEventArgs e)
        {
            // Lấy phim đã chọn trong DataGrid
            if (DG_Phim.SelectedItem is Phim selectedPhim)
            {
                // Xóa phim khỏi cơ sở dữ liệu
                db.Phims.Remove(selectedPhim);
                db.SaveChanges();

                MessageBox.Show("Phim đã được xóa.");
                LoadPhim();  // Cập nhật lại danh sách phim sau khi xóa
            }
            else
            {
                MessageBox.Show("Vui lòng chọn một phim để xóa.");
            }
        }

        private void btnSua_Click(object sender, RoutedEventArgs e)
        {
            // Lấy phim đã chọn trong DataGrid
            if (DG_Phim.SelectedItem is Phim selectedPhim)
            {
                // Cập nhật thông tin phim từ các TextBox
                selectedPhim.TenPhim = txtTenPhim.Text.Trim();
                selectedPhim.ThoiLuong = int.Parse(txtThoiLuong.Text.Trim());
                selectedPhim.TheLoai = txtTheLoai.Text.Trim();
                selectedPhim.QuocGia = txtQuocGia.Text.Trim();
                selectedPhim.NamSX = int.Parse(txtNamSX.Text.Trim());

                // Ép kiểu Ngày Công Chiếu từ chuỗi
                DateTime ngayCongChieu;
                if (DateTime.TryParseExact(txtNCC.Text.Trim(), "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out ngayCongChieu))
                {
                    selectedPhim.NgayCongChieu = ngayCongChieu;
                }
                else
                {
                    MessageBox.Show("Ngày công chiếu không hợp lệ.");
                    return; // Dừng lại nếu ngày không hợp lệ
                }

                // Lưu thay đổi vào cơ sở dữ liệu
                db.SaveChanges();

                MessageBox.Show("Cập nhật thông tin phim thành công.");
                LoadPhim();  // Cập nhật lại danh sách phim
            }
            else
            {
                MessageBox.Show("Vui lòng chọn một phim để sửa.");
            }
        }
    }
}
