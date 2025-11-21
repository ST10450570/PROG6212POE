USE [master]
GO
/****** Object:  Database [ContractClaimSystemDB]    Script Date: 11/21/2025 4:18:06 PM ******/
CREATE DATABASE [ContractClaimSystemDB]
 CONTAINMENT = NONE
 ON  PRIMARY 
( NAME = N'ContractClaimSystemDB', FILENAME = N'C:\Program Files\Microsoft SQL Server\MSSQL16.SQLEXPRESS\MSSQL\DATA\ContractClaimSystemDB.mdf' , SIZE = 8192KB , MAXSIZE = UNLIMITED, FILEGROWTH = 65536KB )
 LOG ON 
( NAME = N'ContractClaimSystemDB_log', FILENAME = N'C:\Program Files\Microsoft SQL Server\MSSQL16.SQLEXPRESS\MSSQL\DATA\ContractClaimSystemDB_log.ldf' , SIZE = 8192KB , MAXSIZE = 2048GB , FILEGROWTH = 65536KB )
 WITH CATALOG_COLLATION = DATABASE_DEFAULT, LEDGER = OFF
GO
ALTER DATABASE [ContractClaimSystemDB] SET COMPATIBILITY_LEVEL = 160
GO
IF (1 = FULLTEXTSERVICEPROPERTY('IsFullTextInstalled'))
begin
EXEC [ContractClaimSystemDB].[dbo].[sp_fulltext_database] @action = 'enable'
end
GO
ALTER DATABASE [ContractClaimSystemDB] SET ANSI_NULL_DEFAULT OFF 
GO
ALTER DATABASE [ContractClaimSystemDB] SET ANSI_NULLS OFF 
GO
ALTER DATABASE [ContractClaimSystemDB] SET ANSI_PADDING OFF 
GO
ALTER DATABASE [ContractClaimSystemDB] SET ANSI_WARNINGS OFF 
GO
ALTER DATABASE [ContractClaimSystemDB] SET ARITHABORT OFF 
GO
ALTER DATABASE [ContractClaimSystemDB] SET AUTO_CLOSE ON 
GO
ALTER DATABASE [ContractClaimSystemDB] SET AUTO_SHRINK OFF 
GO
ALTER DATABASE [ContractClaimSystemDB] SET AUTO_UPDATE_STATISTICS ON 
GO
ALTER DATABASE [ContractClaimSystemDB] SET CURSOR_CLOSE_ON_COMMIT OFF 
GO
ALTER DATABASE [ContractClaimSystemDB] SET CURSOR_DEFAULT  GLOBAL 
GO
ALTER DATABASE [ContractClaimSystemDB] SET CONCAT_NULL_YIELDS_NULL OFF 
GO
ALTER DATABASE [ContractClaimSystemDB] SET NUMERIC_ROUNDABORT OFF 
GO
ALTER DATABASE [ContractClaimSystemDB] SET QUOTED_IDENTIFIER OFF 
GO
ALTER DATABASE [ContractClaimSystemDB] SET RECURSIVE_TRIGGERS OFF 
GO
ALTER DATABASE [ContractClaimSystemDB] SET  ENABLE_BROKER 
GO
ALTER DATABASE [ContractClaimSystemDB] SET AUTO_UPDATE_STATISTICS_ASYNC OFF 
GO
ALTER DATABASE [ContractClaimSystemDB] SET DATE_CORRELATION_OPTIMIZATION OFF 
GO
ALTER DATABASE [ContractClaimSystemDB] SET TRUSTWORTHY OFF 
GO
ALTER DATABASE [ContractClaimSystemDB] SET ALLOW_SNAPSHOT_ISOLATION OFF 
GO
ALTER DATABASE [ContractClaimSystemDB] SET PARAMETERIZATION SIMPLE 
GO
ALTER DATABASE [ContractClaimSystemDB] SET READ_COMMITTED_SNAPSHOT OFF 
GO
ALTER DATABASE [ContractClaimSystemDB] SET HONOR_BROKER_PRIORITY OFF 
GO
ALTER DATABASE [ContractClaimSystemDB] SET RECOVERY SIMPLE 
GO
ALTER DATABASE [ContractClaimSystemDB] SET  MULTI_USER 
GO
ALTER DATABASE [ContractClaimSystemDB] SET PAGE_VERIFY CHECKSUM  
GO
ALTER DATABASE [ContractClaimSystemDB] SET DB_CHAINING OFF 
GO
ALTER DATABASE [ContractClaimSystemDB] SET FILESTREAM( NON_TRANSACTED_ACCESS = OFF ) 
GO
ALTER DATABASE [ContractClaimSystemDB] SET TARGET_RECOVERY_TIME = 60 SECONDS 
GO
ALTER DATABASE [ContractClaimSystemDB] SET DELAYED_DURABILITY = DISABLED 
GO
ALTER DATABASE [ContractClaimSystemDB] SET ACCELERATED_DATABASE_RECOVERY = OFF  
GO
ALTER DATABASE [ContractClaimSystemDB] SET QUERY_STORE = ON
GO
ALTER DATABASE [ContractClaimSystemDB] SET QUERY_STORE (OPERATION_MODE = READ_WRITE, CLEANUP_POLICY = (STALE_QUERY_THRESHOLD_DAYS = 30), DATA_FLUSH_INTERVAL_SECONDS = 900, INTERVAL_LENGTH_MINUTES = 60, MAX_STORAGE_SIZE_MB = 1000, QUERY_CAPTURE_MODE = AUTO, SIZE_BASED_CLEANUP_MODE = AUTO, MAX_PLANS_PER_QUERY = 200, WAIT_STATS_CAPTURE_MODE = ON)
GO
USE [ContractClaimSystemDB]
GO
/****** Object:  Table [dbo].[Users]    Script Date: 11/21/2025 4:18:06 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Users](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[FullName] [nvarchar](200) NOT NULL,
	[Email] [nvarchar](256) NOT NULL,
	[PasswordHash] [nvarchar](max) NOT NULL,
	[Role] [int] NOT NULL,
	[Department] [nvarchar](200) NOT NULL,
	[HourlyRate] [decimal](18, 2) NULL,
	[IsActive] [bit] NOT NULL,
	[CreatedDate] [datetime2](7) NOT NULL,
	[UpdatedDate] [datetime2](7) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
UNIQUE NONCLUSTERED 
(
	[Email] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Claims]    Script Date: 11/21/2025 4:18:06 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Claims](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[ClaimNumber] [nvarchar](50) NOT NULL,
	[WorkDescription] [nvarchar](1000) NOT NULL,
	[HoursWorked] [decimal](18, 2) NOT NULL,
	[HourlyRate] [decimal](18, 2) NOT NULL,
	[TotalAmount] [decimal](18, 2) NOT NULL,
	[Notes] [nvarchar](500) NULL,
	[Status] [int] NOT NULL,
	[CreatedDate] [datetime2](7) NOT NULL,
	[SubmittedDate] [datetime2](7) NULL,
	[UpdatedDate] [datetime2](7) NOT NULL,
	[RejectionReason] [nvarchar](1000) NULL,
	[ReviewerComments] [nvarchar](1000) NULL,
	[UserId] [int] NOT NULL,
	[CoordinatorId] [int] NULL,
	[ManagerId] [int] NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
UNIQUE NONCLUSTERED 
(
	[ClaimNumber] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  View [dbo].[vw_ClaimsSummary]    Script Date: 11/21/2025 4:18:06 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- View: All Claims Summary
CREATE VIEW [dbo].[vw_ClaimsSummary] AS
SELECT 
    c.Id,
    c.ClaimNumber,
    c.WorkDescription,
    c.HoursWorked,
    c.HourlyRate,
    c.TotalAmount,
    c.Status,
    c.CreatedDate,
    c.SubmittedDate,
    u.FullName AS LecturerName,
    u.Email AS LecturerEmail,
    u.Department,
    coord.FullName AS CoordinatorName,
    mgr.FullName AS ManagerName,
    CASE c.Status
        WHEN 0 THEN 'Draft'
        WHEN 1 THEN 'Submitted'
        WHEN 2 THEN 'Verified'
        WHEN 3 THEN 'Approved'
        WHEN 4 THEN 'Rejected'
        WHEN 5 THEN 'Returned'
    END AS StatusText
FROM Claims c
INNER JOIN Users u ON c.UserId = u.Id
LEFT JOIN Users coord ON c.CoordinatorId = coord.Id
LEFT JOIN Users mgr ON c.ManagerId = mgr.Id;
GO
/****** Object:  View [dbo].[vw_ApprovedClaimsForPayment]    Script Date: 11/21/2025 4:18:06 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- View: Approved Claims for Payment Processing
CREATE VIEW [dbo].[vw_ApprovedClaimsForPayment] AS
SELECT 
    c.ClaimNumber,
    u.FullName AS LecturerName,
    u.Email AS LecturerEmail,
    u.Department,
    c.HoursWorked,
    c.HourlyRate,
    c.TotalAmount,
    c.SubmittedDate,
    c.WorkDescription,
    mgr.FullName AS ApprovedBy
FROM Claims c
INNER JOIN Users u ON c.UserId = u.Id
LEFT JOIN Users mgr ON c.ManagerId = mgr.Id
WHERE c.Status = 3; -- Approved
GO
/****** Object:  View [dbo].[vw_MonthlyClaimsByDepartment]    Script Date: 11/21/2025 4:18:06 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- View: Monthly Claims Summary by Department
CREATE VIEW [dbo].[vw_MonthlyClaimsByDepartment] AS
SELECT 
    u.Department,
    YEAR(c.SubmittedDate) AS Year,
    MONTH(c.SubmittedDate) AS Month,
    COUNT(*) AS TotalClaims,
    SUM(c.TotalAmount) AS TotalAmount,
    AVG(c.HoursWorked) AS AvgHoursWorked
FROM Claims c
INNER JOIN Users u ON c.UserId = u.Id
WHERE c.Status = 3 -- Approved only
GROUP BY u.Department, YEAR(c.SubmittedDate), MONTH(c.SubmittedDate);
GO
/****** Object:  Table [dbo].[__EFMigrationsHistory]    Script Date: 11/21/2025 4:18:06 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[__EFMigrationsHistory](
	[MigrationId] [nvarchar](150) NOT NULL,
	[ProductVersion] [nvarchar](32) NOT NULL,
 CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY CLUSTERED 
(
	[MigrationId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Documents]    Script Date: 11/21/2025 4:18:06 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Documents](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[FileName] [nvarchar](255) NOT NULL,
	[ContentType] [nvarchar](100) NOT NULL,
	[EncryptedContent] [varbinary](max) NOT NULL,
	[UploadDate] [datetime2](7) NOT NULL,
	[ClaimId] [int] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_Claims_ClaimNumber]    Script Date: 11/21/2025 4:18:06 PM ******/
CREATE NONCLUSTERED INDEX [IX_Claims_ClaimNumber] ON [dbo].[Claims]
(
	[ClaimNumber] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_Claims_Status]    Script Date: 11/21/2025 4:18:06 PM ******/
CREATE NONCLUSTERED INDEX [IX_Claims_Status] ON [dbo].[Claims]
(
	[Status] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_Claims_SubmittedDate]    Script Date: 11/21/2025 4:18:06 PM ******/
CREATE NONCLUSTERED INDEX [IX_Claims_SubmittedDate] ON [dbo].[Claims]
(
	[SubmittedDate] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_Claims_UserId]    Script Date: 11/21/2025 4:18:06 PM ******/
CREATE NONCLUSTERED INDEX [IX_Claims_UserId] ON [dbo].[Claims]
(
	[UserId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_Documents_ClaimId]    Script Date: 11/21/2025 4:18:06 PM ******/
CREATE NONCLUSTERED INDEX [IX_Documents_ClaimId] ON [dbo].[Documents]
(
	[ClaimId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_Users_Email]    Script Date: 11/21/2025 4:18:06 PM ******/
CREATE NONCLUSTERED INDEX [IX_Users_Email] ON [dbo].[Users]
(
	[Email] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_Users_Role]    Script Date: 11/21/2025 4:18:06 PM ******/
CREATE NONCLUSTERED INDEX [IX_Users_Role] ON [dbo].[Users]
(
	[Role] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
ALTER TABLE [dbo].[Claims] ADD  DEFAULT ((0)) FOR [Status]
GO
ALTER TABLE [dbo].[Claims] ADD  DEFAULT (getutcdate()) FOR [CreatedDate]
GO
ALTER TABLE [dbo].[Claims] ADD  DEFAULT (getutcdate()) FOR [UpdatedDate]
GO
ALTER TABLE [dbo].[Documents] ADD  DEFAULT (getutcdate()) FOR [UploadDate]
GO
ALTER TABLE [dbo].[Users] ADD  DEFAULT ((1)) FOR [IsActive]
GO
ALTER TABLE [dbo].[Users] ADD  DEFAULT (getutcdate()) FOR [CreatedDate]
GO
ALTER TABLE [dbo].[Users] ADD  DEFAULT (getutcdate()) FOR [UpdatedDate]
GO
ALTER TABLE [dbo].[Claims]  WITH CHECK ADD  CONSTRAINT [FK_Claims_Coordinator] FOREIGN KEY([CoordinatorId])
REFERENCES [dbo].[Users] ([Id])
GO
ALTER TABLE [dbo].[Claims] CHECK CONSTRAINT [FK_Claims_Coordinator]
GO
ALTER TABLE [dbo].[Claims]  WITH CHECK ADD  CONSTRAINT [FK_Claims_Manager] FOREIGN KEY([ManagerId])
REFERENCES [dbo].[Users] ([Id])
GO
ALTER TABLE [dbo].[Claims] CHECK CONSTRAINT [FK_Claims_Manager]
GO
ALTER TABLE [dbo].[Claims]  WITH CHECK ADD  CONSTRAINT [FK_Claims_Users] FOREIGN KEY([UserId])
REFERENCES [dbo].[Users] ([Id])
GO
ALTER TABLE [dbo].[Claims] CHECK CONSTRAINT [FK_Claims_Users]
GO
ALTER TABLE [dbo].[Documents]  WITH CHECK ADD  CONSTRAINT [FK_Documents_Claims] FOREIGN KEY([ClaimId])
REFERENCES [dbo].[Claims] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Documents] CHECK CONSTRAINT [FK_Documents_Claims]
GO
ALTER TABLE [dbo].[Claims]  WITH CHECK ADD  CONSTRAINT [CK_Claims_HourlyRate] CHECK  (([HourlyRate]>(0)))
GO
ALTER TABLE [dbo].[Claims] CHECK CONSTRAINT [CK_Claims_HourlyRate]
GO
ALTER TABLE [dbo].[Claims]  WITH CHECK ADD  CONSTRAINT [CK_Claims_HoursWorked] CHECK  (([HoursWorked]>(0) AND [HoursWorked]<=(200)))
GO
ALTER TABLE [dbo].[Claims] CHECK CONSTRAINT [CK_Claims_HoursWorked]
GO
ALTER TABLE [dbo].[Claims]  WITH CHECK ADD  CONSTRAINT [CK_Claims_Status] CHECK  (([Status]=(5) OR [Status]=(4) OR [Status]=(3) OR [Status]=(2) OR [Status]=(1) OR [Status]=(0)))
GO
ALTER TABLE [dbo].[Claims] CHECK CONSTRAINT [CK_Claims_Status]
GO
ALTER TABLE [dbo].[Claims]  WITH CHECK ADD  CONSTRAINT [CK_Claims_TotalAmount] CHECK  (([TotalAmount]>=(0)))
GO
ALTER TABLE [dbo].[Claims] CHECK CONSTRAINT [CK_Claims_TotalAmount]
GO
ALTER TABLE [dbo].[Users]  WITH CHECK ADD  CONSTRAINT [CK_Users_HourlyRate] CHECK  (([HourlyRate] IS NULL OR [HourlyRate]>=(0)))
GO
ALTER TABLE [dbo].[Users] CHECK CONSTRAINT [CK_Users_HourlyRate]
GO
ALTER TABLE [dbo].[Users]  WITH CHECK ADD  CONSTRAINT [CK_Users_Role] CHECK  (([Role]=(3) OR [Role]=(2) OR [Role]=(1) OR [Role]=(0)))
GO
ALTER TABLE [dbo].[Users] CHECK CONSTRAINT [CK_Users_Role]
GO
/****** Object:  StoredProcedure [dbo].[sp_GetLecturerPerformanceReport]    Script Date: 11/21/2025 4:18:06 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- Procedure: Get Lecturer Performance Report
CREATE PROCEDURE [dbo].[sp_GetLecturerPerformanceReport]
    @LecturerId INT = NULL,
    @StartDate DATETIME2 = NULL,
    @EndDate DATETIME2 = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        u.FullName,
        u.Email,
        u.Department,
        u.HourlyRate,
        COUNT(c.Id) AS TotalClaims,
        SUM(CASE WHEN c.Status = 3 THEN 1 ELSE 0 END) AS ApprovedClaims,
        SUM(CASE WHEN c.Status = 4 THEN 1 ELSE 0 END) AS RejectedClaims,
        SUM(CASE WHEN c.Status = 3 THEN c.TotalAmount ELSE 0 END) AS TotalApprovedAmount,
        AVG(CASE WHEN c.Status = 3 THEN c.HoursWorked ELSE NULL END) AS AvgHoursPerClaim
    FROM Users u
    LEFT JOIN Claims c ON u.Id = c.UserId 
        AND (@StartDate IS NULL OR c.SubmittedDate >= @StartDate)
        AND (@EndDate IS NULL OR c.SubmittedDate <= @EndDate)
    WHERE u.Role = 0 -- Lecturers only
        AND (@LecturerId IS NULL OR u.Id = @LecturerId)
    GROUP BY u.Id, u.FullName, u.Email, u.Department, u.HourlyRate;
END
GO
USE [master]
GO
ALTER DATABASE [ContractClaimSystemDB] SET  READ_WRITE 
GO
