using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace DoAnCaNhan
{
    public partial class QLyPhongVaGhe : Window
    {
        private List<PhongChieu> rooms = new List<PhongChieu>();
        private List<Ghe> seats = new List<Ghe>();
        private PhongChieu selectedRoom;
        private Ghe selectedSeat;
        private int currentRows = 8;
        private int currentColumns = 12;

        public QLyPhongVaGhe()
        {
            InitializeComponent();
            LoadRoomsFromDatabase();
            UpdateRoomList();
        }

        #region Database (EF6)

        private void LoadRoomsFromDatabase()
        {
            using (var db = new QLRapDBEntities1())
            {
                rooms = db.PhongChieux.ToList();
            }
        }

        private void LoadSeatsFromDatabase(int roomId)
        {
            using (var db = new QLRapDBEntities1())
            {
                seats = db.Ghes.Where(g => g.MaPhong == roomId).ToList();
            }
        }

        private void SaveRoomToDatabase(string tenPhong)
        {
            using (var db = new QLRapDBEntities1())
            {
                var phong = new PhongChieu { TenPhong = tenPhong };
                db.PhongChieux.Add(phong);
                db.SaveChanges();
            }
        }

        private void UpdateRoomInDatabase(int maPhong, string tenPhong)
        {
            using (var db = new QLRapDBEntities1())
            {
                var phong = db.PhongChieux.Find(maPhong);
                if (phong != null)
                {
                    phong.TenPhong = tenPhong;
                    db.SaveChanges();
                }
            }
        }

        private void DeleteRoomFromDatabase(int maPhong)
        {
            using (var db = new QLRapDBEntities1())
            {
                var phong = db.PhongChieux.Find(maPhong);
                if (phong != null)
                {
                    var gheTrongPhong = db.Ghes.Where(g => g.MaPhong == maPhong).ToList();
                    foreach (var g in gheTrongPhong) db.Ghes.Remove(g);

                    db.PhongChieux.Remove(phong);
                    db.SaveChanges();
                }
            }
        }

        private void SaveSeatToDatabase(int maPhong, char hang, int cot)
        {
            using (var db = new QLRapDBEntities1())
            {
                var ghe = new Ghe { MaPhong = maPhong, Hang = hang.ToString(), Cot = cot };
                db.Ghes.Add(ghe);
                db.SaveChanges();
            }
        }

        private void DeleteAllSeatsFromDatabase(int maPhong)
        {
            using (var db = new QLRapDBEntities1())
            {
                var gheTrongPhong = db.Ghes.Where(g => g.MaPhong == maPhong).ToList();
                foreach (var g in gheTrongPhong) db.Ghes.Remove(g);
                db.SaveChanges();
            }
        }

        #endregion

        #region UI

        private void UpdateRoomList()
        {
            lstRooms.ItemsSource = null;
            lstRooms.ItemsSource = rooms;
        }

        private void RoomSelection_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (lstRooms.SelectedItem is PhongChieu room)
            {
                selectedRoom = room;
                lblSelectedRoom.Text = $"{room.TenPhong} (Mã: {room.MaPhong})";
                lblSelectedRoom.Foreground = new SolidColorBrush(Color.FromRgb(39, 174, 96));

                LoadRoomDataToForm(room);
                LoadSeatsFromDatabase(room.MaPhong);
                GenerateSeatLayout();
            }
        }

        private void LoadRoomDataToForm(PhongChieu room)
        {
            txtRoomId.Text = room.MaPhong.ToString();
            txtRoomName.Text = room.TenPhong;
        }

        private void AddRoom_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtRoomName.Text))
            {
                MessageBox.Show("Vui lòng nhập tên phòng!"); return;
            }

            SaveRoomToDatabase(txtRoomName.Text.Trim());
            LoadRoomsFromDatabase();
            UpdateRoomList();
            ClearRoomForm();
        }

        private void UpdateRoom_Click(object sender, RoutedEventArgs e)
        {
            if (selectedRoom == null)
            {
                MessageBox.Show("Vui lòng chọn phòng để cập nhật!"); return;
            }
            if (string.IsNullOrWhiteSpace(txtRoomName.Text))
            {
                MessageBox.Show("Vui lòng nhập tên phòng!"); return;
            }

            UpdateRoomInDatabase(selectedRoom.MaPhong, txtRoomName.Text.Trim());
            LoadRoomsFromDatabase();
            UpdateRoomList();
        }

        private void DeleteRoom_Click(object sender, RoutedEventArgs e)
        {
            if (selectedRoom == null)
            {
                MessageBox.Show("Vui lòng chọn phòng để xóa!"); return;
            }
            if (MessageBox.Show("Xóa phòng này và tất cả ghế?", "Xác nhận", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                DeleteRoomFromDatabase(selectedRoom.MaPhong);
                LoadRoomsFromDatabase();
                UpdateRoomList();
                ClearRoomForm();
                ClearSeatGrid();

                lblSelectedRoom.Text = "Chưa chọn phòng";
                lblSelectedRoom.Foreground = Brushes.Red;
            }
        }

        private void OnRowsColumnsChanged(object sender, TextChangedEventArgs e)
        {

            if (!IsLoaded) return;

            var rowsBox = txtRows ?? (FindName("txtRows") as TextBox);
            var colsBox = txtColumns ?? (FindName("txtColumns") as TextBox);

            if (rowsBox == null || colsBox == null) return;

            int rows, cols;
            if (int.TryParse(rowsBox.Text, out rows) && int.TryParse(colsBox.Text, out cols))
            {
                currentRows = rows;
                currentColumns = cols;
                if (selectedRoom != null) GenerateSeatLayout();
            }
        }


        private void AutoCreateSeats_Click(object sender, RoutedEventArgs e)
        {
            if (selectedRoom == null) { MessageBox.Show("Chưa chọn phòng"); return; }
            if (!int.TryParse(txtRows.Text, out int rows) || rows <= 0 || rows > 26) return;
            if (!int.TryParse(txtColumns.Text, out int cols) || cols <= 0 || cols > 30) return;

            if (MessageBox.Show($"Tạo {rows * cols} ghế mới?", "Xác nhận", MessageBoxButton.YesNo) != MessageBoxResult.Yes) return;

            DeleteAllSeatsFromDatabase(selectedRoom.MaPhong);
            for (int h = 0; h < rows; h++)
            {
                char hangChar = (char)('A' + h);
                for (int c = 1; c <= cols; c++) SaveSeatToDatabase(selectedRoom.MaPhong, hangChar, c);
            }
            LoadSeatsFromDatabase(selectedRoom.MaPhong);
            LoadRoomsFromDatabase();
            UpdateRoomList();
            GenerateSeatLayout();
        }

        private void DeleteAllSeats_Click(object sender, RoutedEventArgs e)
        {
            if (selectedRoom == null) return;
            if (MessageBox.Show("Xóa tất cả ghế trong phòng?", "Xác nhận", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                DeleteAllSeatsFromDatabase(selectedRoom.MaPhong);
                LoadSeatsFromDatabase(selectedRoom.MaPhong);
                GenerateSeatLayout();
            }
        }

        private void GenerateSeatLayout()
        {
            seatGrid.Children.Clear();
            seatGrid.RowDefinitions.Clear();
            seatGrid.ColumnDefinitions.Clear();

            if (selectedRoom == null) return;

            int maxRow = seats.Any() ? seats.Max(s => s.Hang[0] - 'A' + 1) : currentRows;
            int maxCol = seats.Any() ? seats.Max(s => s.Cot) : currentColumns;

            for (int i = 0; i <= maxRow; i++)
                seatGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            for (int i = 0; i <= maxCol; i++)
                seatGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            // row headers
            for (int r = 0; r < maxRow; r++)
            {
                var lbl = new TextBlock { Text = ((char)('A' + r)).ToString(), Margin = new Thickness(5) };
                Grid.SetRow(lbl, r + 1); Grid.SetColumn(lbl, 0);
                seatGrid.Children.Add(lbl);
            }
            // col headers
            for (int c = 1; c <= maxCol; c++)
            {
                var lbl = new TextBlock { Text = c.ToString(), Margin = new Thickness(5) };
                Grid.SetRow(lbl, 0); Grid.SetColumn(lbl, c);
                seatGrid.Children.Add(lbl);
            }

            // seats
            for (int r = 0; r < maxRow; r++)
            {
                char hangChar = (char)('A' + r);
                for (int c = 1; c <= maxCol; c++)
                {
                    var seat = seats.FirstOrDefault(s => s.Hang[0] == hangChar && s.Cot == c);
                    var btn = new Button
                    {
                        Content = seat != null ? $"{hangChar}{c}" : "•",
                        Width = 30,
                        Height = 30,
                        Margin = new Thickness(2),
                        Background = seat != null ? Brushes.Green : Brushes.Gray,
                        Tag = seat
                    };
                    if (seat != null)
                    {
                        btn.Click += (s, e) =>
                        {
                            selectedSeat = (s as Button).Tag as Ghe;
                            LoadSeatDetails();
                            foreach (var child in seatGrid.Children.OfType<Button>())
                            {
                                if (child.Tag is Ghe) child.Background = Brushes.Green;
                            }
                            (s as Button).Background = Brushes.Red;
                        };
                    }
                    else btn.IsEnabled = false;

                    Grid.SetRow(btn, r + 1);
                    Grid.SetColumn(btn, c);
                    seatGrid.Children.Add(btn);
                }
            }
        }

        private void LoadSeatDetails()
        {
            if (selectedSeat != null)
            {
                txtSeatId.Text = selectedSeat.MaGhe.ToString();
                txtSeatPosition.Text = $"{selectedSeat.Hang}{selectedSeat.Cot}";
                txtHang.Text = selectedSeat.Hang;
                txtCot.Text = selectedSeat.Cot.ToString();
            }
        }

        private void ClearRoomForm()
        {
            txtRoomId.Text = ""; txtRoomName.Text = ""; selectedRoom = null;
        }

        private void ClearSeatGrid() => seatGrid.Children.Clear();

        #endregion
    }
}
