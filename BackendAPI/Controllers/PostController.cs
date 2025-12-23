using BackendAPI.Data;
using BackendAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BackendAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PostController : ControllerBase
    {
        private readonly AppDbContext _context;

        public PostController(AppDbContext context)
        {
            _context = context;
        }

        // 📋 GET: api/post
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetPosts()
        {
            var posts = await _context.Posts
                .Include(p => p.User)
                .Include(p => p.Media)
                .Select(p => new
                {
                    p.Id,
                    p.Title,
                    p.Content,
                    p.CreatedAt,
                    UserName = p.User != null ? p.User.Name : "(Người dùng đã xóa)",

                    Media = p.Media.Select(m => new
                    {
                        m.Id,
                        m.MediaType,
                        m.Url
                    }).ToList()
                })
                .ToListAsync();

            return Ok(posts);
        }



        // 🔍 GET: api/post/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<Post>> GetPost(int id)
        {
            var post = await _context.Posts
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (post == null)
                return NotFound();

            return Ok(new
            {
                post.Id,
                post.Title,
                post.Content,
                post.CreatedAt,
                UserName = post.User?.Name
            });
        }

        // ➕ POST: api/post
        //[HttpPost]
        //public async Task<ActionResult<Post>> CreatePost(Post post)
        //{
        //    if (!_context.Users.Any(u => u.Id == post.UserId))
        //        return BadRequest(new { message = "User không tồn tại." });

        //    post.CreatedAt = DateTime.UtcNow;
        //    _context.Posts.Add(post);
        //    await _context.SaveChangesAsync();

        //    return CreatedAtAction(nameof(GetPost), new { id = post.Id }, post);
        //}

        [HttpPost]
        [Route("create")]
        public async Task<IActionResult> CreatePost([FromForm] PostCreateRequest req)
        {
            if (req == null)
                return BadRequest("Invalid request.");

            var post = new Post
            {
                UserId = req.UserId,
                Title = req.Title ?? "",
                Content = req.Content ?? "",
                CreatedAt = DateTime.UtcNow
            };

            _context.Posts.Add(post);
            await _context.SaveChangesAsync();

            // ============================
            //  Save Images
            // ============================
            if (req.Images != null)
            {
                foreach (var img in req.Images)
                {
                    var fileName = $"{Guid.NewGuid()}{Path.GetExtension(img.FileName)}";
                    var filePath = Path.Combine("Uploads", fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await img.CopyToAsync(stream);
                    }

                    _context.PostMedia.Add(new PostMedia
                    {
                        PostId = post.Id,
                        MediaType = "image",
                        Url = $"{Request.Scheme}://{Request.Host}/Uploads/{fileName}"
                    });
                }
            }

            // ============================
            //  Save Videos
            // ============================
            if (req.Videos != null)
            {
                foreach (var vid in req.Videos)
                {
                    var fileName = $"{Guid.NewGuid()}{Path.GetExtension(vid.FileName)}";
                    var filePath = Path.Combine("Uploads", fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await vid.CopyToAsync(stream);
                    }

                    _context.PostMedia.Add(new PostMedia
                    {
                        PostId = post.Id,
                        MediaType = "video",
                        Url = $"{Request.Scheme}://{Request.Host}/Uploads/{fileName}"
                    });
                }
            }

            await _context.SaveChangesAsync();

            return Ok(new { Message = "Post created", PostId = post.Id });
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePost(int id, [FromForm] PostCreateRequest req)
        {
            var existing = await _context.Posts.FindAsync(id);
            if (existing == null)
                return NotFound();

            existing.Title = req.Title ?? existing.Title;
            existing.Content = req.Content ?? existing.Content;

            // =========================
            // Xóa media đã xóa từ client
            // =========================
            if (req.DeletedMediaIds != null)
            {
                var toDelete = _context.PostMedia.Where(m => req.DeletedMediaIds.Contains(m.Id));
                _context.PostMedia.RemoveRange(toDelete);
            }

            // =========================
            // Thêm media mới
            // =========================
            if (req.Images != null)
            {
                foreach (var img in req.Images)
                {
                    var fileName = $"{Guid.NewGuid()}{Path.GetExtension(img.FileName)}";
                    var filePath = Path.Combine("Uploads", fileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await img.CopyToAsync(stream);
                    }

                    _context.PostMedia.Add(new PostMedia
                    {
                        PostId = existing.Id,
                        MediaType = "image",
                        Url = $"{Request.Scheme}://{Request.Host}/Uploads/{fileName}"
                    });
                }
            }

            if (req.Videos != null)
            {
                foreach (var vid in req.Videos)
                {
                    var fileName = $"{Guid.NewGuid()}{Path.GetExtension(vid.FileName)}";
                    var filePath = Path.Combine("Uploads", fileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await vid.CopyToAsync(stream);
                    }

                    _context.PostMedia.Add(new PostMedia
                    {
                        PostId = existing.Id,
                        MediaType = "video",
                        Url = $"{Request.Scheme}://{Request.Host}/Uploads/{fileName}"
                    });
                }
            }

            await _context.SaveChangesAsync();
            return NoContent();
        }
        // ❌ DELETE: api/post/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePost(int id)
        {
            // Tìm bài viết
            var post = await _context.Posts.FindAsync(id);
            if (post == null)
                return NotFound();

            // Lấy tất cả media liên quan
            var medias = _context.PostMedia.Where(m => m.PostId == id).ToList();

            foreach (var media in medias)
            {
                // Xóa file trên server nếu tồn tại
                try
                {
                    var uri = new Uri(media.Url);
                    var filePath = Path.Combine("Uploads", Path.GetFileName(uri.LocalPath));
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                }
                catch
                {
                    // Bỏ qua lỗi xóa file
                }

                // Xóa media khỏi database
                _context.PostMedia.Remove(media);
            }

            // Xóa bài viết
            _context.Posts.Remove(post);

            await _context.SaveChangesAsync();

            return NoContent();
        }

    }
}
