using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;
using RecruitmentSystem.DTOs.Auth;
using RecruitmentSystem.Models;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace RecruitmentSystem.Services
{
    public class AuthService : IAuthService
    {
        private readonly string _connectionString;
        private readonly IConfiguration _configuration;
        private readonly IEmailService _emailService;

        public AuthService(IConfiguration configuration, IEmailService emailService)
        {
            _configuration = configuration;
            _connectionString = configuration.GetConnectionString("DefaultConnection") ?? "";
            _emailService = emailService;
        }

        public async Task<ApiResponse<object>> RegisterAsync(RegisterDto dto)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    // 1. Kiểm tra Email đã tồn tại chưa
                    string checkQuery = "SELECT COUNT(1) FROM TaiKhoan WHERE Email = @Email";
                    using (var checkCmd = new SqlCommand(checkQuery, connection))
                    {
                        checkCmd.Parameters.AddWithValue("@Email", dto.Email);
                        int count = Convert.ToInt32(await checkCmd.ExecuteScalarAsync());
                        if (count > 0)
                        {
                            return new ApiResponse<object>
                            {
                                Success = false,
                                StatusCode = 400,
                                Message = "Email này đã được sử dụng trong hệ thống.",
                                Data = null,
                                Errors = new[] { "Email đã tồn tại" }
                            };
                        }
                    }

                    // 2. Hash mật khẩu bằng BCrypt
                    string passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.MatKhau);

                    // 3. Sinh mã OTP 6 chữ số và thời hạn (5 phút)
                    string otpCode = new Random().Next(100000, 999999).ToString();
                    DateTime otpExpiry = DateTime.Now.AddMinutes(5);

                    // 4. Thêm tài khoản mới (TrangThaiHoatDong = 0/false)
                    string insertQuery = @"
                        INSERT INTO TaiKhoan (Email, MatKhau, DienThoai, VaiTro, TrangThaiHoatDong, DaDongYDieuKhoan, MaOTP, ThoiHanOTP, NgayTao)
                        VALUES (@Email, @MatKhau, @DienThoai, @VaiTro, 0, 1, @MaOTP, @ThoiHanOTP, GETDATE())";

                    using (var insertCmd = new SqlCommand(insertQuery, connection))
                    {
                        insertCmd.Parameters.AddWithValue("@Email", dto.Email);
                        insertCmd.Parameters.AddWithValue("@MatKhau", passwordHash);
                        insertCmd.Parameters.AddWithValue("@DienThoai", (object?)dto.DienThoai ?? DBNull.Value);
                        insertCmd.Parameters.AddWithValue("@VaiTro", dto.VaiTro);
                        insertCmd.Parameters.AddWithValue("@MaOTP", otpCode);
                        insertCmd.Parameters.AddWithValue("@ThoiHanOTP", otpExpiry);

                        await insertCmd.ExecuteNonQueryAsync();
                    }

                    // 5. Gửi mã OTP trực tiếp về Email
                    await _emailService.SendOtpEmailAsync(dto.Email, otpCode, "xác thực đăng ký tài khoản");

                    return new ApiResponse<object>
                    {
                        Success = true,
                        StatusCode = 201,
                        Message = $"Đăng ký tài khoản thành công. Mã OTP 6 số đã được gửi trực tiếp về email {dto.Email} (Hết hạn trong 5 phút).",
                        Data = new { email = dto.Email },
                        Errors = null
                    };
                }
            }
            catch (Exception ex)
            {
                return new ApiResponse<object>
                {
                    Success = false,
                    StatusCode = 500,
                    Message = "Lỗi hệ thống khi đăng ký tài khoản.",
                    Data = null,
                    Errors = new[] { ex.Message }
                };
            }
        }

        public async Task<ApiResponse<object>> VerifyOtpAsync(VerifyOtpDto dto)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    string selectQuery = "SELECT MaTaiKhoan, TrangThaiHoatDong, MaOTP, ThoiHanOTP FROM TaiKhoan WHERE Email = @Email";
                    
                    int maTaiKhoan = 0;
                    bool trangThaiHoatDong = false;
                    string? dbOtp = null;
                    DateTime? thoiHanOTP = null;

                    using (var cmd = new SqlCommand(selectQuery, connection))
                    {
                        cmd.Parameters.AddWithValue("@Email", dto.Email);
                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            if (!reader.HasRows)
                            {
                                return new ApiResponse<object>
                                {
                                    Success = false,
                                    StatusCode = 404,
                                    Message = "Không tìm thấy tài khoản với email này.",
                                    Data = null,
                                    Errors = new[] { "Tài khoản không tồn tại" }
                                };
                            }

                            await reader.ReadAsync();
                            maTaiKhoan = reader.GetInt32(0);
                            trangThaiHoatDong = reader.GetBoolean(1);
                            dbOtp = reader.IsDBNull(2) ? null : reader.GetString(2);
                            thoiHanOTP = reader.IsDBNull(3) ? null : reader.GetDateTime(3);
                        }
                    }

                    if (trangThaiHoatDong)
                    {
                        return new ApiResponse<object>
                        {
                            Success = false,
                            StatusCode = 400,
                            Message = "Tài khoản này đã được kích hoạt trước đó.",
                            Data = null
                        };
                    }

                    if (dbOtp != dto.MaOTP)
                    {
                        return new ApiResponse<object>
                        {
                            Success = false,
                            StatusCode = 400,
                            Message = "Mã OTP không chính xác.",
                            Data = null,
                            Errors = new[] { "Mã OTP không khớp" }
                        };
                    }

                    if (thoiHanOTP == null || thoiHanOTP < DateTime.Now)
                    {
                        return new ApiResponse<object>
                        {
                            Success = false,
                            StatusCode = 400,
                            Message = "Mã OTP đã hết hạn. Vui lòng yêu cầu lại mã OTP mới.",
                            Data = null,
                            Errors = new[] { "Mã OTP hết hạn" }
                        };
                    }

                    // Kích hoạt tài khoản
                    string updateQuery = "UPDATE TaiKhoan SET TrangThaiHoatDong = 1, MaOTP = NULL, ThoiHanOTP = NULL WHERE MaTaiKhoan = @MaTaiKhoan";
                    using (var updateCmd = new SqlCommand(updateQuery, connection))
                    {
                        updateCmd.Parameters.AddWithValue("@MaTaiKhoan", maTaiKhoan);
                        await updateCmd.ExecuteNonQueryAsync();
                    }

                    return new ApiResponse<object>
                    {
                        Success = true,
                        StatusCode = 200,
                        Message = "Xác thực OTP thành công. Tài khoản đã được kích hoạt.",
                        Data = new { email = dto.Email, isVerified = true },
                        Errors = null
                    };
                }
            }
            catch (Exception ex)
            {
                return new ApiResponse<object>
                {
                    Success = false,
                    StatusCode = 500,
                    Message = "Lỗi hệ thống khi xác thực OTP.",
                    Data = null,
                    Errors = new[] { ex.Message }
                };
            }
        }

        public async Task<ApiResponse<TokenResponseDto>> LoginAsync(LoginDto dto)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    string selectQuery = "SELECT MaTaiKhoan, Email, MatKhau, VaiTro, TrangThaiHoatDong FROM TaiKhoan WHERE Email = @Email";

                    int maTaiKhoan = 0;
                    string email = "";
                    string storedHash = "";
                    string vaiTro = "";
                    bool trangThaiHoatDong = false;

                    using (var cmd = new SqlCommand(selectQuery, connection))
                    {
                        cmd.Parameters.AddWithValue("@Email", dto.Email);
                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            if (!reader.HasRows)
                            {
                                return new ApiResponse<TokenResponseDto>
                                {
                                    Success = false,
                                    StatusCode = 401,
                                    Message = "Email hoặc mật khẩu không chính xác.",
                                    Data = null,
                                    Errors = new[] { "Thông tin đăng nhập không hợp lệ" }
                                };
                            }

                            await reader.ReadAsync();
                            maTaiKhoan = reader.GetInt32(0);
                            email = reader.GetString(1);
                            storedHash = reader.GetString(2);
                            vaiTro = reader.GetString(3);
                            trangThaiHoatDong = reader.GetBoolean(4);
                        }
                    }

                    // Verifying password
                    if (!BCrypt.Net.BCrypt.Verify(dto.MatKhau, storedHash))
                    {
                        return new ApiResponse<TokenResponseDto>
                        {
                            Success = false,
                            StatusCode = 401,
                            Message = "Email hoặc mật khẩu không chính xác.",
                            Data = null,
                            Errors = new[] { "Thông tin đăng nhập không hợp lệ" }
                        };
                    }

                    // Nếu chưa kích hoạt tài khoản, sinh lại OTP và tự động gửi email cho người dùng
                    if (!trangThaiHoatDong)
                    {
                        string newOtp = new Random().Next(100000, 999999).ToString();
                        DateTime newExpiry = DateTime.Now.AddMinutes(5);

                        string updateOtpQuery = "UPDATE TaiKhoan SET MaOTP = @MaOTP, ThoiHanOTP = @ThoiHanOTP WHERE MaTaiKhoan = @MaTaiKhoan";
                        using (var updateCmd = new SqlCommand(updateOtpQuery, connection))
                        {
                            updateCmd.Parameters.AddWithValue("@MaOTP", newOtp);
                            updateCmd.Parameters.AddWithValue("@ThoiHanOTP", newExpiry);
                            updateCmd.Parameters.AddWithValue("@MaTaiKhoan", maTaiKhoan);
                            await updateCmd.ExecuteNonQueryAsync();
                        }

                        // Gửi Mail OTP
                        await _emailService.SendOtpEmailAsync(email, newOtp, "kích hoạt tài khoản đăng nhập");

                        return new ApiResponse<TokenResponseDto>
                        {
                            Success = false,
                            StatusCode = 403,
                            Message = $"Tài khoản chưa được xác thực. Mã OTP kích hoạt mới đã được gửi về email {email}.",
                            Data = null,
                            Errors = new[] { "Tài khoản chưa kích hoạt" }
                        };
                    }

                    // Cập nhật DangNhapCuoi
                    string updateLoginTimeQuery = "UPDATE TaiKhoan SET DangNhapCuoi = GETDATE() WHERE MaTaiKhoan = @MaTaiKhoan";
                    using (var updateCmd = new SqlCommand(updateLoginTimeQuery, connection))
                    {
                        updateCmd.Parameters.AddWithValue("@MaTaiKhoan", maTaiKhoan);
                        await updateCmd.ExecuteNonQueryAsync();
                    }

                    // Sinh JWT Token
                    var jwtKey = _configuration["Jwt:Key"] ?? "ThisIsASecretKeyForJwtAuthenticationInRecruitmentSystem2026!@#";
                    var jwtIssuer = _configuration["Jwt:Issuer"] ?? "RecruitmentSystemAPI";
                    var jwtAudience = _configuration["Jwt:Audience"] ?? "RecruitmentSystemClient";

                    var tokenHandler = new JwtSecurityTokenHandler();
                    var key = Encoding.ASCII.GetBytes(jwtKey);

                    int expiresInSeconds = 28800; // 8 hours

                    var tokenDescriptor = new SecurityTokenDescriptor
                    {
                        Subject = new ClaimsIdentity(new[]
                        {
                            new Claim(ClaimTypes.NameIdentifier, maTaiKhoan.ToString()),
                            new Claim(ClaimTypes.Email, email),
                            new Claim(ClaimTypes.Role, vaiTro),
                            new Claim("maTaiKhoan", maTaiKhoan.ToString()),
                            new Claim("vaiTro", vaiTro)
                        }),
                        Expires = DateTime.UtcNow.AddSeconds(expiresInSeconds),
                        Issuer = jwtIssuer,
                        Audience = jwtAudience,
                        SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
                    };

                    var token = tokenHandler.CreateToken(tokenDescriptor);
                    string tokenString = tokenHandler.WriteToken(token);

                    var tokenResponse = new TokenResponseDto
                    {
                        AccessToken = tokenString,
                        TokenType = "Bearer",
                        ExpiresIn = expiresInSeconds,
                        MaTaiKhoan = maTaiKhoan,
                        Email = email,
                        VaiTro = vaiTro
                    };

                    return new ApiResponse<TokenResponseDto>
                    {
                        Success = true,
                        StatusCode = 200,
                        Message = "Đăng nhập thành công.",
                        Data = tokenResponse,
                        Errors = null
                    };
                }
            }
            catch (Exception ex)
            {
                return new ApiResponse<TokenResponseDto>
                {
                    Success = false,
                    StatusCode = 500,
                    Message = "Lỗi hệ thống khi đăng nhập.",
                    Data = null,
                    Errors = new[] { ex.Message }
                };
            }
        }

        public async Task<ApiResponse<object>> ResendOtpAsync(string email)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    string selectQuery = "SELECT MaTaiKhoan, TrangThaiHoatDong FROM TaiKhoan WHERE Email = @Email";
                    int maTaiKhoan = 0;
                    bool trangThaiHoatDong = false;

                    using (var cmd = new SqlCommand(selectQuery, connection))
                    {
                        cmd.Parameters.AddWithValue("@Email", email);
                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            if (!reader.HasRows)
                            {
                                return new ApiResponse<object>
                                {
                                    Success = false,
                                    StatusCode = 404,
                                    Message = "Không tìm thấy tài khoản với email này."
                                };
                            }
                            await reader.ReadAsync();
                            maTaiKhoan = reader.GetInt32(0);
                            trangThaiHoatDong = reader.GetBoolean(1);
                        }
                    }

                    if (trangThaiHoatDong)
                    {
                        return new ApiResponse<object>
                        {
                            Success = false,
                            StatusCode = 400,
                            Message = "Tài khoản này đã được xác thực trước đó."
                        };
                    }

                    string otpCode = new Random().Next(100000, 999999).ToString();
                    DateTime otpExpiry = DateTime.Now.AddMinutes(5);

                    string updateQuery = "UPDATE TaiKhoan SET MaOTP = @MaOTP, ThoiHanOTP = @ThoiHanOTP WHERE MaTaiKhoan = @MaTaiKhoan";
                    using (var cmd = new SqlCommand(updateQuery, connection))
                    {
                        cmd.Parameters.AddWithValue("@MaOTP", otpCode);
                        cmd.Parameters.AddWithValue("@ThoiHanOTP", otpExpiry);
                        cmd.Parameters.AddWithValue("@MaTaiKhoan", maTaiKhoan);
                        await cmd.ExecuteNonQueryAsync();
                    }

                    await _emailService.SendOtpEmailAsync(email, otpCode, "xác thực lại tài khoản");

                    return new ApiResponse<object>
                    {
                        Success = true,
                        StatusCode = 200,
                        Message = $"Mã OTP mới đã được gửi lại về email {email}."
                    };
                }
            }
            catch (Exception ex)
            {
                return new ApiResponse<object>
                {
                    Success = false,
                    StatusCode = 500,
                    Message = "Lỗi hệ thống khi gửi lại mã OTP.",
                    Errors = new[] { ex.Message }
                };
            }
        }

        public async Task<ApiResponse<object>> ForgotPasswordAsync(ForgotPasswordDto dto)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    string selectQuery = "SELECT MaTaiKhoan FROM TaiKhoan WHERE Email = @Email";
                    int maTaiKhoan = 0;

                    using (var cmd = new SqlCommand(selectQuery, connection))
                    {
                        cmd.Parameters.AddWithValue("@Email", dto.Email);
                        var result = await cmd.ExecuteScalarAsync();
                        if (result == null || result == DBNull.Value)
                        {
                            return new ApiResponse<object>
                            {
                                Success = false,
                                StatusCode = 404,
                                Message = "Email không tồn tại."
                            };
                        }
                        maTaiKhoan = Convert.ToInt32(result);
                    }

                    string otpCode = new Random().Next(100000, 999999).ToString();
                    DateTime otpExpiry = DateTime.Now.AddMinutes(5);

                    string updateQuery = "UPDATE TaiKhoan SET MaOTP = @MaOTP, ThoiHanOTP = @ThoiHanOTP WHERE MaTaiKhoan = @MaTaiKhoan";
                    using (var cmd = new SqlCommand(updateQuery, connection))
                    {
                        cmd.Parameters.AddWithValue("@MaOTP", otpCode);
                        cmd.Parameters.AddWithValue("@ThoiHanOTP", otpExpiry);
                        cmd.Parameters.AddWithValue("@MaTaiKhoan", maTaiKhoan);
                        await cmd.ExecuteNonQueryAsync();
                    }

                    await _emailService.SendOtpEmailAsync(dto.Email, otpCode, "quên mật khẩu");

                    return new ApiResponse<object>
                    {
                        Success = true,
                        StatusCode = 200,
                        Message = "Yêu cầu quên mật khẩu thành công.",
                        Data = new { email = dto.Email, otpCode = otpCode }
                    };
                }
            }
            catch (Exception ex)
            {
                return new ApiResponse<object>
                {
                    Success = false,
                    StatusCode = 500,
                    Message = "Lỗi hệ thống.",
                    Errors = new[] { ex.Message }
                };
            }
        }

        public async Task<ApiResponse<object>> ResetPasswordAsync(ResetPasswordDto dto)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    string selectQuery = "SELECT MaTaiKhoan, MaOTP, ThoiHanOTP FROM TaiKhoan WHERE Email = @Email";
                    int maTaiKhoan = 0;
                    string? dbOtp = null;
                    DateTime? thoiHanOTP = null;

                    using (var cmd = new SqlCommand(selectQuery, connection))
                    {
                        cmd.Parameters.AddWithValue("@Email", dto.Email);
                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            if (!reader.HasRows)
                            {
                                return new ApiResponse<object>
                                {
                                    Success = false,
                                    StatusCode = 404,
                                    Message = "Email không tồn tại."
                                };
                            }
                            await reader.ReadAsync();
                            maTaiKhoan = reader.GetInt32(0);
                            dbOtp = reader.IsDBNull(1) ? null : reader.GetString(1);
                            thoiHanOTP = reader.IsDBNull(2) ? null : reader.GetDateTime(2);
                        }
                    }

                    if (dbOtp != dto.MaOTP || thoiHanOTP == null || thoiHanOTP < DateTime.Now)
                    {
                        return new ApiResponse<object>
                        {
                            Success = false,
                            StatusCode = 400,
                            Message = "Mã OTP không chính xác hoặc đã hết hạn."
                        };
                    }

                    string passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.MatKhauMoi);

                    string updateQuery = "UPDATE TaiKhoan SET MatKhau = @MatKhau, MaOTP = NULL, ThoiHanOTP = NULL WHERE MaTaiKhoan = @MaTaiKhoan";
                    using (var cmd = new SqlCommand(updateQuery, connection))
                    {
                        cmd.Parameters.AddWithValue("@MatKhau", passwordHash);
                        cmd.Parameters.AddWithValue("@MaTaiKhoan", maTaiKhoan);
                        await cmd.ExecuteNonQueryAsync();
                    }

                    return new ApiResponse<object>
                    {
                        Success = true,
                        StatusCode = 200,
                        Message = "Đặt lại mật khẩu thành công."
                    };
                }
            }
            catch (Exception ex)
            {
                return new ApiResponse<object>
                {
                    Success = false,
                    StatusCode = 500,
                    Message = "Lỗi hệ thống.",
                    Errors = new[] { ex.Message }
                };
            }
        }
    }
}
