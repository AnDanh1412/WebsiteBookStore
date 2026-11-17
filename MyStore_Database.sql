-- ============================================================
-- SCRIPT TẠO DATABASE VÀ DỮ LIỆU MẪU CHO WEBSITE MYSTORE
-- Chạy script này trên SQL Server Management Studio (SSMS)
-- ============================================================

USE [master];
GO

-- 1. TẠO DATABASE NẾU CHƯA CÓ
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'MyStore')
BEGIN
    CREATE DATABASE [MyStore];
END
GO

USE [MyStore];
GO

-- 2. XÓA CÁC KHÓA NGOẠI VÀ BẢNG CŨ (NẾU CẦN RESET)
IF OBJECT_ID('dbo.OrderDetail', 'U') IS NOT NULL DROP TABLE dbo.OrderDetail;
IF OBJECT_ID('dbo.Order', 'U') IS NOT NULL DROP TABLE dbo.[Order];
IF OBJECT_ID('dbo.Product', 'U') IS NOT NULL DROP TABLE dbo.Product;
IF OBJECT_ID('dbo.Category', 'U') IS NOT NULL DROP TABLE dbo.Category;
IF OBJECT_ID('dbo.Customer', 'U') IS NOT NULL DROP TABLE dbo.Customer;
IF OBJECT_ID('dbo.User', 'U') IS NOT NULL DROP TABLE dbo.[User];
GO

-- 3. TẠO CÁC BẢNG DỮ LIỆU
CREATE TABLE [dbo].[User] (
    [Username] VARCHAR(50) NOT NULL PRIMARY KEY,
    [Password] VARCHAR(50) NOT NULL,
    [UserRole] VARCHAR(5) NOT NULL
);
GO

CREATE TABLE [dbo].[Customer] (
    [CustomerID] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [CustomerName] NVARCHAR(100) NOT NULL,
    [CustomerPhone] VARCHAR(15) NULL,
    [CustomerEmail] VARCHAR(100) NULL,
    [CustomerAddress] NVARCHAR(250) NULL,
    [Username] VARCHAR(50) NULL,
    CONSTRAINT [FK_Customer_User] FOREIGN KEY ([Username]) REFERENCES [dbo].[User]([Username]) ON DELETE SET NULL
);
GO

CREATE TABLE [dbo].[Category] (
    [CategoryID] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [CategoryName] NVARCHAR(100) NOT NULL
);
GO

CREATE TABLE [dbo].[Product] (
    [ProductID] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [CategoryID] INT NOT NULL,
    [ProductName] NVARCHAR(150) NOT NULL,
    [ProductPrice] DECIMAL(18,2) NOT NULL,
    [ProductImage] VARCHAR(MAX) NULL,
    [ProductDescription] NVARCHAR(MAX) NULL,
    [IsDiscontinued] BIT NOT NULL DEFAULT 0,
    CONSTRAINT [FK_Pro_Category] FOREIGN KEY ([CategoryID]) REFERENCES [dbo].[Category]([CategoryID])
);
GO

CREATE TABLE [dbo].[Order] (
    [OrderID] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [CustomerID] INT NOT NULL,
    [OrderDate] DATETIME NOT NULL,
    [TotalAmount] DECIMAL(18,2) NOT NULL,
    [PaymentStatus] NVARCHAR(50) NOT NULL,
    [AddressDelivery] NVARCHAR(250) NOT NULL,
    CONSTRAINT [FK_Order_Customer] FOREIGN KEY ([CustomerID]) REFERENCES [dbo].[Customer]([CustomerID])
);
GO

CREATE TABLE [dbo].[OrderDetail] (
    [ID] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [ProductID] INT NULL,
    [OrderID] INT NOT NULL,
    [Quantity] INT NOT NULL,
    [UnitPrice] DECIMAL(18,2) NOT NULL,
    [ProductName] NVARCHAR(150) NULL,
    CONSTRAINT [FK_OrderDetail_Order] FOREIGN KEY ([OrderID]) REFERENCES [dbo].[Order]([OrderID]) ON DELETE CASCADE,
    CONSTRAINT [FK_OrderDetail_Product] FOREIGN KEY ([ProductID]) REFERENCES [dbo].[Product]([ProductID]) ON DELETE SET NULL
);
GO

-- 4. CHÈN DỮ LIỆU MẪU
INSERT INTO [User] ([Username], [Password], [UserRole]) VALUES (N'admin@gmail.com', N'123456', N'A');
INSERT INTO [User] ([Username], [Password], [UserRole]) VALUES (N'soul', N'123456                                            ', N'C');
INSERT INTO [User] ([Username], [Password], [UserRole]) VALUES (N'Vo Hoang Quý', N'123456                                            ', N'C');
GO

SET IDENTITY_INSERT [Category] ON;
INSERT INTO [Category] ([CategoryID], [CategoryName]) VALUES (1, N'Kinh Tế - Kỹ Năng');
INSERT INTO [Category] ([CategoryID], [CategoryName]) VALUES (2, N'Văn Học - Tiểu Thuyết');
INSERT INTO [Category] ([CategoryID], [CategoryName]) VALUES (3, N'Tâm Lý Học');
INSERT INTO [Category] ([CategoryID], [CategoryName]) VALUES (4, N'Lịch Sử - Xã Hội');
INSERT INTO [Category] ([CategoryID], [CategoryName]) VALUES (5, N'Sách Văn Học');
INSERT INTO [Category] ([CategoryID], [CategoryName]) VALUES (8, N'Bơm');
SET IDENTITY_INSERT [Category] OFF;
GO

SET IDENTITY_INSERT [Customer] ON;
INSERT INTO [Customer] ([CustomerID], [CustomerName], [CustomerPhone], [CustomerEmail], [CustomerAddress], [Username]) VALUES (4, N'Võ Hoàng Quý', N'497', N'1111111', N'111', N'Vo Hoang Quý');
SET IDENTITY_INSERT [Customer] OFF;
GO

SET IDENTITY_INSERT [Product] ON;
INSERT INTO [Product] ([ProductID], [CategoryID], [ProductName], [ProductPrice], [ProductImage], [ProductDescription], [IsDiscontinued]) VALUES (1, 1, N'Lý Thuyết Trò Chơi', 195000.00, N'~/Images/Gemini_Generated_Image_hi7yhhi7yhhi7yhh.png', N'Phân tích các chiến lược tâm lý và toán học giúp giải quyết xung đột.', 0);
INSERT INTO [Product] ([ProductID], [CategoryID], [ProductName], [ProductPrice], [ProductImage], [ProductDescription], [IsDiscontinued]) VALUES (2, 4, N'Lịch Sử Văn Minh Thế Giới', 350000.00, N'~/Images/Gemini_Generated_Image_hi7yhhi7yhhi7yhh.png', N'Tổng quan lịch sử phát triển các nền văn minh nhân loại.', 0);
INSERT INTO [Product] ([ProductID], [CategoryID], [ProductName], [ProductPrice], [ProductImage], [ProductDescription], [IsDiscontinued]) VALUES (3, 2, N'Tội Lỗi Và Hình Phạt', 220000.00, N'~/Images/Gemini_Generated_Image_hi7yhhi7yhhi7yhh.png', N'Kiệt tác văn học kinh điển về tâm lý tội phạm và sự đấu tranh nội tâm.', 0);
INSERT INTO [Product] ([ProductID], [CategoryID], [ProductName], [ProductPrice], [ProductImage], [ProductDescription], [IsDiscontinued]) VALUES (4, 2, N'Nghìn Lẻ Một Đêm', 185000.00, N'~/Images/Gemini_Generated_Image_hi7yhhi7yhhi7yhh.png', N'Tuyển tập những câu chuyện cổ tích dân gian Ả Rập đặc sắc.', 0);
INSERT INTO [Product] ([ProductID], [CategoryID], [ProductName], [ProductPrice], [ProductImage], [ProductDescription], [IsDiscontinued]) VALUES (5, 3, N'Tâm Lý Học Hiện Đại', 115000.00, N'~/Images/Gemini_Generated_Image_hi7yhhi7yhhi7yhh.png', N'Sách nghiên cứu hành vi và tâm lý học hiện đại.', 0);
INSERT INTO [Product] ([ProductID], [CategoryID], [ProductName], [ProductPrice], [ProductImage], [ProductDescription], [IsDiscontinued]) VALUES (6, 5, N'Hạt Giống Của Tâm Hồn', 65000.00, N'~/Images/Gemini_Generated_Image_hi7yhhi7yhhi7yhh.png', N'Những câu chuyện ý nghĩa bồi dưỡng tâm hồn và truyền cảm hứng.', 0);
INSERT INTO [Product] ([ProductID], [CategoryID], [ProductName], [ProductPrice], [ProductImage], [ProductDescription], [IsDiscontinued]) VALUES (7, 1, N'Tuổi Trẻ Đáng Giá Bao Nhiêu', 90000.00, N'~/Images/Gemini_Generated_Image_hi7yhhi7yhhi7yhh.png', N'Cuốn sách kỹ năng định hướng tư duy và hành động cho người trẻ.', 0);
INSERT INTO [Product] ([ProductID], [CategoryID], [ProductName], [ProductPrice], [ProductImage], [ProductDescription], [IsDiscontinued]) VALUES (8, 5, N'Cây Cam Ngọt Của Tôi', 108000.00, N'~/Images/Gemini_Generated_Image_hi7yhhi7yhhi7yhh.png', N'Câu chuyện cảm động về tình bạn và tuổi thơ.', 0);
INSERT INTO [Product] ([ProductID], [CategoryID], [ProductName], [ProductPrice], [ProductImage], [ProductDescription], [IsDiscontinued]) VALUES (9, 1, N'Đắc Nhân Tâm', 86000.00, N'~/Images/Gemini_Generated_Image_hi7yhhi7yhhi7yhh.png', N'Nghệ thuật thu phục lòng người và tạo ảnh hưởng tích cực.', 0);
INSERT INTO [Product] ([ProductID], [CategoryID], [ProductName], [ProductPrice], [ProductImage], [ProductDescription], [IsDiscontinued]) VALUES (10, 2, N'Nhà Giả Kim', 79000.00, N'~/Images/Gemini_Generated_Image_hi7yhhi7yhhi7yhh.png', N'Hành trình theo đuổi ước mơ và lắng nghe tiếng nói trái tim.', 0);
INSERT INTO [Product] ([ProductID], [CategoryID], [ProductName], [ProductPrice], [ProductImage], [ProductDescription], [IsDiscontinued]) VALUES (17, 8, N'CụC Bụp', 100000.00, N'~/Images/5_26111446634.jpg', N'555', 0);
SET IDENTITY_INSERT [Product] OFF;
GO

SET IDENTITY_INSERT [Order] ON;
INSERT INTO [Order] ([OrderID], [CustomerID], [OrderDate], [TotalAmount], [PaymentStatus], [AddressDelivery]) VALUES (6, 4, '2026-11-14 02:01:43', 10000.00, N'Đã giao', N'0000');
INSERT INTO [Order] ([OrderID], [CustomerID], [OrderDate], [TotalAmount], [PaymentStatus], [AddressDelivery]) VALUES (8, 4, '2026-11-14 07:23:17', 165000.00, N'Đã giao', N'111');
INSERT INTO [Order] ([OrderID], [CustomerID], [OrderDate], [TotalAmount], [PaymentStatus], [AddressDelivery]) VALUES (9, 4, '2026-11-14 07:26:06', 10000.00, N'Đang giao', N'222');
INSERT INTO [Order] ([OrderID], [CustomerID], [OrderDate], [TotalAmount], [PaymentStatus], [AddressDelivery]) VALUES (10, 4, '2026-11-14 07:28:02', 10000.00, N'Đã hủy', N'111');
SET IDENTITY_INSERT [Order] OFF;
GO

SET IDENTITY_INSERT [OrderDetail] ON;
INSERT INTO [OrderDetail] ([ID], [ProductID], [OrderID], [Quantity], [UnitPrice], [ProductName]) VALUES (9, NULL, 6, 1, 10000.00, N'CụC Bụp');
INSERT INTO [OrderDetail] ([ID], [ProductID], [OrderID], [Quantity], [UnitPrice], [ProductName]) VALUES (11, 10, 8, 1, 79000.00, N'Nhà Giả Kim');
INSERT INTO [OrderDetail] ([ID], [ProductID], [OrderID], [Quantity], [UnitPrice], [ProductName]) VALUES (12, 9, 8, 1, 86000.00, N'Đắc Nhân Tâm');
INSERT INTO [OrderDetail] ([ID], [ProductID], [OrderID], [Quantity], [UnitPrice], [ProductName]) VALUES (13, NULL, 9, 1, 10000.00, N'CụC Bụp');
INSERT INTO [OrderDetail] ([ID], [ProductID], [OrderID], [Quantity], [UnitPrice], [ProductName]) VALUES (14, NULL, 10, 1, 10000.00, N'CụC Bụp');
SET IDENTITY_INSERT [OrderDetail] OFF;
GO

PRINT N'==> ĐÃ KHỞI TẠO CSDL MYSTORE THÀNH CÔNG VỚI ĐẦY ĐỦ DỮ LIỆU!';