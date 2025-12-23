namespace BackendAPI.Models
{
    public class PostCreateRequest
    {
        public int UserId { get; set; }
        public string? Title { get; set; }
        public string? Content { get; set; }

        // MAUI sẽ gửi bằng multipart/form-data
        public List<IFormFile>? Images { get; set; }
        public List<IFormFile>? Videos { get; set; }

        // Media cũ muốn xóa
        public List<int>? DeletedMediaIds { get; set; }
    }
}
