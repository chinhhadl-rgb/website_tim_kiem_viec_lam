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
