# AGENTS.md — MultiGuide Backend

> **Dành cho AI coding agent:** Đọc toàn bộ file này TRƯỚC khi viết hoặc sửa bất kỳ dòng code nào.
> Nếu yêu cầu của task mâu thuẫn với file này, hãy dừng lại và hỏi người giao task, không tự quyết.

---

## 1. Tổng quan dự án

**MultiGuide** là app thuyết minh du lịch tự động đa ngôn ngữ (đồ án môn Công nghệ phần mềm, làm nhóm).

Repo này là **backend**, theo kiến trúc **3 lớp (3-layer)**, kèm **CI/CD**.

### Luồng sản phẩm (backend phải phục vụ đúng luồng này)

1. Khách quét QR → mở app → **chọn ngôn ngữ** → **thanh toán 2 USD**.
   - Thanh toán thất bại thì cho thử lại. Thành công thì **cấp quyền sử dụng ứng dụng**.
2. Khách bật GPS (việc xin quyền GPS là của **frontend**, backend không biết) → di chuyển → app gửi toạ độ → backend **nhận diện địa điểm gần nhất** → hiển thị thông tin địa điểm.
3. Khách bấm **nghe thuyết minh** → phát audio theo ngôn ngữ đã chọn → đi tiếp điểm kế tiếp. Rời khu du lịch thì **kết thúc phiên**.

---

## 2. Tech stack (đã chốt, không tự đổi)

| Hạng mục | Lựa chọn |
|---|---|
| Framework | ASP.NET Core Web API (.NET LTS, phiên bản chốt trong `global.json`) |
| ORM | Entity Framework Core + `Pomelo.EntityFrameworkCore.MySql` |
| Database | MySQL 8 (chạy bằng Docker) |
| API docs | Swagger (Swashbuckle) |
| Test | xUnit + Moq; integration test dùng `WebApplicationFactory` |
| CI | GitHub Actions (build + test, build Docker image) |
| Chạy demo | Docker Compose (api + mysql) |

**Không thêm** thư viện/framework mới (MediatR, AutoMapper, Redis, message queue...) nếu chưa được người phụ trách đồng ý.

---

## 3. Kiến trúc 3 lớp — QUY TẮC BẮT BUỘC

```
MultiGuide.Api        → Controllers, DTOs, Middleware, Filters, Program.cs
MultiGuide.Business   → Services (toàn bộ logic nghiệp vụ), Interfaces của Service
MultiGuide.Data       → DbContext, Repositories, Migrations, Seed
MultiGuide.Domain     → Entities, Enums, Interfaces của Repository
MultiGuide.Tests      → Unit tests + Integration tests
```

**Hướng phụ thuộc:** `Api → Business → Data → Domain` (Domain không phụ thuộc ai).

Quy tắc:

- **Controller chỉ làm 3 việc:** nhận request, validate DTO, gọi Service và trả response. **Cấm** đặt logic nghiệp vụ trong Controller.
- **Controller không được dùng `DbContext` hay Repository trực tiếp.** Chỉ gọi Service qua interface.
- **Service chứa toàn bộ logic nghiệp vụ** (kiểm tra trạng thái session, tính khoảng cách, chọn audio theo ngôn ngữ...). Service gọi Repository qua interface.
- **Repository chỉ truy vấn dữ liệu**, không chứa logic nghiệp vụ.
- **Không trả Entity ra ngoài API.** Luôn map sang DTO (viết tay, không cần AutoMapper).
- Dùng **Dependency Injection** qua interface cho mọi Service và Repository. Đăng ký trong `Program.cs` hoặc extension method.
- Code bất đồng bộ: dùng `async/await` và hậu tố `Async` cho method truy cập I/O.

---

## 4. Mô hình dữ liệu

| Bảng | Mục đích / cột chính |
|---|---|
| `Tour` | `Code` (unique, gắn với QR), `Name` |
| `TourLanguage` | Ngôn ngữ mà tour hỗ trợ |
| `Attraction` | `Name`, `Latitude`, `Longitude`, `RadiusMeters`, `OrderIndex`, thuộc Tour |
| `AttractionContent` | `AttractionId` + `Language` (**unique cặp này**), `Title`, `DescriptionText`, `AudioUrl`, `Status` (`DRAFT`/`APPROVED`), `Source` (`MANUAL`/`AI_GENERATED`) |
| `VisitSession` | `TourId`, `Language`, `Status` (`PENDING` → `ACTIVE` → `COMPLETED`), thời gian tạo/kết thúc |
| `Payment` | `SessionId`, `Amount` (2 USD), `Status` (`PENDING`/`SUCCESS`/`FAILED`) |
| `ListenHistory` | `SessionId`, `AttractionId`, thời điểm nghe |

Ghi chú thiết kế:

- Một session có thể có **nhiều Payment** (thất bại rồi thử lại), chỉ cần 1 `SUCCESS` để session thành `ACTIVE`.
- Nội dung thiết kế kiểu **một địa điểm, nhiều bản ngôn ngữ**. Cột `Status` và `Source` có sẵn để v2 cắm pipeline AI mà không đổi schema.
- Mọi thay đổi schema phải qua **EF Core Migration**, không sửa DB bằng tay.

---

## 5. Danh sách API (MVP)

| Bước trong luồng | API |
|---|---|
| Quét QR, chọn ngôn ngữ | `GET /tours/{code}`, `POST /sessions` |
| Thanh toán (có thử lại) | `POST /sessions/{id}/payments`, `POST /payments/{id}/mock-callback` |
| Nhận diện vị trí | `POST /sessions/{id}/location` (nhận lat/lng, trả địa điểm gần nhất hoặc `null`) |
| Xem thông tin địa điểm | `GET /sessions/{id}/attractions/{aid}` |
| Nghe thuyết minh | `GET /sessions/{id}/attractions/{aid}/audio` (ghi `ListenHistory`) |
| Rời khu du lịch | `POST /sessions/{id}/complete` |

**Quy tắc truy cập:** mọi API location / attraction / audio **bắt buộc session phải ở trạng thái `ACTIVE`**. Kiểm tra bằng một Filter hoặc Guard dùng chung, không copy-paste kiểm tra vào từng endpoint.

---

## 6. Quy tắc nghiệp vụ quan trọng

- Ngôn ngữ của session phải nằm trong danh sách ngôn ngữ mà tour hỗ trợ.
- Không cho tạo payment mới nếu session đã `ACTIVE` hoặc `COMPLETED`.
- Payment `SUCCESS` → session chuyển `ACTIVE`. Payment `FAILED` → cho phép tạo payment mới.
- Nhận diện vị trí: tính khoảng cách bằng **Haversine** trong một class/hàm thuần (`GeoHelper`), không phụ thuộc DB. Chọn địa điểm gần nhất nằm trong `RadiusMeters`, không có thì trả `null`.
- Audio và description lấy theo `Language` của **session**, không lấy từ tham số do client gửi lên.
- Thanh toán đi qua interface `IPaymentGateway`. MVP chỉ cài `MockPaymentGateway`. Không gọi trực tiếp cổng thanh toán thật.

---

## 7. Phạm vi MVP

**Làm trong MVP:**
- Toàn bộ API ở mục 5, thanh toán **mock**, audio là **file tĩnh** (DB chỉ lưu đường dẫn), nội dung viết sẵn trong DB.

**KHÔNG làm trong MVP (đừng tự thêm):**
- Thanh toán thật (Stripe/PayPal), gọi AI/LLM/TTS lúc chạy, Redis/cache, message queue, phân quyền admin, deploy cloud, microservices.
- **Tuyệt đối không gọi AI real-time khi khách bấm nghe.** Nội dung phải lấy từ DB.

**Giai đoạn nâng cấp (sau MVP, chỉ làm khi được yêu cầu):** thanh toán sandbox, pipeline AI (dịch → TTS → lưu → duyệt), cloud storage cho audio, CD, admin API, cache/rate limit/logging.

---

## 8. Quy ước code

- Tên class/method/property: `PascalCase`; biến cục bộ/tham số: `camelCase`; interface bắt đầu bằng `I`.
- Tên tiếng Anh cho code, comment có thể tiếng Việt hoặc Anh nhưng nhất quán trong một file.
- DTO đặt tên rõ mục đích: `CreateSessionRequest`, `SessionResponse`...
- Validate input bằng DataAnnotations hoặc FluentValidation (chỉ dùng nếu đã được thêm vào dự án).
- **Response lỗi thống nhất** qua global exception middleware (mã lỗi + message). Không để lộ stack trace. Không `catch` rồi nuốt lỗi.
- Không hard-code chuỗi kết nối, mật khẩu, secret. Dùng `appsettings` + biến môi trường. **Không commit file `.env` hay secret.**
- Không để code chết, `TODO` không rõ chủ, hay `Console.WriteLine` debug trong code nộp.
- Giữ method ngắn, một method làm một việc.

---

## 9. Testing

- **Mỗi Service mới phải có unit test** (xUnit + Moq, mock Repository).
- Logic thuần như `GeoHelper` phải test các trường hợp: trong bán kính, ngoài bán kính, nhiều điểm gần nhau.
- Luồng chính phải có **integration test**: tạo session → thanh toán → location → audio → complete.
- Test cho cả nhánh lỗi (session chưa `ACTIVE`, ngôn ngữ không hỗ trợ, payment thất bại/thử lại, thanh toán hai lần).
- Code chưa pass `dotnet test` thì chưa được coi là xong.

---

## 10. Git workflow và CI/CD

- Nhánh: `main` (ổn định), `develop` (tích hợp), `feature/<tên-ngắn>` cho mỗi task.
- Mỗi task một branch, mở **Pull Request vào `develop`**. Không push thẳng vào `main`/`develop`.
- Commit message rõ ràng, ví dụ: `feat(session): add create session endpoint`, `fix(location): correct haversine rounding`.
- PR chỉ được merge khi **CI xanh** (build + test) và **có người review**.
- CI (GitHub Actions) chạy `dotnet restore`, `dotnet build`, `dotnet test` mỗi lần push/PR; giai đoạn hoàn thiện thêm bước build Docker image.
- Demo chạy bằng `docker compose up`. Mọi thay đổi không được làm hỏng lệnh này.

---

## 11. Quy trình AI agent phải làm cho mỗi task

**Trước khi code:**
1. Đọc file này và xem **bản mẫu đã có sẵn trong repo** (tính năng làm trọn vẹn Controller → Service → Repository → test) để viết đúng phong cách.
2. Xác nhận task thuộc phạm vi MVP (mục 7) và đúng lớp trách nhiệm (mục 3).
3. Nếu task mơ hồ hoặc thiếu thông tin (input/output API, quy tắc nghiệp vụ), **hỏi lại** thay vì đoán.

**Khi code:**
4. Chỉ sửa những file liên quan đến task. Không refactor lan man, không đổi cấu trúc solution.
5. Có thay đổi schema thì tạo Migration, và cập nhật seed data nếu cần.
6. Viết test đi kèm.

**Trước khi báo hoàn thành:**
7. Chạy `dotnet build` và `dotnet test`, cả hai phải thành công.
8. Tóm tắt: đã sửa file nào, logic nằm ở lớp nào, đã test những gì, còn giả định nào chưa chắc.

---

## 12. Lộ trình (để biết task đang ở đâu)

- **Sprint 0 — Nền móng & CI:** repo, solution 5 project, config, Docker Compose, GitHub Actions.
- **Sprint 1 — Database:** ERD, entity, DbContext, migration, seed data.
- **Sprint 2 — Tour & Session:** `GET /tours/{code}`, `POST /sessions`, exception middleware, validation.
- **Sprint 3 — Thanh toán mock:** payment, mock-callback, Guard kiểm tra `ACTIVE`, `IPaymentGateway`. *(Mục tiêu xong trước kiểm tra giữa kỳ tuần 7.)*
- **Sprint 4 — Location & Audio:** `GeoHelper`, nhận diện địa điểm, thông tin địa điểm, audio, `ListenHistory`, complete.
- **Sprint 5 — Hoàn thiện MVP:** Swagger đầy đủ, integration test, CI build Docker, README, CORS, health check.

> Cập nhật trạng thái các sprint ở đây khi hoàn thành: _(đánh dấu ✅ cạnh sprint đã xong)_

---

## 13. Cấm tuyệt đối

- Đặt logic nghiệp vụ trong Controller hoặc Repository.
- Trả Entity trực tiếp ra API.
- Commit secret/mật khẩu/`.env`.
- Gọi AI/dịch vụ bên ngoài trong luồng khách đang dùng (real-time).
- Tự ý đổi tech stack, thêm thư viện lớn, hoặc đổi cấu trúc 3 lớp.
- Sửa schema DB bằng tay mà không qua Migration.
- Merge khi CI đỏ hoặc test chưa pass.
