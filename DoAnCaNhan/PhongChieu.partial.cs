using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace DoAnCaNhan
{
    public partial class PhongChieu
    {
        [NotMapped]
        public int TongSoGhe { get; set; }   // chỉ là số, gán từ code
    }
}
