# MultiGuide — Setup sau khi clone

## 1. Cấu hình biến môi trường

Từ thư mục repo, dùng PowerShell:

```powershell
$env:MYSQL_ROOT_PASSWORD="local-mysql-password"
$env:ConnectionStrings__Default="Server=127.0.0.1;Port=3308;Database=multiguide;User=root;Password=local-mysql-password"
$env:Seed__Enabled="true"
$env:Seed__AdminEmail="admin@example.com"
$env:Seed__AdminPassword="local-admin-password"
```

Không commit các mật khẩu này vào Git.

## 2. Bật MySQL Docker

```powershell
docker compose up -d
docker compose exec -T mysql mysqladmin ping -uroot -p$env:MYSQL_ROOT_PASSWORD --silent
```

Kết quả cần thấy: `mysqld is alive`.

## 3. Tạo database và chạy API

```powershell
dotnet restore MultiGuide\MultiGuide.slnx
dotnet ef database update -p MultiGuide\MultiGuide.Data -s MultiGuide\MultiGuide.Api
dotnet run --project MultiGuide\MultiGuide.Api
```

Migration tạo schema; khi `Seed__Enabled=true`, API tự seed dữ liệu mẫu:
3 languages, 1 tour area, 6 POIs và 1 admin user. Mỗi máy có Docker volume
riêng nên không copy database runtime của máy khác; migration và seed tạo lại
môi trường tương đương.