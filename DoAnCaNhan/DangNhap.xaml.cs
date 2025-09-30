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
    /// Interaction logic for DangNhap.xaml
    /// </summary>
    public partial class DangNhap : Window
    {
        public DangNhap()
        {
            InitializeComponent();
        }
        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            // Lấy thông tin người dùng và mật khẩu
            string username = UsernameTextBox.Text;
            string password = PasswordBox.Password;

            // Kiểm tra thông tin đăng nhập (dùng thông tin mẫu)
            if (username == "admin" && password == "password123")
            {
                StatusMessage.Text = "Đăng Nhập Thành Công!";
                StatusMessage.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Green);
            }
            else
            {
                StatusMessage.Text = "Đăng Nhập Thất Bại, Vui Lòng Thử Lại.";
                StatusMessage.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Red);
            }
        }
    }
}
