# PRD v1.1 — MultiGuide: Backend hệ thống thuyết minh tự động đa ngôn ngữ

> **Dùng cùng `AGENTS.md`.** PRD = *làm cái gì* (story, AC, ERD, API). AGENTS.md = *làm thế nào* (quy ước, quy tắc tầng). Mâu thuẫn giữa hai file → xem AGENTS.md mục 1 và mục 13.
> Bản chuyển từ PRD_v1.1.docx sang Markdown; nội dung giữ nguyên, trừ một chỗ sửa lỗi gõ: US-27 ghi "2 role: super_admin" → nay là `content_manager` và `super_admin` (khớp ma trận quyền mục 4.3).

Môn Công nghệ phần mềm · Phiên bản 1.1 · 09/10/2026 · Tác giả: Quang (BE Dev) · Phạm vi tài liệu: **Backend kiến trúc 3 lớp + CI/CD**

**Quy ước:** (MVP) = nằm trong bản MVP; không đánh dấu = Later. Ưu tiên theo MoSCoW. Cột **Owner**: BE / FE / DevOps. Mọi thời gian lưu và trả về theo **UTC (ISO 8601)**.

**Thay đổi so với v1.0:** chốt geofence stateless ở server (debounce/cooldown ở client); AccessPass là token opaque; sửa quy tắc phụ thuộc tầng; tách story theo Owner; thêm API còn thiếu; sửa ERD (idempotency, khóa tài khoản, index); AC CI/CD đo được; dời CD lên Sprint 1; chốt phạm vi demo; cắt phần so sánh bài mẫu ra ADR.

## 1. Tổng quan

### 1.1 Vấn đề

Khách du lịch quốc tế thiếu hướng dẫn viên đúng ngôn ngữ; thuê guide tốn kém, bảng tĩnh nhàm và ít ngôn ngữ. Đơn vị vận hành khó cập nhật nội dung đa ngôn ngữ đồng bộ cho nhiều điểm tham quan.

### 1.2 Giải pháp

**MultiGuide** là web app (PWA): khách quét QR tại cổng, chọn ngôn ngữ, thanh toán một lần (2 USD, cổng mock ở MVP), bật GPS; khi đến gần điểm tham quan (POI) thì xem thông tin và nghe audio đúng ngôn ngữ. Admin quản lý nội dung qua Admin Portal. **Tài liệu này chỉ đặc tả phần Backend (REST API + CSDL + hạ tầng CI/CD)**; giao diện, GPS trên trình duyệt, debounce/cooldown, bản đồ thuộc FE.

### 1.3 Phạm vi chốt cho MVP

| Hạng mục | Giá trị chốt |
|---|---|
| Số khu du lịch demo | 1 |
| Số POI | 6 |
| Ngôn ngữ | vi, en, ja (thêm ngôn ngữ = thêm dữ liệu, không sửa code) |
| Thanh toán | Mock gateway, qua interface IPaymentGateway |
| Thời hạn AccessPass | 24 giờ kể từ issued_at, mỗi pass gắn 1 khu |
| Stack | ASP.NET Core, EF Core, MySQL, Docker Compose, GitHub Actions |

**Ngoài phạm vi MVP:** thanh toán thật, AI dịch/TTS, offline pack, dashboard, owner portal, app native, bản đồ (FE).

### 1.4 Actor

- **Tourist:** khách dùng điện thoại, không cài app, không tài khoản.

- **Content Manager:** nhân viên quản lý nội dung, QR.

- **Super Admin:** quản lý tài khoản, phân quyền.

- **Payment Provider:** hệ thống ngoài (Mock ở MVP).

### 1.5 Mục tiêu và chỉ số (đo được ở BE)

| Chỉ số | Ngưỡng MVP | Cách đo |
|---|---|---|
| p95 GET /pois/nearby (≤ 500 POI) | ≤ 300 ms | Test tải đơn giản (k6/bombardier) trong CI nightly hoặc thủ công trước báo cáo |
| p95 GET /pois/{id} | ≤ 200 ms | Như trên |
| Test coverage tầng Business | ≥ 60% | Coverlet, CI fail nếu thấp hơn |
| CI trên main | Luôn xanh | Branch protection |
| Thời gian CD từ merge đến /health/ready OK | ≤ 10 phút | Thời gian workflow |
| Khởi động toàn hệ thống | 1 lệnh docker compose up | Kiểm tra thủ công |

### 1.6 Giả định và ràng buộc

- Nhóm 2–3 người; báo cáo giữa kỳ (tuần 7), cuối kỳ (tuần 11–12); GitHub + CI/CD.

- Geolocation của trình duyệt cần HTTPS, nên **môi trường demo bắt buộc có HTTPS** (xem mục 5).

- iOS Safari chặn auto-play, MVP dùng nút **Nghe** (FE).

## 2. Product Backlog

### 2.1 Epic

| ID | Epic | MVP |
|---|---|---|
| E1 | Phiên truy cập và ngôn ngữ | (MVP) |
| E2 | Thanh toán và AccessPass | (MVP) |
| E3 | Tìm POI gần (Geo) | (MVP) |
| E4 | Nội dung và audio | (MVP) |
| E5 | Quản trị nội dung (Admin API) | (MVP) |
| E6 | Tài khoản và phân quyền | (MVP) |
| E7 | Ghi log lượt nghe | (MVP) |
| E8 | Vận hành: CI/CD, health, Docker | (MVP) |

### 2.2 User story MVP (viết theo góc nhìn API)

**E1 — Phiên truy cập và ngôn ngữ**

| ID | Story | Acceptance Criteria | Owner | Ưu tiên |
|---|---|---|---|---|
| US-01 | Là FE, tôi gọi GET /areas/{code} để lấy thông tin khu từ mã trong QR | 200 kèm tên, giá, thời hạn pass; mã không tồn tại hoặc khu tắt → 404 AREA_NOT_FOUND | BE | Must |
| US-02 | Là FE, tôi lấy danh sách ngôn ngữ đang bật của khu | GET /areas/{code}/languages trả các ngôn ngữ is_active có ít nhất một POI có bản dịch | BE | Must |
| US-03 | Là FE, tôi tạo session với ngôn ngữ đã chọn | POST /sessions trả session_id (GUID); ngôn ngữ không hợp lệ → 422 | BE | Must |
| US-04 | Là FE, tôi đổi ngôn ngữ giữa chừng | PATCH /sessions/{id}/language cập nhật; các request sau dùng ngôn ngữ mới | BE | Should |
| US-05 | Là Tourist, tôi muốn app tự gợi ý ngôn ngữ theo trình duyệt | FE đọc navigator.language, cho đổi tay | FE | Could |

**E2 — Thanh toán và AccessPass**

| ID | Story | Acceptance Criteria | Owner | Ưu tiên |
|---|---|---|---|---|
| US-06 | Là FE, tôi gọi thanh toán 2 USD cho session | POST /payments bắt buộc header Idempotency-Key; thành công → 201 kèm access_token (chỉ trả một lần) và expires_at; thất bại → 402 PAYMENT_FAILED, cho thử lại với key mới | BE | Must |
| US-07 | Là hệ thống, tôi không trừ tiền hai lần | Cùng (session_id, key) và cùng body → trả lại kết quả cũ (200); cùng key khác body → 422 IDEMPOTENCY_KEY_REUSED; hai request song song cùng key chỉ tạo 1 payment (unique constraint) | BE | Must |
| US-08 | Là hệ thống, tôi cấp AccessPass sau thanh toán thành công | Sinh token ngẫu nhiên 32 byte (CSPRNG), DB chỉ lưu SHA-256; hạn 24 giờ; mỗi payment tối đa 1 pass | BE | Must |
| US-09 | Là hệ thống, tôi chặn người chưa trả tiền | Thiếu token → 401 PASS_MISSING; token sai hoặc hết hạn hoặc bị thu hồi → 403 PASS_INVALID/PASS_EXPIRED; pass khác khu so với POI → 403 PASS_AREA_MISMATCH | BE | Must |
| US-10 | Là Tourist, tôi mở lại app không phải trả lại | FE lưu token (localStorage); BE cung cấp GET /passes/current trả trạng thái và expires_at | FE + BE | Should |
| US-11 | Tích hợp cổng thanh toán thật, webhook, đối soát | — | BE | Later |

**E3 — Tìm POI gần**

| ID | Story | Acceptance Criteria | Owner | Ưu tiên |
|---|---|---|---|---|
| US-12 | Là FE, tôi gọi GET /pois/nearby?lat&lng&lang&radius | Cần AccessPass; validate lat ∈ [-90,90], lng ∈ [-180,180] (sai → 422); trả POI is_active trong bán kính (mặc định 100 m, tối đa 500 m) kèm distance_m (làm tròn 1 m) và within_trigger (= distance ≤ trigger_radius_m); sắp theo **priority giảm dần, rồi distance tăng dần**; **server stateless, không lưu tọa độ** | BE | Must |
| US-13 | Là Tourist, tôi chỉ được nhắc khi đứng trong vùng liên tục ≥ 3 giây | Debounce 3 giây dựa trên within_trigger của các lần gọi liên tiếp | FE | Must |
| US-14 | Là Tourist, tôi không bị nhắc lại một điểm trong 5 phút | Cooldown lưu ở client theo poi_id | FE | Should |
| US-15 | Hướng dẫn bật GPS, xử lý từ chối quyền, bản đồ, màn cảm ơn | — | FE | Must/Later |

**E4 — Nội dung và audio**

| ID | Story | Acceptance Criteria | Owner | Ưu tiên |
|---|---|---|---|---|
| US-16 | Là FE, tôi lấy nội dung POI theo ngôn ngữ | GET /pois/{id}?lang trả title, description, image_url, audio_url, language thực tế và fallback_language (null nếu không fallback) | BE | Must |
| US-17 | Là hệ thống, tôi fallback ngôn ngữ | Thứ tự: ngôn ngữ chọn → en → vi. POI không có bản dịch nào hoặc POI tắt → 404 POI_CONTENT_NOT_FOUND. Bản dịch có chữ nhưng thiếu audio → trả audio_url = null | BE | Must |
| US-18 | Là Tourist, tôi phát, tạm dừng, nghe lại audio | Audio phục vụ qua HTTP có **Range request** (206) và Content-Type: audio/mpeg; FE lo giao diện play/pause | BE + FE | Must |

**E5 — Quản trị nội dung (Admin API)**

| ID | Story | Acceptance Criteria | Owner | Ưu tiên |
|---|---|---|---|---|
| US-19 | Quản lý khu | CRUD; code unique (chữ thường, số, gạch ngang); validate giá > 0; không xóa cứng, dùng is_active | BE | Must |
| US-20 | Quản lý POI | CRUD; validate lat/lng; trigger_radius_m ∈ [5, 200]; priority 0–100; soft delete bằng is_active; danh sách có phân trang (page, page_size ≤ 100) và lọc theo khu | BE | Must |
| US-21 | Quản lý bản dịch | Tối đa 1 bản dịch cho mỗi (poi_id, language_code) (trùng → 409 TRANSLATION_EXISTS); title ≤ 200 ký tự | BE | Must |
| US-22 | Tải audio cho bản dịch | Chỉ mp3: kiểm tra **magic bytes** (ID3 hoặc frame sync), không tin đuôi file; tối đa 10 MB; lưu qua IFileStorage; tính SHA-256 và duration_sec; thay audio cũ thì file cũ được đánh dấu xóa | BE | Must |
| US-23 | Tải ảnh cho POI | jpg/png/webp, kiểm tra magic bytes, tối đa 5 MB | BE | Should |
| US-24 | Tạo QR cho khu | GET /admin/areas/{id}/qr trả PNG chứa URL {PUBLIC_WEB_URL}/a/{code} | BE | Must |
| US-25 | Xem tiến độ nội dung theo ngôn ngữ | GET /admin/areas/{id}/content-status trả POI nào thiếu bản dịch/audio theo từng ngôn ngữ | BE | Should |

**E6 — Tài khoản và phân quyền**

| ID | Story | Acceptance Criteria | Owner | Ưu tiên |
|---|---|---|---|---|
| US-26 | Đăng nhập admin | Mật khẩu băm Argon2id hoặc bcrypt (cost ≥ 12); JWT hạn 60 phút; sai 5 lần liên tiếp → khóa 15 phút (locked_until), trả 423 ACCOUNT_LOCKED; thông báo sai không tiết lộ email có tồn tại hay không | BE | Must |
| US-27 | Phân vai trò | 2 role: content_manager và super_admin; ma trận quyền ở mục 4.3; sai quyền → 403 FORBIDDEN | BE | Must |
| US-28 | Quản lý tài khoản admin, audit log | — | BE | Later |

**E7 — Ghi log lượt nghe**

| ID | Story | Acceptance Criteria | Owner | Ưu tiên |
|---|---|---|---|---|
| US-29 | Ghi PlayLog | POST /play-logs cần AccessPass; body gồm poi_id, language_code, listened_sec, completed; session lấy từ pass chứ không tin body; **không lưu tọa độ**; listened_sec ≥ 0 và ≤ 6 giờ | BE | Must |

**E8 — Vận hành**

| ID | Story | Acceptance Criteria | Owner | Ưu tiên |
|---|---|---|---|---|
| US-30 | CI mỗi PR/push | Đủ các bước ở mục 5.1; main có branch protection, bắt buộc CI xanh mới merge | DevOps | Must |
| US-31 | CD khi merge main | Đủ các bước ở mục 5.2, có rollback | DevOps | Must |
| US-32 | Chạy cả hệ thống bằng một lệnh | docker compose up khởi động nginx/Caddy, api, mysql; seed dữ liệu demo bằng cờ cấu hình | DevOps | Must |
| US-33 | Health check | /health (liveness, luôn 200 nếu process sống); /health/ready (kiểm tra kết nối DB, 503 nếu lỗi) | BE | Must |

### 2.3 Backlog tương lai (Later)

| Ý tưởng | Ghi chú |
|---|---|
| Cổng thanh toán thật (Stripe/PayOS), webhook, đối soát | Thay MockPaymentGateway |
| Biên lai email | Sau thanh toán |
| Dịch AI, TTS tự sinh audio | Bản nháp chờ duyệt, job nền |
| Gói audio offline | Checksum theo ngôn ngữ |
| Dashboard thống kê, audit log, quản lý tài khoản admin | Cần AuditLog |
| Bản đồ tương tác, auto-play, speechSynthesis fallback | FE |
| Spatial index khi POI lớn | Đổi truy vấn nearby |

### 2.4 Luồng chính của khách (sequence, mô tả)

1.  FE quét QR → GET /areas/{code} → GET /areas/{code}/languages.

2.  POST /sessions → nhận session_id.

3.  POST /payments (Idempotency-Key) → nhận access_token một lần, FE lưu.

4.  FE đọc GPS, định kỳ gọi GET /pois/nearby kèm Authorization: Bearer <access_token>; FE debounce/cooldown dựa trên within_trigger.

5.  Khi hiện thông báo và khách bấm Nghe → GET /pois/{id}?lang → phát audio_url.

6.  Nghe xong hoặc dừng → POST /play-logs.

## 3. Yêu cầu phi chức năng (NFR)

| Nhóm | Yêu cầu |
|---|---|
| Hiệu năng | Theo mục 1.5; truy vấn nearby lọc sơ bộ bằng bounding box trên (lat, lng) rồi mới tính Haversine |
| Bảo mật | HTTPS; HSTS; JWT admin ký bằng key từ secret; AccessPass opaque, DB lưu SHA-256; không lưu thông tin thẻ; CORS chỉ cho phép origin của web (cấu hình); security header cơ bản |
| Rate limit | /admin/auth/login: 10 request/phút/IP; /payments: 5 request/phút/session; chung: 120 request/phút/IP (ASP.NET Core RateLimiter) |
| Quyền riêng tư | Không lưu tọa độ khách ở DB hay log; log ứng dụng không ghi lat/lng và token; tuân thủ tinh thần Nghị định 13/2023/NĐ-CP |
| Lỗi | Chuẩn **ProblemDetails (RFC 7807)** kèm trường code (mã lỗi ổn định cho FE) và traceId |
| Quan sát | Structured logging (Serilog, JSON), **correlation id** (X-Correlation-Id, sinh nếu thiếu, trả lại trong response) |
| Đa ngôn ngữ | UTF-8; collation utf8mb4_0900_ai_ci; thêm ngôn ngữ chỉ thêm dữ liệu |
| Thời gian | Lưu UTC; DTO trả ISO 8601 có hậu tố Z |
| Cấu hình và secret | Cấu hình qua biến môi trường; secret (JWT key, mật khẩu DB, SSH key) nằm ở GitHub Secrets và file .env trên server, không commit; có .env.example |
| Dữ liệu | Migration chạy lại được (idempotent) và có script rollback; backup MySQL hằng ngày bằng mysqldump cho môi trường demo |
| Chiến lược test | Unit test Service bằng mock Repository/Gateway; integration test Controller + DB thật (Testcontainers MySQL) cho các luồng: payment idempotent, pass check, nearby, upload; test nhánh fallback ngôn ngữ |
| Tương thích | API versioning /api/v1; OpenAPI/Swagger bật ở demo |

## 4. Kiến trúc 3 lớp

Backend là **modular monolith 3 lớp**, đóng gói thành 3 project chính cùng 1 project dùng chung.

### 4.1 Phân tầng và trách nhiệm

| Tầng (project) | Chứa | Không được |
|---|---|---|
| **Presentation** (MultiGuide.Api) | Controller, DTO request/response, validation, middleware (xác thực JWT, xác thực AccessPass, xử lý lỗi, correlation id, log), cấu hình DI (composition root) | Chứa quy tắc nghiệp vụ; truy cập DbContext trực tiếp |
| **Business** (MultiGuide.Business) | Service, quy tắc nghiệp vụ, **định nghĩa mọi interface**: IXxxRepository, IPaymentGateway, IFileStorage, IClock, IPasswordHasher | Biết EF Core, đường dẫn file, SDK thanh toán, HTTP |
| **Data/Infra** (MultiGuide.Data) | DbContext, migration, Repository (implement interface của Business), LocalDiskFileStorage, MockPaymentGateway | Chứa quy tắc nghiệp vụ |
| **Shared** (MultiGuide.Domain) | Entity, enum, hằng số, exception nghiệp vụ | Phụ thuộc bất kỳ tầng nào |

**Quy tắc phụ thuộc:** Api → Business ← Data, cả hai cùng tham chiếu Domain. Data implement interface do Business định nghĩa (đảo ngược phụ thuộc); Api đăng ký DI. Business không tham chiếu Data hay Api. Kiểm tra bằng test kiến trúc (NetArchTest) trong CI.

### 4.2 Thành phần

| Thành phần | Tầng | Trách nhiệm |
|---|---|---|
| Controllers: Area, Session, Payment, Poi, Play, AdminAuth, AdminArea, AdminPoi, AdminTranslation, AdminMedia | Presentation | Nhận request, validate DTO, gọi Service, trả response |
| Middleware: Correlation, ExceptionHandler (→ ProblemDetails), JwtAuth, AccessPassAuth | Presentation | **Chỉ xác thực** danh tính và tính hợp lệ của token |
| SessionService | Business | Tạo session, đổi ngôn ngữ |
| PaymentService | Business | Idempotency, gọi IPaymentGateway, tạo Payment |
| AccessPassService | Business | Cấp, xác minh (băm, tra, hạn, trạng thái), thu hồi pass |
| GeoService | Business | Haversine, bounding box, within_trigger, sắp xếp (stateless) |
| ContentService | Business | Lấy nội dung, fallback ngôn ngữ, dựng audio_url |
| AdminContentService | Business | CRUD khu/POI/bản dịch, validate upload |
| AuthService | Business | Đăng nhập, khóa tài khoản, phát JWT |
| AnalyticsService | Business | Ghi PlayLog |
| Repository, DbContext | Data | Truy vấn/lưu MySQL |
| LocalDiskFileStorage | Data | Lưu và phục vụ file (sau này thay bằng S3/Blob) |
| MockPaymentGateway | Data | Giả lập thanh toán (cấu hình được kết quả để test lỗi) |

### 4.3 Phân định xác thực và phân quyền

- **Middleware (Presentation):** token tồn tại, đúng định dạng, băm tra DB, chưa hết hạn, chưa bị thu hồi → gắn PassContext (session_id, area_id) vào request. JWT hợp lệ → gắn AdminContext (user_id, role).

- **Service (Business):** kiểm tra quyền theo tài nguyên: pass có thuộc khu của POI không (PASS_AREA_MISMATCH), role nào được thao tác gì.

| Tài nguyên | content_manager | super_admin |
|---|---|---|
| Khu, POI, bản dịch, audio, ảnh, QR | Đọc/ghi | Đọc/ghi |
| Xóa mềm khu | Không | Có |
| Tài khoản admin (Later) | Không | Có |

### 4.4 Sơ đồ (mô tả để vẽ lại)

- **Component diagram:** 3 hộp tầng xếp dọc; mũi tên Api → Business, Data → Business (implements), cả ba → Domain; MySQL, thư mục audio, Payment Provider nằm ngoài, nối với Data qua interface.

- **Use case:** actor Tourist (quét QR, chọn ngôn ngữ, thanh toán, xem POI gần, nghe audio), Content Manager (quản lý nội dung, tạo QR), Super Admin (kế thừa Content Manager + quản lý tài khoản), Payment Provider (xử lý thanh toán).

## 5. Triển khai, CI/CD

### 5.1 CI (mỗi PR và push)

1.  dotnet restore (có cache NuGet).

2.  dotnet build -warnaserror.

3.  dotnet format --verify-no-changes.

4.  dotnet test kèm coverage; **fail nếu coverage Business < 60%**; có test kiến trúc (không vi phạm quy tắc phụ thuộc).

5.  docker build (không push) để chắc chắn image build được.

6.  Quét lỗ hổng gói NuGet (dotnet list package --vulnerable), fail nếu có mức High trở lên.

**AC:** workflow chạy ≤ 8 phút; main bật branch protection (yêu cầu CI xanh và ít nhất 1 review nếu nhóm ≥ 2 người).

### 5.2 CD (merge vào main)

1.  Build image, gắn tag sha-<commit> và latest, đẩy lên **GHCR**.

2.  SSH vào server demo, docker compose pull && docker compose up -d.

3.  Chạy migration (bước riêng bằng container one-shot migrator, **không** tự migrate lúc API khởi động) trước khi chuyển lưu lượng sang API mới.

4.  Smoke test: GET /health/ready trả 200 trong 60 giây.

5.  Nếu smoke test fail: tự động deploy lại tag trước đó và đánh dấu workflow đỏ.

**AC:** thời gian từ merge đến sẵn sàng ≤ 10 phút; lần deploy đầu tiên (Sprint 1) chỉ cần /health chạy qua HTTPS trên domain thật.

### 5.3 Sơ đồ triển khai

| Node | Thành phần | Ghi chú |
|---|---|---|
| Thiết bị khách, máy admin | Trình duyệt | Cần HTTPS cho GPS |
| GitHub | Repo quang950/MultiGuide, Actions, GHCR | CI/CD, registry |
| Server demo (VPS) | Container caddy (reverse proxy, HTTPS tự động Let's Encrypt, phục vụ web tĩnh), api (ASP.NET Core), mysql, migrator (one-shot); volume audio-storage, mysql-data | Docker Compose; cần domain trỏ về VPS |
| Dịch vụ ngoài | Payment Provider (Later), OSM tiles (FE, Later) | Qua Internet |

Ghi chú: dùng Caddy để có HTTPS tự động; nếu dùng nginx thì cần certbot. Người phụ trách thiết lập domain/HTTPS: **Quang (DevOps kiêm BE)**, hoàn thành trong Sprint 1.

## 6. ERD và quan hệ dữ liệu

### 6.1 Thực thể

| Bảng | Cột chính | MVP |
|---|---|---|
| Language | **code** PK, name, is_active | (MVP) |
| TourArea | **id** PK, code (unique), name, center_lat, center_lng, radius_m, price_usd decimal(10,2), pass_ttl_hours (mặc định 24), is_active | (MVP) |
| Poi | **id** PK, area_id FK, lat, lng, trigger_radius_m (mặc định 30), priority, image_url, is_active, created_at, updated_at. Index (area_id, is_active) và (lat, lng) | (MVP) |
| PoiTranslation | **id** PK, poi_id FK, language_code FK, title, description, audio_file_id FK **unique** nullable, updated_at. Unique (poi_id, language_code) | (MVP) |
| AudioFile | **id** PK, file_path, duration_sec, size_bytes, checksum (SHA-256), source (manual/tts), created_at | (MVP) |
| VisitorSession | **id** GUID PK, area_id FK, language_code FK, started_at, ended_at | (MVP) |
| Payment | **id** PK, session_id FK, amount decimal(10,2), currency, provider, provider_ref, idempotency_key, request_hash, status (pending/succeeded/failed), created_at. **Unique (session_id, idempotency_key)** | (MVP) |
| AccessPass | **id** PK, session_id FK, payment_id FK unique, token_hash (SHA-256) **unique**, issued_at, expires_at, status (active/revoked) | (MVP) |
| PlayLog | **id** PK, session_id FK, poi_id FK, language_code FK, started_at, listened_sec, completed. Index (poi_id, started_at) | (MVP) |
| Role | **id** PK, name unique | (MVP) |
| AdminUser | **id** PK, email unique, password_hash, role_id FK, is_active, failed_login_count, **locked_until** nullable, created_at | (MVP) |
| AuditLog | id PK, admin_user_id FK, action, resource, created_at | Later |

area_id của pass **không lưu riêng**: suy ra từ AccessPass → VisitorSession → area_id, tránh dữ liệu dư.

### 6.2 Quan hệ

| Quan hệ | Bản số |
|---|---|
| TourArea — Poi | 1 – N |
| Poi — PoiTranslation | 1 – N |
| Language — PoiTranslation | 1 – N |
| PoiTranslation — AudioFile | 1 – 0..1 |
| TourArea — VisitorSession | 1 – N |
| VisitorSession — Payment | 1 – N |
| Payment — AccessPass | 1 – 0..1 |
| VisitorSession — PlayLog | 1 – N |
| Poi — PlayLog | 1 – N |
| Role — AdminUser | 1 – N |

### 6.3 Quy tắc dữ liệu và quyết định thiết kế

- **Không xóa cứng** khu, POI, bản dịch đã có dữ liệu liên quan: dùng is_active; mọi FK dùng ON DELETE RESTRICT.

- Khách không có tài khoản: danh tính là VisitorSession + AccessPass.

- Tách PoiTranslation khỏi Poi để thêm ngôn ngữ chỉ là thêm dòng; tách AudioFile để sau gắn nguồn TTS và checksum; tách Payment khỏi AccessPass để thu hồi hoặc gia hạn pass độc lập.

- **Idempotency:** request_hash = SHA-256 của body chuẩn hóa; xem US-07.

- **Pass:** token plaintext chỉ tồn tại trong response tạo pass; mất token thì khách phải mua lại (hoặc admin cấp lại ở Later).

- Nearby MVP dùng bounding box + Haversine trong Service; khi POI > vài nghìn thì chuyển sang spatial index.

## 7. API (prefix /api/v1)

| Nhóm | Endpoint | Auth | Ghi chú |
|---|---|---|---|
| Khách | GET /areas/{code} | Public | 404 nếu khu tắt hoặc không tồn tại |
| Khách | GET /areas/{code}/languages | Public | Danh sách ngôn ngữ |
| Khách | POST /sessions | Public | Trả session_id |
| Khách | PATCH /sessions/{id}/language | Session id | Đổi ngôn ngữ |
| Khách | POST /payments | Session + Idempotency-Key | Trả status, access_token (một lần), expires_at |
| Khách | GET /passes/current | AccessPass | Trạng thái và hạn |
| Khách | GET /pois/nearby?lat&lng&lang&radius | AccessPass | Trả distance_m, within_trigger |
| Khách | GET /pois/{id}?lang | AccessPass | Nội dung, audio_url, fallback_language |
| Khách | GET /media/{fileId} | AccessPass | Phục vụ audio/ảnh, hỗ trợ Range |
| Khách | POST /play-logs | AccessPass | Ghi lượt nghe |
| Admin | POST /admin/auth/login | Public + rate limit | Trả JWT |
| Admin | CRUD /admin/areas, /admin/pois, /admin/pois/{id}/translations | JWT + role | Phân trang, lọc |
| Admin | POST /admin/translations/{id}/audio | JWT | Magic bytes, ≤ 10 MB |
| Admin | POST /admin/pois/{id}/image | JWT | Magic bytes, ≤ 5 MB |
| Admin | GET /admin/areas/{id}/qr | JWT | PNG |
| Admin | GET /admin/areas/{id}/content-status | JWT | Tiến độ nội dung |
| Hệ thống | GET /health, /health/ready | Public | Liveness/readiness |

**Bảng mã lỗi:**

| HTTP | code | Khi nào |
|---|---|---|
| 401 | PASS_MISSING / AUTH_REQUIRED | Thiếu token |
| 403 | PASS_INVALID, PASS_EXPIRED, PASS_AREA_MISMATCH, FORBIDDEN | Token sai, hết hạn, sai khu, sai role |
| 404 | AREA_NOT_FOUND, POI_CONTENT_NOT_FOUND | Không có hoặc đã tắt |
| 409 | TRANSLATION_EXISTS | Trùng bản dịch |
| 402 | PAYMENT_FAILED | Cổng từ chối |
| 422 | VALIDATION_ERROR, IDEMPOTENCY_KEY_REUSED | Dữ liệu sai, key tái dùng khác body |
| 423 | ACCOUNT_LOCKED | Tài khoản bị khóa tạm |
| 429 | RATE_LIMITED | Vượt rate limit |

## 8. Kế hoạch Sprint

| Sprint | Nội dung | Story |
|---|---|---|
| S0 (xong) | Solution 3 lớp, CI cơ bản | US-30 (bản đầu) |
| S1 | ERD, entity, DbContext, migration, seed; Docker Compose + MySQL; domain + HTTPS; **CD skeleton: deploy /health lên demo** | US-32, US-33, US-31 (bản đầu) |
| S2 | Auth + RBAC; CRUD khu/POI/bản dịch; upload audio/ảnh | US-19→US-23, US-26, US-27 |
| S3 | Language, Session, Payment mock idempotent, AccessPass + middleware | US-01→US-04, US-06→US-09 |
| S4 | Nearby, Content + fallback, media Range, PlayLog, QR, content-status | US-12, US-16→US-18, US-24, US-25, US-29 |
| S5 | Coverage ≥ 60%, test kiến trúc, rate limit, logging, hardening, báo cáo | NFR, US-30 (hoàn chỉnh) |

**Definition of Done mỗi story:** code qua review, có unit test Service, CI xanh, cập nhật OpenAPI, deploy được lên demo.

## 9. Rủi ro

| Rủi ro | Tác động | Xử lý |
|---|---|---|
| GPS kém chính xác | Nhận nhầm điểm | trigger_radius_m theo POI; priority làm tiêu chí phụ; debounce ở FE |
| iOS chặn auto-play | Không tự phát | Nút Nghe (FE) |
| Nội dung nhiều ngôn ngữ tốn công | Trễ tiến độ | MVP 6 POI × 3 ngôn ngữ; endpoint content-status để theo dõi |
| Không có HTTPS | GPS không chạy trên demo | Làm ở Sprint 1, không để cuối |
| Phụ thuộc tiến độ FE | Không demo được luồng khách | Chốt hợp đồng OpenAPI ngay S1; FE dùng mock server từ file OpenAPI |
| Mất file audio khi redeploy | Mất nội dung | Volume audio-storage và backup cùng MySQL |
| Dữ liệu vị trí nhạy cảm | Rủi ro riêng tư | Server không lưu tọa độ, log không ghi tọa độ |

**Câu hỏi mở còn lại:** (1) tên domain demo; (2) tên khu và 6 POI cụ thể; (3) người phụ trách FE. Cần chốt trước cuối S1.

## Phụ lục A — ADR rút gọn (thay cho mục 8 cũ)

| Quyết định | Lựa chọn | Lý do |
|---|---|---|
| Stack | ASP.NET Core 3 lớp + MySQL | Đã có Sprint 0 và CI; dữ liệu quan hệ chặt (payment, pass, session) |
| Geofence | Server stateless tính khoảng cách; client debounce/cooldown | Không cần Redis, dễ test, không lưu tọa độ |
| AccessPass | Token opaque, lưu hash | Thu hồi được, nhất quán với việc tách Payment/AccessPass |
| Audio | File mp3 tĩnh, phục vụ qua API có Range | Kiểm soát nội dung, TTS để Later |
| Migration | Bước riêng trong pipeline | Tránh nhiều instance cùng migrate, rollback rõ ràng |
| Tham khảo bài mẫu | Học ý tưởng: bảng bản dịch, fallback, idempotency, RBAC, tách audio | Không sao chép phạm vi |