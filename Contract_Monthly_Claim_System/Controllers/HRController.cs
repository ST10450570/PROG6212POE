using Contract_Monthly_Claim_System.Models;
using Contract_Monthly_Claim_System.Services;
using Contract_Monthly_Claim_System.ViewModels;
using Microsoft.AspNetCore.Mvc;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Contract_Monthly_Claim_System.Controllers
{
    public class HRController : Controller
    {
        private readonly IUserService _userService;
        private readonly IClaimService _claimService;
        private readonly IUserSessionService _userSessionService;

        public HRController(IUserService userService, IClaimService claimService, IUserSessionService userSessionService)
        {
            _userService = userService;
            _claimService = claimService;
            _userSessionService = userSessionService;

            // Set QuestPDF license (free for community use)
            QuestPDF.Settings.License = LicenseType.Community;
        }

        // Authorization Helper
        private ApplicationUser? AuthorizeHR()
        {
            var user = _userSessionService.GetCurrentUser();
            if (user == null || user.Role != UserRole.HR)
            {
                return null;
            }
            return user;
        }

        // Dashboard
        public async Task<IActionResult> Dashboard()
        {
            var currentUser = AuthorizeHR();
            if (currentUser == null) return RedirectToAction("AccessDenied", "Account");

            ViewBag.UserName = currentUser.FullName;
            ViewBag.UserRole = "HR Administrator";
            ViewBag.UserInitials = currentUser.Initials;

            var users = await _userService.GetAllUsersAsync();
            var claims = await _claimService.GetAllClaimsAsync();

            ViewBag.TotalUsers = users.Count();
            ViewBag.TotalLecturers = users.Count(u => u.Role == UserRole.Lecturer);
            ViewBag.TotalClaims = claims.Count();
            ViewBag.ApprovedClaims = claims.Count(c => c.Status == ClaimStatus.Approved);

            return View();
        }

        // User Management
        public async Task<IActionResult> Users()
        {
            var currentUser = AuthorizeHR();
            if (currentUser == null) return RedirectToAction("AccessDenied", "Account");

            var users = await _userService.GetAllUsersAsync();
            return View(users);
        }

        [HttpGet]
        public IActionResult CreateUser()
        {
            var currentUser = AuthorizeHR();
            if (currentUser == null) return RedirectToAction("AccessDenied", "Account");

            return View(new UserCreateViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateUser(UserCreateViewModel model)
        {
            var currentUser = AuthorizeHR();
            if (currentUser == null) return RedirectToAction("AccessDenied", "Account");

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Validate hourly rate for lecturers
            if (model.Role == UserRole.Lecturer && (!model.HourlyRate.HasValue || model.HourlyRate.Value <= 0))
            {
                ModelState.AddModelError("HourlyRate", "Hourly rate is required for lecturers.");
                return View(model);
            }

            try
            {
                var user = await _userService.CreateUserAsync(model);
                TempData["Success"] = $"User {user.FullName} created successfully. Login credentials have been set.";
                return RedirectToAction("Users");
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"An error occurred: {ex.Message}");
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> EditUser(int id)
        {
            var currentUser = AuthorizeHR();
            if (currentUser == null) return RedirectToAction("AccessDenied", "Account");

            var user = await _userService.GetUserByIdAsync(id);
            if (user == null) return NotFound();

            var model = new UserEditViewModel
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role,
                Department = user.Department,
                HourlyRate = user.HourlyRate,
                IsActive = user.IsActive
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUser(int id, UserEditViewModel model)
        {
            var currentUser = AuthorizeHR();
            if (currentUser == null) return RedirectToAction("AccessDenied", "Account");

            if (id != model.Id)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Validate hourly rate for lecturers
            if (model.Role == UserRole.Lecturer && (!model.HourlyRate.HasValue || model.HourlyRate.Value <= 0))
            {
                ModelState.AddModelError("HourlyRate", "Hourly rate is required for lecturers.");
                return View(model);
            }

            try
            {
                await _userService.UpdateUserAsync(id, model);
                TempData["Success"] = "User updated successfully.";
                return RedirectToAction("Users");
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"An error occurred: {ex.Message}");
                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeactivateUser(int id)
        {
            var currentUser = AuthorizeHR();
            if (currentUser == null) return RedirectToAction("AccessDenied", "Account");

            // Prevent deactivating self
            if (id == currentUser.Id)
            {
                TempData["Error"] = "You cannot deactivate your own account.";
                return RedirectToAction("Users");
            }

            try
            {
                await _userService.DeleteUserAsync(id);
                TempData["Success"] = "User deactivated successfully.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error deactivating user: {ex.Message}";
            }

            return RedirectToAction("Users");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> HardDeleteUser(int id)
        {
            var currentUser = AuthorizeHR();
            if (currentUser == null) return RedirectToAction("AccessDenied", "Account");

            // Prevent deleting self
            if (id == currentUser.Id)
            {
                TempData["Error"] = "You cannot delete your own account.";
                return RedirectToAction("Users");
            }

            try
            {
                var result = await _userService.HardDeleteUserAsync(id);
                if (result)
                {
                    TempData["Success"] = "User permanently deleted successfully.";
                }
                else
                {
                    TempData["Error"] = "User not found.";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error deleting user: {ex.Message}";
            }

            return RedirectToAction("Users");
        }

        // Reports & Claims Management
        public async Task<IActionResult> Reports()
        {
            var currentUser = AuthorizeHR();
            if (currentUser == null) return RedirectToAction("AccessDenied", "Account");

            var approvedClaims = await _claimService.GetApprovedClaimsAsync();
            return View(approvedClaims);
        }

        public async Task<IActionResult> AllClaims()
        {
            var currentUser = AuthorizeHR();
            if (currentUser == null) return RedirectToAction("AccessDenied", "Account");

            var claims = await _claimService.GetAllClaimsAsync();
            return View(claims);
        }

        // Lecturer Performance Report
        [HttpGet]
        public async Task<IActionResult> LecturerPerformance()
        {
            var currentUser = AuthorizeHR();
            if (currentUser == null) return RedirectToAction("AccessDenied", "Account");

            var performanceData = await GetLecturerPerformanceData();
            return View(performanceData);
        }

        [HttpPost]
        public async Task<IActionResult> GeneratePerformanceReport(string format = "csv")
        {
            var currentUser = AuthorizeHR();
            if (currentUser == null) return RedirectToAction("AccessDenied", "Account");

            try
            {
                var performanceData = await GetLecturerPerformanceData();

                if (format.ToLower() == "pdf")
                {
                    var pdfBytes = GeneratePerformanceReportPDF(performanceData);
                    return File(pdfBytes, "application/pdf", $"LecturerPerformance_{DateTime.Now:yyyyMMdd}.pdf");
                }
                else
                {
                    var csv = GeneratePerformanceReportCSV(performanceData);
                    var bytes = System.Text.Encoding.UTF8.GetBytes(csv);
                    return File(bytes, "text/csv", $"LecturerPerformance_{DateTime.Now:yyyyMMdd}.csv");
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error generating report: {ex.Message}";
                return RedirectToAction("LecturerPerformance");
            }
        }

        [HttpPost]
        public async Task<IActionResult> GenerateIndividualReport(int lecturerId, string type)
        {
            var currentUser = AuthorizeHR();
            if (currentUser == null) return RedirectToAction("AccessDenied", "Account");

            try
            {
                var lecturer = await _userService.GetUserByIdAsync(lecturerId);
                if (lecturer == null)
                {
                    TempData["Error"] = "Lecturer not found.";
                    return RedirectToAction("LecturerPerformance");
                }

                var claims = await _claimService.GetClaimsForUserAsync(lecturerId);
                var approvedClaims = claims.Where(c => c.Status == ClaimStatus.Approved);
                var rejectedClaims = claims.Where(c => c.Status == ClaimStatus.Rejected);
                var totalDecidedClaims = approvedClaims.Count() + rejectedClaims.Count();
                var successRate = totalDecidedClaims > 0 ? (approvedClaims.Count() * 100.0 / totalDecidedClaims) : 100;

                if (type.ToLower() == "csv")
                {
                    var csv = new System.Text.StringBuilder();
                    csv.AppendLine("Lecturer Performance Report - Individual");
                    csv.AppendLine($"Lecturer: {lecturer.FullName}");
                    csv.AppendLine($"Department: {lecturer.Department}");
                    csv.AppendLine($"Hourly Rate: {lecturer.HourlyRate?.ToString("C", new System.Globalization.CultureInfo("en-ZA"))}");
                    csv.AppendLine();
                    csv.AppendLine("Metric,Value");
                    csv.AppendLine($"Total Claims,{claims.Count()}");
                    csv.AppendLine($"Approved Claims,{approvedClaims.Count()}");
                    csv.AppendLine($"Rejected Claims,{rejectedClaims.Count()}");
                    csv.AppendLine($"Total Amount,{approvedClaims.Sum(c => c.TotalAmount)}");
                    csv.AppendLine($"Average Hours,{(approvedClaims.Any() ? approvedClaims.Average(c => c.HoursWorked) : 0):F1}");
                    csv.AppendLine($"Success Rate,{successRate:F1}%");

                    var bytes = System.Text.Encoding.UTF8.GetBytes(csv.ToString());
                    return File(bytes, "text/csv", $"{lecturer.FullName.Replace(" ", "_")}_Performance_{DateTime.Now:yyyyMMdd}.csv");
                }
                else if (type.ToLower() == "pdf")
                {
                    var pdfBytes = GenerateIndividualPerformancePDF(lecturer, claims, approvedClaims, rejectedClaims, successRate);
                    return File(pdfBytes, "application/pdf", $"{lecturer.FullName.Replace(" ", "_")}_Performance_{DateTime.Now:yyyyMMdd}.pdf");
                }
                else
                {
                    TempData["Error"] = "Invalid report type.";
                    return RedirectToAction("LecturerPerformance");
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error generating individual report: {ex.Message}";
                return RedirectToAction("LecturerPerformance");
            }
        }

        // Generate Invoice/Report (Simple CSV export)
        [HttpPost]
        public async Task<IActionResult> GenerateReport(string reportType, string format = "csv")
        {
            var currentUser = AuthorizeHR();
            if (currentUser == null) return RedirectToAction("AccessDenied", "Account");

            try
            {
                if (reportType == "approved")
                {
                    var claims = await _claimService.GetApprovedClaimsAsync();

                    if (format.ToLower() == "pdf")
                    {
                        var pdfBytes = GenerateApprovedClaimsPDF(claims);
                        return File(pdfBytes, "application/pdf", $"ApprovedClaims_{DateTime.Now:yyyyMMdd}.pdf");
                    }
                    else
                    {
                        var csv = GenerateApprovedClaimsCSV(claims);
                        var bytes = System.Text.Encoding.UTF8.GetBytes(csv);
                        return File(bytes, "text/csv", $"ApprovedClaims_{DateTime.Now:yyyyMMdd}.csv");
                    }
                }
                else if (reportType == "all")
                {
                    var claims = await _claimService.GetAllClaimsAsync();

                    if (format.ToLower() == "pdf")
                    {
                        var pdfBytes = GenerateAllClaimsPDF(claims);
                        return File(pdfBytes, "application/pdf", $"AllClaims_{DateTime.Now:yyyyMMdd}.pdf");
                    }
                    else
                    {
                        var csv = GenerateAllClaimsCSV(claims);
                        var bytes = System.Text.Encoding.UTF8.GetBytes(csv);
                        return File(bytes, "text/csv", $"AllClaims_{DateTime.Now:yyyyMMdd}.csv");
                    }
                }

                TempData["Error"] = "Invalid report type.";
                return RedirectToAction("Reports");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error generating report: {ex.Message}";
                return RedirectToAction("Reports");
            }
        }

        private async Task<List<LecturerPerformanceViewModel>> GetLecturerPerformanceData()
        {
            var lecturers = await _userService.GetLecturersAsync();
            var performanceData = new List<LecturerPerformanceViewModel>();

            foreach (var lecturer in lecturers)
            {
                var claims = await _claimService.GetClaimsForUserAsync(lecturer.Id);
                var approvedClaims = claims.Where(c => c.Status == ClaimStatus.Approved);
                var rejectedClaims = claims.Where(c => c.Status == ClaimStatus.Rejected);

                // Improved success rate logic: Approved / (Approved + Rejected)
                var totalDecidedClaims = approvedClaims.Count() + rejectedClaims.Count();
                var successRate = totalDecidedClaims > 0 ? (approvedClaims.Count() * 100.0 / totalDecidedClaims) : 100;

                performanceData.Add(new LecturerPerformanceViewModel
                {
                    LecturerId = lecturer.Id,
                    LecturerName = lecturer.FullName,
                    Department = lecturer.Department,
                    HourlyRate = lecturer.HourlyRate ?? 0,
                    TotalClaims = claims.Count(),
                    ApprovedClaims = approvedClaims.Count(),
                    RejectedClaims = rejectedClaims.Count(),
                    TotalAmount = approvedClaims.Sum(c => c.TotalAmount),
                    AverageHours = approvedClaims.Any() ? approvedClaims.Average(c => c.HoursWorked) : 0,
                    SuccessRate = successRate
                });
            }

            return performanceData;
        }

        private string GeneratePerformanceReportCSV(List<LecturerPerformanceViewModel> performanceData)
        {
            var csv = new System.Text.StringBuilder();
            csv.AppendLine("Lecturer Name,Department,Hourly Rate,Total Claims,Approved Claims,Rejected Claims,Total Amount,Average Hours,Success Rate");

            foreach (var item in performanceData)
            {
                csv.AppendLine($"\"{item.LecturerName}\",\"{item.Department}\",{item.HourlyRate},{item.TotalClaims},{item.ApprovedClaims},{item.RejectedClaims},{item.TotalAmount},{item.AverageHours:F1},{item.SuccessRate:F1}%");
            }

            return csv.ToString();
        }

        private byte[] GeneratePerformanceReportPDF(List<LecturerPerformanceViewModel> performanceData)
        {
            var document = QuestPDF.Fluent.Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape());
                    page.Margin(2, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    page.Header()
                        .AlignCenter()
                        .Text("Lecturer Performance Report - All Lecturers")
                        .SemiBold().FontSize(16).FontColor(Colors.Blue.Medium);

                    page.Content()
                        .PaddingVertical(1, Unit.Centimetre)
                        .Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn();   // Lecturer
                                columns.RelativeColumn();   // Department
                                columns.ConstantColumn(60); // Hourly Rate
                                columns.ConstantColumn(40); // Total Claims
                                columns.ConstantColumn(40); // Approved
                                columns.ConstantColumn(40); // Rejected
                                columns.ConstantColumn(70); // Total Amount
                                columns.ConstantColumn(50); // Avg Hours
                                columns.ConstantColumn(50); // Success Rate
                            });

                            table.Header(header =>
                            {
                                header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Lecturer").SemiBold();
                                header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Department").SemiBold();
                                header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Hourly Rate").SemiBold();
                                header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Total").SemiBold();
                                header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Approved").SemiBold();
                                header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Rejected").SemiBold();
                                header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Total Amount").SemiBold();
                                header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Avg Hours").SemiBold();
                                header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Success %").SemiBold();
                            });

                            foreach (var item in performanceData)
                            {
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(item.LecturerName);
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(item.Department);
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(item.HourlyRate.ToString("C", new System.Globalization.CultureInfo("en-ZA")));
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).AlignCenter().Text($"{item.TotalClaims}");
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).AlignCenter().Text($"{item.ApprovedClaims}");
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).AlignCenter().Text($"{item.RejectedClaims}");
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(item.TotalAmount.ToString("C", new System.Globalization.CultureInfo("en-ZA")));
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).AlignCenter().Text($"{item.AverageHours:F1}");
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).AlignCenter().Text($"{item.SuccessRate:F1}%");
                            }

                            // Footer with totals
                            table.Cell().ColumnSpan(3).AlignRight().Padding(5).Text("Totals:").SemiBold();
                            table.Cell().Padding(5).AlignCenter().Text($"{performanceData.Sum(x => x.TotalClaims)}").SemiBold();
                            table.Cell().Padding(5).AlignCenter().Text($"{performanceData.Sum(x => x.ApprovedClaims)}").SemiBold();
                            table.Cell().Padding(5).AlignCenter().Text($"{performanceData.Sum(x => x.RejectedClaims)}").SemiBold();
                            table.Cell().Padding(5).Text(performanceData.Sum(x => x.TotalAmount).ToString("C", new System.Globalization.CultureInfo("en-ZA"))).SemiBold();
                            table.Cell().ColumnSpan(2).Padding(5).Text("").SemiBold();
                        });

                    page.Footer()
                        .AlignCenter()
                        .Text(x =>
                        {
                            x.Span("Generated on: ").SemiBold();
                            x.Span($"{DateTime.Now:dd MMMM yyyy HH:mm}");
                            x.Span($" | Total Lecturers: {performanceData.Count}");
                        });
                });
            });

            return document.GeneratePdf();
        }

        private string GenerateApprovedClaimsCSV(IEnumerable<Claim> claims)
        {
            var csv = new System.Text.StringBuilder();
            csv.AppendLine("Claim Number,Lecturer Name,Email,Department,Hours Worked,Hourly Rate,Total Amount,Submitted Date,Approved By");

            foreach (var claim in claims)
            {
                csv.AppendLine($"\"{claim.ClaimNumber}\",\"{claim.User?.FullName}\",\"{claim.User?.Email}\",\"{claim.User?.Department}\",{claim.HoursWorked},{claim.HourlyRate},{claim.TotalAmount},\"{claim.SubmittedDate:yyyy-MM-dd}\",\"{claim.Manager?.FullName}\"");
            }

            return csv.ToString();
        }

        private string GenerateAllClaimsCSV(IEnumerable<Claim> claims)
        {
            var csv = new System.Text.StringBuilder();
            csv.AppendLine("Claim Number,Lecturer Name,Email,Status,Hours Worked,Total Amount,Created Date,Submitted Date");

            foreach (var claim in claims)
            {
                csv.AppendLine($"\"{claim.ClaimNumber}\",\"{claim.User?.FullName}\",\"{claim.User?.Email}\",\"{claim.Status}\",{claim.HoursWorked},{claim.TotalAmount},\"{claim.CreatedDate:yyyy-MM-dd}\",\"{claim.SubmittedDate:yyyy-MM-dd}\"");
            }

            return csv.ToString();
        }

        private byte[] GenerateApprovedClaimsPDF(IEnumerable<Claim> claims)
        {
            var document = QuestPDF.Fluent.Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape()); // Use landscape for better fit
                    page.Margin(2, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(9)); // Smaller font to fit all columns

                    page.Header()
                        .AlignCenter()
                        .Text("Approved Claims Report - Ready for Payment")
                        .SemiBold().FontSize(16).FontColor(Colors.Blue.Medium);

                    page.Content()
                        .PaddingVertical(1, Unit.Centimetre)
                        .Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(80);  // Claim #
                                columns.RelativeColumn(1.5f); // Lecturer
                                columns.RelativeColumn(2f);   // Email
                                columns.RelativeColumn(1.2f); // Department
                                columns.ConstantColumn(50);   // Hours
                                columns.ConstantColumn(70);   // Rate
                                columns.ConstantColumn(80);   // Amount
                                columns.ConstantColumn(70);   // Submitted Date
                                columns.RelativeColumn(1.5f); // Approved By
                            });

                            table.Header(header =>
                            {
                                header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Claim #").SemiBold();
                                header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Lecturer").SemiBold();
                                header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Email").SemiBold();
                                header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Department").SemiBold();
                                header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Hours").SemiBold();
                                header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Rate").SemiBold();
                                header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Amount").SemiBold();
                                header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Submitted").SemiBold();
                                header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Approved By").SemiBold();
                            });

                            foreach (var claim in claims)
                            {
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(claim.ClaimNumber);
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(claim.User?.FullName ?? "N/A");
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(claim.User?.Email ?? "N/A");
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(claim.User?.Department ?? "N/A");
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).AlignCenter().Text($"{claim.HoursWorked}");
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(claim.HourlyRate.ToString("C", new System.Globalization.CultureInfo("en-ZA")));
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(claim.TotalAmount.ToString("C", new System.Globalization.CultureInfo("en-ZA")));
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(claim.SubmittedDate?.ToString("dd/MM/yyyy") ?? "N/A");
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(claim.Manager?.FullName ?? "N/A");
                            }

                            // Footer with total
                            table.Cell().ColumnSpan(8).AlignRight().Padding(5).Text("Total Amount:").SemiBold();
                            table.Cell().Padding(5).Text(claims.Sum(c => c.TotalAmount).ToString("C", new System.Globalization.CultureInfo("en-ZA"))).SemiBold();
                        });

                    page.Footer()
                        .AlignCenter()
                        .Text(x =>
                        {
                            x.Span("Generated on: ").SemiBold();
                            x.Span($"{DateTime.Now:dd MMMM yyyy HH:mm}");
                            x.Span($" | Total Approved Claims: {claims.Count()}");
                            x.Span($" | Total Amount: {claims.Sum(c => c.TotalAmount).ToString("C", new System.Globalization.CultureInfo("en-ZA"))}");
                        });
                });
            });

            return document.GeneratePdf();
        }

        private byte[] GenerateIndividualPerformancePDF(ApplicationUser lecturer, IEnumerable<Claim> claims,
            IEnumerable<Claim> approvedClaims, IEnumerable<Claim> rejectedClaims, double successRate)
        {
            var document = QuestPDF.Fluent.Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(11));

                    page.Header()
                        .AlignCenter()
                        .Text("Lecturer Performance Report")
                        .SemiBold().FontSize(16).FontColor(Colors.Blue.Medium);

                    page.Content()
                        .PaddingVertical(1, Unit.Centimetre)
                        .Column(column =>
                        {
                            column.Spacing(15);

                            // Lecturer Information
                            column.Item().Background(Colors.Grey.Lighten3).Padding(10).Column(infoColumn =>
                            {
                                infoColumn.Item().Text($"Lecturer: {lecturer.FullName}").SemiBold();
                                infoColumn.Item().Text($"Department: {lecturer.Department}");
                                infoColumn.Item().Text($"Hourly Rate: {lecturer.HourlyRate?.ToString("C", new System.Globalization.CultureInfo("en-ZA"))}");
                                infoColumn.Item().Text($"Report Date: {DateTime.Now:dd MMMM yyyy}");
                            });

                            // Performance Summary
                            column.Item().Text("Performance Summary").SemiBold().FontSize(14);
                            column.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                });

                                table.Cell().Background(Colors.Blue.Lighten5).Padding(8).AlignCenter().Text("Total Claims").SemiBold();
                                table.Cell().Background(Colors.Green.Lighten5).Padding(8).AlignCenter().Text("Approved").SemiBold();
                                table.Cell().Background(Colors.Red.Lighten5).Padding(8).AlignCenter().Text("Rejected").SemiBold();
                                table.Cell().Background(Colors.Orange.Lighten5).Padding(8).AlignCenter().Text("Success Rate").SemiBold();

                                table.Cell().Padding(8).AlignCenter().Text($"{claims.Count()}").FontSize(12);
                                table.Cell().Padding(8).AlignCenter().Text($"{approvedClaims.Count()}").FontSize(12);
                                table.Cell().Padding(8).AlignCenter().Text($"{rejectedClaims.Count()}").FontSize(12);
                                table.Cell().Padding(8).AlignCenter().Text($"{successRate:F1}%").FontSize(12);
                            });

                            // Financial Summary
                            column.Item().Text("Financial Summary").SemiBold().FontSize(14);
                            column.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                });

                                table.Cell().Background(Colors.Grey.Lighten4).Padding(8).Text("Total Amount Approved").SemiBold();
                                table.Cell().Background(Colors.Grey.Lighten4).Padding(8).Text("Average Hours per Claim").SemiBold();

                                table.Cell().Padding(8).Text(approvedClaims.Sum(c => c.TotalAmount).ToString("C", new System.Globalization.CultureInfo("en-ZA")));
                                table.Cell().Padding(8).Text(approvedClaims.Any() ? approvedClaims.Average(c => c.HoursWorked).ToString("F1") + " hrs" : "0 hrs");
                            });
                        });

                    page.Footer()
                        .AlignCenter()
                        .Text(x =>
                        {
                            x.Span("Confidential - For HR Use Only | Generated on: ");
                            x.Span($"{DateTime.Now:dd MMMM yyyy HH:mm}");
                        });
                });
            });

            return document.GeneratePdf();
        }

        [HttpPost]
        public async Task<IActionResult> GenerateAllClaimsReport(string format = "pdf")
        {
            var currentUser = AuthorizeHR();
            if (currentUser == null) return RedirectToAction("AccessDenied", "Account");

            try
            {
                var claims = await _claimService.GetAllClaimsAsync();

                if (format.ToLower() == "pdf")
                {
                    var pdfBytes = GenerateAllClaimsPDF(claims);
                    return File(pdfBytes, "application/pdf", $"AllClaims_{DateTime.Now:yyyyMMdd}.pdf");
                }
                else
                {
                    // Fallback to CSV if needed
                    var csv = GenerateAllClaimsCSV(claims);
                    var bytes = System.Text.Encoding.UTF8.GetBytes(csv);
                    return File(bytes, "text/csv", $"AllClaims_{DateTime.Now:yyyyMMdd}.csv");
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error generating report: {ex.Message}";
                return RedirectToAction("AllClaims");
            }
        }

        private byte[] GenerateAllClaimsPDF(IEnumerable<Claim> claims)
        {
            var document = QuestPDF.Fluent.Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape());
                    page.Margin(2, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    page.Header()
                        .AlignCenter()
                        .Text("All Claims Report - Complete Overview")
                        .SemiBold().FontSize(16).FontColor(Colors.Blue.Medium);

                    page.Content()
                        .PaddingVertical(1, Unit.Centimetre)
                        .Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(80);  // Claim #
                                columns.RelativeColumn();    // Lecturer
                                columns.ConstantColumn(80);  // Status
                                columns.ConstantColumn(60);  // Hours
                                columns.ConstantColumn(80);  // Amount
                                columns.ConstantColumn(80);  // Hourly Rate
                                columns.ConstantColumn(80);  // Created
                                columns.ConstantColumn(80);  // Submitted
                            });

                            table.Header(header =>
                            {
                                header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Claim #").SemiBold();
                                header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Lecturer").SemiBold();
                                header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Status").SemiBold();
                                header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Hours").SemiBold();
                                header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Amount").SemiBold();
                                header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Hourly Rate").SemiBold();
                                header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Created").SemiBold();
                                header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Submitted").SemiBold();
                            });

                            foreach (var claim in claims)
                            {
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(claim.ClaimNumber);
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(claim.User?.FullName ?? "N/A");
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(claim.Status.ToString());
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).AlignCenter().Text($"{claim.HoursWorked}");
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(claim.TotalAmount.ToString("C", new System.Globalization.CultureInfo("en-ZA")));
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(claim.HourlyRate.ToString("C", new System.Globalization.CultureInfo("en-ZA")));
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(claim.CreatedDate.ToString("dd/MM/yyyy"));
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(claim.SubmittedDate?.ToString("dd/MM/yyyy") ?? "N/A");
                            }

                            // Footer with summary
                            var approvedClaims = claims.Where(c => c.Status == ClaimStatus.Approved);
                            table.Cell().ColumnSpan(3).AlignRight().Padding(5).Text("Summary:").SemiBold();
                            table.Cell().Padding(5).AlignCenter().Text($"Total: {claims.Count()}").SemiBold();
                            table.Cell().Padding(5).Text(approvedClaims.Sum(c => c.TotalAmount).ToString("C", new System.Globalization.CultureInfo("en-ZA"))).SemiBold();
                            table.Cell().Padding(5).Text("");
                            table.Cell().Padding(5).Text($"Approved: {approvedClaims.Count()}").SemiBold();
                            table.Cell().Padding(5).Text("");
                        });

                    page.Footer()
                        .AlignCenter()
                        .Text(x =>
                        {
                            x.Span("Generated on: ").SemiBold();
                            x.Span($"{DateTime.Now:dd MMMM yyyy HH:mm}");
                            x.Span($" | Total Claims: {claims.Count()}");
                        });
                });
            });

            return document.GeneratePdf();
        }
    }
}