using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace BackEnd.Models
{
    public class VideoPlaylist
    {
		//Trong cơ sở dữ liệu, một khóa chính tổ hợp là một khóa chính tạo thành từ hai
		//hoặc nhiều cột trong một bảng. Các cột này khi kết hợp lại với nhau tạo ra
		//một khóa duy nhất, giúp xác định một dòng dữ liệu cụ thể trong bảng.
		[Key, Column(Order = 0)]
		[ForeignKey("Video")]
		public string? VideoId { get; set; }
		public virtual Video Video { get; set; } = null!;

		[Key, Column(Order = 1)]
		[ForeignKey("Playlist")]
		public string? PlaylistId { get; set; }
		public virtual Playlist Playlist { get; set; } = null!;

		//Lazy Loading là một kỹ thuật trong lập trình, nó cho phép trì hoãn việc
		//tải dữ liệu cho đến khi dữ liệu đó thực sự cần thiết. Trong Entity Framework,
		//Lazy Loading được sử dụng để trì hoãn việc tải các đối tượng liên quan cho đến
		//khi bạn thực sự truy cập chúng. Điều này có thể giúp cải thiện hiệu suất bằng
		//cách giảm bớt số lượng dữ liệu được tải từ cơ sở dữ liệu.
	}
}
