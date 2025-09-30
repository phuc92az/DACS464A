using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DoAnCaNhan
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        SqlConnection conn;
        SqlDataAdapter da;
        DataTable dt;

        public MainWindow()
        {
            InitializeComponent();
            conn = new SqlConnection(@"Data Source=.\SQLEXPRESS;Initial Catalog=RapChieuPhim;Integrated Security=True");
            LoadVe();
            LoadPhim();
            LoadTrangThai();
        }
        private void LoadVe(string keyword = "")
        {
            string sql = "SELECT v.MaVe, kh.TenKH AS KhachHang, p.TenPhim, sc.NgayChieu, sc.GioChieu, " +
                         "g.SoHang + CAST(g.SoCot AS NVARCHAR) AS Ghe, v.GiaVe, v.TrangThai " +
                         "FROM Ve v " +
                         "JOIN SuatChieu sc ON v.MaSuatChieu = sc.MaSuatChieu " +
                         "JOIN Phim p ON sc.MaPhim = p.MaPhim " +
                         "JOIN Ghe g ON v.MaGhe = g.MaGhe " +
                         "LEFT JOIN KhachHang kh ON v.MaKH = kh.MaKH " +
                         "WHERE p.TenPhim LIKE @kw OR kh.TenKH LIKE @kw";

            da = new SqlDataAdapter(sql, conn);
            da.SelectCommand.Parameters.AddWithValue("@kw", "%" + keyword + "%");

            dt = new DataTable();
        conn = new SqlConnection(@"Data Source=.;Initial Catalog=RapChieuPhim;Integrated Security=True");
            dgVe.ItemsSource = dt.DefaultView;
        }
        private void LoadPhim()
        {
            SqlDataAdapter daPhim = new SqlDataAdapter("SELECT MaPhim, TenPhim FROM Phim", conn);
            DataTable dtPhim = new DataTable();
            conn = new SqlConnection(@"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=C:\Users\PC\Documents\RapChieuPhim.mdf;Integrated Security=True");

            cbPhim.ItemsSource = dtPhim.DefaultView;
            cbPhim.DisplayMemberPath = "TenPhim";
            cbPhim.SelectedValuePath = "MaPhim";
        }
        private void LoadLichChieu(int maPhim)
        {
            SqlDataAdapter daLC = new SqlDataAdapter(
                "SELECT MaSuatChieu, CONVERT(varchar, NgayChieu, 103) + ' - ' + CONVERT(varchar, GioChieu, 108) AS Lich " +
                "FROM SuatChieu WHERE MaPhim=@MaPhim", conn);
            daLC.SelectCommand.Parameters.AddWithValue("@MaPhim", maPhim);

            DataTable dtLC = new DataTable();
            daLC.Fill(dtLC);

            cbLichChieu.ItemsSource = dtLC.DefaultView;
            cbLichChieu.DisplayMemberPath = "Lich";
            cbLichChieu.SelectedValuePath = "MaSuatChieu";
        }
        private void LoadGhe(int maPhong)
        {
            SqlDataAdapter daGhe = new SqlDataAdapter("SELECT MaGhe, SoHang + CAST(SoCot AS NVARCHAR) AS TenGhe FROM Ghe WHERE MaPhong=@MaPhong", conn);
            daGhe.SelectCommand.Parameters.AddWithValue("@MaPhong", maPhong);

            DataTable dtGhe = new DataTable();
            daGhe.Fill(dtGhe);

            cbGhe.ItemsSource = dtGhe.DefaultView;
            cbGhe.DisplayMemberPath = "TenGhe";
            cbGhe.SelectedValuePath = "MaGhe";
        }

        private void LoadTrangThai()
        {
            cbTrangThai.Items.Clear();
            cbTrangThai.Items.Add("Trống");
            cbTrangThai.Items.Add("Đã đặt");
            cbTrangThai.SelectedIndex = 0;
        }
        private void cbPhim_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbPhim.SelectedValue != null)
            {
                LoadLichChieu((int)cbPhim.SelectedValue);
            }
        }
        private void cbLichChieu_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbLichChieu.SelectedValue != null)
            {
                SqlCommand cmd = new SqlCommand("SELECT MaPhong FROM SuatChieu WHERE MaSuatChieu=@MaSC", conn);
                cmd.Parameters.AddWithValue("@MaSC", cbLichChieu.SelectedValue);
                conn.Open();
                int maPhong = (int)cmd.ExecuteScalar();
                conn.Close();

                LoadGhe(maPhong);
            }
        }
        private void btnTimKiem_Click(object sender, RoutedEventArgs e)
        {
            LoadVe(txtTimKiem.Text);
        }
        private void btnLamMoi_Click(object sender, RoutedEventArgs e)
        {
            txtKhachHang.Clear();
            txtGiaVe.Clear();
            cbPhim.SelectedIndex = -1;
            cbLichChieu.SelectedIndex = -1;
            cbGhe.SelectedIndex = -1;
            cbTrangThai.SelectedIndex = 0;
            LoadVe();
        }
        private void btnThem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string sql = "INSERT INTO Ve(MaSuatChieu, MaGhe, GiaVe, TrangThai) VALUES (@MaSC, @MaGhe, @GiaVe, @TrangThai)";
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@MaSC", cbLichChieu.SelectedValue);
                cmd.Parameters.AddWithValue("@MaGhe", cbGhe.SelectedValue);
                cmd.Parameters.AddWithValue("@GiaVe", decimal.Parse(txtGiaVe.Text));
                cmd.Parameters.AddWithValue("@TrangThai", cbTrangThai.Text);

                conn.Open();
                cmd.ExecuteNonQuery();
                conn.Close();

                MessageBox.Show("Thêm vé thành công!");
                LoadVe();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi: " + ex.Message);
                if (conn.State == ConnectionState.Open) conn.Close();
            }
        }
        private void btnSua_Click(object sender, RoutedEventArgs e)
        {
            if (dgVe.SelectedItem == null) return;
            DataRowView row = (DataRowView)dgVe.SelectedItem;
            int maVe = (int)row["MaVe"];

            string sql = "UPDATE Ve SET GiaVe=@GiaVe, TrangThai=@TrangThai WHERE MaVe=@MaVe";
            SqlCommand cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@GiaVe", decimal.Parse(txtGiaVe.Text));
            cmd.Parameters.AddWithValue("@TrangThai", cbTrangThai.Text);
            cmd.Parameters.AddWithValue("@MaVe", maVe);

            conn.Open();
            cmd.ExecuteNonQuery();
            conn.Close();

            MessageBox.Show("Sửa vé thành công!");
            LoadVe();
        }
        private void btnXoa_Click(object sender, RoutedEventArgs e)
        {
            if (dgVe.SelectedItem == null) return;
            DataRowView row = (DataRowView)dgVe.SelectedItem;
            int maVe = (int)row["MaVe"];

            SqlCommand cmd = new SqlCommand("DELETE FROM Ve WHERE MaVe=@MaVe", conn);
            cmd.Parameters.AddWithValue("@MaVe", maVe);

            conn.Open();
            cmd.ExecuteNonQuery();
            conn.Close();

            MessageBox.Show("Xóa vé thành công!");
            LoadVe();
        }
    }
}
