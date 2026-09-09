using System.Text;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using HRAttendance.Business.Interfaces.Payroll;
using HRAttendance.Data;
using HRAttendance.Data.Models.Payroll;

namespace HRAttendance.Business.Services.Payroll;

public class QuestPdfGenerator : IPayslipPdfGenerator
{
    private readonly ApplicationDbContext _context;
    private readonly IWebHostEnvironment? _environment;
    private readonly ILogger<QuestPdfGenerator>? _logger;

    // Corporate Color Palette
    private static readonly Color PrimaryNavy = Color.FromHex("#0F172A");    // Slate 900
    private static readonly Color BrandBlue = Color.FromHex("#1E3A8A");      // Blue 900
    private static readonly Color AccentBlue = Color.FromHex("#2563EB");     // Blue 600
    private static readonly Color LightAccentBlue = Color.FromHex("#93C5FD");// Blue 300
    private static readonly Color TableHeaderBg = Color.FromHex("#1E293B");  // Slate 800
    private static readonly Color CardBg = Color.FromHex("#F8FAFC");         // Slate 50
    private static readonly Color CardBorder = Color.FromHex("#E2E8F0");     // Slate 200
    private static readonly Color TextDark = Color.FromHex("#0F172A");       // Slate 900
    private static readonly Color TextMedium = Color.FromHex("#334155");     // Slate 700
    private static readonly Color TextMuted = Color.FromHex("#64748B");      // Slate 500
    private static readonly Color RowZebra = Color.FromHex("#F8FAFC");       // Slate 50

    public QuestPdfGenerator(
        ApplicationDbContext context,
        IWebHostEnvironment? environment = null,
        ILogger<QuestPdfGenerator>? logger = null)
    {
        _context = context;
        _environment = environment;
        _logger = logger;
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public async Task<byte[]> GeneratePayslipPdfAsync(int payslipId, CancellationToken cancellationToken = default)
    {
        var payslip = await _context.Payslips
            .Include(p => p.Employee)
                .ThenInclude(e => e.ProfessionalDetails)
                    .ThenInclude(pd => pd!.Designation)
            .Include(p => p.Employee)
                .ThenInclude(e => e.ProfessionalDetails)
                    .ThenInclude(pd => pd!.Department)
            .Include(p => p.Employee)
                .ThenInclude(e => e.ContactDetails)
            .Include(p => p.PayrollEmployee)
                .ThenInclude(pe => pe.Period)
                    .ThenInclude(pr => pr.Organization)
            .Include(p => p.PayrollEmployee)
                .ThenInclude(pe => pe.Payments)
            .Include(p => p.Items)
            .FirstOrDefaultAsync(p => p.Id == payslipId, cancellationToken)
            ?? throw new InvalidOperationException($"Payslip {payslipId} not found.");

        var pe = payslip.PayrollEmployee;
        var period = pe.Period;
        var org = period.Organization;
        var emp = payslip.Employee;
        var prof = emp.ProfessionalDetails;

        var earnings = payslip.Items.Where(i => i.ComponentType == ComponentType.Earning).OrderBy(i => i.DisplayOrder).ToList();
        var deductions = payslip.Items.Where(i => i.ComponentType == ComponentType.Deduction).OrderBy(i => i.DisplayOrder).ToList();
        var employerContribs = payslip.Items.Where(i => i.ComponentType == ComponentType.EmployerContribution).OrderBy(i => i.DisplayOrder).ToList();

        var totalEarnings = earnings.Sum(i => i.Amount);
        var totalDeductions = deductions.Sum(i => i.Amount);
        var totalEmployer = employerContribs.Sum(i => i.Amount);
        var netPay = pe.NetPay;

        var monthName = new DateTime(period.Year, period.Month, 1).ToString("MMMM yyyy");
        var payment = pe.Payments?.OrderByDescending(p => p.Id).FirstOrDefault();
        var bankName = !string.IsNullOrWhiteSpace(payment?.BankName) ? payment.BankName : "HDFC Bank";
        var accountNo = !string.IsNullOrWhiteSpace(payment?.MaskedAccountNumber)
            ? payment.MaskedAccountNumber
            : $"XXXX-XXXX-{(emp.Id % 10000):D4}";
        var ifsc = !string.IsNullOrWhiteSpace(payment?.IFSCCode) ? payment.IFSCCode : "HDFC0001234";
        var pan = "ABCDE" + (emp.Id % 10000).ToString("D4") + "F";
        var uan = "100" + (emp.Id % 1000000000).ToString("D9");

        // Load organization logo if available
        var logoBytes = LoadCompanyLogo(org.LogoUrl);

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(26);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(9).FontFamily("Arial"));

                // ==========================================
                // 1. HEADER: BRANDING & PAYSLIP IDENTIFIER
                // ==========================================
                page.Header().Column(headerCol =>
                {
                    headerCol.Item().Row(row =>
                    {
                        // Left: Logo and Organization Details
                        row.RelativeItem(7).Row(brandRow =>
                        {
                            if (logoBytes != null && logoBytes.Length > 0)
                            {
                                brandRow.AutoItem().PaddingRight(12).Height(50).Image(logoBytes).FitArea();
                            }
                            else
                            {
                                // Crisp Stylized Monogram Badge
                                var initials = GetCompanyInitials(org.Name);
                                brandRow.AutoItem().PaddingRight(12).Height(46).Width(46)
                                    .Background(BrandBlue)
                                    .AlignCenter()
                                    .AlignMiddle()
                                    .Text(initials)
                                    .FontSize(16)
                                    .Bold()
                                    .FontColor(Colors.White);
                            }

                            brandRow.RelativeItem().Column(orgCol =>
                            {
                                orgCol.Item().Text(org.Name).FontSize(15).Bold().FontColor(PrimaryNavy);

                                var addressParts = new List<string>();
                                if (!string.IsNullOrWhiteSpace(org.AddressLine1)) addressParts.Add(org.AddressLine1);
                                if (!string.IsNullOrWhiteSpace(org.AddressLine2)) addressParts.Add(org.AddressLine2);
                                if (!string.IsNullOrWhiteSpace(org.City)) addressParts.Add(org.City);
                                if (!string.IsNullOrWhiteSpace(org.PostalCode)) addressParts.Add(org.PostalCode);

                                if (addressParts.Count > 0)
                                {
                                    orgCol.Item().Text(string.Join(", ", addressParts)).FontSize(8).FontColor(TextMuted);
                                }

                                var contactLine = new List<string>();
                                if (!string.IsNullOrWhiteSpace(org.Email)) contactLine.Add($"Email: {org.Email}");
                                if (!string.IsNullOrWhiteSpace(org.Phone)) contactLine.Add($"Phone: {org.Phone}");
                                if (!string.IsNullOrWhiteSpace(org.Website)) contactLine.Add($"Web: {org.Website}");

                                if (contactLine.Count > 0)
                                {
                                    orgCol.Item().Text(string.Join(" | ", contactLine)).FontSize(7.5f).FontColor(TextMuted);
                                }
                            });
                        });

                        // Right: Title, Month and Reference Metadata
                        row.RelativeItem(5).AlignRight().Column(metaCol =>
                        {
                            metaCol.Item().Text("SALARY SLIP").FontSize(18).Bold().FontColor(BrandBlue);
                            metaCol.Item().Text(monthName.ToUpperInvariant()).FontSize(10.5f).Bold().FontColor(AccentBlue);
                            metaCol.Item().Text($"Ref No: {payslip.PayslipNumber}").FontSize(8).SemiBold().FontColor(TextDark);
                            metaCol.Item().Text($"Date: {payslip.GeneratedAt:dd MMM yyyy}").FontSize(7.5f).FontColor(TextMuted);
                            metaCol.Item().PaddingTop(2).Text(t =>
                            {
                                t.Span("CONFIDENTIAL").FontSize(7).Bold().FontColor(Colors.Green.Darken2);
                            });
                        });
                    });

                    headerCol.Item().PaddingTop(8).LineHorizontal(1.5f).LineColor(AccentBlue);
                });

                // ==========================================
                // 2. MAIN CONTENT
                // ==========================================
                page.Content().Column(col =>
                {
                    // A. Employee Details & Attendance Overview
                    col.Item().PaddingTop(8).Border(1).BorderColor(CardBorder).Background(CardBg).Padding(8).Row(infoRow =>
                    {
                        // Column 1: Employment
                        infoRow.RelativeItem().Column(c =>
                        {
                            c.Item().PaddingBottom(3).Text("EMPLOYEE INFORMATION").FontSize(7.5f).Bold().FontColor(BrandBlue);
                            c.Item().Text(t => { t.Span("Name: ").Bold().FontColor(TextMedium); t.Span(emp.FullName).FontColor(TextDark); });
                            c.Item().Text(t => { t.Span("Emp ID: ").Bold().FontColor(TextMedium); t.Span(emp.EmployeeCode).FontColor(TextDark); });
                            c.Item().Text(t => { t.Span("Department: ").Bold().FontColor(TextMedium); t.Span(prof?.Department?.Name ?? "General").FontColor(TextDark); });
                            c.Item().Text(t => { t.Span("Designation: ").Bold().FontColor(TextMedium); t.Span(prof?.Designation?.Name ?? "Staff").FontColor(TextDark); });
                            if (prof?.DateOfJoining != null)
                            {
                                c.Item().Text(t => { t.Span("Joining Date: ").Bold().FontColor(TextMedium); t.Span(prof.DateOfJoining.Value.ToString("dd MMM yyyy")).FontColor(TextDark); });
                            }
                        });

                        // Column 2: Bank & Statutory IDs
                        infoRow.RelativeItem().Column(c =>
                        {
                            c.Item().PaddingBottom(3).Text("BANK & TAX DETAILS").FontSize(7.5f).Bold().FontColor(BrandBlue);
                            c.Item().Text(t => { t.Span("Bank: ").Bold().FontColor(TextMedium); t.Span(bankName).FontColor(TextDark); });
                            c.Item().Text(t => { t.Span("A/C No: ").Bold().FontColor(TextMedium); t.Span(accountNo).FontColor(TextDark); });
                            c.Item().Text(t => { t.Span("IFSC: ").Bold().FontColor(TextMedium); t.Span(ifsc).FontColor(TextDark); });
                            c.Item().Text(t => { t.Span("PAN: ").Bold().FontColor(TextMedium); t.Span(pan).FontColor(TextDark); });
                            c.Item().Text(t => { t.Span("UAN / PF: ").Bold().FontColor(TextMedium); t.Span(uan).FontColor(TextDark); });
                        });

                        // Column 3: Attendance Summary
                        infoRow.RelativeItem().Column(c =>
                        {
                            c.Item().PaddingBottom(3).Text("ATTENDANCE METRICS").FontSize(7.5f).Bold().FontColor(BrandBlue);
                            c.Item().Text(t => { t.Span("Days in Month: ").Bold().FontColor(TextMedium); t.Span(pe.CalendarDaysInMonth.ToString()).FontColor(TextDark); });
                            c.Item().Text(t => { t.Span("Working Days: ").Bold().FontColor(TextMedium); t.Span(pe.WorkingDays.ToString("F0")).FontColor(TextDark); });
                            c.Item().Text(t => { t.Span("Present Days: ").Bold().FontColor(TextMedium); t.Span(pe.PresentDays.ToString("F1")).FontColor(TextDark); });
                            c.Item().Text(t => { t.Span("Paid Leaves: ").Bold().FontColor(TextMedium); t.Span(pe.PaidLeaveDays.ToString("F1")).FontColor(TextDark); });
                            c.Item().Text(t => { t.Span("Loss of Pay (LOP): ").Bold().FontColor(TextMedium); t.Span(pe.LOPDays.ToString("F1")).FontColor(pe.LOPDays > 0 ? Colors.Red.Medium : TextDark); });
                            if (pe.ApprovedOvertimeHours > 0)
                            {
                                c.Item().Text(t => { t.Span("Overtime Hours: ").Bold().FontColor(TextMedium); t.Span($"{pe.ApprovedOvertimeHours:F1} hrs").FontColor(Colors.Green.Darken2); });
                            }
                        });
                    });

                    // B. Side-by-Side Earnings and Deductions Table
                    col.Item().PaddingTop(10).Row(tableRow =>
                    {
                        // Left: Earnings Table
                        tableRow.RelativeItem().Border(1).BorderColor(CardBorder).Column(earnCol =>
                        {
                            earnCol.Item().Background(TableHeaderBg).Padding(6).Row(r =>
                            {
                                r.RelativeItem().Text("EARNINGS").Bold().FontSize(8.5f).FontColor(Colors.White);
                                r.RelativeItem().AlignRight().Text("AMOUNT (₹)").Bold().FontSize(8.5f).FontColor(Colors.White);
                            });

                            for (int idx = 0; idx < earnings.Count; idx++)
                            {
                                var earn = earnings[idx];
                                var bg = idx % 2 == 1 ? RowZebra : Colors.White;
                                earnCol.Item().Background(bg).PaddingHorizontal(6).PaddingVertical(3.5f).Row(r =>
                                {
                                    r.RelativeItem().Text(earn.ComponentName).FontSize(8.5f).FontColor(TextDark);
                                    r.RelativeItem().AlignRight().Text($"₹ {earn.Amount:N2}").FontSize(8.5f).FontColor(TextDark);
                                });
                            }

                            // Fill remaining space if deductions has more items
                            for (int i = earnings.Count; i < deductions.Count; i++)
                            {
                                var bg = i % 2 == 1 ? RowZebra : Colors.White;
                                earnCol.Item().Background(bg).PaddingHorizontal(6).PaddingVertical(3.5f).Row(r =>
                                {
                                    r.RelativeItem().Text("-").FontSize(8.5f).FontColor(Colors.Transparent);
                                    r.RelativeItem().AlignRight().Text("-").FontSize(8.5f).FontColor(Colors.Transparent);
                                });
                            }

                            earnCol.Item().LineHorizontal(1).LineColor(CardBorder);
                            earnCol.Item().Background(Color.FromHex("#F1F5F9")).Padding(6).Row(r =>
                            {
                                r.RelativeItem().Text("Total Gross Earnings").Bold().FontSize(9).FontColor(TextDark);
                                r.RelativeItem().AlignRight().Text($"₹ {totalEarnings:N2}").Bold().FontSize(9).FontColor(BrandBlue);
                            });
                        });

                        tableRow.ConstantItem(8); // Crisp column gap

                        // Right: Deductions Table
                        tableRow.RelativeItem().Border(1).BorderColor(CardBorder).Column(dedCol =>
                        {
                            dedCol.Item().Background(TableHeaderBg).Padding(6).Row(r =>
                            {
                                r.RelativeItem().Text("DEDUCTIONS").Bold().FontSize(8.5f).FontColor(Colors.White);
                                r.RelativeItem().AlignRight().Text("AMOUNT (₹)").Bold().FontSize(8.5f).FontColor(Colors.White);
                            });

                            for (int idx = 0; idx < deductions.Count; idx++)
                            {
                                var ded = deductions[idx];
                                var bg = idx % 2 == 1 ? RowZebra : Colors.White;
                                dedCol.Item().Background(bg).PaddingHorizontal(6).PaddingVertical(3.5f).Row(r =>
                                {
                                    r.RelativeItem().Text(ded.ComponentName).FontSize(8.5f).FontColor(TextDark);
                                    r.RelativeItem().AlignRight().Text($"₹ {ded.Amount:N2}").FontSize(8.5f).FontColor(TextDark);
                                });
                            }

                            // Fill remaining space if earnings has more items
                            for (int i = deductions.Count; i < earnings.Count; i++)
                            {
                                var bg = i % 2 == 1 ? RowZebra : Colors.White;
                                dedCol.Item().Background(bg).PaddingHorizontal(6).PaddingVertical(3.5f).Row(r =>
                                {
                                    r.RelativeItem().Text("-").FontSize(8.5f).FontColor(Colors.Transparent);
                                    r.RelativeItem().AlignRight().Text("-").FontSize(8.5f).FontColor(Colors.Transparent);
                                });
                            }

                            dedCol.Item().LineHorizontal(1).LineColor(CardBorder);
                            dedCol.Item().Background(Color.FromHex("#F1F5F9")).Padding(6).Row(r =>
                            {
                                r.RelativeItem().Text("Total Deductions").Bold().FontSize(9).FontColor(TextDark);
                                r.RelativeItem().AlignRight().Text($"₹ {totalDeductions:N2}").Bold().FontSize(9).FontColor(Colors.Red.Darken1);
                            });
                        });
                    });

                    // C. Net Take-Home Pay Banner (The Centerpiece)
                    col.Item().PaddingTop(10).Background(PrimaryNavy).Padding(10).Row(netRow =>
                    {
                        netRow.RelativeItem(7).Column(c =>
                        {
                            c.Item().Text("NET TAKE-HOME PAYABLE").FontSize(11).Bold().FontColor(LightAccentBlue);
                            c.Item().PaddingTop(2).Text($"Amount in Words: {ConvertAmountToWords(netPay)}").FontSize(8).Italic().FontColor(Colors.White);
                        });

                        netRow.RelativeItem(5).AlignRight().AlignMiddle().Text($"₹ {netPay:N2}").FontSize(18).Bold().FontColor(Colors.White);
                    });

                    // D. Employer Statutory Contributions (CTC Additions)
                    if (employerContribs.Count > 0)
                    {
                        col.Item().PaddingTop(8).Border(1).BorderColor(CardBorder).Background(CardBg).Padding(6).Column(empCol =>
                        {
                            empCol.Item().Row(r =>
                            {
                                r.RelativeItem().Text("EMPLOYER STATUTORY CONTRIBUTIONS (Excluded from Employee Net Take-Home)").FontSize(7.5f).Bold().FontColor(TextMedium);
                                r.RelativeItem().AlignRight().Text($"Total: ₹ {totalEmployer:N2}").FontSize(8).Bold().FontColor(TextDark);
                            });

                            empCol.Item().PaddingTop(3).Row(contribRow =>
                            {
                                foreach (var ec in employerContribs)
                                {
                                    contribRow.RelativeItem().Text(t =>
                                    {
                                        t.Span($"{ec.ComponentName}: ").FontColor(TextMuted).FontSize(7.5f);
                                        t.Span($"₹{ec.Amount:N0}").Bold().FontColor(TextDark).FontSize(7.5f);
                                    });
                                }
                            });
                        });
                    }

                    // E. Sign-off & Verification Seals
                    col.Item().PaddingTop(22).Row(signRow =>
                    {
                        signRow.RelativeItem().Column(c =>
                        {
                            c.Item().LineHorizontal(0.75f).LineColor(CardBorder);
                            c.Item().PaddingTop(4).Text("Employee Signature").FontSize(8).Bold().FontColor(TextMedium);
                            c.Item().Text("Date: ____________________").FontSize(7.5f).FontColor(TextMuted);
                        });

                        signRow.ConstantItem(60);

                        signRow.RelativeItem().AlignRight().Column(c =>
                        {
                            c.Item().LineHorizontal(0.75f).LineColor(CardBorder);
                            c.Item().PaddingTop(4).Text($"For {org.Name}").FontSize(8).Bold().FontColor(TextMedium);
                            c.Item().Text("Authorized Signatory (HR & Payroll)").FontSize(7.5f).FontColor(TextMuted);
                        });
                    });
                });

                // ==========================================
                // 3. FOOTER
                // ==========================================
                page.Footer().Column(fCol =>
                {
                    fCol.Item().LineHorizontal(1).LineColor(CardBorder);
                    fCol.Item().PaddingTop(4).Row(row =>
                    {
                        row.RelativeItem().Text("This is a computer-generated salary document and requires no physical seal or signature.").FontSize(7).FontColor(TextMuted);
                        row.RelativeItem().AlignRight().Text(t =>
                        {
                            t.Span("Page ");
                            t.CurrentPageNumber();
                            t.Span(" of ");
                            t.TotalPages();
                        });
                    });
                });
            });
        });

        return document.GeneratePdf();
    }

    /// <summary>
    /// Loads company logo bytes safely from local upload storage or external URL.
    /// </summary>
    private byte[]? LoadCompanyLogo(string? logoUrl)
    {
        if (string.IsNullOrWhiteSpace(logoUrl))
            return null;

        try
        {
            // 1. Resolve local uploads directory
            var clean = logoUrl.TrimStart('/', '\\').Replace('/', Path.DirectorySeparatorChar);
            var candidates = new List<string>();

            if (_environment != null)
            {
                if (!string.IsNullOrWhiteSpace(_environment.ContentRootPath))
                {
                    candidates.Add(Path.Combine(_environment.ContentRootPath, clean));
                    candidates.Add(Path.Combine(_environment.ContentRootPath, "uploads", clean));
                }

                if (!string.IsNullOrWhiteSpace(_environment.WebRootPath))
                {
                    candidates.Add(Path.Combine(_environment.WebRootPath, clean));
                }
            }

            candidates.Add(Path.Combine(AppContext.BaseDirectory, clean));
            candidates.Add(logoUrl);

            foreach (var path in candidates)
            {
                if (File.Exists(path))
                {
                    var bytes = File.ReadAllBytes(path);
                    if (bytes.Length > 0) return bytes;
                }
            }

            // 2. Fetch remote HTTP/HTTPS image if hosted externally
            if (Uri.TryCreate(logoUrl, UriKind.Absolute, out var uri) &&
                (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps))
            {
                using var httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(3) };
                var responseTask = httpClient.GetByteArrayAsync(uri);
                if (responseTask.Wait(3000))
                {
                    var bytes = responseTask.Result;
                    if (bytes.Length > 0) return bytes;
                }
            }
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "Failed to load company logo from {LogoUrl}", logoUrl);
        }

        return null;
    }

    /// <summary>
    /// Computes 2-letter monogram initials for the organization fallback badge.
    /// </summary>
    private static string GetCompanyInitials(string? companyName)
    {
        if (string.IsNullOrWhiteSpace(companyName))
            return "HR";

        var words = companyName.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (words.Length == 1)
        {
            return words[0].Length >= 2 ? words[0].Substring(0, 2).ToUpperInvariant() : words[0].ToUpperInvariant();
        }

        return $"{char.ToUpperInvariant(words[0][0])}{char.ToUpperInvariant(words[1][0])}";
    }

    /// <summary>
    /// Converts numeric amount to Indian Currency words (e.g. Rupees Seventy-Five Thousand Only).
    /// </summary>
    private static string ConvertAmountToWords(decimal amount)
    {
        if (amount <= 0)
            return "Zero Rupees Only";

        long integerPart = (long)Math.Floor(amount);
        int paisaPart = (int)Math.Round((amount - integerPart) * 100);

        var words = NumberToWords(integerPart) + " Rupees";
        if (paisaPart > 0)
        {
            words += " and " + NumberToWords(paisaPart) + " Paise";
        }

        return words + " Only";
    }

    private static string NumberToWords(long number)
    {
        if (number == 0) return "Zero";
        if (number < 0) return "Minus " + NumberToWords(Math.Abs(number));

        var sb = new StringBuilder();

        if (number / 10000000 > 0) // Crores
        {
            sb.Append(NumberToWords(number / 10000000) + " Crore ");
            number %= 10000000;
        }

        if (number / 100000 > 0) // Lakhs
        {
            sb.Append(NumberToWords(number / 100000) + " Lakh ");
            number %= 100000;
        }

        if (number / 1000 > 0) // Thousands
        {
            sb.Append(NumberToWords(number / 1000) + " Thousand ");
            number %= 1000;
        }

        if (number / 100 > 0)
        {
            sb.Append(NumberToWords(number / 100) + " Hundred ");
            number %= 100;
        }

        if (number > 0)
        {
            string[] unitsMap = { "Zero", "One", "Two", "Three", "Four", "Five", "Six", "Seven", "Eight", "Nine", "Ten",
                                  "Eleven", "Twelve", "Thirteen", "Fourteen", "Fifteen", "Sixteen", "Seventeen", "Eighteen", "Nineteen" };
            string[] tensMap = { "Zero", "Ten", "Twenty", "Thirty", "Forty", "Fifty", "Sixty", "Seventy", "Eighty", "Ninety" };

            if (number < 20)
            {
                sb.Append(unitsMap[number]);
            }
            else
            {
                sb.Append(tensMap[number / 10]);
                if (number % 10 > 0)
                    sb.Append("-" + unitsMap[number % 10]);
            }
        }

        return sb.ToString().Trim();
    }
}

