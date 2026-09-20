# 📘 ĐẶC TẢ HƯỚNG DẪN KẾT NỐI BACKEND .NET API CHO FRONTEND (REACTJS & FLUTTER)

Tài liệu này cung cấp quy chuẩn kết nối, cấu trúc dữ liệu trả về và danh sách các API đã sẵn sàng để đội ngũ Frontend (ReactJS Web & Flutter Mobile) kết nối.

---

## 1. THÔNG TIN KẾT NỐI CHUNG
* **Base URL**: `http://localhost:5000`
* **Swagger UI (API Doc & Test)**: `http://localhost:5000/swagger`
* **Content-Type**: `application/json` (ngoại trừ các API upload file sử dụng `multipart/form-data`)
* **Xác thực**: JWT Bearer Token qua HTTP Header `Authorization: Bearer <accessToken>`

---

## 2. QUY CHUẨN JSON RESPONSE TẬP TRUNG (`ApiResponse<T>`)

Mọi API (dù thành công 2xx hay thất bại 4xx/5xx) **ĐỀU TRẢ VỀ DUY NHẤT 1 CẤU TRÚC JSON CHUẨN** bên dưới:

```json
{
  "success": true,           // boolean (true nếu 2xx, false nếu 4xx/5xx)
  "statusCode": 200,        // int (200, 201, 400, 401, 403, 404, 500)
  "message": "Thông báo thành công hoặc thông báo lỗi",
  "data": { ... },           // Tập dữ liệu Object / Array / null
  "errors": null,            // Danh sách string mảng lỗi hoặc null
  "timestamp": "2026-09-19T16:00:00Z"
}
```

---

## 3. DANH SÁCH CHI TIẾT CÁC API ĐÃ HOÀN THÀNH

### 🔑 MODULE 1: AUTH & XÁC THỰC TÀI KHOẢN (`/api/v1/auth`)

#### 1. Đăng ký tài khoản mới (`POST /api/v1/auth/register`)
* **Phân quyền**: Public (Ai cũng gọi được)
* **Request Body**:
  ```json
  {
    "email": "ungvien1@gmail.com",
    "matKhau": "123456",
    "dienThoai": "0987654321",
    "vaiTro": "UngVien" // Phải là 'NhaTuyenDung' hoặc 'UngVien'
  }
  ```
* **Response Data (201 Created)**:
  ```json
  {
    "success": true,
    "statusCode": 201,
    "message": "Đăng ký tài khoản thành công. Mã OTP xác thực của bạn là: 849201 (Hết hạn trong 5 phút).",
    "data": {
      "email": "ungvien1@gmail.com",
      "otpCode": "849201"
    },
    "errors": null
  }
  ```

---

#### 2. Xác thực OTP kích hoạt tài khoản (`POST /api/v1/auth/verify-otp`)
* **Phân quyền**: Public
* **Request Body**:
  ```json
  {
    "email": "ungvien1@gmail.com",
    "maOTP": "849201"
  }
  ```
* **Response Data (200 OK)**:
  ```json
  {
    "success": true,
    "statusCode": 200,
    "message": "Xác thực OTP thành công. Tài khoản đã được kích hoạt.",
    "data": {
      "email": "ungvien1@gmail.com",
      "isVerified": true
    },
    "errors": null
  }
  ```

---

#### 3. Đăng nhập & Lấy JWT Token (`POST /api/v1/auth/login`)
* **Phân quyền**: Public
* **Request Body**:
  ```json
  {
    "email": "ungvien1@gmail.com",
    "matKhau": "123456"
  }
  ```
* **Response Data (200 OK)**:
  ```json
  {
    "success": true,
    "statusCode": 200,
    "message": "Đăng nhập thành công.",
    "data": {
      "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
      "tokenType": "Bearer",
      "expiresIn": 28800,
      "maTaiKhoan": 1,
      "email": "ungvien1@gmail.com",
      "vaiTro": "UngVien"
    },
    "errors": null
  }
  ```

---

### 🗺️ MODULE 2: DANH MỤC HỆ THỐNG (`/api/v1/danhmuc`)

#### 4. Lấy danh sách Tỉnh / Thành phố (`GET /api/v1/danhmuc/tinh`)
* **Phân quyền**: Public
* **Response Data (200 OK)**:
  ```json
  {
    "success": true,
    "statusCode": 200,
    "message": "Lấy danh sách tỉnh thành công",
    "data": [
      { "maDanhMuc": 1, "tenDanhMuc": "Hà Nội" },
      { "maDanhMuc": 2, "tenDanhMuc": "TP. Hồ Chí Minh" }
    ]
  }
  ```

#### 5. Lấy danh sách Quận / Huyện theo Tỉnh (`GET /api/v1/danhmuc/tinh/{maTinh}/quan-huyen`)
* **Phân quyền**: Public
* **Ví dụ**: `GET /api/v1/danhmuc/tinh/1/quan-huyen`
* **Response Data (200 OK)**:
  ```json
  {
    "success": true,
    "statusCode": 200,
    "message": "Lấy danh sách quận huyện thành công",
    "data": [
      { "maDanhMuc": 101, "tenDanhMuc": "Quận Ba Đình" },
      { "maDanhMuc": 102, "tenDanhMuc": "Quận Cầu Giấy" }
    ]
  }
  ```

---

### 👤 MODULE 4: HỒ SƠ ỨNG VIÊN (`/api/v1/ung-vien`)

> ⚠️ **Lưu ý**: Toàn bộ API nhóm Ứng viên đều yêu cầu gửi kèm JWT Token (Role: `UngVien`) trong Header HTTP `Authorization`.

#### 6. Lấy Hồ Sơ Ứng Viên (`GET /api/v1/ung-vien/profile`)
* **Phân quyền**: UngVien
* **Response Data (200 OK)**:
  ```json
  {
    "success": true,
    "statusCode": 200,
    "message": "Lấy hồ sơ ứng viên thành công",
    "data": {
      "MaUngVien": 1,
      "HoTen": "Nguyễn Văn A",
      "NgaySinh": "1999-01-01T00:00:00",
      "GioiTinh": "Nam",
      "MucLuongMongMuonMin": 10000000,
      "DanhSachNgoaiNgu": "[{\"NgonNgu\":\"Tiếng Anh\",\"TrinhDo\":\"IELTS 7.0\"}]"
    },
    "errors": null
  }
  ```

#### 7. Cập Nhật Hồ Sơ Ứng Viên (`PUT /api/v1/ung-vien/profile`)
* **Phân quyền**: UngVien
* **Request Body**:
  ```json
  {
    "hoTen": "Nguyễn Văn A",
    "ngaySinh": "1999-01-01",
    "gioiTinh": "Nam",
    "maDiaDiem": 1,
    "diaChiCuThe": "123 Đường X, Hà Nội",
    "maViTriMongMuon": 2,
    "gioiThieuBanThan": "Tôi là dev 3 năm kinh nghiệm...",
    "choPhepNhaTuyenDung": true,
    "mucLuongMongMuonMin": 15000000,
    "mucLuongMongMuonMax": 20000000,
    "danhSachNgoaiNgu": "[{\"NgonNgu\":\"Tiếng Anh\"}]",
    "danhSachChungChi": "[]",
    "danhSachHocVan": "[]"
  }
  ```

#### 8. Thêm Kinh Nghiệm Làm Việc (`POST /api/v1/ung-vien/kinh-nghiem`)
* **Phân quyền**: UngVien
* **Request Body**:
  ```json
  {
    "tenCongTy": "Tập đoàn công nghệ ABC",
    "chucDanh": "Senior Developer",
    "ngayBatDau": "2020-01-01",
    "ngayKetThuc": "2023-01-01",
    "moTaChiTiet": "Phát triển Backend .NET",
    "maNganhNghe": 1,
    "maViTri": 1
  }
  ```

#### 9. Xóa Kinh Nghiệm Làm Việc (`DELETE /api/v1/ung-vien/kinh-nghiem/{id}`)
* **Phân quyền**: UngVien

#### 10. Cập Nhật Kỹ Năng (`POST /api/v1/ung-vien/ky-nang`)
* **Phân quyền**: UngVien
* **Request Body**:
  ```json
  {
    "maKyNang": 1,
    "soNamKinhNghiem": 3.5
  }
  ```

---

### 📄 QUẢN LÝ CV (`/api/v1/cv`)

> ⚠️ **Lưu ý**: Nhóm CV yêu cầu gửi kèm JWT Token (Role: `UngVien`).

#### 11. Lấy Danh Sách CV (`GET /api/v1/cv`)
* **Phân quyền**: UngVien
* **Response Data (200 OK)**: Trả về Array danh sách các CV đã tải lên hoặc tạo trực tuyến.

#### 12. Upload CV Mới (`POST /api/v1/cv/upload`)
* **Phân quyền**: UngVien
* **Content-Type**: `multipart/form-data`
* **Request Form**:
  - `file`: File nhị phân (.pdf, .doc, .docx - Dung lượng tối đa 5MB)
* **Response Data (201 Created)**: 
  ```json
  {
    "success": true,
    "statusCode": 201,
    "message": "Upload CV thành công.",
    "data": {
      "FilePath": "/uploads/cvs/abcd-1234-xyz.pdf"
    },
    "errors": null
  }
  ```

#### 13. Xóa CV (`DELETE /api/v1/cv/{id}`)
* **Phân quyền**: UngVien
* **Logic Backend**: Xóa record trong CSDL và tự động xóa vĩnh viễn file vật lý trong `/uploads/cvs/`.

#### 37. Lưu Tin Tuyển Dụng (`POST /api/v1/ung-vien/viec-lam-da-luu/{maTin}`)
* **Phân quyền**: UngVien
* **Logic**: Kiểm tra chưa lưu thì thêm vào bảng `ViecLamDaLuu`.

#### 38. Bỏ Lưu Tin Tuyển Dụng (`DELETE /api/v1/ung-vien/viec-lam-da-luu/{maTin}`)
* **Phân quyền**: UngVien

#### 39. Danh Sách Tin Tuyển Dụng Đã Lưu (`GET /api/v1/ung-vien/viec-lam-da-luu`)
* **Phân quyền**: UngVien
* **Response Data**: Danh sách công việc kèm thông tin doanh nghiệp.

#### 40. Theo Dõi Doanh Nghiệp (`POST /api/v1/ung-vien/theo-doi-doanh-nghiep/{maDN}`)
* **Phân quyền**: UngVien

#### 41. Bỏ Theo Dõi Doanh Nghiệp (`DELETE /api/v1/ung-vien/theo-doi-doanh-nghiep/{maDN}`)
* **Phân quyền**: UngVien

#### 42. Danh Sách Doanh Nghiệp Đang Theo Dõi (`GET /api/v1/ung-vien/theo-doi-doanh-nghiep`)
* **Phân quyền**: UngVien

---

### 🏢 MODULE 3: DOANH NGHIỆP (`/api/v1/doanh-nghiep`)

#### 14. Lấy chi tiết Doanh Nghiệp & Tin tuyển dụng của họ (`GET /api/v1/doanh-nghiep/{id}`)
* **Phân quyền**: Public
* **Response Data**: Gồm thông tin `CongTy` và mảng `TinTuyenDung` (chỉ những tin đang ở trạng thái 'DaDuyet' và còn hạn).

#### 15. Lấy hồ sơ Doanh Nghiệp (Dành cho HR) (`GET /api/v1/doanh-nghiep/profile`)
* **Phân quyền**: NhaTuyenDung
* **Response Data**: Trả về chi tiết `HoSoDoanhNghiep` (Tự động khởi tạo rỗng nếu HR mới tạo tài khoản).

#### 16. Cập nhật hồ sơ Doanh Nghiệp (`PUT /api/v1/doanh-nghiep/profile`)
* **Phân quyền**: NhaTuyenDung
* **Content-Type**: `multipart/form-data`
* **Request Form**:
  - `TenCongTy` (Required)
  - `MaSoThue`, `QuyMo`, `MoTa`, `MaDiaDiem`, `DiaChiChiTiet`, `DuongDanWebsite`
  - `Logo`: File ảnh logo doanh nghiệp
  - `GiayPhepKinhDoanh`: File PDF/Ảnh giấy phép
* **Logic**: Khi cập nhật, hệ thống tự động gán `TrangThaiXacThuc = 'ChoDuyet'`.

---

### 📢 MODULE 5: TIN TUYỂN DỤNG (`/api/v1/tin-tuyen-dung`)

#### 17. Tìm kiếm & Lọc Tin Tuyển Dụng (`GET /api/v1/tin-tuyen-dung`)
* **Phân quyền**: Public
* **Query Parameters**:
  - `TuKhoa` (string), `MaNganhNghe` (int), `MaViTri` (int), `MaDiaDiem` (int)
  - `HinhThucLamViec` (string), `LuongMin` (decimal), `LuongMax` (decimal), `SoNamKinhNghiem` (int)
  - `Page` (default: 1), `PageSize` (default: 10)
* **Response Data**: Trả về `Items` (Mảng tin), `TotalPages`, `TotalItems`.

#### 18. Lấy chi tiết Tin Tuyển Dụng (`GET /api/v1/tin-tuyen-dung/{id}`)
* **Phân quyền**: Public
* **Response Data**: Trả về dữ liệu chi tiết của 1 tin, có kèm theo `DanhSachMaKyNang` và thông tin cơ bản của `DoanhNghiep`.

#### 19. Đăng Tin Tuyển Dụng Mới (`POST /api/v1/tin-tuyen-dung`)
* **Phân quyền**: NhaTuyenDung
* **Request Body**:
  ```json
  {
    "tieuDe": "Tuyển dụng Backend Developer .NET",
    "maNganhNghe": 1,
    "maViTri": 2,
    "maCapBac": 3,
    "maTrinhDoHocVan": 2,
    "maDiaDiem": 1,
    "diaChiLamViec": "Quận 1, TP.HCM",
    "hinhThucLamViec": "ToanThoiGian",
    "soNamKinhNghiemToiThieu": 2,
    "soLuongTuyen": 5,
    "moTaCongViec": "Làm việc với .NET 8 và SQL Server...",
    "yeuCauUngVien": "Thành thạo C#...",
    "quyenLoi": "Bảo hiểm sức khỏe, thưởng tháng 13...",
    "luongToiThieu": 15000000,
    "luongToiDa": 30000000,
    "hanNopHoSo": "2026-12-31T00:00:00",
    "danhSachMaKyNang": [1, 2, 5]
  }
  ```
* **Logic**: Tin mới mặc định được đặt `TrangThaiTin = 'ChoDuyet'`.

#### 20. Đổi Trạng Thái Tin Tuyển Dụng (`PUT /api/v1/tin-tuyen-dung/{id}/trang-thai`)
* **Phân quyền**: NhaTuyenDung
* **Request Body**:
  ```json
  {
    "status": "TamDung" // Hoặc "DaDong"
  }
  ```

---

### 📝 MODULE 6: ỨNG TUYỂN (`/api/v1/ung-tuyen`)

#### 21. Ứng viên Nộp Hồ Sơ (`POST /api/v1/ung-tuyen`)
* **Phân quyền**: UngVien
* **Request Body**:
  ```json
  {
    "maTinTuyenDung": 1,
    "maCV": 2,
    "thuGioiThieu": "Chào anh/chị, tôi rất mong muốn ứng tuyển..."
  }
  ```
* **Logic**: Backend tự động kiểm tra trùng lặp (không nộp 2 lần 1 tin) và chỉ cho nộp tin 'DaDuyet' + còn hạn. Hệ thống tự động tạo 1 dòng ghi vết trạng thái `DaNop`.

#### 22. Lấy Lịch Sử Ứng Tuyển Của Cá Nhân (`GET /api/v1/ung-tuyen/lich-su`)
* **Phân quyền**: UngVien
* **Response Data**: Mảng các công việc đã nộp kèm thông tin công ty, chức danh, trạng thái hiện tại.

#### 23. Doanh Nghiệp Lấy DS Ứng Viên Của 1 Tin (`GET /api/v1/ung-tuyen/tin/{maTin}`)
* **Phân quyền**: NhaTuyenDung
* **Response Data**: Mảng hồ sơ ứng viên (kèm file CV đính kèm) đã nộp vào tin tuyển dụng.

#### 24. Cập Nhật Trạng Thái Ứng Tuyển (`PUT /api/v1/ung-tuyen/{id}/trang-thai`)
* **Phân quyền**: NhaTuyenDung
* **Request Body**:
  ```json
  {
    "trangThaiUngTuyen": "MoiPhongVan", // 'DaNop'|'DangXuLy'|'MoiPhongVan'|'TrungTuyen'|'TuChoi'
    "ghiChu": "Hồ sơ ấn tượng"
  }
  ```

---

### 📅 MODULE 7: LỊCH PHỎNG VẤN (`/api/v1/lich-phong-van`)

#### 25. Tạo Lịch Phỏng Vấn Mới (`POST /api/v1/lich-phong-van`)
* **Phân quyền**: NhaTuyenDung
* **Request Body**:
  ```json
  {
    "maUngTuyen": 10,
    "thoiGianPhongVan": "2026-12-15T10:00:00",
    "hinhThuc": "TrucTuyen", // Hoặc 'TrucTiep'
    "diaDiemHoacDuongDan": "meet.google.com/abc-xyz"
  }
  ```
* **Logic**: Khi lên lịch, Backend tự động Update trạng thái ứng tuyển thành `MoiPhongVan` và ghi vết log.

#### 26. Ứng Viên Phản Hồi Lịch (`PUT /api/v1/lich-phong-van/{id}/phan-hoi`)
* **Phân quyền**: UngVien
* **Request Body**:
  ```json
  {
    "phanHoiUngVien": "YeuCauDoiLich", // 'DongY' | 'TuChoi' | 'YeuCauDoiLich'
    "lyDoDoiLich": "Xin phép dời sang thứ 5 vì bận lịch thi"
  }
  ```

#### 27. Cập Nhật Kết Quả Phỏng Vấn (`PUT /api/v1/lich-phong-van/{id}/ket-qua`)
* **Phân quyền**: NhaTuyenDung
* **Request Body**:
  ```json
  {
    "diemChuyenMon": 8.5,
    "nhanXetNoiBo": "Kỹ năng code tốt, giao tiếp khá",
    "ketQuaNoiBo": "Dat" // 'ChoDuyet'|'Dat'|'KhongDat'|'CanCanNhac'
  }
  ```
* **Logic**: Nếu kết quả là `Dat` (Đạt) hoặc `KhongDat` (Không đạt), Backend sẽ tự động cập nhật Trạng thái ứng tuyển lần lượt thành `TrungTuyen` hoặc `TuChoi`.

---

### 🔔 MODULE 8: THÔNG BÁO & ĐÁNH GIÁ

#### 28. Danh Sách Thông Báo Của Tôi (`GET /api/v1/thong-bao`)
* **Phân quyền**: Authorize (Mọi tài khoản)
* **Response Data**: Mảng danh sách thông báo.

#### 29. Đánh Dấu Thông Báo Đã Đọc (`PUT /api/v1/thong-bao/{id}/da-doc`)
* **Phân quyền**: Authorize (Mọi tài khoản)

#### 30. Viết Đánh Giá (`POST /api/v1/danh-gia`)
* **Phân quyền**: Authorize (Mọi tài khoản)
* **Request Body**:
  ```json
  {
    "maDoanhNghiep": 1,
    "maUngVien": 2,
    "maUngTuyen": 15,
    "nguoiDanhGia": "Candidate", // Hoặc 'Employer'
    "soSao": 5, // 1 đến 5
    "tieuDe": "Môi trường tốt",
    "nhanXet": "Rất hài lòng về buổi phỏng vấn"
  }
  ```

#### 31. Xem Điểm Đánh Giá Doanh Nghiệp (`GET /api/v1/danh-gia/doanh-nghiep/{id}`)
* **Phân quyền**: Public
* **Response Data**: Trả về `TrungBinhSao`, `TongDanhGia`, và mảng `DanhSach` đánh giá.

---

### 👑 MODULE 9: ADMIN & THỐNG KÊ (`/api/v1/admin`)

> ⚠️ **Lưu ý**: Các API này yêu cầu Header Token mang Role `Admin`.

#### 32. Lấy DS Doanh Nghiệp Chờ Duyệt (`GET /api/v1/admin/doanh-nghiep/cho-duyet`)
* **Phân quyền**: Admin

#### 33. Thẩm Định Doanh Nghiệp (`PUT /api/v1/admin/doanh-nghiep/{id}/tham-dinh`)
* **Phân quyền**: Admin
* **Request Body**:
  ```json
  {
    "trangThaiXacThuc": "DaDuyet", // Hoặc 'TuChoi'
    "lyDoTuChoi": ""
  }
  ```

#### 34. Kiểm Duyệt Tin Tuyển Dụng (`PUT /api/v1/admin/tin-tuyen-dung/{id}/kiem-duyet`)
* **Phân quyền**: Admin
* **Request Body**:
  ```json
  {
    "trangThaiTin": "DaDuyet", // Hoặc 'TuChoi'
    "lyDoTuChoi": ""
  }
  ```

#### 35. Thống Kê Tổng Quan (`GET /api/v1/admin/thong-ke/tong-quan`)
* **Phân quyền**: Admin
* **Response Data**: `TongDoanhNghiep`, `TongUngVien`, `TongTinTuyenDung`, `TongLuotUngTuyen`.

#### 36. Sao Lưu Dữ Liệu Tức Thì (`POST /api/v1/admin/sao-luu`)
* **Phân quyền**: Admin
* **Request Body**:
  ```json
  {
    "loaiSaoLuu": "FULL", // 'FULL', 'DIFFERENTIAL', 'LOG'
    "duongDanThuMuc": "D:\\Backup\\" // Đường dẫn thư mục máy chủ
  }
  ```
* **Logic**: Gọi lệnh T-SQL tự động xuất file `.bak` hoặc `.trn` vào thư mục và ghi lại lịch sử vào bảng `SaoLuuDuLieu`.

---

## 4. MẪU CODE KẾT NỐI THAM KHẢO

### ReactJS (Sử dụng Axios Client Interceptor)
```javascript
import axios from 'axios';

const api = axios.create({
  baseURL: 'http://localhost:5000/api/v1',
  headers: {
    'Content-Type': 'application/json',
  },
});

// Tự động gắn Token vào Request
api.interceptors.request.use((config) => {
  const token = localStorage.getItem('accessToken');
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

export default api;
```

### Flutter (Sử dụng Dio Package)
```dart
import 'package:dio/dio.dart';

class ApiClient {
  static final Dio dio = Dio(
    BaseOptions(
      baseUrl: 'http://localhost:5000/api/v1',
      headers: {'Content-Type': 'application/json'},
    ),
  );

  static void setAuthToken(String token) {
    dio.options.headers['Authorization'] = 'Bearer $token';
  }
}
```
