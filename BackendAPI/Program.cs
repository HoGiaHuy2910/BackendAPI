using BackendAPI.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);

// ✅ Đăng ký DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ✅ Cấu hình CORS (cho phép mọi domain — tạm dùng cho dev)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod());
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// ✅ Bật CORS sớm (phải trước MapControllers)
app.UseCors("AllowAll");

// ✅ Swagger (chỉ bật khi dev)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "Uploads");
if (!Directory.Exists(uploadsPath))
{
    Directory.CreateDirectory(uploadsPath);
}


app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(Directory.GetCurrentDirectory(), "Uploads")),
    RequestPath = "/Uploads"
});


// ❗ KHÔNG ép HTTPS cho Android emulator (vì nó chỉ chạy http://10.0.2.2)
app.UseAuthorization();
app.MapControllers();

// ✅ Cấu hình cho phép listen cả HTTP và HTTPS
// Nếu bạn chạy bằng "dotnet run" hoặc cấu hình WebAPI profile thì tự động nhận
// nhưng thêm dòng này để đảm bảo khi chạy trong VS vẫn mở đúng cổng
app.Urls.Add("http://0.0.0.0:5180");   // Cho Android emulator truy cập
app.Urls.Add("https://localhost:7191"); // Cho Swagger HTTPS

app.Run();
