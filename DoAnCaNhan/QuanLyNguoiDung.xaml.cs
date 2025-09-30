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
using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;
namespace DoAnCaNhan
{
    /// <summary>
    /// Interaction logic for QuanLyNguoiDung.xaml
    /// </summary>
    public partial class QuanLyNguoiDung : Window
    {
        private SqlConnection conn;
        private SqlDataAdapter da;
        private DataTable dt;

        public QuanLyNguoiDung()
        {
            InitializeComponent();
            conn = new SqlConnection(
             @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=|DataDirectory|\QLRapDB.mdf;Integrated Security=True");

            // hoặc @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=C:\Path\RapChieuPhim.mdf;Integrated Security=True"

            LoadNguoiDung();
        }
        private void LoadNguoiDung(string keyword = "", string vaiTro = "")
        {
            try
            {
                string sql = "SELECT MaNguoiDung, HoTen, SoDienThoai, Email, TenDangNhap, VaiTro FROM NguoiDung WHERE 1=1";

                if (!string.IsNullOrWhiteSpace(keyword))
                {
                    sql += " AND (HoTen LIKE @kw OR SoDienThoai LIKE @kw OR Email LIKE @kw)";
                }
                if (!string.IsNullOrWhiteSpace(vaiTro) && vaiTro != "Tất cả")
                {
                    sql += " AND VaiTro = @vaiTro";
                }

                da = new SqlDataAdapter(sql, conn);
                if (!string.IsNullOrWhiteSpace(keyword))
                    da.SelectCommand.Parameters.AddWithValue("@kw", "%" + keyword + "%");
                if (!string.IsNullOrWhiteSpace(vaiTro) && vaiTro != "Tất cả")
                    da.SelectCommand.Parameters.AddWithValue("@vaiTro", vaiTro);

                dt = new DataTable();
                da.Fill(dt);
                dgNguoiDung.ItemsSource = dt.DefaultView;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi Load: " + ex.Message);
            }
        }
        private void btnTimKiem_Click(object sender, RoutedEventArgs e)
        {
            string key = txtTimKiem.Text.Trim();
            string vt = (cbLocVaiTro.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Tất cả";
            LoadNguoiDung(key, vt);
        }

        private void cbLocVaiTro_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            btnTimKiem_Click(null, null);
        }
        private void dgNguoiDung_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgNguoiDung.SelectedItem == null) return;
            DataRowView row = dgNguoiDung.SelectedItem as DataRowView;
            if (row == null) return;

            txtHoTen.Text = row["HoTen"].ToString();
            txtSDT.Text = row["SoDienThoai"].ToString();
            txtEmail.Text = row["Email"].ToString();
            txtTenDN.Text = row["TenDangNhap"].ToString();
            // Không đặt mật khẩu lên giao diện; để trống nếu không đổi
            var role = row["VaiTro"].ToString();
            if (role == "NhanVien") cbVaiTro.SelectedIndex = 1; else cbVaiTro.SelectedIndex = 0;
        }
        private void btnMoi_Click(object sender, RoutedEventArgs e)
        {
            ClearForm();
            LoadNguoiDung();

        }
        private void ClearForm()
        {
            txtHoTen.Clear();
            txtSDT.Clear();
            txtEmail.Clear();
            txtTenDN.Clear();
            pwdMatKhau.Password = "";
            cbVaiTro.SelectedIndex = 0;
            dgNguoiDung.SelectedIndex = -1;
        }
        private bool ValidateForm(bool checkLoginFields)
        {
            if (string.IsNullOrWhiteSpace(txtHoTen.Text))
            {
                MessageBox.Show("Họ tên không được rỗng.");
                return false;
            }
            if (checkLoginFields)
            {
                if (string.IsNullOrWhiteSpace(txtTenDN.Text))
                {
                    MessageBox.Show("Tên đăng nhập không được rỗng.");
                    return false;
                }
                if (string.IsNullOrWhiteSpace(pwdMatKhau.Password))
                {
                    MessageBox.Show("Mật khẩu không được rỗng.");
                    return false;
                }
            }
            return true;
        }
        private void btnThem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Nếu vai trò là KhachHang, TenDangNhap/MatKhau có thể để null (tuỳ yêu cầu)
                bool requireLogin = (cbVaiTro.SelectedItem as ComboBoxItem)?.Content?.ToString() == "NhanVien";
                if (!ValidateForm(requireLogin)) return;

                string sql = "INSERT INTO NguoiDung(HoTen, SoDienThoai, Email, TenDangNhap, MatKhau, VaiTro) " +
                             "VALUES (@HoTen, @SDT, @Email, @TenDN, @MatKhau, @VaiTro)";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@HoTen", txtHoTen.Text.Trim());
                    cmd.Parameters.AddWithValue("@SDT", string.IsNullOrWhiteSpace(txtSDT.Text) ? (object)DBNull.Value : txtSDT.Text.Trim());
                    cmd.Parameters.AddWithValue("@Email", string.IsNullOrWhiteSpace(txtEmail.Text) ? (object)DBNull.Value : txtEmail.Text.Trim());
                    cmd.Parameters.AddWithValue("@TenDN", string.IsNullOrWhiteSpace(txtTenDN.Text) ? (object)DBNull.Value : txtTenDN.Text.Trim());
                    // NOTE: Ở đây lưu thẳng mật khẩu để demo. Trong thực tế hãy hash + salt trước khi lưu.
                    cmd.Parameters.AddWithValue("@MatKhau", string.IsNullOrWhiteSpace(pwdMatKhau.Password) ? (object)DBNull.Value : pwdMatKhau.Password);
                    cmd.Parameters.AddWithValue("@VaiTro", (cbVaiTro.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "KhachHang");

                    conn.Open();
                    cmd.ExecuteNonQuery();
                    conn.Close();
                }

                MessageBox.Show("Thêm thành công.");
                LoadNguoiDung();
                ClearForm();
            }
            catch (Exception ex)
            {
                if (conn.State == ConnectionState.Open) conn.Close();
                MessageBox.Show("Lỗi thêm: " + ex.Message);
            }
        }
        private void btnSua_Click(object sender, RoutedEventArgs e)
        {
            if (dgNguoiDung.SelectedItem == null) { MessageBox.Show("Chọn người dùng để sửa."); return; }
            DataRowView row = dgNguoiDung.SelectedItem as DataRowView;
            int id = Convert.ToInt32(row["MaNguoiDung"]);

            try
            {
                string sql = "UPDATE NguoiDung SET HoTen=@HoTen, SoDienThoai=@SDT, Email=@Email, TenDangNhap=@TenDN, VaiTro=@VaiTro {0} WHERE MaNguoiDung=@Id";

                // nếu người dùng nhập mật khẩu mới thì cập nhật
                string pwdClause = "";
                if (!string.IsNullOrWhiteSpace(pwdMatKhau.Password))
                {
                    pwdClause = ", MatKhau=@MatKhau";
                }

                sql = string.Format(sql, pwdClause);

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@HoTen", txtHoTen.Text.Trim());
                    cmd.Parameters.AddWithValue("@SDT", string.IsNullOrWhiteSpace(txtSDT.Text) ? (object)DBNull.Value : txtSDT.Text.Trim());
                    cmd.Parameters.AddWithValue("@Email", string.IsNullOrWhiteSpace(txtEmail.Text) ? (object)DBNull.Value : txtEmail.Text.Trim());
                    cmd.Parameters.AddWithValue("@TenDN", string.IsNullOrWhiteSpace(txtTenDN.Text) ? (object)DBNull.Value : txtTenDN.Text.Trim());
                    cmd.Parameters.AddWithValue("@VaiTro", (cbVaiTro.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "KhachHang");
                    if (!string.IsNullOrWhiteSpace(pwdMatKhau.Password))
                        cmd.Parameters.AddWithValue("@MatKhau", pwdMatKhau.Password);
                    cmd.Parameters.AddWithValue("@Id", id);

                    conn.Open();
                    cmd.ExecuteNonQuery();
                    conn.Close();
                }

                MessageBox.Show("Cập nhật thành công.");
                LoadNguoiDung();
            }
            catch (Exception ex)
            {
                if (conn.State == ConnectionState.Open) conn.Close();
                MessageBox.Show("Lỗi sửa: " + ex.Message);
            }
        }
        private void btnXoa_Click(object sender, RoutedEventArgs e)
        {
            if (dgNguoiDung.SelectedItem == null) { MessageBox.Show("Chọn người dùng để xóa."); return; }
            DataRowView row = dgNguoiDung.SelectedItem as DataRowView;
            int id = Convert.ToInt32(row["MaNguoiDung"]);

            if (MessageBox.Show("Bạn có chắc muốn xóa?", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes)
                return;

            try
            {
                string sql = "DELETE FROM NguoiDung WHERE MaNguoiDung=@Id";
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    conn.Open();
                    cmd.ExecuteNonQuery();
                    conn.Close();
                }

                MessageBox.Show("Xóa thành công.");
                LoadNguoiDung();
                ClearForm();
            }
            catch (Exception ex)
            {
                if (conn.State == ConnectionState.Open) conn.Close();
                MessageBox.Show("Lỗi xóa: " + ex.Message);
            }
        }
    }

}

