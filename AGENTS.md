# AGENTS.md — MultiGuide Backend

Đọc toàn bộ file này **trước khi** viết hoặc sửa bất kỳ dòng code nào. Nếu yêu cầu của người dùng mâu thuẫn với file này, hãy nói ra mâu thuẫn và hỏi lại, đừng tự chọn.

## 1. Dự án là gì
MultiGuide: hệ thống thuyết minh tự động đa ngôn ngữ cho khách du lịch. Khách quét QR, chọn ngôn ngữ, trả 2 USD một lần (cổng mock), bật GPS; khi đến gần điểm tham quan (POI) thì xem nội dung và nghe audio đúng ngôn ngữ. Admin quản lý nội dung qua API.

**Phạm vi của repo này: chỉ Backend (REST API + CSDL + CI/CD).** Không viết giao diện, không viết logic GPS/debounce/cooldown phía client.

**Nguồn sự thật:** PRD v1.1 (người dùng sẽ đính kèm phần liên quan trong từng task). Không tự thêm tính năng ngoài phạm vi MVP.

## 2. Stack
- .NET 10 (`10.0.x`), ASP.NET Core Web API, EF Core, MySQL (utf8mb4).
- Provider MySQL cho EF Core: **kiểm tra bản tương thích EF Core 10 trước khi cài**, hỏi người dùng nếu không chắc.
- Test: xUnit. Mock: NSubstitute hoặc Moq (dùng một loại, đã chọn thì giữ nguyên).
- Log: Serilog (JSON). Lỗi: ProblemDetails (RFC 7807).
- Hạ tầng: Docker, Docker Compose, GitHub Actions, GHCR, Caddy (HTTPS).
- **Không thêm NuGet package mới nếu chưa nói rõ lý do và được đồng ý.**

## 3. Cấu trúc solution

```
MultiGuide.slnx
MultiGuide.Api        Presentation: Controller, DTO, middleware, DI (composition root)
MultiGuide.Business   Service, quy tắc nghiệp vụ, ĐỊNH NGHĨA mọi interface
MultiGuide.Data       DbContext, migration, Repository, LocalDiskFileStorage, MockPaymentGateway
MultiGuide.Domain     Entity, enum, hằng số, exception nghiệp vụ
MultiGuide.Tests      Unit test (Service) + integration test + test kiến trúc
```

### Quy tắc phụ thuộc (bắt buộc)
- `Api → Business`, `Api → Domain`, `Api → Data` (chỉ để đăng ký DI trong `Program.cs`).
- `Data → Business` (implement interface), `Data → Domain`.
- `Business → Domain` và **không gì khác**. Business không được tham chiếu Data, Api, EF Core, đường dẫn file, SDK thanh toán.
- Mọi interface (`IXxxRepository`, `IPaymentGateway`, `IFileStorage`, `IClock`, `IPasswordHasher`) **định nghĩa ở Business**.
- Controller chỉ: nhận request, validate DTO, gọi Service, trả response. Không có logic nghiệp vụ, không dùng `DbContext`.
- Không trả Entity ra ngoài API. Luôn dùng DTO.

### Phân định xác thực và phân quyền
- Middleware (Api): chỉ xác thực token (có, đúng dạng, chưa hết hạn, chưa thu hồi) rồi gắn `PassContext` hoặc `AdminContext` vào request.
- Service (Business): kiểm tra quyền theo tài nguyên (pass có thuộc khu của POI không, role nào được làm gì).

## 4. Quyết định thiết kế đã chốt (không tự đổi)
1. **Geofence stateless ở server.** `GET /pois/nearby` chỉ lọc bounding box, tính Haversine, trả `distance_m` và `within_trigger`. Sắp xếp: `priority` giảm dần rồi `distance_m` tăng dần. Debounce 3 giây và cooldown 5 phút là việc của FE. Server **không lưu** tọa độ khách, **không log** `lat/lng`.
2. **AccessPass là token opaque.** 32 byte từ CSPRNG, trả plaintext **một lần** khi thanh toán thành công. DB chỉ lưu SHA-256 (`token_hash` unique). Mỗi request: băm token, tra DB, kiểm tra `status`, `expires_at`, và pass thuộc đúng khu. Hạn mặc định 24 giờ.
3. **Idempotency thanh toán.** Header `Idempotency-Key` bắt buộc. Unique `(session_id, idempotency_key)`. Cùng key + cùng `request_hash` → trả lại kết quả cũ. Cùng key + khác body → 422 `IDEMPOTENCY_KEY_REUSED`. Hai request song song phải chỉ tạo một Payment (dựa vào unique constraint, bắt lỗi trùng).
4. **Fallback ngôn ngữ:** ngôn ngữ chọn → `en` → `vi`. Trả `fallback_language` (null nếu không fallback). Không có bản dịch nào hoặc POI tắt → 404 `POI_CONTENT_NOT_FOUND`. Có chữ nhưng thiếu audio → `audio_url = null`.
5. **Không xóa cứng** khu, POI, bản dịch. Dùng `is_active`. Mọi FK `ON DELETE RESTRICT`.
6. **Thời gian lưu UTC**, trả ISO 8601 có `Z`. Dùng `IClock`, không gọi `DateTime.UtcNow` trực tiếp trong Service.
7. **Upload:** kiểm tra magic bytes (không tin đuôi file). Audio: mp3, ≤ 10 MB. Ảnh: jpg/png/webp, ≤ 5 MB. Lưu qua `IFileStorage`, tính SHA-256.
8. **Migration chạy ở bước riêng** (container `migrator`), không tự migrate khi API khởi động.

## 5. Quy ước code
- Ngôn ngữ: **tên class/method/biến/commit bằng tiếng Anh**; comment giải thích "tại sao" có thể tiếng Việt, ngắn gọn.
- Tên: PascalCase cho type/method, camelCase cho biến cục bộ, hậu tố `Async` cho method async, interface bắt đầu bằng `I`.
- Mọi I/O dùng `async/await`, truyền `CancellationToken` từ Controller xuống Repository.
- Bật nullable. Không bỏ qua cảnh báo (CI chạy `-warnaserror`).
- Cột DB: `snake_case` (cấu hình trong EF), tên bảng theo PRD mục 6.
- Validate đầu vào ở DTO (và Service cho quy tắc nghiệp vụ). Sai → 422 với `code` ổn định.
- Lỗi nghiệp vụ: ném exception của Domain, `ExceptionHandler` middleware chuyển thành ProblemDetails kèm trường `code` và `traceId`.
- API prefix `/api/v1`. Phân trang: `page`, `page_size` (≤ 100).
- Không hard-code secret, connection string, URL. Đọc từ cấu hình/biến môi trường. Có `.env.example`.
- Không log token, mật khẩu, tọa độ.

### Mã lỗi
| HTTP | `code` |
| :-- | :-- |
| 401 | `PASS_MISSING`, `AUTH_REQUIRED` |
| 403 | `PASS_INVALID`, `PASS_EXPIRED`, `PASS_AREA_MISMATCH`, `FORBIDDEN` |
| 404 | `AREA_NOT_FOUND`, `POI_CONTENT_NOT_FOUND` |
| 409 | `TRANSLATION_EXISTS` |
| 402 | `PAYMENT_FAILED` |
| 422 | `VALIDATION_ERROR`, `IDEMPOTENCY_KEY_REUSED` |
| 423 | `ACCOUNT_LOCKED` |
| 429 | `RATE_LIMITED` |

## 6. Mô hình dữ liệu (tóm tắt, chi tiết trong PRD mục 6)
`Language`, `TourArea`, `Poi`, `PoiTranslation`, `AudioFile`, `VisitorSession`, `Payment`, `AccessPass`, `PlayLog`, `Role`, `AdminUser` (`AuditLog` để sau).
Ràng buộc quan trọng: unique `TourArea.code`; unique `(poi_id, language_code)`; unique `PoiTranslation.audio_file_id`; unique `(session_id, idempotency_key)`; unique `AccessPass.payment_id`; unique `AccessPass.token_hash`; `AdminUser.locked_until`. Tiền dùng `decimal(10,2)`. `AccessPass` **không** có `area_id` riêng (suy ra qua session).

## 7. Test
- Service: unit test với mock Repository/Gateway. Mục tiêu coverage tầng Business ≥ 60%.
- Bắt buộc có test cho: idempotency thanh toán, kiểm tra AccessPass (thiếu/sai/hết hạn/sai khu), nearby (sắp xếp, bán kính, validate), fallback ngôn ngữ, khóa tài khoản sau 5 lần sai.
- Integration test dùng DB thật (Testcontainers MySQL) cho các luồng trên khi đến sprint tương ứng.
- Có test kiến trúc kiểm tra quy tắc phụ thuộc ở mục 3.
- Mỗi bug sửa xong phải có test tái hiện.

## 8. CI/CD
- CI (mỗi PR/push): restore → build `-warnaserror` → `dotnet format --verify-no-changes` → test + coverage → docker build (không push) → quét package có lỗ hổng.
- CD (merge `main`): build image, push GHCR (tag `sha-<commit>` và `latest`) → SSH, `docker compose pull && up -d` → chạy migrator → smoke test `/health/ready` → fail thì quay về tag trước.
- Secret chỉ ở GitHub Secrets và `.env` trên server. Không commit.
- Workflow GitHub chỉ đọc từ `.github/workflows/` ở **gốc repo git**. Không để workflow ở thư mục con.

## 9. Git
- Nhánh: `feature/<ID>-<mo-ta-ngan>` tách từ `develop`; PR vào `develop`; `main` chỉ nhận từ `develop` khi ổn định.
- Commit nhỏ, thông điệp theo Conventional Commits: `feat:`, `fix:`, `test:`, `chore:`, `docs:`, `ci:`.
- Không commit `bin/`, `obj/`, file `.env`, file tạm, file tài liệu không liên quan (docx, ảnh nặng).

## 10. Cách làm việc của AI agent
1. **Đọc task trước, hỏi nếu mơ hồ.** Nếu thiếu thông tin quyết định (tên bảng, quy tắc), hỏi một câu ngắn thay vì đoán.
2. **Task lớn thì lập kế hoạch ngắn trước** (danh sách file sẽ tạo/sửa theo từng tầng), chờ xác nhận, rồi mới viết code.
3. **Làm đúng phạm vi task.** Không refactor, đổi tên, hay "tiện tay" sửa file ngoài danh sách. Phát hiện vấn đề ngoài phạm vi thì ghi chú cuối câu trả lời.
4. **Đi theo thứ tự tầng:** Domain → Business (interface + Service) → Data (implement) → Api → Tests.
5. **Tự kiểm tra trước khi báo xong:** chạy `dotnet build -warnaserror` và `dotnet test`. Báo kết quả thật, không nói "chắc là chạy được".
6. **Báo cáo cuối task** theo mẫu: file đã tạo/sửa (theo tầng), quyết định đã đưa ra, kết quả build/test, việc còn dở hoặc rủi ro.
7. Không bịa API, package, hay cú pháp. Không chắc thì nói không chắc.

## 11. Definition of Done (mỗi story)
Code qua build không cảnh báo · có unit test Service · đúng quy tắc phụ thuộc · OpenAPI cập nhật · CI xanh · không có secret/tọa độ trong log · không file thừa trong commit.

## 12. Dọn dẹp đã biết (ưu tiên Sprint 1)
- Xóa file template `WeatherForecast.cs` và controller mẫu nếu có.
- Chỉ giữ **một** thư mục `.github/workflows` ở gốc repo git; xác nhận đường dẫn solution trong `ci.yml` (cần `working-directory` nếu `.slnx` nằm trong thư mục con).
- Bỏ `TourLingo.docx` và tài liệu không liên quan khỏi repo.